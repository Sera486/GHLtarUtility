using LibUsbDotNet;
using Nefarius.ViGEm.Client.Targets;

namespace GHLtarUtility
{
    abstract class PSPeripheral
    {
        public UsbDevice device;
        public IXbox360Controller controller;

        public abstract bool isReadable();

        public abstract void destroy();
    }
}
