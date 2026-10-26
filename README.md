# Boulderhalle VR – Prototyp „Sturzbereiche freihalten“

VR-Lernumgebung, die neuen Kunden einen Einblick in die Verhaltensregeln einer Boulderhalle gibt.
Ein Menübefehl in Unity erzeugt die Szene vollständig per Skript aus Unity-Primitiven (keine externen 3D-Modelle).
Die Szene besteht aus zwei Teilen:

1. **Statisches Modell:** die Boulderhalle mit Eingangsbereich, Boulderwand, Matte, Regalen, Sofas und Kunden.
2. **Dynamisches Modell:** ein exploratives Szenario zur Regel *„Sturzbereiche freihalten“*.

## Ablauf des Szenarios

1. Der Nutzer startet an einem **festen Startpunkt im Eingangsbereich**.
   Der Mitarbeiter am Tresen begrüßt ihn mit einer Sprechblase:
   *„Hallo und herzlich willkommen! Du bist neu hier? Dann mach' bitte unsere Sicherheitseinweisung. Betritt dafür den sportlichen Hallenbereich.“*
2. Danach bewegt sich der Nutzer mit den Controllern frei durch die Halle (Laufen per Stick oder Teleport) und kann die Matten betreten.
3. Vier Objekte sind **türkis markiert** (pulsierender Leuchtring und schwebende Raute):

   | Objekt | Ort | Auslösen | Pop-up | Ton |
   |---|---|---|---|---|
   | Chalkbag | Matte, neben einem sitzenden Kunden | Strahl + Trigger/Greif-Taste oder anfassen | Erklärung „Chalkbag“ | Hinweiston |
   | Bürste | Matte, neben einem sitzenden Kunden | Strahl + Trigger/Greif-Taste oder anfassen | Erklärung „Bürste“ | Hinweiston |
   | Startgriff **Gelb** | Wandsegment 1; dort bouldert ein Kunde | Griff mit dem Controller **berühren** | „Du hast verloren! …“ | negativer Ton |
   | Startgriff **Pink** | Wandsegment 5; dort bouldert niemand | Griff mit dem Controller **berühren** | „Glückwunsch, hier bist du richtig! …“ | positiver Ton |

4. Nach dem Feedback geht es so weiter:
   - **Verloren:** Der Knopf „Nochmal versuchen“ setzt den Nutzer an den Startpunkt zurück.
   - **Richtig:** Der Knopf „Einweisung abschließen“ führt zu einer kurzen Abschlussmeldung. Mit „Neu starten“ beginnt ein neuer Durchlauf.
   - **Chalkbag/Bürste:** „OK“ schließt das Fenster, und der Nutzer erkundet weiter.

Die Pop-ups erscheinen direkt vor dem Nutzer und werden immer über der Geometrie gezeichnet.
Auch direkt an der Wand bleiben sie lesbar.

### Zuordnung zu den Anforderungen der Konzeption

| Anforderung | Umsetzung |
|---|---|
| FA1.1 Fester Startpunkt | `Spawnpunkt_Eingang` im Eingangsbereich, Blick auf Mitarbeiter und Durchgang |
| FA3.1 Exploratives Szenario | Keine Menüs vorab; der Nutzer startet direkt in der Halle und erkundet frei |
| FA2.2 / FA4.1 Regel „Sturzbereiche freihalten“ | Zwei Startgriffe: einer unter einem boulderenden Kunden, einer an einer freien Route |
| FA3.2 / FA8.1 Entscheidungen, aktive Interaktion | Freie Bewegung; der Nutzer wählt Objekte aus und berührt Startgriffe |
| FA4.4 Pop-up-Fenster | `FeedbackPopup` (Titel, kurzer Text, Knopf) |
| FA4.5 Begriffe | Chalkbag, Bürste, Startgriff |
| FA5.1 / FA5.2 / FA5.4 Feedback | Pop-up erscheint sofort, dazu ein positiver, negativer bzw. neutraler Ton |
| NFA1.1 Farbliche Markierung | Türkiser, pulsierender Leuchtring und schwebende Raute an allen auswählbaren Objekten |
| NFA1.2 Kurze Texte | Texte aus der Konzeption, 1–3 Sätze |
| NFA2.3 Spielerischer Umgang | Markierungen, „verloren“/„Glückwunsch“, Töne |
| NFA5.1 Keine Überforderung | Ein einzelnes, kurzes Szenario |

