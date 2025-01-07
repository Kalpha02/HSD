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
| 4 | AcceptModification |
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
- role (1 Byte)
- ack  (1 Byte)

C2S (General Pck)
- Raumnummerlänge    (4 Byte)
- Standortlänge      (4 Byte)
- Descriptionlänge   (4 Byte)
- Raumnummer         (UTF-8)
- Standort           (UTF-8)
- Description        (UTF-8)
- RequestID (GUID?)       
- AcceptModificationID

Login 
- Usernamelänge 
- Passwortlänge 
- Username 
- Password 
