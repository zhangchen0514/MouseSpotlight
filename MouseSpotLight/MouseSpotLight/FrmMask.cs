using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MouseSpotLight
{
    public partial class FrmMask : Form
    {
        private Properties.Settings settings = Properties.Settings.Default;

        private Win32.BLENDFUNCTION blendFunc;
        private IntPtr screenDC;
        private IntPtr memDC;

        private FrmHelp frmHelp = new FrmHelp();
        private NotifyIcon notifyIcon;

        private Bitmap bmpMask;
        private Graphics graphMask;

        private int lightW, lightH;

        private const int SIZE_UNIT = 20;
        private const int SIZE_MAX = 40;
        private const int SIZE_MIN = 1;
        private const int SIZE_DEFAULT = 7;

        private const double ASPECT_UNIT = 0.2;
        private const int ASPECT_MAX = 10;
        private const int ASPECT_MIN = -10;
        private const int ASPECT_DEFAULT = 0;

        public FrmMask()
        {
            CreateSystemTrayIcon();

            InitializeComponent();
        }

        #region Override
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                // release resource
                if(notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                }

                Win32.UnregisterHotKey(this.Handle, 0);
                
                Win32.ReleaseDC(IntPtr.Zero, screenDC);
                Win32.DeleteDC(memDC);
            }
            base.Dispose(disposing);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            UpdateStyles();

            base.OnHandleCreated(e);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cParms = base.CreateParams;
                cParms.ExStyle |= 0x00080000; // WS_EX_LAYERED
                return cParms;
            }
        }
        #endregion

        #region draw mask
        private void ReDraw()
        {
            Point p = Cursor.Position;
            int x = p.X - lightW / 2;
            int y = p.Y - lightH / 2;

            graphMask.FillRectangle(new SolidBrush(Color.FromArgb(200, Color.Black)), 0, 0, this.Width, this.Height);

            //long t = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //for(int i = 8; i > 0; i--)
            //{
            //    Pen pen = new Pen(Color.FromArgb(200*i/9, Color.Black), 2);
            //    pen.Alignment = PenAlignment.Inset;
            //    graphMask.DrawEllipse(pen, x - i, y - i, lightW + i*2, lightH + i*2);
            //}
            //Console.WriteLine(DateTimeOffset.Now.ToUnixTimeMilliseconds() - t);

            graphMask.FillEllipse(new SolidBrush(Color.FromArgb(1, Color.White)), x, y, lightW, lightH);

            ShowBmp(bmpMask);
        }

        public void ShowBmp(Bitmap bitmap)
        {
            IntPtr oldBits = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            Win32.Point pointZero = new Win32.Point(0, 0);
            Win32.Size bitMapSize = new Win32.Size(bitmap.Width, bitmap.Height);

            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBits = Win32.SelectObject(memDC, hBitmap);
                
                Win32.UpdateLayeredWindow(Handle, screenDC, ref pointZero, ref bitMapSize, memDC, ref pointZero, 0, ref blendFunc, Win32.ULW_ALPHA);
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    Win32.SelectObject(memDC, oldBits);
                    Win32.DeleteObject(hBitmap);
                }
            }
        }
        #endregion

        #region CreateSystemTrayIcon
        private void CreateSystemTrayIcon()
        {

            // Initialize menu
            ContextMenu contextMenu = new ContextMenu();

            MenuItem menuItem2 = new MenuItem();
            menuItem2.Index = 1;
            menuItem2.Text = Properties.Strings.exit;
            menuItem2.Click += (sender, e) =>
            {
                this.Dispose();
                Application.Exit();
            };

            MenuItem menuItem1 = new MenuItem();
            menuItem1.Index = 0;
            menuItem1.Text = Properties.Strings.help;
            menuItem1.Click += (sender, e) =>
            {
                if(!frmHelp.Visible)
                {
                    frmHelp.Show();
                }
            };
            
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem1, menuItem2 });

            // Initialize System Tray Icon
            Container components = new Container();
            notifyIcon = new NotifyIcon(components);
            
            notifyIcon.Icon = Properties.Resources.icon;
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Text = Properties.Strings.appName;
            notifyIcon.Visible = true;

            notifyIcon.Click += (sender, e) =>
            {
                if (!frmHelp.Visible)
                {
                    frmHelp.Show();
                }
            };
        }
        #endregion

        private void FrmMask_Load(object sender, EventArgs e)
        {
            // Upgrade setting
            if (settings.isNewVersionInstalled)
            {
                settings.Upgrade();
                settings.isNewVersionInstalled = false;
                settings.Save();
            }

            // init draw parameters
            screenDC = Win32.GetDC(IntPtr.Zero);
            memDC = Win32.CreateCompatibleDC(screenDC);

            blendFunc = new Win32.BLENDFUNCTION();
            blendFunc.BlendOp = Win32.AC_SRC_OVER;
            blendFunc.SourceConstantAlpha = 255;
            blendFunc.AlphaFormat = Win32.AC_SRC_ALPHA;
            blendFunc.BlendFlags = 0;

            bmpMask = new Bitmap(this.Width, this.Height);
            graphMask = Graphics.FromImage(bmpMask);
            graphMask.SmoothingMode = SmoothingMode.AntiAlias;
            graphMask.CompositingMode = CompositingMode.SourceCopy;

            // init hotkey
            Win32.RegisterHotKey(this.Handle, 0, (int)Win32.KeyModifier.Control, Keys.Q.GetHashCode());

            CalculateLightSize();
        }

        private void CalculateLightSize()
        {
            lightW = settings.size * SIZE_UNIT;
            lightH = lightW;
            
            double aspect = Math.Pow(ASPECT_UNIT + 1, settings.aspect);

            lightW = (int)((double)lightH / aspect);

            //if (settings.aspect > 0)
            //{
            //    lightH = (int)((double)lightW / aspect);
            //}
            //else if (settings.aspect < 0)
            //{
            //    lightW = (int)((double)lightH / aspect);
            //}

            ReDraw();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                // ctrl + Q
                if (this.Visible)
                {
                    this.Hide();
                }
                else
                {
                    this.Show();
                }
            }
        }

        private void FrmMask_VisibleChanged(object sender, EventArgs e)
        {
            if(this.Visible)
            {
                Cursor.Hide();
            }
            else
            {
                Cursor.Show();
            }
        }

        private void FrmMask_MouseClick(object sender, MouseEventArgs e)
        {
            if (!this.Visible)
            {
                return;
            }
            this.Hide();
        }

        private void FrmMask_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Z)
            {
                if(settings.size >= SIZE_MAX)
                {
                    return;
                }
                settings.size += 1;
            }
            else if (e.KeyCode == Keys.X)
            {
                if (settings.size <= SIZE_MIN)
                {
                    return;
                }
                settings.size -= 1;
            }
            else if (e.KeyCode == Keys.S)
            {
                if (settings.aspect >= ASPECT_MAX)
                {
                    return;
                }
                settings.aspect += 1;
            }
            else if (e.KeyCode == Keys.A)
            {
                if (settings.aspect <= ASPECT_MIN)
                {
                    return;
                }
                settings.aspect -= 1;
            }
            else if (e.KeyCode == Keys.W)
            {
                settings.size = SIZE_DEFAULT;
                settings.aspect = ASPECT_DEFAULT;
            }
            else
            {
                return;
            }
            settings.Save();
            CalculateLightSize();
        }

        private void FrmMask_MouseMove(object sender, MouseEventArgs e)
        {
            if (!this.Visible)
            {
                return;
            }
            ReDraw();
        }
    }
}