## Bedienung

**VR (Meta Quest mit Controllern):**
- Laufen mit dem linken Stick, Drehen mit dem rechten Stick, Teleport durch Stick nach vorne drücken und loslassen.
- Chalkbag/Bürste: Strahl darauf richten und **Trigger** (Zeigefinger) oder **Greif-Taste** (Mittelfinger) drücken.
  Alternativ das Objekt direkt mit dem Controller greifen.
- Startgriff: zur Wand gehen und den markierten Griff mit dem Controller **berühren**.
- Knopf im Pop-up: Strahl auf den Knopf richten und **Trigger** drücken.

**Desktop-Test ohne Brille:**
- WASD/Pfeiltasten: laufen. Maus: umsehen. Shift: schneller.
- **Linksklick oder E:** Objekt in der Bildmitte auswählen. Startgriffe funktionieren nur aus maximal 3 m Entfernung (einstellbar über `startHoldReach` am Objekt `Spieler_Desktop`).
- **Enter/Leertaste** oder Mausklick: Knopf im Pop-up drücken.
- Esc gibt die Maus frei.

## Szene erzeugen

Menü **Boulderhalle** in Unity:

| Menüpunkt | Ergebnis |
|---|---|
| **Szene mit Szenario erzeugen (VR)** | Halle + Szenario, XR-Rig am Startpunkt (Standard) |
| **Szene mit Szenario erzeugen (Desktop-Test ohne VR)** | Halle + Szenario, Desktop-Steuerung statt XR-Rig |
| **Nur statisches Modell erzeugen (VR)** | Nur die Halle ohne Szenario (Stand von Schritt 1) |

Die Szene wird unter `Assets/Boulderhalle/Scenes/Boulderhalle.unity` gespeichert und in die Build Settings eingetragen.
**Achtung:** Beim Erzeugen wird die Szene komplett neu aufgebaut. Änderungen, die du direkt in der Szene gemacht hast, gehen dabei verloren.
Dauerhafte Änderungen gehören deshalb in die Skripte (siehe „Anpassen“).

Weitere Details beim Erzeugen:
- Gesucht wird ein XR-Origin-Prefab, bevorzugt „XR Origin (XR Rig)“ aus den Starter Assets des XR Interaction Toolkits.
- Böden und Matten erhalten eine `TeleportationArea`.
- Fehlt in der Szene ein EventSystem, legt das Szenario beim Start automatisch eines mit `XRUIInputModule` an, damit die Pop-up-Knöpfe mit den Controllern funktionieren.

## Grundriss (Draufsicht)

```
 z=30 ┌───────────────────────────────────────────────┐
      │▓▓        [Regal ][Regal ][Regal ]             │
      │▓▓ ★Pink  frei        Bank   (Kunde)           ▯
      │▓▓ ░░░░░░                                      ▯  Fenster-
      │▓▓ ░░░░░░ (sitzt) ◆Chalkbag ◆Bürste     Sofa   ▯  front
      │▓▓ ░░░░░░ (klettert) (spottet)                 ▯  (Tageslicht)
      │▓▓ ░Matte░                              Sofa   ▯
      │▓▓ ░░░░░░  (sitzt)                             ▯
      │▓▓ ★Gelb (klettert) = belegt                   │
 z=8  ├────────────────────┐ Durchgang ┌──── Uhr ─────┤
      │ Leih-  Mitarbeiter │           │  Tisch  Sessel│
      │ schuhe   [Tresen+PCs]          │        Couch ▯ Sofa
      │ Regal  └────┘    ● Start       │  Tisch  Sessel│
 z=0  └──────────────── [ Tür ] ───[ Schaufenster ]───┘
     x=-10               x=0                        x=10
  ▓ Boulderwand  ░ Matte  ▯ Fenster  ★/◆ markierte Objekte  ● Startpunkt
```

Maße: 20 m × 30 m, 6 m hoch. Eingangsbereich z = 0–8 m, Halle z = 8–30 m.

## Projektstruktur

