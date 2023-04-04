using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ScreenMy
{
    class Program
    {
        private static int nst;

        private static Dictionary<string, string> base_ip = new Dictionary<string, string>(){
            { "remote", "1.1"},
        };
        static void Main(string[] args)
        {
            Console.ReadKey();
            TimerCallback callback = new TimerCallback(Tick);
            Console.WriteLine("Creating timer: {0}\n",
                               DateTime.Now.ToString("h:mm:ss"));
            System.Threading.Timer stateTimer = new System.Threading.Timer(callback, null, 0, 1000);
            for (; ; )
            {
                Thread.Sleep(100);
            }
            Console.ReadKey();
        }

        static public void Tick(Object stateInfo)
        {
            nst = nst + 1;
            string fl_file = @"\\192.168.88.100\11\fl_data.txt";
            if (File.Exists(fl_file))
            {
                string line1 = File.ReadLines(fl_file).Last().Trim();
                string tbase = line1.Substring(0, 3).ToLower();
                screen1(tbase,"" + line1+"_" + nst.ToString("F0").PadLeft(7, '0'));
                //Удалить Флаг
                File.Delete(fl_file);
            }
            
            Console.WriteLine("Creating timer: {0}\n",
                               DateTime.Now.ToString("h:mm:ss"));
        }

        static void screen1(string tbase,string i)
        {
            string ip = base_ip[tbase];
            IntPtr hwnd = FindWindow(null, "mRemoteNG - confCons.xml - "+ tbase + "_"+ ip);
            CaptureWindow(hwnd, @"e:\screen\" + i + ".jpg");
        }

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr handle, ref Rectangle rect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        static void CaptureWindow(IntPtr handle, string pth)
        {
            Rectangle rect = new Rectangle();
            GetWindowRect(handle, ref rect);

            rect.Width = rect.Width - rect.X;
            rect.Height = rect.Height - rect.Y;

            using ( Bitmap bitmap = new Bitmap(rect.Width, rect.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    IntPtr hdc = g.GetHdc();
                    if (!PrintWindow(handle, hdc, 0))
                    {
                        int error = Marshal.GetLastWin32Error();
                        var exception = new System.ComponentModel.Win32Exception(error);
                        Debug.WriteLine("ERROR: " + error + ": " + exception.Message);
                    }
                    g.ReleaseHdc(hdc);
                }
                Rectangle section = new Rectangle(new Point(230, 200), new Size(1130, 470));  
                Bitmap CroppedImage = CropImage(bitmap, section);
                CroppedImage.Save(pth);
            }
        }
        static Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
            }
        }

    }

}
