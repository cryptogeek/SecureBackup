using memStorageLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SecureBackupQueue
{
    class Program
    {
        ////visibilité console
        //[DllImport("kernel32.dll")]
        //static extern IntPtr GetConsoleWindow();
        //[DllImport("user32.dll")]
        //static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        //const int SW_HIDE = 0;
        //const int SW_SHOW = 5;

        static string startupPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static string execDir = Directory.GetParent(startupPath).ToString();

        //liste des backups a executer automatiquement
        static List<string> listAuto = new List<string>();
        static void getAuto()
        {
            listAuto.Clear();
            int found = 0;
            foreach (string file in Directory.GetFiles(execDir + "\\backupParams"))
            {
                StreamReader paramReader = new StreamReader(file);
                string line;
                
                while ((line = paramReader.ReadLine()) != null)
                {
                    string name = Path.GetFileName(file);
                    name = name.Substring(0, name.Length - 4);
                    if (line.Split('|')[9] == "True")
                    {
                        listAuto.Add(name+"|"+ line.Split('|')[8]);
                        found = 1;
                    }
                }
                paramReader.Close();
            }
            if (found == 1)
            {
                //backups automatiques trouvés
                //Console.WriteLine("Ajout au démarrage auto...");
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey StartupPath;
                StartupPath = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                //if (StartupPath.GetValue("SecureBackupQueue") == null)
                //{
                    StartupPath.SetValue("SecureBackupQueue", startupPath, RegistryValueKind.ExpandString);
                //}
            }else
            {
                try {
                    string keyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName, true))
                    {
                       key.DeleteValue("SecureBackupQueue");
                    }
                }catch { }
            }
        }

       static void checkSignal() {
            string memFileName = "SecureBackupQueue-Recheck-z34b5923z5";
            int memFileBytes = 1000;
            using (MemoryMappedFile mmfCheck = MemoryMappedFile.CreateOrOpen(memFileName, memFileBytes))
            {
                using (MemoryMappedViewAccessor accessorRecheck = mmfCheck.CreateViewAccessor())
                {
                    do
                    {
                        if (memStorageClass.getMem(accessorRecheck).ToString() == "1")
                        {
                            //liste des backups a executer automatiquement
                            getAuto();

                            //stop le signal
                            byte[] asciiBytes = Encoding.ASCII.GetBytes("0");
                            accessorRecheck.WriteArray(0, asciiBytes, 0, asciiBytes.Length);
                        }
                        Thread.Sleep(1000);
                    } while (true);
                }
            }
        }

        static void Main(string[] args)
        {
            ////cache la console
            //var handle = GetConsoleWindow();
            //ShowWindow(handle, SW_HIDE);

            //démare queue downloadUpload
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = execDir + "\\SecureBackupQueuedownloadUpload.exe";
            Process.Start(startInfo);

            //quitte si queue deja en execution
            Process[] processes = Process.GetProcessesByName("SecureBackupQueue");
            if (processes.Length > 1)
            {
                Environment.Exit(0);
            }

            //liste des backups a executer automatiquement
            getAuto();

            //on check régulièrement si les paramètres de backups ont changés
            Task.Run(()=>checkSignal());
            
            float i = 0;

            string memFileName = "SecureBackupQueue-queue-z34b5923z5";
            int memFileBytes = 1000;
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(memFileName, memFileBytes)) //absolument utiliser using autrement le fichier mémoire est collecté par la poubelle après quelques minutes
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                {
                    do
                    {
                        StringBuilder message = memStorageClass.getMem(accessor);
                        //Console.WriteLine(message);

                        //on execute les taches auto
                        List<string> listAutoTemp = new List<string>();
                        foreach (string item in listAuto) listAutoTemp.Add(item);
                        foreach (string item in listAutoTemp)
                        {
                            if ((i / 60) % Convert.ToUInt32(item.Split('|')[1]) == 0)
                            {
                                StreamReader paramReader = new StreamReader(execDir + "\\backupParams\\" + item.Split('|')[0] + ".txt");
                                string line = paramReader.ReadLine();
                                paramReader.Close();
                                startInfo = new ProcessStartInfo();
                                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                startInfo.FileName = execDir + "\\SecureBackupExecution.exe";
                                startInfo.Arguments = line.Split('|')[0] + " " + line.Split('|')[1] + " " + line.Split('|')[2] + " " + line.Split('|')[3] + " " + line.Split('|')[4] + " \"" + line.Split('|')[5] + "\" \"" + line.Split('|')[6] + "\" " + line.Split('|')[7] + " \"" + item.Split('|')[0] + "\" " + line.Split('|')[10] + " "+ line;
                                var process = Process.Start(startInfo);
                                process.WaitForExit();

                            }
                        }

                        //execute les taches dans la queue
                        foreach (string item in message.ToString().Replace("/", "").Split('|'))
                        {
                            if (item != "")
                            {
                                //retire tache de la queue
                                //string messageS = message.ToString().Replace("|"+item+"|","");
                                var regex = new Regex(Regex.Escape("|" + item + "|"));
                                string messageS = regex.Replace(message.ToString(), "", 1);
                                string fill = "";
                                for (int ii = 0; ii < 500; ii++) fill = fill + "/";
                                byte[] asciiBytes = Encoding.ASCII.GetBytes(fill);
                                accessor.WriteArray(0, asciiBytes, 0, asciiBytes.Length);
                                asciiBytes = Encoding.ASCII.GetBytes(messageS);
                                accessor.WriteArray(0, asciiBytes, 0, asciiBytes.Length);

                                //execute la tache
                                StreamReader paramReader = new StreamReader(execDir + "\\backupParams\\" + item + ".txt");
                                string line = paramReader.ReadLine();
                                paramReader.Close();
                                startInfo = new ProcessStartInfo();
                                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                startInfo.FileName = execDir + "\\SecureBackupExecution.exe";
                                startInfo.Arguments = line.Split('|')[0] + " " + line.Split('|')[1] + " " + line.Split('|')[2] + " " + line.Split('|')[3] + " " + line.Split('|')[4] + " \"" + line.Split('|')[5] + "\" \"" + line.Split('|')[6] + "\" " + line.Split('|')[7] + " \"" + item + "\" " + line.Split('|')[10] + " " + line;
                                var process = Process.Start(startInfo);
                                process.WaitForExit();

                                message = memStorageClass.getMem(accessor);

                            }
                        }

                        Thread.Sleep(1000);

                        i++;

                    } while (true);
                }
            }     
        }
    }
}
