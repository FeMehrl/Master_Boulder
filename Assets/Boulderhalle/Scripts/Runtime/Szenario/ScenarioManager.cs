using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Boulderhalle
{
    /// <summary>
    /// Steuert das explorative Szenario "Sturzbereiche freihalten":
    /// reagiert auf ausgewählte Objekte bzw. berührte Startgriffe, zeigt Pop-ups und spielt Feedback-Töne.
    /// </summary>
    public class ScenarioManager : MonoBehaviour
    {
        public static ScenarioManager Instance { get; private set; }

        static readonly List<ScenarioObject> Objects = new List<ScenarioObject>();

        public FeedbackPopup popup;

        [Header("Abschluss")]
        public string completionTitle = "Einweisung abgeschlossen";
        [TextArea(3, 6)]
        public string completionMessage = "Gut gemacht! Denk immer daran: Halte den Sturzbereich unter boulderenden Personen frei " +
                                          "und starte erst, wenn über dir niemand an der Wand ist.";

        [Header("Knopfbeschriftungen")]
        public string okLabel = "OK";
        public string retryLabel = "Nochmal versuchen";
        public string finishLabel = "Einweisung abschließen";
        public string restartLabel = "Neu starten";

        [Header("Töne (leer = automatisch erzeugte Töne)")]
        public AudioClip positiveSound;
        public AudioClip negativeSound;
        public AudioClip infoSound;

        readonly List<Transform> hands = new List<Transform>();
        readonly HashSet<ScenarioObject> touching = new HashSet<ScenarioObject>();
        AudioSource audioSource;
        float nextHandSearch;
        float blockedUntil;

        public bool IsPopupOpen => popup != null && popup.IsOpen;

        /// <summary>Alle auswählbaren Objekte der Szene.</summary>
        public static IReadOnlyList<ScenarioObject> All => Objects;

        public static void Register(ScenarioObject o)
        {
            if (!Objects.Contains(o)) Objects.Add(o);
        }

        public static void Unregister(ScenarioObject o) => Objects.Remove(o);

        void Awake()
        {
            Instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            if (positiveSound == null) positiveSound = ToneGenerator.Positive();
            if (negativeSound == null) negativeSound = ToneGenerator.Negative();
            if (infoSound == null) infoSound = ToneGenerator.Info();
        }

        void Start()
        {
            EnsureEventSystem();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (Time.time >= nextHandSearch)
            {
                FindHands();
                nextHandSearch = Time.time + 2f;
            }
            CheckStartHoldTouches();
        }

        // ------------------------------------------------------------------ Ereignisse

        public void OnObjectActivated(ScenarioObject o)
        {
            if (IsPopupOpen || Time.time < blockedUntil) return;
            Debug.Log($"Szenario: '{o.name}' ausgelöst ({o.kind}).");

            switch (o.kind)
            {
                case ScenarioObjectKind.Info:
                    Play(infoSound);
                    popup.Show(PopupStyle.Info, o.title, o.message, okLabel, Unblock);
                    break;
                case ScenarioObjectKind.StartHoldOccupied:
                    Play(negativeSound);
                    popup.Show(PopupStyle.Negative, o.title, o.message, retryLabel, Restart);
                    break;
                case ScenarioObjectKind.StartHoldFree:
                    Play(positiveSound);
                    popup.Show(PopupStyle.Positive, o.title, o.message, finishLabel, ShowCompletion);
                    break;
            }
        }

        void ShowCompletion()
        {
            popup.Show(PopupStyle.Positive, completionTitle, completionMessage, restartLabel, Restart);
        }

        void Unblock()
        {
            // Kurze Sperre, damit der Klick auf "OK" nicht sofort ein weiteres Objekt auslöst
            blockedUntil = Time.time + 0.5f;
        }

        /// <summary>Startet das Szenario neu (Nutzer steht wieder im Eingangsbereich).</summary>
        public void Restart()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }

        /// <summary>Für den Desktop-Test: drückt den Knopf des offenen Pop-ups.</summary>
        public void PressPopupButton()
        {
            if (IsPopupOpen) popup.PressButton();
        }

        void Play(AudioClip clip)
        {
            if (clip != null) audioSource.PlayOneShot(clip);
        }

        // ------------------------------------------------------------------ Berühren der Startgriffe

        void FindHands()
        {
            hands.Clear();
            var cam = Camera.main;
            if (cam == null) return;
            var rig = cam.transform.root;
            foreach (var driver in rig.GetComponentsInChildren<TrackedPoseDriver>())
                if (driver.GetComponent<Camera>() == null) hands.Add(driver.transform);
            foreach (var poke in rig.GetComponentsInChildren<XRPokeInteractor>())
                hands.Add(poke.attachTransform != null ? poke.attachTransform : poke.transform);
        }

        void CheckStartHoldTouches()
        {
            foreach (var o in Objects)
            {
                if (!o.IsStartHold) continue;

                bool inside = false;
                foreach (var hand in hands)
                {
                    if (hand != null && (hand.position - o.transform.position).sqrMagnitude <= o.touchRadius * o.touchRadius)
                    {
                        inside = true;
                        break;
                    }
                }

                // Nur beim Hineinbewegen auslösen, nicht dauerhaft
                if (inside && touching.Add(o)) o.Activate();
                else if (!inside) touching.Remove(o);
            }
        }

        // ------------------------------------------------------------------ UI-Eingabe

        static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<XRUIInputModule>();
        }
    }
}
