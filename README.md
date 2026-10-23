# Boulderhalle VR – Schritt 1: statisches Modell

Unity-Paket, das per Menübefehl eine einfache Boulderhalle mit einem Raum als Szene erzeugt.
Die gesamte Geometrie wird per Skript aus Unity-Primitiven aufgebaut (keine externen 3D-Modelle).
So lässt sich das Modell jederzeit neu erzeugen und leicht anpassen.

## Inhalt der Szene

**Eingangsbereich** (Spieler startet hier, Blick Richtung Halle)
- Tresen in L-Form mit **Mitarbeiter**, der winkt, und Sprechblase *„Hallo und herzlich willkommen! …“*
- **Anmeldecomputer für Neukunden** (Bildschirm zur Kundenseite, Schild „Neukunden-Anmeldung“)
- **Mitarbeitercomputer** (Check-in/Kasse, zum Mitarbeiter gedreht)
- Regal mit Leihschuhen, Logo an der Wand
- **Wanduhr** mit echter Systemzeit
- **Sitzecke:** Sofa, zwei Sessel, Couchtisch, zwei Bistrotische mit Stühlen
- **Pflanzen** (vier im Raum, eine auf dem Tresen)
- Glastüren, Schaufenster, ein Seitenfenster

**Boulderhalle** (durch einen 4 m breiten Durchgang erreichbar)
- **Boulderwand** an der linken Wand aus 5 Segmenten (senkrecht, Überhang −20°/−30°/−8°, Platte +10°)
  mit 10 farbigen **Routen**, Start- und Top-Tape, Routenschildern und Volumen
- **Weichbodenmatte** (30 cm) unter der gesamten Wand
- **Fächerregale** an der Rückwand mit Rucksäcken, Jacken, Schuhen und Chalkbags, davor eine Bank
- **Sofas** mit Couchtischen und Blick auf die Wand
- **Fensterfront** gegenüber der Boulderwand. Durch sie fällt Sonnenlicht mit Schatten in die Halle.
- **Kunden:** 2 klettern an der Wand (Griffe unter Händen und Füßen), 2 sitzen auf der Matte,
  1 sichert (spottet), 1 sitzt auf dem Sofa, 1 auf der Bank, 1 steht am Regal

## Grundriss (Draufsicht)

```
 z=30 ┌───────────────────────────────────────────────┐
      │▓▓        [Regal ][Regal ][Regal ]             │
      │▓▓ ░░░░░░     Bank   (Kunde)                   ▯
      │▓▓ ░░░░░░                                      ▯  Fenster-
      │▓▓ ░Matte░  (sitzt)                     Sofa   ▯  front
      │▓▓ ░░░░░░  (spottet)                           ▯  (Tageslicht)
      │▓▓ ░░░░░░  (klettert)                   Sofa   ▯
      │▓▓ ░░░░░░  (sitzt)                             ▯
      │▓▓ ░░░░░░  (klettert)                          │
 z=8  ├────────────────────┐ Durchgang ┌──── Uhr ─────┤
      │ Leih-  Mitarbeiter │           │  Tisch  Sessel│
      │ schuhe   [Tresen+PCs]          │        Couch ▯ Sofa
      │ Regal  └────┘      Spawn       │  Tisch  Sessel│
 z=0  └──────────────── [ Tür ] ───[ Schaufenster ]───┘
     x=-10               x=0                        x=10
  ▓ = Boulderwand   ░ = Matte   ▯ = Fenster
```

Maße: 20 m × 30 m, 6 m hoch. Eingangsbereich z = 0–8 m, Halle z = 8–30 m.

## Einrichtung in Unity

Ausgelegt ist das Paket für **Unity 6 (6000.0 LTS)** mit **URP**; Unity 2022.3 sollte ebenfalls funktionieren.
Die Built-in-Render-Pipeline wird auch unterstützt, **HDRP nicht**.

1. In Unity Hub ein neues Projekt mit dem Template **„VR“** anlegen.
   Es enthält URP, OpenXR und das XR Interaction Toolkit samt XR-Rig.
   Alternativ geht auch „Universal 3D“; dann siehe Schritt 3.
2. Den Ordner `Assets/Boulderhalle` aus diesem Repository in den `Assets`-Ordner des Projekts kopieren.
3. *(Nur falls nicht das VR-Template verwendet wurde)*
   *Window → Package Manager → XR Interaction Toolkit* installieren und unter *Samples* die **Starter Assets** importieren.
   Danach unter *Project Settings → XR Plug-in Management* **OpenXR** aktivieren.
4. Im Menü **Boulderhalle → Statisches Modell erzeugen (VR)** wählen.
   - Die Szene wird unter `Assets/Boulderhalle/Scenes/Boulderhalle.unity` gespeichert und in die Build Settings eingetragen.
   - Materialien landen in `Assets/Boulderhalle/Generated/Materials`.
   - Ein XR-Origin-Prefab wird automatisch gesucht und am Spawnpunkt im Eingang platziert.
     Gesucht wird nach „Complete XR Origin Set Up“, dann „XR Origin (XR Rig)“ usw.
   - Böden und Matten erhalten eine `TeleportationArea`, sodass Teleport und kontinuierliches Laufen funktionieren.
5. Mit einer Quest per Link/Air Link auf **Play** drücken oder für Android (Quest) bzw. PC VR bauen.

**Ohne VR-Brille testen:** **Boulderhalle → Statisches Modell erzeugen (Desktop-Test ohne VR)**.
Steuerung: WASD/Pfeiltasten laufen, Maus umsehen, Shift schneller, Esc gibt die Maus frei, Klick fängt sie wieder.
Alternativ funktioniert in der VR-Variante der *XR Device Simulator* des XR Interaction Toolkits.

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
│   └── DesktopRigController.cs   Ego-Steuerung für den Desktop-Test
├── Scripts/Editor/
│   └── BoulderhalleSceneBuilder.cs   Menü „Boulderhalle“, Szene speichern, XR-Rig, Teleport-Flächen
└── Shaders/WorldText.shader      3D-Text mit Tiefentest (Schilder scheinen nicht durch Wände)
```

## Anpassen

- **Layout/Positionen:** Konstanten und Koordinaten in `BoulderhalleBuilder.cs`
  (z. B. `HalfWidth`, `Length`, `EntranceDepth`, Positionen der Möbel).
- **Wandsegmente und Routen:** Arrays `Segments` und `Routes` in `ClimbingWallBuilder.cs`.
- **Zufallsverteilung** (Griffe, Regalinhalte, Pflanzen): `seed` in `BoulderhalleBuilder.Build`.
- **Farben:** `Palette.cs`.
- Nach Änderungen einfach den Menübefehl erneut ausführen. Die Szene wird komplett neu erzeugt.

## Hinweise

- Alle Lichter sind Echtzeitlichter (Sonne mit Schatten, 8 Deckenleuchten).
  Für bessere Performance auf der Quest kann man später Licht backen (*Window → Rendering → Lighting*).
- Die Größe der 3D-Texte (Schilder, Sprechblase) hängt von der Schrift ab und lässt sich über `lineHeight` in `Geo.Text` nachjustieren.
- Die Figuren sind bewusst einfach gehalten (Platzhalter-Avatare).
  Sie lassen sich später durch Rigged-Modelle ersetzen, z. B. Mixamo/Ready Player Me mit Animationen.
- Nächste mögliche Schritte: Interaktion mit dem Anmeldecomputer, Begrüßungsdialog mit Audio,
  animierte Kunden, Klettermechanik.
