using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace KeyDecryptor
{
    class Program
    {
        private static string _fileName = Application.StartupPath + "//logs";
        private static byte[] _pass0 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        private static byte[] _pass1 = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        static void Main(string[] args)
        {
            if (!File.Exists(_fileName))
            {
                Console.Write("Enter log path: ");
                while (true)
                {
                    _fileName = Console.ReadLine();
                    if (File.Exists(_fileName))
                    {
                        SaveDecrypted();
                        break;
                    }
                    Console.Write("File doesn't exist. (Invalid Path)\nEnter log path: ");
                }
            }
            else
            {
                SaveDecrypted();
            }
        }
        static void SaveDecrypted()
        {
            StreamReader streamReader = new StreamReader(_fileName);
            StreamWriter streamWriter = new StreamWriter(_fileName + "Decrypted");
            while (!streamReader.EndOfStream)
            {
                try
                {
                    streamWriter.WriteLine(Decrypt(streamReader.ReadLine()));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            streamWriter.Close();
            Console.WriteLine("Done.\nPress any key to exit . . .");
            Console.ReadKey();
        }
        static string Decrypt(string cyphertext)
        {
            var aes = Aes.Create();
            var key = aes.CreateDecryptor(_pass0, _pass1);
            MemoryStream mem = new MemoryStream(Convert.FromBase64String(cyphertext));
            CryptoStream cryptoStream = new CryptoStream(mem, key, CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(cryptoStream);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            return text;

        }
    }
}
