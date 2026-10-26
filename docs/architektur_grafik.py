"""Erzeugt die Grafik "Technologische Architektur" als SVG in Originalgröße.

Die Grafik ist 16 cm breit (übliche Textbreite auf DIN A4). Alle Maße sind in Punkt (pt) angegeben,
die Schrift ist durchgehend Arial 11 pt. Bei Einfügen in 100 % Größe entspricht sie Arial 11 pt im Dokument.

Aufruf:  python3 architektur_grafik.py
PNG (300 dpi): rsvg-convert -d 300 -p 300 Technologische_Architektur.svg -o Technologische_Architektur.png
"""
from xml.sax.saxutils import escape

W, H = 453.5, 290          # 16,0 cm x 10,2 cm (1 pt = 1/72 Zoll)
FS = 11                     # Schriftgröße in pt
LH = 13.2                   # Zeilenabstand
FONT = "Arial"
INK, DARK, GREY = "#1F2328", "#3A3A3A", "#5B6270"
out = []


def text(x, y, lines, anchor="middle", weight="normal", color=INK, style="normal"):
    """Mehrzeiliger Text, vertikal um y zentriert."""
    if isinstance(lines, str):
        lines = [lines]
    y0 = y - (len(lines) - 1) * LH / 2 + FS * 0.35
    for i, l in enumerate(lines):
        out.append(f'<text x="{x}" y="{y0 + i * LH:.1f}" font-family="{FONT}" font-size="{FS}" text-anchor="{anchor}" '
                   f'font-weight="{weight}" font-style="{style}" fill="{color}">{escape(l)}</text>')


def box(x, y, w, h, fill="#FFFFFF", stroke=INK, sw=0.9, rx=2):
    out.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}"/>')
    return dict(l=(x, y + h / 2), r=(x + w, y + h / 2), t=(x + w / 2, y), b=(x + w / 2, y + h), cx=x + w / 2, cy=y + h / 2)


def block(x, y, w, h, title, sub=(), dark=False):
    """Kasten mit fettem Titel und optionalen Unterzeilen."""
    b = box(x, y, w, h, fill=DARK if dark else "#FFFFFF", stroke=DARK if dark else INK)
    lines = [title] + list(sub)
    y0 = b["cy"] - (len(lines) - 1) * LH / 2
    text(b["cx"], y0, title, weight="bold", color="#FFFFFF" if dark else INK)
    for i, s in enumerate(sub, start=1):
        text(b["cx"], y0 + i * LH, s, color="#E3E6EA" if dark else GREY)
    return b


def arrow(*pts, both=False):
    d = " ".join(f"{x},{y}" for x, y in pts)
    start = ' marker-start="url(#pfeil)"' if both else ""
    out.append(f'<polyline points="{d}" fill="none" stroke="{INK}" stroke-width="0.9"{start} marker-end="url(#pfeil)"/>')


def label(x, y, lines, anchor="start"):
    text(x, y, lines, anchor=anchor, style="italic")


out.append(f'<rect width="{W}" height="{H}" fill="#FFFFFF"/>')
out.append(f'<defs><marker id="pfeil" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="6" markerHeight="6" '
           f'orient="auto-start-reverse"><path d="M0,0 L10,5 L0,10 z" fill="{INK}"/></marker></defs>')

# ---------------- Entwicklung ----------------
box(4, 4, 160, H - 8, sw=1)
text(84, 18, "Entwicklung", weight="bold", color=GREY)

konz = block(16, 30, 136, 46, "Konzeption", ["Anforderungen, Szenario,", "Texte (Autor/in)"])
claude = block(16, 126, 136, 34, "Claude Code", ["KI-Assistent (Anthropic)"])
arrow(konz["b"], claude["t"])
label(88, 101, ["Vorgaben in", "natürlicher", "Sprache"])
code = block(26, 184, 116, 22, "C#-Skripte & Shader", dark=True)
arrow(claude["b"], code["t"])
label(88, 172, "generiert")
git = block(16, 234, 136, 46, "Git / GitHub", ["Versionierung,", "GitHub Desktop"])
arrow(code["b"], git["t"])
label(88, 220, "Commit / Push")

# ---------------- Anwendung ----------------
box(172, 4, W - 176, H - 8, sw=1)
text((172 + W - 4) / 2, 18, "Anwendung", weight="bold", color=GREY)

unity = block(184, 30, 150, 58, "Unity 6", ["Spiel-Engine und Editor,", "Szenengenerator", "(Menü „Boulderhalle“)"])
aus = block(352, 30, 90, 58, "Ausgabe", ["Meta Quest", "(OpenXR)", "Desktop-Test"])
arrow(unity["r"], aus["l"])

stat = block(184, 116, 120, 34, "Statisches Modell", ["Halle"], dark=True)
dyn = block(322, 116, 120, 34, "Dynamisches Modell", ["Szenario"], dark=True)
arrow(stat["r"], dyn["l"], both=True)

script = block(262, 206, 140, 58, "Scripting (C#)", ["Szenengenerierung,", "Interaktion, Feedback", "(Pop-ups, Töne)"])
arrow((script["cx"] - 40, 206), (script["cx"] - 40, 180), (stat["cx"], 180), stat["b"])
arrow((script["cx"] + 40, 206), (script["cx"] + 40, 180), (dyn["cx"], 180), dyn["b"])
label(script["cx"], 172, "C#", anchor="middle")

arrow(git["r"], (200, 257), (200, 235), script["l"])
label(206, 271, ["Pull in das", "Unity-Projekt"], anchor="start")

svg = (f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}pt" height="{H}pt" viewBox="0 0 {W} {H}">\n'
       + "\n".join(out) + "\n</svg>\n")
open("Technologische_Architektur.svg", "w", encoding="utf-8").write(svg)
print("Technologische_Architektur.svg geschrieben")
