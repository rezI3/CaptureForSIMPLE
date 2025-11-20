using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices; // これをファイルの先頭(using群)に追加してください

namespace CaptureForSIMPLE
{
    public partial class Form1 : Form
    {
        private bool _suspendRendering = false;

        private int CAPTURE_INTERVAL_MS = 10; // キャプチャ間隔（ミリ秒）

        private int systemCursorSize = 32; // システムカーソルのサイズ（ピクセル）

        // ▼▼▼ API定義の追加（Form1クラスの内側、メソッドの外側に貼り付け） ▼▼▼


[StructLayout(LayoutKind.Sequential)]
    struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public Point ptScreenPos;
}

[DllImport("user32.dll")]
        static extern bool GetCursorInfo(ref CURSORINFO pci);

        const int CURSOR_SHOWING = 0x00000001;

        // ▲▲▲ API定義の追加ここまで ▲▲▲

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

            // マウスカーソルの大きさを取得
            systemCursorSize = GetSystemCursorSize();
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

                var newSize = new Size(systemCursorSize, systemCursorSize);

                // 2. 描画位置の調整
                var cursorPosition = Cursor.Position;
                cursorPosition.Offset(-bounds.Left, -bounds.Top);
                var cursorBounds = new Rectangle(cursorPosition, newSize);

                // 3. 現在のカーソル情報の取得（API使用）
                CURSORINFO pci = new CURSORINFO();
                pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

                if (GetCursorInfo(ref pci))
                {
                    // カーソルが表示されている場合のみ描画
                    if (pci.flags == CURSOR_SHOWING)
                    {
                        // pci.hCursor には「現在表示されている色付きカーソル」のハンドルが入っています
                        // これを DrawIcon で描画します
                        try
                        {
                            using (var icon = Icon.FromHandle(pci.hCursor))
                            {
                                g.DrawIcon(icon, cursorBounds);
                            }
                        }
                        catch
                        {
                            // 万が一ハンドルの取得に失敗した場合は、デフォルトを描画（保険）
                            Cursors.Default.Draw(g, cursorBounds);
                        }
                    }
                }

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

        /// <summary>
        /// Windowsの設定（アクセシビリティ）で指定されたカーソルのサイズを取得します。
        /// 取得できない場合は標準の32を返します。
        /// </summary>
        private int GetSystemCursorSize()
        {
            try
            {
                // ユーザーごとの設定が保存されているレジストリキーを開く
                using (var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Cursors"))
                {
                    if (key != null)
                    {
                        // "CursorBaseSize" という名前の値を取得
                        var val = key.GetValue("CursorBaseSize");
                        if (val != null)
                        {
                            return Convert.ToInt32(val);
                        }
                    }
                }
            }
            catch
            {
                // エラーが発生した場合やキーがない場合は標準サイズを返す
            }
            return 32; // デフォルト（32x32）
        }
    }
}