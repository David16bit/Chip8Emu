using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows;



namespace Chip8EmulatorWPF
{
    /// <summary>
    /// 2016 - David Brown (asapdavid91@gmail.com)
    /// Chip8 WPF Hexadecimal editor
    /// </summary>

    public static class Utli
    {
        
        public static string StringToHex(string hexstring)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char t in hexstring)
            {
                //Note: X for upper, x for lower case letters
                sb.Append(Convert.ToInt32(t).ToString("X"));
            }
            return sb.ToString();
        }

        public static byte getHighByte(ushort val)
        {
            return (byte)(val >> 8);
        }

        public static byte getUpperByteHighNibble(ushort val)
        {
            return (byte)(val >> 12);
        }

        public static byte getUpperByteLowNibble(ushort val)
        {
            return (byte)((val >> 8) & 0x0F);
        }

        public static byte getLowerByte(ushort val)
        {
            return (byte)(val & 0x00FF);
        }

        public static byte getLowerByteHighNibble(ushort val)
        {
            return (byte)((val & 0x00FF) >> 4);

        }

        public static byte getLowerByteLowNibble(ushort val)
        {
            return (byte)((val & 0x00FF) & 0x0F);
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

    }
}
