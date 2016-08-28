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

namespace SecureBackupQueuedownloadUpload
{
    class Program
    {
        static string startupPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        static string execDir = Directory.GetParent(startupPath).ToString();

        static void Main(string[] args)
        {
            //quitte si deja en execution
            Process[] processes = Process.GetProcessesByName("SecureBackupQueuedownloadUpload");
            if (processes.Length > 1)
            {
                Environment.Exit(0);
            }

            string memFileNameQueue = "SecureBackup-downloadUploadQueue-z34b5923z5";
            int memFileBytesQueue = 1000;
            string memFileNameExecute = "SecureBackup-downloadUploadExecute-z34b5923z5";
            int memFileBytesExecute = 1000;
            using (MemoryMappedFile mmfQueue = MemoryMappedFile.CreateOrOpen(memFileNameQueue, memFileBytesQueue))
            using (MemoryMappedFile mmfExecute = MemoryMappedFile.CreateOrOpen(memFileNameExecute, memFileBytesExecute))
            {
                using (MemoryMappedViewAccessor accessorQueue = mmfQueue.CreateViewAccessor())
                using (MemoryMappedViewAccessor accessorExecute = mmfExecute.CreateViewAccessor())
                {
                    byte[] asciiBytes = Encoding.ASCII.GetBytes("done");
                    accessorExecute.WriteArray(0, asciiBytes, 0, asciiBytes.Length);

                    string previous="";

                    do
                    {
                        string ticket = memStorageClass.getMem(accessorQueue).ToString();

                        if(previous!=ticket) {
                            asciiBytes = Encoding.ASCII.GetBytes(ticket);
                            accessorExecute.WriteArray(0, asciiBytes, 0, asciiBytes.Length);
                        }

                        previous = ticket;

                        Thread.Sleep(200);

                        do
                        {
                            asciiBytes = Encoding.ASCII.GetBytes("done");
                            accessorExecute.WriteArray(0, asciiBytes, 0, asciiBytes.Length);

                            Thread.Sleep(200);
                            
                        }while(!memStorageClass.getMem(accessorExecute).ToString().Contains("done"));

                    }while(true);
                }
            }
        }
    }
}
