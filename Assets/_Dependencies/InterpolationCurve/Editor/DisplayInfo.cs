using System.Runtime.InteropServices;

using UnityEngine;

namespace InterpolationCurve.Editor
{
    public static class DisplayInfo
    {
        public static Vector2Int Resolution
        {
            get
            {
                DEVMODE devMode = default;
                devMode.dmSize = (short) Marshal.SizeOf(devMode);
                EnumDisplaySettings(null, -1, ref devMode);
                return new Vector2Int(devMode.dmPelsWidth, devMode.dmPelsHeight);
            }
        }

        public static Vector2Int CursorPosition
        {
            get
            {
                GetCursorPos(out POINT lpPoint);
                return new Vector2Int(lpPoint.X, lpPoint.Y);
            }
        }

        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [StructLayout(LayoutKind.Sequential)]
        struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)] public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)] public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);
    }
}
