/*
 * WsPipeClient for CTIclient
 * 
 * Sends and receives messages to the WsPipeServer 
 * which in turn connects to the websocket client.
 * 
 * @author: V. Vogelesang
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace CTIclient
{
    class WsPipeClient
    {
        private ClientController controller;
        private String pipeName;
        private NamedPipeClientStream inClient;
        private NamedPipeClientStream outClient;

        public WsPipeClient(ClientController controller, String pipeName)
        {
            this.controller = controller;
            this.pipeName = pipeName;
            this.outClient = getClientStream();
            this.inClient = getClientStream();    
        }

        /**
         * Start listening for messages.
         *
         */
        public void startClient()
        {            
            Task.Factory.StartNew(() =>
            {
                try
                {
                    // Get a client stream
                    StreamReader reader = new StreamReader(this.inClient);

                    // Wait for a message, send to controller if active
                    while (true)
                    {
                        String message = reader.ReadLine(); 
                        if (this.controller != null && this.controller.getActiveTab())
                        {
                            this.controller.receiveCommand(message);                        
                        }
                    }
                }
                catch 
                {                    
                }
            });    
        }

        /**
         * Send a message to the pipeServer
         * 
         * @param message
         * @return result
         * 
         */
        public Boolean sendMessage(String message)
        {
            try
            {
                // Get a client stream 
                StreamWriter writer = new StreamWriter(this.outClient);
                writer.WriteLine(message);
                writer.Flush();                
                return true;
            }
            catch
            {                            
                return false;
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
                //var client = new NamedPipeClientStream("wsPipesOfPiece");
                var client = new NamedPipeClientStream(this.pipeName);
                client.Connect(1);
                return client;
            }

            catch
            {               
            }
            return null;
        }

    }
}
