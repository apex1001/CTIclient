/*
 * Call Control View for CTIclient.
 * 
 * @Author: V. Vogelesang  
 * 
 * Based on the tutorials and fixes @
 * http://cgeers.com/2008/02/16/internet-explorer-toolbar/#bandobjects
 * http://www.codeproject.com/Articles/2219/Extending-Explorer-with-Band-Objects-using-NET-and
 * http://weblogs.com.pk/kadnan/articles/1500.aspx
 * http://www.codeproject.com/Articles/19820/Issues-faced-while-extending-IE-with-Band-Objects
 * 
 * Many thanx to to the authors of these articles :-).
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Timers;
using System.Threading;
using System.Windows.Forms.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using BandObjectLib;
using mshtml;

namespace CTIclient
{

    /**
     * CallControllerView class
     * Shows the call control bar in IE
     * 
     */
    public class CallControlView : ICTIView
    {
        private ClientController controller;
        private ClientController toolbar;
        private ToolStrip toolStrip;
        private ToolStripDropDownButton dropMenu;
        private ToolStripMenuItem settings;
        private ToolStripMenuItem history;
        private ToolStripMenuItem admin;
        private ToolStripComboBox comboBox;
        private CommandObject commandObject;
        private ToolStripButton onHookButton;
        private ToolStripButton offHookButton;
        private ToolStripButton transferButton;

        private String CallConnected;

        [DllImport("Shell32.dll")]
        public extern static int ExtractIconEx(string libName, int iconIndex,
        IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);
        
        public CallControlView(ClientController toolbar)
        {
            this.toolbar = toolbar;
            this.controller = toolbar;
            this.CallConnected = this.controller.getSettingsList()["CallConnected"];
        }

        /**
         * Update routine for all views
         * 
         */
        public void update()
        {           
            this.commandObject = controller.getCommandObject();
            comboBox.Items.Clear();            
            comboBox.Items.Add(commandObject.To);       
            comboBox.Text = commandObject.To;
            
            if (!commandObject.Target.Equals(""))
            {
                comboBox.Items.Add(commandObject.Target);
                comboBox.Text = commandObject.Target;
            }           

            String status = this.commandObject.Status.ToString();
            enableButtons(status.Equals(CallConnected));
            
            toolStrip.PerformLayout(); 
        }
 
        /**
         * Call controller on offHook/dial event. 
         * Looks for active/selected number in combobox
         * 
         */
        public void offHookButton_Click(object sender, System.EventArgs e)
        {
            String text = Util.CleanPhoneNumber(comboBox.Text);
            if (!text.Equals("") && comboBox.Text.Length > 2)
            {
                controller.dial(comboBox.Text);
            }
        }

        /**
         * Call controller on onHook/hangup event. 
         * Looks for active/selected number in combobox
         *       
         */
        public void onHookButton_Click(object sender, System.EventArgs e)
        {
            controller.hangup(comboBox.Text);
        }

        /**
         * Call controller on transfer event. 
         * Looks for from(0)/to(1) items in combobox Items list
         *       
         */
        public void transferButton_Click(object sender, System.EventArgs e)
        {
            if (comboBox.Items.Count == 1)
            {
                controller.transfer(comboBox.Items[0].ToString(), comboBox.Text);
            }
            if (comboBox.Items.Count == 2)
            {
                controller.transfer(comboBox.Items[0].ToString(), comboBox.Items[1].ToString());
            }
        }
   
        /**
         * Open the settings view
         * 
         */
        private void settings_Click(object sender, System.EventArgs e)
        {
            controller.showSettings();
        }

        /**
         * Open the history view
         * 
         */
        private void history_Click(object sender, System.EventArgs e)
        {
            controller.showHistory();           
        }

        /**
         * Show the admin panel in a new tab
         * 
         */
        private void admin_Click(object sender, EventArgs e)
        {
            controller.showAdmin();
        }

        /**
         * ToolStripSystemRenderer Class
         * Overrides OnRenderToolStripBorder to avoid painting the borders.
         * 
         */
        internal class NoBorderToolStripRenderer : ToolStripSystemRenderer
        {
            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }
        }

        /**
         * Enable buttons on active call;
         * 
         */
        private void enableButtons(Boolean enable)
        {
            if (enable)
            {
                onHookButton.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.landline_onhook));
                transferButton.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.transfer));
            }
            else
            {
                onHookButton.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.landline_onhook_grey));
                transferButton.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.transfer_right_left_grey));
            }
        }

        /**
         * Add the admin option to the dropDownMenu
         * 
         */
        public void addAdminItem()
        {
            this.dropMenu.DropDownItems.Add(admin);
        }

        /**
         * Enable the keydown events for the 
         * DOM docuemnt and combobox
         * 
         * @param exlorer instance of tab
         * 
         */
        public void enableShortCuts(SHDocVw.WebBrowserClass explorer)
        {
            // Hook into keydown event of toolbar
            comboBox.KeyDown += new KeyEventHandler(toolbar_KeyDown);
            
            // Hook into keydown event for document
            HTMLDocument document = (HTMLDocument) explorer.Document;
            HTMLDocumentEvents2_Event docEvent = (document as HTMLDocumentEvents2_Event);
            docEvent.onkeydown += new HTMLDocumentEvents2_onkeydownEventHandler(docEvent_onkeydown);
        }

        /**
         * Handle keydown in the combobox
         * 
         * @param object sender
         * @param KeyeEventArgs
         * 
         */
        private void toolbar_KeyDown(object sender, KeyEventArgs e)
        {
            // Chec for Alt + shortcut key
            if (e.Alt && e.KeyValue > 48)
                handleShortcutKey(e.KeyValue); 
           
            // Check for enter
            if (e.KeyValue == 13)
                offHookButton_Click(sender, e);
        }

        /**
        * Catch on keydown
        * 
        * @param eventObject
        * 
        */
        private void docEvent_onkeydown(IHTMLEventObj pEvtObj)
        {           
            if (pEvtObj.altKey && pEvtObj.keyCode > 48)                
                handleShortcutKey(pEvtObj.keyCode);            
        }

        /**
        * Handle the given shortcut
        *          
        * @param eventObject
        * 
        */
        private void handleShortcutKey(int keyCode)
        {
            Thread.Sleep(100);
            switch (keyCode)
            {
                // Alt + 1 -> select first line
                case 49:
                    this.comboBox.Focus();
                    if (this.comboBox.Items.Count > 0)
                    {
                        this.comboBox.Text = this.comboBox.Items[0].ToString();
                    }
                    break;

                // Alt + 2 -> select first line
                case 50:
                    this.comboBox.Focus();
                    if (this.comboBox.Items.Count > 1)
                    {
                        this.comboBox.Text = this.comboBox.Items[1].ToString();
                    }
                    break;

                // Alt + i -> focus combobox for input
                case 73:
                    this.comboBox.Text = "";
                    this.comboBox.Focus();                    
                    break;

                // Alt + j -> transfer
                case 74:
                    this.transferButton_Click(null, null);
                    break;

                // Alt + n -> hangup
                case 78:
                    this.onHookButton_Click(null, null);
                    break;
            }
        }

        /**
         * Create all GUI elements for the ToolStrip
         * 
         */
        public ToolStrip InitializeComponent()
        {
            // Init objects
            toolStrip = new ToolStrip();
            toolStrip.Renderer = new NoBorderToolStripRenderer();
            comboBox = new ToolStripComboBox();
            offHookButton = new ToolStripButton();
            onHookButton = new ToolStripButton();
            transferButton = new ToolStripButton();
            System.Drawing.Size buttonSize = new System.Drawing.Size(27, 27);

            // Suspend layout
            toolStrip.SuspendLayout();

            // Initialize buttons
            offHookButton.AutoSize = false;
            offHookButton.Size = buttonSize;
            offHookButton.BackgroundImageLayout = ImageLayout.Stretch;
            offHookButton.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.landline_offhook));
            offHookButton.ToolTipText = "Nummer bellen (invoeren + Enter)";
            offHookButton.Click += new System.EventHandler(offHookButton_Click);

            onHookButton.AutoSize = false;
            onHookButton.Size = buttonSize;
            onHookButton.BackgroundImageLayout = ImageLayout.Stretch;
            onHookButton.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.landline_onhook_grey));
            onHookButton.ToolTipText = "Gesprek beëindigen (Alt + n)";
            onHookButton.Click += new System.EventHandler(onHookButton_Click);

            transferButton.AutoSize = false;
            transferButton.Size = buttonSize;
            transferButton.BackgroundImageLayout = ImageLayout.Stretch;
            transferButton.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.transfer_right_left_grey));
            transferButton.ToolTipText = "Doorverbinden (Alt + j)";
            transferButton.Click += new System.EventHandler(transferButton_Click);

            // Initialize combobox            
            comboBox.AutoSize = false;
            comboBox.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            comboBox.Size = new System.Drawing.Size(115, 28);
            comboBox.Items.AddRange(new object[] { });
            comboBox.ToolTipText = "Invoeren nummer (Alt + i)\r\nSelecteer lijn 1 (Alt + 1)\r\nSelecteer lijn 2 (Alt + 2)";
            comboBox.GotFocus += new EventHandler(toolbar.comboBox_GotFocus);

            // Initialize drop menu
            settings = new System.Windows.Forms.ToolStripMenuItem();
            settings.Name = "settingsMenuItem";
            settings.Text = "Instellingen";
            settings.Click += new EventHandler(settings_Click);
            settings.Image = getIconFromIndex(72).ToBitmap();            

            history = new System.Windows.Forms.ToolStripMenuItem();
            history.Name = "historyMenuItem";
            history.Text = "Historie";
            history.Click += new EventHandler(history_Click);
            history.Image = getIconFromIndex(172).ToBitmap();

            admin = new System.Windows.Forms.ToolStripMenuItem();
            admin.Name = "adminMenuItem";
            admin.Text = "Beheer";
            admin.Click += new EventHandler(admin_Click);
            admin.Image = getIconFromIndex(45).ToBitmap();

            dropMenu = new ToolStripDropDownButton();
            dropMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { settings, history });
            // Add admin option
            if (controller.getRole().Equals("admin"))
                dropMenu.DropDownItems.Add(admin);

            // Initialize toolstrip and add buttons
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Dock = DockStyle.None;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.AutoSize = false;
            toolStrip.Size = new System.Drawing.Size(220, 32);
            toolStrip.BackColor = System.Drawing.Color.Transparent;
            toolStrip.Items.AddRange(new ToolStripItem[] { comboBox, offHookButton, onHookButton, transferButton, dropMenu });

            // Add everything to CallControl toolbar
            toolbar.Controls.AddRange(new System.Windows.Forms.Control[] { toolStrip });
            toolbar.MinSize = new System.Drawing.Size(210, 32);
            toolbar.BackColor = System.Drawing.Color.Transparent;

            // Perform final layout
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            return toolStrip;
        }

        /**
         * Extract icons from shell32.dll
         * 
         * @param index
         * @return icon
         * 
         */
        private System.Drawing.Icon getIconFromIndex(int index)
        {     
            IntPtr[] largeIcon = new IntPtr[1];       
            ExtractIconEx("shell32.dll", index-1, largeIcon, null, 1);
            return System.Drawing.Icon.FromHandle(largeIcon[0]);
        }
    }
}
