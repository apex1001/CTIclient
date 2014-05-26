/*
 * WebSockets client for CTIclient
 * 
 * Uses websocket4net library from:
 * http://websocket4net.codeplex.com/
 * 
 * @author: V. Vogelesang
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using SuperSocket.ClientEngine;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using System.Threading.Tasks;

namespace CTIclient
{
    class WebSocketClient
    {
        private WebSocket websocket = null;
        private WsPipeServer pipeServer;
        private String url;
        private bool connectionOpen = false;
        private System.Timers.Timer timer;
        private CryptoModule encryptionModule;

        public WebSocketClient(String url, String pipeName, CryptoModule encryptionModule = null)
        {            
            this.url = url;            
            this.pipeServer = new WsPipeServer(this, pipeName);
            this.pipeServer.startServer();
            this.encryptionModule = encryptionModule;
        }

        /**
         * Send a message over the websocket connection
         * 
         * @param text to send
         * @boolean result
         * 
         */
        public Boolean sendMessage(String text)
        {
            // Make sure connection is open
            if (!connectionOpen)
            {
                openConnection();
            }

            // Send the message, encrypt if necessary
            if (connectionOpen)
            {
                if (encryptionModule != null)
                {
                    text = this.encryptionModule.EncryptRJ128(text);
                }
                websocket.Send(text);
            }
                
            return connectionOpen;
        }

        /**
         * Open a websocket connection
         * 
         */
        private void openConnection()
        {
            if (!connectionOpen)
            {
                websocket = new WebSocket(url);
                addEventListeners();
                websocket.Open();
                for (int i=0; i < 300; i++) 
                {
                    Thread.Sleep(10);
                    if (connectionOpen) break;                
                }
                keepAlive(connectionOpen);
            }            
        }

        /**
         * Close the connection
         * 
         */
        public void closeConnection()
        {
            if (connectionOpen)
            {
                websocket.Close();
                websocket = null;
                connectionOpen = false;
                keepAlive(false);
            }
        }

        /**
         * Handle the websocket open event
         * 
         * @param sender object
         * @param eventargs
         * 
         */
        private void websocket_Opened(object sender, EventArgs e)
        {
            connectionOpen = true;            
        }

        /**
         * Handle the websocket closed event
         * 
         * @param sender object
         * @param eventargs
         * 
         */
        private void websocket_Closed(object sender, EventArgs e)
        {
            connectionOpen = false;
        }

        /**
         * Handle the websocket error event
         * 
         * @param sender object
         * @param eventargs
         * 
         */
        private void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            // MessageBox.Show("Error!" + e.Exception.Message);
        }

        /**
         * Handle the received message event
         * Decrypt the message if needed.
         * 
         * @param sender object
         * @param eventargs
         * 
         */
        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        { 
            String message = e.Message;
            try
            {
                if (this.encryptionModule != null)
                {
                    message = encryptionModule.DecryptRJ128(message).Trim();
                }
                this.pipeServer.sendMessage(message);
            }
            catch
            {
            }                       
        }

        /**
         * Add al the event listeners
         * 
         */
        private void addEventListeners()
        {
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
            websocket.Closed += new EventHandler(websocket_Closed);
            websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);
        }

        /**
         * Keep the connection alive for windows machines
         * 
         */
        private void keepAlive(bool active)
        {
            if (timer != null && !active)
            {
                timer.Enabled = active;
                timer = null;                
            }
                            
            timer = new System.Timers.Timer(10000);
            timer.Elapsed += new ElapsedEventHandler(sendPing);
            timer.Enabled = active;
        }

        /**
         * Send a ping
         * 
         * @param sender object
         * @param eventargs
         * 
         */
        private void sendPing(object source, ElapsedEventArgs e)
        {
            sendMessage("ping");
        }
    }
}





