using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Boulderhalle
{
    /// <summary>
    /// Erzeugt und cached einfache Materialien. Funktioniert mit URP und der Built-in-Pipeline.
    /// Der Editor-Builder speichert alle Materialien anschließend als Assets.
    /// </summary>
    public class MaterialLibrary
    {
        readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();
        readonly Shader litShader;
        readonly bool isUrp;

        public IEnumerable<KeyValuePair<string, Material>> All => cache;

        public MaterialLibrary()
        {
            isUrp = GraphicsSettings.currentRenderPipeline != null;
            litShader = isUrp ? Shader.Find("Universal Render Pipeline/Lit") : null;
            if (litShader == null)
            {
                litShader = Shader.Find("Standard");
                isUrp = false;
            }
        }

        public Material Get(string name, Color color, float smoothness = 0.2f, float metallic = 0f)
        {
            if (cache.TryGetValue(name, out var cached)) return cached;
            var m = Create(name, color, smoothness, metallic);
            cache[name] = m;
            return m;
        }

        public Material Emissive(string name, Color color, Color emission)
        {
            if (cache.TryGetValue(name, out var cached)) return cached;
            var m = Create(name, color, 0.5f, 0f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            cache[name] = m;
            return m;
        }

        public Material Transparent(string name, Color color, float smoothness = 0.95f)
        {
            if (cache.TryGetValue(name, out var cached)) return cached;
            var m = Create(name, color, smoothness, 0f);
            if (isUrp)
            {
                m.SetFloat("_Surface", 1f);
                m.SetFloat("_Blend", 0f);
                m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                if (m.HasProperty("_SrcBlendAlpha")) m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
                if (m.HasProperty("_DstBlendAlpha")) m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                m.SetFloat("_Mode", 3f);
                m.SetInt("_SrcBlend", (int)BlendMode.One);
                m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.DisableKeyword("_ALPHATEST_ON");
                m.DisableKeyword("_ALPHABLEND_ON");
                m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)RenderQueue.Transparent;
            cache[name] = m;
            return m;
        }

        /// <summary>Material für 3D-Texte (TextMesh), das korrekt von Geometrie verdeckt wird.</summary>
        public Material Text()
        {
            if (cache.TryGetValue("Text", out var cached)) return cached;
            var shader = Shader.Find("Boulderhalle/WorldText");
            if (shader == null) return null; // Fallback: Standard-Font-Material
            var m = new Material(shader) { name = "Text" };
            cache["Text"] = m;
            return m;
        }

        Material Create(string name, Color color, float smoothness, float metallic)
        {
            var m = new Material(litShader) { name = name };
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            return m;
        }
    }
}
