using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDServer.Database
{
    internal class ConnectionStringHandler
    {
        private static ConnectionStringHandler instance = null;
        private Dictionary<string, string> connectionStrings = null;
        public static ConnectionStringHandler Instance
        {
            get
            {
                if (instance == null)
                    instance = new ConnectionStringHandler();
                return instance;
            }
        }

        public string this[string ident]
        {
            get
            {
                return connectionStrings[ident];
            }
        }

        private ConnectionStringHandler()
        {
            connectionStrings = new Dictionary<string, string>();
            StreamReader reader = new StreamReader("Databases.env");

            string line;
            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                int idx = line.IndexOf('=');
                connectionStrings.Add(line.Substring(0, idx), line.Substring(idx+1, line.Length-(idx+1)));
            }
            reader.Close();
        }
    }
}
