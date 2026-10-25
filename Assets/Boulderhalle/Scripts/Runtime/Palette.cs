using UnityEngine;

namespace Boulderhalle
{
    /// <summary>Kleidung/Farben einer Person.</summary>
    public struct Outfit
    {
        public Material Skin, Hair, Top, Bottom, Shoes, Eyes;
    }

    /// <summary>Alle Materialien der Boulderhalle an einer Stelle.</summary>
    public class Palette
    {
        readonly MaterialLibrary lib;

        public readonly Material Wall, WallOutside, FloorEntrance, FloorHall, Ceiling, Grass, Paving, Doormat;
        public readonly Material Wood, WoodDark, CounterFront, Countertop, Metal, DarkPlastic;
        public readonly Material ScreenCustomer, ScreenStaff, Glass, Frame;
        public readonly Material SofaA, SofaB, SofaC, ChairSeat;
        public readonly Material Terracotta, PotWhite, Soil, Leaf, LeafDark, Stem;
        public readonly Material MatTop, MatSide, Panel, PanelAlt, Volume, LampPanel, Highlight;
        public readonly Material White, Black, Red, Brand, TextMat;
        public readonly Material[] Skins, Hairs, Tops, Bottoms, Shoes, Bags;

        public Palette(MaterialLibrary lib)
        {
            this.lib = lib;

            Wall = Solid("Wand", Hex("#ECE8E1"));
            WallOutside = Solid("Fassade", Hex("#5B6770"));
            FloorEntrance = Solid("Boden_Eingang", Hex("#B98B5E"), 0.35f);
            FloorHall = Solid("Boden_Halle", Hex("#8E9196"), 0.15f);
            Ceiling = Solid("Decke", Hex("#D8D8D8"));
            Grass = Solid("Rasen", Hex("#5E8C3A"), 0.05f);
            Paving = Solid("Pflaster", Hex("#9A958C"), 0.1f);
            Doormat = Solid("Fussmatte", Hex("#3A3A3A"), 0f);

            Wood = Solid("Holz", Hex("#C49A6C"), 0.3f);
            WoodDark = Solid("Holz_Dunkel", Hex("#6B4A30"), 0.3f);
            CounterFront = Solid("Tresen_Front", Hex("#2F3B45"), 0.3f);
            Countertop = Solid("Tresen_Platte", Hex("#E6E1D8"), 0.6f);
            Metal = lib.Get("Metall", Hex("#9EA3A8"), 0.6f, 0.8f);
            DarkPlastic = Solid("Kunststoff_Dunkel", Hex("#1E1F22"), 0.4f);

            ScreenCustomer = lib.Emissive("Bildschirm_Anmeldung", Hex("#1D5FA8"), Hex("#2C7BD6") * 1.2f);
            ScreenStaff = lib.Emissive("Bildschirm_Mitarbeiter", Hex("#1F6B4A"), Hex("#2E9C6B"));
            Glass = lib.Transparent("Glas", new Color(0.75f, 0.88f, 0.95f, 0.18f));
            Frame = Solid("Fensterrahmen", Hex("#2B2B2B"), 0.4f);

            SofaA = Solid("Stoff_Petrol", Hex("#2E6E73"), 0.1f);
            SofaB = Solid("Stoff_Grau", Hex("#6E6A66"), 0.1f);
            SofaC = Solid("Stoff_Senf", Hex("#C9962E"), 0.1f);
            ChairSeat = Solid("Stuhl", Hex("#E9E4DA"), 0.3f);

            Terracotta = Solid("Terrakotta", Hex("#B5643C"), 0.2f);
            PotWhite = Solid("Topf_Weiss", Hex("#EDEDED"), 0.5f);
            Soil = Solid("Erde", Hex("#3B2A1E"), 0f);
            Leaf = Solid("Blatt", Hex("#3F8F3A"), 0.25f);
            LeafDark = Solid("Blatt_Dunkel", Hex("#2D6B2E"), 0.25f);
            Stem = Solid("Stamm", Hex("#5A4632"), 0.1f);

            MatTop = Solid("Matte", Hex("#2E4B7C"), 0.1f);
            MatSide = Solid("Matte_Kante", Hex("#F0C419"), 0.1f);
            Panel = Solid("Wandplatte", Hex("#D9C6A5"), 0.2f);
            PanelAlt = Solid("Wandplatte_Grau", Hex("#BFC3C7"), 0.2f);
            Volume = Solid("Volumen", Hex("#4A5560"), 0.2f);
            LampPanel = lib.Emissive("Deckenleuchte", Color.white, new Color(1f, 0.97f, 0.9f) * 2f);
            Highlight = lib.Emissive("Markierung", Hex("#19E3FF"), Hex("#19E3FF") * 1.6f);

            White = Solid("Weiss", Hex("#F5F5F5"), 0.3f);
            Black = Solid("Schwarz", Hex("#151515"), 0.4f);
            Red = Solid("Rot", Hex("#C62828"), 0.4f);
            Brand = Solid("Firmenfarbe", Hex("#E8572A"), 0.3f);
            TextMat = lib.Text();

            Skins = Set("Haut", 0.3f, "#F1C9A5", "#C68B59", "#8D5524", "#E0AC69");
            Hairs = Set("Haare", 0.3f, "#2B1D14", "#6B4423", "#D8B26E", "#111111", "#A0522D");
            Tops = Set("Shirt", 0.15f, "#2F6FD6", "#D63A3A", "#3BA55C", "#F2C318", "#8A4FC7", "#EDEDED", "#21B5B0");
            Bottoms = Set("Hose", 0.1f, "#2A2F3A", "#4B5320", "#1C1C1C", "#5C6B7A", "#7A5C3E");
            Shoes = Set("Schuhe", 0.3f, "#C0392B", "#2C3E50", "#E67E22", "#16A085");
            Bags = Set("Tasche", 0.2f, "#1F3A5F", "#7D2E2E", "#2E5E3B", "#444444", "#C9A227", "#5B3F86");
        }

        public Material Solid(string name, Color color, float smoothness = 0.2f) => lib.Get(name, color, smoothness);

        public Outfit Outfit(int i) => new Outfit
        {
            Skin = Skins[i % Skins.Length],
            Hair = Hairs[(i * 3 + 1) % Hairs.Length],
            Top = Tops[i % Tops.Length],
            Bottom = Bottoms[(i * 2) % Bottoms.Length],
            Shoes = Shoes[i % Shoes.Length],
            Eyes = Black
        };

        Material[] Set(string prefix, float smoothness, params string[] hex)
        {
            var mats = new Material[hex.Length];
            for (int i = 0; i < hex.Length; i++) mats[i] = lib.Get($"{prefix}_{i + 1}", Hex(hex[i]), smoothness);
            return mats;
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
