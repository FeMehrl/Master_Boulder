using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Boulderhalle
{
    /// <summary>Hilfsfunktionen zum Aufbau der Geometrie aus Unity-Primitiven.</summary>
    public static class Geo
    {
        static Font font;

        public static Transform Group(string name, Transform parent, Vector3 localPos, float yaw = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            return go.transform;
        }

        public static GameObject Prim(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale,
            Material mat, Vector3 euler = default, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        public static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material mat,
            Vector3 euler = default, bool collider = true)
            => Prim(PrimitiveType.Cube, name, parent, pos, size, mat, euler, collider);

        /// <summary>Zylinder; Achse entlang lokaler y-Achse (vor der Rotation).</summary>
        public static GameObject Cyl(string name, Transform parent, Vector3 pos, float diameter, float height, Material mat,
            Vector3 euler = default, bool collider = true)
            => Prim(PrimitiveType.Cylinder, name, parent, pos, new Vector3(diameter, height / 2f, diameter), mat, euler, collider);

        public static GameObject Ball(string name, Transform parent, Vector3 pos, Vector3 size, Material mat,
            Vector3 euler = default, bool collider = false)
            => Prim(PrimitiveType.Sphere, name, parent, pos, size, mat, euler, collider);

        public static GameObject Caps(string name, Transform parent, Vector3 pos, float diameter, float length, Material mat,
            Vector3 euler = default, bool collider = false)
            => Prim(PrimitiveType.Capsule, name, parent, pos, new Vector3(diameter, length / 2f, diameter), mat, euler, collider);

        public static void NoShadows(GameObject go)
        {
            var r = go.GetComponent<Renderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        /// <summary>
        /// 3D-Text. Lesbar für einen Betrachter, der in lokale +z-Richtung blickt
        /// (der Text "schaut" also in Richtung -z).
        /// </summary>
        public static TextMesh Text(string name, Transform parent, Vector3 pos, float yaw, string text, float lineHeight,
            Color color, Material textMat, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var tm = go.AddComponent<TextMesh>();
            tm.font = GetFont();
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = lineHeight * 10f / tm.fontSize;
            tm.anchor = anchor;
            tm.alignment = TextAlignment.Center;
            tm.color = color;

            var r = go.GetComponent<MeshRenderer>();
            if (r == null) r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = textMat != null ? textMat : tm.font.material;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            go.AddComponent<WorldText>();
            return tm;
        }

        public static Font GetFont()
        {
            if (font != null) return font;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        /// <summary>
        /// Wand entlang der lokalen x-Achse (0..length) mit rechteckigen Öffnungen (Türen/Fenster).
        /// Öffnungen in lokalen Koordinaten: x entlang der Wand, y = Höhe.
        /// </summary>
        public static Transform WallWithOpenings(string name, Transform parent, Vector3 start, float yaw, float length,
            float height, float thickness, Material mat, params Rect[] openings)
        {
            var g = Group(name, parent, start, yaw);
            var sorted = new List<Rect>(openings);
            sorted.Sort((a, b) => a.xMin.CompareTo(b.xMin));
            float x = 0f;
            int i = 0;
            foreach (var o in sorted)
            {
                if (o.xMin > x + 0.001f)
                    Box($"Wandstueck_{i++}", g, new Vector3((x + o.xMin) / 2f, height / 2f, 0f), new Vector3(o.xMin - x, height, thickness), mat);
                if (o.yMin > 0.001f)
                    Box($"Bruestung_{i++}", g, new Vector3(o.center.x, o.yMin / 2f, 0f), new Vector3(o.width, o.yMin, thickness), mat);
                if (o.yMax < height - 0.001f)
                    Box($"Sturz_{i++}", g, new Vector3(o.center.x, (o.yMax + height) / 2f, 0f), new Vector3(o.width, height - o.yMax, thickness), mat);
                x = o.xMax;
            }
            if (length > x + 0.001f)
                Box($"Wandstueck_{i}", g, new Vector3((x + length) / 2f, height / 2f, 0f), new Vector3(length - x, height, thickness), mat);
            return g;
        }

        /// <summary>Verglasung samt Rahmen und Sprossen in einer Wandöffnung (lokale Wandkoordinaten).</summary>
        public static void Glazing(string name, Transform wall, Rect r, float wallThickness, Material glass, Material frame)
        {
            var g = Group(name, wall, Vector3.zero);
            var pane = Box("Glas", g, new Vector3(r.center.x, r.center.y, 0f), new Vector3(r.width, r.height, 0.02f), glass);
            NoShadows(pane);

            const float f = 0.07f;
            float d = wallThickness + 0.04f;
            Box("Rahmen_unten", g, new Vector3(r.center.x, r.yMin + f / 2f, 0f), new Vector3(r.width, f, d), frame, collider: false);
            Box("Rahmen_oben", g, new Vector3(r.center.x, r.yMax - f / 2f, 0f), new Vector3(r.width, f, d), frame, collider: false);
            Box("Rahmen_links", g, new Vector3(r.xMin + f / 2f, r.center.y, 0f), new Vector3(f, r.height, d), frame, collider: false);
            Box("Rahmen_rechts", g, new Vector3(r.xMax - f / 2f, r.center.y, 0f), new Vector3(f, r.height, d), frame, collider: false);

            int panes = Mathf.Max(1, Mathf.RoundToInt(r.width / 1.5f));
            for (int i = 1; i < panes; i++)
            {
                float x = r.xMin + r.width * i / panes;
                Box($"Sprosse_{i}", g, new Vector3(x, r.center.y, 0f), new Vector3(f * 0.7f, r.height, d * 0.6f), frame, collider: false);
            }
        }

        /// <summary>Großflächige Box in Kacheln aufgeteilt (bessere Beleuchtung bei vielen Punktlichtern).</summary>
        public static List<GameObject> Tiled(string name, Transform parent, Vector3 center, Vector3 size, int nx, int nz, Material mat)
        {
            var list = new List<GameObject>();
            var g = Group(name, parent, Vector3.zero);
            float sx = size.x / nx, sz = size.z / nz;
            for (int ix = 0; ix < nx; ix++)
            for (int iz = 0; iz < nz; iz++)
            {
                var pos = center + new Vector3(-size.x / 2f + sx * (ix + 0.5f), 0f, -size.z / 2f + sz * (iz + 0.5f));
                list.Add(Box($"{name}_{ix}_{iz}", g, pos, new Vector3(sx, size.y, sz), mat));
            }
            return list;
        }

        public static float Rand(System.Random rnd, float min, float max) => min + (float)rnd.NextDouble() * (max - min);
    }
}
