"""Erzeugt das UML-Aktivitätsdiagramm "Ablauf eines neuen Kunden" als SVG (feste Anordnung).

Aufruf:  python3 aktivitaetsdiagramm_kundenablauf.py
Ergebnis: Aktivitaetsdiagramm_Kundenablauf.svg (PNG z. B. mit: rsvg-convert -z 2 ... .svg -o ... .png)
Texte und Positionen lassen sich unten direkt anpassen.
"""
from xml.sax.saxutils import escape

W, H = 1420, 1090
FONT = "Arial, Liberation Sans, Helvetica, sans-serif"
LINE, KUNDE, SYSTEM, RAUTE = "#2F3B45", "#D6EAF8", "#FFFFFF", "#F5F5F5"
NOTIZ, NOTIZ_RAND = "#FFF8DC", "#C9A227"
out = []


def text(x, y, lines, size=15, anchor="middle", weight="normal", color=LINE):
    if isinstance(lines, str):
        lines = [lines]
    lh = size * 1.3
    y0 = y - (len(lines) - 1) * lh / 2 + size * 0.35
    for i, l in enumerate(lines):
        out.append(f'<text x="{x}" y="{y0 + i * lh:.1f}" font-family="{FONT}" font-size="{size}" '
                   f'text-anchor="{anchor}" font-weight="{weight}" fill="{color}">{escape(l)}</text>')


def action(cx, cy, w, h, lines, fill=SYSTEM):
    out.append(f'<rect x="{cx - w / 2}" y="{cy - h / 2}" width="{w}" height="{h}" rx="16" '
               f'fill="{fill}" stroke="{LINE}" stroke-width="1.6"/>')
    text(cx, cy, lines)
    return dict(top=(cx, cy - h / 2), bottom=(cx, cy + h / 2), left=(cx - w / 2, cy), right=(cx + w / 2, cy))


def diamond(cx, cy, hw, hh, lines=None):
    pts = f"{cx},{cy - hh} {cx + hw},{cy} {cx},{cy + hh} {cx - hw},{cy}"
    out.append(f'<polygon points="{pts}" fill="{RAUTE}" stroke="{LINE}" stroke-width="1.6"/>')
    if lines:
        text(cx, cy, lines, size=14)
    return dict(top=(cx, cy - hh), bottom=(cx, cy + hh), left=(cx - hw, cy), right=(cx + hw, cy))


def flow(*pts, guard=None, gpos=None, ganchor="middle"):
    d = " ".join(f"{x},{y}" for x, y in pts)
    out.append(f'<polyline points="{d}" fill="none" stroke="{LINE}" stroke-width="1.6" marker-end="url(#pfeil)"/>')
    if guard:
        text(gpos[0], gpos[1], guard, size=14, anchor=ganchor)


