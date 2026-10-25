using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Boulderhalle
{
    /// <summary>
    /// Baut das explorative Szenario "Sturzbereiche freihalten" in die Halle:
    /// Chalkbag und Bürste auf der Matte, zwei markierte Startgriffe (belegte und freie Route),
    /// Szenario-Steuerung und Pop-up-Fenster.
    /// </summary>
    public static class ScenarioBuilder
    {
        // Texte aus der Konzeption
        const string ChalkbagText = "Hier ist das sogenannte Chalk drin. Das nutzt du, um deine Hände für einen besseren Griff trocken zu halten.";
        const string BrushText = "Tritte und Griffe müssen regelmäßig von Chalk befreit werden. Dafür helfen dir diese kleinen Bürsten.";
        const string LoseTitle = "Du hast verloren!";
        const string LoseText = "Du hast gerade zwei Menschen in Gefahr gebracht. Achte immer auf deine Umgebung und ob jemand an der Wand bouldert!";
        const string WinTitle = "Glückwunsch, hier bist du richtig!";
        const string WinText = "Du startest deine Route an den markierten Startgriffen. Es dürfen lediglich Griffe der gleichen Farbe verwendet werden. " +
                               "Du hast deine Route erfolgreich beendet, wenn du oben am markierten Topgriff angekommen bist.";

        /// <param name="occupiedStart">Startgriff der Route, an der ein Kunde bouldert.</param>
        /// <param name="freeStart">Startgriff einer freien Route.</param>
        /// <param name="matItemsPos">Position auf der Matte neben dem sitzenden Kunden (Chalkbag und Bürste).</param>
        public static GameObject Build(Palette p, Transform occupiedStart, Transform freeStart, Vector3 matItemsPos)
        {
            var root = new GameObject("Szenario_Sturzbereich");
            var t = root.transform;

            var manager = root.AddComponent<ScenarioManager>();
            manager.popup = BuildPopup(t);

            // Chalkbag und Bürste neben dem sitzenden Kunden auf der Matte
            var chalkbag = Chalkbag(t, matItemsPos + new Vector3(0.05f, 0f, 0.65f), p);
            Setup(chalkbag.gameObject, ScenarioObjectKind.Info, "Chalkbag", ChalkbagText);
            Marker(t, "Markierung_Chalkbag", chalkbag.position, Vector3.up, 0.42f, p);

            var brush = Brush(t, matItemsPos + new Vector3(0.1f, 0f, -0.7f), p);
            Setup(brush.gameObject, ScenarioObjectKind.Info, "Bürste", BrushText);
            Marker(t, "Markierung_Buerste", brush.position, Vector3.up, 0.45f, p);

            // Startgriffe an der Wand
            SetupStartHold(occupiedStart, ScenarioObjectKind.StartHoldOccupied, LoseTitle, LoseText);
            Marker(t, "Markierung_Startgriff_belegt", occupiedStart.position, -occupiedStart.forward, 0.34f, p);
            SetupStartHold(freeStart, ScenarioObjectKind.StartHoldFree, WinTitle, WinText);
            Marker(t, "Markierung_Startgriff_frei", freeStart.position, -freeStart.forward, 0.34f, p);

            return root;
        }

        static void Setup(GameObject go, ScenarioObjectKind kind, string title, string message)
        {
            var o = go.AddComponent<ScenarioObject>();
            o.kind = kind;
            o.title = title;
            o.message = message;
        }

        static void SetupStartHold(Transform anchor, ScenarioObjectKind kind, string title, string message)
        {
            Setup(anchor.gameObject, kind, title, message);
            // Trigger-Collider nur für den Desktop-Test (Anklicken); in VR zählt die Nähe des Controllers
            var c = anchor.gameObject.AddComponent<SphereCollider>();
            c.isTrigger = true;
            c.radius = 0.12f;
        }

        // ------------------------------------------------------------------ Objekte

        static Transform Chalkbag(Transform parent, Vector3 pos, Palette p)
        {
            var g = Geo.Group("Chalkbag", parent, pos, 25f);
            var bag = p.Bags[4];
            Geo.Cyl("Beutel", g, new Vector3(0f, 0.085f, 0f), 0.16f, 0.17f, bag, collider: false);
            Geo.Cyl("Rand", g, new Vector3(0f, 0.175f, 0f), 0.17f, 0.025f, p.Black, collider: false);
            Geo.Cyl("Chalk", g, new Vector3(0f, 0.18f, 0f), 0.135f, 0.01f, p.White, collider: false);
            Geo.Box("Guertelschlaufe", g, new Vector3(0f, 0.09f, -0.085f), new Vector3(0.05f, 0.1f, 0.015f), p.Black, collider: false);
            Geo.Box("Pinselhalter", g, new Vector3(0.08f, 0.1f, 0.02f), new Vector3(0.012f, 0.08f, 0.03f), p.Black, collider: false);

            // Großzügiger Collider, damit das kleine Objekt leicht mit dem Strahl zu treffen ist
            var c = g.gameObject.AddComponent<BoxCollider>();
            c.center = new Vector3(0f, 0.12f, 0f);
            c.size = new Vector3(0.32f, 0.26f, 0.32f);
            return g;
        }

        static Transform Brush(Transform parent, Vector3 pos, Palette p)
        {
            var g = Geo.Group("Buerste", parent, pos, -30f);
            Geo.Box("Stiel", g, new Vector3(0f, 0.018f, 0.07f), new Vector3(0.035f, 0.03f, 0.2f), p.Wood, collider: false);
            Geo.Box("Kopf", g, new Vector3(0f, 0.025f, -0.08f), new Vector3(0.06f, 0.03f, 0.11f), p.Brand, collider: false);
            Geo.Box("Borsten", g, new Vector3(0f, 0.008f, -0.08f), new Vector3(0.055f, 0.016f, 0.1f), p.White, collider: false);

            var c = g.gameObject.AddComponent<BoxCollider>();
            c.center = new Vector3(0f, 0.08f, 0f);
            c.size = new Vector3(0.28f, 0.18f, 0.42f);
            return g;
        }

        /// <summary>Leuchtender Ring (pulsiert) und schwebende Raute über dem Objekt.</summary>
        static void Marker(Transform parent, string name, Vector3 position, Vector3 normal, float size, Palette p)
        {
            var g = new GameObject(name).transform;
            g.SetParent(parent, false);
            g.position = position;
            g.rotation = Quaternion.FromToRotation(Vector3.up, normal);

            var ring = Geo.Cyl("Leuchtring", g, new Vector3(0f, 0.004f, 0f), size, 0.008f, p.Highlight, collider: false);
            Geo.NoShadows(ring);

            // Raute schwebt über dem Objekt (in Weltrichtung "oben"), bei Wandgriffen etwas vor der Wand
            var diamondPos = g.InverseTransformPoint(position + Vector3.up * 0.32f + normal * (normal == Vector3.up ? 0f : 0.12f));
            var diamond = Geo.Box("Raute", g, diamondPos, new Vector3(0.07f, 0.07f, 0.07f), p.Highlight, collider: false);
            diamond.transform.rotation = Quaternion.Euler(45f, 0f, 45f);
            Geo.NoShadows(diamond);

            var pulse = g.gameObject.AddComponent<PulseMarker>();
            pulse.ring = ring.transform;
            pulse.diamond = diamond.transform;
        }

        // ------------------------------------------------------------------ Pop-up (World-Space-UI)

        static FeedbackPopup BuildPopup(Transform parent)
        {
            var go = new GameObject("Popup", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster), typeof(TrackedDeviceGraphicRaycaster));
            go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            go.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 3f;

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(900f, 580f);
            rt.localScale = Vector3.one * 0.001f; // 1 px = 1 mm -> 0,9 m x 0,58 m

            var background = UiImage("Hintergrund", rt, new Color(0.97f, 0.97f, 0.96f, 0.98f));
            Stretch(background.rectTransform, 0f, 0f, 0f, 0f);

            var header = UiImage("Kopfzeile", rt, Color.gray);
            Anchor(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -130f), Vector2.zero);

            var title = UiText("Titel", header.rectTransform, 44f, Color.white, FontStyles.Bold);
            Stretch(title.rectTransform, 30f, 30f, 10f, 10f);

            var body = UiText("Text", rt, 34f, new Color(0.12f, 0.12f, 0.12f), FontStyles.Normal);
            body.enableAutoSizing = true;
            body.fontSizeMin = 24f;
            body.fontSizeMax = 34f;
            Stretch(body.rectTransform, 50f, 50f, 155f, 150f);

            var buttonImage = UiImage("Knopf", rt, Color.gray);
            Anchor(buttonImage.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-230f, 35f), new Vector2(230f, 125f));
            var button = buttonImage.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.pressedColor = new Color(0.65f, 0.65f, 0.65f);
            button.colors = colors;

            var label = UiText("Beschriftung", buttonImage.rectTransform, 36f, Color.white, FontStyles.Bold);
            Stretch(label.rectTransform, 10f, 10f, 5f, 5f);

            var popup = go.AddComponent<FeedbackPopup>();
            popup.header = header;
            popup.titleText = title;
            popup.bodyText = body;
            popup.button = button;
            popup.buttonLabel = label;

            go.SetActive(false);
            return popup;
        }

        static Image UiImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        static TextMeshProUGUI UiText(string name, Transform parent, float size, Color color, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.color = color;
            text.fontStyle = style;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        static void Stretch(RectTransform rt, float left, float right, float top, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }
    }
}
