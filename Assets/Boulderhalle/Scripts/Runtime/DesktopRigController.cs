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
        public float touchReach = 1.6f;

        CharacterController controller;
        float pitch;
        float verticalVelocity;

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
            if (use && scenario != null) UseObjectInView();

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

        void UseObjectInView()
        {
            if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, 4f, ~0, QueryTriggerInteraction.Collide)) return;
            var target = hit.collider.GetComponentInParent<ScenarioObject>();
            if (target == null) return;
            if (target.IsStartHold && hit.distance > touchReach) return; // Startgriffe nur direkt an der Wand
            target.Activate();
        }

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
