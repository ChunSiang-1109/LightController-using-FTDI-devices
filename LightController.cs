using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using FTD2XX_NET;
using System.IO.Ports;

using System.Timers;
using System.Threading;

using System.IO;
using System.Reflection.Emit;
using System.Threading.Tasks;
namespace Light_Controller
{
    public partial class LightController : Form
    {
        UInt32 ftdiDeviceCount = 0;
        FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

        // Create new instance of the FTDI device class
        FTDI myFtdiDevice = new FTDI();

        static SerialPort myPort;

        bool Calibrator_Flag = false;

        bool Trigger_Flag = false;

        int Channel_Select = 0;
        int Mode_Select = 1;
        int Edge_Select = 1;
        int Strobe_Select = 0;
        private string ftdiSerialNumber;
        public LightController()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string message;

            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);

            // Check status
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                DeviceMessage.Items.Add("Number of FTDI devices: " + ftdiDeviceCount.ToString());
            }
            else
            {
                DeviceMessage.Items.Add("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                return;
            }

            // If no devices available, return
            if (ftdiDeviceCount == 0)
            {
                DeviceMessage.Items.Add("Failed to get number of devices (error " + ftStatus.ToString() + ")");
                return;
            }

            // Allocate storage for device info list
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

            // Populate our device list
            ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);

            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                for (UInt32 i = 0; i < ftdiDeviceCount; i++)
                {
                    DeviceMessage.Items.Add("Device Index: " + i.ToString());
                    DeviceMessage.Items.Add("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags));
                    DeviceMessage.Items.Add("Type: " + ftdiDeviceList[i].Type.ToString());
                    DeviceMessage.Items.Add("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID));
                    DeviceMessage.Items.Add("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId));
                    DeviceMessage.Items.Add("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString());
                    DeviceMessage.Items.Add("Description: " + ftdiDeviceList[i].Description.ToString());

                    ftdiSerialNumber = ftdiDeviceList[i].SerialNumber.ToString();
                }
            }

            // Open first device in our list by serial number
            ftStatus = myFtdiDevice.OpenBySerialNumber(ftdiDeviceList[0].SerialNumber);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                DeviceMessage.Items.Add("Failed to open device (error " + ftStatus.ToString() + ")");
                return;
            }

            string cport;
            myFtdiDevice.GetCOMPort(out cport);

            DeviceMessage.Items.Add(cport);

            // Close
            myFtdiDevice.Close();



            myPort = new SerialPort(cport);

            myPort.BaudRate = 115200;
            myPort.Parity = Parity.None;
            myPort.StopBits = StopBits.One;
            myPort.DataBits = 8;
            myPort.Handshake = Handshake.None;
            myPort.RtsEnable = true;

            // Set the read/write timeouts
            myPort.ReadTimeout = 500;
            myPort.WriteTimeout = 500;

            if (myPort.IsOpen == false) //if not open, open the port
                myPort.Open();




            myPort.WriteLine("LV\r\n");



            message = myPort.ReadLine();



            //if ((String.Compare(readData, 0, "Eleconic Vision Calibrator Board", 0, 32, true)) == 0)
            //if ((String.Compare(message, 0, "LV8USB", 0, 6, true)) == 0)
            if ((String.Compare(message, 0, "LV8CV2", 0, 6, true)) == 0)
            {
                Calibrator_Flag = true;
                btnConnect.Text = "Exit";

                //MessageBox.Show("Welcome");

                txtSerialNum.Enabled = true;
                lblSerialNum.Enabled = true;
                grpLED_Controller.Enabled = true;
                Recipe.Enabled=true;
                btnLoad.Enabled = true;
                txtSerialNum.Text = ftdiSerialNumber;

                // Load intensity settings for this specific device
                //LoadSettingsForCurrentDevice();

            }
            else
            {
                Calibrator_Flag = false;
                btnConnect.Text = "Connect";

                //MessageBox.Show("OMG");
            }

            myPort.WriteLine("EY\r\n");
            message = myPort.ReadLine();
            


            DeviceMessage.Items.Add("LV " + message);

            myPort.WriteLine("ST0\r\n");
            message = myPort.ReadLine();
            DeviceMessage.Items.Add("ST0 " + message);


            myPort.WriteLine("US0,0\r\n");
            myPort.WriteLine("US1,0\r\n");
            myPort.WriteLine("US2,0\r\n");
            myPort.WriteLine("US3,0\r\n");

            myPort.WriteLine("US4,0\r\n");
            myPort.WriteLine("US5,0\r\n");
            myPort.WriteLine("US6,0\r\n");
            myPort.WriteLine("US7,0\r\n");



        }

        private void checkBox0_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox0.Checked)
            {
                myPort.WriteLine("US0,1\r\n");
            }
            else
            {
                myPort.WriteLine("US0,0\r\n");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                myPort.WriteLine("US1,1\r\n"); // Changed from US0 to US1
            }
            else
            {
                myPort.WriteLine("US1,0\r\n"); // Changed from US0 to US1
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                myPort.WriteLine("US2,1\r\n"); // Changed from US0 to US2
            }
            else
            {
                myPort.WriteLine("US2,0\r\n"); // Changed from US0 to US2
            }
        }

        // Continue this pattern for checkBox3 to checkBox7...
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                myPort.WriteLine("US3,1\r\n");
            }
            else
            {
                myPort.WriteLine("US3,0\r\n");
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                myPort.WriteLine("US4,1\r\n");
            }
            else
            {
                myPort.WriteLine("US4,0\r\n");
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                myPort.WriteLine("US5,1\r\n");
            }
            else
            {
                myPort.WriteLine("US5,0\r\n");
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                myPort.WriteLine("US6,1\r\n");
            }
            else
            {
                myPort.WriteLine("US6,0\r\n");
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                myPort.WriteLine("US7,1\r\n");
            }
            else
            {
                myPort.WriteLine("US7,0\r\n");
            }
        }



        private void hScrollBar0_Scroll(object sender, ScrollEventArgs e)
        {
            HandleScroll(0, hScrollBar0, txtIntensity0);
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            HandleScroll(1, hScrollBar1, txtIntensity1);
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            HandleScroll(2, hScrollBar2, txtIntensity2);
        }

        private void hScrollBar3_Scroll(object sender, ScrollEventArgs e)
        {
            HandleScroll(3, hScrollBar3, txtIntensity3);
        }

        private void hScrollBar4_Scroll(object sender, ScrollEventArgs e)
        {
            HandleScroll(4, hScrollBar4, txtIntensity4);
        }

        private void hScrollBar5_Scroll(object sender, ScrollEventArgs e)
        {
            HandleScroll(5, hScrollBar5, txtIntensity5);
        }

        private void hScrollBar6_Scroll(object sender, ScrollEventArgs e)
        {
            HandleScroll(6, hScrollBar6, txtIntensity6);
        }

        private void hScrollBar7_Scroll(object sender, ScrollEventArgs e)
        {
            HandleScroll(7, hScrollBar7, txtIntensity7);
        }

        private void HandleScroll(int channel, HScrollBar scrollBar, System.Windows.Forms.TextBox txtBox)
        {
            try
            {
                int intensity = scrollBar.Value;
                txtBox.Text = intensity.ToString();

                string cmd = $"RS{channel},{intensity}\r\n";

                // Display command in log
                DeviceMessage.Items.Add(cmd);

                // ✅ Send the command to the connected device
                if (myPort != null && myPort.IsOpen)
                {
                    myPort.WriteLine(cmd);
                }
                else
                {
                    DeviceMessage.Items.Add("⚠️ Port not open. Please connect device first.");
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error adjusting intensity: {ex.Message}");
            }
        }

        private void btnadd0_Click(object sender, EventArgs e)
        {
            AdjustIntensity(0, +1, hScrollBar0, txtIntensity0);
        }

        private void btnminus0_Click(object sender, EventArgs e)
        {
            AdjustIntensity(0, -1, hScrollBar0, txtIntensity0);
        }

        private void btnadd1_Click(object sender, EventArgs e)
        {
            AdjustIntensity(1, +1, hScrollBar1, txtIntensity1);
        }

        private void btnminus1_Click(object sender, EventArgs e)
        {
            AdjustIntensity(1, -1, hScrollBar1, txtIntensity1);
        }

        private void btnadd2_Click(object sender, EventArgs e)
        {
            AdjustIntensity(2, +1, hScrollBar2, txtIntensity2);
        }

        private void btnminus2_Click(object sender, EventArgs e)
        {
            AdjustIntensity(2, -1, hScrollBar2, txtIntensity2);
        }

        private void btnadd3_Click(object sender, EventArgs e)
        {
            AdjustIntensity(3, +1, hScrollBar3, txtIntensity3);
        }

        private void btnminus3_Click(object sender, EventArgs e)
        {
            AdjustIntensity(3, -1, hScrollBar3, txtIntensity3);
        }

        private void btnadd4_Click(object sender, EventArgs e)
        {
            AdjustIntensity(4, +1, hScrollBar4, txtIntensity4);
        }

        private void btnminus4_Click(object sender, EventArgs e)
        {
            AdjustIntensity(4, -1, hScrollBar4, txtIntensity4);
        }

        private void btnadd5_Click(object sender, EventArgs e)
        {
            AdjustIntensity(5, +1, hScrollBar5, txtIntensity5);
        }

        private void btnminus5_Click(object sender, EventArgs e)
        {
            AdjustIntensity(5, -1, hScrollBar5, txtIntensity5);
        }

        private void btnadd6_Click(object sender, EventArgs e)
        {
            AdjustIntensity(6, +1, hScrollBar6, txtIntensity6);
        }

        private void btnminus6_Click(object sender, EventArgs e)
        {
            AdjustIntensity(6, -1, hScrollBar6, txtIntensity6);
        }

        private void btnadd7_Click(object sender, EventArgs e)
        {
            AdjustIntensity(7, +1, hScrollBar7, txtIntensity7);
        }

        private void btnminus7_Click(object sender, EventArgs e)
        {
            AdjustIntensity(7, -1, hScrollBar7, txtIntensity7);
        }
        private void AdjustIntensity(int channel, int step, HScrollBar scrollBar, System.Windows.Forms.TextBox txtBox)
        {
            // 1️⃣ Clamp the new value within the scrollbar’s min/max range
            int newValue = scrollBar.Value + step;
            if (newValue > scrollBar.Maximum)
                newValue = scrollBar.Maximum;
            else if (newValue < scrollBar.Minimum)
                newValue = scrollBar.Minimum;

            // 2️⃣ Update UI elements
            scrollBar.Value = newValue;
            txtBox.Text = newValue.ToString();

            // 3️⃣ Send command to device
            string cmd = $"RS{channel},{newValue}\r\n";
            DeviceMessage.Items.Add(cmd);
            myPort.WriteLine(cmd);
        }

        private void HandleTextChanged(int channel, TextBox textBox, HScrollBar scrollBar)
        {
            if (int.TryParse(textBox.Text, out int value))
            {
                // Clamp to scrollbar range
                if (value < scrollBar.Minimum) value = scrollBar.Minimum;
                if (value > scrollBar.Maximum) value = scrollBar.Maximum;

                // If textbox value exceeds limit, correct it immediately
                if (textBox.Text != value.ToString())
                {
                    int caretPos = textBox.SelectionStart;
                    textBox.Text = value.ToString();
                    textBox.SelectionStart = Math.Min(caretPos, textBox.Text.Length);
                }

                // Avoid recursive updates
                if (scrollBar.Value != value)
                    scrollBar.Value = value;

                // Optional: call your existing scroll logic
                HandleScroll(channel, scrollBar, textBox);
            }
            else if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                // Remove invalid (non-numeric) input
                textBox.Text = scrollBar.Value.ToString();
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        private void txtIntensity0_TextChanged(object sender, EventArgs e)
        {
            HandleTextChanged(0, txtIntensity0, hScrollBar0);
        }

        private void txtIntensity1_TextChanged(object sender, EventArgs e)
        {
            HandleTextChanged(1, txtIntensity1, hScrollBar1);
        }

        private void txtIntensity2_TextChanged(object sender, EventArgs e)
        {
            HandleTextChanged(2, txtIntensity2, hScrollBar2);
        }

        private void txtIntensity3_TextChanged(object sender, EventArgs e)
        {
            HandleTextChanged(3, txtIntensity3, hScrollBar3);
        }

        private void txtIntensity4_TextChanged(object sender, EventArgs e)
        {
            HandleTextChanged(4, txtIntensity4, hScrollBar4);
        }

        private void txtIntensity5_TextChanged(object sender, EventArgs e)
        {
            HandleTextChanged(5, txtIntensity5, hScrollBar5);
        }

        private void txtIntensity6_TextChanged(object sender, EventArgs e)
        {
            HandleTextChanged(6, txtIntensity6, hScrollBar6);
        }

        private void txtIntensity7_TextChanged(object sender, EventArgs e)
        {
            HandleTextChanged(7, txtIntensity7, hScrollBar7);
        }

        [Serializable]
        public class DeviceSettings
        {
            public string SerialNumber { get; set; }
            public int[] ChannelIntensities { get; set; } = new int[8];
            public bool[] ChannelStates { get; set; } = new bool[8];
            public string Remark { get; set; } = "";
            public DateTime LastUpdated { get; set; }

            public DeviceSettings()
            {
                ChannelIntensities = new int[8];
                ChannelStates = new bool[8];
                Remark = "";
                LastUpdated = DateTime.Now;
            }
        }

        [Serializable]
        public class AllDeviceSettings
        {
            public Dictionary<string, DeviceSettings> Devices { get; set; } = new Dictionary<string, DeviceSettings>();
        }

        private string GetSettingsFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "LightController");
            Directory.CreateDirectory(appFolder);
            return Path.Combine(appFolder, "all_device_settings.json");
        }

        private void SaveSettings(string recipeName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("Cannot save settings: No serial number available");
                    return;
                }

                if (string.IsNullOrEmpty(recipeName))
                    recipeName = currentActiveRecipe;
                if (string.IsNullOrEmpty(recipeName))
                    recipeName = "Default";

                string filePath = GetSettingsFilePath();
                var allSettings = new Dictionary<string, DeviceSettings>();

                // Load existing settings if present
                if (File.Exists(filePath))
                    allSettings = LoadAllSettingsSimple();

                string key = $"{txtSerialNum.Text}_{recipeName}";

                var currentSettings = new DeviceSettings
                {
                    SerialNumber = txtSerialNum.Text,
                    ChannelIntensities = new int[8],
                    ChannelStates = new bool[8],
                    LastUpdated = DateTime.Now
                };

                // populate intensities
                currentSettings.ChannelIntensities[0] = hScrollBar0.Value;
                currentSettings.ChannelIntensities[1] = hScrollBar1.Value;
                currentSettings.ChannelIntensities[2] = hScrollBar2.Value;
                currentSettings.ChannelIntensities[3] = hScrollBar3.Value;
                currentSettings.ChannelIntensities[4] = hScrollBar4.Value;
                currentSettings.ChannelIntensities[5] = hScrollBar5.Value;
                currentSettings.ChannelIntensities[6] = hScrollBar6.Value;
                currentSettings.ChannelIntensities[7] = hScrollBar7.Value;

                // populate checkbox states
                currentSettings.ChannelStates[0] = checkBox0.Checked;
                currentSettings.ChannelStates[1] = checkBox1.Checked;
                currentSettings.ChannelStates[2] = checkBox2.Checked;
                currentSettings.ChannelStates[3] = checkBox3.Checked;
                currentSettings.ChannelStates[4] = checkBox4.Checked;
                currentSettings.ChannelStates[5] = checkBox5.Checked;
                currentSettings.ChannelStates[6] = checkBox6.Checked;
                currentSettings.ChannelStates[7] = checkBox7.Checked;

                // 获取对应配方的备注
                currentSettings.Remark = GetRemarkByRecipe(recipeName);

                // Save/update
                allSettings[key] = currentSettings;

                // Write file (simple plain-text format)
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    foreach (var kv in allSettings)
                    {
                        writer.WriteLine("[Device]");
                        writer.WriteLine("Key=" + kv.Key);
                        writer.WriteLine("SerialNumber=" + kv.Value.SerialNumber);
                        writer.WriteLine("LastUpdated=" + kv.Value.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss"));
                        writer.WriteLine("Intensities=" + string.Join(",", kv.Value.ChannelIntensities));
                        writer.WriteLine("CheckboxStates=" + string.Join(",", kv.Value.ChannelStates));
                        writer.WriteLine("Remark=" + kv.Value.Remark); // 保存备注
                        writer.WriteLine();
                    }
                }

                DeviceMessage.Items.Add($"Settings saved for {recipeName} ({txtSerialNum.Text})");
                if (!string.IsNullOrEmpty(currentSettings.Remark))
                {
                    DeviceMessage.Items.Add($"Remark: {currentSettings.Remark}");
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error saving settings: {ex.Message}");
            }
        }

        // 根据配方名称获取对应的备注文本框内容
        private string GetRemarkByRecipe(string recipeName)
        {
            if (recipeName == "R1")
            {
                return remark1.Text;
            }
            else if (recipeName == "R2")
            {
                return remark2.Text;
            }
            else if (recipeName == "R3")
            {
                return remark3.Text;
            }
            else if (recipeName == "R4")
            {
                return remark4.Text;
            }
            else if (recipeName == "R5")
            {
                return remark5.Text;
            }
            else
            {
                return "";
            }
        }



        private Dictionary<string, DeviceSettings> LoadAllSettingsSimple()
        {
            var allSettings = new Dictionary<string, DeviceSettings>();

            try
            {
                string filePath = GetSettingsFilePath();
                if (!File.Exists(filePath))
                    return allSettings;

                string[] lines = File.ReadAllLines(filePath);
                DeviceSettings currentSettings = null;
                string currentKey = "";

                foreach (string line in lines)
                {
                    if (line.StartsWith("[Device]"))
                    {
                        if (currentSettings != null && !string.IsNullOrEmpty(currentKey))
                        {
                            allSettings[currentKey] = currentSettings;
                        }
                        currentSettings = new DeviceSettings();
                        currentKey = "";
                    }
                    else if (line.StartsWith("Key=") && currentSettings != null)
                    {
                        currentKey = line.Substring("Key=".Length);
                    }
                    else if (line.StartsWith("SerialNumber=") && currentSettings != null)
                    {
                        currentSettings.SerialNumber = line.Substring("SerialNumber=".Length);
                    }
                    else if (line.StartsWith("LastUpdated=") && currentSettings != null)
                    {
                        if (DateTime.TryParse(line.Substring("LastUpdated=".Length), out DateTime lastUpdated))
                        {
                            currentSettings.LastUpdated = lastUpdated;
                        }
                    }
                    else if (line.StartsWith("Intensities=") && currentSettings != null)
                    {
                        string intensitiesStr = line.Substring("Intensities=".Length);
                        string[] intensityValues = intensitiesStr.Split(',');

                        for (int i = 0; i < Math.Min(intensityValues.Length, 8); i++)
                        {
                            if (int.TryParse(intensityValues[i], out int intensity))
                            {
                                currentSettings.ChannelIntensities[i] = intensity;
                            }
                        }
                    }
                    else if (line.StartsWith("CheckboxStates=") && currentSettings != null)
                    {
                        string checkboxStr = line.Substring("CheckboxStates=".Length);
                        string[] checkboxValues = checkboxStr.Split(',');

                        for (int i = 0; i < Math.Min(checkboxValues.Length, 8); i++)
                        {
                            if (bool.TryParse(checkboxValues[i], out bool checkboxState))
                            {
                                currentSettings.ChannelStates[i] = checkboxState;
                            }
                        }
                    }
                    else if (line.StartsWith("Remark=") && currentSettings != null)
                    {
                        currentSettings.Remark = line.Substring("Remark=".Length);
                    }
                }

                // Don't forget to add the last device
                if (currentSettings != null && !string.IsNullOrEmpty(currentKey))
                {
                    allSettings[currentKey] = currentSettings;
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading settings: {ex.Message}");
            }

            return allSettings;
        }

        private void LoadSettingsForCurrentDevice()
        {
            try
            {
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No serial number available to load settings.");
                    return;
                }

                var allSettings = LoadAllSettingsSimple();

                if (allSettings.TryGetValue(txtSerialNum.Text, out DeviceSettings settings))
                {
                    // Load intensity values
                    hScrollBar0.Value = settings.ChannelIntensities[0];
                    hScrollBar1.Value = settings.ChannelIntensities[1];
                    hScrollBar2.Value = settings.ChannelIntensities[2];
                    hScrollBar3.Value = settings.ChannelIntensities[3];
                    hScrollBar4.Value = settings.ChannelIntensities[4];
                    hScrollBar5.Value = settings.ChannelIntensities[5];
                    hScrollBar6.Value = settings.ChannelIntensities[6];
                    hScrollBar7.Value = settings.ChannelIntensities[7];

                    // Update text boxes
                    txtIntensity0.Text = settings.ChannelIntensities[0].ToString();
                    txtIntensity1.Text = settings.ChannelIntensities[1].ToString();
                    txtIntensity2.Text = settings.ChannelIntensities[2].ToString();
                    txtIntensity3.Text = settings.ChannelIntensities[3].ToString();
                    txtIntensity4.Text = settings.ChannelIntensities[4].ToString();
                    txtIntensity5.Text = settings.ChannelIntensities[5].ToString();
                    txtIntensity6.Text = settings.ChannelIntensities[6].ToString();
                    txtIntensity7.Text = settings.ChannelIntensities[7].ToString();

                    // Load checkbox states (temporarily disable event handlers to avoid sending commands)
                    DisableCheckboxEvents();

                    checkBox0.Checked = settings.ChannelStates[0];
                    checkBox1.Checked = settings.ChannelStates[1];
                    checkBox2.Checked = settings.ChannelStates[2];
                    checkBox3.Checked = settings.ChannelStates[3];
                    checkBox4.Checked = settings.ChannelStates[4];
                    checkBox5.Checked = settings.ChannelStates[5];
                    checkBox6.Checked = settings.ChannelStates[6];
                    checkBox7.Checked = settings.ChannelStates[7];

                    EnableCheckboxEvents();

                    // Apply to device
                    ApplyIntensitySettingsToDevice(settings);
                    ApplyCheckboxSettingsToDevice(settings);

                    DeviceMessage.Items.Add($"Settings loaded for device: {txtSerialNum.Text}");
                }
                else
                {
                    DeviceMessage.Items.Add($"No saved settings found for device: {txtSerialNum.Text}");
                    SetDefaultValues();
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading settings: {ex.Message}");
                SetDefaultValues();
            }
        }

        private void DisableCheckboxEvents()
        {
            checkBox0.CheckedChanged -= checkBox0_CheckedChanged;
            checkBox1.CheckedChanged -= checkBox1_CheckedChanged;
            checkBox2.CheckedChanged -= checkBox2_CheckedChanged;
            checkBox3.CheckedChanged -= checkBox3_CheckedChanged;
            checkBox4.CheckedChanged -= checkBox4_CheckedChanged;
            checkBox5.CheckedChanged -= checkBox5_CheckedChanged;
            checkBox6.CheckedChanged -= checkBox6_CheckedChanged;
            checkBox7.CheckedChanged -= checkBox7_CheckedChanged;
        }

        private void EnableCheckboxEvents()
        {
            checkBox0.CheckedChanged += checkBox0_CheckedChanged;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;
            checkBox3.CheckedChanged += checkBox3_CheckedChanged;
            checkBox4.CheckedChanged += checkBox4_CheckedChanged;
            checkBox5.CheckedChanged += checkBox5_CheckedChanged;
            checkBox6.CheckedChanged += checkBox6_CheckedChanged;
            checkBox7.CheckedChanged += checkBox7_CheckedChanged;
        }

        private void ApplyCheckboxSettingsToDevice(DeviceSettings settings)
        {
            try
            {
                // Apply checkbox states to device
                for (int i = 0; i < 8; i++)
                {
                    string cmd = $"US{i},{(settings.ChannelStates[i] ? "1" : "0")}\r\n";
                    myPort.WriteLine(cmd);
                    Thread.Sleep(50); // Small delay between commands
                }

                DeviceMessage.Items.Add("Checkbox settings applied to device successfully");
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error applying checkbox settings to device: {ex.Message}");
            }
        }

        private void SetDefaultValues()
        {
            // Set all intensities to 0 and checkboxes to unchecked
            hScrollBar0.Value = 0;
            hScrollBar1.Value = 0;
            hScrollBar2.Value = 0;
            hScrollBar3.Value = 0;
            hScrollBar4.Value = 0;
            hScrollBar5.Value = 0;
            hScrollBar6.Value = 0;
            hScrollBar7.Value = 0;

            txtIntensity0.Text = "0";
            txtIntensity1.Text = "0";
            txtIntensity2.Text = "0";
            txtIntensity3.Text = "0";
            txtIntensity4.Text = "0";
            txtIntensity5.Text = "0";
            txtIntensity6.Text = "0";
            txtIntensity7.Text = "0";

            // Temporarily disable events to set default checkbox states
            DisableCheckboxEvents();

            checkBox0.Checked = false;
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            checkBox4.Checked = false;
            checkBox5.Checked = false;
            checkBox6.Checked = false;
            checkBox7.Checked = false;

            EnableCheckboxEvents();
        }

        private void ApplyIntensitySettingsToDevice(DeviceSettings settings)
        {
            try
            {
                // Apply intensity settings only
                for (int i = 0; i < 8; i++)
                {
                    string intensityCmd = $"RS{i},{settings.ChannelIntensities[i]}\r\n";
                    myPort.WriteLine(intensityCmd);
                    Thread.Sleep(50); // Small delay between commands
                }

                DeviceMessage.Items.Add("Intensity settings applied to device successfully");
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error applying intensity settings to device: {ex.Message}");
            }
        }

        private string currentActiveRecipe = "";

        private void btnR1On_Click(object sender, EventArgs e)
        {
            // Change background color to green
            btnR1On.BackColor = Color.Green;

            // Disable On button
            btnR1On.Enabled = false;

            // Enable Off button
            btnR1Off.Enabled = true;
            btnR1Off.BackColor = SystemColors.Control;
            btnR2Off.BackColor = SystemColors.Control;
            btnR3Off.BackColor = SystemColors.Control;
            btnR4Off.BackColor = SystemColors.Control;
            btnR5Off.BackColor = SystemColors.Control;

            // Disable Load button
            btnL1.Enabled = false;

            // Enable Save button
            btnS1.Enabled = true;

            currentActiveRecipe = "R1";
            DisableOtherRecipes("R1");
        }

        private void btnR1Off_Click(object sender, EventArgs e)
        {
            // Change background color to red
            btnR1Off.BackColor = Color.Red;

            // Disable Off button
            btnR1Off.Enabled = false;

            // Enable On button
            btnR1On.Enabled = true;

            // Enable Load button
            btnL1.Enabled = true;

            // Disable Save button
            btnS1.Enabled = false;
            SetDefaultValues();
            // Reset On button color
            btnR1On.BackColor = SystemColors.Control;
            currentActiveRecipe = "";
            EnableAllRecipes();
        }

        private void btnL1_Click(object sender, EventArgs e)
        {
            try
            {
                currentActiveRecipe = "R1";
                DisableOtherRecipes("R1");
                btnR1Off.BackColor = SystemColors.Control;
                btnR2Off.BackColor = SystemColors.Control;
                btnR3Off.BackColor = SystemColors.Control;
                btnR4Off.BackColor = SystemColors.Control;
                btnR5Off.BackColor = SystemColors.Control;
                // Load the saved data for this device
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot load settings.");
                    return;
                }

                var allSettings = LoadAllSettingsSimple();

                if (allSettings.TryGetValue($"{txtSerialNum.Text}_R1", out DeviceSettings settings))
                {
                    // Apply the loaded intensity settings
                    hScrollBar0.Value = settings.ChannelIntensities[0];
                    hScrollBar1.Value = settings.ChannelIntensities[1];
                    hScrollBar2.Value = settings.ChannelIntensities[2];
                    hScrollBar3.Value = settings.ChannelIntensities[3];
                    hScrollBar4.Value = settings.ChannelIntensities[4];
                    hScrollBar5.Value = settings.ChannelIntensities[5];
                    hScrollBar6.Value = settings.ChannelIntensities[6];
                    hScrollBar7.Value = settings.ChannelIntensities[7];

                    remark1.Text = settings.Remark;
                    // Update text boxes
                    txtIntensity0.Text = settings.ChannelIntensities[0].ToString();
                    txtIntensity1.Text = settings.ChannelIntensities[1].ToString();
                    txtIntensity2.Text = settings.ChannelIntensities[2].ToString();
                    txtIntensity3.Text = settings.ChannelIntensities[3].ToString();
                    txtIntensity4.Text = settings.ChannelIntensities[4].ToString();
                    txtIntensity5.Text = settings.ChannelIntensities[5].ToString();
                    txtIntensity6.Text = settings.ChannelIntensities[6].ToString();
                    txtIntensity7.Text = settings.ChannelIntensities[7].ToString();

                    // Load checkbox states (temporarily disable event handlers to avoid sending commands)
                    DisableCheckboxEvents();

                    checkBox0.Checked = settings.ChannelStates[0];
                    checkBox1.Checked = settings.ChannelStates[1];
                    checkBox2.Checked = settings.ChannelStates[2];
                    checkBox3.Checked = settings.ChannelStates[3];
                    checkBox4.Checked = settings.ChannelStates[4];
                    checkBox5.Checked = settings.ChannelStates[5];
                    checkBox6.Checked = settings.ChannelStates[6];
                    checkBox7.Checked = settings.ChannelStates[7];

                    EnableCheckboxEvents();

                    // Apply to device
                    ApplyIntensitySettingsToDevice(settings);
                    ApplyCheckboxSettingsToDevice(settings);

                    // Automatically set to On status as requested
                    btnR1On.BackColor = Color.Green;
                    btnR1On.Enabled = false;
                    btnR1Off.Enabled = true;
                    btnR1Off.BackColor = SystemColors.Control;
                    btnL1.Enabled = false;
                    btnS1.Enabled = true;

                    DeviceMessage.Items.Add($"R1 settings loaded for device: {txtSerialNum.Text}");

                    // Visual feedback for successful load
                    btnL1.BackColor = Color.LightGreen;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL1.InvokeRequired)
                        {
                            btnL1.Invoke(new Action(() => btnL1.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL1.BackColor = SystemColors.Control;
                        }
                    });
                }
                else
                {
                    DeviceMessage.Items.Add($"No saved R1 settings found for device: {txtSerialNum.Text}");
                    remark1.Text = "";
                    // Visual feedback for no settings found
                    btnL1.BackColor = Color.LightYellow;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL1.InvokeRequired)
                        {
                            btnL1.Invoke(new Action(() => btnL1.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL1.BackColor = SystemColors.Control;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading R1 settings: {ex.Message}");

                // Visual feedback for error
                btnL1.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnL1.InvokeRequired)
                    {
                        btnL1.Invoke(new Action(() => btnL1.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnL1.BackColor = SystemColors.Control;
                    }
                });
            }
        }

        private void btnS1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot save settings.");
                    return;
                }
                // Optional: Show a brief message
                string savedMessage = $"Settings saved for {txtSerialNum.Text}";
                DeviceMessage.Items.Add(savedMessage);

                // Show current intensity values in message
                string intensities = $"Intensities: {hScrollBar0.Value}, {hScrollBar1.Value}, {hScrollBar2.Value}, {hScrollBar3.Value}, {hScrollBar4.Value}, {hScrollBar5.Value}, {hScrollBar6.Value}, {hScrollBar7.Value}";
                DeviceMessage.Items.Add(intensities);

                // Show current checkbox states in message
                string checkboxStates = $"Channels ON: {checkBox0.Checked}, {checkBox1.Checked}, {checkBox2.Checked}, {checkBox3.Checked}, {checkBox4.Checked}, {checkBox5.Checked}, {checkBox6.Checked}, {checkBox7.Checked}";
                DeviceMessage.Items.Add(checkboxStates);

                // Save current intensity settings and checkbox states for R1
                SaveSettings("R1");

                DeviceMessage.Items.Add($"R1 settings saved for device: {txtSerialNum.Text}");

                // Update button states as requested
                btnR1Off.BackColor = Color.Red;
                btnR1Off.Enabled = false;
                SetDefaultValues();
                btnR1On.BackColor = SystemColors.Control;
                btnR1On.Enabled = true;
                btnR1Off.BackColor = SystemColors.Control;
                btnL1.Enabled = true;
                btnS1.Enabled = false;
                currentActiveRecipe = "";
                EnableAllRecipes();

                // Visual feedback that save was successful - btnS1 becomes blue
                btnS1.BackColor = Color.LightBlue;

                

                // Reset btnS1 color after a short delay (keep other button states as they are)
                Task.Delay(1500).ContinueWith(t =>
                {
                    if (btnS1.InvokeRequired)
                    {
                        btnS1.Invoke(new Action(() => btnS1.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS1.BackColor = SystemColors.Control;
                    }
                });
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error saving R1 settings: {ex.Message}");

                // Visual feedback for error - btnS1 becomes red temporarily
                btnS1.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnS1.InvokeRequired)
                    {
                        btnS1.Invoke(new Action(() => btnS1.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS1.BackColor = SystemColors.Control;
                    }
                });
            }
        }
        private void btnR2On_Click(object sender, EventArgs e)
        {
            // Change background color to green
            btnR2On.BackColor = Color.Green;

            // Disable On button
            btnR2On.Enabled = false;

            // Enable Off button
            btnR2Off.Enabled = true;
            btnR1Off.BackColor = SystemColors.Control;
            btnR2Off.BackColor = SystemColors.Control;
            btnR3Off.BackColor = SystemColors.Control;
            btnR4Off.BackColor = SystemColors.Control;
            btnR5Off.BackColor = SystemColors.Control;
            // Disable Load button
            btnL2.Enabled = false;

            // Enable Save button
            btnS2.Enabled = true;
            currentActiveRecipe = "R2";
            DisableOtherRecipes("R2");
        }

        private void btnR2Off_Click(object sender, EventArgs e)
        {
            // Change background color to red
            btnR2Off.BackColor = Color.Red;

            // Disable Off button
            btnR2Off.Enabled = false;

            // Enable On button
            btnR2On.Enabled = true;

            // Enable Load button
            btnL2.Enabled = true;

            // Disable Save button
            btnS2.Enabled = false;
            SetDefaultValues();
            // Reset On button color
            btnR2On.BackColor = SystemColors.Control;
            currentActiveRecipe = "";
            EnableAllRecipes();
        }

        private void btnL2_Click(object sender, EventArgs e)
        {
            try
            {
                currentActiveRecipe = "R2";
                DisableOtherRecipes("R2");
                btnR1Off.BackColor = SystemColors.Control;
                btnR2Off.BackColor = SystemColors.Control;
                btnR3Off.BackColor = SystemColors.Control;
                btnR4Off.BackColor = SystemColors.Control;
                btnR5Off.BackColor = SystemColors.Control;
                // Load the saved data for this device
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot load settings.");
                    return;
                }

                var allSettings = LoadAllSettingsSimple();

                if (allSettings.TryGetValue($"{txtSerialNum.Text}_R2", out DeviceSettings settings))
                {
                    // Apply the loaded intensity settings
                    hScrollBar0.Value = settings.ChannelIntensities[0];
                    hScrollBar1.Value = settings.ChannelIntensities[1];
                    hScrollBar2.Value = settings.ChannelIntensities[2];
                    hScrollBar3.Value = settings.ChannelIntensities[3];
                    hScrollBar4.Value = settings.ChannelIntensities[4];
                    hScrollBar5.Value = settings.ChannelIntensities[5];
                    hScrollBar6.Value = settings.ChannelIntensities[6];
                    hScrollBar7.Value = settings.ChannelIntensities[7];

                    remark2.Text = settings.Remark;
                    // Update text boxes
                    txtIntensity0.Text = settings.ChannelIntensities[0].ToString();
                    txtIntensity1.Text = settings.ChannelIntensities[1].ToString();
                    txtIntensity2.Text = settings.ChannelIntensities[2].ToString();
                    txtIntensity3.Text = settings.ChannelIntensities[3].ToString();
                    txtIntensity4.Text = settings.ChannelIntensities[4].ToString();
                    txtIntensity5.Text = settings.ChannelIntensities[5].ToString();
                    txtIntensity6.Text = settings.ChannelIntensities[6].ToString();
                    txtIntensity7.Text = settings.ChannelIntensities[7].ToString();

                    // Load checkbox states (temporarily disable event handlers to avoid sending commands)
                    DisableCheckboxEvents();

                    checkBox0.Checked = settings.ChannelStates[0];
                    checkBox1.Checked = settings.ChannelStates[1];
                    checkBox2.Checked = settings.ChannelStates[2];
                    checkBox3.Checked = settings.ChannelStates[3];
                    checkBox4.Checked = settings.ChannelStates[4];
                    checkBox5.Checked = settings.ChannelStates[5];
                    checkBox6.Checked = settings.ChannelStates[6];
                    checkBox7.Checked = settings.ChannelStates[7];

                    EnableCheckboxEvents();

                    // Apply to device
                    ApplyIntensitySettingsToDevice(settings);
                    ApplyCheckboxSettingsToDevice(settings);

                    // Automatically set to On status as requested
                    btnR2On.BackColor = Color.Green;
                    btnR2On.Enabled = false;
                    btnR2Off.Enabled = true;
                    btnR2Off.BackColor = SystemColors.Control;
                    btnL2.Enabled = false;
                    btnS2.Enabled = true;

                    DeviceMessage.Items.Add($"R2 settings loaded for device: {txtSerialNum.Text}");

                    // Visual feedback for successful load
                    btnL2.BackColor = Color.LightGreen;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL2.InvokeRequired)
                        {
                            btnL2.Invoke(new Action(() => btnL2.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL2.BackColor = SystemColors.Control;
                        }
                    });
                }
                else
                {
                    DeviceMessage.Items.Add($"No saved R2 settings found for device: {txtSerialNum.Text}");
                    remark2.Text = "";
                    // Visual feedback for no settings found
                    btnL2.BackColor = Color.LightYellow;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL2.InvokeRequired)
                        {
                            btnL2.Invoke(new Action(() => btnL2.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL2.BackColor = SystemColors.Control;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading R2 settings: {ex.Message}");

                // Visual feedback for error
                btnL2.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnL2.InvokeRequired)
                    {
                        btnL2.Invoke(new Action(() => btnL2.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnL2.BackColor = SystemColors.Control;
                    }
                });
            }
        }

        private void btnS2_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot save settings.");
                    return;
                }
                // Optional: Show a brief message
                string savedMessage = $"Settings saved for {txtSerialNum.Text}";
                DeviceMessage.Items.Add(savedMessage);

                // Show current intensity values in message
                string intensities = $"Intensities: {hScrollBar0.Value}, {hScrollBar1.Value}, {hScrollBar2.Value}, {hScrollBar3.Value}, {hScrollBar4.Value}, {hScrollBar5.Value}, {hScrollBar6.Value}, {hScrollBar7.Value}";
                DeviceMessage.Items.Add(intensities);

                // Show current checkbox states in message
                string checkboxStates = $"Channels ON: {checkBox0.Checked}, {checkBox1.Checked}, {checkBox2.Checked}, {checkBox3.Checked}, {checkBox4.Checked}, {checkBox5.Checked}, {checkBox6.Checked}, {checkBox7.Checked}";
                DeviceMessage.Items.Add(checkboxStates);

                // Save current intensity settings and checkbox states for R1
                SaveSettings("R2");

                DeviceMessage.Items.Add($"R1 settings saved for device: {txtSerialNum.Text}");

                // Update button states as requested
                btnR2Off.BackColor = Color.Red;
                btnR2Off.Enabled = false;
                SetDefaultValues();
                btnR2On.BackColor = SystemColors.Control;
                btnR2On.Enabled = true;
                btnR2Off.BackColor = SystemColors.Control;
                btnL2.Enabled = true;
                btnS2.Enabled = false;
                currentActiveRecipe = "";
                EnableAllRecipes();

                // Visual feedback that save was successful - btnS1 becomes blue
                btnS2.BackColor = Color.LightBlue;

                

                // Reset btnS2 color after a short delay (keep other button states as they are)
                Task.Delay(1500).ContinueWith(t =>
                {
                    if (btnS2.InvokeRequired)
                    {
                        btnS2.Invoke(new Action(() => btnS2.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS2.BackColor = SystemColors.Control;
                    }
                });
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error saving R2 settings: {ex.Message}");

                // Visual feedback for error - btnS1 becomes red temporarily
                btnS2.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnS2.InvokeRequired)
                    {
                        btnS2.Invoke(new Action(() => btnS2.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS2.BackColor = SystemColors.Control;
                    }
                });
            }
        }
        private void btnR3On_Click(object sender, EventArgs e)
        {
            // Change background color to green
            btnR3On.BackColor = Color.Green;

            // Disable On button
            btnR3On.Enabled = false;

            // Enable Off button
            btnR3Off.Enabled = true;
            btnR1Off.BackColor = SystemColors.Control;
            btnR2Off.BackColor = SystemColors.Control;
            btnR3Off.BackColor = SystemColors.Control;
            btnR4Off.BackColor = SystemColors.Control;
            btnR5Off.BackColor = SystemColors.Control;
            // Disable Load button
            btnL3.Enabled = false;

            // Enable Save button
            btnS3.Enabled = true;
            currentActiveRecipe = "R3";
            DisableOtherRecipes("R3");
        }

        private void btnR3Off_Click(object sender, EventArgs e)
        {
            // Change background color to red
            btnR3Off.BackColor = Color.Red;

            // Disable Off button
            btnR3Off.Enabled = false;

            // Enable On button
            btnR3On.Enabled = true;

            // Enable Load button
            btnL3.Enabled = true;

            // Disable Save button
            btnS3.Enabled = false;
            SetDefaultValues();
            // Reset On button color
            btnR3On.BackColor = SystemColors.Control;
            currentActiveRecipe = "";
            EnableAllRecipes();
        }

        private void btnL3_Click(object sender, EventArgs e)
        {
            try
            {
                currentActiveRecipe = "R3";
                DisableOtherRecipes("R3");
                btnR1Off.BackColor = SystemColors.Control;
                btnR2Off.BackColor = SystemColors.Control;
                btnR3Off.BackColor = SystemColors.Control;
                btnR4Off.BackColor = SystemColors.Control;
                btnR5Off.BackColor = SystemColors.Control;
                // Load the saved data for this device
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot load settings.");
                    return;
                }

                var allSettings = LoadAllSettingsSimple();

                if (allSettings.TryGetValue($"{txtSerialNum.Text}_R3", out DeviceSettings settings))
                {
                    // Apply the loaded intensity settings
                    hScrollBar0.Value = settings.ChannelIntensities[0];
                    hScrollBar1.Value = settings.ChannelIntensities[1];
                    hScrollBar2.Value = settings.ChannelIntensities[2];
                    hScrollBar3.Value = settings.ChannelIntensities[3];
                    hScrollBar4.Value = settings.ChannelIntensities[4];
                    hScrollBar5.Value = settings.ChannelIntensities[5];
                    hScrollBar6.Value = settings.ChannelIntensities[6];
                    hScrollBar7.Value = settings.ChannelIntensities[7];

                    remark3.Text = settings.Remark;
                    // Update text boxes
                    txtIntensity0.Text = settings.ChannelIntensities[0].ToString();
                    txtIntensity1.Text = settings.ChannelIntensities[1].ToString();
                    txtIntensity2.Text = settings.ChannelIntensities[2].ToString();
                    txtIntensity3.Text = settings.ChannelIntensities[3].ToString();
                    txtIntensity4.Text = settings.ChannelIntensities[4].ToString();
                    txtIntensity5.Text = settings.ChannelIntensities[5].ToString();
                    txtIntensity6.Text = settings.ChannelIntensities[6].ToString();
                    txtIntensity7.Text = settings.ChannelIntensities[7].ToString();

                    // Load checkbox states (temporarily disable event handlers to avoid sending commands)
                    DisableCheckboxEvents();

                    checkBox0.Checked = settings.ChannelStates[0];
                    checkBox1.Checked = settings.ChannelStates[1];
                    checkBox2.Checked = settings.ChannelStates[2];
                    checkBox3.Checked = settings.ChannelStates[3];
                    checkBox4.Checked = settings.ChannelStates[4];
                    checkBox5.Checked = settings.ChannelStates[5];
                    checkBox6.Checked = settings.ChannelStates[6];
                    checkBox7.Checked = settings.ChannelStates[7];

                    EnableCheckboxEvents();

                    // Apply to device
                    ApplyIntensitySettingsToDevice(settings);
                    ApplyCheckboxSettingsToDevice(settings);

                    // Automatically set to On status as requested
                    btnR3On.BackColor = Color.Green;
                    btnR3On.Enabled = false;
                    btnR3Off.Enabled = true;
                    btnR3Off.BackColor = SystemColors.Control;
                    btnL3.Enabled = false;
                    btnS3.Enabled = true;

                    DeviceMessage.Items.Add($"R3 settings loaded for device: {txtSerialNum.Text}");

                    // Visual feedback for successful load
                    btnL3.BackColor = Color.LightGreen;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL3.InvokeRequired)
                        {
                            btnL3.Invoke(new Action(() => btnL3.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL3.BackColor = SystemColors.Control;
                        }
                    });
                }
                else
                {
                    DeviceMessage.Items.Add($"No saved R3 settings found for device: {txtSerialNum.Text}");
                    remark3.Text = "";
                    // Visual feedback for no settings found
                    btnL3.BackColor = Color.LightYellow;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL3.InvokeRequired)
                        {
                            btnL3.Invoke(new Action(() => btnL3.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL3.BackColor = SystemColors.Control;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading R3 settings: {ex.Message}");

                // Visual feedback for error
                btnL3.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnL3.InvokeRequired)
                    {
                        btnL3.Invoke(new Action(() => btnL3.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnL3.BackColor = SystemColors.Control;
                    }
                });
            }
        }

        private void btnS3_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot save settings.");
                    return;
                }

                // Optional: Show a brief message
                string savedMessage = $"Settings saved for {txtSerialNum.Text}";
                DeviceMessage.Items.Add(savedMessage);

                // Show current intensity values in message
                string intensities = $"Intensities: {hScrollBar0.Value}, {hScrollBar1.Value}, {hScrollBar2.Value}, {hScrollBar3.Value}, {hScrollBar4.Value}, {hScrollBar5.Value}, {hScrollBar6.Value}, {hScrollBar7.Value}";
                DeviceMessage.Items.Add(intensities);

                // Show current checkbox states in message
                string checkboxStates = $"Channels ON: {checkBox0.Checked}, {checkBox1.Checked}, {checkBox2.Checked}, {checkBox3.Checked}, {checkBox4.Checked}, {checkBox5.Checked}, {checkBox6.Checked}, {checkBox7.Checked}";
                DeviceMessage.Items.Add(checkboxStates);
                // Save current intensity settings and checkbox states for R3
                SaveSettings("R3");

                DeviceMessage.Items.Add($"R3 settings saved for device: {txtSerialNum.Text}");

                // Update button states as requested
                btnR3Off.BackColor = Color.Red;
                btnR3Off.Enabled = false;
                SetDefaultValues();
                btnR3On.BackColor = SystemColors.Control;
                btnR3On.Enabled = true;
                btnR3Off.BackColor = SystemColors.Control;
                btnL3.Enabled = true;
                btnS3.Enabled = false;
                currentActiveRecipe = "";
                EnableAllRecipes();

                // Visual feedback that save was successful - btnS3 becomes blue
                btnS3.BackColor = Color.LightBlue;

                

                // Reset btnS3 color after a short delay (keep other button states as they are)
                Task.Delay(1500).ContinueWith(t =>
                {
                    if (btnS3.InvokeRequired)
                    {
                        btnS3.Invoke(new Action(() => btnS3.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS3.BackColor = SystemColors.Control;
                    }
                });
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error saving R3 settings: {ex.Message}");

                // Visual feedback for error - btnS3 becomes red temporarily
                btnS3.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnS3.InvokeRequired)
                    {
                        btnS3.Invoke(new Action(() => btnS3.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS3.BackColor = SystemColors.Control;
                    }
                });
            }
        }
        private void btnR4On_Click(object sender, EventArgs e)
        {
            // Change background color to green
            btnR4On.BackColor = Color.Green;

            // Disable On button
            btnR4On.Enabled = false;

            // Enable Off button
            btnR4Off.Enabled = true;
            btnR1Off.BackColor = SystemColors.Control;
            btnR2Off.BackColor = SystemColors.Control;
            btnR3Off.BackColor = SystemColors.Control;
            btnR4Off.BackColor = SystemColors.Control;
            btnR5Off.BackColor = SystemColors.Control;
            // Disable Load button
            btnL4.Enabled = false;

            // Enable Save button
            btnS4.Enabled = true;
            currentActiveRecipe = "R4";
            DisableOtherRecipes("R4");
        }

        private void btnR4Off_Click(object sender, EventArgs e)
        {
            // Change background color to red
            btnR4Off.BackColor = Color.Red;

            // Disable Off button
            btnR4Off.Enabled = false;

            // Enable On button
            btnR4On.Enabled = true;

            // Enable Load button
            btnL4.Enabled = true;

            // Disable Save button
            btnS4.Enabled = false;
            SetDefaultValues();
            // Reset On button color
            btnR4On.BackColor = SystemColors.Control;
            currentActiveRecipe = "";
            EnableAllRecipes();
        }

        private void btnL4_Click(object sender, EventArgs e)
        {
            try
            {
                currentActiveRecipe = "R4";
                DisableOtherRecipes("R4");
                btnR1Off.BackColor = SystemColors.Control;
                btnR2Off.BackColor = SystemColors.Control;
                btnR3Off.BackColor = SystemColors.Control;
                btnR4Off.BackColor = SystemColors.Control;
                btnR5Off.BackColor = SystemColors.Control;
                // Load the saved data for this device
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot load settings.");
                    return;
                }

                var allSettings = LoadAllSettingsSimple();

                if (allSettings.TryGetValue($"{txtSerialNum.Text}_R4", out DeviceSettings settings))
                {
                    // Apply the loaded intensity settings
                    hScrollBar0.Value = settings.ChannelIntensities[0];
                    hScrollBar1.Value = settings.ChannelIntensities[1];
                    hScrollBar2.Value = settings.ChannelIntensities[2];
                    hScrollBar3.Value = settings.ChannelIntensities[3];
                    hScrollBar4.Value = settings.ChannelIntensities[4];
                    hScrollBar5.Value = settings.ChannelIntensities[5];
                    hScrollBar6.Value = settings.ChannelIntensities[6];
                    hScrollBar7.Value = settings.ChannelIntensities[7];

                    remark4.Text = settings.Remark;
                    // Update text boxes
                    txtIntensity0.Text = settings.ChannelIntensities[0].ToString();
                    txtIntensity1.Text = settings.ChannelIntensities[1].ToString();
                    txtIntensity2.Text = settings.ChannelIntensities[2].ToString();
                    txtIntensity3.Text = settings.ChannelIntensities[3].ToString();
                    txtIntensity4.Text = settings.ChannelIntensities[4].ToString();
                    txtIntensity5.Text = settings.ChannelIntensities[5].ToString();
                    txtIntensity6.Text = settings.ChannelIntensities[6].ToString();
                    txtIntensity7.Text = settings.ChannelIntensities[7].ToString();

                    // Load checkbox states (temporarily disable event handlers to avoid sending commands)
                    DisableCheckboxEvents();

                    checkBox0.Checked = settings.ChannelStates[0];
                    checkBox1.Checked = settings.ChannelStates[1];
                    checkBox2.Checked = settings.ChannelStates[2];
                    checkBox3.Checked = settings.ChannelStates[3];
                    checkBox4.Checked = settings.ChannelStates[4];
                    checkBox5.Checked = settings.ChannelStates[5];
                    checkBox6.Checked = settings.ChannelStates[6];
                    checkBox7.Checked = settings.ChannelStates[7];

                    EnableCheckboxEvents();

                    // Apply to device
                    ApplyIntensitySettingsToDevice(settings);
                    ApplyCheckboxSettingsToDevice(settings);

                    // Automatically set to On status as requested
                    btnR4On.BackColor = Color.Green;
                    btnR4On.Enabled = false;
                    btnR4Off.Enabled = true;
                    btnR4Off.BackColor = SystemColors.Control;
                    btnL4.Enabled = false;
                    btnS4.Enabled = true;

                    DeviceMessage.Items.Add($"R4 settings loaded for device: {txtSerialNum.Text}");

                    // Visual feedback for successful load
                    btnL4.BackColor = Color.LightGreen;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL4.InvokeRequired)
                        {
                            btnL4.Invoke(new Action(() => btnL4.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL4.BackColor = SystemColors.Control;
                        }
                    });
                }
                else
                {
                    DeviceMessage.Items.Add($"No saved R4 settings found for device: {txtSerialNum.Text}");
                    remark4.Text = "";
                    // Visual feedback for no settings found
                    btnL4.BackColor = Color.LightYellow;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL4.InvokeRequired)
                        {
                            btnL4.Invoke(new Action(() => btnL4.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL4.BackColor = SystemColors.Control;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading R4 settings: {ex.Message}");

                // Visual feedback for error
                btnL4.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnL4.InvokeRequired)
                    {
                        btnL4.Invoke(new Action(() => btnL4.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnL4.BackColor = SystemColors.Control;
                    }
                });
            }
        }

        private void btnS4_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot save settings.");
                    return;
                }
                // Optional: Show a brief message
                string savedMessage = $"Settings saved for {txtSerialNum.Text}";
                DeviceMessage.Items.Add(savedMessage);

                // Show current intensity values in message
                string intensities = $"Intensities: {hScrollBar0.Value}, {hScrollBar1.Value}, {hScrollBar2.Value}, {hScrollBar3.Value}, {hScrollBar4.Value}, {hScrollBar5.Value}, {hScrollBar6.Value}, {hScrollBar7.Value}";
                DeviceMessage.Items.Add(intensities);

                // Show current checkbox states in message
                string checkboxStates = $"Channels ON: {checkBox0.Checked}, {checkBox1.Checked}, {checkBox2.Checked}, {checkBox3.Checked}, {checkBox4.Checked}, {checkBox5.Checked}, {checkBox6.Checked}, {checkBox7.Checked}";
                DeviceMessage.Items.Add(checkboxStates);
                // Save current intensity settings and checkbox states for R4
                SaveSettings("R4");

                DeviceMessage.Items.Add($"R4 settings saved for device: {txtSerialNum.Text}");

                // Update button states as requested
                btnR4Off.BackColor = Color.Red;
                btnR4Off.Enabled = false;
                SetDefaultValues();
                btnR4On.BackColor = SystemColors.Control;
                btnR4On.Enabled = true;
                btnR4Off.BackColor = SystemColors.Control;
                btnL4.Enabled = true;
                btnS4.Enabled = false;
                currentActiveRecipe = "";
                EnableAllRecipes();

                // Visual feedback that save was successful - btnS4 becomes blue
                btnS4.BackColor = Color.LightBlue;

                

                // Reset btnS4 color after a short delay (keep other button states as they are)
                Task.Delay(1500).ContinueWith(t =>
                {
                    if (btnS4.InvokeRequired)
                    {
                        btnS4.Invoke(new Action(() => btnS4.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS4.BackColor = SystemColors.Control;
                    }
                });
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error saving R4 settings: {ex.Message}");

                // Visual feedback for error - btnS4 becomes red temporarily
                btnS4.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnS4.InvokeRequired)
                    {
                        btnS4.Invoke(new Action(() => btnS4.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS4.BackColor = SystemColors.Control;
                    }
                });
            }
        }
        private void btnR5On_Click(object sender, EventArgs e)
        {
            // Change background color to green
            btnR5On.BackColor = Color.Green;

            // Disable On button
            btnR5On.Enabled = false;

            // Enable Off button
            btnR5Off.Enabled = true;
            btnR1Off.BackColor = SystemColors.Control;
            btnR2Off.BackColor = SystemColors.Control;
            btnR3Off.BackColor = SystemColors.Control;
            btnR4Off.BackColor = SystemColors.Control;
            btnR5Off.BackColor = SystemColors.Control;
            // Disable Load button
            btnL5.Enabled = false;

            // Enable Save button
            btnS5.Enabled = true;
            currentActiveRecipe = "R5";
            DisableOtherRecipes("R5");
        }

        private void btnR5Off_Click(object sender, EventArgs e)
        {
            // Change background color to red
            btnR5Off.BackColor = Color.Red;

            // Disable Off button
            btnR5Off.Enabled = false;

            // Enable On button
            btnR5On.Enabled = true;

            // Enable Load button
            btnL5.Enabled = true;

            // Disable Save button
            btnS5.Enabled = false;
            SetDefaultValues();
            // Reset On button color
            btnR5On.BackColor = SystemColors.Control;
            currentActiveRecipe = "";
            EnableAllRecipes();
        }

        private void btnL5_Click(object sender, EventArgs e)
        {
            try
            {
                currentActiveRecipe = "R5";
                DisableOtherRecipes("R5");
                btnR1Off.BackColor = SystemColors.Control;
                btnR2Off.BackColor = SystemColors.Control;
                btnR3Off.BackColor = SystemColors.Control;
                btnR4Off.BackColor = SystemColors.Control;
                btnR5Off.BackColor = SystemColors.Control;
                // Load the saved data for this device
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot load settings.");
                    return;
                }

                var allSettings = LoadAllSettingsSimple();

                if (allSettings.TryGetValue($"{txtSerialNum.Text}_R5", out DeviceSettings settings))
                {
                    // Apply the loaded intensity settings
                    hScrollBar0.Value = settings.ChannelIntensities[0];
                    hScrollBar1.Value = settings.ChannelIntensities[1];
                    hScrollBar2.Value = settings.ChannelIntensities[2];
                    hScrollBar3.Value = settings.ChannelIntensities[3];
                    hScrollBar4.Value = settings.ChannelIntensities[4];
                    hScrollBar5.Value = settings.ChannelIntensities[5];
                    hScrollBar6.Value = settings.ChannelIntensities[6];
                    hScrollBar7.Value = settings.ChannelIntensities[7];

                    remark5.Text = settings.Remark;
                    // Update text boxes
                    txtIntensity0.Text = settings.ChannelIntensities[0].ToString();
                    txtIntensity1.Text = settings.ChannelIntensities[1].ToString();
                    txtIntensity2.Text = settings.ChannelIntensities[2].ToString();
                    txtIntensity3.Text = settings.ChannelIntensities[3].ToString();
                    txtIntensity4.Text = settings.ChannelIntensities[4].ToString();
                    txtIntensity5.Text = settings.ChannelIntensities[5].ToString();
                    txtIntensity6.Text = settings.ChannelIntensities[6].ToString();
                    txtIntensity7.Text = settings.ChannelIntensities[7].ToString();

                    // Load checkbox states (temporarily disable event handlers to avoid sending commands)
                    DisableCheckboxEvents();

                    checkBox0.Checked = settings.ChannelStates[0];
                    checkBox1.Checked = settings.ChannelStates[1];
                    checkBox2.Checked = settings.ChannelStates[2];
                    checkBox3.Checked = settings.ChannelStates[3];
                    checkBox4.Checked = settings.ChannelStates[4];
                    checkBox5.Checked = settings.ChannelStates[5];
                    checkBox6.Checked = settings.ChannelStates[6];
                    checkBox7.Checked = settings.ChannelStates[7];

                    EnableCheckboxEvents();

                    // Apply to device
                    ApplyIntensitySettingsToDevice(settings);
                    ApplyCheckboxSettingsToDevice(settings);

                    // Automatically set to On status as requested
                    btnR5On.BackColor = Color.Green;
                    btnR5On.Enabled = false;
                    btnR5Off.Enabled = true;
                    btnR5Off.BackColor = SystemColors.Control;
                    btnL5.Enabled = false;
                    btnS5.Enabled = true;

                    DeviceMessage.Items.Add($"R5 settings loaded for device: {txtSerialNum.Text}");

                    // Visual feedback for successful load
                    btnL5.BackColor = Color.LightGreen;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL5.InvokeRequired)
                        {
                            btnL5.Invoke(new Action(() => btnL5.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL5.BackColor = SystemColors.Control;
                        }
                    });
                }
                else
                {
                    DeviceMessage.Items.Add($"No saved R5 settings found for device: {txtSerialNum.Text}");
                    remark5.Text = "";
                    // Visual feedback for no settings found
                    btnL5.BackColor = Color.LightYellow;
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        if (btnL5.InvokeRequired)
                        {
                            btnL5.Invoke(new Action(() => btnL5.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnL5.BackColor = SystemColors.Control;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading R5 settings: {ex.Message}");

                // Visual feedback for error
                btnL5.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnL5.InvokeRequired)
                    {
                        btnL5.Invoke(new Action(() => btnL5.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnL5.BackColor = SystemColors.Control;
                    }
                });
            }
        }

        private void btnS5_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot save settings.");
                    return;
                }
                // Optional: Show a brief message
                string savedMessage = $"Settings saved for {txtSerialNum.Text}";
                DeviceMessage.Items.Add(savedMessage);

                // Show current intensity values in message
                string intensities = $"Intensities: {hScrollBar0.Value}, {hScrollBar1.Value}, {hScrollBar2.Value}, {hScrollBar3.Value}, {hScrollBar4.Value}, {hScrollBar5.Value}, {hScrollBar6.Value}, {hScrollBar7.Value}";
                DeviceMessage.Items.Add(intensities);

                // Show current checkbox states in message
                string checkboxStates = $"Channels ON: {checkBox0.Checked}, {checkBox1.Checked}, {checkBox2.Checked}, {checkBox3.Checked}, {checkBox4.Checked}, {checkBox5.Checked}, {checkBox6.Checked}, {checkBox7.Checked}";
                DeviceMessage.Items.Add(checkboxStates);
                // Save current intensity settings and checkbox states for R5
                SaveSettings("R5");

                DeviceMessage.Items.Add($"R5 settings saved for device: {txtSerialNum.Text}");

                // Update button states as requested
                btnR5Off.BackColor = Color.Red;
                btnR5Off.Enabled = false;
                SetDefaultValues();
                btnR5On.BackColor = SystemColors.Control;
                btnR5On.Enabled = true;
                btnR5Off.BackColor = SystemColors.Control;
                btnL5.Enabled = true;
                btnS5.Enabled = false;
                currentActiveRecipe = "";
                EnableAllRecipes();

                // Visual feedback that save was successful - btnS5 becomes blue
                btnS5.BackColor = Color.LightBlue;

                

                // Reset btnS5 color after a short delay (keep other button states as they are)
                Task.Delay(1500).ContinueWith(t =>
                {
                    if (btnS5.InvokeRequired)
                    {
                        btnS5.Invoke(new Action(() => btnS5.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS5.BackColor = SystemColors.Control;
                    }
                });
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error saving R5 settings: {ex.Message}");

                // Visual feedback for error - btnS5 becomes red temporarily
                btnS5.BackColor = Color.LightCoral;
                Task.Delay(1000).ContinueWith(t =>
                {
                    if (btnS5.InvokeRequired)
                    {
                        btnS5.Invoke(new Action(() => btnS5.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnS5.BackColor = SystemColors.Control;
                    }
                });
            }
        }

        private void DisableOtherRecipes(string activeRecipe)
        {
            // Disable all recipe buttons except the active one
            switch (activeRecipe)
            {
                case "R1":
                    SetRecipeEnabledState("R2", false);
                    SetRecipeEnabledState("R3", false);
                    SetRecipeEnabledState("R4", false);
                    SetRecipeEnabledState("R5", false);
                    break;
                case "R2":
                    SetRecipeEnabledState("R1", false);
                    SetRecipeEnabledState("R3", false);
                    SetRecipeEnabledState("R4", false);
                    SetRecipeEnabledState("R5", false);
                    break;
                case "R3":
                    SetRecipeEnabledState("R1", false);
                    SetRecipeEnabledState("R2", false);
                    SetRecipeEnabledState("R4", false);
                    SetRecipeEnabledState("R5", false);
                    break;
                case "R4":
                    SetRecipeEnabledState("R1", false);
                    SetRecipeEnabledState("R2", false);
                    SetRecipeEnabledState("R3", false);
                    SetRecipeEnabledState("R5", false);
                    break;
                case "R5":
                    SetRecipeEnabledState("R1", false);
                    SetRecipeEnabledState("R2", false);
                    SetRecipeEnabledState("R3", false);
                    SetRecipeEnabledState("R4", false);
                    break;
            }

            DeviceMessage.Items.Add($"{activeRecipe} is now active - other recipes disabled");
        }

        private void EnableAllRecipes()
        {
            // Enable all recipe buttons
            SetRecipeEnabledState("R1", true);
            SetRecipeEnabledState("R2", true);
            SetRecipeEnabledState("R3", true);
            SetRecipeEnabledState("R4", true);
            SetRecipeEnabledState("R5", true);

            DeviceMessage.Items.Add("All recipes are now available");
        }

        private void SetRecipeEnabledState(string recipe, bool enabled)
        {
            switch (recipe)
            {
                case "R1":
                    btnR1On.Enabled = enabled;
                    btnR1Off.Enabled = false; ;
                    btnL1.Enabled = enabled;
                    // Don't enable Save button if it should be disabled by default
                    if (!enabled) btnS1.Enabled = false;
                    break;
                case "R2":
                    btnR2On.Enabled = enabled;
                    btnR2Off.Enabled = false;
                    btnL2.Enabled = enabled;
                    if (!enabled) btnS2.Enabled = false;
                    break;
                case "R3":
                    btnR3On.Enabled = enabled;
                    btnR3Off.Enabled = false;
                    btnL3.Enabled = enabled;
                    if (!enabled) btnS3.Enabled = false;
                    break;
                case "R4":
                    btnR4On.Enabled = enabled;
                    btnR4Off.Enabled = false;
                    btnL4.Enabled = enabled;
                    if (!enabled) btnS4.Enabled = false;
                    break;
                case "R5":
                    btnR5On.Enabled = enabled;
                    btnR5Off.Enabled = false;
                    btnL5.Enabled = enabled;
                    if (!enabled) btnS5.Enabled = false;
                    break;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                // 使用 OpenFileDialog 让用户选择要加载的文件
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Light Controller Settings (*.lcs)|*.lcs|All files (*.*)|*.*";
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.Title = "Load Recipe Settings File";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        LoadFromFile(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading recipe file: {ex.Message}");
            }
        }

        // 从文件加载设置
        // 从文件加载设置
        private void LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    DeviceMessage.Items.Add($"File not found: {filePath}");
                    return;
                }

                var allSettings = LoadSettingsFromFile(filePath);

                if (allSettings.Count == 0)
                {
                    DeviceMessage.Items.Add("No valid settings found in the selected file.");
                    return;
                }

                // 读取项目名称并更新txtProject
                string projectName = ExtractProjectNameFromFile(filePath);
                if (!string.IsNullOrEmpty(projectName))
                {
                    txtProject.Text = projectName;
                    DeviceMessage.Items.Add($"Project loaded: {projectName}");
                }

                // 首先显示文件中所有配方的备注
                DisplayAllRemarksFromFile(allSettings, filePath);

                // 创建选择对话框
                using (var form = new Form())
                {
                    form.Text = $"Load Recipe - {Path.GetFileName(filePath)}";
                    form.Size = new Size(450, 350);
                    form.StartPosition = FormStartPosition.CenterParent;

                    // 显示项目名称
                    var projectLabel = new System.Windows.Forms.Label()
                    {
                        Text = $"Project: {projectName}",
                        Left = 20,
                        Top = 10,
                        Width = 400,
                        Font = new Font("Arial", 9, FontStyle.Bold),
                        ForeColor = Color.DarkBlue
                    };

                    var label = new System.Windows.Forms.Label() { Text = "Select recipe to load:", Left = 20, Top = 35, Width = 350 };
                    var comboBox = new ComboBox() { Left = 20, Top = 60, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };

                    // 显示每个配方的详细信息
                    var listView = new ListView() { Left = 20, Top = 90, Width = 400, Height = 150, View = View.Details };
                    listView.Columns.Add("Recipe", 60);
                    listView.Columns.Add("Intensities", 150);
                    listView.Columns.Add("Remark", 180);

                    // 填充可用的配方
                    var recipeSettings = new Dictionary<string, DeviceSettings>();

                    foreach (var kv in allSettings)
                    {
                        if (kv.Key.Contains("_R"))
                        {
                            string recipe = kv.Key.Split('_')[1];
                            recipeSettings[recipe] = kv.Value;

                            // 添加到下拉框
                            if (!comboBox.Items.Contains(recipe))
                            {
                                comboBox.Items.Add(recipe);
                            }

                            // 添加到列表视图显示详细信息
                            var intensities = string.Join(",", kv.Value.ChannelIntensities);
                            var item = new ListViewItem(recipe);
                            item.SubItems.Add(intensities);
                            item.SubItems.Add(kv.Value.Remark);
                            listView.Items.Add(item);
                        }
                    }

                    if (comboBox.Items.Count == 0)
                    {
                        DeviceMessage.Items.Add("No valid recipes found in the selected file.");
                        return;
                    }

                    comboBox.SelectedIndex = 0;

                    // 显示文件信息
                    var fileInfoLabel = new System.Windows.Forms.Label()
                    {
                        Text = $"File: {Path.GetFileName(filePath)} | Recipes: {recipeSettings.Count}",
                        Left = 20,
                        Top = 250,
                        Width = 400,
                        Font = new Font("Arial", 8),
                        ForeColor = Color.Blue
                    };

                    var button = new Button() { Text = "Load Selected", Left = 20, Top = 275, Width = 100 };
                    var cancelButton = new Button() { Text = "Cancel", Left = 130, Top = 275, Width = 80 };

                    button.Click += (s, ev) => { form.DialogResult = DialogResult.OK; form.Close(); };
                    cancelButton.Click += (s, ev) => { form.DialogResult = DialogResult.Cancel; form.Close(); };

                    form.Controls.Add(projectLabel);
                    form.Controls.Add(label);
                    form.Controls.Add(comboBox);
                    form.Controls.Add(listView);
                    form.Controls.Add(fileInfoLabel);
                    form.Controls.Add(button);
                    form.Controls.Add(cancelButton);

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        string selectedRecipe = comboBox.SelectedItem.ToString();
                        if (recipeSettings.ContainsKey(selectedRecipe))
                        {
                            LoadSpecificRecipeSettings(selectedRecipe, recipeSettings[selectedRecipe], filePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading from file: {ex.Message}");
            }
        }

        // 从文件中提取项目名称
        private string ExtractProjectNameFromFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("# Project Name:"))
                    {
                        return line.Substring("# Project Name:".Length).Trim();
                    }
                }

                // 如果没有找到项目名称，使用文件名（不含扩展名）
                return Path.GetFileNameWithoutExtension(filePath);
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error extracting project name: {ex.Message}");
                return Path.GetFileNameWithoutExtension(filePath);
            }
        }

        // 显示文件中所有配方的备注
        private void DisplayAllRemarksFromFile(Dictionary<string, DeviceSettings> allSettings, string filePath)
        {
            try
            {
                DeviceMessage.Items.Add("=== All Remarks from File ===");
                DeviceMessage.Items.Add($"File: {Path.GetFileName(filePath)}");

                string[] recipes = { "R1", "R2", "R3", "R4", "R5" };
                bool hasRemarks = false;

                foreach (string recipe in recipes)
                {
                    string key = $"{txtSerialNum.Text}_{recipe}";

                    // 如果找不到当前设备序列号的设置，尝试匹配任何序列号
                    if (!allSettings.ContainsKey(key))
                    {
                        var matchingKey = allSettings.Keys.FirstOrDefault(k => k.EndsWith($"_{recipe}"));
                        if (matchingKey != null)
                        {
                            key = matchingKey;
                        }
                    }

                    if (allSettings.ContainsKey(key))
                    {
                        var settings = allSettings[key];
                        if (!string.IsNullOrEmpty(settings.Remark))
                        {
                            DeviceMessage.Items.Add($"{recipe}: {settings.Remark}");
                            hasRemarks = true;

                            // 同时更新界面上的备注文本框
                            SetRemarkByRecipe(recipe, settings.Remark);
                        }
                        else
                        {
                            DeviceMessage.Items.Add($"{recipe}: [No remark]");
                        }
                    }
                    else
                    {
                        DeviceMessage.Items.Add($"{recipe}: [Not found in file]");
                    }
                }

                if (!hasRemarks)
                {
                    DeviceMessage.Items.Add("No remarks found in the file.");
                }

                DeviceMessage.Items.Add("=== End Remarks ===");
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error displaying remarks: {ex.Message}");
            }
        }

        // 加载特定配方的设置
        private void LoadSpecificRecipeSettings(string recipeName, DeviceSettings settings, string sourceFile)
        {
            try
            {
                // 应用强度设置
                hScrollBar0.Value = settings.ChannelIntensities[0];
                hScrollBar1.Value = settings.ChannelIntensities[1];
                hScrollBar2.Value = settings.ChannelIntensities[2];
                hScrollBar3.Value = settings.ChannelIntensities[3];
                hScrollBar4.Value = settings.ChannelIntensities[4];
                hScrollBar5.Value = settings.ChannelIntensities[5];
                hScrollBar6.Value = settings.ChannelIntensities[6];
                hScrollBar7.Value = settings.ChannelIntensities[7];

                // 更新文本框
                txtIntensity0.Text = settings.ChannelIntensities[0].ToString();
                txtIntensity1.Text = settings.ChannelIntensities[1].ToString();
                txtIntensity2.Text = settings.ChannelIntensities[2].ToString();
                txtIntensity3.Text = settings.ChannelIntensities[3].ToString();
                txtIntensity4.Text = settings.ChannelIntensities[4].ToString();
                txtIntensity5.Text = settings.ChannelIntensities[5].ToString();
                txtIntensity6.Text = settings.ChannelIntensities[6].ToString();
                txtIntensity7.Text = settings.ChannelIntensities[7].ToString();

                // 加载复选框状态
                DisableCheckboxEvents();
                checkBox0.Checked = settings.ChannelStates[0];
                checkBox1.Checked = settings.ChannelStates[1];
                checkBox2.Checked = settings.ChannelStates[2];
                checkBox3.Checked = settings.ChannelStates[3];
                checkBox4.Checked = settings.ChannelStates[4];
                checkBox5.Checked = settings.ChannelStates[5];
                checkBox6.Checked = settings.ChannelStates[6];
                checkBox7.Checked = settings.ChannelStates[7];
                EnableCheckboxEvents();

                // 加载备注
                SetRemarkByRecipe(recipeName, settings.Remark);

                // 应用到设备
                ApplyIntensitySettingsToDevice(settings);
                ApplyCheckboxSettingsToDevice(settings);

                DeviceMessage.Items.Add($"✓ Loaded {recipeName} from: {Path.GetFileName(sourceFile)}");
                DeviceMessage.Items.Add($"  Intensities: {string.Join(", ", settings.ChannelIntensities)}");
                DeviceMessage.Items.Add($"  Remark: {settings.Remark}");

                // 视觉反馈
                btnLoad.BackColor = Color.LightBlue;
                Task.Delay(1500).ContinueWith(t =>
                {
                    if (btnLoad.InvokeRequired)
                    {
                        btnLoad.Invoke(new Action(() => btnLoad.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnLoad.BackColor = SystemColors.Control;
                    }
                });
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading {recipeName}: {ex.Message}");
            }
        }

        // 从文件加载设置到字典
        private Dictionary<string, DeviceSettings> LoadSettingsFromFile(string filePath)
        {
            var allSettings = new Dictionary<string, DeviceSettings>();

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                DeviceSettings currentSettings = null;
                string currentKey = "";

                foreach (string line in lines)
                {
                    if (line.StartsWith("[Device]"))
                    {
                        if (currentSettings != null && !string.IsNullOrEmpty(currentKey))
                        {
                            allSettings[currentKey] = currentSettings;
                        }
                        currentSettings = new DeviceSettings();
                        currentKey = "";
                    }
                    else if (line.StartsWith("Key=") && currentSettings != null)
                    {
                        currentKey = line.Substring("Key=".Length);
                    }
                    else if (line.StartsWith("SerialNumber=") && currentSettings != null)
                    {
                        currentSettings.SerialNumber = line.Substring("SerialNumber=".Length);
                    }
                    else if (line.StartsWith("LastUpdated=") && currentSettings != null)
                    {
                        if (DateTime.TryParse(line.Substring("LastUpdated=".Length), out DateTime lastUpdated))
                        {
                            currentSettings.LastUpdated = lastUpdated;
                        }
                    }
                    else if (line.StartsWith("Intensities=") && currentSettings != null)
                    {
                        string intensitiesStr = line.Substring("Intensities=".Length);
                        string[] intensityValues = intensitiesStr.Split(',');

                        for (int i = 0; i < Math.Min(intensityValues.Length, 8); i++)
                        {
                            if (int.TryParse(intensityValues[i], out int intensity))
                            {
                                currentSettings.ChannelIntensities[i] = intensity;
                            }
                        }
                    }
                    else if (line.StartsWith("CheckboxStates=") && currentSettings != null)
                    {
                        string checkboxStr = line.Substring("CheckboxStates=".Length);
                        string[] checkboxValues = checkboxStr.Split(',');

                        for (int i = 0; i < Math.Min(checkboxValues.Length, 8); i++)
                        {
                            if (bool.TryParse(checkboxValues[i], out bool checkboxState))
                            {
                                currentSettings.ChannelStates[i] = checkboxState;
                            }
                        }
                    }
                    else if (line.StartsWith("Remark=") && currentSettings != null)
                    {
                        currentSettings.Remark = line.Substring("Remark=".Length);
                    }
                }

                // 添加最后一个设备
                if (currentSettings != null && !string.IsNullOrEmpty(currentKey))
                {
                    allSettings[currentKey] = currentSettings;
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error loading settings from file: {ex.Message}");
            }

            return allSettings;
        }

        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtSerialNum.Text))
                {
                    DeviceMessage.Items.Add("No device connected. Cannot save settings.");
                    return;
                }

                // 使用 SaveFileDialog 让用户选择保存位置
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Light Controller Settings (*.lcs)|*.lcs|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    // 使用项目名称作为默认文件名
                    string projectName = string.IsNullOrEmpty(txtProject.Text) ? "LightControllerSettings" : txtProject.Text;
                    saveFileDialog.FileName = $"{projectName}.lcs";
                    saveFileDialog.Title = "Save All Recipe Settings";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = saveFileDialog.FileName;
                        SaveAllRecipesToFile(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error saving all recipes: {ex.Message}");

                // 错误视觉反馈
                btnSaveAll.BackColor = Color.LightCoral;
                Task.Delay(1500).ContinueWith(t =>
                {
                    if (btnSaveAll.InvokeRequired)
                    {
                        btnSaveAll.Invoke(new Action(() => btnSaveAll.BackColor = SystemColors.Control));
                    }
                    else
                    {
                        btnSaveAll.BackColor = SystemColors.Control;
                    }
                });
            }
        }

        // 保存所有配方到指定文件
        private void SaveAllRecipesToFile(string filePath)
        {
            try
            {
                var allSettings = new Dictionary<string, DeviceSettings>();

                // 保存所有5个配方的设置（从已保存的数据中读取，而不是当前界面）
                string[] recipes = { "R1", "R2", "R3", "R4", "R5" };
                bool anySaved = false;

                // 首先从默认设置文件加载所有现有的配方设置
                var existingSettings = LoadAllSettingsSimple();

                foreach (string recipe in recipes)
                {
                    string key = $"{txtSerialNum.Text}_{recipe}";
                    DeviceSettings settings;

                    if (existingSettings.ContainsKey(key))
                    {
                        // 使用已保存的设置
                        settings = existingSettings[key];
                        DeviceMessage.Items.Add($"Using saved settings for {recipe}");
                    }
                    else
                    {
                        // 如果没有保存过的设置，使用当前界面设置
                        settings = new DeviceSettings
                        {
                            SerialNumber = txtSerialNum.Text,
                            ChannelIntensities = GetCurrentIntensities(),
                            ChannelStates = GetCurrentCheckboxStates(),
                            Remark = GetRemarkByRecipe(recipe),
                            LastUpdated = DateTime.Now
                        };
                        DeviceMessage.Items.Add($"Using current settings for {recipe} (no saved data)");
                    }

                    allSettings[key] = settings;
                    anySaved = true;

                    DeviceMessage.Items.Add($"Saved {recipe} - Remark: {settings.Remark}");
                }

                if (anySaved)
                {
                    // 获取项目名称
                    string projectName = string.IsNullOrEmpty(txtProject.Text) ? "UnnamedProject" : txtProject.Text;

                    // 写入到用户选择的文件
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        writer.WriteLine("# Light Controller Settings Export");
                        writer.WriteLine($"# Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        writer.WriteLine($"# Device Serial: {txtSerialNum.Text}");
                        writer.WriteLine($"# Project Name: {projectName}");
                        writer.WriteLine($"# Recipes: R1, R2, R3, R4, R5");
                        writer.WriteLine();

                        foreach (var kv in allSettings)
                        {
                            writer.WriteLine("[Device]");
                            writer.WriteLine("Key=" + kv.Key);
                            writer.WriteLine("SerialNumber=" + kv.Value.SerialNumber);
                            writer.WriteLine("LastUpdated=" + kv.Value.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss"));
                            writer.WriteLine("Intensities=" + string.Join(",", kv.Value.ChannelIntensities));
                            writer.WriteLine("CheckboxStates=" + string.Join(",", kv.Value.ChannelStates));
                            writer.WriteLine("Remark=" + kv.Value.Remark);
                            writer.WriteLine();
                        }
                    }

                    DeviceMessage.Items.Add($"All recipes exported successfully to: {filePath}");
                    DeviceMessage.Items.Add($"Project: {projectName}");
                    DeviceMessage.Items.Add($"File contains: {allSettings.Count} recipe settings");

                    // 视觉反馈
                    btnSaveAll.BackColor = Color.LightGreen;
                    Task.Delay(1500).ContinueWith(t =>
                    {
                        if (btnSaveAll.InvokeRequired)
                        {
                            btnSaveAll.Invoke(new Action(() => btnSaveAll.BackColor = SystemColors.Control));
                        }
                        else
                        {
                            btnSaveAll.BackColor = SystemColors.Control;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                DeviceMessage.Items.Add($"Error saving to file: {ex.Message}");
                throw;
            }
        }

        // 获取当前强度值
        private int[] GetCurrentIntensities()
        {
            return new int[]
            {
        hScrollBar0.Value,
        hScrollBar1.Value,
        hScrollBar2.Value,
        hScrollBar3.Value,
        hScrollBar4.Value,
        hScrollBar5.Value,
        hScrollBar6.Value,
        hScrollBar7.Value
            };
        }

        // 获取当前复选框状态
        private bool[] GetCurrentCheckboxStates()
        {
            return new bool[]
            {
        checkBox0.Checked,
        checkBox1.Checked,
        checkBox2.Checked,
        checkBox3.Checked,
        checkBox4.Checked,
        checkBox5.Checked,
        checkBox6.Checked,
        checkBox7.Checked
            };
        }


        // 根据配方名称设置备注
        private void SetRemarkByRecipe(string recipeName, string remark)
        {
            if (recipeName == "R1")
                remark1.Text = remark;
            else if (recipeName == "R2")
                remark2.Text = remark;
            else if (recipeName == "R3")
                remark3.Text = remark;
            else if (recipeName == "R4")
                remark4.Text = remark;
            else if (recipeName == "R5")
                remark5.Text = remark;
        }
    }
}
