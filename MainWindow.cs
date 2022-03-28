using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.WinUsb;
using Nefarius.ViGEm.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;

namespace GHLtarUtility
{
    public partial class MainWindow : Form
    {
        ViGEmClient client;
        BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();

        List<PSPeripheral> PSPeripherals = new List<PSPeripheral>();
        List<iOSGuitar> iOSGuitars = new List<iOSGuitar>();

        public MainWindow()
        {
            InitializeComponent();
            this.FormClosing += this_FormClosing;

            try
            {
                client = new ViGEmClient();
            }
            catch (Exception)
            {
                MessageBox.Show("You need to install the ViGEm Bus Driver to use this application.", "GHLtar Utility", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void this_FormClosing(object sender, EventArgs e)
        {
            watcher.Stop();
            foreach (iOSGuitar peripheral in iOSGuitars) peripheral.destroy();
            foreach (PSPeripheral peripheral in PSPeripherals) peripheral.destroy();
        }

        private void UpdatePS3Display()
        {
            if (PSPeripherals.Count >= 1)
            {
                ps3P1Panel.BackColor = Color.LimeGreen;
                ps3P1Label.Text = "Connected!";

                UpdateIndicator(ps3P1Indicator, PSPeripherals[0].controller.UserIndex);
            }
            else
            {
                ps3P1Panel.BackColor = Color.LightGray;
                ps3P1Label.Text = "Not Connected";
                ps3P1Indicator.Image = Properties.Resources.player0;
            }

            if (PSPeripherals.Count >= 2)
            {
                ps3P2Panel.BackColor = Color.LimeGreen;
                ps3P2Label.Text = "Connected!";

                UpdateIndicator(ps3P2Indicator, PSPeripherals[1].controller.UserIndex);
            }
            else
            {
                ps3P2Panel.BackColor = Color.LightGray;
                ps3P2Label.Text = "Not Connected";
                ps3P2Indicator.Image = Properties.Resources.player0;
            }

            if (PSPeripherals.Count >= 3)
            {
                ps3P3Panel.BackColor = Color.LimeGreen;
                ps3P3Label.Text = "Connected!";

                UpdateIndicator(ps3P3Indicator, PSPeripherals[2].controller.UserIndex);
            }
            else
            {
                ps3P3Panel.BackColor = Color.LightGray;
                ps3P3Label.Text = "Not Connected";
                ps3P3Indicator.Image = Properties.Resources.player0;
            }

            if (PSPeripherals.Count >= 4)
            {
                ps3P4Panel.BackColor = Color.LimeGreen;
                ps3P4Label.Text = "Connected!";

                UpdateIndicator(ps3P4Indicator, PSPeripherals[3].controller.UserIndex);
            }
            else
            {
                ps3P4Panel.BackColor = Color.LightGray;
                ps3P4Label.Text = "Not Connected";
                ps3P4Indicator.Image = Properties.Resources.player0;
            }
        }

        private void UpdateIndicator(PictureBox deviceIndicator, int userIndex)
        {
            try
            {
                switch (userIndex)
                {
                    case 0: deviceIndicator.Image = Properties.Resources.player1; break;
                    case 1: deviceIndicator.Image = Properties.Resources.player2; break;
                    case 2: deviceIndicator.Image = Properties.Resources.player3; break;
                    case 3: deviceIndicator.Image = Properties.Resources.player4; break;
                    default: deviceIndicator.Image = Properties.Resources.player0; break;
                }
            }
            catch (Exception)
            {
                deviceIndicator.Image = Properties.Resources.player0;
            }
        }

        private void UpdateiOSDisplay()
        {
            if (iOSGuitars.Count >= 1)
            {
                iOSP1Panel.BackColor = Color.LimeGreen;
                iOSP1Label.Text = "Connected!";
                iOSP1Disconnect.Enabled = true;

                UpdateIndicator(iOSP1Indicator, iOSGuitars[0].controller.UserIndex);
            }
            else
            {
                iOSP1Panel.BackColor = Color.LightGray;
                iOSP1Label.Text = "Not Connected";
                iOSP1Disconnect.Enabled = false;
                iOSP1Indicator.Image = Properties.Resources.player0;
            }

            if (iOSGuitars.Count >= 2)
            {
                iOSP2Panel.BackColor = Color.LimeGreen;
                iOSP2Label.Text = "Connected!";
                iOSP2Disconnect.Enabled = true;

                UpdateIndicator(iOSP2Indicator, iOSGuitars[1].controller.UserIndex);
            }
            else
            {
                iOSP2Panel.BackColor = Color.LightGray;
                iOSP2Label.Text = "Not Connected";
                iOSP2Disconnect.Enabled = false;
                iOSP2Indicator.Image = Properties.Resources.player0;
            }

            if (iOSGuitars.Count >= 3)
            {
                iOSP3Panel.BackColor = Color.LimeGreen;
                iOSP3Label.Text = "Connected!";
                iOSP3Disconnect.Enabled = true;

                UpdateIndicator(iOSP3Indicator, iOSGuitars[2].controller.UserIndex);
            }
            else
            {
                iOSP3Panel.BackColor = Color.LightGray;
                iOSP3Label.Text = "Not Connected";
                iOSP3Disconnect.Enabled = false;
                iOSP3Indicator.Image = Properties.Resources.player0;
            }

            if (iOSGuitars.Count >= 4)
            {
                iOSP4Panel.BackColor = Color.LimeGreen;
                iOSP4Label.Text = "Connected!";
                iOSP4Disconnect.Enabled = true;

                UpdateIndicator(iOSP4Indicator, iOSGuitars[3].controller.UserIndex);
            }
            else
            {
                iOSP4Panel.BackColor = Color.LightGray;
                iOSP4Label.Text = "Not Connected";
                iOSP4Disconnect.Enabled = false;
                iOSP4Indicator.Image = Properties.Resources.player0;
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            watcher.ScanningMode = BluetoothLEScanningMode.Active;
            watcher.Received += OnBLEAdvertisement;
            DisplayTimer_Tick(sender, e);
        }

        async private void OnBLEAdvertisement(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            if (eventArgs.Advertisement.LocalName.Contains("Ble Guitar"))
            {
                BluetoothLEDevice guitar = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);
                iOSGuitar newGuitar = new iOSGuitar(guitar, client.CreateXbox360Controller());
                iOSGuitars.Add(newGuitar);
            }
        }

        private void iOSSearching_CheckedChanged(object sender, EventArgs e)
        {
            if (iOSSearching.Checked) watcher.Start();
            if (!iOSSearching.Checked) watcher.Stop();
        }

        private void DisplayTimer_Tick(object sender, EventArgs e)
        {
            foreach (iOSGuitar guitar in iOSGuitars.ToList())
            {
                if (guitar.isDisconnected)
                {
                    iOSGuitars.Remove(guitar);
                }
            }

            // Create list of devices to prevent re-attaching existing dongles.
            List<string> devices = new List<string>();

            foreach (PSPeripheral peripheral in PSPeripherals.ToList())
            {
                // Remove any peripherals that can't be found anymore
                if (!peripheral.isReadable())
                {
                    peripheral.destroy();
                    PSPeripherals.Remove(peripheral);
                }
                else
                {
                    // Add guitars that are still found to the list of existing devices
                    devices.Add(peripheral.device.DevicePath);
                }
            }

            // Enumerate through WinUSB devices and set those up if they are valid dongles.
            foreach (UsbRegistry device in UsbDevice.AllDevices)
            {
                if ((!PS3Guitar.isCorrectDevice(device) &&
                    !PS4Guitar.isCorrectDevice(device) &&
                    !PS3Turntable.isCorrectDevice(device)) ||
                    PSPeripherals.Count >= 4)
                    continue;

                UsbDevice trueDevice;
                device.Open(out trueDevice);
                if (trueDevice != null && !devices.Contains(trueDevice.DevicePath))
                {
                    PSPeripheral newPeripheral = null;
                    if (PS3Guitar.isCorrectDevice(device))
                    {
                        newPeripheral = new PS3Guitar(trueDevice, client.CreateXbox360Controller());
                    }
                    if (PS3Turntable.isCorrectDevice(device))
                    {
                        newPeripheral = new PS3Turntable(trueDevice, client.CreateXbox360Controller());
                    }
                    if (PS4Guitar.isCorrectDevice(device))
                    {
                        newPeripheral = new PS4Guitar(trueDevice, client.CreateXbox360Controller());
                    }

                    PSPeripherals.Add(newPeripheral);
                }
            }

            UpdateiOSDisplay();
            UpdatePS3Display();
        }

        private void iOSDisconnect_Click(object sender, EventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "iOSP1Disconnect":
                    if (iOSGuitars.Count >= 1) iOSGuitars[0].destroy(); break;
                case "iOSP2Disconnect":
                    if (iOSGuitars.Count >= 2) iOSGuitars[1].destroy(); break;
                case "iOSP3Disconnect":
                    if (iOSGuitars.Count >= 3) iOSGuitars[2].destroy(); break;
                case "iOSP4Disconnect":
                    if (iOSGuitars.Count >= 4) iOSGuitars[3].destroy(); break;
            }
            MessageBox.Show("iOS guitars don't power off yet. Please disconnect the batteries to fully turn off your guitar, or wait for the guitar to time out.", "iOS Guitars", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
