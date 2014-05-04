﻿/*
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
using System.Windows.Forms.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using BandObjectLib;

namespace CTIclient
{

    /**
     * CallControllerView class
     * Shows the call control bar in IE
     * 
     */
    public class CallControlView : ICTIView
    {
        private BHOController controller;
        private BHOController toolbar;
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

        private const string CallConnected = "Confirmed Dialog";
        
        public CallControlView(BHOController toolbar)
        {
            this.toolbar = toolbar;
            this.controller = toolbar;            
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

            string status = this.commandObject.Status.ToString();
            enableButtons(status.Equals(CallConnected));
            
            toolStrip.PerformLayout(); 
        }
 
         /**
          * Call controller on offHook/dial event. 
          * Looks for active/selected number in combobox
          * 
          */
        private void offHookButton_Click(object sender, System.EventArgs e)
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
        private void onHookButton_Click(object sender, System.EventArgs e)
        {
            controller.hangup(comboBox.Text);
        }

        /**
         * Call controller on transfer event. 
         * Looks for from(0)/to(1) items in combobox Items list
         *       
         */
        private void transferButton_Click(object sender, System.EventArgs e)
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

        public void addAdminItem()
        {
            this.dropMenu.DropDownItems.Add(admin);
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
            offHookButton.Click += new System.EventHandler(offHookButton_Click);

            onHookButton.AutoSize = false;
            onHookButton.Size = buttonSize;
            onHookButton.BackgroundImageLayout = ImageLayout.Stretch;
            onHookButton.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.landline_onhook_grey));
            onHookButton.Click += new System.EventHandler(onHookButton_Click);

            transferButton.AutoSize = false;
            transferButton.Size = buttonSize;
            transferButton.BackgroundImageLayout = ImageLayout.Stretch;
            transferButton.BackgroundImage = ((System.Drawing.Image)(Properties.Resources.transfer_right_left_grey));
            transferButton.Click += new System.EventHandler(transferButton_Click);

            // Initialize combobox            
            comboBox.AutoSize = false;
            comboBox.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            comboBox.Size = new System.Drawing.Size(115, 28);
            comboBox.Items.AddRange(new object[] { }); // Remove this later!!
            comboBox.GotFocus += new EventHandler(toolbar.comboBox_GotFocus);

            // Initialize drop menu
            var menuWidth = 80;

            settings = new System.Windows.Forms.ToolStripMenuItem();
            settings.Name = "settingsMenuItem";
            settings.Size = new System.Drawing.Size(menuWidth, 22);
            settings.Text = "Instellingen";
            settings.Click += new EventHandler(settings_Click);

            history = new System.Windows.Forms.ToolStripMenuItem();
            history.Name = "historyMenuItem";
            history.Size = new System.Drawing.Size(menuWidth, 22);
            history.Text = "Historie";
            history.Click += new EventHandler(history_Click);

            admin = new System.Windows.Forms.ToolStripMenuItem();
            admin.Name = "adminMenuItem";
            admin.Size = new System.Drawing.Size(menuWidth, 22);
            admin.Text = "Beheer";
            admin.Click += new EventHandler(admin_Click);
            
            dropMenu = new ToolStripDropDownButton();
            dropMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { settings, history });

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
    }
}
