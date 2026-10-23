using UnityEngine;
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Boulderhalle
{
    /// <summary>
    /// Einfache Ego-Steuerung zum Testen ohne VR-Brille:
    /// WASD/Pfeiltasten = laufen, Maus = umsehen, Shift = schneller, Esc = Maus freigeben, Klick = Maus fangen.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class DesktopRigController : MonoBehaviour
    {
        public Transform cameraTransform;
        public float moveSpeed = 2.0f;
        public float runSpeed = 4.0f;
        public float lookSensitivity = 0.12f;

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
            bool run = false, release = false, capture = false;

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
#endif
            if (release) SetCursorLocked(false);
            if (capture) SetCursorLocked(true);

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

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
