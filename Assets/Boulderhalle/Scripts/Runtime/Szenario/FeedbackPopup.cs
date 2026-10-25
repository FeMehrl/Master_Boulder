using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Boulderhalle
{
    public enum PopupStyle { Info, Positive, Negative }

    /// <summary>
    /// Pop-up-Fenster im Raum. Erscheint vor dem Nutzer und wird immer über der Geometrie gezeichnet,
    /// damit es auch direkt an der Wand lesbar bleibt.
    /// </summary>
    public class FeedbackPopup : MonoBehaviour
    {
        public Image header;
        public TMP_Text titleText;
        public TMP_Text bodyText;
        public Button button;
        public TMP_Text buttonLabel;

        public Color infoColor = new Color(0.16f, 0.45f, 0.75f);
        public Color positiveColor = new Color(0.18f, 0.6f, 0.3f);
        public Color negativeColor = new Color(0.8f, 0.2f, 0.2f);

        [Tooltip("Abstand vor dem Kopf des Nutzers (m).")]
        public float distance = 0.9f;

        Action onButton;
        bool initialized;

        public bool IsOpen => gameObject.activeSelf;

        public void Show(PopupStyle style, string title, string body, string buttonText, Action action)
        {
            gameObject.SetActive(true);
            Initialize();

            var color = style == PopupStyle.Positive ? positiveColor : style == PopupStyle.Negative ? negativeColor : infoColor;
            header.color = color;
            button.image.color = color;
            titleText.text = title;
            bodyText.text = body;
            buttonLabel.text = buttonText;
            onButton = action;

            PlaceInFrontOfUser();
        }

        public void Hide() => gameObject.SetActive(false);

        public void PressButton()
        {
            var action = onButton;
            onButton = null;
            Hide();
            action?.Invoke();
        }

        void Initialize()
        {
            if (initialized) return;
            initialized = true;
            button.onClick.AddListener(PressButton);
            DrawOnTop();
        }

        void PlaceInFrontOfUser()
        {
            var cam = Camera.main;
            if (cam == null) return;
            GetComponent<Canvas>().worldCamera = cam;

            var head = cam.transform;
            var forward = head.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = head.up;
            forward.Normalize();

            // Steht der Nutzer direkt vor einer Wand, das Fenster vor die Wand holen
            float d = distance;
            if (Physics.Raycast(head.position, forward, out var hit, distance + 0.2f, ~0, QueryTriggerInteraction.Ignore))
                d = Mathf.Clamp(hit.distance - 0.15f, 0.45f, distance);

            transform.position = head.position + forward * d + Vector3.down * 0.1f;
            transform.rotation = Quaternion.LookRotation(forward);
        }

        /// <summary>Ersetzt die UI-Materialien durch Kopien ohne Tiefentest.</summary>
        void DrawOnTop()
        {
            Material overlay = null;
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            {
                if (graphic is TMP_Text text)
                {
                    var m = text.fontMaterial; // erzeugt eine Instanz
                    m.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
                    m.renderQueue = (int)RenderQueue.Overlay;
                }
                else
                {
                    if (overlay == null)
                    {
                        overlay = new Material(graphic.defaultMaterial);
                        overlay.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
                        overlay.renderQueue = (int)RenderQueue.Overlay;
                    }
                    graphic.material = overlay;
                }
            }
        }
    }
}
