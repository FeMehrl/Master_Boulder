using UnityEngine;

namespace Boulderhalle
{
    public enum ShelfContent { Belongings, RentalShoes }

    /// <summary>Möbel und Ausstattung. Alle Objekte "schauen" in lokale +z-Richtung, sofern nicht anders angegeben.</summary>
    public static class Props
    {
        // ---------- Sitzmöbel ----------

        /// <summary>Sofa: Sitzende blicken in +z, Rückenlehne bei -z. Sitzhöhe ca. 0,45 m.</summary>
        public static Transform Sofa(string name, Transform parent, Vector3 pos, float yaw, float width, Material fabric, Palette p)
        {
            var g = Geo.Group(name, parent, pos, yaw);
            const float depth = 0.9f;
            Geo.Box("Unterbau", g, new Vector3(0f, 0.2f, 0f), new Vector3(width, 0.28f, depth), fabric);
            Geo.Box("Rueckenlehne", g, new Vector3(0f, 0.6f, -depth / 2f + 0.1f), new Vector3(width, 0.52f, 0.2f), fabric);
            Geo.Box("Armlehne_L", g, new Vector3(-width / 2f + 0.09f, 0.5f, 0f), new Vector3(0.18f, 0.32f, depth), fabric);
            Geo.Box("Armlehne_R", g, new Vector3(width / 2f - 0.09f, 0.5f, 0f), new Vector3(0.18f, 0.32f, depth), fabric);

            float inner = width - 0.36f;
            int seats = Mathf.Max(1, Mathf.RoundToInt(inner / 0.65f));
            float cw = inner / seats;
            for (int i = 0; i < seats; i++)
            {
                float x = -inner / 2f + cw * (i + 0.5f);
                Geo.Box($"Sitzkissen_{i + 1}", g, new Vector3(x, 0.395f, 0.08f), new Vector3(cw - 0.02f, 0.11f, depth - 0.24f), fabric);
                Geo.Box($"Rueckenkissen_{i + 1}", g, new Vector3(x, 0.66f, -0.22f), new Vector3(cw - 0.02f, 0.4f, 0.14f), fabric,
                    new Vector3(-8f, 0f, 0f), collider: false);
            }

            for (int i = 0; i < 4; i++)
            {
                float fx = (i % 2 == 0 ? -1f : 1f) * (width / 2f - 0.06f);
                float fz = (i < 2 ? -1f : 1f) * (depth / 2f - 0.06f);
                Geo.Box($"Fuss_{i + 1}", g, new Vector3(fx, 0.03f, fz), new Vector3(0.06f, 0.06f, 0.06f), p.WoodDark, collider: false);
            }
            return g;
        }

        public static Transform Chair(string name, Transform parent, Vector3 pos, float yaw, Palette p)
        {
            var g = Geo.Group(name, parent, pos, yaw);
            Geo.Box("Sitz", g, new Vector3(0f, 0.45f, 0f), new Vector3(0.44f, 0.04f, 0.44f), p.ChairSeat);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.19f;
                float z = (i < 2 ? -1f : 1f) * 0.19f;
                Geo.Cyl($"Bein_{i + 1}", g, new Vector3(x, 0.215f, z), 0.03f, 0.43f, p.Metal, collider: false);
            }
            Geo.Cyl("Pfosten_L", g, new Vector3(-0.19f, 0.64f, -0.2f), 0.025f, 0.36f, p.Metal, collider: false);
            Geo.Cyl("Pfosten_R", g, new Vector3(0.19f, 0.64f, -0.2f), 0.025f, 0.36f, p.Metal, collider: false);
            Geo.Box("Lehne", g, new Vector3(0f, 0.74f, -0.2f), new Vector3(0.42f, 0.24f, 0.03f), p.ChairSeat);
            return g;
        }

        public static Transform Bench(string name, Transform parent, Vector3 pos, float yaw, float length, Palette p)
        {
            var g = Geo.Group(name, parent, pos, yaw);
            Geo.Box("Sitzflaeche", g, new Vector3(0f, 0.44f, 0f), new Vector3(length, 0.05f, 0.38f), p.Wood);
            Geo.Box("Bein_L", g, new Vector3(-length / 2f + 0.15f, 0.21f, 0f), new Vector3(0.06f, 0.42f, 0.34f), p.Metal);
            Geo.Box("Bein_R", g, new Vector3(length / 2f - 0.15f, 0.21f, 0f), new Vector3(0.06f, 0.42f, 0.34f), p.Metal);
            return g;
        }

