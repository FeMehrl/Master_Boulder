using System.Collections.Generic;
using UnityEngine;

namespace Boulderhalle
{
    /// <summary>
    /// Ein Segment der Boulderwand. Lokales Koordinatensystem des Pivots:
    /// x = entlang der Wand, y = an der Wand hoch, -z = Richtung Halle (Kletterfläche liegt bei z = 0).
    /// </summary>
    public class WallPanel
    {
        public Transform Pivot;
        public float Width, Height, Angle;
        public readonly List<Material> RouteMaterials = new List<Material>();
        /// <summary>Ankerpunkt des Startgriffs je Route (gleiche Reihenfolge wie RouteMaterials).</summary>
        public readonly List<Transform> StartHolds = new List<Transform>();
    }

    /// <summary>Boulderwand mit Segmenten unterschiedlicher Neigung, Routen, Volumen und Weichbodenmatte.</summary>
    public static class ClimbingWallBuilder
    {
        public const float BaseX = -9.0f;         // Fußlinie der Wand
        public const float PanelHeight = 4.3f;
        const float PanelThickness = 0.2f;
        public const float MatHeight = 0.3f;
        public const float MatFrontX = -4.2f;

        static readonly (string name, string hex, string grade)[] Routes =
        {
            ("Gelb", "#F2C318", "leicht"), ("Gruen", "#3BA55C", "leicht+"), ("Blau", "#2F6FD6", "mittel"),
            ("Rot", "#D63A3A", "schwer"), ("Schwarz", "#222222", "sehr schwer"), ("Lila", "#8A4FC7", "schwer"),
            ("Orange", "#F28A1E", "mittel"), ("Weiss", "#F2F2F2", "leicht"), ("Pink", "#E85A9C", "mittel+"),
            ("Tuerkis", "#21B5B0", "schwer+")
        };

        // Segmente entlang der linken Hallenwand (z-Bereich, Neigung: + = Platte, - = Überhang)
        static readonly (float zMin, float zMax, float angle)[] Segments =
        {
            (9f, 13f, 0f), (13f, 17f, -20f), (17f, 21f, 10f), (21f, 25f, -30f), (25f, 29f, -8f)
        };

        public static List<WallPanel> Build(Transform g, Palette p, System.Random rnd, BuildResult result)
        {
            var panels = new List<WallPanel>();
            var wall = Geo.Group("Boulderwand", g, Vector3.zero);

            for (int i = 0; i < Segments.Length; i++)
            {
                var s = Segments[i];
                float width = s.zMax - s.zMin;
                var pivot = Geo.Group($"Wandsegment_{i + 1}", wall, new Vector3(BaseX, 0f, (s.zMin + s.zMax) / 2f));
                pivot.localRotation = Quaternion.Euler(s.angle, -90f, 0f);
                var panel = new WallPanel { Pivot = pivot, Width = width, Height = PanelHeight, Angle = s.angle };
                panels.Add(panel);

                Geo.Box("Platte", pivot, new Vector3(0f, PanelHeight / 2f, PanelThickness / 2f),
                    new Vector3(width - 0.02f, PanelHeight, PanelThickness), i % 2 == 0 ? p.Panel : p.PanelAlt);

                // Abdeckung von der Oberkante bis zur Hallenwand
                Vector3 top = pivot.TransformPoint(new Vector3(0f, PanelHeight, 0f));
                float wallX = -BoulderhalleBuilder.HalfWidth;
                float depth = top.x - wallX;
                Geo.Box("Abdeckung_" + (i + 1), wall, new Vector3(wallX + depth / 2f, top.y + 0.05f, top.z),
                    new Vector3(depth, 0.1f, width), p.PanelAlt);

                for (int k = 0; k < 2; k++)
                {
                    var route = Routes[(i * 2 + k) % Routes.Length];
                    var mat = p.Solid("Griff_" + route.name, Palette.Hex(route.hex), 0.35f);
                    panel.RouteMaterials.Add(mat);
                    float lane = k == 0 ? -width / 4f : width / 4f;
                    panel.StartHolds.Add(AddRoute(pivot, $"{route.name}", route.grade, lane, width, mat, p, rnd));
                }

                int volumes = rnd.Next(1, 3);
                for (int v = 0; v < volumes; v++)
                {
                    var vp = new Vector3(Geo.Rand(rnd, -1.4f, 1.4f), Geo.Rand(rnd, 1.6f, 3.6f), 0f);
                    float vs = Geo.Rand(rnd, 0.4f, 0.65f);
                    Geo.Box($"Volumen_{v + 1}", pivot, vp, Vector3.one * vs, p.Volume,
                        new Vector3(Geo.Rand(rnd, 30f, 60f), 45f, Geo.Rand(rnd, 0f, 90f)), collider: false);
                }
            }

            BuildMat(g, p, result);
            return panels;
        }