```
Assets/Boulderhalle/
├── Scripts/Runtime/
│   ├── BoulderhalleBuilder.cs    Gebäude, Eingangsbereich, Halle, Kunden, Licht (Hauptlayout)
│   ├── ClimbingWallBuilder.cs    Wandsegmente, Routen/Griffe, Volumen, Matte, Kletterer
│   ├── Props.cs                  Sofa, Stuhl, Tische, Bank, Pflanze, Uhr, Computer, Fächerregal
│   ├── PersonBuilder.cs          Figuren aus Primitiven + Posen (stehen, winken, sitzen, klettern …)
│   ├── Palette.cs                alle Farben/Materialien
│   ├── MaterialLibrary.cs        Materialerzeugung (URP & Built-in, opak/transparent/leuchtend)
│   ├── Geo.cs                    Hilfsfunktionen (Primitive, Wände mit Öffnungen, Fenster, 3D-Text)
│   ├── ClockHands.cs             Uhrzeiger zeigen die echte Uhrzeit
│   ├── WorldText.cs              Font-Textur für 3D-Texte
│   ├── DesktopRigController.cs   Ego-Steuerung für den Desktop-Test (inkl. Auswahl per Klick)
│   └── Szenario/
│       ├── ScenarioBuilder.cs    Baut Chalkbag, Bürste, Markierungen, Startgriff-Auslöser und Pop-up; enthält die Texte
│       ├── ScenarioManager.cs    Ablaufsteuerung: Auslösen, Pop-ups, Töne, Neustart, Berührungserkennung
│       ├── ScenarioObject.cs     Auswählbares Objekt (Art, Titel, Text)
│       ├── FeedbackPopup.cs      Pop-up-Fenster (World-Space-UI)
│       ├── PulseMarker.cs        Animation der farbigen Markierung
│       └── ToneGenerator.cs      Erzeugt die Feedback-Töne
├── Scripts/Editor/
│   └── BoulderhalleSceneBuilder.cs   Menü „Boulderhalle“, Szene speichern, XR-Rig, Teleport-Flächen
└── Shaders/WorldText.shader      3D-Text mit Tiefentest (Schilder scheinen nicht durch Wände)
```

## Anpassen

- **Texte der Pop-ups:** Konstanten oben in `Szenario/ScenarioBuilder.cs`.
  Abschlusstext und Knopfbeschriftungen stehen in `ScenarioManager.cs`.
  Zum Ausprobieren lassen sich alle Texte auch im Inspector ändern, und zwar an den Objekten mit `ScenarioObject` bzw. am Objekt `Szenario_Sturzbereich`.
  Solche Änderungen gehen beim Neuerzeugen aber verloren.
- **Eigene Töne:** Am Objekt `Szenario_Sturzbereich` beim `ScenarioManager` eigene AudioClips eintragen. Leere Felder verwenden die erzeugten Töne.
- **Berührungsradius der Startgriffe:** `touchRadius` (Standard 0,15 m) am jeweiligen `ScenarioObject`.
- **Welche Routen verwendet werden:** am Ende von `BoulderhalleBuilder.Build`
  (`panels[0].StartHolds[0]` = Gelb, belegt; `panels[4].StartHolds[0]` = Pink, frei).
- **Layout/Positionen:** Konstanten und Koordinaten in `BoulderhalleBuilder.cs`.
- **Wandsegmente und Routen:** Arrays `Segments` und `Routes` in `ClimbingWallBuilder.cs`.
- **Farben:** `Palette.cs` (die Markierungsfarbe heißt `Highlight`).
- Nach Änderungen an den Skripten den Menübefehl erneut ausführen.

## Hinweise

- Alle Lichter sind Echtzeitlichter (Sonne mit Schatten, 8 Deckenleuchten).
  Für bessere Performance auf der Quest kann man später Licht backen (*Window → Rendering → Lighting*).
- Die Größe der 3D-Texte (Schilder, Sprechblase) lässt sich über `lineHeight` in `Geo.Text` nachjustieren.
- Die Figuren sind bewusst einfach gehalten (Platzhalter-Avatare).
  Sie lassen sich später durch Rigged-Modelle ersetzen, z. B. Mixamo/Ready Player Me mit Animationen.
