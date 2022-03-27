using LibUsbDotNet;
using LibUsbDotNet.Main;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GHLtarUtility
{
    class PS4Guitar : PS3Peripheral
    {
        private Timer runTimer;
        private Thread t;
        private bool shouldStop;

        public PS4Guitar(UsbDevice dongle, IXbox360Controller newController)
        {
            device = dongle;
            controller = newController;

            // Timer to send control packets
            runTimer = new Timer(10000);
            runTimer.Elapsed += sendControlPacket;
            runTimer.Start();

            // Thread to constantly read inputs
            t = new Thread(new ThreadStart(updateRoutine));
            t.Start();

            controller.Connect();
        }

        public override bool isReadable()
        {
            // If device isn't open (closes itself), assume disconnected.
            if (!device.IsOpen) return false;
            if (!device.UsbRegistryInfo.IsAlive) return false;
            return true;
        }

        private void updateRoutine()
        {
            while (!shouldStop)
            {
                // Read 64 bytes from the guitar
                int bytesRead;
                var readBuffer = new byte[64];
                var reader = device.OpenEndpointReader(ReadEndpointID.Ep01);
                reader.Read(readBuffer, 100, out bytesRead);

                // Prevent default 0x00 when no bytes are read
                if (bytesRead > 0)
                {
                    //var bufferString = string.Join(" ", readBuffer) + "\n";
                    //Console.WriteLine(bufferString);

                    // Set the fret inputs on the virtual 360 controller
                    byte frets = readBuffer[5];
                    byte wFrets = readBuffer[6];
                    controller.SetButtonState(Xbox360Button.A, ((frets >> 5) & 1) == 1); // B1
                    controller.SetButtonState(Xbox360Button.B, ((frets >> 6) & 1) == 1); // B2
                    controller.SetButtonState(Xbox360Button.Y, ((frets >> 7) & 1) == 1); // B3
                    controller.SetButtonState(Xbox360Button.X, ((frets >> 4) & 1) == 1); // W1
                    controller.SetButtonState(Xbox360Button.LeftShoulder, ((wFrets >>0) & 1) == 1); // W2
                    controller.SetButtonState(Xbox360Button.RightShoulder, ((wFrets >> 1) & 1) == 1); // W3

                    // Set the strum bar values - can probably be more efficient but eh
                    byte strum = readBuffer[2];
                    if (strum == 0xFF)
                    {
                        // Strum Down
                        controller.SetButtonState(Xbox360Button.Down, true);
                        controller.SetAxisValue(Xbox360Axis.LeftThumbY, -32768);
                        controller.SetButtonState(Xbox360Button.Up, false);
                    }
                    else if (strum == 0x00)
                    {
                        // Strum Up
                        controller.SetButtonState(Xbox360Button.Down, false);
                        controller.SetAxisValue(Xbox360Axis.LeftThumbY, 32767);
                        controller.SetButtonState(Xbox360Button.Up, true);
                    }
                    else
                    {
                        // No Strum
                        controller.SetButtonState(Xbox360Button.Down, false);
                        controller.SetAxisValue(Xbox360Axis.LeftThumbY, 0);
                        controller.SetButtonState(Xbox360Button.Up, false);
                    }

                    // Set the buttons (pause/HP only for now)
                    byte buttons = readBuffer[6];
                    controller.SetButtonState(Xbox360Button.Start, ((buttons >> 6) & 1) == 1); // Pause
                    controller.SetButtonState(Xbox360Button.Back, ((buttons >> 7) & 1) == 1); // Hero Power
                    controller.SetButtonState(Xbox360Button.LeftThumb, (buttons & 0x04) != 0x00); // GHTV Button
                    controller.SetButtonState(Xbox360Button.Guide, (buttons & 0x10) != 0x00); // Sync Button

                    //// Set the tilt and whammy
                    controller.SetAxisValue(Xbox360Axis.RightThumbY, (short)((readBuffer[3] * 0x101) - 32768));
                    controller.SetAxisValue(Xbox360Axis.RightThumbX, (short)((readBuffer[4] * 0x101) - 32768));

                    // TODO: Proper D-Pad emulation
                }
            }
        }

        private void sendControlPacket(Object source, ElapsedEventArgs e)
        {
            // Send the control packet (this is what keeps strumming alive)
            byte[] buffer = new byte[9] { 0x30, 0x02, 0x08, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int bytesWrote;
            UsbSetupPacket setupPacket = new UsbSetupPacket(0x21, 0x09, 0x0201, 0x0000, 0x0008);
            device.ControlTransfer(ref setupPacket, buffer, 0x0008, out bytesWrote);
        }

        public override void destroy()
        {
            // Destroy EVERYTHING.
            shouldStop = true;
            try { controller.Disconnect(); } catch (Exception) { }
            runTimer.Stop();
            runTimer.Dispose();
            t.Abort();
            device.Close();
        }
    }
}
