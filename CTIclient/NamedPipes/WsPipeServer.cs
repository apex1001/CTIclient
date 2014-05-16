/*
 * WsPipeServer for CTIclient
 * 
 * Sends and receives messages to the WsPipeClient
 * communicates with WebSocketClient 
 * 
 * @author: V. Vogelesang
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTIclient
{
    class WsPipeServer
    {
        private WebSocketClient wsClient;
        private ArrayList serverList;
        private String pipeName;
        public WsPipeServer(WebSocketClient wsClient, String pipeName)
        {
            this.wsClient = wsClient;
            this.pipeName = pipeName;
        }

        public void startServer()
        {
            Task.Factory.StartNew(() =>
            {
                this.serverList = new ArrayList();
                //int clientCount = 0;
                while (true)
                {
                    try
                    {
                        // Get connections for input and output
                        var serverIn = new NamedPipeServerStream(this.pipeName, PipeDirection.InOut, 254);
                        serverIn.WaitForConnection();
                        var serverOut = new NamedPipeServerStream(this.pipeName, PipeDirection.InOut, 254);
                        serverOut.WaitForConnection();
                        serverList.Add(serverOut);

                        //clientCount++;
                        //MessageBox.Show("wsclient count:" + clientCount);
                        
                        Task.Factory.StartNew(() =>
                        {                            
                            StreamReader reader = new StreamReader(serverIn);                            
                            while (true)
                            {
                                String message;
                                while ((message = reader.ReadLine()) != null)
                                {                                        
                                    if (message.Equals("closeConnection"))
                                        wsClient.closeConnection();
                                    else if (message.Equals("closeTab"))
                                        this.serverList.Remove(serverOut);
                                    else
                                    {
                                        wsClient.sendMessage(message);
                                        sendMessage(message);
                                    }
                                }                                
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Util.showMessageBox("WSP Error" + e.Message);
                    }
                }
            });
        }

        /** 
         * Send the received message to all clients
         * 
         * @param message
         * 
         */
        public void sendMessage(String message)
        {
            foreach (NamedPipeServerStream serverItem in this.serverList)
            {
                try
                {
                    StreamWriter writer = new StreamWriter(serverItem);
                    writer.WriteLine(message);                    
                    writer.Flush();                  
                }
                catch (Exception e)
                {
                    Util.showMessageBox("WSP error" + e.Message);                    
                }    
            }            
        }
    }
}
