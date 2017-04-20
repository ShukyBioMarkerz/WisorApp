using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WisorLib;

namespace WisorAppWpf
{
    partial class DisplayCombinations
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new DataGridView();
            this.button3 = new Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 24);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(1377, 621);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells; // Fill;
            this.dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9.75F, FontStyle.Bold);
            this.dataGridView1.RowPostPaint += new DataGridViewRowPostPaintEventHandler(this.RowPostPaint);

            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(137, 673);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(133, 35);
            this.button3.TabIndex = 3;
            this.button3.Text = "Close";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // DisplayCombinations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1401, 729);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.dataGridView1);
            this.Name = "DisplayCombinations";
            this.Text = "DisplayCombinations";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        private void RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(dataGridView1.RowHeadersDefaultCellStyle.ForeColor))
            {
                e.Graphics.DrawString((e.RowIndex + 1).ToString(), e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 10, e.RowBounds.Location.Y + 4);
            }
        }

        //private void RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        //{

        //    //// add the numbering
        //    //foreach (DataGridViewRow row in dataGridView1.Rows)
        //    //{
        //    //    row.HeaderCell.Value = (row.Index + 1).ToString();
        //    //}


        //}

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;

        public void SetPrintResultsInListData(List<string> header, List<ChosenComposition> compositions,
            int selectedIndex, int sortIndex = MiscConstants.UNDEFINED_INT)
        {
            DataGridViewColumnCollection columns = this.dataGridView1.Columns;
            foreach (DataGridViewColumn dgc in columns)
            {
                dgc.HeaderCell.Style.BackColor = dataGridView1.BackgroundColor;
            }
 
            if (null != header)
            {
                dataGridView1.ColumnCount = header.Count;
                string[] headerA = header.ToArray();
                for (int i = 0; i < header.Count; i++)
                {
                    dataGridView1.Columns[i].Name = headerA[i];
                }
            }

            //int counter = 0;
            foreach (ChosenComposition comp in compositions)
            {
                dataGridView1.Rows.Add(comp.ToArray());
                //    if (counter++ > Share.numberOfPrintResultsInList)
                //        break;
            }

             if (MiscConstants.UNDEFINED_INT < selectedIndex && selectedIndex < dataGridView1.Columns.Count)
                dataGridView1.Columns[selectedIndex].HeaderCell.Style.BackColor = Color.Gray;

            if (MiscConstants.UNDEFINED_INT != sortIndex)
            {
                 dataGridView1.Sort(dataGridView1.Columns[sortIndex], ListSortDirection.Ascending);
            }

  
        }
        private System.Windows.Forms.Button button3;

        //private List<string> header;
        //private List<ChosenComposition> compositions;
    }
}