        // ---------- Tische ----------

        public static Transform RoundTable(string name, Transform parent, Vector3 pos, Palette p)
        {
            var g = Geo.Group(name, parent, pos);
            Geo.Cyl("Platte", g, new Vector3(0f, 0.745f, 0f), 0.8f, 0.03f, p.Wood);
            Geo.Cyl("Saeule", g, new Vector3(0f, 0.37f, 0f), 0.06f, 0.72f, p.Metal, collider: false);
            Geo.Cyl("Fuss", g, new Vector3(0f, 0.01f, 0f), 0.45f, 0.02f, p.Metal, collider: false);
            return g;
        }

        public static void TableWithChairs(string name, Transform parent, Vector3 center, int chairs, float startAngle, Palette p)
        {
            var g = Geo.Group(name, parent, Vector3.zero);
            RoundTable("Tisch", g, center, p);
            for (int i = 0; i < chairs; i++)
            {
                float a = startAngle + i * 360f / chairs;
                var dir = new Vector3(Mathf.Sin(a * Mathf.Deg2Rad), 0f, Mathf.Cos(a * Mathf.Deg2Rad));
                Chair($"Stuhl_{i + 1}", g, center + dir * 0.72f, a + 180f, p);
            }
        }

        public static Transform CoffeeTable(string name, Transform parent, Vector3 pos, float yaw, Palette p)
        {
            var g = Geo.Group(name, parent, pos, yaw);
            Geo.Box("Platte", g, new Vector3(0f, 0.4f, 0f), new Vector3(1.0f, 0.04f, 0.55f), p.Wood);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1f : 1f) * 0.44f;
                float z = (i < 2 ? -1f : 1f) * 0.22f;
                Geo.Box($"Bein_{i + 1}", g, new Vector3(x, 0.19f, z), new Vector3(0.04f, 0.38f, 0.04f), p.WoodDark, collider: false);
            }
            return g;
        }

        // ---------- Deko ----------

        public static Transform Plant(string name, Transform parent, Vector3 pos, float scale, System.Random rnd, Palette p)
        {
            var g = Geo.Group(name, parent, pos);
            bool tall = rnd.NextDouble() < 0.5;
            var potMat = rnd.NextDouble() < 0.5 ? p.Terracotta : p.PotWhite;
            Geo.Cyl("Topf", g, new Vector3(0f, 0.2f, 0f) * scale, 0.36f * scale, 0.4f * scale, potMat);
            Geo.Cyl("Topfrand", g, new Vector3(0f, 0.39f, 0f) * scale, 0.4f * scale, 0.04f * scale, potMat, collider: false);
            Geo.Cyl("Erde", g, new Vector3(0f, 0.405f, 0f) * scale, 0.33f * scale, 0.01f * scale, p.Soil, collider: false);

            float crownY = tall ? 1.1f : 0.6f;
            if (tall) Geo.Cyl("Stamm", g, new Vector3(0f, 0.72f, 0f) * scale, 0.04f * scale, 0.65f * scale, p.Stem, collider: false);

            int leaves = tall ? 9 : 7;
            float spread = tall ? 0.2f : 0.15f;
            for (int i = 0; i < leaves; i++)
            {
                float a = i * 2.39996f; // goldener Winkel
                float r = spread * Mathf.Sqrt((i + 0.5f) / leaves);
                var lp = new Vector3(Mathf.Cos(a) * r, crownY + Geo.Rand(rnd, -0.12f, 0.12f), Mathf.Sin(a) * r) * scale;
                float d = Geo.Rand(rnd, 0.22f, 0.34f) * scale;
                Geo.Ball($"Blattwerk_{i + 1}", g, lp, new Vector3(d, d * 0.85f, d), i % 2 == 0 ? p.Leaf : p.LeafDark);
            }
            return g;
        }

        /// <summary>Wanduhr mit echter Uhrzeit. Ziffernblatt zeigt in Richtung -z.</summary>
        public static Transform Clock(string name, Transform parent, Vector3 pos, float yaw, Palette p)
        {
            var g = Geo.Group(name, parent, pos, yaw);
            var faceRot = new Vector3(90f, 0f, 0f);
            Geo.Cyl("Rahmen", g, new Vector3(0f, 0f, 0.01f), 0.56f, 0.05f, p.Black, faceRot, collider: false);
            Geo.Cyl("Ziffernblatt", g, new Vector3(0f, 0f, -0.005f), 0.5f, 0.04f, p.White, faceRot, collider: false);

            for (int k = 0; k < 12; k++)
            {
                float a = k * 30f;
                var mp = new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 0.2f, Mathf.Cos(a * Mathf.Deg2Rad) * 0.2f, -0.027f);
                var size = k % 3 == 0 ? new Vector3(0.025f, 0.06f, 0.005f) : new Vector3(0.012f, 0.035f, 0.005f);
                Geo.Box($"Markierung_{k}", g, mp, size, p.Black, new Vector3(0f, 0f, -a), collider: false);
            }

            var hour = Geo.Group("Stundenzeiger", g, new Vector3(0f, 0f, -0.03f));
            Geo.Box("Zeiger", hour, new Vector3(0f, 0.055f, 0f), new Vector3(0.025f, 0.14f, 0.006f), p.Black, collider: false);
            var minute = Geo.Group("Minutenzeiger", g, new Vector3(0f, 0f, -0.035f));
            Geo.Box("Zeiger", minute, new Vector3(0f, 0.085f, 0f), new Vector3(0.016f, 0.2f, 0.005f), p.Black, collider: false);
            var second = Geo.Group("Sekundenzeiger", g, new Vector3(0f, 0f, -0.039f));
            Geo.Box("Zeiger", second, new Vector3(0f, 0.08f, 0f), new Vector3(0.006f, 0.2f, 0.004f), p.Red, collider: false);
            Geo.Cyl("Achse", g, new Vector3(0f, 0f, -0.042f), 0.03f, 0.01f, p.Red, faceRot, collider: false);

            var hands = g.gameObject.AddComponent<ClockHands>();
            hands.hourHand = hour;
            hands.minuteHand = minute;
            hands.secondHand = second;
            return g;
        }

        /// <summary>Monitor + Tastatur. Der Benutzer steht bei -z und blickt in +z.</summary>
        public static Transform Computer(string name, Transform parent, Vector3 pos, float yaw, string screenText, Material screen, Palette p)
        {
            var g = Geo.Group(name, parent, pos, yaw);
            Geo.Box("Standfuss", g, new Vector3(0f, 0.01f, 0.08f), new Vector3(0.22f, 0.02f, 0.16f), p.DarkPlastic);
            Geo.Box("Hals", g, new Vector3(0f, 0.17f, 0.1f), new Vector3(0.04f, 0.3f, 0.03f), p.DarkPlastic, collider: false);
            Geo.Box("Gehaeuse", g, new Vector3(0f, 0.36f, 0.07f), new Vector3(0.56f, 0.35f, 0.04f), p.DarkPlastic);
            Geo.Box("Bildschirm", g, new Vector3(0f, 0.36f, 0.049f), new Vector3(0.52f, 0.31f, 0.002f), screen, collider: false);
            Geo.Text("Bildschirmtext", g, new Vector3(0f, 0.36f, 0.046f), 0f, screenText, 0.045f, Color.white, p.TextMat);
            Geo.Box("Tastatur", g, new Vector3(0f, 0.012f, -0.22f), new Vector3(0.44f, 0.024f, 0.14f), p.DarkPlastic);
            Geo.Box("Maus", g, new Vector3(0.3f, 0.015f, -0.22f), new Vector3(0.06f, 0.03f, 0.1f), p.DarkPlastic, collider: false);
            return g;
        }

        // ---------- Regale ----------

        /// <summary>Fächerregal; die offenen Fächer zeigen in lokale +z-Richtung.</summary>
        public static Transform Shelf(string name, Transform parent, Vector3 pos, float yaw, int cols, int rows,
            ShelfContent content, System.Random rnd, Palette p)
        {
            const float cellW = 0.55f, cellH = 0.46f, depth = 0.45f, board = 0.03f, plinth = 0.1f;
            float w = cols * cellW + board;
            float h = plinth + rows * cellH + board;
            var g = Geo.Group(name, parent, pos, yaw);

            Geo.Box("Rueckwand", g, new Vector3(0f, h / 2f, -depth / 2f + 0.01f), new Vector3(w, h, 0.02f), p.WoodDark);
            Geo.Box("Sockel", g, new Vector3(0f, plinth / 2f, -0.02f), new Vector3(w, plinth, depth - 0.04f), p.WoodDark);
            for (int r = 0; r <= rows; r++)
                Geo.Box($"Boden_{r}", g, new Vector3(0f, plinth + r * cellH + board / 2f, 0f), new Vector3(w, board, depth), p.Wood);
            for (int c = 0; c <= cols; c++)
                Geo.Box($"Trennwand_{c}", g, new Vector3(-w / 2f + board / 2f + c * cellW, h / 2f, 0f), new Vector3(board, h, depth), p.Wood);

            var items = Geo.Group("Inhalt", g, Vector3.zero);
            for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                float cx = -w / 2f + board + c * cellW + (cellW - board) / 2f;
                float yb = plinth + r * cellH + board;
                var cell = Geo.Group($"Fach_{r + 1}_{c + 1}", items, new Vector3(cx, yb, 0f));
                if (content == ShelfContent.RentalShoes)
                {
                    ShoePair(cell, new Vector3(-0.12f, 0f, 0.02f), p.Shoes[rnd.Next(p.Shoes.Length)]);
                    ShoePair(cell, new Vector3(0.12f, 0f, 0.02f), p.Shoes[rnd.Next(p.Shoes.Length)]);
                    continue;
                }

                double roll = rnd.NextDouble();
                var bag = p.Bags[rnd.Next(p.Bags.Length)];
                if (roll < 0.3) Backpack(cell, bag);
                else if (roll < 0.5) Jacket(cell, p.Tops[rnd.Next(p.Tops.Length)]);
                else if (roll < 0.62) ShoePair(cell, new Vector3(0f, 0f, 0.02f), p.Shoes[rnd.Next(p.Shoes.Length)]);
                else if (roll < 0.72) ChalkBag(cell, bag, p);
                // sonst: leeres Fach
            }
            return g;
        }

        static void Backpack(Transform cell, Material m)
        {
            Geo.Box("Rucksack", cell, new Vector3(0f, 0.17f, 0f), new Vector3(0.3f, 0.34f, 0.2f), m, collider: false);
            Geo.Box("Rucksack_Tasche", cell, new Vector3(0f, 0.11f, 0.11f), new Vector3(0.22f, 0.14f, 0.04f), m, collider: false);
            Geo.Box("Rucksack_Deckel", cell, new Vector3(0f, 0.33f, 0.02f), new Vector3(0.31f, 0.04f, 0.21f), m, collider: false);
        }

        static void Jacket(Transform cell, Material m)
        {
            Geo.Box("Jacke", cell, new Vector3(0f, 0.04f, 0.02f), new Vector3(0.42f, 0.08f, 0.32f), m, collider: false);
            Geo.Box("Jacke_Lage2", cell, new Vector3(0.01f, 0.11f, 0.01f), new Vector3(0.38f, 0.06f, 0.28f), m, new Vector3(0f, 8f, 0f), collider: false);
        }

        static void ShoePair(Transform cell, Vector3 offset, Material m)
        {
            Geo.Box("Schuh_L", cell, offset + new Vector3(-0.055f, 0.035f, 0f), new Vector3(0.09f, 0.07f, 0.24f), m, collider: false);
            Geo.Box("Schuh_R", cell, offset + new Vector3(0.055f, 0.035f, 0f), new Vector3(0.09f, 0.07f, 0.24f), m, collider: false);
        }

        static void ChalkBag(Transform cell, Material m, Palette p)
        {
            Geo.Cyl("Chalkbag", cell, new Vector3(0f, 0.08f, 0.05f), 0.13f, 0.16f, m, collider: false);
            Geo.Cyl("Chalk", cell, new Vector3(0f, 0.165f, 0.05f), 0.11f, 0.01f, p.White, collider: false);
        }
    }
}
