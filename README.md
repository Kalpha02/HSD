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

<a href="PackageDefinitions.md">Hier</a> findet man die package Definitionen.

