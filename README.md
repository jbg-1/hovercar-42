# HoverCar 42 (Cubes)
Das Projekt **HoverCar 42** ist ein mit Unity entwickeltes Rennspiel, das auf der [HelloCubes - DemoApp](https://github.com/dasdigitalefoyer/cubes-hellocubes-unity) aufbaut.

Weitere wichtige Ressourcen: 

- [Cubes HelloCubes Wiki](https://github.com/dasdigitalefoyer/cubes-hellocubes/wiki)
- [Cubes HelloCubes Web](https://github.com/dasdigitalefoyer/cubes-hellocubes-web)
- [Cubes Cube Backend](https://github.com/dasdigitalefoyer/cubes-cube-backend)

## Game Design

### Kernkonzept

- Das Spiel wird über die Webanwendung gestartet
- Jedem Würfel/Client wird zufällig einer Farbe zugeteilt
- Das Spiel beginnt mit allen Spielern an der Startlinie, gefolgt von einem Countdown zum Start
- Das Spiel endet, wenn der erste Spieler drei Runden absolviert hat

### Prototyp

- Grundlegende UI mit Platz, Rundenanzahl und Countdown
- Charaktere basierend auf den zugewiesenen Farben
- Automatische Beschleunigung, wobei die maximale Geschwindigkeit innerhalb einer festgelegten Zeit erreicht wird
- Eine einfache Map mit Wänden, Hindernissen, einer Start- und Ziellinie (hauptsächlich mit Placeholder-Assets)
- Kollision mit Wänden (Rückstoß + Geschwindigkeit auf null / Zurücksetzen zum letzten Checkpoint)
- Interaktion mit Items (zunächst nur ein Event auslösen)
- Interaktion mit der Ziellinie (zunächst nur ein Event auslösen)

### Erweiterungen

- Implementierung von mind. drei verschiedenen Items (einschließlich Modelle, Animationen, UI usw.)
  - Item 1
  - Item 2
  - Item 3  
- evtl. interaktive Nutzung von Items (Motion Controls)
- evtl. interaktive Beschleunigung (Motion Controls)
- Individuelle Charaktermodelle
- Mapdesign: Gestaltung der Strecke, Placeholder-Assets ersetzen, ...
- Weiterentwicklung der UI (2D-Map mit Positionen der Spieler, Items, usw.)
- 3D-Animationen und Partikel
