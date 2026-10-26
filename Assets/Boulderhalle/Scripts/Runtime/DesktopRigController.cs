using UnityEngine;
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Boulderhalle
{
    /// <summary>
    /// Einfache Ego-Steuerung zum Testen ohne VR-Brille:
    /// WASD/Pfeiltasten = laufen, Maus = umsehen, Shift = schneller, Esc = Maus freigeben, Klick = Maus fangen.
    /// Szenario: Klick oder E = Objekt in der Bildmitte auswählen bzw. Startgriff berühren (nur aus der Nähe),
    /// Enter/Leertaste = Knopf im Pop-up drücken.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class DesktopRigController : MonoBehaviour
    {
        public Transform cameraTransform;
        public float moveSpeed = 2.0f;
        public float runSpeed = 4.0f;
        public float lookSensitivity = 0.12f;
        [Tooltip("Maximale Entfernung (m), aus der ein Startgriff 'berührt' werden kann.")]
        public float startHoldReach = 3f;

        CharacterController controller;
        float pitch;
        float verticalVelocity;
        ScenarioObject aimed;      // Objekt, auf das gerade gezielt wird
        bool aimedTooFar;          // Startgriff anvisiert, aber zu weit weg
        GUIStyle labelStyle;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            if (cameraTransform == null) cameraTransform = GetComponentInChildren<Camera>().transform;
            SetCursorLocked(true);
        }

        void Update()
        {
            Vector2 move = Vector2.zero, look = Vector2.zero;
            bool run = false, release = false, capture = false, use = false, confirm = false;

#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            var mouse = Mouse.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1f;
                run = kb.leftShiftKey.isPressed;
                release = kb.escapeKey.wasPressedThisFrame;
                use = kb.eKey.wasPressedThisFrame;
                confirm = kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
            }
            if (mouse != null)
            {
                look = mouse.delta.ReadValue();
                capture = mouse.leftButton.wasPressedThisFrame;
            }
#else
            move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 10f;
            run = Input.GetKey(KeyCode.LeftShift);
            release = Input.GetKeyDown(KeyCode.Escape);
            capture = Input.GetMouseButtonDown(0);
            use = Input.GetKeyDown(KeyCode.E);
            confirm = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space);
#endif
            var scenario = ScenarioManager.Instance;
            if (scenario != null && scenario.IsPopupOpen)
            {
                // Pop-up offen: Maus freigeben (Knopf anklickbar), nicht bewegen
                SetCursorLocked(false);
                if (confirm) scenario.PressPopupButton();
                return;
            }

            if (release) SetCursorLocked(false);
            if (capture && Cursor.lockState == CursorLockMode.Locked) use = true;
            else if (capture) SetCursorLocked(true);

            UpdateAim();
            if (use) UseAimedObject();

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                transform.Rotate(0f, look.x * lookSensitivity, 0f);
                pitch = Mathf.Clamp(pitch - look.y * lookSensitivity, -85f, 85f);
                cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            var dir = transform.right * move.x + transform.forward * move.y;
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            verticalVelocity = controller.isGrounded ? -1f : verticalVelocity + Physics.gravity.y * Time.deltaTime;
            var velocity = dir * (run ? runSpeed : moveSpeed) + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// Zielhilfe: wählt das auswählbare Objekt, das der Bildmitte am nächsten liegt
        /// (kleiner Toleranzkegel, freie Sicht nötig). So muss man kleine Objekte nicht pixelgenau treffen.
        /// </summary>
        void UpdateAim()
        {
            aimed = null;
            aimedTooFar = false;
            var origin = cameraTransform.position;
            var forward = cameraTransform.forward;
            float best = float.MaxValue;

            foreach (var o in ScenarioManager.All)
            {
                if (o == null) continue;
                var toObject = o.transform.position - origin;
                float along = Vector3.Dot(toObject, forward);
                if (along < 0.1f || along > 6f) continue;

                float offAxis = (toObject - forward * along).magnitude;
                if (offAxis > 0.15f + 0.05f * along) continue;
                if (!HasLineOfSight(origin, o)) continue;

                float score = offAxis / along;
                if (score < best)
                {
                    best = score;
                    aimed = o;
                }
            }

            if (aimed != null && aimed.IsStartHold)
                aimedTooFar = Vector3.Distance(origin, aimed.transform.position) > startHoldReach;
        }

        bool HasLineOfSight(Vector3 origin, ScenarioObject o)
        {
            var toObject = o.transform.position - origin;
            float distance = toObject.magnitude;
            if (!Physics.Raycast(origin, toObject / distance, out var hit, distance, ~0, QueryTriggerInteraction.Ignore)) return true;
            return hit.distance >= distance - 0.3f || hit.collider.GetComponentInParent<ScenarioObject>() == o;
        }

        void UseAimedObject()
        {
            if (aimed == null)
            {
                Debug.Log("Desktop-Test: Kein auswählbares Objekt in der Bildmitte.");
                return;
            }
            if (aimedTooFar)
            {
                Debug.Log("Desktop-Test: Startgriff zu weit entfernt – geh näher an die Wand.");
                return;
            }
            aimed.Activate();
        }

        void OnGUI()
        {
            var scenario = ScenarioManager.Instance;
            if (scenario != null && scenario.IsPopupOpen) return;

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 20, fontStyle = FontStyle.Bold };
                labelStyle.normal.textColor = Color.white;
            }

            // Fadenkreuz
            float cx = Screen.width / 2f, cy = Screen.height / 2f;
            var color = aimed != null ? (aimedTooFar ? new Color(1f, 0.6f, 0.2f) : new Color(0.1f, 0.9f, 1f)) : Color.white;
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(cx - 10f, cy - 1f, 20f, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cx - 1f, cy - 10f, 2f, 20f), Texture2D.whiteTexture);
            GUI.color = previous;

            if (aimed == null) return;
            string name = aimed.IsStartHold ? "Startgriff" : aimed.title;
            string text = aimedTooFar ? $"{name} – geh näher an die Wand" : $"[Klick / E]  {name}";
            var rect = new Rect(cx - 250f, cy + 24f, 500f, 30f);
            GUI.color = new Color(0f, 0f, 0f, 0.8f);
            GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), text, labelStyle);
            GUI.color = color;
            GUI.Label(rect, text, labelStyle);
            GUI.color = previous;
        }

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
