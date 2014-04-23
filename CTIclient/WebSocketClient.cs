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

namespace CTIclient
{
    class WebSocketClient
    {
        private WebSocket websocket = null;
        private BHOController controller;
        private string url;
        private bool connectionOpen = false;
        private System.Timers.Timer timer;

        public WebSocketClient(BHOController controller, string url)
        {
            this.controller = controller;
            this.url = url;
        }

        public void sendMessage(string text)
        {
            if (!connectionOpen)
            {
                openConnection();
            }  
            
            websocket.Send(text);
        }

        private void openConnection()
        {
            if (!connectionOpen)
            {
                websocket = new WebSocket(url);
                addEventListeners();
                websocket.Open();
                for (int i=0; i < 30; i++) 
                {
                    Thread.Sleep(100);
                    if (connectionOpen) break;                
                }
                if (!connectionOpen) MessageBox.Show("Connection error");               
                keepAlive(true);
            }
        }

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

        void websocket_Opened(object sender, EventArgs e)
        {
            connectionOpen = true;            
        }

        void websocket_Closed(object sender, EventArgs e)
        {
            connectionOpen = false;
        }

        void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            MessageBox.Show("Error!" + e.Exception.Message);
        }

        void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {   
            this.controller.receiveCommand(e.Message);
        }

        private void addEventListeners()
        {
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
            websocket.Closed += new EventHandler(websocket_Closed);
            websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);
        }

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

        private void sendPing(object source, ElapsedEventArgs e)
        {
            websocket.Send("ping");
        }
    }
}





