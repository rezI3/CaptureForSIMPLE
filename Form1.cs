using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CaptureForSIMPLE
{
    public partial class Form1 : Form
    {
        private bool _suspendRendering = false;

        private int CAPTURE_INTERVAL_MS = 100; // キャプチャ間隔（ミリ秒）


        public Form1()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi; // DPIスケーリング対応

         }

        private void Form1_Load(object sender, EventArgs e)
        {
            // フォーム全画面＆PictureBox をフォームにフィット
            this.WindowState = FormWindowState.Maximized;

            // PictureBox はフォームに追随して拡大縮小
            pictureBoxDisplay.Dock = DockStyle.Fill;
            pictureBoxDisplay.SizeMode = PictureBoxSizeMode.Zoom; // アスペクト比維持＋片側のみ余白
            pictureBoxDisplay.BackColor = Color.Black;            // 余白の色（任意）

            // キャプチャ頻度（任意で調整）
            captureTimer.Interval = CAPTURE_INTERVAL_MS;

            // まずクリア
            comboBoxMonitors.Items.Clear();

            // モニタ一覧
            comboBoxMonitors.Items.AddRange(Screen.AllScreens.Select(s => s.DeviceName).ToArray());
            if (comboBoxMonitors.Items.Count > 0)
            {
                comboBoxMonitors.SelectedIndex = 0;
            }
        }

        private void comboBoxMonitors_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxMonitors.SelectedIndex >= 0 && !_suspendRendering)
            {
                if (!captureTimer.Enabled) captureTimer.Start();
                // すぐに1回表示を更新
                CaptureSelectedMonitor();
            }
            else
            {
                if (captureTimer.Enabled) captureTimer.Stop();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // 最小化中はレンダリング停止
            _suspendRendering = (this.WindowState == FormWindowState.Minimized);

            if (_suspendRendering)
            {
                if (captureTimer.Enabled) captureTimer.Stop();
                return;
            }

            // 復帰時は再開 & 1回キャプチャ
            if (comboBoxMonitors.SelectedIndex >= 0 && !captureTimer.Enabled)
            {
                captureTimer.Start();
            }
            CaptureSelectedMonitor();
        }

        private void captureTimer_Tick(object sender, EventArgs e)
        {
            CaptureSelectedMonitor();
        }

        private void CaptureSelectedMonitor()
        {
            // ガード：最小化 or モニタ未選択
            if (_suspendRendering) return;
            if (comboBoxMonitors.SelectedIndex < 0) return;

            // PictureBox の描画領域が 0 のときは何もしない
            var w = pictureBoxDisplay.ClientSize.Width;
            var h = pictureBoxDisplay.ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            // 選択モニタ
            var selectedScreen = Screen.AllScreens[comboBoxMonitors.SelectedIndex];
            var bounds = selectedScreen.Bounds;

            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            // キャプチャ元の Bitmap を作成（原寸）
            using (var captureBitmap = new Bitmap(bounds.Width, bounds.Height))
            using (var g = Graphics.FromImage(captureBitmap))
            {
                // 画面キャプチャ
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);

                // マウスカーソルを描画（必要なら）
                var cursorPosition = Cursor.Position;
                cursorPosition.Offset(-bounds.Left, -bounds.Top);
                var cursorBounds = new Rectangle(cursorPosition, Cursors.Default.Size);
                Cursors.Default.Draw(g, cursorBounds);

                // 既存イメージを破棄
                if (pictureBoxDisplay.Image != null)
                {
                    pictureBoxDisplay.Image.Dispose();
                    pictureBoxDisplay.Image = null;
                }

                // ★ここが①の要点：
                // 手動スケーリングはせずに、原寸のビットマップを PictureBox に渡す。
                // PictureBoxSizeMode.Zoom がアスペクト比を保ちつつ、片側のみ余白でフィットさせる。
                pictureBoxDisplay.Image = (Image)captureBitmap.Clone();
            }
        }
    }
}