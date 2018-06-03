using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace _NSIS_Version_Getter
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Syntax: " + Path.GetFileName(Assembly.GetExecutingAssembly().Location) + " <assembly> [outfile]");
                return 1;
            }

            TextWriter output;
            if (args.Length >= 2)
            {
                try
                {
                    // This particular encoding happens to work for the Copyright symbol, so good enough.
                    output = new StreamWriter(args[1], false, Encoding.GetEncoding("iso-8859-1"));
                }
                catch
                {
                    return 999;
                }
            }
            else
                output = Console.Out;

            string filename = args[0];
            FileVersionInfo fvi;
            DateTime timestamp;
            try
            {
                FileStream fs = File.OpenRead(filename);
                // IMAGE_DOS_HEADER always starts at offset 0.
                // 0x3C is offset into IMAGE_DOS_HEADER where IMAGE_NT_HEADERS offset is stored.
                byte[] e_lfanew = new byte[4];
                fs.Seek(0x3C, SeekOrigin.Begin);
                fs.Read(e_lfanew, 0, e_lfanew.Length);
                int ntOffset = BitConverter.ToInt32(e_lfanew, 0);

                // IMAGE_NT_HEADERS has a 4-byte signature followed by an IMAGE_FILE_HEADER.
                // Timestamp is 4 bytes into IMAGE_FILE_HEADER (8 bytes total past the start of IMAGE_NT_HEADERS).
                byte[] secsFrom1970 = new byte[4];
                fs.Seek(ntOffset + 8, SeekOrigin.Begin);
                fs.Read(secsFrom1970, 0, secsFrom1970.Length);
                timestamp = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(BitConverter.ToInt32(secsFrom1970, 0)).ToLocalTime();
                fs.Close();

                fvi = FileVersionInfo.GetVersionInfo(filename);
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("Assembly not found.");
                return 2;
            }
            catch (BadImageFormatException)
            {
                Console.Error.WriteLine("Not a valid .NET assembly");
                return 3;
            }

            output.WriteLine("!define PROGNAME \"" + fvi.ProductName + "\"");
            output.WriteLine("!define PROGVER \"" + fvi.ProductVersion + "\"");
            output.WriteLine("!define PROGDATE \"" + timestamp.ToString("yyyy-MM-dd") + "\"");
            output.WriteLine("!define COMPANY \"" + fvi.CompanyName + "\"");
            output.WriteLine("!define PACKAGE \"The Controller Series\"");
            output.WriteLine("!define COPYRIGHT \"" + fvi.LegalCopyright + "\"");
            if (args.Length >= 2)
                output.Close();
            return 0;
        }
    }
}
