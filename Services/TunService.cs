using System;
using System.Threading;
using System.Runtime.InteropServices;
using EchWorkersManager.Helpers;

namespace EchWorkersManager.Services
{
    public class TunService
    {
        private IntPtr adapter;
        private IntPtr session;
        private Thread readThread;
        private bool running;

        public event Action<byte[]> OnPacketReceived;
        public event Func<byte[]> OnPacketRequest;

        public void Start()
        {
            adapter = WinTunInterop.WintunCreateAdapter("ECH-TUN", "ECH Tunnel", IntPtr.Zero);
            session = WinTunInterop.WintunStartSession(adapter, capacity: 0x400000); // 4MB

            running = true;

            readThread = new Thread(ReadLoop);
            readThread.Start();

            new Thread(WriteLoop).Start();
        }

        private void ReadLoop()
        {
            while (running)
            {
                uint size;
                IntPtr packet = WinTunInterop.WintunAllocateReceivePacket(session, out size);
                if (packet == IntPtr.Zero) continue;

                byte[] data = new byte[size];
                Marshal.Copy(packet, data, 0, (int)size);

                OnPacketReceived?.Invoke(data);

                WinTunInterop.WintunReleaseReceivePacket(session, packet);
            }
        }

        private void WriteLoop()
        {
            while (running)
            {
                byte[] data = OnPacketRequest?.Invoke();
                if (data == null) 
                {
                    Thread.Sleep(2);
                    continue;
                }

                IntPtr packet = WinTunInterop.WintunAllocateSendPacket(session, (uint)data.Length);
                Marshal.Copy(data, 0, packet, data.Length);
                WinTunInterop.WintunSendPacket(session, packet);
            }
        }

        public void Stop()
        {
            running = false;
            WinTunInterop.WintunEndSession(session);
            WinTunInterop.WintunDeleteAdapter(adapter);
        }
    }
}
