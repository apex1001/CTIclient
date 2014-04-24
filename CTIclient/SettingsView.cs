using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace CTIclient
{
    class SettingsView : Form
    {
        //private System.ComponentModel.IContainer components = null;
        private BHOController controller;
        private TableLayoutPanel tableLayoutPanel;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button addButton;
        private Button cancelButton;
        private Button saveButton;
        private String[][] extensionList;
        private TextBox[] phoneNumberBoxes;
        private TextBox[] pinBoxes;
        private RadioButton[] radioButtons;
        private Button[] removeButtons;

        public SettingsView(BHOController controller)
        {
            this.controller = controller;
        }

        public void showSettingsMenu()
        {
            this.extensionList = controller.getExtensionList();
            InitializeComponent();
            this.Show();            
        }

        /**
         * Dispose of window (just hides it)
         * 
         */
        protected override void Dispose(bool disposing)
        {
            this.Hide();
            //if (disposing && (components != null))
            //{
            //    components.Dispose();
           // }
           // base.Dispose(disposing);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        /**
         * initialize window
         * 
         */
        private void InitializeComponent()
        {
            try
            {
                int extensionCount = this.extensionList.Length;
                int rowCount = this.extensionList.Length + 4;

                // Init all the element arrays
                this.phoneNumberBoxes = new TextBox[extensionCount];
                this.pinBoxes = new TextBox[extensionCount];
                this.radioButtons = new RadioButton[extensionCount];
                this.removeButtons = new Button[extensionCount];

                // Table settings & layout
                this.tableLayoutPanel = new TableLayoutPanel();
                this.tableLayoutPanel.Dock = DockStyle.Fill;
                this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
                this.tableLayoutPanel.Name = "tableLayoutPanel";
                this.tableLayoutPanel.Size = new System.Drawing.Size(350, 150);
                this.tableLayoutPanel.TabIndex = 0;
                this.tableLayoutPanel.Paint += new PaintEventHandler(this.tableLayoutPanel_Paint);

                this.tableLayoutPanel.ColumnCount = 4;
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 83F));
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));

                this.tableLayoutPanel.RowCount = rowCount;
                this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 48.14815F));

                // Add rows and init row elements
                for (int i = 0; i < extensionCount; i++)
                {
                    Array extensionItems = (Array) extensionList.GetValue(i);                    
                    this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));

                    // PhoneNumber textbox
                    this.phoneNumberBoxes[i] = new TextBox();
                    this.phoneNumberBoxes[i].Anchor = AnchorStyles.None;
                    this.phoneNumberBoxes[i].Enabled = false;
                    this.phoneNumberBoxes[i].Location = new System.Drawing.Point(10, 34);
                    this.phoneNumberBoxes[i].Name = "phoneNumber" + i.ToString();
                    this.phoneNumberBoxes[i].Text = extensionItems.GetValue(1).ToString();
                    //this.phoneNumberBoxes[i].TabIndex = 7;

                    // Pin textbox
                    this.pinBoxes[i] = new TextBox();
                    this.pinBoxes[i].Anchor = AnchorStyles.None;
                    this.pinBoxes[i].Enabled = false;
                    this.pinBoxes[i].Location = new System.Drawing.Point(121, 34);
                    this.pinBoxes[i].Name = "pin1";
                    this.pinBoxes[i].Text = extensionItems.GetValue(4).ToString();
                    this.pinBoxes[i].Size = new System.Drawing.Size(51, 20);
                    //this.pinBoxes[i].TabIndex = 6;

                    // Radio button primary phone
                    this.radioButtons[i] = new RadioButton();
                    this.radioButtons[i].Anchor = AnchorStyles.None;
                    this.radioButtons[i].Location = new System.Drawing.Point(213, 38);
                    this.radioButtons[i].Name = "radioButton" + i.ToString();
                    this.radioButtons[i].Size = new System.Drawing.Size(14, 13);
                    this.radioButtons[i].TabStop = true;
                    this.radioButtons[i].UseVisualStyleBackColor = true;
                    this.radioButtons[i].Checked = extensionItems.GetValue(2).ToString().Equals('t');
                    //this.radioButtons[i].TabIndex = 4;

                    // Remove button
                    this.removeButtons[i] = new Button();
                    this.removeButtons[i].Anchor = AnchorStyles.Left;
                    this.removeButtons[i].Location = new System.Drawing.Point(265, 33);
                    this.removeButtons[i].Name = "removeButton" + i.ToString();
                    this.removeButtons[i].Size = new System.Drawing.Size(75, 23);
                    this.removeButtons[i].Text = "Verwijderen";
                    this.removeButtons[i].UseVisualStyleBackColor = true;
                    this.removeButtons[i].Click += new EventHandler(removeButton_Click);
                    //this.removeButtons[i].TabIndex = 2;
                }

                this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
                this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));

                // Init the other elements 

                // Phone number label
                this.label1 = new Label();
                this.label1.Anchor = AnchorStyles.None;
                this.label1.AutoSize = true;
                this.label1.Location = new System.Drawing.Point(18, 8);
                this.label1.Name = "label1";
                this.label1.Size = new System.Drawing.Size(79, 13);
                this.label1.TabIndex = 8;
                this.label1.Text = "Toestelnummer";

                // Pin label
                this.label2 = new Label();
                this.label2.Anchor = AnchorStyles.None;
                this.label2.AutoSize = true;
                this.label2.Location = new System.Drawing.Point(136, 8);
                this.label2.Name = "label2";
                this.label2.Size = new System.Drawing.Size(22, 13);
                this.label2.TabIndex = 9;
                this.label2.Text = "Pin";

                // Primary phone label
                this.label3 = new Label();
                this.label3.Anchor = AnchorStyles.None;
                this.label3.AutoSize = true;
                this.label3.Location = new System.Drawing.Point(184, 8);
                this.label3.Name = "label3";
                this.label3.Size = new System.Drawing.Size(72, 13);
                this.label3.TabIndex = 10;
                this.label3.Text = "Primair toestel";

                // saveButton  
                this.saveButton = new Button();
                this.saveButton.Anchor = AnchorStyles.Left;
                this.saveButton.Location = new System.Drawing.Point(265, 118);
                this.saveButton.Name = "saveButton";
                this.saveButton.Size = new System.Drawing.Size(75, 23);
                this.saveButton.TabIndex = 16;
                this.saveButton.Text = "Opslaan";
                this.saveButton.UseVisualStyleBackColor = true;
                this.saveButton.Click += new EventHandler(saveButton_Click);

                // cancelButton
                this.cancelButton = new Button();
                this.cancelButton.Anchor = AnchorStyles.None;
                this.cancelButton.Location = new System.Drawing.Point(184, 118);
                this.cancelButton.Name = "cancelButton";
                this.cancelButton.Size = new System.Drawing.Size(73, 23);
                this.cancelButton.TabIndex = 15;
                this.cancelButton.Text = "Annuleren";
                this.cancelButton.UseVisualStyleBackColor = true;
                this.cancelButton.Click += new EventHandler(cancelButton_Click);

                // addButton
                this.addButton = new Button();
                this.addButton.Anchor = AnchorStyles.Left;
                this.addButton.Location = new System.Drawing.Point(265, 64);
                this.addButton.Name = "addButton";
                this.addButton.Size = new System.Drawing.Size(75, 23);
                this.addButton.TabIndex = 14;
                this.addButton.Text = "Toevoegen";
                this.addButton.UseVisualStyleBackColor = true;
                this.addButton.Click += new EventHandler(addButton_Click);

                // Suspend layout for adding elements
                this.tableLayoutPanel.SuspendLayout();
                this.SuspendLayout();

                // Add everything to the table
                this.tableLayoutPanel.Controls.Add(this.label1, 0, 0);
                this.tableLayoutPanel.Controls.Add(this.label2, 1, 0);
                this.tableLayoutPanel.Controls.Add(this.label3, 2, 0);

                // Add elements to table
                for (int i = 0; i < extensionCount; i++)
                {
                    this.tableLayoutPanel.Controls.Add(this.phoneNumberBoxes[i], 0, i + 1);
                    this.tableLayoutPanel.Controls.Add(this.pinBoxes[i], 1, i + 1);
                    this.tableLayoutPanel.Controls.Add(this.radioButtons[i], 2, i + 1);
                    this.tableLayoutPanel.Controls.Add(this.removeButtons[i], 3, i + 1);
                }

                this.tableLayoutPanel.Controls.Add(this.saveButton, 3, 4);
                this.tableLayoutPanel.Controls.Add(this.cancelButton, 2, 4);
                this.tableLayoutPanel.Controls.Add(this.addButton, 0, 4);

                // Form1
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(350, 150);
                this.Controls.Add(this.tableLayoutPanel);
                this.MaximizeBox = false;
                this.MaximumSize = new System.Drawing.Size(365, 187);
                this.MinimizeBox = false;
                this.MinimumSize = new System.Drawing.Size(365, 187);
                this.Name = "Form1";
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Text = "Instellingen";
                this.Load += new System.EventHandler(this.Form1_Load);
                this.tableLayoutPanel.ResumeLayout(false);
                this.tableLayoutPanel.PerformLayout();
                this.ResumeLayout(false);
            
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
            }  
      
        }

        void saveButton_Click(object sender, EventArgs e)
        {
            
        }

        void addButton_Click(object sender, EventArgs e)
        {
            
        }

        void removeButton_Click(object sender, EventArgs e)
        {

        }

        void cancelButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
