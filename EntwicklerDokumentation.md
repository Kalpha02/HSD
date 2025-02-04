##Dokumentation Packages:

Es gibt insgesamt zwei General Packages, welche zwischen Server und Client gesendet werden: 
- CLient to Server (C2S)
- Server to Client (S2C)

Wir arbeiten in den Packages mit Bytes, sodass die Größe der Informationen im Package exakt festgelegt ist.
Für die RequestId gibt es eine GUID und die vom User angegebenen Informationen wie Raumnummer, Standort und Description sind UTF-8. 
Es gibt zwei Klassen, durch die das Package erstellt wird: RequestInfo / AccountInfo.

#C2S - General Pack:

| Bestandteil | Typ | Klasse | Description | 
|---|---|---|---|
|Raumnummer  | 4 Bytes  | RequestInfo | Raumnummer, in der der Notfall abgesetzt wurde |
|Standort    | 4 Bytes  | RequestInfo | Standort, in der der Notfall abgesetzt wurde   | 
|Description | 4 Bytes  | RequestInfo | Beschreibung des Notfalls, was passiert ist    | 
|ResquestId  | GUID     | RequestInfo | Die Id des Notfalls, der abgesetzt wurde       | 
|Raumnummer  | UTF-8    | RequestInfo |                                                |
|Standort    | UTF-8    | RequestInfo |                                                |
|Description | UTF-8    | RequestInfo |                                                |
|AccountID   | bigInt   | AccountInfo |                                                |
|Username    | 4 Bytes  | AccountInfo |                                                |
|PasswordHash| byte[32] | AccountInfo |                                                |
|Permission  | 4 Bytes  | AccountInfo |                                                |
|Username    | UTF-8    | AccountInfo |                                                |


|ID |PackageType |Description |
|---|---|---|
|0  | Login		           |    |
|1  | Logout             |    |
|2  | Request            |    |
|3  | RequestAccepted    |    |
|4  | AccountModified    |    |
|5  | RequestedInfo      |    |
|6  | RequestDescription |    |


#S2C - General Pack
Das Server-to-Client-Package (S2C) unterscheidet sich dahingehend, dass ein Acknowledge-Byte am Anfang des Packages steht.  
Ansonsten ist das Package wie Client-to-Server-Package (C2S) aufgebaut. Es besteht aus den Informationen der AccountInfo und RequestInfo. 

|ID |PackageType |Description |
|---|---|---|
|0  |Login             |    |
|1  |Logout            |    |
|2  |RequestReceive    |    |
|3  |RequestAccepted   |    |
|4  |RequestAck        |    |
|5  |RoleChange        |    |
|6  |AccountInfo       |    |
|7  |RequestInfo       |    |


#Permissions
Anstelle von Rollen haben wir uns für Rechte entschieden, welche einen Bit-Index enthalten und an die jeweiligen User verteilt werden.
Aktuell gibt es 6 Permissions. Es ist genügend Freiraum, um weitere Permissions anzulegen. 

|ID |PackageType | Description |
|---|---|---|
|0  |SuperUser?   | Enthält alle Berechtigungen.
|1  |Can receive? | Kann Notfall empfangen.
|2  |Can send?    | Kann Notfall senden.
|3  |Can create?  | Kann Notfall erstellen.
|4  |Can modify?  | Kann Benutzer verwalten, bearbeiten etc.
|5  |Can delete?  | Kann Benutzer löschen.
|6  |- |- | 
|7  |- |- |

Die Permissions sind in der Datenbank den Usern zugewiesen. Unsere Datenbank besteht aus einer Tabelle mit folgenden Daten: 
|PK |Username |PasswordHash |Permissions | 
|---|---|---|---|
|1 |MaxMu |01#b7@!?x |1 |
|2 |TestX |02#c8@!?y |2 |



