using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace CTIclient
{
    class StatusPipeServer
    {
        private BHOController controller;
        private String pipeName;
        private CommandObject commandObject;

        public StatusPipeServer(BHOController controller, String pipeName = "")
        {
            this.controller = controller;
            this.pipeName = pipeName;
            this.commandObject = new CommandObject();
            this.commandObject.User = "invalid user";
        }

        public void StartServer()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    int clientCount = 0;
                    Dictionary<string, object> tabStatusMap = initTabStatusMap();
                    while (true)
                    {
                        try
                        {
                            var server = new NamedPipeServerStream("PipesOfPiece", PipeDirection.InOut, 254);
                            server.WaitForConnection();
                            clientCount++;
                            //Util.showMessageBox("client count:" + clientCount);
                            Task.Factory.StartNew(() =>
                            {
                                StreamReader reader = new StreamReader(server);
                                StreamWriter writer = new StreamWriter(server);
                                while (true)
                                {
                                    var line = reader.ReadLine();
                                    if (line.Contains("get"))
                                    {                                        
                                        BinaryFormatter formatter = new BinaryFormatter();
                                        formatter.Serialize(writer.BaseStream, tabStatusMap);
                                        writer.Flush();
                                    }
                                    else if (line.Contains("put"))
                                    {
                                        BinaryFormatter formatter = new BinaryFormatter();
                                        Dictionary<String, Object> tempStatusMap = (Dictionary<String, Object>)formatter.Deserialize(reader.BaseStream);
                                        tabStatusMap = tempStatusMap;
         
                                        if (checkMapValid(tempStatusMap))
                                        {
                                            tabStatusMap = tempStatusMap;
                                        }
                                    }
                                    reader.DiscardBufferedData();
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            Util.showMessageBox("error" + e.Message);
                        }
                    }

                });

            }
            catch (Exception e)
            {
                Util.showMessageBox("error" + e.Message);
            }
        }

        private Dictionary<string, object> initTabStatusMap()
        {
            Dictionary<string, object> tabStatusMap = new Dictionary<string, object>();

            // Fill with at least one valid object
            tabStatusMap["commandObject"] = this.commandObject;
            tabStatusMap["statusObject"] = null;
            tabStatusMap["settingsList"] = null;
            tabStatusMap["extensionList"] = null;
                        
            return tabStatusMap;
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
