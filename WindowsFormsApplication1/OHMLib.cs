using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Management;
using OpenHardwareMonitor;
using OpenHardwareMonitor.Collections;
using OpenHardwareMonitor.Hardware;

// REQUIRES OpenHardwareMonitorLib.dll added to project

namespace WindowsFormsApplication1
{
    static class OHMLib
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //nothing here...
        }
            }
    /// <summary>
    /// Wrapper For OpenHarwareMonitor.dll
    /// </summary>
    public class OHData
    {
        // DATA ACCESSOR
        public List<OHMitem> DataList
        {
            get
            {
                return ReportItems;
            }
            set
            {

            }

        }
        // UPDATE METHOD
        public void Update()
        {
            UpdateOHM();
        }

        // for report compilation
        public class OHMitem
        {
            public OHMitem()
            {
            }
            public string name
            {
                get
                {
                    return Name;
                }
                set
                {
                    Name = value;
                }
            }
            public string type
            {
                get
                {
                    return OHMType;
                }
                set
                {
                    OHMType = value;
                }
            }
            public string reading
            {
                get
                {
                    return OHMValue;
                }
                set
                {
                    OHMValue = value;
                }
            }

            private string Name = String.Empty;
            private string OHMType = String.Empty;
            private string OHMValue = String.Empty;

        }
        // for report compilation
        private List<OHMitem> ReportItems = new List<OHMitem>();
        // ADDS ITEMS TO REPORT
        private void AddReportItem(string ARIName, string ARIType, string ARIValue)
        {
            // int readingwidth = 26;
            if (ARIType == "Data")
            {
                return; // ignore data items
            }
            OHMitem ARItem = new OHMitem();
            ARItem.name = ARIName;
            ARItem.type = ARIType + ": ";
            ARItem.reading = ARIValue;
            if (ARIType == "GpuAti")
            {
                ARItem.type = "Graphics Card";
            }

            if (ARIType == "Temperature")
            {
                try
                {
                    double temp = Convert.ToDouble(ARIValue);
                    //ARItem.reading = ((((9.0 / 5.0) * temp) + 32).ToString("000.0") + " F");
                    //boi quit being american
                    ARItem.reading = temp.ToString();
                }
                catch (FormatException) {
                    
                    return;
                }
            }
            if (ARIType == "Clock")
            {
                try
                {
                    double temp = Convert.ToDouble(ARIValue);
                    if (temp < 1000)
                    {
                        ARItem.reading = (temp.ToString("F1") + " MHZ");
                    }
                    else
                    {
                        temp = temp / 1000;
                        ARItem.reading = (temp.ToString("F1") + " GHZ");
                    }
                }
                catch (FormatException) {
                    return;
                }

            }
            if (ARIType == "Control" || ARIType == "Load")
            {
                try
                {
                    double temp = Convert.ToDouble(ARIValue);
                    ARItem.name = ARIName;
                    ARItem.reading = (temp.ToString("F0") + " %");
                }
                catch(FormatException)
                {
                    return;
                }
            }
            if (ARIType == "Voltage")
            {
                try
                {
                    double temp = Convert.ToDouble(ARIValue);
                    ARItem.name = ARIName;
                    ARItem.reading = (temp.ToString("F1") + " V");
                }
                catch (FormatException) {
                    return;
                }
            }
            // 07-28-2016 Added This item
            if (ARIType == "Fan")
            {
                try
                {
                    double rpm = Convert.ToDouble(ARIValue);
                    ARItem.name = ARIName;
                    ARItem.reading = (rpm.ToString("F0") + " RPM");
                }
                catch (FormatException) {
                    return;
                }
            }



            ReportItems.Add(ARItem);
        }
        // LOCAL INSTANCE OHM
        private OpenHardwareMonitor.Hardware.Computer computerHardware = new OpenHardwareMonitor.Hardware.Computer();
        // UPDATE OHM DATA
        private void UpdateOHM()
        {
            
            string s = String.Empty;
            string name = string.Empty;
            string type = string.Empty;
            string value = string.Empty;
            int x, y, z, n;
            int hardwareCount;
            int subcount;
            int sensorcount;
            ReportItems.Clear();
            computerHardware.MainboardEnabled = true;
            computerHardware.FanControllerEnabled = true;
            computerHardware.CPUEnabled = true;
            computerHardware.GPUEnabled = true;
            computerHardware.RAMEnabled = true;
            computerHardware.HDDEnabled = true;
            computerHardware.Open();
            hardwareCount = computerHardware.Hardware.Count();
            for (x = 0; x < hardwareCount; x++)
            {
                name = computerHardware.Hardware[x].Name;
                type = computerHardware.Hardware[x].HardwareType.ToString();
                value = ""; // no value for non-sensors;
                AddReportItem(name, type, value);

                subcount = computerHardware.Hardware[x].SubHardware.Count();

                // ADDED 07-28-2016
                // NEED Update to view Subhardware
                for (y = 0; y < subcount; y++)
                {
                    computerHardware.Hardware[x].SubHardware[y].Update();
                }
                //
               
                if (subcount > 0)
                {
                    for (y = 0; y < subcount; y++)
                    {
                        sensorcount = computerHardware.Hardware[x].SubHardware[y].Sensors.Count();
                        name = computerHardware.Hardware[x].SubHardware[y].Name;
                        type = computerHardware.Hardware[x].SubHardware[y].HardwareType.ToString();
                        value = "";
                        AddReportItem(name, type, value);

                        if (sensorcount > 0)
                        {
                            
                            for (z = 0; z < sensorcount; z++)
                            {
                                
                                name = computerHardware.Hardware[x].SubHardware[y].Sensors[z].Name;
                                type = computerHardware.Hardware[x].SubHardware[y].Sensors[z].SensorType.ToString();
                                value = computerHardware.Hardware[x].SubHardware[y].Sensors[z].Value.ToString();
                                AddReportItem(name, type, value);
                                
                            }
                        }

                    }
                }
                sensorcount = computerHardware.Hardware[x].Sensors.Count();
                
                if (sensorcount > 0)
                {
                    for (z = 0; z < sensorcount; z++)
                    {
                        name = computerHardware.Hardware[x].Sensors[z].Name;
                        type = computerHardware.Hardware[x].Sensors[z].SensorType.ToString();
                        value = computerHardware.Hardware[x].Sensors[z].Value.ToString();
                        AddReportItem(name, type, value);
                        
                    }
                }

            }
           computerHardware.Close();
        }
    }
    /// <summary>
    /// Wrapper for BIOS Information from Win_32BIOS WMI
    /// </summary>
    public class WMIBIOS
    {
        public string Name
        {
            get
            {
                return name;
            }
           
        }
        public string Manufacturer
        {
            get
            {
                return manufacturer;
            }
        }
        public string Date
        {
            get
            {
                return FormatDate(date);
            }

        }
        public string Version
        {
            get
            {
                return version;
            }

        }
        public void Update()
        {
            update();
        }
        private string name = String.Empty;
        private string manufacturer = String.Empty;
        private string date = String.Empty;
        private string version = String.Empty;
        // Get BIOS Data using WMI
        private void update()
        {
            
            
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"\\.\root\cimv2", "SELECT * FROM Win32_BIOS");
            ManagementObjectCollection collection = searcher.Get();
            foreach (ManagementObject o in collection)
            {
                try
                {
                    name = Convert.ToString(o.GetPropertyValue("Name"));
                }
                catch
                {
                    name = String.Empty;
                }
                try
                {
                    date = Convert.ToString(o.GetPropertyValue("ReleaseDate"));
                }
                catch
                {
                    date = String.Empty;
                }
                try
                {
                    manufacturer = Convert.ToString(o.GetPropertyValue("Manufacturer"));
                }
                catch
                {
                    manufacturer = String.Empty;
                }
                try
                {
                    version = Convert.ToString(o.GetPropertyValue("SMBIOSBIOSVersion"));
                }
                catch
                {
                    version = String.Empty;
                }

            }
            searcher.Dispose();
            return;
        }
        // FORMAT DATE FROM WIN_32 BIOS INTO USABLE FORM
        private string FormatDate(string rawdata)
        {
            string result = String.Empty;
            string year = String.Empty;
            string month = String.Empty;
            string day = String.Empty;
            try
            {
                year = rawdata.Substring(0, 4);
                month = rawdata.Substring(4, 2);
                day = rawdata.Substring(6, 2);
            }
            catch
            {
                return result;
            }
            result = month + "-" + day + "-" + year;
            return result;


        }

    }
}
        
    


        
    
