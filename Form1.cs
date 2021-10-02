using System;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CreateRecorder
{
    public partial class Form1 : Form
    {

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        public Form1()
        {
            InitializeComponent();
            panel1.MouseMove += new MouseEventHandler(panel1_MouseMove);

            SoftBlink(lblWarning, Color.FromArgb(30, 30, 30), Color.Red, 2000, false);
            SoftBlink(lblSoftBlink, Color.FromArgb(30, 30, 30), Color.Red, 2000, false);
            SoundPlayer player = new SoundPlayer(@"C:\Users\justin.ross\source\repos\CreateRecorder\CreateRecorder\Resources\intro.wav");
            player.Play();
        }

        private async void SoftBlink(Control ctrl, Color c1, Color c2, short CycleTime_ms, bool BkClr)
        {
            var sw = new Stopwatch(); sw.Start();
            short halfCycle = (short)Math.Round(CycleTime_ms * 0.5);
            while (true)
            {
                await Task.Delay(1);
                var n = sw.ElapsedMilliseconds % CycleTime_ms;
                var per = (double)Math.Abs(n - halfCycle) / halfCycle;
                var red = (short)Math.Round((c2.R - c1.R) * per) + c1.R;
                var grn = (short)Math.Round((c2.G - c1.G) * per) + c1.G;
                var blw = (short)Math.Round((c2.B - c1.B) * per) + c1.B;
                var clr = Color.FromArgb(red, grn, blw);
                if (BkClr) ctrl.BackColor = clr; else ctrl.ForeColor = clr;
            }
        }
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {

                Point loc1 = MousePosition;
                this.Location = loc1;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();

        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            Form2 frm = new Form2();
            frm.Show();
            Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }


}
