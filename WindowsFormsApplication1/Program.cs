using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO.Ports;
using NAudio.CoreAudioApi;
using Microsoft.Win32;
using System.Reflection;
using System.Timers;

namespace WindowsFormsApplication1 {
    static class Program {
        [STAThread]
        static void Main(String[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Task.Factory.StartNew(() => Application.Run(new Form1()));

            //Variable junk
            SerialPort serial = new SerialPort("COM3", 57600);
            int FadeVal = 0;
            int FadeMode = 1;
            int[] FadeHSL = new int[3] { 255, 0, 0 };
            int FadeHSLMode = 0;
            DateTime StartTime = DateTime.Now;
            bool ReadyRedo = true;
            OHData hardwareData = new OHData();
            var initpins = new byte[3] { 0x0B, 0x06, 0x03 };
            MMDevice audio = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

            //Capture system resume event and redo serial stuff
            SystemEvents.PowerModeChanged += OnPowerChange;
            void OnPowerChange(object s, PowerModeChangedEventArgs e)
            {
                if (e.Mode == PowerModes.Resume) {
                    Console.WriteLine("REDOING SERIAL STUFF");
                    serial.Close();
                    serial = new SerialPort("COM3", 57600);                            //HA n00bz this is hardcoded!
                    serial.Open();
                    for (int i = 0; i <= 2; i++) {
                        Console.WriteLine("Configuring pin {0}", initpins[i]);
                        var initmsg = new byte[3];
                        initmsg[0] = 0xF4;
                        initmsg[1] = initpins[i];
                        initmsg[2] = 0x03;
                        serial.Write(initmsg, 0, 3);
                        Thread.Sleep(50);
                    }
                }
            }

            //Do some config stuffy
            Form1.enable = Properties.Settings.Default.Enable;
            Form1.mode = Properties.Settings.Default.Mode;
            Form1.chosencolor = new int[3] { Properties.Settings.Default.ColourR, Properties.Settings.Default.ColourG, Properties.Settings.Default.ColourB };
            Form1.freq = Properties.Settings.Default.Freq;

            //Initialize serial stuff
            serial.ReadTimeout = 500;
            serial.WriteTimeout = 500;
            try { serial.Open(); }
            catch (UnauthorizedAccessException) {
                MessageBox.Show("Access to serial port COM3 denied!", "RGB LED Controller", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(50);
            }
            catch (System.IO.IOException) {
                MessageBox.Show("Serial port COM3 does not exist!", "RGB LED Controller", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(51);
            }
            Console.Write("Opening serial port...");
            while (!serial.IsOpen) { } //doot doot...
            Console.WriteLine("done!");

            //Go back to the bitwise junk and set pins 11, 6, and 3 to PWM mode
            Thread.Sleep(1000);
            for (int i = 0; i <= 2; i++) {
                Console.WriteLine("Configuring pin {0}", initpins[i]);
                var initmsg = new byte[3];
                initmsg[0] = 0xF4;
                initmsg[1] = initpins[i];
                initmsg[2] = 0x03;
                serial.Write(initmsg, 0, 3);
                Thread.Sleep(50);
            }

            //Now do our pwm junkers
            while (true) {
                if (Form1.enable) {
                    switch (Form1.mode) {
                        case 1:
                            setRGB(serial, (int)Math.Floor(audio.AudioMeterInformation.MasterPeakValue * Form1.chosencolor[0]), (int)Math.Floor(audio.AudioMeterInformation.MasterPeakValue * Form1.chosencolor[1]), (int)Math.Floor(audio.AudioMeterInformation.MasterPeakValue * Form1.chosencolor[2]));
                            break;
                        case 2:
                            setRGB(serial, Form1.chosencolor[0], Form1.chosencolor[1], Form1.chosencolor[2]);
                            break;
                        case 3:
                            setRGB(serial, (int)(Form1.chosencolor[0] * ((double)FadeVal / 255)), (int)(Form1.chosencolor[1] * ((double)FadeVal / 255)), (int)(Form1.chosencolor[2] * ((double)FadeVal / 255)));
                            break;
                        case 4:
                            setRGB(serial, (FadeVal >> 7) % 2 * Form1.chosencolor[0], (FadeVal >> 7) % 2 * Form1.chosencolor[1], (FadeVal >> 7) % 2 * Form1.chosencolor[2]);
                            break;
                        case 5:
                            switch (FadeHSLMode) {
                                case 0:
                                    FadeHSL[1] += 1;
                                    if (FadeHSL[1] == 255) { FadeHSLMode++; }
                                    break;
                                case 1:
                                    FadeHSL[0] -= 1;
                                    if (FadeHSL[0] == 0) { FadeHSLMode++; }
                                    break;
                                case 2:
                                    FadeHSL[2] += 1;
                                    if (FadeHSL[2] == 255) { FadeHSLMode++; }
                                    break;
                                case 3:
                                    FadeHSL[1] -= 1;
                                    if (FadeHSL[1] == 0) { FadeHSLMode++; }
                                    break;
                                case 4:
                                    FadeHSL[0] += 1;
                                    if (FadeHSL[0] == 255) { FadeHSLMode++; }
                                    break;
                                case 5:
                                    FadeHSL[2] -= 1;
                                    if (FadeHSL[2] == 0) { FadeHSLMode = 0; }
                                    break;
                                default:
                                    throw new Exception("Error fading Color Cycle's color.");
                            }
                            setRGB(serial, FadeHSL[0], FadeHSL[1], FadeHSL[2]);
                            break;
                        case 6:
                            hardwareData.Update();
                            try {
                                List<OHData.OHMitem> hardwareItems = hardwareData.DataList;
                                int cpuTemp = 20;
                                foreach (OHData.OHMitem hwItem in hardwareItems) {
                                    if (hwItem.type == "Temperature: " && hwItem.name == "CPU Package") {
                                        cpuTemp = Convert.ToInt16(hwItem.reading);
                                        Console.WriteLine(cpuTemp);
                                    }
                                }
                                cpuTemp -= 20;
                                setRGB(serial, (int)Math.Round((cpuTemp) * 4.25d), (int)Math.Round((60 - cpuTemp) * 4.25d), 0);
                            }
                            catch (FormatException) { setRGB(serial, 0, 0, 0); }
                            break;
                        default:
                            setRGB(serial, 0, 0, 0);
                            break;
                    }
                    FadeVal += FadeMode * 7;
                    if (FadeVal >= 245) { FadeMode = -1; }
                    else if (FadeVal <= 0) { FadeMode = 1; }
                }
                else {
                    //setRGB(serial, 0, 0, 0);
                }

                //Reset the Arduino, if needed
                //(Deprecated and wastes code, this method is never called)
                //(Resetting the Arduino also requires the pins to be reconfigured)
                if (Form1.resetArd == 1) {
                    serial.Write(new byte[1] { 0xFF }, 0, 1);
                    Form1.resetArd = 0;
                }

                //If it's 10 seconds, send dat redo serial
                if (DateTime.Now - StartTime == TimeSpan.FromSeconds(10) && ReadyRedo) {
                    for (int i = 0; i <= 2; i++) {
                        Console.WriteLine("Configuring pin {0}", initpins[i]);
                        var initmsg = new byte[3];
                        initmsg[0] = 0xF4;
                        initmsg[1] = initpins[i];
                        initmsg[2] = 0x03;
                        serial.Write(initmsg, 0, 3);
                        Thread.Sleep(50);
                        Console.WriteLine("Timed Serial Redo");
                    }
                    ReadyRedo = false; //We do this because the code executes multiple times a second, so by setting this we disable this block from executing ever again.
                }

                //Save the settings to the config
                if (Form1.saveToFile == 1) {
                    Properties.Settings.Default.ColourR = Form1.chosencolor[0];
                    Properties.Settings.Default.ColourG = Form1.chosencolor[1];
                    Properties.Settings.Default.ColourB = Form1.chosencolor[2];
                    Properties.Settings.Default.Enable = Form1.enable;
                    Properties.Settings.Default.Mode = Form1.mode;
                    Properties.Settings.Default.Freq = Form1.freq;
                    Properties.Settings.Default.Save();
                    Console.WriteLine("Saved!");
                    Form1.saveToFile = 0;
                }
                Thread.Sleep(110 - Form1.freq * 5);

            }
        }

        //Converts the int's into serial bytes and sends then to the arduino
        static void setRGB(SerialPort ser, int r, int g, int b) {
            var pins = new int[3];
            var color = new int[3];
            pins[0] = 11;
            pins[1] = 6;
            pins[2] = 3;
            color[0] = r;
            color[1] = g;
            color[2] = b;
            for (int i = 0; i <= 2; i++) {
                var msg = new byte[3];
                msg[0] = (byte)(0xE0 | (pins[i] & 0x0F));
                msg[1] = (byte)(color[i] & 0x7F);
                msg[2] = (byte)(color[i] >> 7);
                if (ser.IsOpen) { ser.Write(msg, 0, 3); }
            }
        }
        public static void closeApp() {
            Environment.Exit(1);
        }
    }
}