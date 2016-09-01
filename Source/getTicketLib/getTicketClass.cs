using memStorageLib;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace getTicketLib
{
    public class getTicketClass
    {
        //on signale download/upload toujours actif
        public static int status;
        static void reportStatus()
        {
            while (status == 1)
            {

                string memFileName;
                int memFileBytes;

                memFileName = "SecureBackup-downloadUploadExecute-z34b5923z5";
                memFileBytes = 1000;
                using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(memFileName, memFileBytes))
                {
                    using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                    {
                        byte[] asciiBytes = Encoding.ASCII.GetBytes("1");
                        accessor.WriteArray(0, asciiBytes, 0, asciiBytes.Length);
                    }
                }

                Thread.Sleep(100);
            }
        }

        public static void getTicket()
        {
            //on attend que la voie downloadUpload soit libre
            ///////////////////////////////////////
            string ticket = Guid.NewGuid().ToString();

            string memFileName;
            int memFileBytes;

            //ajoute a la queue
            addToQueue:
            memFileName = "SecureBackup-downloadUploadQueue-z34b5923z5";
            memFileBytes = 1000;
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(memFileName, memFileBytes))
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                {
                    byte[] asciiBytes = Encoding.ASCII.GetBytes(ticket);
                    accessor.WriteArray(0, asciiBytes, 0, asciiBytes.Length);
                }
            }

            Thread.Sleep(100);

            string execute;

            memFileName = "SecureBackup-downloadUploadExecute-z34b5923z5";
            memFileBytes = 1000;
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(memFileName, memFileBytes))
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                {
                    execute = memStorageClass.getMem(accessor).ToString();
                }
            }

            if (execute != ticket)
            {
                goto addToQueue;
            }

            status = 1;
            Task.Run(() => reportStatus());
            ///////////////////////////////////////
        }
    }
}