        /// <returns>Ankerpunkt am Startgriff (lokal: Kletterfläche, z = 0).</returns>
        static Transform AddRoute(Transform pivot, string colorName, string grade, float lane, float width, Material mat, Palette p, System.Random rnd)
        {
            var g = Geo.Group("Route_" + colorName, pivot, Vector3.zero);
            Transform start = null;
            int n = rnd.Next(7, 11);
            const float yStart = 0.55f, yEnd = PanelHeight - 0.35f;
            for (int j = 0; j < n; j++)
            {
                float y = Mathf.Lerp(yStart, yEnd, j / (float)(n - 1)) + Geo.Rand(rnd, -0.1f, 0.1f);
                float x = Mathf.Clamp(lane + Geo.Rand(rnd, -0.6f, 0.6f), -width / 2f + 0.25f, width / 2f - 0.25f);
                float size = j == n - 1 ? 0.2f : Geo.Rand(rnd, 0.09f, 0.22f);
                var pos = new Vector3(x, y, 0f);
                AddHold(g, $"Griff_{j + 1}", pos, size, mat, rnd);

                if (j == 1) start = Geo.Group("Startgriff", g, pos);

                // Start- und Topmarkierung mit Tape
                if (j == 1 || j == n - 1)
                    Geo.Box(j == 1 ? "Starttape" : "Toptape", g, pos + new Vector3(0f, -size * 0.6f - 0.03f, -0.004f),
                        new Vector3(0.12f, 0.025f, 0.006f), mat, collider: false);
            }

            Geo.Box("Routenschild", g, new Vector3(lane, 0.72f, -0.006f), new Vector3(0.46f, 0.2f, 0.01f), p.White, collider: false);
            Geo.Box("Routenschild_Farbe", g, new Vector3(lane, 0.83f, -0.007f), new Vector3(0.46f, 0.04f, 0.012f), mat, collider: false);
            Geo.Text("Routenname", g, new Vector3(lane, 0.71f, -0.013f), 0f, $"{colorName}\n{grade}", 0.06f, new Color(0.1f, 0.1f, 0.1f), p.TextMat);
            return start;
        }

        public static GameObject AddHold(Transform parent, string name, Vector3 localPos, float size, Material mat, System.Random rnd)
        {
            float roll = Geo.Rand(rnd, 0f, 360f);
            switch (rnd.Next(0, 3))
            {
                case 0:
                    return Geo.Ball(name, parent, localPos + new Vector3(0f, 0f, -size * 0.12f),
                        new Vector3(size, size * 0.7f, size * 0.5f), mat, new Vector3(0f, 0f, roll));
                case 1:
                    return Geo.Box(name, parent, localPos + new Vector3(0f, 0f, -size * 0.1f),
                        new Vector3(size, size * 0.55f, size * 0.35f), mat,
                        new Vector3(Geo.Rand(rnd, -15f, 15f), Geo.Rand(rnd, -15f, 15f), roll), collider: false);
                default:
                    return Geo.Caps(name, parent, localPos + new Vector3(0f, 0f, -size * 0.1f),
                        size * 0.55f, size * 1.3f, mat, new Vector3(0f, 0f, roll + 90f));
            }
        }

        static void BuildMat(Transform g, Palette p, BuildResult result)
        {
            var mat = Geo.Group("Weichbodenmatte", g, Vector3.zero);
            const float x0 = BaseX, x1 = MatFrontX, zStart = 8.7f, segLen = 3f;
            const int segments = 7;
            for (int s = 0; s < segments; s++)
            {
                var seg = Geo.Box($"Mattensegment_{s + 1}", mat,
                    new Vector3((x0 + x1) / 2f, MatHeight / 2f, zStart + segLen * (s + 0.5f)),
                    new Vector3(x1 - x0, MatHeight, segLen - 0.02f), p.MatTop);
                result.WalkableSurfaces.Add(seg);
            }
            float len = segments * segLen;
            Geo.Box("Mattenkante", mat, new Vector3(x1 + 0.01f, MatHeight / 2f, zStart + len / 2f),
                new Vector3(0.02f, MatHeight, len), p.MatSide, collider: false);
        }

        /// <summary>Setzt eine kletternde Figur an ein Wandsegment und erzeugt Griffe unter Händen und Füßen.</summary>
        public static Transform AddClimber(Transform parent, WallPanel panel, string name, float lx, float pelvisY, Pose pose,
            Outfit outfit, Material holdMat, System.Random rnd)
        {
            var pos = panel.Pivot.TransformPoint(new Vector3(lx, pelvisY - pose.PelvisHeight, -0.32f));
            var person = PersonBuilder.Create(name, parent, pos, panel.Pivot.rotation, pose, outfit, blocking: false);

            foreach (var part in new[] { "Hand_L", "Hand_R", "Fuss_L", "Fuss_R" })
            {
                var t = PersonBuilder.FindDeep(person, part);
                if (t == null) continue;
                var local = panel.Pivot.InverseTransformPoint(t.position);
                AddHold(panel.Pivot, $"Griff_{name}_{part}", new Vector3(local.x, local.y, 0f), 0.15f, holdMat, rnd);
            }
            return person;
        }
    }
}
