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

# C2S PckIDs

|ID|Package type|
|---|---|
| 0 | Login |
| 1 | Logout |
| 2 | Request |
| 3 | RequestAccept |
| 4 | AccountModification |
| 5 | - |

# S2C PckIDs

|ID|Package type|
|---|---|
| 0 | Login | 
| 1 | Logout | 
| 2 | RequestReceive | 
| 3 | RequestAccepted | 
| 4 | RequestAcknowledged| 
| 5 | RoleChanged | 
| 6 | AccountInfo |

S2C (General Pck)
- ack  (1 Byte)
- role (1 Byte)

C2S (General Pck)
- Raumnummerlänge    (4 Byte)
- Standortlänge      (4 Byte)
- Descriptionlänge   (4 Byte)
- Raumnummer         (UTF-8)
- Standort           (UTF-8)
- Description        (UTF-8)
- RequestID (GUID?)       
- AccountModificationID

Login 
- Usernamelänge 
- Passwortlänge 
- Username 
- Password 

------------------------------------------------------------

Tabelle 
|PK|Username|PasswordHash|Permission|
|---|---|---|---|
|0|Test|01#b7@!?x|1|
|1|Test1|02#c8@!?y|2|

------------------------------------------------------------

C2S (General Pack) 
- Raumnummer (4 Bytes)
- Standort (4 Bytes)
- Description (4 Bytes)
- ResquestId (GUID)
- |AccountID (bigInt)      | 
- |Username (4 Bytes)      |
- |PasswordHash (byte[32]) |
- |Permission (4 Bytes)    |(1)
- Raumnummer (UTF-8)
- Standort (UTF-8)
- Description (UTF-8)
- |Username (UTF-8)|

|....|(1) Als eigene Klasse anlegen.

|ID|PackageType|
|---|---|
|0|Login|
|1|Logout|
|2|Request|
|3|RequestAccepted|
|4|AccountModified|

----------------------------------------------------------------

S2C (General Pack)

- ack (1 byte)
- RequestID (GUID)
- AccountInfo 

|ID|PackageType|
|---|---|
|0|Login|
|1|Logout|
|2|RequestReceive|
|3|RequestAccepted|
|4|RequestAck|
|5|RoleChange|
|6|AccountInfo|

