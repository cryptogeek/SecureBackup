using getTicketLib;
using multiLangLib;
using nukeDirLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using WinSCP;

namespace showLog
{
    class Program
    {
        static string ip;
        static int port;
        static string key;
        static string user;
        static string pass;
        static string source;
        static string dest;
        static string encryptionKey;
        static string backupName;
        static string workDir;
        static string execDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        static void decryptFile(string file, string pass)
        {
            //chemin de l'executable
            string execDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //string dir = Path.GetDirectoryName(file);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = execDir + @"\7-Zip\7z.exe";
            startInfo.Arguments = "e \"" + file + "\" -p" + pass + " -o\"" + workDir + "\"";
            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        static string ext;

        static void Main(string[] args)
        {      
            if (args.Length < 7) System.Environment.Exit(0);

            multiLangClass.translate();

            Console.WriteLine(multiLangClass.getText(5));

            ip = args[0];
            //Console.WriteLine("ip: "+ip);

            port = Convert.ToInt32(args[1]);
            //Console.WriteLine("port: "+port);

            key = args[2];
            //Console.WriteLine("Clé SSH hôte distant: " + key);

            user = args[3];
            //Console.WriteLine("Utilisateur: " + user);

            pass = args[4];
            //Console.WriteLine("Mot de passe: " + pass);

            source = args[5];
            //Console.WriteLine("Dossier source: " + source);

            dest = args[6];
            //Console.WriteLine("Dossier destination: " + dest);

            encryptionKey = args[7];

            backupName = args[8];

            workDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SecureBackupWorkDirShowLog " + backupName;

            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            //db fichiers
            string fileDBName = "remoteFilesList.txt";
            string fileDBLocalPath = workDir + @"\" + fileDBName;
            string fileDBArchiveName = "db.7z";
            string fileDBArchiveLocalPath = workDir + @"\" + fileDBArchiveName;
            string fileDBArchiveRemotePath = dest + @"/" + fileDBArchiveName;

            //log fichiers
            string fileLogName = "log.txt";
            string fileLogLocalPath = workDir + @"\" + fileLogName;
            string fileLogArchiveName = "log.7z";
            string fileLogArchiveLocalPath = workDir + @"\" + fileLogArchiveName;
            string fileLogArchiveRemotePath = dest + @"/" + fileLogArchiveName;

            //db dossiers
            string dirDBName = "localDirList.txt";
            string dirDBLocalPath = workDir + @"\" + dirDBName;
            string dirDBArchiveName = "DirDB.7z";
            string dirDBArchiveLocalPath = workDir + @"\" + dirDBArchiveName;
            string dirDBArchiveRemotePath = dest + @"/" + dirDBArchiveName;

            //log dossiers
            string dirLogName = "dirLog.txt";
            string dirLogLocalPath = workDir + @"\" + dirLogName;
            string dirLogArchiveName = "dirLog.7z";
            string dirLogArchiveLocalPath = workDir + @"\" + dirLogArchiveName;
            string dirLogArchiveRemotePath = dest + @"/" + dirLogArchiveName;

            string newDBsName = "newDBs";
            string newDBsLocal = workDir + "\\" + newDBsName;
            string newDBsRemote = dest + "/" + newDBsName;

            string humanLog = workDir + @"\humanReadableLog.txt";

            File.Delete(fileLogLocalPath);
            File.Delete(humanLog);
            

            // options sftp
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = ip,
                UserName = user,
                Password = pass,
                SshHostKeyFingerprint = "ssh-rsa 2048 " + key,
                PortNumber = port
            };

            //connexion sftp
            Session session = new Session();

            //session.ExecutablePath = execDir+@"\WinSCP\WinSCP.exe";

            //Console.WriteLine("Ouverture connexion SFTP...");
            session.Open(sessionOptions);

            //on attend que la voie downloadUpload soit libre
            ///////////////////////////////////////
            getTicketClass.getTicket();
            ///////////////////////////////////////

            //on utilise les DBs les plus récentes
            ///////////////////////////////////////
                ext = ".new";
                if (session.FileExists(newDBsRemote))
                {
                    if (session.FileExists(fileDBArchiveRemotePath + ext))
                    {
                        session.RemoveFiles(fileDBArchiveRemotePath);
                        session.MoveFile(fileDBArchiveRemotePath + ext, fileDBArchiveRemotePath);
                    }
                    if (session.FileExists(fileLogArchiveRemotePath + ext))
                    {
                        session.RemoveFiles(fileLogArchiveRemotePath);
                        session.MoveFile(fileLogArchiveRemotePath + ext, fileLogArchiveRemotePath);
                    }
                    if (session.FileExists(dirDBArchiveRemotePath + ext))
                    {
                        session.RemoveFiles(dirDBArchiveRemotePath);
                        session.MoveFile(dirDBArchiveRemotePath + ext, dirDBArchiveRemotePath);
                    }
                    if (session.FileExists(dirLogArchiveRemotePath + ext))
                    {
                        session.RemoveFiles(dirLogArchiveRemotePath);
                        session.MoveFile(dirLogArchiveRemotePath + ext, dirLogArchiveRemotePath);
                    }

                    session.RemoveFiles(newDBsRemote);
                }
            ////////////////////////////////////////

            //download log fichiers
            ///////////////////////////////
                string destArchive;
                string localArchive;
                string localFile;

                //log des fichiers
                //param
                destArchive = fileLogArchiveRemotePath;
                localArchive = fileLogArchiveLocalPath;
                localFile = fileLogLocalPath;
                //end param
                if (session.FileExists(destArchive))
                {
                    File.Delete(localArchive);
                    session.GetFiles(
                        destArchive,
                        localArchive
                    );
                }
            ///////////////////////////////

            //déchiffre les dbs
            //////////////////////////////
                decryptFile(fileLogArchiveLocalPath,encryptionKey);
            //////////////////////////////

            //On signale que la voie downloadUpload est libre
            ////////////////
            getTicketClass.status =0;
            ///////////////

            session.Close();

            List<string> items = new List<string>();

            StreamReader logReader = new StreamReader(fileLogLocalPath);
            string line;
            while ((line = logReader.ReadLine()) != null)
            {
                items.Add( line.Split('|')[0] + "|" + line.Split('|')[1] + "|" + line.Split('|')[2] + "|" + line.Split('|')[3] );
            }
            logReader.Close();

            items.Reverse();

            StreamWriter logWriter = new StreamWriter(humanLog,true);
            foreach (string item in items) {
                string ioEvent;
                if (item.Split('|')[1] == "1")
                {
                    ioEvent = multiLangClass.getText(1);
                }
                else
                {
                    ioEvent = multiLangClass.getText(2);
                }
                logWriter.WriteLine( multiLangClass.getText(3)+":"+UnixTimeStampToDateTime( double.Parse(item.Split('|')[0]) ) + "   " + ioEvent + ":\"" + item.Split('|')[2] +"\"   "+multiLangClass.getText(4)+":"+ UnixTimeStampToDateTime(double.Parse(item.Split('|')[3])) );
            }
            logWriter.Close();

            var process = Process.Start("notepad.exe", humanLog);
            Console.WriteLine(multiLangClass.getText(6));
            process.WaitForExit();

            //Console.WriteLine("Nettoyage fichiers temp: "+workDir);
            nukeDirClass.nukeDir(workDir);
        }
    }
}
