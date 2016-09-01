using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBackupStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            try
            {
                File.Create("test").Dispose();
                File.Delete("test");
            }
            catch
            {
                 startInfo.Verb = "runas";
            }
            
            startInfo.WorkingDirectory = "SecureBackup";
            startInfo.FileName = "SecureBackupMainGUI.exe";
            var process = Process.Start(startInfo);
        }
    }
}
