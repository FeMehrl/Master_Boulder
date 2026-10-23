using System;
using UnityEngine;

namespace Boulderhalle
{
    /// <summary>Stellt die Zeiger der Wanduhr auf die aktuelle Systemzeit.</summary>
    [ExecuteAlways]
    public class ClockHands : MonoBehaviour
    {
        public Transform hourHand;
        public Transform minuteHand;
        public Transform secondHand;

        void Update()
        {
            var now = DateTime.Now;
            float seconds = now.Second + now.Millisecond / 1000f;
            float minutes = now.Minute + seconds / 60f;
            float hours = now.Hour % 12 + minutes / 60f;

            // Negative z-Rotation = im Uhrzeigersinn (Ziffernblatt zeigt in -z)
            if (hourHand) hourHand.localRotation = Quaternion.Euler(0f, 0f, -hours * 30f);
            if (minuteHand) minuteHand.localRotation = Quaternion.Euler(0f, 0f, -minutes * 6f);
            if (secondHand) secondHand.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Floor(seconds) * 6f);
        }
    }
}