def note(x, y, w, lines, anchor_pt=None, attach=None):
    h = len(lines) * 19 + 16
    f = 14
    out.append(f'<path d="M{x},{y} h{w - f} l{f},{f} v{h - f} h{-w} z" fill="{NOTIZ}" stroke="{NOTIZ_RAND}" stroke-width="1.3"/>')
    out.append(f'<path d="M{x + w - f},{y} v{f} h{f}" fill="none" stroke="{NOTIZ_RAND}" stroke-width="1.3"/>')
    for i, l in enumerate(lines):
        out.append(f'<text x="{x + 12}" y="{y + 24 + i * 19}" font-family="{FONT}" font-size="13.5" fill="#333">{escape(l)}</text>')
    if attach:
        (x1, y1), (x2, y2) = attach
        out.append(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{NOTIZ_RAND}" stroke-width="1.3" stroke-dasharray="6,4"/>')
    return h


C = 760
out.append(f'<rect width="{W}" height="{H}" fill="#FFFFFF"/>')
out.append(f'<defs><marker id="pfeil" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="9" markerHeight="9" orient="auto-start-reverse">'
           f'<path d="M0,0 L10,5 L0,10 z" fill="{LINE}"/></marker></defs>')
text(W / 2, 38, "Ablauf eines neuen Kunden in der VR-Boulderhalle – UML-Aktivitätsdiagramm", size=20, weight="bold")

# Hauptpfad
out.append(f'<circle cx="{C}" cy="90" r="13" fill="#000"/>')
m1 = diamond(C, 150, 15, 15)
flow((C, 103), m1["top"])
a1 = action(C, 225, 320, 58, ["Kunde kommt am Startpunkt", "in der Eingangshalle an"], KUNDE)
flow(m1["bottom"], a1["top"])
a2 = action(C, 315, 320, 50, "Mitarbeiter begrüßt den Kunden")
flow(a1["bottom"], a2["top"])
note(960, 278, 370, ["„Hallo und herzlich willkommen! Du bist neu hier?",
                     "Dann mach' bitte unsere Sicherheitseinweisung.",
                     "Betritt dafür den sportlichen Hallenbereich.“"],
     attach=(a2["right"], (960, 315)))
a3 = action(C, 400, 320, 50, "Kunde betritt die Boulderhalle", KUNDE)
flow(a2["bottom"], a3["top"])
m2 = diamond(C, 475, 15, 15)
flow(a3["bottom"], m2["top"])
a4 = action(C, 550, 320, 50, "Kunde klickt ein virtuelles Objekt an", KUNDE)
flow(m2["bottom"], a4["top"])
d = diamond(C, 645, 100, 46, ["Angeklicktes", "Objekt?"])
flow(a4["bottom"], d["top"])

# Zweige
by = 790
b1 = action(475, by, 210, 56, ["Pop-up-Fenster", "„Bürste“ anzeigen"])
b2 = action(705, by, 210, 56, ["Pop-up-Fenster", "„Chalkbag“ anzeigen"])
b3 = action(950, by, 190, 56, "Szenario beenden")
b4 = action(1200, by, 240, 56, ["Kunde wird zum Startpunkt", "zurückportiert"])
flow(d["left"], (475, 645), b1["top"], guard="[Bürste]", gpos=(485, 705), ganchor="start")
flow(d["bottom"], (C, 715), (705, 715), b2["top"], guard="[Chalkbag]", gpos=(715, 742), ganchor="start")
flow(d["bottom"], (C, 715), (950, 715), b3["top"], guard="[pinker Startgriff]", gpos=(960, 742), ganchor="start")
flow(d["right"], (1200, 645), b4["top"], guard="[gelber Startgriff]", gpos=(1210, 705), ganchor="start")

# Bürste/Chalkbag -> zurück zum Anklicken
m3 = diamond(590, 880, 15, 15)
flow(b1["bottom"], (475, 880), m3["left"])
flow(b2["bottom"], (705, 880), m3["right"])
flow(m3["bottom"], (590, 935), (30, 935), (30, 475), m2["left"])

# Gelber Startgriff -> zurück zum Startpunkt
flow(b4["right"], (1370, by), (1370, 150), m1["right"])

# Pinker Startgriff -> Ende
flow(b3["bottom"], (950, 875))
out.append(f'<circle cx="950" cy="890" r="15" fill="none" stroke="#000" stroke-width="1.8"/>')
out.append(f'<circle cx="950" cy="890" r="9.5" fill="#000"/>')

# Notizen zu den Pop-ups
note(50, 752, 290, ["„Tritte und Griffe müssen regelmäßig",
                    "von Chalk befreit werden. Dafür helfen",
                    "dir diese kleinen Bürsten.“"], attach=((340, 790), b1["left"]))
note(620, 965, 320, ["„Hier ist das sogenannte Chalk drin.",
                     "Das nutzt du, um deine Hände für einen",
                     "besseren Griff trocken zu halten.“"], attach=((770, 965), (770, 818)))

# Legende
lx, ly = 1080, 960
out.append(f'<rect x="{lx}" y="{ly}" width="290" height="104" rx="8" fill="#FAFAFA" stroke="#999"/>')
text(lx + 14, ly + 22, "Legende", size=14, anchor="start", weight="bold")
out.append(f'<rect x="{lx + 14}" y="{ly + 36}" width="44" height="22" rx="8" fill="{KUNDE}" stroke="{LINE}"/>')
text(lx + 70, ly + 47, "Aktion des Kunden", size=13.5, anchor="start")
out.append(f'<rect x="{lx + 14}" y="{ly + 68}" width="44" height="22" rx="8" fill="{SYSTEM}" stroke="{LINE}"/>')
text(lx + 70, ly + 79, "Aktion der VR-Lernumgebung", size=13.5, anchor="start")

svg = (f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">\n'
       + "\n".join(out) + "\n</svg>\n")
open("Aktivitaetsdiagramm_Kundenablauf.svg", "w", encoding="utf-8").write(svg)
print("Aktivitaetsdiagramm_Kundenablauf.svg geschrieben")
