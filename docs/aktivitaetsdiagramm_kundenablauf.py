"""Erzeugt das UML-Aktivitätsdiagramm "Ablauf eines neuen Kunden" als SVG in Originalgröße.

Die Grafik ist 16 cm breit (übliche Textbreite auf DIN A4, Hochformat). Alle Maße sind in Punkt (pt)
angegeben, die Schrift ist durchgehend Arial 11 pt. Bei Einfügen in 100 % Größe entspricht sie
Arial 11 pt im Dokument. Zeilenumbrüche werden mit den Zeichenbreiten von Liberation Sans berechnet
(metrisch identisch mit Arial).

Aufruf:  python3 aktivitaetsdiagramm_kundenablauf.py
PNG (300 dpi): rsvg-convert -d 300 -p 300 Aktivitaetsdiagramm_Kundenablauf.svg -o Aktivitaetsdiagramm_Kundenablauf.png
"""
from xml.sax.saxutils import escape
from PIL import ImageFont

FS, LH = 11, 12.65
FONT = "Arial"
LINE, KUNDE, SYSTEM, RAUTE = "#2F3B45", "#D6EAF8", "#FFFFFF", "#F5F5F5"
NOTIZ, NOTIZ_RAND = "#FFF8DC", "#C9A227"
_metric = {False: ImageFont.truetype("/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf", 1100),
           True: ImageFont.truetype("/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf", 1100)}
out = []


def width(s, bold=False):
    return _metric[bold].getlength(s) / 100.0


def wrap(s, max_w):
    """Zeilenumbruch nach Wörtern; zu lange Wörter werden an weichen Trennstellen (\u00ad) getrennt."""
    SHY = "\u00ad"
    lines, cur = [], ""
    for word in s.split():
        while True:
            test = f"{cur} {word}".strip()
            if width(test.replace(SHY, "")) <= max_w:
                cur = test
                break
            parts = word.split(SHY)
            split = None
            for i in range(len(parts) - 1, 0, -1):
                head = f"{cur} {''.join(parts[:i])}-".strip()
                if width(head.replace(SHY, "")) <= max_w:
                    split = (head, SHY.join(parts[i:]))
                    break
            if split:
                lines.append(split[0])
                cur, word = "", split[1]
            elif cur:
                lines.append(cur)
                cur = ""
            else:
                cur = word  # nicht trennbar
                break
    lines.append(cur)
    return [l.replace(SHY, "") for l in lines]


def text(x, y, lines, anchor="middle", weight="normal", color="#1F2328"):
    if isinstance(lines, str):
        lines = [lines]
    y0 = y - (len(lines) - 1) * LH / 2 + FS * 0.35
    for i, l in enumerate(lines):
        out.append(f'<text x="{x:.1f}" y="{y0 + i * LH:.1f}" font-family="{FONT}" font-size="{FS}" '
                   f'text-anchor="{anchor}" font-weight="{weight}" fill="{color}">{escape(l)}</text>')


def action(cx, cy, w, lines, fill=SYSTEM):
    h = len(lines) * LH + 10
    out.append(f'<rect x="{cx - w / 2:.1f}" y="{cy - h / 2:.1f}" width="{w}" height="{h:.1f}" rx="8" '
               f'fill="{fill}" stroke="{LINE}" stroke-width="0.9"/>')
    text(cx, cy, lines)
    return dict(top=(cx, cy - h / 2), bottom=(cx, cy + h / 2), left=(cx - w / 2, cy), right=(cx + w / 2, cy))


def diamond(cx, cy, hw=6, hh=6, lines=None):
    pts = f"{cx},{cy - hh} {cx + hw},{cy} {cx},{cy + hh} {cx - hw},{cy}"
    out.append(f'<polygon points="{pts}" fill="{RAUTE}" stroke="{LINE}" stroke-width="0.9"/>')
    if lines:
        text(cx, cy, lines)
    return dict(top=(cx, cy - hh), bottom=(cx, cy + hh), left=(cx - hw, cy), right=(cx + hw, cy))


def flow(*pts, guard=None, gpos=None, ganchor="start"):
    d = " ".join(f"{x:.1f},{y:.1f}" for x, y in pts)
    out.append(f'<polyline points="{d}" fill="none" stroke="{LINE}" stroke-width="0.9" marker-end="url(#pfeil)"/>')
    if guard:
        text(gpos[0], gpos[1], guard, anchor=ganchor)


