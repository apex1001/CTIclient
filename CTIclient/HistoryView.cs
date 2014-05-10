/*
 * History View for CTIclient.
 * 
 * @Author: V. Vogelesang  
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CTIclient
{

    /**
     * HistoryView class
     * Shows the history of made calls
     * 
     */    
    class HistoryView : Form, ICTIView
    {
        private String[][] historyList;
        private BHOController controller;    
                
        public HistoryView(BHOController controller)
        {
            this.controller = controller;     
        }

        /**
         * Show the history view
         * 
         */
        public void showHistoryView()
        {
            this.Hide();
            this.historyList = controller.getHistoryList(); 
            if (this.historyList != null && this.historyList.Length > 0)
            {
                InitializeComponent();
                this.ShowDialog();
            }
            else
            {
                Util.showMessageBox("Er zijn geen historische gegevens gevonden.");
            }
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
         * Handle click on cell text
         * 
         * @param object sender
         * @param eventargs column row
         * 
         */
        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                DataGridView dataGridView = (DataGridView)sender;
                DataGridViewCell cell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                this.controller.dial(Util.CleanPhoneNumber(cell.Value.ToString()));
            }
        }

        /**
         * Update the view
         * 
         */
        public void update()
        {
            showHistoryView();
        }

        /**
         * Initialize the historyView
         * 
         */
        private void InitializeComponent()
        {
            this.Controls.Clear();
            int rowCount = this.historyList.Length;
            int windowHeight = 20 + (rowCount * 22);
            int windowWidth = 380;
            if (windowHeight > 450) windowHeight = 450;

            // Initialize elements 
            DataGridView dataGridView = new DataGridView();
            DataGridViewTextBoxColumn dialledParty = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn start = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn duration = new DataGridViewTextBoxColumn();  
            DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle dpDataGridViewCellStyle = new DataGridViewCellStyle();
            DataGridViewRow row = null;
            
            // Stop layout            
            this.SuspendLayout();

            // Define general dataGridViewStyle           
            dataGridViewCellStyle.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle.WrapMode = DataGridViewTriState.False;
            dataGridViewCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Define dialledParty dataGridViewStyle           
            dpDataGridViewCellStyle.ForeColor = System.Drawing.Color.Blue;
            dpDataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dpDataGridViewCellStyle.WrapMode = DataGridViewTriState.False;
            dpDataGridViewCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            
            // Define dialledParty column  
            dialledParty.Name = "dialledParty";             
            dialledParty.HeaderText = "Gebeld nummer";       
            dialledParty.MinimumWidth = 130;
            dialledParty.Width = dialledParty.MinimumWidth;
            dialledParty.Resizable = DataGridViewTriState.False;
            dialledParty.DefaultCellStyle = dpDataGridViewCellStyle;

            // Define dateTime column
            start.Name = "dateTime";
            start.HeaderText = "Datum / tijd";
            start.MinimumWidth = 150;
            start.Width = start.MinimumWidth;
            start.Resizable = DataGridViewTriState.False;

            // Define duration column 
            duration.Name = "duration";
            duration.HeaderText = "Duur";
            duration.MinimumWidth = 80;
            duration.Width = duration.MinimumWidth;
            duration.Resizable = DataGridViewTriState.False;            
            
            // Define dataGridView
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle;
            dataGridView.RowHeadersVisible = false;
            dataGridView.Location = new System.Drawing.Point(0, 0);
            dataGridView.Size = new System.Drawing.Size(windowWidth + 1, windowHeight);

            // Add items to gridView
            dataGridView.Columns.AddRange(new DataGridViewColumn[] { dialledParty, start, duration });

            // Iterate the history list
            foreach (String[] historyItem in historyList)
            {
                row = (DataGridViewRow)dataGridView.Rows[0].Clone();
                row.Resizable = DataGridViewTriState.False;

                row.Cells[0].Value = historyItem[0];                
                try
                {                        
                    DateTime dateFrom = DateTime.Parse(historyItem[1]);
                    DateTime dateTo = DateTime.Parse(historyItem[2]);
                    TimeSpan difference = dateTo - dateFrom;    
                    row.Cells[1].Value = dateFrom.AddHours(-2).ToLocalTime();
                    row.Cells[2].Value = difference.ToString(@"mm\:ss");
                }
                catch
                {
                }
                dataGridView.Rows.Add(row);
            }

            dataGridView.AllowUserToAddRows = false;
            dataGridView.CellContentClick += new DataGridViewCellEventHandler(dataGridView_CellContentClick);
            
            // History form   
            this.ClientSize = new System.Drawing.Size(windowWidth, windowHeight);
            this.Controls.Add(dataGridView);
            this.Name = "HistoryView";
            this.Text = "History";
            this.MinimumSize = new System.Drawing.Size(windowWidth + 17, windowHeight + 22);
            this.MaximumSize = new System.Drawing.Size(windowWidth + 17, windowHeight + 38);
            this.StartPosition = FormStartPosition.CenterScreen;           
            this.ResumeLayout(false);            
        }
    }
}
