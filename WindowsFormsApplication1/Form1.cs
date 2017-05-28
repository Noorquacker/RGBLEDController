using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

//Let the required info begin.
namespace WindowsFormsApplication1 {
    public partial class Form1 : Form {
        public static bool enable = true;
        public static int mode = 1;
        public static int freq = 3;
        public static int resetArd = 0;
        public static int saveToFile = 0;
        public static int[] chosencolor = new int[3];
        public static bool allowshowdisplay;
        public Form1(){
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            //I have learned my mistake.
            //Thread.Sleep(1000);
            List<RadioButton> radioButtons = new List<RadioButton> { radioButton1, radioButton2, radioButton3, radioButton4, radioButton5, radioButton6 };
            for(int i = 0;i<=5;i++) {
                radioButtons[i].Checked = (mode == i+1);
            }
            label2.Text = enable ? "ON" : "OFF";
            label2.ForeColor = enable ? Color.FromName("Green") : Color.FromName("Red");
            trackBar1.Value = freq;
        }
        protected override void SetVisibleCore(bool value) {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Program.closeApp();
            Environment.Exit(1);
        }
        protected override void OnFormClosing(FormClosingEventArgs e) {
            Program.closeApp();
            Environment.Exit(1);
            base.OnFormClosing(e);
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            allowshowdisplay = true;
            Show();
        }

        private void button3_Click(object sender, EventArgs e) {
            Hide();
            allowshowdisplay = false;
            notifyIcon1.BalloonTipText = "I'm still here";
            notifyIcon1.BalloonTipTitle = "RGB LED Controller";
            //notifyIcon1.ShowBalloonTip(2);
        }
        //Now let's do some good stuff.
        private void button1_Click(object sender, EventArgs e) {
            //enable rgb stuff
            
            enable = true;
            this.label2.Text = "ON";
            this.label2.ForeColor = Color.FromName("Green");
        }

        private void button2_Click(object sender, EventArgs e) {
            //disable rgb stuff
            enable = false;
            this.label2.Text = "OFF";
            this.label2.ForeColor = Color.FromName("Red");
        }
        private void label1_Click(object sender, EventArgs e) {
            Console.WriteLine("clicked");
            if(enable) {
                MessageBox.Show("on");
            }
            else {
                MessageBox.Show("off");
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            colorDialog1.ShowDialog();
            int test = colorDialog1.Color.ToArgb();
            Console.WriteLine(test.ToString());
            chosencolor[0] = (test >> 16) & 255;
            chosencolor[1] = (test >> 8) & 255;
            chosencolor[2] = test & 255;
        }
        public static void ShowWaiting() {
            //label2.Text = "WAITING...";
            //label2.ForeColor = Color.FromName("Yellow");
        }
        private void label2_Click(object sender, EventArgs e) {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            checkMode();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e) {
            checkMode();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e) {
            checkMode();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e) {
            checkMode();
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e) {
            checkMode();
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e) {
            checkMode();
        }
        private void checkMode() {
            if(radioButton1.Checked) {
                mode = 1;
            }
            else if (radioButton2.Checked) {
                mode = 2;
            }
            else if (radioButton3.Checked) {
                mode = 3;
            }
            else if (radioButton4.Checked) {
                mode = 4;
            }
            else if (radioButton5.Checked) {
                mode = 5;
            }
            else if (radioButton6.Checked) {
                mode = 6;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e) {
            freq = trackBar1.Value;
        }

        private void button4_Click(object sender, EventArgs e) {
            resetArd = 1;
        }

        private void button5_Click(object sender, EventArgs e) {
            saveToFile = 1;
        }
    }
}
