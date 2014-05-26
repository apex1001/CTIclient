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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using BandObjectLib;
using mshtml;

namespace CTIclient
{

    /**
     * ClientController class
     * Controls the CTI client
     * 
     */
    [Guid("E0DE0DE0-36D4-4B08-AF68-0333EAC71C71")]
    [BandObject("CTIClient", BandObjectStyle.Horizontal | BandObjectStyle.ExplorerToolbar, HelpText = "CTIclient")]
    public class ClientController : BandObject
    {
        private ADUser adUser;
        private DOMChanger domChanger;
        private WebSocketClient wsClient;
        private Container components = null;
        private CommandObject commandObject;
        private CommandObject statusObject; 
        private CallControlView callControlView;
        private SettingsView settingsView;
        private HistoryView historyView;
        private StatusPipeServer statusPipeServer;
        private StatusPipeClient statusPipeClient;       
        private WsPipeClient wsPipeClient;
        private String statusPipeName;
        private String wsPipeName;
        private Dictionary<String, ICTIView> viewList;
        private Dictionary<String, String> settingsList;    
        private String[][] extensionList;
        private String[][] historyList;
        private Boolean isActiveTab;
        private Boolean extensionValid;

        // Call status constants
        private String CallSetup;
        private String CallConnected;
        private String CallTerminated;
        private String CallBusy;

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
        public ClientController()
        {
            // Create new statusObject & read the settings file
            this.statusObject = new CommandObject("", "", "", "", "", "", "", "", null);
            this.commandObject = new CommandObject("", "", "", "", "", "", "", "", null);

            // Read & apply the settings file, get ADuser
            readSettingsFile();
            getAdUser();
            
            // Create NamedPipes names
            this.statusPipeName = Util.getPipeName(this.statusObject.User, "status");
            this.wsPipeName = Util.getPipeName(this.statusObject.User, "ws");

            // Start NamedPipeClient (& Server if needed) for same tabStatus in all tabs  
            // Also start one instance of WsClient for communication with CallControlServer            
            this.statusPipeClient = new StatusPipeClient(this.statusPipeName); 
            if (statusPipeClient.getClientStream() == null)
            {
                startPipeServer();               
                startWsClient();
            }                
            // PipeServer already exists, get tab settings
            else
            {
                getTabSettings();
            }

            this.wsPipeClient = new WsPipeClient(this, this.wsPipeName);
            this.wsPipeClient.startClient();            

            // Init DOMChanger
            domChanger = new DOMChanger(this);

            // Init views 
            callControlView = new CallControlView(this);
            initCallControlView();
            settingsView = new SettingsView(this);
            historyView = new HistoryView(this);
            
            // Add views to the view list
            this.viewList = new Dictionary<string, ICTIView>();
            this.viewList.Add("callControlView", callControlView);
            this.viewList.Add("settingsView", settingsView);
            this.viewList.Add("historyView", historyView);

            // Update the views if settingsList is available              
            doViewUpdate("settingsView");
            if (!this.commandObject.Status.Equals(CallTerminated) || !this.statusObject.Status.Equals(""))
                doViewUpdate("callControlView");

            // Attach explorer & document event
            this.ExplorerAttached += new EventHandler(CallControlView_ExplorerAttached);
            this.isActiveTab = true;            
        }

