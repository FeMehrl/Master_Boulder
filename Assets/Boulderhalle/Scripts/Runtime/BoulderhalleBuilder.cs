using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Boulderhalle
{
    public class BuildResult
    {
        public GameObject Root;
        public Transform SpawnPoint;
        /// <summary>Interaktives Szenario (eigenes Root-Objekt, nicht statisch).</summary>
        public GameObject ScenarioRoot;
        public readonly List<GameObject> WalkableSurfaces = new List<GameObject>();
    }

    /// <summary>
    /// Baut das statische Modell der Boulderhalle.
    ///
    /// Grundriss (Draufsicht, Einheiten in Metern):
    ///   x: -10 (links, Boulderwand) .. +10 (rechts, Fensterfront)
    ///   z:   0 (Eingangstür) .. 8 (Durchgang zur Halle) .. 30 (Rückwand mit Regalen)
    /// </summary>
    public static class BoulderhalleBuilder
    {
        public const float HalfWidth = 10f;
        public const float Length = 30f;
        public const float Height = 6f;
        public const float EntranceDepth = 8f;
        public const float WallThickness = 0.3f;

        static readonly Vector3 HallSofa2Pos = new Vector3(8.9f, 0f, 21.5f);
        // Kunde auf der Matte, neben dem Chalkbag und Bürste liegen
        static readonly Vector3 MatSitterPos = new Vector3(-4.75f, ClimbingWallBuilder.MatHeight, 21.4f);
        const float HallSofaYaw = -90f;

        /// <param name="withScenario">true = dynamisches Modell mit dem Szenario "Sturzbereiche freihalten".</param>
        public static BuildResult Build(MaterialLibrary library, bool withScenario = true, int seed = 42)
        {
            var p = new Palette(library);
            var rnd = new System.Random(seed);
            var result = new BuildResult { Root = new GameObject("Boulderhalle") };
            var root = result.Root.transform;

            BuildShell(Geo.Group("Gebaeude", root, Vector3.zero), p, result);
            BuildEntrance(Geo.Group("Eingangsbereich", root, Vector3.zero), p, rnd);
            var panels = ClimbingWallBuilder.Build(Geo.Group("Boulderbereich", root, Vector3.zero), p, rnd, result);
            BuildHallFurniture(Geo.Group("Halle_Ausstattung", root, Vector3.zero), p, rnd);
            BuildCustomers(Geo.Group("Kunden", root, Vector3.zero), p, panels, rnd);
            BuildLighting(Geo.Group("Beleuchtung", root, Vector3.zero), p);

            // Fester Startpunkt im Eingangsbereich, Blick zwischen Mitarbeiter und Durchgang zur Halle
            result.SpawnPoint = Geo.Group("Spawnpunkt_Eingang", root, new Vector3(-0.5f, 0f, 1.5f), -30f);

            if (withScenario)
            {
                // Belegte Route: Gelb auf Segment 1 (dort klettert ein Kunde).
                // Freie Route: Pink auf Segment 5 (dort klettert niemand).
                result.ScenarioRoot = ScenarioBuilder.Build(p, panels[0].StartHolds[0], panels[4].StartHolds[0], MatSitterPos);
            }
            return result;
        }

        // ------------------------------------------------------------------ Gebäude

        static void BuildShell(Transform g, Palette p, BuildResult result)
        {
            const float w = HalfWidth, t = WallThickness;

            Geo.Box("Aussengelaende", g, new Vector3(0f, -0.15f, Length / 2f), new Vector3(120f, 0.1f, 120f), p.Grass);
            Geo.Box("Gehweg", g, new Vector3(0f, -0.08f, -3.5f), new Vector3(3f, 0.06f, 6f), p.Paving);

            result.WalkableSurfaces.AddRange(Geo.Tiled("Boden_Eingang", g, new Vector3(0f, -0.05f, EntranceDepth / 2f),
                new Vector3(2f * w, 0.1f, EntranceDepth), 4, 2, p.FloorEntrance));
            result.WalkableSurfaces.AddRange(Geo.Tiled("Boden_Halle", g, new Vector3(0f, -0.05f, (EntranceDepth + Length) / 2f),
                new Vector3(2f * w, 0.1f, Length - EntranceDepth), 4, 4, p.FloorHall));
            Geo.Tiled("Decke", g, new Vector3(0f, Height + 0.1f, Length / 2f), new Vector3(2f * w + 2f * t, 0.2f, Length + 2f * t), 4, 6, p.Ceiling);

            // Vorderwand: Eingangstür + Schaufenster
            float frontLen = 2f * w + 2f * t;
            var door = Rect.MinMaxRect(frontLen / 2f - 1.25f, 0f, frontLen / 2f + 1.25f, 2.6f);
            var shopWindow = Rect.MinMaxRect(frontLen / 2f + 3.5f, 0.9f, frontLen / 2f + 8.5f, 2.8f);
            var front = Geo.WallWithOpenings("Wand_Vorne", g, new Vector3(-w - t, 0f, -t / 2f), 0f, frontLen, Height, t, p.Wall, door, shopWindow);
            Geo.Glazing("Schaufenster", front, shopWindow, t, p.Glass, p.Frame);
            Geo.Text("Schild_Aussen", g, new Vector3(0f, 3.4f, -t - 0.02f), 0f, "BOULDERHALLE", 0.5f, Color.white, p.TextMat);

            // Geöffnete Glastüren
            for (int s = -1; s <= 1; s += 2)
            {
                var leaf = Geo.Box(s < 0 ? "Tuer_links" : "Tuer_rechts", g, new Vector3(1.22f * s, 1.27f, 0.6f), new Vector3(0.04f, 2.5f, 1.15f), p.Glass);
                Geo.NoShadows(leaf);
                Geo.Box(s < 0 ? "Tuerrahmen_links" : "Tuerrahmen_rechts", g, new Vector3(1.22f * s, 2.5f, 0.6f), new Vector3(0.06f, 0.06f, 1.15f), p.Frame, collider: false);
                Geo.Box(s < 0 ? "Tuergriff_links" : "Tuergriff_rechts", g, new Vector3(1.17f * s, 1.05f, 1.05f), new Vector3(0.03f, 0.6f, 0.03f), p.Metal, collider: false);
            }

            // Rückwand
            Geo.WallWithOpenings("Wand_Hinten", g, new Vector3(-w - t, 0f, Length + t / 2f), 0f, frontLen, Height, t, p.Wall);

            // Linke Wand (hinter der Boulderwand)
            Geo.WallWithOpenings("Wand_Links", g, new Vector3(-w - t / 2f, 0f, 0f), -90f, Length, Height, t, p.Wall);

            // Rechte Wand mit Fensterfront (gegenüber der Boulderwand) + ein Fenster im Eingangsbereich
            var windows = new List<Rect> { Rect.MinMaxRect(2.5f, 1.0f, 5.5f, 2.8f) };
            const int hallWindows = 5;
            const float winW = 3f, hallStart = EntranceDepth + 0.5f, hallEnd = Length - 0.5f;
            float gap = (hallEnd - hallStart - hallWindows * winW) / (hallWindows + 1);
            for (int i = 0; i < hallWindows; i++)
            {
                float z0 = hallStart + gap + i * (winW + gap);
                windows.Add(Rect.MinMaxRect(z0, 1.0f, z0 + winW, 4.8f));
            }
            var right = Geo.WallWithOpenings("Wand_Rechts_Fensterfront", g, new Vector3(w + t / 2f, 0f, 0f), -90f, Length, Height, t, p.Wall, windows.ToArray());
            for (int i = 0; i < windows.Count; i++)
                Geo.Glazing($"Fenster_{i + 1}", right, windows[i], t, p.Glass, p.Frame);

            // Trennwand zwischen Eingangsbereich und Halle mit breitem Durchgang
            Geo.WallWithOpenings("Trennwand", g, new Vector3(-w, 0f, EntranceDepth), 0f, 2f * w, Height, 0.2f, p.Wall,
                Rect.MinMaxRect(w - 2f, 0f, w + 2f, 3.2f));
            Geo.Text("Schild_Zur_Halle", g, new Vector3(0f, 3.65f, EntranceDepth - 0.12f), 0f, "Zur Boulderhalle", 0.28f, new Color(0.15f, 0.2f, 0.25f), p.TextMat);
            Geo.Text("Schild_Ausgang", g, new Vector3(0f, 3.65f, EntranceDepth + 0.12f), 180f, "Eingang / Ausgang", 0.28f, new Color(0.15f, 0.2f, 0.25f), p.TextMat);
        }

        // ------------------------------------------------------------------ Eingangsbereich

        static void BuildEntrance(Transform g, Palette p, System.Random rnd)
        {
            Geo.Box("Fussmatte", g, new Vector3(0f, 0.005f, 0.9f), new Vector3(2.2f, 0.01f, 1.4f), p.Doormat, collider: false);

            // Tresen (L-Form), Kundenseite zeigt nach +x
            var counter = Geo.Group("Tresen", g, Vector3.zero);
            const float cx = -4.8f;
            Geo.Box("Korpus", counter, new Vector3(cx, 0.55f, 4.3f), new Vector3(0.8f, 1.1f, 4.4f), p.CounterFront);
            Geo.Box("Platte", counter, new Vector3(cx, 1.12f, 4.3f), new Vector3(0.95f, 0.04f, 4.6f), p.Countertop);
            Geo.Box("Seitenteil", counter, new Vector3(-6.4f, 0.55f, 2.3f), new Vector3(2.4f, 1.1f, 0.6f), p.CounterFront);
            Geo.Box("Seitenplatte", counter, new Vector3(-6.45f, 1.12f, 2.3f), new Vector3(2.6f, 0.04f, 0.75f), p.Countertop);
            Geo.Box("Zierstreifen", counter, new Vector3(cx + 0.405f, 0.85f, 4.3f), new Vector3(0.01f, 0.12f, 4.4f), p.Brand, collider: false);
            Geo.Text("Tresen_Schriftzug", counter, new Vector3(cx + 0.415f, 0.5f, 4.3f), -90f, "Willkommen!", 0.22f, Color.white, p.TextMat);

            // Computer: Anmeldung (für Kunden, Bildschirm zur Kundenseite) und Mitarbeiter-PC
            Props.Computer("Anmeldecomputer", counter, new Vector3(-4.75f, 1.14f, 5.7f), -90f,
                "Willkommen!\nNeu hier?\nHier registrieren", p.ScreenCustomer, p);
            Geo.Cyl("Schildhalter", counter, new Vector3(-4.62f, 1.47f, 6.2f), 0.02f, 0.66f, p.Metal, collider: false);
            Geo.Box("Schild_Anmeldung", counter, new Vector3(-4.62f, 1.84f, 5.85f), new Vector3(0.02f, 0.16f, 0.9f), p.Brand, collider: false);
            Geo.Text("Schild_Anmeldung_Text", counter, new Vector3(-4.6f, 1.84f, 5.85f), -90f, "Neukunden-Anmeldung", 0.08f, Color.white, p.TextMat);

            Props.Computer("Mitarbeitercomputer", counter, new Vector3(-4.95f, 1.14f, 3.1f), 90f,
                "Check-in\nKasse", p.ScreenStaff, p);
            Props.Plant("Pflanze_Tresen", counter, new Vector3(-4.8f, 1.14f, 6.3f), 0.45f, rnd, p);

            // Rückregal mit Leihschuhen + Wandbeschriftung
            Props.Shelf("Regal_Leihschuhe", g, new Vector3(-HalfWidth + 0.225f, 0f, 4.3f), 90f, 6, 4, ShelfContent.RentalShoes, rnd, p);
            Geo.Text("Schild_Leihschuhe", g, new Vector3(-HalfWidth + 0.02f, 2.45f, 4.3f), -90f, "Leihschuhe  ·  Chalk  ·  Shop", 0.16f, new Color(0.2f, 0.2f, 0.2f), p.TextMat);
            Geo.Text("Logo", g, new Vector3(-HalfWidth + 0.02f, 3.6f, 4.3f), -90f, "BOULDERHALLE", 0.6f, Palette.Hex("#E8572A"), p.TextMat);

            // Mitarbeiter hinter dem Tresen, begrüßt neue Kunden
            var staffOutfit = new Outfit
            {
                Skin = p.Skins[1], Hair = p.Hairs[0], Top = p.Brand, Bottom = p.Bottoms[0], Shoes = p.Shoes[1], Eyes = p.Black
            };
            PersonBuilder.Create("Mitarbeiter", g, new Vector3(-5.75f, 0f, 4.4f), 90f, PersonBuilder.Waving(), staffOutfit);

            // Sprechblase zeigt zum Startpunkt
            var bubble = Geo.Group("Sprechblase", g, new Vector3(-5.75f, 2.45f, 4.4f), -61f);
            var bg = Geo.Box("Hintergrund", bubble, new Vector3(0f, 0f, 0.02f), new Vector3(2.4f, 0.7f, 0.02f), p.White, collider: false);
            Geo.NoShadows(bg);
            var tip = Geo.Box("Spitze", bubble, new Vector3(0f, -0.37f, 0.02f), new Vector3(0.14f, 0.14f, 0.02f), p.White, new Vector3(0f, 0f, 45f), collider: false);
            Geo.NoShadows(tip);
            Geo.Text("Begruessung", bubble, Vector3.zero, 0f,
                "Hallo und herzlich willkommen!\nDu bist neu hier? Dann mach' bitte\nunsere Sicherheitseinweisung.\nBetritt dafür den sportlichen Hallenbereich.", 0.11f, new Color(0.12f, 0.12f, 0.12f), p.TextMat);

            // Uhr an der Trennwand (Seite Eingangsbereich)
            Props.Clock("Wanduhr", g, new Vector3(5f, 3.4f, EntranceDepth - 0.13f), 0f, p);

            // Sitzecke
            var lounge = Geo.Group("Sitzecke", g, Vector3.zero);
            Props.Sofa("Sofa", lounge, new Vector3(9.45f, 0f, 4f), -90f, 2.2f, p.SofaA, p);
            Props.CoffeeTable("Couchtisch", lounge, new Vector3(8.2f, 0f, 4f), 90f, p);
            Props.Sofa("Sessel_1", lounge, new Vector3(8.1f, 0f, 6.1f), 180f, 1.0f, p.SofaC, p);
            Props.Sofa("Sessel_2", lounge, new Vector3(8.1f, 0f, 1.9f), 0f, 1.0f, p.SofaC, p);
            Props.TableWithChairs("Tischgruppe_1", lounge, new Vector3(4.6f, 0f, 2.3f), 3, 0f, p);
            Props.TableWithChairs("Tischgruppe_2", lounge, new Vector3(4.6f, 0f, 5.6f), 2, 90f, p);

            // Pflanzen
            Props.Plant("Pflanze_1", g, new Vector3(9.3f, 0f, 0.7f), 1.2f, rnd, p);
            Props.Plant("Pflanze_2", g, new Vector3(-9.3f, 0f, 0.8f), 1.0f, rnd, p);
            Props.Plant("Pflanze_3", g, new Vector3(2.7f, 0f, 7.4f), 0.9f, rnd, p);
            Props.Plant("Pflanze_4", g, new Vector3(9.3f, 0f, 7.3f), 1.1f, rnd, p);
        }

        // ------------------------------------------------------------------ Halle

        static void BuildHallFurniture(Transform g, Palette p, System.Random rnd)
        {
            // Regale mit Fächern an der Rückwand
            var shelves = Geo.Group("Regale", g, Vector3.zero);
            for (int i = 0; i < 3; i++)
                Props.Shelf($"Faecherregal_{i + 1}", shelves, new Vector3(i * 2.9f, 0f, Length - 0.225f), 180f, 5, 4, ShelfContent.Belongings, rnd, p);
            Geo.Text("Schild_Regale", shelves, new Vector3(2.9f, 2.6f, Length - 0.02f), 0f,
                "Ablage für Wertsachen, Rucksäcke & Jacken", 0.18f, new Color(0.2f, 0.2f, 0.2f), p.TextMat);
            Props.Bench("Bank", shelves, new Vector3(2.9f, 0f, 28.75f), 0f, 3f, p);

            // Sofas mit Blick auf die Boulderwand
            var lounge = Geo.Group("Sitzbereich", g, Vector3.zero);
            Props.Sofa("Sofa_1", lounge, new Vector3(8.9f, 0f, 14.5f), HallSofaYaw, 2.2f, p.SofaB, p);
            Props.CoffeeTable("Couchtisch_1", lounge, new Vector3(7.7f, 0f, 14.5f), 90f, p);
            Props.Sofa("Sofa_2", lounge, HallSofa2Pos, HallSofaYaw, 2.2f, p.SofaA, p);
            Props.CoffeeTable("Couchtisch_2", lounge, new Vector3(7.7f, 0f, 21.5f), 90f, p);

            Props.Plant("Pflanze_Halle_1", g, new Vector3(9.3f, 0f, 17f), 1.1f, rnd, p);
            Props.Plant("Pflanze_Halle_2", g, new Vector3(9.3f, 0f, 25f), 1.0f, rnd, p);
            Props.Plant("Pflanze_Halle_3", g, new Vector3(9.2f, 0f, 8.7f), 1.0f, rnd, p);
        }

        static void BuildCustomers(Transform g, Palette p, List<WallPanel> panels, System.Random rnd)
        {
            // An der Wand
            ClimbingWallBuilder.AddClimber(g, panels[0], "Kunde_klettert_1", -1.0f, 1.75f, PersonBuilder.ClimbingA(), p.Outfit(0), panels[0].RouteMaterials[0], rnd);
            ClimbingWallBuilder.AddClimber(g, panels[2], "Kunde_klettert_2", 1.0f, 2.3f, PersonBuilder.ClimbingB(), p.Outfit(1), panels[2].RouteMaterials[1], rnd);

            // Auf der Matte
            float matY = ClimbingWallBuilder.MatHeight;
            PersonBuilder.Create("Kunde_Matte_sitzend_1", g, new Vector3(-5.0f, matY, 11.6f), -105f, PersonBuilder.SittingKneesUp(), p.Outfit(2), blocking: false);
            PersonBuilder.Create("Kunde_Matte_sitzend_2", g, MatSitterPos, -100f, PersonBuilder.SittingLegsOut(), p.Outfit(3), blocking: false);
            PersonBuilder.Create("Kunde_Matte_spottet", g, new Vector3(-7.6f, matY, 19.7f), -90f, PersonBuilder.Spotting(), p.Outfit(4));

            // Auf dem Sofa, auf der Bank und am Regal
            var sofaRot = Quaternion.Euler(0f, HallSofaYaw, 0f);
            PersonBuilder.Create("Kunde_Sofa", g, HallSofa2Pos + sofaRot * new Vector3(0.6f, 0f, -0.02f), HallSofaYaw, PersonBuilder.SittingOnSeat(), p.Outfit(5), blocking: false);
            PersonBuilder.Create("Kunde_Bank", g, new Vector3(2.2f, 0f, 28.75f), 180f, PersonBuilder.SittingOnSeat(), p.Outfit(6), blocking: false);
            PersonBuilder.Create("Kunde_Regal", g, new Vector3(6.3f, 0f, 29.0f), 0f, PersonBuilder.Reaching(), p.Outfit(7));
        }

        // ------------------------------------------------------------------ Licht

        static void BuildLighting(Transform g, Palette p)
        {
            // Tageslicht fällt durch die Fensterfront (rechts) in die Halle
            var sunGo = new GameObject("Sonne_Tageslicht");
            sunGo.transform.SetParent(g, false);
            sunGo.transform.localRotation = Quaternion.Euler(32f, -110f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.6f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.95f;
            RenderSettings.sun = sun;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.66f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.5f, 0.5f, 0.5f);
            RenderSettings.ambientGroundColor = new Color(0.3f, 0.28f, 0.26f);

            var lamps = new[]
            {
                new Vector2(-5f, 3.5f), new Vector2(4.5f, 3.5f),                           // Eingang
                new Vector2(-4f, 12f), new Vector2(3f, 12f), new Vector2(-4f, 19f),       // Halle
                new Vector2(3f, 19f), new Vector2(-4f, 26f), new Vector2(3f, 26f)
            };
            for (int i = 0; i < lamps.Length; i++)
            {
                var pos = new Vector3(lamps[i].x, Height - 0.03f, lamps[i].y);
                var panel = Geo.Box($"Deckenleuchte_{i + 1}", g, pos, new Vector3(1.4f, 0.06f, 0.5f), p.LampPanel, collider: false);
                Geo.NoShadows(panel);

                var lg = new GameObject($"Licht_{i + 1}");
                lg.transform.SetParent(g, false);
                lg.transform.localPosition = pos + Vector3.down * 0.3f;
                var l = lg.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 12f;
                l.intensity = 1.1f;
                l.color = new Color(1f, 0.96f, 0.9f);
                l.shadows = LightShadows.None;
            }
        }
    }
}
