using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Boulderhalle
{
    public enum ScenarioObjectKind
    {
        /// <summary>Begriff erklären (Chalkbag, Bürste): Auswahl per Strahl + Trigger oder Anfassen.</summary>
        Info,
        /// <summary>Startgriff einer Route, an der gerade jemand bouldert (falsches Verhalten). Auslösung durch Berühren.</summary>
        StartHoldOccupied,
        /// <summary>Startgriff einer freien Route (richtiges Verhalten). Auslösung durch Berühren.</summary>
        StartHoldFree
    }

    /// <summary>
    /// Auswählbares Objekt des Szenarios. Titel und Text lassen sich im Inspector anpassen.
    /// </summary>
    public class ScenarioObject : MonoBehaviour
    {
        public ScenarioObjectKind kind;
        public string title;
        [TextArea(3, 6)] public string message;

        [Tooltip("Abstand (m) zwischen Controller und Griff, ab dem ein Startgriff als berührt gilt.")]
        public float touchRadius = 0.15f;

        readonly HashSet<IXRHoverInteractor> hovering = new HashSet<IXRHoverInteractor>();

        public bool IsStartHold => kind != ScenarioObjectKind.Info;

        void Awake()
        {
            if (kind != ScenarioObjectKind.Info) return;

            // Chalkbag/Bürste: Greif-Taste (Select) oder Trigger (Activate) während der Strahl darauf zeigt
            var interactable = gameObject.AddComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(_ => Activate());
            interactable.hoverEntered.AddListener(args => hovering.Add(args.interactorObject));
            interactable.hoverExited.AddListener(args => hovering.Remove(args.interactorObject));
        }

        void OnEnable() => ScenarioManager.Register(this);
        void OnDisable() => ScenarioManager.Unregister(this);

        void Update()
        {
            foreach (var interactor in hovering)
            {
                if (interactor is XRBaseInputInteractor input && input.activateInput.ReadWasPerformedThisFrame())
                {
                    Activate();
                    break;
                }
            }
        }

        public void Activate()
        {
            if (ScenarioManager.Instance != null) ScenarioManager.Instance.OnObjectActivated(this);
        }
    }
}
