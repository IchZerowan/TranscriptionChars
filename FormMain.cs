using Microsoft.Win32;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TranscriptionChars
{
    public partial class FormMain : Form
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        const UInt32 WM_KEYDOWN = 0x0100;
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        public const int WM_PASTE = 0x0302;
        public const int EM_REPLACESEL = 0x00C2;
        public const int EM_SETSEL = 0x00B1;

        IntPtr previousWindow = IntPtr.Zero;
        IntPtr lastWindow = IntPtr.Zero;

        public FormMain()
        {
            DoubleBuffered = true;
            SuspendLayout();
            int x = 0;
            int y = 0;
            foreach (string line in new string[] {
                "ɑʌəɛæɜʒıɪŋɔɒʃðθʤ", "ʊbdefghijklmnprʧ", "stuvwz[ ]ˌˈ:ː↵←"})
            {
                x = 0;
                foreach (char cur in line)
                {
                    Button button = new Button();
                    button.Location = new Point(x * 54, y * 54);
                    if (cur == ' ')
                    {
                        button.Size = new Size(104, 50);
                        x++;
                    } else
                    {
                        button.Size = new Size(50, 50);
                    }
                    button.Text = cur.ToString();
                    button.Font = new Font("Lucida Sans Unicode", 20);
                    button.BackColor = Color.FromArgb(179, 205, 224);
                    button.FlatStyle = FlatStyle.Flat;
                    button.TabIndex = 1;
                    button.Click += new EventHandler(button_Click);
                    Controls.Add(button);
                    x++;
                }
                y++;
            }

            Button btnClose = new Button();
            btnClose.Location = new Point(54 * 16, 0);
            btnClose.Size = new Size(20, 20);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Text = "X";
            btnClose.TabIndex = 0;
            btnClose.Click += BtnClose_Click;
            btnClose.BackColor = Color.FromArgb(179, 205, 224);
            Controls.Add(btnClose);

            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
            timer.Start();
            
            TopMost = true;
            MouseDown += new MouseEventHandler(FormMain_MouseDown);

            ClientSize = new Size(870, 120);

            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software");
                key = key.OpenSubKey("TranscriptionChars");
                Location = new Point((int) key.GetValue("position_x"), (int) key.GetValue("position_y"));
            }
            catch (Exception) {
                Location = new Point(100, 500);
            }

            ResumeLayout(true);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            IntPtr currentWindow = GetForegroundWindow();
            if(currentWindow != lastWindow)
            {
                previousWindow = lastWindow;
                lastWindow = currentWindow;
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);
                key = key.CreateSubKey("TranscriptionChars");
                key.SetValue("position_x", Location.X);
                key.SetValue("position_y", Location.Y);
            }
            catch (Exception) { }
            Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams ret = base.CreateParams;
                ret.Style = (int)Flags.WindowStyles.WS_CHILD;
                ret.ExStyle |= (int)Flags.WindowStyles.WS_EX_NOACTIVATE | (int)Flags.WindowStyles.WS_EX_TOOLWINDOW;
                ret.X = Location.X;
                ret.Y = Location.Y;
                return ret;
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            SetForegroundWindow(previousWindow);
            string text = ((Button)sender).Text;
            if(text == "↵")
            {
                text = "~";
            } else if(text == "←")
            {
                text = "{BS}";
            }
            SendKeys.Send(text);
            SetForegroundWindow(lastWindow);
        }

        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
