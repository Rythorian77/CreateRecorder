using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CreateRecorder
{
    public partial class Form2 : Form
    {
        // Create class-level accessible variables to store the audio recorder and capturer instance
        private WaveFileWriter RecordedAudioWriter = null;
        private WasapiLoopbackCapture CaptureInstance = null;

        //New
        public WaveIn waveSource = null;
        public WaveFileWriter waveFile = null;

        private readonly Stopwatch pendulum = new Stopwatch();

        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);


        public Form2()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message Rythorian)
        {
            base.WndProc(ref Rythorian);
            if (Rythorian.Msg == WM_NCHITTEST)
                Rythorian.Result = (IntPtr)(HT_CAPTION);
        }
        private const int WM_NCHITTEST = 0x84;
        private const int HT_CAPTION = 0x2;

        //Plays System Recordings
        private void sysAudio_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\Users\justin.ross\Desktop\trimmed_file_audio.wav");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();
            pendulum.Start();
            string outputFilePath = @"C:\Users\justin.ross\Desktop\trimmed_file_audio.wav";

            // Redefine the capturer instance with a new instance of the LoopbackCapture class
            CaptureInstance = new WasapiLoopbackCapture();

            // Redefine the audio writer instance with the given configuration
            RecordedAudioWriter = new WaveFileWriter(outputFilePath, CaptureInstance.WaveFormat);

            // When the capturer receives audio, start writing the buffer into the mentioned file
            CaptureInstance.DataAvailable += (s, a) =>
            {
                RecordedAudioWriter.Write(a.Buffer, 0, a.BytesRecorded);
            };

            // When the Capturer Stops
            CaptureInstance.RecordingStopped += (s, a) =>
            {
                RecordedAudioWriter.Dispose();
                RecordedAudioWriter = null;
                CaptureInstance.Dispose();
            };

            // Enable "Stop button" and disable "Start Button"
            button1.Enabled = false;
            button2.Enabled = true;

            // Start recording !
            CaptureInstance.StartRecording();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Stop recording !
            CaptureInstance.StopRecording();

            // Enable "Start button" and disable "Stop Button"
            button1.Enabled = true;
            button2.Enabled = false;
            timer1.Stop();
            pendulum.Stop();
        }

        public static void TrimWavFile(string inPath, string outPath, TimeSpan duration)
        {
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat))
                {
                    float bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000f;

                    int startPos = 0;//Change start position to desired time length
                    startPos -= startPos % reader.WaveFormat.BlockAlign;

                    int endBytes = (int)Math.Round(duration.TotalMilliseconds * bytesPerMillisecond);
                    endBytes -= endBytes % reader.WaveFormat.BlockAlign;
                    int endPos = endBytes;

                    TrimWavFile(reader, writer, startPos, endBytes);
                }
            }
        }
        //I have two recordings of same event, different lengths, started at different times.
        //I want to synchronize them, time offset is known. I want to achieve the following:
        //Align second one in time by the time offset.
        //Trim second one to match the length of the first one
        //When there is nothing to trim, add silence to match the length of the first one.
        public static void WriteSilence(WaveFormat waveFormat, int silenceMilliSecondLength, WaveFileWriter waveFileWriter)
        {
            int bytesPerMillisecond = waveFormat.AverageBytesPerSecond / 1000;
            //an new all zero byte array will play silence
            var silentBytes = new byte[silenceMilliSecondLength * bytesPerMillisecond];
            waveFileWriter.Write(silentBytes, 0, silentBytes.Length);
            waveFileWriter.Dispose();
        }

        private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = startPos;
            byte[] buffer = new byte[reader.BlockAlign * 1024];
            while (reader.Position < endPos)
            {
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        //This allows you to select an audio file
        private void button5_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBox1.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(1));
            }
            else if (checkBox2.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(2));
            }
            else if (checkBox3.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(3));
            }
            else if (checkBox4.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(4));
            }
            else if (checkBox5.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(5));
            }
            else if (checkBox6.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(6));
            }
            else if (checkBox7.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(7));
            }

            else if (checkBox8.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(8));
            }

            else if (checkBox9.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(9));
            }

            else if (checkBox10.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromSeconds(10));
            }

            else if (checkBox11.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(1));
            }

            else if (checkBox12.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(2));
            }

            else if (checkBox13.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(3));
            }

            else if (checkBox14.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(4));
            }

            else if (checkBox15.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(5));
            }

            else if (checkBox16.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(6));
            }

            else if (checkBox17.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(7));
            }

            else if (checkBox18.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(8));
            }

            else if (checkBox19.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(9));
            }
            else if (checkBox20.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromMinutes(10));
            }

            else if (checkBox21.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromHours(1));
            }

            else if (checkBox22.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromHours(2));
            }

            else if (checkBox23.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromHours(3));
            }

            else if (checkBox24.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromHours(4));
            }

            else if (checkBox25.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromHours(5));
            }

            else if (checkBox26.Checked)
            {
                var filename = textBox1.Text;
                FileInfo fi = new FileInfo(filename);
                var outputPath = Path.Combine(fi.Directory.FullName, string.Format("{0}_Shorter{1}", fi.Name.Replace(fi.Extension, ""), fi.Extension));
                TrimWavFile(filename, outputPath, TimeSpan.FromHours(6));
            }

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            lblCounter.Text = pendulum.Elapsed.ToString(@"hh\:mm\:ss");
        }

        public string ElapsedTime
        {
            get => lblCounter.Text;
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            StartBtn.Enabled = false;
            StopBtn.Enabled = true;

            waveSource = new WaveIn
            {
                WaveFormat = new WaveFormat(44100, 1)
            };

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);

            waveFile = new WaveFileWriter(@"C:\Users\justin.ross\Desktop\Test.wav", waveSource.WaveFormat);

            waveSource.StartRecording();
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            StopBtn.Enabled = false;

            waveSource.StopRecording();
        }

        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
        }

        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }

            StartBtn.Enabled = true;
        }

        private void PlayBtn_Click(object sender, EventArgs e)
        {
            Process.Start(@"C:\Users\justin.ross\Desktop\Test.wav");
        }

       
    }
}
