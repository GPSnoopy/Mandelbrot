namespace Mandelbrot
{
    partial class MainForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._pictureBox = new System.Windows.Forms.PictureBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this._toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this._zoomTextBox = new System.Windows.Forms.TextBox();
            this._benchmarkButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this._maxIterationsTextBox = new System.Windows.Forms.TextBox();
            this._doublePrecisionCheckBox = new System.Windows.Forms.CheckBox();
            this._backEndComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this._aboutButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this._pictureBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.statusStrip1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(802, 653);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // _pictureBox
            // 
            this._pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pictureBox.Location = new System.Drawing.Point(3, 42);
            this._pictureBox.Name = "_pictureBox";
            this._pictureBox.Size = new System.Drawing.Size(796, 583);
            this._pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this._pictureBox.TabIndex = 0;
            this._pictureBox.TabStop = false;
            this._pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
            this._pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            this._pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseUp);
            this._pictureBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseWheel);
            this._pictureBox.Resize += new System.EventHandler(this.pictureBox_Resize);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 628);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(802, 25);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // _toolStripStatusLabel
            // 
            this._toolStripStatusLabel.Name = "_toolStripStatusLabel";
            this._toolStripStatusLabel.Size = new System.Drawing.Size(143, 20);
            this._toolStripStatusLabel.Text = "toolStripStatusLabel";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 10;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this._zoomTextBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this._benchmarkButton, 8, 0);
            this.tableLayoutPanel2.Controls.Add(this.label4, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this._maxIterationsTextBox, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this._doublePrecisionCheckBox, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this._backEndComboBox, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this._aboutButton, 9, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(796, 33);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 33);
            this.label3.TabIndex = 4;
            this.label3.Text = "Zoom:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _zoomTextBox
            // 
            this._zoomTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._zoomTextBox.Location = new System.Drawing.Point(54, 5);
            this._zoomTextBox.Name = "_zoomTextBox";
            this._zoomTextBox.ReadOnly = true;
            this._zoomTextBox.Size = new System.Drawing.Size(100, 22);
            this._zoomTextBox.TabIndex = 5;
            this._zoomTextBox.Text = "1.000000E+000";
            // 
            // _benchmarkButton
            // 
            this._benchmarkButton.AutoSize = true;
            this._benchmarkButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._benchmarkButton.Location = new System.Drawing.Point(672, 3);
            this._benchmarkButton.Name = "_benchmarkButton";
            this._benchmarkButton.Size = new System.Drawing.Size(89, 27);
            this._benchmarkButton.TabIndex = 1;
            this._benchmarkButton.Text = "Benchmark";
            this._benchmarkButton.UseVisualStyleBackColor = true;
            this._benchmarkButton.Click += new System.EventHandler(this.benchmarkButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(160, 0);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 33);
            this.label4.TabIndex = 7;
            this.label4.Text = "Iterations:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _maxIterationsTextBox
            // 
            this._maxIterationsTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._maxIterationsTextBox.Location = new System.Drawing.Point(233, 5);
            this._maxIterationsTextBox.Name = "_maxIterationsTextBox";
            this._maxIterationsTextBox.Size = new System.Drawing.Size(60, 22);
            this._maxIterationsTextBox.TabIndex = 8;
            this._maxIterationsTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.maxIterationsTextBox_KeyPress);
            this._maxIterationsTextBox.Validated += new System.EventHandler(this.maxIterationsTextBox_Validated);
            // 
            // _doublePrecisionCheckBox
            // 
            this._doublePrecisionCheckBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._doublePrecisionCheckBox.AutoSize = true;
            this._doublePrecisionCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._doublePrecisionCheckBox.Location = new System.Drawing.Point(445, 6);
            this._doublePrecisionCheckBox.Name = "_doublePrecisionCheckBox";
            this._doublePrecisionCheckBox.Size = new System.Drawing.Size(75, 21);
            this._doublePrecisionCheckBox.TabIndex = 9;
            this._doublePrecisionCheckBox.Text = "Double";
            this._doublePrecisionCheckBox.UseVisualStyleBackColor = true;
            this._doublePrecisionCheckBox.CheckedChanged += new System.EventHandler(this.doublePrecisionCheckBox_CheckedChanged);
            // 
            // _backEndComboBox
            // 
            this._backEndComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._backEndComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._backEndComboBox.FormattingEnabled = true;
            this._backEndComboBox.Location = new System.Drawing.Point(369, 4);
            this._backEndComboBox.Name = "_backEndComboBox";
            this._backEndComboBox.Size = new System.Drawing.Size(70, 24);
            this._backEndComboBox.TabIndex = 6;
            this._backEndComboBox.SelectedIndexChanged += new System.EventHandler(this.backEndComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(299, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Backend:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _aboutButton
            // 
            this._aboutButton.AutoSize = true;
            this._aboutButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._aboutButton.Location = new System.Drawing.Point(767, 3);
            this._aboutButton.Name = "_aboutButton";
            this._aboutButton.Size = new System.Drawing.Size(26, 27);
            this._aboutButton.TabIndex = 11;
            this._aboutButton.Text = "?";
            this._aboutButton.UseVisualStyleBackColor = true;
            this._aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(802, 653);
            this.Controls.Add(this.tableLayoutPanel1);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(960, 700);
            this.Name = "MainForm";
            this.Text = "Mandelbrot";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox _pictureBox;
        private System.Windows.Forms.Button _benchmarkButton;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel _toolStripStatusLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox _zoomTextBox;
        private System.Windows.Forms.ComboBox _backEndComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox _maxIterationsTextBox;
        private System.Windows.Forms.CheckBox _doublePrecisionCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _aboutButton;
    }
}

