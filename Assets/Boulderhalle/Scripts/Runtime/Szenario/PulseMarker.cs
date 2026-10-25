using UnityEngine;

namespace Boulderhalle
{
    /// <summary>Lässt die farbige Markierung eines auswählbaren Objekts pulsieren, schweben und rotieren.</summary>
    public class PulseMarker : MonoBehaviour
    {
        public Transform ring;
        public Transform diamond;
        public float speed = 2.5f;

        Vector3 ringScale;
        Vector3 diamondPos;

        void Start()
        {
            if (ring) ringScale = ring.localScale;
            if (diamond) diamondPos = diamond.localPosition;
        }

        void Update()
        {
            float t = Time.time * speed;
            if (ring)
            {
                float s = 1f + 0.12f * Mathf.Sin(t * 2f);
                ring.localScale = new Vector3(ringScale.x * s, ringScale.y, ringScale.z * s);
            }
            if (diamond)
            {
                diamond.localPosition = diamondPos + transform.InverseTransformDirection(Vector3.up) * (0.04f * Mathf.Sin(t));
                diamond.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
            }
        }
    }
}
