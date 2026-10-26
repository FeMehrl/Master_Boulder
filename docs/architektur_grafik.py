"""Erzeugt die Grafik "Technologische Architektur" als SVG.

Aufruf:  python3 architektur_grafik.py
Ergebnis: Technologische_Architektur.svg (PNG z. B. mit: rsvg-convert -z 2.5 ... .svg -o ... .png)
"""
from xml.sax.saxutils import escape

W, H = 1420, 660
FONT = "Arial, Liberation Sans, Helvetica, sans-serif"
INK, DARK, GREY = "#1F2328", "#3A3A3A", "#6B7280"
out = []


def text(x, y, lines, size=15, anchor="middle", weight="normal", color=INK, style="normal"):
    if isinstance(lines, str):
        lines = [lines]
    lh = size * 1.28
    y0 = y - (len(lines) - 1) * lh / 2 + size * 0.35
    for i, l in enumerate(lines):
        out.append(f'<text x="{x}" y="{y0 + i * lh:.1f}" font-family="{FONT}" font-size="{size}" text-anchor="{anchor}" '
                   f'font-weight="{weight}" font-style="{style}" fill="{color}">{escape(l)}</text>')


def box(x, y, w, h, fill="#FFFFFF", stroke=INK, sw=2, rx=4):
    out.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{rx}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}"/>')
    return dict(l=(x, y + h / 2), r=(x + w, y + h / 2), t=(x + w / 2, y), b=(x + w / 2, y + h), cx=x + w / 2, cy=y + h / 2)


def dark(x, y, w, h, lines, sub=None):
    b = box(x, y, w, h, fill=DARK, stroke=DARK, sw=1, rx=3)
    if sub:
        text(b["cx"], b["cy"] - 12, lines, size=15, color="#FFFFFF", weight="bold")
        text(b["cx"], b["cy"] + 12, sub, size=12, color="#D9DDE3")
    else:
        text(b["cx"], b["cy"], lines, size=15, color="#FFFFFF", weight="bold")
    return b


def arrow(*pts, dashed=False, both=False):
    d = " ".join(f"{x},{y}" for x, y in pts)
    dash = ' stroke-dasharray="7,5"' if dashed else ""
    start = ' marker-start="url(#pfeil)"' if both else ""
    out.append(f'<polyline points="{d}" fill="none" stroke="{INK}" stroke-width="1.8"{dash}{start} marker-end="url(#pfeil)"/>')


def label(x, y, s, anchor="start"):
    text(x, y, s, size=13.5, anchor=anchor, style="italic")


def monitor(x, y):
    out.append(f'<rect x="{x}" y="{y}" width="58" height="40" rx="3" fill="none" stroke="{INK}" stroke-width="2.4"/>')
    out.append(f'<path d="M{x + 22},{y + 40} v9 M{x + 36},{y + 40} v9 M{x + 15},{y + 50} h28" stroke="{INK}" stroke-width="2.4" fill="none"/>')
    out.append(f'<rect x="{x + 68}" y="{y - 4}" width="20" height="54" rx="2" fill="none" stroke="{INK}" stroke-width="2.4"/>')
    out.append(f'<path d="M{x + 72},{y + 6} h12 M{x + 72},{y + 13} h12" stroke="{INK}" stroke-width="2"/>')


def headset(x, y):
    out.append(f'<path d="M{x},{y + 8} q0,-8 10,-8 h56 q10,0 10,8 v22 q0,10 -10,10 h-18 q-5,0 -8,-6 q-3,-5 -6,0 '
               f'q-3,6 -8,6 h-16 q-10,0 -10,-10 z" fill="none" stroke="{INK}" stroke-width="2.4"/>')
    text(x + 38, y + 19, "VR", size=12, weight="bold")


out.append(f'<rect width="{W}" height="{H}" fill="#FFFFFF"/>')
out.append(f'<defs><marker id="pfeil" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="8" markerHeight="8" orient="auto-start-reverse">'
           f'<path d="M0,0 L10,5 L0,10 z" fill="{INK}"/></marker></defs>')

