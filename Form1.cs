using System;
using System.Drawing;
using System.Drawing.Drawing2D; // 追加: 描画品質向上のため
using System.Linq;
using System.Windows.Forms;

namespace CaptureForSIMPLE
{
    public partial class Form1 : Form
    {
        private bool _suspendRendering = false;

        // キャプチャ間隔（ミリ秒）
        private int CAPTURE_INTERVAL_MS = 10;

        // ▼▼▼ 設定エリア ▼▼▼

        // カーソルの大きさ（ピクセル）
        // 50〜100くらいが大きく見やすいサイズです
        private int _fixedCursorSize = 90;

        // カーソルの色
        private Color _fixedCursorColor = Color.Yellow;

        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        public Form1()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // フォーム全画面設定
            this.WindowState = FormWindowState.Maximized;

            // PictureBox設定
            pictureBoxDisplay.Dock = DockStyle.Fill;
            pictureBoxDisplay.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxDisplay.BackColor = Color.Black;

            // タイマー設定
            captureTimer.Interval = CAPTURE_INTERVAL_MS;

            // モニタ一覧取得
            comboBoxMonitors.Items.Clear();
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
                CaptureSelectedMonitor();
            }
            else
            {
                if (captureTimer.Enabled) captureTimer.Stop();
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // 最小化中は処理停止
            _suspendRendering = (this.WindowState == FormWindowState.Minimized);

            if (_suspendRendering)
            {
                if (captureTimer.Enabled) captureTimer.Stop();
                return;
            }

            // 復帰時
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
            if (_suspendRendering) return;
            if (comboBoxMonitors.SelectedIndex < 0) return;

            var w = pictureBoxDisplay.ClientSize.Width;
            var h = pictureBoxDisplay.ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            var selectedScreen = Screen.AllScreens[comboBoxMonitors.SelectedIndex];
            var bounds = selectedScreen.Bounds;

            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            // キャプチャ用ビットマップ作成
            using (var captureBitmap = new Bitmap(bounds.Width, bounds.Height))
            using (var g = Graphics.FromImage(captureBitmap))
            {
                // 1. 画面キャプチャ
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);

                // 2. マウス位置計算
                var cursorPosition = Cursor.Position;
                cursorPosition.Offset(-bounds.Left, -bounds.Top);

                // 描画品質を上げる設定（ギザギザ軽減）
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 3. カーソル形状の定義（Windows標準ポインタ比率）
                float s = _fixedCursorSize;
                float x = cursorPosition.X;
                float y = cursorPosition.Y;

                // 標準的な矢印カーソルの頂点（7点）
                // 歪みをなくすため、標準的なジオメトリ比率を使用しています
                PointF[] arrowPoints = {
                    new PointF(x, y),                                   // 1. 先端
                    new PointF(x, y + s),                               // 2. 左辺の下端
                    new PointF(x + (0.27f * s), y + (0.72f * s)),       // 3. 左のくびれ開始
                    new PointF(x + (0.45f * s), y + (1.05f * s)),       // 4. しっぽの左下
                    new PointF(x + (0.62f * s), y + (0.90f * s)),       // 5. しっぽの右下
                    new PointF(x + (0.44f * s), y + (0.58f * s)),       // 6. 右のくびれ開始
                    new PointF(x + (0.78f * s), y + (0.58f * s))        // 7. 右の翼端
                };

                // 4. 描画（塗りつぶし）
                using (var brush = new SolidBrush(_fixedCursorColor))
                {
                    g.FillPolygon(brush, arrowPoints);
                }

                // 5. 縁取り（黒枠）
                // 幅を3pxにして視認性を向上
                using (var pen = new Pen(Color.Black, 3))
                {
                    pen.LineJoin = LineJoin.Round; // 角を丸めて自然に見せる
                    g.DrawPolygon(pen, arrowPoints);
                }

                // 6. 表示更新
                if (pictureBoxDisplay.Image != null)
                {
                    pictureBoxDisplay.Image.Dispose();
                    pictureBoxDisplay.Image = null;
                }

                pictureBoxDisplay.Image = (Image)captureBitmap.Clone();
            }
        }
    }
}