def note(x, y, w, content, title=None):
    """UML-Notiz; liefert die Höhe zurück."""
    lines = wrap(content, w - 16)
    rows = ([title] if title else []) + lines
    h = len(rows) * LH + 8
    f = 7
    out.append(f'<path d="M{x},{y} h{w - f} l{f},{f} v{h - f:.1f} h{-w} z" fill="{NOTIZ}" stroke="{NOTIZ_RAND}" stroke-width="0.8"/>')
    out.append(f'<path d="M{x + w - f},{y} v{f} h{f}" fill="none" stroke="{NOTIZ_RAND}" stroke-width="0.8"/>')
    for i, r in enumerate(rows):
        bold = title is not None and i == 0
        out.append(f'<text x="{x + 7}" y="{y + 5 + FS + i * LH:.1f}" font-family="{FONT}" font-size="{FS}" '
                   f'font-weight="{"bold" if bold else "normal"}" fill="#1F2328">{escape(r)}</text>')
    return h


def dashed(p1, p2):
    out.append(f'<line x1="{p1[0]}" y1="{p1[1]}" x2="{p2[0]}" y2="{p2[1]}" stroke="{NOTIZ_RAND}" '
               f'stroke-width="0.8" stroke-dasharray="3,2"/>')


# ------------------------------------------------------------------ Texte
T_BEGR = "„Hallo und herzlich willkommen! Du bist neu hier? Dann mach' bitte unsere Sicherheits\u00adein\u00adweisung. Betritt dafür den sportlichen Hallenbereich.“"
T_BUER = "„Tritte und Griffe müssen regelmäßig von Chalk befreit werden. Dafür helfen dir diese kleinen Bürsten.“"
T_CHALK = "„Hier ist das sogenannte Chalk drin. Das nutzt du, um deine Hände für einen besseren Griff trocken zu halten.“"
T_GLUECK = ("„Glückwunsch, hier bist du richtig! Du startest deine Route an den markierten Startgriffen. Es dürfen "
            "lediglich Griffe der gleichen Farbe verwendet werden. Du hast deine Route erfolgreich beendet, wenn du "
            "oben am markierten Topgriff angekommen bist.“")
T_VERL = "„Du hast verloren! Achte immer auf deine Umgebung und ob jemand an der Wand bouldert!“"
T_ABSCHL = ("„Einweisung abgeschlossen – Gut gemacht! Denk immer daran: Halte den Sturz\u00adbereich unter boul\u00adderenden "
            "Personen frei und starte erst, wenn über dir niemand an der Wand ist.“")

# ------------------------------------------------------------------ Layout (pt)
W = 453.5
X0 = 228                               # Hauptspalte
C1, C2, C3, C4 = 62, 172, 284, 388     # Spalten der Zweige
XL, XR = 5, 448                        # Rücksprünge links / rechts

out.append(f'<defs><marker id="pfeil" viewBox="0 0 10 10" refX="10" refY="5" markerWidth="6" markerHeight="6" '
           f'orient="auto-start-reverse"><path d="M0,0 L10,5 L0,10 z" fill="{LINE}"/></marker></defs>')

# Hauptpfad
out.append(f'<circle cx="{X0}" cy="9" r="6" fill="#000"/>')
m1 = diamond(X0, 28)
flow((X0, 15), m1["top"])
a1 = action(X0, 55, 168, ["Kunde kommt am Startpunkt", "in der Eingangshalle an"], KUNDE)
flow(m1["bottom"], a1["top"])
a2 = action(X0, 99, 130, ["Mitarbeiter begrüßt", "den Kunden (1)"])
flow(a1["bottom"], a2["top"])
a3 = action(X0, 137, 168, ["Kunde betritt die Boulderhalle"], KUNDE)
flow(a2["bottom"], a3["top"])
m2 = diamond(X0, 165)
flow(a3["bottom"], m2["top"])
a4 = action(X0, 190, 200, ["Kunde klickt ein virtuelles Objekt an"], KUNDE)
flow(m2["bottom"], a4["top"])
d = diamond(X0, 234, 66, 24, ["Angeklicktes", "Objekt?"])
flow(a4["bottom"], d["top"])

