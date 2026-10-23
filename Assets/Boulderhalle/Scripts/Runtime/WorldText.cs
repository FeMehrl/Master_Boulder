using UnityEngine;

namespace Boulderhalle
{
    /// <summary>
    /// Verbindet das Text-Material mit der (zur Laufzeit erzeugten) Font-Textur.
    /// Nötig, weil dynamische Font-Texturen nicht im Material gespeichert werden.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TextMesh))]
    public class WorldText : MonoBehaviour
    {
        void OnEnable()
        {
            Font.textureRebuilt += OnFontTextureRebuilt;
            Apply();
        }

        void OnDisable()
        {
            Font.textureRebuilt -= OnFontTextureRebuilt;
        }

        void OnFontTextureRebuilt(Font font)
        {
            if (font == GetComponent<TextMesh>().font) Apply();
        }

        void Apply()
        {
            var tm = GetComponent<TextMesh>();
            var r = GetComponent<MeshRenderer>();
            if (tm.font == null || r == null || r.sharedMaterial == null) return;
            if (r.sharedMaterial == tm.font.material) return;
            r.sharedMaterial.mainTexture = tm.font.material.mainTexture;
        }
    }
}
