using System;
using System.Runtime.InteropServices;

namespace EchWorkersManager.Helpers
{
    internal static class WinTunInterop
    {
        [DllImport("wintun.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr WintunCreateAdapter(
            [MarshalAs(UnmanagedType.LPWStr)] string name,
            [MarshalAs(UnmanagedType.LPWStr)] string tunnelType,
            IntPtr guid
        );

        [DllImport("wintun.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void WintunDeleteAdapter(IntPtr adapter);

        [DllImport("wintun.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr WintunStartSession(IntPtr adapter, uint capacity);

        [DllImport("wintun.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void WintunEndSession(IntPtr session);

        [DllImport("wintun.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr WintunAllocateReceivePacket(IntPtr session, out uint size);

        [DllImport("wintun.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void WintunReleaseReceivePacket(IntPtr session, IntPtr packet);

        [DllImport("wintun.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr WintunAllocateSendPacket(IntPtr session, uint size);

        [DllImport("wintun.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void WintunSendPacket(IntPtr session, IntPtr packet);
    }
}
