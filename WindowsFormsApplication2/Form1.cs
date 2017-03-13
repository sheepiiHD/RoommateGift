using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        Thread run;
        Thread checkForActivity;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            run = new Thread(runProgram);
            run.Start();

            checkForActivity = new Thread(checkActivity);

        }

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private void runProgram()
        {
            bool running = true;
            while (running)
            {
                TimeSpan start = new TimeSpan(0, 0, 0);
                TimeSpan end = new TimeSpan(8, 0, 0);

                int minute = 60000; // Minute in milliseconds
                int idle_stage1 = minute * 30;
                int idle_stage2 = minute * 59;
                int idle_stage3 = minute * 60;

                if (IsBetween(DateTime.Now, start, end))
                {
                    //Reset
                    if (IdleTimeFinder.GetIdleTime() < 3000)
                    {
                        setTextBox_Text("Initializing...");

                        setForm_Color(Color.ForestGreen);
                       
                    }

                    else if (IdleTimeFinder.GetIdleTime() >= 3000 && IdleTimeFinder.GetIdleTime() < idle_stage1)
                    {
                        setTextBox_Text("Started, idle time is " + Math.Ceiling(TimeSpan.FromMilliseconds(IdleTimeFinder.GetIdleTime()).TotalMinutes) + " minutes.");
                    }
                    //Warn them after 30 minutes
                    else if (IdleTimeFinder.GetIdleTime() >= idle_stage1 && IdleTimeFinder.GetIdleTime() < idle_stage2)
                    {
                        setTextBox_Text("Turning sound off in 30 minutes.");
                    }
                    //Tell them they're going to turn off in 1 minute.
                    else if (IdleTimeFinder.GetIdleTime() >= idle_stage2 && IdleTimeFinder.GetIdleTime() < idle_stage3)
                    {
                        setTextBox_Text("Turning sound off in 1 minute.");
                        setForm_Color(Color.Red);
                        System.Media.SystemSounds.Hand.Play();
                        Thread.Sleep(5000);
                    }
                    // Turn off sound. 
                    else if (IdleTimeFinder.GetIdleTime() >= idle_stage3)
                    {
                        setTextBox_Text("Sound is disabled.");
                        mute();
                        running = false;
                        enableThread(1);
                    }
                    else
                    {
                        setTextBox_Text("ERROR");
                    }
                }
                else
                {
                    setTextBox_Text("Time will start at 12am and end at 8am.");
                    setForm_Color(Color.Orange);
                }
                Thread.Sleep(1000);
            }
        }
       
        public void checkActivity()
        {
            bool running = true;
            while (running)
            {
                if (IdleTimeFinder.GetIdleTime() < 3000)
                {
                    mute();
                    enableThread(2);
                    running = false;
                }
            }
        }

        public bool IsBetween(DateTime now, TimeSpan start, TimeSpan end)
        {
            var time = now.TimeOfDay;
            if (start <= end)
            {
                return time >= start && time <= end;
            }
            return time >= start || time <= end;
        }

        public void setTextBox_Text(string s)
        {
            MethodInvoker inv = delegate
            {
                this.label1.Text = s;
            };
            try
            {
                this.Invoke(inv);
            }
            catch (Exception e)
            {
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            }
        }

        public void setForm_Color(Color c)
        {
            MethodInvoker inv = delegate
            {
                this.BackColor = c;
            };
            try
            {
                this.Invoke(inv);
            }
            catch (Exception e)
            {
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            }
        }

        public void mute()
        {
            MethodInvoker inv = delegate
            {
                SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
            };
            try
            {
                this.Invoke(inv);
            }
            catch (Exception e)
            {
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            }
        }

        //Thread Controller
        public void enableThread(int i){
            MethodInvoker inv = delegate
            {
                switch (i)
                {
                    case 1:
                        checkForActivity = new Thread(checkActivity);
                        checkForActivity.Start();
                        break;
                    case 2:
                        run = new Thread(runProgram);
                        run.Start();
                        break;
                }
            };
            try
            {
                this.Invoke(inv);
    }
            catch (Exception e)
            {
                Application.Exit();
                Environment.Exit(Environment.ExitCode);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(Environment.ExitCode);
        }



        protected override void OnMouseDown(MouseEventArgs e)

        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                this.Capture = false;
                Message msg = Message.Create(this.Handle, 0XA1, new IntPtr(2), IntPtr.Zero);
                this.WndProc(ref msg);
            }
        }
    }
}
