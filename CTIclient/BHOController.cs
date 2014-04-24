/*
 * CTIclient controller for IE.
 * 
 * @Author: V. Vogelesang
 * 
 * Based on the tutorials and fixes @
 * http://cgeers.com/2008/02/16/internet-explorer-toolbar/#bandobjects
 * http://www.codeproject.com/Articles/2219/Extending-Explorer-with-Band-Objects-using-NET-and
 * http://weblogs.com.pk/kadnan/articles/1500.aspx
 * http://www.codeproject.com/Articles/19820/Issues-faced-while-extending-IE-with-Band-Objects
 * 
 * Many thanx to the authors of these articles :-).
 * 
 */

using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using BandObjectLib;

/**
 * BHOController class
 * Controls the CTI client
 * 
 */
namespace CTIclient
{
    [Guid("E0DE0DE0-46D4-4a98-AF68-0333EA26E113")]
    [BandObject("CTIClient", BandObjectStyle.Horizontal | BandObjectStyle.ExplorerToolbar, HelpText = "CTIclient")]
    public class BHOController : BandObject
    {
        private ADUser adUser;
        private DOMChanger domChanger;
        private WebSocketClient wsClient;
        private Container components = null;
        private CommandObject commandObject; 
        private CallControlView callControlView;
        private SettingsView settingsView;
        private Dictionary<string, ICTIView> viewList;        
        private String[][] extensionList;
        
        private string url = "ws://localhost:7777/";
        private string status;
        private string from;
        private string pin;
        private string user;

        // Call status constants from the sipXecs platform.
        private const string CallSetup = "Early Dialog";
        private const string CallConnected = "Confirmed Dialog";
        private const string CallTerminated = "Terminated Dialog";
        private const string CallBusy = "Busy Dialog";

        //Shared 128 bit Key and IV here
        private const string sKy = "lkirwf897+22#bbt"; //16 chr shared ascii string (16 * 8 = 128 bit)
        private const string sIV = "741952hheeyy66#c"; //16 chr shared ascii string (16 * 8 = 128 bit)

        // SendMessage from Win32 API. Needed for handling accelerator/control keys in text/combobox
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, UInt32 Msg, UInt32 wParam, Int32 lParam);
        [DllImport("user32.dll")]
        public static extern int TranslateMessage(ref MSG lpMsg);
        [DllImport("user32", EntryPoint = "DispatchMessage")]
        static extern bool DispatchMessage(ref MSG msg);

        /**
         * CallControlView constructor
         * 
         */
        public BHOController()
        {
            // Init DOMChanger & ADuser
            domChanger = new DOMChanger(this);

            // Init view list & views            
            viewList = new Dictionary<string, ICTIView>();            
            callControlView = new CallControlView(this);
            initCallControlView();
            settingsView = new SettingsView(this);
            //historyView = new HistoryView(this);  

            // Attach explorer & document
            this.ExplorerAttached += new EventHandler(CallControlView_ExplorerAttached);
        }

        /**
         * Receive command from server
         * 
         * @param commandObject;
         * 
         */
        public void receiveCommand(string message)
        {
            commandObject = Util.fromJSON(message);
            string command = commandObject.Command.ToString();
            string callStatus = commandObject.Status.ToString();

            if (command.Equals("settingsList"))
            {
                this.from = commandObject.From;
                this.pin = commandObject.Pin;
                this.extensionList = commandObject.Value;
            }

            if (command.Equals("call") && callStatus.Equals(CallTerminated))
            {
                this.status = CallTerminated;
                hangup(commandObject.To.ToString());
            }

            if (command.Equals("call") && callStatus.Equals(CallBusy))
            {
                MessageBox.Show("Toestel is in gesprek.", "Melding");
                this.status = CallTerminated;
                hangup(commandObject.To.ToString());
            }

            if (command.Equals("call") && callStatus.Equals(CallConnected))
            {
                this.status = CallConnected;
                doViewUpdate("callControlView");
            }
        }

        /**
         * Dial a number
         * 
         * @param to number
         * 
         */
        public void dial(String to)
        {
            this.status = CallSetup;
            commandObject.To = Util.CleanPhoneNumber(to);
            commandObject.Command = "call";
            commandObject.Status = this.status;
            sendCommand(commandObject);
            doViewUpdate("callControlView");
        }

