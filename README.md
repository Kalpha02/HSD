# HSD
Eine App für das Berufskolleg Haspel zum Management des Schülersanitätsdienstes.

# Setup
- tba

# Documentation
Client wird mit .Net Maui entwickelt.

Der Client hat den Server als Dependency, da der Server die Interfaces zur Kommunikation definiert. 

Client und Server sind zwei unterschiedliche Projekte!
Grund: 
Kann die Sicherheit der Daten auf Serverseite erhöhen 
Die Clientoberfläche kann ohne Veränderung am Server bearbeitet werden.

# Unittests
Unit tests wurden durch eine eigene implementation gelöst.
Diese sucht nach allen Klassen welche mindestens eine Methode mit dem TestAttribute tag haben, erstellt eine Objekt instanz und führt die methode aus.

# SSDServer
Der SSDServer nutzt einen Singleton. Der Server kann nur geschlossen werden wenn alle emergency requests bearbeitet wurden oder wenn die requests ignoriert werden.
Aktuell gibt es keinen Weg emergency requests zu schließen, dies wird sich noch mit der entwicklung des clients ergeben.

Der Server hat eine grobe dokumentation der package architektur in den dazu korrespondierenden Methoden, jedoch folgen alle einem ähnlichen Konzept:
```
(Client zu Server)
[ 0 -  0]	package type: Ein byte welcher den typen des packages definiert
[ 1 - 16]	Guid
[17 - 20]	raumnummern länge
[21 - 24]	standort länge
[24 -  n]	raumnummer als UTF-8 string 
[ n -  m]	standort als UTF-8 string

(Server zu Client)
[0-n] package data: Daten sind Kontextabhängig sprich: Wenn eine emergency requests gemacht wurde wird die nächste Nachricht ein byte der den success angibt(ob die request angenommen wurde)
```

Package struktur wird sich rapide verändern weshalb die exakte dokumentation(außerhalb des codens) noch nicht relevant ist bis es eine minimale app gibt.
