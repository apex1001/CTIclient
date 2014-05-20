/*
 * Settings View for CTIclient.
 * 
 * @Author: V. Vogelesang  
 * 
 */

using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace CTIclient
{

    /**
     * SettingsView class
     * view and change the phone settings
     * 
     */ 
    class SettingsView : Form, ICTIView
    {
        private ClientController controller;
        private TableLayoutPanel tableLayoutPanel;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button addButton;
        private Button cancelButton;
        private Button saveButton;
        private MaskedTextBox newPhoneNumberBox;
        private MaskedTextBox newPinBox;
        private RadioButton newRadioButton;
        private String[][] extensionList;
        private String[][] deletedExtensionList;
        private MaskedTextBox[] phoneNumberBoxes;
        private MaskedTextBox[] pinBoxes;
        private RadioButton[] radioButtons;
        private Button[] removeButtons;

        private int extensionCount;

        public SettingsView(ClientController controller)
        {
            this.controller = controller;
        }

        /**
         * Show the settings menu
         * 
         */
        public void showSettingsMenu()
        {
            update();
            this.Show();                   
        }

        /**
         * Save button has been pressed
         * 
         * @param sender object
         * @param eventarguments
         */
        private void saveButton_Click(object sender, EventArgs e)
        {
            // Save any deleted settings
            if (this.deletedExtensionList != null && this.deletedExtensionList.Length > 0)
            {
                this.controller.deleteSettings(this.deletedExtensionList);
                this.deletedExtensionList = null;
            }            
            
            // Save all new and updated settings
            for (int i = 0; i < extensionList.Length; i++)
            {
                string value = radioButtons[i].Checked.ToString().Substring(0,1).ToLower();
                this.extensionList[i][2] = value;
            }
            this.controller.updateSettings(this.extensionList);
            Thread.Sleep(100);

            this.Dispose();
        }

        /**
         * Add button has been pressed
         * 
         * @param sender object
         * @param eventarguments
         */
        private void addButton_Click(object sender, EventArgs e)
        {
            String phoneNumber = this.newPhoneNumberBox.Text;
            String mobilePhoneNumber = this.controller.getMobilePhoneNumber();
                        
            if (!this.newPhoneNumberBox.Text.Equals(""))
            {
                if (phoneNumber.StartsWith("06") && (phoneNumber.Equals(mobilePhoneNumber) || mobilePhoneNumber.Equals(""))
                    || !phoneNumber.StartsWith("06"))
                {
                    int itemLength = 6;
                    String[] item = new String[itemLength];
                    item[0] = null;
                    item[1] = phoneNumber.Trim();
                    item[3] = this.controller.getUsername();
                    item[4] = this.newPinBox.Text.Trim();
                    item[5] = "t";
                    item[2] = "f";
                    if (this.newRadioButton.Checked) item[2] = "t";

                    String[][] tempArray = new String[1][];
                    this.extensionList = Util.ArrayAddItem(item, this.extensionList, itemLength);
                    this.InitializeComponent();
                }
                else Util.showMessageBox("Dit nummer staat niet op uw naam!");
            }           
        }

        /**
         * Remove button has been pressed
         * 
         * @param sender object
         * @param eventarguments
         */
        private void removeButton_Click(object sender, EventArgs e)
        {
            int index;
            Button button = (Button) sender;
            Int32.TryParse(button.Name, out index);
            Thread.Sleep(50);

            if (this.deletedExtensionList == null)
            {
                this.deletedExtensionList = new String[1][];
                this.deletedExtensionList[0] = this.extensionList[index];
            }
            else
            {
                this.deletedExtensionList = Util.ArrayAddItem(this.extensionList[index], this.deletedExtensionList, this.extensionList[0].Length);
            }
            
            this.extensionList = Util.ArrayRemoveAt(index, this.extensionList);           
            this.InitializeComponent();
        }

        /**
         * Cancel button has been pressed
         * 
         * @param sender object
         * @param eventarguments
         */
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.extensionList = controller.getExtensionList();
            this.deletedExtensionList = null;       
            this.Dispose();            
        }

        /**
         * Catch close with X 
         * 
         * @param FormClosingEventArgs
         * 
         */
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            cancelButton_Click(null, null);
            base.OnFormClosing(e);
        }

        /**
         * Dispose of window (just hides it)
         * 
         */
        protected override void Dispose(bool disposing)
        {
            this.Hide();
        }

        /**
         * Dispose of window
         * 
         */
        public void componentDispose()
        {
            base.Dispose();
        }

        /**
         * Update the view
         * 
         */
        public void update()
        {
            this.extensionList = this.controller.getExtensionList();
            InitializeComponent();
        }

        /**
         * initialize window
         * 
         */
        private void InitializeComponent()
        {
            if (this.extensionList == null)
            {
                this.extensionList = new String[0][];
                this.extensionCount = 0;
            }
            else
            {                
                this.Controls.Clear();
                this.extensionCount = this.extensionList.Length;
            }

            try
            {                
                int rowCount = this.extensionList.Length + 5;
                String mask = "9999999999";
                String pinMask = "9999";

                if (this.extensionCount > 0)
                {
                    // Init all the element arrays
                    this.phoneNumberBoxes = new MaskedTextBox[extensionCount];
                    this.pinBoxes = new MaskedTextBox[extensionCount];
                    this.radioButtons = new RadioButton[extensionCount];
                    this.removeButtons = new Button[extensionCount];
                }

                // Table settings & layout
                this.tableLayoutPanel = new TableLayoutPanel();
                this.tableLayoutPanel.Dock = DockStyle.Fill;
                this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
                this.tableLayoutPanel.Name = "tableLayoutPanel";
                this.tableLayoutPanel.Size = new System.Drawing.Size(350, 135 + ((extensionCount -1) * 30));
                this.tableLayoutPanel.Paint += new PaintEventHandler(this.tableLayoutPanel_Paint);

                // Suspend layout for adding elements
                this.tableLayoutPanel.SuspendLayout();
                this.SuspendLayout();

                this.tableLayoutPanel.ColumnCount = 4;
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 83F));
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));

                this.tableLayoutPanel.RowCount = rowCount;
                this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));

                if (this.extensionCount > 0)
                {
                    // Add rows and init row elements
                    for (int i = 0; i < extensionCount; i++)
                    {
                        Array extensionItems = (Array)extensionList.GetValue(i);
                        this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

                        // PhoneNumber textbox
                        this.phoneNumberBoxes[i] = new MaskedTextBox();
                        this.phoneNumberBoxes[i].Anchor = AnchorStyles.None;
                        this.phoneNumberBoxes[i].Enabled = false;
                        this.phoneNumberBoxes[i].Name = "phoneNumber" + i.ToString();
                        this.phoneNumberBoxes[i].Text = extensionItems.GetValue(1).ToString();
                        if (!Char.IsLetter(extensionItems.GetValue(1).ToString(),0))
                            this.phoneNumberBoxes[i].Mask = mask;
                        this.phoneNumberBoxes[i].HidePromptOnLeave = true;

                        // Pin textbox
                        this.pinBoxes[i] = new MaskedTextBox();
                        this.pinBoxes[i].Anchor = AnchorStyles.None;
                        this.pinBoxes[i].Enabled = false;
                        this.pinBoxes[i].Name = "pin" + i.ToString();
                        this.pinBoxes[i].Text = extensionItems.GetValue(4).ToString();
                        this.pinBoxes[i].Size = new System.Drawing.Size(51, 20);
                        this.pinBoxes[i].Mask = pinMask;
                        this.phoneNumberBoxes[i].HidePromptOnLeave = true;

                        // Radio button primary phone
                        this.radioButtons[i] = new RadioButton();
                        this.radioButtons[i].Anchor = AnchorStyles.None;
                        this.radioButtons[i].Name = "radioButton" + i.ToString();
                        this.radioButtons[i].Size = new System.Drawing.Size(14, 13);
                        this.radioButtons[i].UseVisualStyleBackColor = true;

                        // Remove button
                        this.removeButtons[i] = new Button();
                        this.removeButtons[i].Anchor = AnchorStyles.Left;
                        this.removeButtons[i].Name = i.ToString();
                        this.removeButtons[i].Size = new System.Drawing.Size(75, 23);
                        this.removeButtons[i].Text = "Verwijderen";
                        this.removeButtons[i].UseVisualStyleBackColor = true;
                        this.removeButtons[i].Click += new EventHandler(removeButton_Click);

                    }
                }

                // Initialize the other elements 
                this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
                this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));                
                this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

                // Elements for new entry
                this.newPhoneNumberBox = new MaskedTextBox();
                this.newPhoneNumberBox.Anchor = AnchorStyles.None;
                this.newPhoneNumberBox.Enabled = true;
                this.newPhoneNumberBox.Name = "newPhoneNumber";
                this.newPhoneNumberBox.Mask = mask;
                this.newPhoneNumberBox.HidePromptOnLeave = true;

                this.newPinBox = new MaskedTextBox();
                this.newPinBox.Anchor = AnchorStyles.None;
                this.newPinBox.Enabled = true;
                this.newPinBox.Name = "newPin";
                this.newPinBox.Size = new System.Drawing.Size(51, 20);
                this.newPinBox.Mask = pinMask;
                this.newPinBox.HidePromptOnLeave = true;

                this.newRadioButton = new RadioButton();
                this.newRadioButton.Anchor = AnchorStyles.None;
                this.newRadioButton.Name = "newRadioButton";
                this.newRadioButton.Size = new System.Drawing.Size(14, 13);
                this.newRadioButton.UseVisualStyleBackColor = true;

                // Phone number label
                this.label1 = new Label();
                this.label1.Anchor = AnchorStyles.None;
                this.label1.AutoSize = true;
                this.label1.Name = "label1";
                this.label1.Size = new System.Drawing.Size(79, 13);
                this.label1.TabIndex = 8;
                this.label1.Text = "Toestelnummer";

                // Pin label
                this.label2 = new Label();
                this.label2.Anchor = AnchorStyles.None;
                this.label2.AutoSize = true;
                this.label2.Name = "label2";
                this.label2.Size = new System.Drawing.Size(22, 13);
                this.label2.TabIndex = 9;
                this.label2.Text = "Pin";

                // Primary phone label
                this.label3 = new Label();
                this.label3.Anchor = AnchorStyles.None;
                this.label3.AutoSize = true;
                this.label3.Name = "label3";
                this.label3.Size = new System.Drawing.Size(72, 13);
                this.label3.Text = "Primair toestel";

                // saveButton  
                this.saveButton = new Button();
                this.saveButton.Anchor = AnchorStyles.Left;
                this.saveButton.Name = "saveButton";
                this.saveButton.Size = new System.Drawing.Size(75, 23);
                this.saveButton.Text = "Opslaan";
                this.saveButton.UseVisualStyleBackColor = true;
                this.saveButton.Click += new EventHandler(saveButton_Click);

                // cancelButton
                this.cancelButton = new Button();
                this.cancelButton.Anchor = AnchorStyles.None;
                this.cancelButton.Name = "cancelButton";
                this.cancelButton.Size = new System.Drawing.Size(73, 23);
                this.cancelButton.Text = "Annuleren";
                this.cancelButton.UseVisualStyleBackColor = true;
                this.cancelButton.Click += new EventHandler(cancelButton_Click);

                // addButton
                this.addButton = new Button();
                this.addButton.Anchor = AnchorStyles.Left;
                this.addButton.Name = "addButton";
                this.addButton.Size = new System.Drawing.Size(75, 23);
                this.addButton.Text = "Toevoegen";
                this.addButton.UseVisualStyleBackColor = true;
                this.addButton.Click += new EventHandler(addButton_Click);

                // Add everything to the table
                this.tableLayoutPanel.Controls.Add(this.label1, 0, 0);
                this.tableLayoutPanel.Controls.Add(this.label2, 1, 0);
                this.tableLayoutPanel.Controls.Add(this.label3, 2, 0);   
                
                // Add elements to table
                if (this.extensionCount > 0)
                {
                    for (int i = 0; i < extensionCount; i++)
                    {
                        Array extensionItems = (Array)extensionList.GetValue(i);
                        this.tableLayoutPanel.Controls.Add(this.phoneNumberBoxes[i], 0, i + 1);
                        this.tableLayoutPanel.Controls.Add(this.pinBoxes[i], 1, i + 1);
                        this.tableLayoutPanel.Controls.Add(this.radioButtons[i], 2, i + 1);
                        if (extensionItems.GetValue(5).ToString().Equals("t"))
                        {
                            this.tableLayoutPanel.Controls.Add(this.removeButtons[i], 3, i + 1);
                        }
                    }
                }

                this.tableLayoutPanel.Controls.Add(this.newPhoneNumberBox, 0, rowCount-4);
                this.tableLayoutPanel.Controls.Add(this.newPinBox, 1, rowCount-4);
                this.tableLayoutPanel.Controls.Add(this.newRadioButton, 2, rowCount-4);
                this.tableLayoutPanel.Controls.Add(this.addButton, 3, rowCount-4);

                this.tableLayoutPanel.Controls.Add(this.saveButton, 3, rowCount-2);
                this.tableLayoutPanel.Controls.Add(this.cancelButton, 2, rowCount-2);                

                // Form1
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(350, 135 + ((extensionCount - 1) * 30));
                this.Controls.Add(this.tableLayoutPanel);
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.MinimumSize = new System.Drawing.Size(365, 170 + ((extensionCount - 1) * 30));
                this.MaximumSize = this.MinimumSize;
                this.Name = "Form1";
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Text = "Instellingen";
                this.Load += new System.EventHandler(this.Form1_Load);
                this.tableLayoutPanel.ResumeLayout(false);
                this.tableLayoutPanel.PerformLayout();
                this.ResumeLayout(false);

                // Set primary extension radio button
                if (this.extensionCount > 0)
                {
                    Boolean primary = false;
                    for (int i = 0; i < extensionCount; i++)
                    {
                        Array extensionItems = (Array)extensionList.GetValue(i);
                        if (extensionItems.GetValue(2).Equals("t"))
                        {
                            this.radioButtons[i].Checked = true;
                            primary = true;
                        }

                    }
                    if (!primary) this.radioButtons[0].Checked = true;
                }
            }
            catch (Exception e)
            {
                 Util.showMessageBox(e.Message + e.StackTrace);
            }        
        }

        private void Form1_Load(object sender, EventArgs e) { }
        private void tableLayoutPanel_Paint(object sender, PaintEventArgs e) { }  
    }
}
