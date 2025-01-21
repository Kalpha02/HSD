# Package Definitions

# Permissions 
|Bit index|Permission|
|---|---|
| 0 | SuperUser?  | 
| 1 | Can receive?| 
| 2 | Can send?   |
| 3 | Can create? | 
| 4 | Can modify? |
| 5 | Can delete? |
| 6 | -           | 
| 7 | -           | 

------------------------------------------------------------

User Database 
|PK|Username|PasswordHash|Permission|
|---|---|---|---|
|0|Test|01#b7@!?x|1|
|1|Test1|02#c8@!?y|2|

------------------------------------------------------------

C2S (General Pack) 
- |Raumnummer (4 Bytes)		|(1)
- |Standort (4 Bytes)		|
- |Description (4 Bytes)	|
- |ResquestId (GUID)		|
- |Raumnummer (UTF-8)		|
- |Standort (UTF-8)			|
- |Description (UTF-8)		|

- |AccountID (bigInt)		|(2)
- |Username (4 Bytes)		|
- |PasswordHash (byte[32])	|
- |Permission (4 Bytes)		|
- |Username (UTF-8)			|

|....|(1) RequestInfo Klasse
|....|(2) AccountInfo Klasse

|ID|PackageType|
|---|---|
|0|Login|
|1|Logout|
|2|Request|
|3|RequestAccepted|
|4|AccountModified|
|5|RequestedInfo|
|6|RequestDescription|

----------------------------------------------------------------

S2C (General Pack)

- ack (1 byte)
- AccountInfo 
- RequestInfo 

|ID|PackageType|
|---|---|
|0|Login|
|1|Logout|
|2|RequestReceive|
|3|RequestAccepted|
|4|RequestAck|
|5|RoleChange|
|6|AccountInfo|
|7|RequestInfo|