        /**
         * Receive command from server
         * 
         * @param commandObject;
         * 
         */
        public void receiveCommand(String message)
        {            
            CommandObject tempObject = Util.fromJSON(message);            
            if (tempObject != null)
            {
                this.commandObject = tempObject;
                string command = commandObject.Command.ToString();
                string callStatus = commandObject.Status.ToString();

                // Receiving settingsList
                if (command.Equals("settingsList"))
                {
                    this.statusObject.From = commandObject.From;
                    this.statusObject.Pin = commandObject.Pin;
                    this.statusObject.Role = commandObject.Role;
                    this.extensionList = commandObject.Value;
                    closeConnection();
                    
                    // Update statusPipeServer with tab status
                    statusPipeClient.putTabStatusMap(this.getCurrentTabStatus());

                    // Add admin option
                    if (this.statusObject.Role.Equals("admin"))
                        callControlView.addAdminItem();
                }

                // Receiving user history
                if (command.Equals("userHistory"))
                {
                    this.historyList = commandObject.Value;
                    closeConnection();
                }

                // Receiving the url for the admin page
                if (command.Equals("adminUrl"))
                {
                    String[][] valueArray = commandObject.Value;
                    String adminUrl = valueArray[0][0];
                    closeConnection();
                    navigateToAdmin(adminUrl);
                }

                // Receiving termination of call. Value field has terminated To extension
                if (command.Equals("call") && callStatus.Equals(CallTerminated))
                {
                    String[] value = (String[]) commandObject.Value.GetValue(0);
                    if (value != null)
                    {
                        String extension = value.GetValue(0).ToString();
                        hangup(extension, false);                    
                    }
                    else hangup(this.statusObject.To, false);
                }

                // Receiving termination of call from other tab. Value field has terminated To extension
                if (command.Equals("terminate"))
                {                    
                    String[] value = (String[])commandObject.Value.GetValue(0);
                    if (value != null)
                    {
                        String extension = value.GetValue(0).ToString();
                        if (this.statusObject.Status.Equals(CallConnected) &&
                            (this.statusObject.To.Equals(extension) || this.statusObject.Target.Equals(extension)))
                        {
                            hangup(extension, false);
                        }
                    }
                }

                // Receiving 403 busy from the REST interface
                if (command.Equals("call") && callStatus.Equals(CallBusy))
                {
                    Util.showMessageBox("Toestel is in gesprek.", "Melding");
                    if (!this.statusObject.Target.Equals("") && this.statusObject.Status.Equals(CallConnected))
                    {                        
                        hangup(this.statusObject.Target, false);        
                    }
                    else
                    {                        
                        clearCallStatus();
                    }
                }

                // Receiving call connected confirmed
                if (command.Equals("call")  && callStatus.Equals(CallConnected))
                {
                    this.statusObject.Status = CallConnected;
                    this.statusObject.To = commandObject.To;
                    this.statusObject.Target = commandObject.Target;
                    doViewUpdate("callControlView");                   
                }

                // Receiving early dialog, no connection yet!
                if (command.Equals("call") && callStatus.Equals(CallSetup))
                {
                    this.statusObject.Status = CallSetup;                    
                }

                // Receiving transfer from other tab
                if (command.Equals("transfer") && callStatus.Equals(CallConnected))
                {
                    clearCallStatus();
                }
                
                // Receiving extension check
                if (command.Equals("checkExtension"))
                {                    
                    try
                    {
                        if (commandObject != null)
                        {
                            String[] value = (String[])commandObject.Value.GetValue(0);
                            if (value != null)
                            {
                                String result = value.GetValue(0).ToString();
                                this.extensionValid = result.Equals("true");
                            }
                            closeConnection();
                        }
                    }
                    catch (Exception)
                    {
                        closeConnection();                       
                    }
                }

                // Update statusPipeServer with tab status
                statusPipeClient.putTabStatusMap(this.getCurrentTabStatus());
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
            to = Util.CleanPhoneNumber(to);

            if (!to.Equals("") && !this.statusObject.From.Equals("") &&
                this.statusObject.Target.Equals("") && !this.statusObject.Status.Equals(CallSetup))
            {
                // Check if there is a connected call, offer to transfer
                if (this.statusObject.Status.Equals(CallConnected))
                {
                    DialogResult dialogResult = Util.showMessageBox(
                         "Er is al een gesprek. Wilt u doorverbinden naar het gekozen nummer?",
                         "Melding", MessageBoxButtons.YesNoCancel);
                    if (dialogResult == DialogResult.Yes)
                    {
                        transfer(this.statusObject.To, to);
                        return;
                    }

                    // User wants second call, set second call as target
                    if (dialogResult == DialogResult.No)
                    {
                        this.statusObject.Target = to;                        
                    }

                    // User wants to cancel
                    if (dialogResult == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                // If there is no current call
                else
                {
                    this.statusObject.Status = CallSetup;
                    this.statusObject.To = to;
                }

                // Create commandobject
                commandObject.From = this.statusObject.From;
                commandObject.To = this.statusObject.To;
                commandObject.Target = this.statusObject.Target;
                commandObject.Pin = this.statusObject.Pin;
                commandObject.Command = "call";
                commandObject.Status = this.statusObject.Status;
                commandObject.Value = new String[0][];
                sendCommand(commandObject);
                doViewUpdate("callControlView");
            }

            else if (this.statusObject.From.Equals(""))
                Util.showMessageBox("Er is geen primair toestel ingesteld!");
        }

        /**
         * Hangup a call
         * 
         * @param to number
         * 
         */
        public void hangup(String to, Boolean terminateLine = true)
        {
            if ((this.statusObject.Status.Equals(CallConnected) || this.statusObject.Status.Equals(CallSetup)) &&
                !to.Equals("") && (to.Equals(this.statusObject.To) || to.Equals(this.statusObject.Target)))
            {                
                // Terminate the connection if offhook was pressed (default)
                if (terminateLine)
                {
                    //MessageBox.Show("a" + to);
                    // Terminate the given line
                    commandObject.Command = "terminate";
                    commandObject.Status = CallTerminated;
                    commandObject.From = this.statusObject.From;
                    commandObject.To = to;
                    commandObject.Target = "";
                    commandObject.Value = new String[1][] {new String[1] { to }};  
                    sendCommand(commandObject);
                    Thread.Sleep(200);
                }

                // Make 'target' the new 'to' if it is the only call left
                if (to.Equals(this.statusObject.To) && !this.statusObject.Target.Equals(""))
                {
                    //MessageBox.Show("b" + to);
                    this.statusObject.To = this.statusObject.Target;
                    this.statusObject.Target = "";
                    commandObject.To = this.statusObject.To;
                    commandObject.Target = "";
                    commandObject.Status = CallConnected;
                    doViewUpdate("callControlView");
                }

                // Clear 'target', leaving only 'to'.
                else if (to.Equals(this.statusObject.Target))
                {
                    //MessageBox.Show("c" + to);
                    this.statusObject.Target = "";
                    commandObject.Target = "";
                    commandObject.To = this.statusObject.To;
                    commandObject.Status = CallConnected;
                    doViewUpdate("callControlView");
                }

                // If there was only one call clear everything
                else if (to.Equals(this.statusObject.To) && this.statusObject.Target.Equals(""))
                {
                    clearCallStatus();
                }

                // Update statusPipeServer with tab status
                statusPipeClient.putTabStatusMap(this.getCurrentTabStatus());
            }
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
            if(this.statusObject.Status.Equals(CallConnected) && 
                !target.Equals(this.statusObject.To) && 
                !target.Equals(this.statusObject.From))
            {
                commandObject.Command = "transfer";            
                commandObject.From = this.statusObject.From;
                commandObject.To = to;
                commandObject.Target = target;
                sendCommand(commandObject);
                Thread.Sleep(200);

                // Clear call status
                clearCallStatus();
            }

            // Update statusPipeServer with tab status
            statusPipeClient.putTabStatusMap(this.getCurrentTabStatus());
        }

        /**
         * Clear call status & close connection
         * 
         */
        private void clearCallStatus()
        {
            // Clear call status
            this.statusObject.Status = CallTerminated;
            this.statusObject.To = "";
            this.statusObject.Target = ""; 
            
            // Clear commandObject
            commandObject.Command = "terminate";
            commandObject.Status = CallTerminated;
            commandObject.To = "";
            commandObject.Target = "";

            // Close connection
            closeConnection();
            doViewUpdate("callControlView");
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
            // Get the history list
            this.historyList = null;
            commandObject.Command = "getHistory";
            commandObject.User = this.statusObject.User;
            commandObject.Value = null;
            sendCommand(commandObject);
            
            // Wait for the list to be sent then show it
            for (int i=0; i < 200; i++)
            {
                if (this.historyList != null)
                    break;            
                Thread.Sleep(10);
            }
            historyView.showHistory();
        }

        /**
         * Show admin panel
         * 
         */
        public void showAdmin()
        {
            commandObject.Command = "getAdminUrl";
            commandObject.User = this.statusObject.User;
            commandObject.Value = null;
            sendCommand(commandObject);
        }

        /**
         * Navigates to the admin panel
         * username is sent by POST
         * 
         * @param url of admin page
         * 
         */
        public void navigateToAdmin(String adminUrl)
        {
            byte[] post = new ASCIIEncoding().GetBytes("username=" + this.statusObject.User);
            string headers = "Content-Type: application/x-www-form-urlencoded";
            this.Explorer.Navigate(adminUrl, null, null, post, headers);
        }

        /**
         * Send command to server
         * 
         * @param commandObject;
         * 
         */
        private void sendCommand(CommandObject command)
        {            
            // Send message to the server, via wsPipeServer & WebSocketClient
            string json = Util.toJSON(command);
            wsPipeClient.sendMessage(json);

            // Update statusPipeServer with tab status
            statusPipeClient.putTabStatusMap(this.getCurrentTabStatus());
        }

        /**
         * Update all the views
         * 
         * @param view name, all if none given
         * 
         */
        private void doViewUpdate(String viewName = null)
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
            // Set window events for DOMChange & tabfocus
            Explorer.DocumentComplete += 
                new SHDocVw.DWebBrowserEvents2_DocumentCompleteEventHandler(Explorer_DocumentComplete);
            Explorer.WindowStateChanged += 
                new SHDocVw.DWebBrowserEvents2_WindowStateChangedEventHandler(Explorer_WindowStateChanged);
                   
            // Init settings
            if (extensionList == null)
            {                
                getSettings();                
            }
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
            // Scan DOM for phonenumbers
            domChanger.changeDOM(this.Explorer);

            // Attach key down event for key shortcuts
            callControlView.enableShortCuts(this.Explorer);
        }

         /**
         * Handle WindowStateChange event
         * Update the tab on statechange
         * 
         * @param WindowStatFlags
         * @param ValidWindowStatflags
         * 
         */
        private void Explorer_WindowStateChanged(uint dwWindowStateFlags, uint dwValidFlagsMask)
        {
            // Tab is activated
            if (dwWindowStateFlags == 3)
            {
                this.isActiveTab = true;
   
                // Get tabStatus from statusPipeServer
                getTabSettings();

                // Update the views       
                doViewUpdate("settingsView");

                // Clear connection if active
                if (this.statusObject.Status.Equals(CallTerminated))
                    clearCallStatus();
                else
                    doViewUpdate("callControlView");
            }
            else
            {
                this.isActiveTab = false;
            }
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
         * This controller is the actual BandObject
         * 
         */
        public void initCallControlView()
        {
            // Add everything to CallControl toolbar (i.e. this)          
            this.Controls.AddRange(new Control[] { callControlView.InitializeComponent() });
            this.MinSize = new Size(220, 32);
            this.BackColor = Color.Transparent;
            
            // Perform final layout
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        /**
         * Get the current AD user and add SID hash
         * 
         * @return username+hash
         * 
         */
        private void getAdUser()
        {
            // Get current AD user
            this.adUser = new ADUser(this);
            String sid = this.adUser.getUserSid();
            this.statusObject.User = adUser.getUserName() + "-" + Util.getHash(sid).Substring(0, 8);     
        }

        /**
         * Get settings from server
         * 
         */
        private void getSettings()
        {
            // Create command object
            this.commandObject = new CommandObject(command: "getSettings", 
                                                    user: this.statusObject.User,
                                                    pin: this.statusObject.Pin,
                                                    from: this.statusObject.From);
            sendCommand(commandObject);           
        }

        /**
         * Save settings to server
         * 
         */
        public void updateSettings(String[][] extensionList)
        {
            this.extensionList = extensionList;
            
            // Check if list is empty
            if (this.extensionList == null || this.extensionList.Length < 1)
            {
                this.statusObject.From = "";
                this.statusObject.Pin = "";
            }            
            
            // Get new primary extension
            foreach (String[] item in this.extensionList)
            {
                if (item.GetValue(2).Equals("t"))
                {
                    this.statusObject.From = item.GetValue(1).ToString();
                    this.statusObject.Pin = item.GetValue(4).ToString();
                    break;
                }                
            }

            // Create command object and save settings
            this.commandObject = new CommandObject(command: "updateSettings",
                                                   from: this.statusObject.From,
                                                   user: this.statusObject.User,
                                                    pin: this.statusObject.Pin,
                                                   value: extensionList);
            sendCommand(commandObject);
            closeConnection();
        }

        /**
         * Delete settings from server
         * 
         */
        public void deleteSettings(String[][] deletedExtensionList)
        {
            // Create command object and save settings
            this.commandObject = new CommandObject(command: "deleteSettings",
                                                   from: this.statusObject.From,
                                                   user: this.statusObject.User,
                                                    pin: this.statusObject.Pin,
                                                   value: deletedExtensionList);
            sendCommand(commandObject);
            closeConnection();
        }

        /**
         * Read and apply the settings file
         * 
         */
        private void readSettingsFile()
        {
            String programPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles);
            String filePath = programPath + "\\CTIclient\\";
            Dictionary<String, String> settingsList = Util.parseSettingsFile(filePath + "settings.ini");
            try
            {
                this.settingsList = settingsList;
                this.settingsList["ccsUrl"] = "ws://" + settingsList["ccsHost"] + ":" + settingsList["ccsPort"] + "/";
                applyStatusConstants(settingsList);
            }
            catch
            {
            }
        }

        /**
         * Apply Status constants from settingslist
         * 
         * @param settingsList
         * 
         */
        private void applyStatusConstants(Dictionary<String, String> settingsList)
        {
            this.CallSetup = settingsList["CallSetup"];
            this.CallConnected = settingsList["CallConnected"];
            this.CallTerminated = settingsList["CallTerminated"];
            this.CallBusy = settingsList["CallBusy"];
        }

        /**
         * Start a NamedPipeServer for tab synchronisation
         * 
         */
        private void startPipeServer()
        {
            this.statusPipeServer = new StatusPipeServer(this, this.statusPipeName);
            this.statusPipeServer.StartServer();
        }

        /**
         * Start a webSocketClient
         * 
         */
        private void startWsClient()
        {         
            // Create websocket client
            try
            {
                String sKy = settingsList["sKy"];
                String sIV = settingsList["sIV"];
                this.wsClient = new WebSocketClient(this.settingsList["ccsUrl"], this.wsPipeName, new CryptoModule(sKy,sIV));               
            }
            catch
            {
                // Do nothing. If server is down user shouldn't be bothered when opening new tab.
            }         
        }

        /**
         * Close the websocket connection if there is no active call
         * 
         */
        private void closeConnection()
        {
            if (!this.statusObject.Status.Equals(CallConnected) && !this.statusObject.Status.Equals(CallSetup))
                wsPipeClient.sendMessage("closeConnection");
        }


        /**
         * Get & apply the settings for this tab
         * preferrably from the statusPipeServer
         * 
         */
        private void getTabSettings()
        {
            // Check if there is already a valid tabStatusMap at the statusPipeServer
            Dictionary<String, Object> tabStatusMap = statusPipeClient.getTabStatusMap();
            if (tabStatusMap != null)
            {
                applyMapSettings(tabStatusMap);
            }
        }

        /**
         * Apply the information in a tabStatusMap to this tab
         * 
         * @param tabStatusMap
         * 
         */
        private void applyMapSettings(Dictionary<String, Object> tabStatusMap)
        {
            try
            {
                this.commandObject = (CommandObject)tabStatusMap["commandObject"];
                this.statusObject = (CommandObject)tabStatusMap["statusObject"];
                this.settingsList = (Dictionary<String, String>)tabStatusMap["settingsList"];
                this.extensionList = (String[][])tabStatusMap["extensionList"];
                applyStatusConstants(this.settingsList);
            }

            catch (Exception e)
            {
                Util.showMessageBox("ClientController err" + e.Message + e.StackTrace);
            }
        }

        /**
         * Get the status of the current tab
         * 
         * @return tabStatusMap
         * 
         */
        public Dictionary<String, Object> getCurrentTabStatus()
        {
            Dictionary<String, object> tabStatusMap = new Dictionary<String, object>();
            tabStatusMap["commandObject"] = this.commandObject;
            tabStatusMap["statusObject"] = this.statusObject;
            tabStatusMap["settingsList"] = this.settingsList;
            tabStatusMap["extensionList"] = this.extensionList;
            return tabStatusMap;
        }

        /**
         * Get historyList from server
         * 
         * @return historyList array
         * 
         */
        public String[][] getHistoryList()
        {
            return this.historyList;
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
         * Get the active username
         * 
         * @return username
         * 
         */
        public String getUsername()
        {
            return this.statusObject.User;
        }

        /**
         * Get user domain
         * 
         * @return domain
         * 
         */
        public String getDomain()
        {
            return this.settingsList["domain"];
        }

        /**
         * Get user role
         * 
         * @return role
         * 
         */
        public String getRole()
        {
            return this.statusObject.Role;
        }

        /**
         * Get the mobile phone of the current user
         * if registered in AD.
         * 
         * @return String phonenumber
         * 
         */
        public String getMobilePhoneNumber()
        {
            return this.adUser.getMobilePhone();
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
         * Find out if this is the active tab
         * 
         * @return true if active
         * 
         */
        public Boolean getActiveTab()
        {
            return this.isActiveTab;
        }

        /**
         * Get the settingsList
         * 
         * @return the settingsList
         * 
         */
        public Dictionary<String,String> getSettingsList()
        {
            return this.settingsList;
        }

        /**
         * Get the current call status
         * 
         * @return call status
         * 
         */
        public String getCallStatus()
        {
            return this.statusObject.Status;
        }

        /**
         * Check with the controller if the phone/pin is valid
         * 
         * @param phonenumber
         * @param pin
         * @return boolean true if valid
         * 
         */
        public Boolean checkExtensionValid(string phoneNumber, string pin)
        {
            extensionValid = false;

            // Create command object
            this.commandObject = new CommandObject(command: "checkExtension",
                                                    from: phoneNumber,
                                                    pin: pin
                                                    );
            sendCommand(commandObject);

            // Wait for the status to change.
            for (int i = 0; i < 200; i++)
            {
                if (extensionValid)
                    return true;
                Thread.Sleep(10);
            }
            return false;
        }

        /**
         * Dispose of toolbar
         * 
         */
        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }

            // Close connections & dispose of views
            wsPipeClient.sendMessage("closeTab");
            this.settingsView.componentDispose();
            this.historyView.componentDispose();
            base.Dispose(disposing);            
        }
    } 
}
