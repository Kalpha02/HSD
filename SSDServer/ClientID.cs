using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer
{
    public enum ClientType
    {
        Untermensch = 0, // Kann weder Notfälle melden, noch erhalten
        Sanitäter = 1, // Kann Notfälle erhalten
        Lehrer = 2, // Kann Notfälle erstellen
        Admin = 3 // Kann Notfälle erstellen und erhalten
    }

    public struct ClientID
    {
        public ClientType clientType;
        public Guid clientID;
        public static ClientID NewClientID(ClientType type)
        {
            ClientID clientID = new ClientID();
            clientID.clientType = type;
            clientID.clientID = new Guid();
            return clientID;
        }
    }
}
