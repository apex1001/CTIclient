using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace CTIclient
{
    class StatusPipeClient
    {
        private String pipeName;
            
        public StatusPipeClient(String pipeName)
        {
            this.pipeName = pipeName;
        }

        /**
         * Get the statusMap for an initialized tab
         * 
         * @return tabStatusMap
         * 
         */
        public Dictionary<String, Object> getTabStatusMap()
        {            
            try
            {
                var client = getClientStream();
                if (client != null)
                {

                    // Send get request
                    StreamReader reader = new StreamReader(client);
                    StreamWriter writer = new StreamWriter(client);
                    writer.WriteLine("get");
                    writer.Flush();
                    

                    // Parse response into tabStatusMap object
                    BinaryFormatter formatter = new BinaryFormatter();
                    Dictionary<String, Object> tabStatusMap = (Dictionary<String, Object>)formatter.Deserialize(reader.BaseStream);                    
                    if (checkMapValid(tabStatusMap))
                    {  
                        return tabStatusMap;
                    }
                    else return null;

                }
                else Util.showMessageBox("nostream");
            }
            catch (Exception e)
            {
                Util.showMessageBox("error" + e.Message);
            }
            return null;
        }

        /**
         * Save a statusMap to the pipeServer
         * 
         * @param tabStatusMap
         * 
         */
        public void putTabStatusMap(Dictionary<String, Object> tabStatusMap)
        {            
            try
            {
                var client = getClientStream();
               
                // Send get request
                StreamWriter writer = new StreamWriter(client);
                writer.WriteLine("put");
                writer.Flush();

                // Serialize tabStatusMap object and send to pipe
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(writer.BaseStream, tabStatusMap);
                writer.Flush();
            }
            catch (Exception e)
            {
                Util.showMessageBox("error" + e.Message);
            }            
        }

        /**
         * Open a client stream to the pipeServer
         * 
         * @return clientstream
         * 
         */
        public NamedPipeClientStream getClientStream()
        {
            try
            {
                // Create client & connect to pipe
                //var client = new NamedPipeClientStream("PipesOfPiece");
                var client = new NamedPipeClientStream(this.pipeName);
                client.Connect(1);                
                return client;
            }

            catch (Exception e)
            {
                Util.showMessageBox("error" + e.Message);
            }
            return null;
        }

        /**
         * Check if the given tabStatusMap is valid
         * 
         * @param tabStatusMap
         * @return true if valid
         * 
         */
        private Boolean checkMapValid(Dictionary<string, object> tabStatusMap)
        {
            return (tabStatusMap != null && tabStatusMap["commandObject"] != null && 
                    tabStatusMap["statusObject"] != null && tabStatusMap["settingsList"] != null &&
                    tabStatusMap["extensionList"] != null);
        }
    }
}
