// Form1.Designer.cs
namespace CaptureForSIMPLE
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ComboBox comboBoxMonitors;
        private System.Windows.Forms.PictureBox pictureBoxDisplay;
        private System.Windows.Forms.Timer captureTimer; // タイマーを追加
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel; // TableLayoutPanelを追加

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            comboBoxMonitors = new ComboBox();
            pictureBoxDisplay = new PictureBox();
            captureTimer = new System.Windows.Forms.Timer(components);
            tableLayoutPanel = new TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)pictureBoxDisplay).BeginInit();
            tableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // comboBoxMonitors
            // 
            comboBoxMonitors.Dock = DockStyle.Fill;
            comboBoxMonitors.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxMonitors.FormattingEnabled = true;
            comboBoxMonitors.Location = new Point(5, 6);
            comboBoxMonitors.Margin = new Padding(5, 6, 5, 6);
            comboBoxMonitors.Name = "comboBoxMonitors";
            comboBoxMonitors.Size = new Size(1297, 33);
            comboBoxMonitors.TabIndex = 0;
            comboBoxMonitors.SelectedIndexChanged += comboBoxMonitors_SelectedIndexChanged;
            // 
            // pictureBoxDisplay
            // 
            pictureBoxDisplay.Dock = DockStyle.Fill;
            pictureBoxDisplay.Location = new Point(5, 82);
            pictureBoxDisplay.Margin = new Padding(5, 6, 5, 6);
            pictureBoxDisplay.Name = "pictureBoxDisplay";
            pictureBoxDisplay.Size = new Size(1297, 991);
            pictureBoxDisplay.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxDisplay.TabIndex = 1;
            pictureBoxDisplay.TabStop = false;
            // 
            // captureTimer
            // 
            captureTimer.Tick += captureTimer_Tick;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(comboBoxMonitors, 0, 0);
            tableLayoutPanel.Controls.Add(pictureBoxDisplay, 0, 1);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);
            tableLayoutPanel.Margin = new Padding(5, 6, 5, 6);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 2;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Size = new Size(1307, 1079);
            tableLayoutPanel.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1307, 1079);
            Controls.Add(tableLayoutPanel);
            Margin = new Padding(5, 6, 5, 6);
            Name = "Form1";
            Text = "Screen Capture";
            Load += Form1_Load;
            Resize += Form1_Resize;
            ((System.ComponentModel.ISupportInitialize)pictureBoxDisplay).EndInit();
            tableLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}