# ---------------- Panel links: Entwicklung ----------------
box(15, 15, 470, 630, sw=2)
text(250, 40, "Entwicklung", size=16, weight="bold", color=GREY)

konz = box(65, 65, 370, 92)
text(konz["cx"], 95, "Konzeption", size=19, weight="bold")
text(konz["cx"], 128, ["Anforderungen · Szenario · Texte", "(Autor/in)"], size=13.5, color=GREY)

claude = box(65, 222, 370, 100)
text(claude["cx"], 255, "Claude Code", size=22, weight="bold")
text(claude["cx"], 290, ["KI-Assistent (Anthropic)"], size=14, color=GREY)
arrow(konz["b"], claude["t"])
label(260, 190, "Vorgaben in natürlicher Sprache")

code = dark(115, 385, 270, 50, "C#-Skripte & Shader")
arrow(claude["b"], code["t"])
label(260, 354, "generiert")

git = box(65, 500, 370, 92)
text(git["cx"], 530, "Git / GitHub", size=19, weight="bold")
text(git["cx"], 562, ["Versionierung · Repository", "Abgleich über GitHub Desktop"], size=13.5, color=GREY)
arrow(code["b"], git["t"])
label(260, 468, "Commit / Push")

# ---------------- Panel rechts: Anwendung ----------------
box(510, 15, 895, 630, sw=2)
text(958, 40, "Anwendung", size=16, weight="bold", color=GREY)

pk = [("OpenXR", 92), ("XR Interaction", 118), ("Input System", 110), ("URP", 62), ("uGUI · TMP", 98)]
x = 545
pkb = []
for name, w in pk:
    b = box(x, 60, w, 46, sw=1.6)
    lines = ["XR Interaction", "Toolkit"] if name == "XR Interaction" else name
    text(b["cx"], b["cy"], lines, size=12.5, weight="bold")
    pkb.append(b)
    x += w + 8

unity = box(545, 120, 512, 110, sw=2)
text(unity["cx"], 162, "Unity 6", size=40, weight="bold")
text(unity["cx"], 205, "Spiel-Engine · Editor · Szenengenerator (Menü „Boulderhalle“)", size=13.5, color=GREY)

stat = dark(555, 285, 230, 64, "Statisches Modell", "Halle aus Unity-Grundkörpern")
dyn = dark(820, 285, 230, 64, "Dynamisches Modell", "Szenario „Sturzbereiche freihalten“")
arrow(stat["r"], dyn["l"], both=True)

script = box(640, 430, 325, 120)
text(script["cx"], 462, "Scripting", size=19, weight="bold")
text(script["cx"], 506, ["Szenengenerierung · Interaktion", "Feedback (Pop-ups, Töne)"], size=13.5, color=GREY)
arrow((script["cx"] - 70, 430), (script["cx"] - 70, 392), (stat["cx"], 392), stat["b"])
arrow((script["cx"] + 70, 430), (script["cx"] + 70, 392), (dyn["cx"], 392), dyn["b"])
label(script["cx"], 410, "C#", anchor="middle")

# Repository -> Unity-Projekt
arrow(git["r"], (560, 546), (560, 490), script["l"])
text(600, 575, ["Pull in das", "Unity-Projekt"], size=13, style="italic")


# Ausgabe
aus = box(1110, 120, 270, 110, sw=2)
text(aus["cx"], 148, "Ausgabe", size=18, weight="bold")
text(aus["cx"], 192, ["Meta Quest (OpenXR)", "Quest Link / Android-Build"], size=14)
arrow(unity["r"], aus["l"])
monitor(1140, 300)
headset(1270, 305)
text(1185, 378, ["Desktop-", "Testmodus"], size=12.5, color=GREY)
text(1308, 378, ["VR-Brille"], size=12.5, color=GREY)
out.append(f'<line x1="1245" y1="270" x2="1245" y2="400" stroke="#D0D4DA" stroke-width="1.2"/>')

svg = (f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">\n'
       + "\n".join(out) + "\n</svg>\n")
open("Technologische_Architektur.svg", "w", encoding="utf-8").write(svg)
print("Technologische_Architektur.svg geschrieben")
