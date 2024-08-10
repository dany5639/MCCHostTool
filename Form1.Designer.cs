namespace MCCHostTool
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            textBox1 = new TextBox();
            statusStrip1 = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            dataGridView1 = new DataGridView();
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column9 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column10 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            Column7 = new DataGridViewTextBoxColumn();
            button3 = new Button();
            folderBrowserDialog1 = new FolderBrowserDialog();
            richTextBox1 = new RichTextBox();
            comboBox1_gameVariantOverrides = new ComboBox();
            label1 = new Label();
            comboBox2 = new ComboBox();
            label2 = new Label();
            textBox2 = new TextBox();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button1.Location = new Point(12, 588);
            button1.Name = "button1";
            button1.Size = new Size(100, 23);
            button1.TabIndex = 0;
            button1.Text = "Move Files";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_moveFiles;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button2.Location = new Point(118, 588);
            button2.Name = "button2";
            button2.Size = new Size(100, 23);
            button2.TabIndex = 1;
            button2.Text = "Clear previous";
            button2.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.Location = new Point(118, 11);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(944, 23);
            textBox1.TabIndex = 2;
            textBox1.Text = "E:\\_DLD_Large\\Games\\DigitalRetail\\SteamLibrary\\steamapps\\common\\Halo The Master Chief Collection\\";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            statusStrip1.Location = new Point(0, 614);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1074, 22);
            statusStrip1.TabIndex = 3;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(118, 17);
            toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // dataGridView1
            // 
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column9, Column3, Column10, Column4, Column5, Column6, Column7 });
            dataGridView1.ImeMode = ImeMode.Off;
            dataGridView1.Location = new Point(12, 76);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowTemplate.Height = 25;
            dataGridView1.Size = new Size(1050, 506);
            dataGridView1.TabIndex = 9;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            // 
            // Column1
            // 
            Column1.HeaderText = "Name";
            Column1.Name = "Column1";
            // 
            // Column2
            // 
            Column2.HeaderText = "Map variant";
            Column2.Name = "Column2";
            // 
            // Column9
            // 
            Column9.HeaderText = "Map description";
            Column9.Name = "Column9";
            // 
            // Column3
            // 
            Column3.HeaderText = "Game variant";
            Column3.Name = "Column3";
            // 
            // Column10
            // 
            Column10.HeaderText = "Game description";
            Column10.Name = "Column10";
            // 
            // Column4
            // 
            Column4.HeaderText = "Enabled";
            Column4.Name = "Column4";
            // 
            // Column5
            // 
            Column5.HeaderText = "Player count";
            Column5.Name = "Column5";
            // 
            // Column6
            // 
            Column6.HeaderText = "Comments";
            Column6.Name = "Column6";
            // 
            // Column7
            // 
            Column7.HeaderText = "Good or bad";
            Column7.Name = "Column7";
            // 
            // button3
            // 
            button3.Location = new Point(12, 11);
            button3.Name = "button3";
            button3.Size = new Size(100, 23);
            button3.TabIndex = 10;
            button3.Text = "Locate MCC";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click_locateMCC;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.Location = new Point(12, 76);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(1050, 506);
            richTextBox1.TabIndex = 11;
            richTextBox1.Text = "";
            // 
            // comboBox1_gameVariantOverrides
            // 
            comboBox1_gameVariantOverrides.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            comboBox1_gameVariantOverrides.FormattingEnabled = true;
            comboBox1_gameVariantOverrides.Location = new Point(324, 584);
            comboBox1_gameVariantOverrides.Name = "comboBox1_gameVariantOverrides";
            comboBox1_gameVariantOverrides.Size = new Size(635, 23);
            comboBox1_gameVariantOverrides.TabIndex = 12;
            comboBox1_gameVariantOverrides.SelectedValueChanged += comboBox1_gameVariantOverrides_SelectedValueChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(224, 592);
            label1.Name = "label1";
            label1.Size = new Size(94, 15);
            label1.TabIndex = 13;
            label1.Text = "Override variant:";
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(118, 40);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(151, 23);
            comboBox2.TabIndex = 14;
            comboBox2.SelectedValueChanged += comboBox2_SelectedValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 43);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 15;
            label2.Text = "Game";
            // 
            // textBox2
            // 
            textBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            textBox2.Location = new Point(965, 584);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(97, 23);
            textBox2.TabIndex = 16;
            textBox2.TextChanged += textBox2_TextChanged_overrideFilter;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1074, 636);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(comboBox2);
            Controls.Add(label1);
            Controls.Add(comboBox1_gameVariantOverrides);
            Controls.Add(button3);
            Controls.Add(dataGridView1);
            Controls.Add(statusStrip1);
            Controls.Add(textBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(richTextBox1);
            Name = "Form1";
            Text = "Form1";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private TextBox textBox1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private DataGridView dataGridView1;
        private Button button3;
        private FolderBrowserDialog folderBrowserDialog1;
        private RichTextBox richTextBox1;
        private ComboBox comboBox1_gameVariantOverrides;
        private Label label1;
        private ComboBox comboBox2;
        private Label label2;
        private TextBox textBox2;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column9;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column10;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column6;
        private DataGridViewTextBoxColumn Column7;
    }
}