        /**
         * Hangup a call
         * 
         * @param to number
         * 
         */
        public void hangup(String to)
        {
            commandObject.Status = CallTerminated;
            wsClient.closeConnection();
            doViewUpdate("callControlView");
        }

        /**
         * Transfer call to target
         * 
         * @param to number
         * @param target number
         * 
         */
        public void transfer(String to, String target)
        {
            MessageBox.Show(to + " " + target);
        }

        /**
         * Show settings view
         * 
         */
        public void showSettings()
        {
            this.settingsView.showSettingsMenu();
        }

        /**
         * Show history view
         * 
         */
        public void showHistory()
        {
            MessageBox.Show("history!");
        }

        /**
         * Send command to server
         * 
         * @param commandObject;
         * 
         */
        private void sendCommand(CommandObject command)
        {            
            string json = Util.toJSON(command);
            wsClient.sendMessage(json); // Activate  AES later
            //wsClient.sendMessage(AESModule.EncryptRJ128(sKy, sIV, json));
        }

        /**
         * Update all the views
         * 
         * @param view name, all if none given
         * 
         */
        private void doViewUpdate(String viewName)
        {
            if (viewName.Equals("") || viewName == null) 
            {
                foreach (KeyValuePair<string, ICTIView> view in viewList) 
                {
                    view.Value.update();
                }
            }
            else
            {
                viewList[viewName].update();
            }           
        }

        /**
         * Handle ExplorerAttached event
         * Subscribes to document complete and applies settings 
         * 
         * @param sender of the event
         * @param args
         * 
         */
        private void CallControlView_ExplorerAttached(object sender, EventArgs e)
        {
            Explorer.DocumentComplete += 
                new SHDocVw.DWebBrowserEvents2_DocumentCompleteEventHandler(Explorer_DocumentComplete);

            // Create websocket client
            try
            {
                this.wsClient = new WebSocketClient(this, url);
            }
            catch (Exception ex) { MessageBox.Show("WS Error:" + ex.Message); }           
            
            // Init settings
            getSettings();
        }

        /**
         * Handle DocumentComplete event
         * Attaches other document events and higlights phone numbers
         * 
         * @param document
         * @param url of document
         * 
         */
        private void Explorer_DocumentComplete(object pDisp, ref object URL)
        {
            domChanger.changeDOM(this.Explorer);
        }

        /**
         * Send combobox focus to parent BandObject
         * 
         * @param sender of the event
         * @param args
         * 
         */
        public void comboBox_GotFocus(object sender, EventArgs e)
        {
            this.OnGotFocus(e);
        }

        /**
         * Override accelerator key method for text/combobox
         * 
         * @param key message
         * 
         */
        public override int TranslateAcceleratorIO(ref MSG msg)
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
            return 0; //S_OK
        }
 
        /**
         * Create Call Control View
         * 
         */
        private void initCallControlView()
        {
           
            // Add everything to CallControl toolbar
            this.Controls.AddRange(new Control[] { callControlView.InitializeComponent() });
            this.MinSize = new Size(210, 32);
            this.BackColor = Color.Transparent;

            // Perform final layout
            this.ResumeLayout(false);
            this.PerformLayout();

            // Add view to the view list
            viewList.Add("callControlView", callControlView);
        }

        /**
         * Get settings from server
         * 
         */
        private void getSettings()
        {
            // Get current user
            this.adUser = new ADUser();
            this.user = adUser.getUserName();
                        
            // Create command object
            this.commandObject = new CommandObject(command: "getSettings", user: user);
            sendCommand(commandObject);
        }

        /**
         * Get the active commandobject
         * 
         * @return commandObject
         * 
         */
        public CommandObject getCommandObject()
        {
            return commandObject;
        }

        /**
         * Get the extenston list in nested array format
         * 
         * @return extensionlist 
         * 
         */
        public String[][] getExtensionList()
        {
            return this.extensionList;
        }

        /**
         * Dispose of toolbar
         * 
         */
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            wsClient.closeConnection();
            base.Dispose(disposing);            
        }
    }
}