# Zweige
BY = 322
b1 = action(C1, BY, 100, ["Pop-up-Fenster", "„Bürste“", "anzeigen (2)"])
b2 = action(C2, BY, 100, ["Pop-up-Fenster", "„Chalkbag“", "anzeigen (3)"])
b3 = action(C3, BY, 104, ["Pop-up-Fenster", "„Glückwunsch …“", "anzeigen (4)"])
b4 = action(C4, BY, 100, ["Pop-up-Fenster", "„Du hast verlo-", "ren!“ anzeigen (6)"])
flow(d["left"], (C1, 234), b1["top"], guard="[Bürste]", gpos=(C1 + 4, 278))
flow(d["bottom"], (X0, 268), (C2, 268), b2["top"], guard="[Chalkbag]", gpos=(C2 + 4, 283))
flow(d["bottom"], (X0, 268), (C3, 268), b3["top"], guard="[pinker", gpos=(C3 + 4, 278))
text(C3 + 4, 290, "Startgriff]", anchor="start")
flow(d["right"], (C4, 234), b4["top"], guard="[gelber", gpos=(C4 + 4, 266))
text(C4 + 4, 278, "Startgriff]", anchor="start")

# Bürste/Chalkbag -> zurück zum Anklicken
m3 = diamond((C1 + C2) / 2, 368)
flow(b1["bottom"], (C1, 368), m3["left"])
flow(b2["bottom"], (C2, 368), m3["right"])
flow(m3["bottom"], ((C1 + C2) / 2, 384), (XL, 384), (XL, 165), m2["left"])

# Pinker Startgriff
b3c = action(C3, 380, 104, ["Abschlussmeldung", "anzeigen (5)"])
flow(b3["bottom"], b3c["top"])
d2 = diamond(C3, 432, 60, 23, ["Szenario", "neu starten?"])
flow(b3c["bottom"], d2["top"])
b3b = action(128, 432, 92, ["Szenario", "beenden"])
flow(d2["left"], b3b["right"], guard="[nein]", gpos=(199, 422), ganchor="middle")
flow(b3b["bottom"], (128, 461))
out.append(f'<circle cx="128" cy="468" r="7" fill="none" stroke="#000" stroke-width="1"/>')
out.append(f'<circle cx="128" cy="468" r="4.5" fill="#000"/>')

# Gelber Startgriff
b4b = action(C4, 380, 100, ["Kunde wird zum", "Startpunkt", "zurückportiert"])
flow(b4["bottom"], b4b["top"])
m4 = diamond(C4, 432)
flow(b4b["bottom"], m4["top"])
flow(d2["right"], m4["left"], guard="[ja]", gpos=(d2["right"][0] + 4, 424))
flow(m4["right"], (XR, 432), (XR, 28), m1["right"])

# Legende (oben links)
out.append(f'<rect x="6" y="4" width="104" height="52" rx="3" fill="#FAFAFA" stroke="#999" stroke-width="0.6"/>')
text(12, 15, "Legende", anchor="start", weight="bold")
out.append(f'<rect x="12" y="24" width="18" height="10" rx="4" fill="{KUNDE}" stroke="{LINE}" stroke-width="0.7"/>')
text(35, 29, "Kunde", anchor="start")
out.append(f'<rect x="12" y="40" width="18" height="10" rx="4" fill="{SYSTEM}" stroke="{LINE}" stroke-width="0.7"/>')
text(35, 45, "VR-Umgebung", anchor="start")

# Notizen (nummeriert)
note(320, 40, 122, T_BEGR, "(1) Begrüßung")
note(6, 61, 134, T_BUER, "(2) Bürste")
NY, GAP = 486, 6
hc = note(8, NY, 143, T_CHALK, "(3) Chalkbag")
hv = note(8, NY + hc + GAP, 143, T_VERL, "(6) Verloren")
hg = note(157, NY, 140, T_GLUECK, "(4) Glückwunsch")
ha = note(303, NY, 142, T_ABSCHL, "(5) Abschlussmeldung")
H = NY + max(hc + GAP + hv, hg, ha) + 4
print("Höhe:", round(H), "pt =", round(H / 72 * 2.54, 1), "cm")
svg = (f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}pt" height="{H}pt" viewBox="0 0 {W} {H}">\n'
       f'<rect width="{W}" height="{H}" fill="#FFFFFF"/>\n' + "\n".join(out) + "\n</svg>\n")
open("Aktivitaetsdiagramm_Kundenablauf.svg", "w", encoding="utf-8").write(svg)
print("Aktivitaetsdiagramm_Kundenablauf.svg geschrieben")
