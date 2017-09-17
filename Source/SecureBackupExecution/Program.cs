using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing;
using WinSCP;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Net.Mail;
using getTicketLib;
using multiLangLib;
using nukeDirLib;
using getParamValueLib;
using memStorageLib;

namespace SecureBackupExecution
{
    public class Program
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
        static string maxBackups;
        static int uploadSpeedLimit;

        //chemin de l'executable
        static string execDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        //dossier de travail
        static string workDir;
        static string uploadDir;

        static void permaMemLock(string name)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes("locked");

            string memFileName;
            int memFileBytes;

            memFileName = name;
            memFileBytes = 1000;
            using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(memFileName, memFileBytes))
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                {
                    do
                    {
                        accessor.WriteArray(0, asciiBytes, 0, asciiBytes.Length);
                        Thread.Sleep(100);
                    } while (true);
                }
            }
        }

        static void error() {
            Process.Start(execDir+@"\backupFailed.bat");
        }

        static void writeToLog(string text, Color color) {
            try {
                Form1.writeToLog(text, color);
            }catch { };
        }

        static List<string> localFilesList = new List<string>();
        static List<string> localDirList = new List<string>();

        //fonction pour obtenir liste recursive des fichiers et dossiers
        public static void customGetFiles(string root)
        {
            try
            {
                // Data structure to hold names of subfolders to be
                // examined for files.
                Stack<string> dirs = new Stack<string>();

                if (!System.IO.Directory.Exists(root))
                {
                    throw new ArgumentException();
                }
                dirs.Push(root);

                while (dirs.Count > 0)
                {
                    string currentDir = dirs.Pop();
                    string[] subDirs;
                    try
                    {
                        subDirs = System.IO.Directory.GetDirectories(currentDir);
                    }
                    // An UnauthorizedAccessException exception will be thrown if we do not have
                    // discovery permission on a folder or file. It may or may not be acceptable 
                    // to ignore the exception and continue enumerating the remaining files and 
                    // folders. It is also possible (but unlikely) that a DirectoryNotFound exception 
                    // will be raised. This will happen if currentDir has been deleted by
                    // another application or thread after our call to Directory.Exists. The 
                    // choice of which exceptions to catch depends entirely on the specific task 
                    // you are intending to perform and also on how much you know with certainty 
                    // about the systems on which this code will run.
                    catch (UnauthorizedAccessException e)
                    {
                        writeToLog(e.Message, Color.Red);
                        continue;
                    }
                    catch (System.IO.DirectoryNotFoundException e)
                    {
                        writeToLog(e.Message, Color.Red);
                        continue;
                    }

                    string[] files = null;
                    try
                    {
                        files = System.IO.Directory.GetFiles(currentDir);
                    }

                    catch (UnauthorizedAccessException e)
                    {

                        writeToLog(e.Message, Color.Red);
                        continue;
                    }

                    catch (System.IO.DirectoryNotFoundException e)
                    {
                        writeToLog(e.Message, Color.Red);
                        continue;
                    }
                    // Perform the required action on each file here.
                    // Modify this block to perform your required task.
                    foreach (string file in files)
                    {
                        try
                        {
                            DateTime date = File.GetLastWriteTime(file);
                            long size = new System.IO.FileInfo(file).Length;
                            if (!file.Contains("$RECYCLE.BIN") && !file.Contains("System Volume Information"))
                                localFilesList.Add((@"\" + file.Replace(source, "")).Replace(@"\\", @"\") + "|" + DateTimeToUnixTimestamp(date) + "|" + size);
                        }
                        catch { }
                    }

                    // Push the subdirectories onto the stack for traversal.
                    // This could also be done before handing the files.
                    foreach (string str in subDirs)
                    {
                        if (!str.Contains("$RECYCLE.BIN") && !str.Contains("System Volume Information"))
                        {
                            localDirList.Add((@"\" + str.Replace(source, "")).Replace(@"\\", @"\"));
                            dirs.Push(str);
                        }
                    }
                }
            }
            catch (Exception er)
            {
                writeToLog(multiLangClass.getText(21) + er, Color.Red);
                error();
                Thread.Sleep(10000);
                Environment.Exit(1);
            }
        }

        static string encryptFile(string file,string pass,string output)
        {
            tryDeleteFileUntilSuccess(output);
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.UseShellExecute = false;
                startInfo.FileName = execDir + @"\7-Zip\7z.exe";
                if(file.EndsWith(".txt")) {
                    startInfo.Arguments = " a \"" + output + "\" -p" + pass + " -mhe -mx9 \"" + file + "\"";
                }else {
                    startInfo.Arguments = " a \"" + output + "\" -p" + pass + " -mhe -mx0 \"" + file + "\"";
                }

                int done=0;
                int tries=6;
                while(done==0) {
                    tries--;
                    var process = Process.Start(startInfo);
                    process.WaitForExit();
                    if(process.ExitCode==0) {
                        done=1;
                    }else {
                        tryDeleteFileUntilSuccess(output);
                        if(tries==0) return "error";
                        writeToLog(multiLangClass.getText(1).Replace("$1",tries.ToString())+" "+ file, Color.Red);
                        Thread.Sleep(1000);
                    }
                }
            }catch (Exception er)
            {
                writeToLog(multiLangClass.getText(2)+": " +er, Color.Red);
                error();
                Thread.Sleep(10000);
                Environment.Exit(1);
            }

            //writeToLog("Process exit code: {0}", process.ExitCode);

            return output;
        }

        static void decryptFile(string file, string pass)
        {
            //chemin de l'executable
            string execDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //string dir = Path.GetDirectoryName(file);

            try {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.UseShellExecute = false;
                startInfo.FileName = execDir + @"\7-Zip\7z.exe";
                startInfo.Arguments = "e \""+ file +"\" -p"+pass+" -o\""+workDir+"\" -y";
                var process = Process.Start(startInfo);
                process.WaitForExit();
                if(process.ExitCode!=0) {
                    error();
                    MessageBox.Show(multiLangClass.getText(3) + " " + file);
                    Environment.Exit(1);
                }
            }catch (Exception er)
            {
                writeToLog(multiLangClass.getText(3)+": " +er, Color.Red);
                error();
                Thread.Sleep(10000);
                Environment.Exit(1);
            }
        }

        static string fileTransferring = "";
        private static void SessionFileTransferProgress(object sender, FileTransferProgressEventArgs e)
        {
            //affichage progrès pour winscp
            try
            {
                if (e.Side== ProgressSide.Local) {
                    Form1.writeToSameLine(multiLangClass.getText(9) + " " + e.FileProgress * 100 + "% " + e.CPS / 1000 + "KB/s " + fileTransferring, Color.Cyan);
                }
                else {
                    Form1.writeToSameLine(multiLangClass.getText(17) + " " + e.FileProgress * 100 + "% " + e.CPS / 1000 + "KB/s " + fileTransferring, Color.Cyan);
                }
                
            }catch{}
        }

        static void uploadFile(string file, Session session, SessionOptions sessionOptions)
        {
            try
            {
                openSessionIfNeeded(session, sessionOptions);


                TransferOptions uploadTransferOptions = new TransferOptions();
                uploadTransferOptions.SpeedLimit = uploadSpeedLimit;

                // Upload files
                session.PutFiles(
                    file,
                    dest+"/"+Path.GetFileName(file),
                    false, //delete
                    uploadTransferOptions
                );
                //writeToLog("");  

                if (!session.FileExists(dest+"/"+Path.GetFileName(file)))  throw new ArgumentNullException();
            }
            catch (Exception er)
            {
                writeToLog(er.ToString(), Color.Red);
                writeToLog(multiLangClass.getText(4) + " " + file, Color.Red);
                error();
                Thread.Sleep(10000);
                Environment.Exit(1);
            }
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        static void tryDeleteFileUntilSuccess(string file)
        {
            while (File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch {
                    Thread.Sleep(100);
                }
                
            }
        }

        static void showWindowWhenMainGUIOpen(Session session) {
            try {
                while (true) {
                    Process[] proc = Process.GetProcessesByName("SecureBackupMainGUI");
                    if (proc.Length != 0) {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new Form1());
                        session.Abort();
                        Environment.Exit(1);
                    }
                    Thread.Sleep(500);
                }
            }catch { }
        }

        static void openSessionIfNeeded(Session session, SessionOptions sessionOptions) {
            try
            {
                if (!session.Opened) session.Open(sessionOptions);
            }
            catch
            {
                error();
                Environment.Exit(1);
            }
        }

        public static string fileFormd5="";

        static string ext = ".new";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string line;
            StreamWriter writer;
            StreamReader logReader;
            string encryptedFile;
            double backupTime = DateTimeToUnixTimestamp(DateTime.Now);
            string execDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 8) {
                Environment.Exit(1);
            }

            multiLangClass.translate();

            ip = args[1];

            port = Convert.ToInt32(args[2]);

            key = args[3];

            user = args[4];

            pass = args[5];

            source = args[6];

            dest = args[7];

            encryptionKey = args[8];

            backupName = args[9];

            maxBackups = args[10];

            //memLock to prevent same backup running in paralel
            ///
                string lockSalt = "secureBackup73zbg937b";

                byte[] asciiBytes = Encoding.ASCII.GetBytes("free");

                string memFileName = backupName + lockSalt;
                int memFileBytes = 1000;
                using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(memFileName, memFileBytes))
                {
                    using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                    {
                        accessor.WriteArray(0, asciiBytes, 0, asciiBytes.Length);
                        Thread.Sleep(200);
                        if (!memStorageClass.getMem(accessor).ToString().Contains("free"))
                        {
                            //MessageBox.Show("locked");
                            Environment.Exit(1);
                        }
                    }
                }

                Task.Run(() => permaMemLock(backupName + lockSalt));
            ///

            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = ip,
                UserName = user,
                Password = pass,
                PortNumber = port
            };

            string argsLine = string.Join(" ", args);

            if (getParamValueClass.getParamValue(argsLine, "<ignoreSSHFingerprint>") == "True")
            {
               sessionOptions.GiveUpSecurityAndAcceptAnySshHostKey = true;
            }
            else
            {
                sessionOptions.SshHostKeyFingerprint = "ssh-rsa 2048 " + key;
            }

            if (getParamValueClass.getParamValue(argsLine, "<uploadLimitParam>") != "<notFound>")
            {
                uploadSpeedLimit = Convert.ToInt32(getParamValueClass.getParamValue(argsLine, "<uploadLimitParam>"));
            }
            else
            {
                uploadSpeedLimit = 0;
            }

            string SSHprivateKey = getParamValueClass.getParamValue(argsLine, "<SSHprivateKey>");
            if (SSHprivateKey != "<notFound>")
            {
                if (SSHprivateKey != "")
                {
                    sessionOptions.SshPrivateKeyPath = SSHprivateKey;
                }
            }

            Session session = new Session();

            session.FileTransferProgress += SessionFileTransferProgress;

            Task.Run( ()=> showWindowWhenMainGUIOpen(session) );

            string localAppdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            workDir = localAppdata + @"\SecureBackupWorkDirBackup " + backupName;
            uploadDir = workDir + @"\uploadDir";

            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir);
            }

            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            session.XmlLogPath = workDir + @"\winSCPLog-"+ Guid.NewGuid() + ".xml";

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

            if (File.Exists(workDir + @"\DbOp"))
            {
                //writeToLog("Écriture corrompue dans les bases de données ! Restauration état correct...");

                //db dossiers
                if (File.Exists(dirDBLocalPath + ".good"))
                {
                    File.Delete(dirDBLocalPath);
                    File.Move(dirDBLocalPath + ".good", dirDBLocalPath);
                }

                //log dossiers
                if (File.Exists(dirLogLocalPath + ".good"))
                {
                    File.Delete(dirLogLocalPath);
                    File.Move(dirLogLocalPath + ".good", dirLogLocalPath);
                }

                //db fichiers
                if (File.Exists(fileDBLocalPath + ".good"))
                {
                    File.Delete(fileDBLocalPath);
                    File.Move(fileDBLocalPath + ".good", fileDBLocalPath);
                }

                //log fichiers
                if (File.Exists(fileLogLocalPath + ".good"))
                {
                    File.Delete(fileLogLocalPath);
                    File.Move(fileLogLocalPath + ".good", fileLogLocalPath);
                }

                File.Delete(workDir + @"\DbOp");
            }

            List<string> remoteFilesListFull = new List<string>();
            List<string> remoteFilesList = new List<string>();
            List<string> remoteFilesListEncryptedFiles = new List<string>();
            StreamReader remoteFilesListReader;
           
            string dirsChanged = workDir + @"\dirsChanged";
            string filesChanged = workDir + @"\filesChanged";

            string newDBsName = "newDBs";
            string newDBsLocal = workDir + "\\" + newDBsName;
            string newDBsRemote = dest + "/" + newDBsName;

            if (!File.Exists(workDir+"\\DBsDownloaded")) {

                openSessionIfNeeded(session, sessionOptions);
                
                //on attend que la voie downloadUpload soit libre
                getTicketClass.getTicket();

                //on utilise les DBs les plus récentes
                ///////////////////////////////////////
                    
                try
                {
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
                }
                catch
                {
	                error();
	                Environment.Exit(1);
                }
                ////////////////////////////////////////
           
                //download des dbs
                ///////////////////////////////
                try
                {
                    string destArchive;
                    string localArchive;
                    string localFile;
                    string changeFile;

                    //db fichiers
                    //param
                    destArchive = fileDBArchiveRemotePath;
                    localArchive = fileDBArchiveLocalPath;
                    localFile = fileDBLocalPath;
                    changeFile = filesChanged;
                    //end param
                    if (session.FileExists(destArchive))
                    {
                        File.Delete(localArchive);
                        fileTransferring=destArchive;
                        session.GetFiles(
                            destArchive,
                            localArchive
                        );
                        writeToLog("", Color.Aqua);
                        decryptFile(localArchive,encryptionKey);
                    }
                    else
                    {
                        File.Create(localFile).Dispose();
                        File.Create(changeFile).Dispose();
                    }


                    //db des dossiers
                    //param
                    destArchive = dirDBArchiveRemotePath;
                    localArchive = dirDBArchiveLocalPath;
                    localFile = dirDBLocalPath;
                    changeFile = dirsChanged;
                    //end param
                    if (session.FileExists(destArchive))
                    {
                        File.Delete(localArchive);
                        fileTransferring=destArchive;
                        session.GetFiles(
                            destArchive,
                            localArchive
                        );
                        writeToLog("", Color.Aqua);
                        decryptFile(localArchive,encryptionKey);
                    }
                    else
                    {
                        File.Create(localFile).Dispose();
                        File.Create(changeFile).Dispose();
                    }


                    //log des dossiers
                    //param
                    destArchive = dirLogArchiveRemotePath;
                    localArchive = dirLogArchiveLocalPath;
                    localFile = dirLogLocalPath;
                    changeFile = dirsChanged;
                    //end param
                    if (session.FileExists(destArchive))
                    {
                        File.Delete(localArchive);
                        fileTransferring=destArchive;
                        session.GetFiles(
                            destArchive,
                            localArchive
                        );
                        writeToLog("", Color.Aqua);
                        decryptFile(localArchive,encryptionKey);
                    }
                    else
                    {
                        File.Create(localFile).Dispose();
                        File.Create(changeFile).Dispose();
                    }

                    //log des fichiers
                    //param
                    destArchive = fileLogArchiveRemotePath;
                    localArchive = fileLogArchiveLocalPath;
                    localFile = fileLogLocalPath;
                    changeFile = filesChanged;
                    //end param
                    if (session.FileExists(destArchive))
                    {
                        File.Delete(localArchive);
                        fileTransferring=destArchive;
                        session.GetFiles(
                            destArchive,
                            localArchive
                        );
                        writeToLog("", Color.Aqua);
                        decryptFile(localArchive,encryptionKey);
                    }
                    else
                    {
                        File.Create(localFile).Dispose();
                        File.Create(changeFile).Dispose();
                    }
                }
                catch
                {
	                error();
	                Environment.Exit(1);
                }
                ///////////////////////////////

                File.Create(workDir + "\\DBsDownloaded").Dispose();

                //On signale que la voie downloadUpload est libre
                ////////////////
                getTicketClass.status=0;
                ///////////////
            }
           
            //liste avec: chemin relatif fichier|date modif fichier non encrypté|nom encrypté fichier|taille fichier non encrypté
            remoteFilesListReader = new StreamReader(fileDBLocalPath);
            //List<string> remoteFilesListFull = new List<string>();
            while ((line = remoteFilesListReader.ReadLine()) != null)
            {
                remoteFilesListFull.Add(line);
            }
            remoteFilesListReader.Close();
            
            //liste avec: chemin relatif fichier|date modif fichier non encrypté|taille fichier non encrypté
            //List<string> remoteFilesList = new List<string>();
            for (int i=0;i<remoteFilesListFull.Count;i++)
            {
                remoteFilesList.Add(remoteFilesListFull[i].Split('|')[0]+"|"+remoteFilesListFull[i].Split('|')[1]+"|"+remoteFilesListFull[i].Split('|')[3]);
                Console.WriteLine(i+"/"+ remoteFilesListFull.Count);
            }
            
            //liste avec: fichier encryptés
            for (int i=0;i<remoteFilesListFull.Count;i++)
            {
                remoteFilesListEncryptedFiles.Add(remoteFilesListFull[i].Split('|')[2]);
            }

            //charge en mémoire les dossiers remote
            StreamReader reader = new StreamReader(dirDBLocalPath);
            List<string> remoteDirs = new List<string>();
            while ((line = reader.ReadLine()) != null)
            {
                remoteDirs.Add(line);
            }
            reader.Close();
           
            writeToLog(multiLangClass.getText(6), Color.Lime);
            customGetFiles(source);

            File.Delete(dirDBLocalPath+".good");
            File.Copy(dirDBLocalPath, dirDBLocalPath + ".good");
            File.Delete(dirLogLocalPath + ".good");
            File.Copy(dirLogLocalPath, dirLogLocalPath + ".good");
            File.Create(workDir+@"\DbOp").Dispose();

                //writeToLog("Écriture liste des dossier locaux...");
                File.WriteAllLines(dirDBLocalPath,localDirList);

                //writeToLog("Écriture nouveaux dossiers dans le log des dossiers...");
            
                foreach (string item in localDirList)
                {
                    if (!remoteDirs.Contains(item)){
                        using (writer = new StreamWriter(dirLogLocalPath, true))
                        { //true = append
                            writeToLog(multiLangClass.getText(7)+" "+item, Color.Lime);
                            writer.WriteLine(backupTime + "|1|" + item);
                            File.Create(dirsChanged).Dispose();
                        }
                    } 
                }

                //writeToLog("Écriture supression de dossiers dans le log des dossiers...");
                foreach (string item in remoteDirs)
                {
                    if (!localDirList.Contains(item))
                    {
                        using (writer = new StreamWriter(dirLogLocalPath, true))
                        { //true = append
                            writeToLog(multiLangClass.getText(8)+" "+item, Color.Fuchsia);
                            writer.WriteLine(backupTime + "|0|" + item);
                            File.Create(dirsChanged).Dispose();
                        }
                    }
                }

            File.Delete(workDir + @"\DbOp");

            //writeToLog("Continuation des uploads...");
            //List<string> noUpload = new List<string> {"output.7z","db.7z","DirDB.7z","dirLog.7z","log.7z" };
            foreach(string file in Directory.GetFiles(uploadDir)) {
                //if(file.EndsWith(".7z") && noUpload.FirstOrDefault(l=>file.Contains(l))==null ) {
                if(remoteFilesListEncryptedFiles.Contains(Path.GetFileName(file))) {
                    //writeToLog(multiLangClass.getText(9)+" " + file, Color.Lime);
                    fileTransferring=file;
                    uploadFile(file,session,sessionOptions);
                    writeToLog("", Color.Aqua);
                    tryDeleteFileUntilSuccess(file);
                }else
                {
                    tryDeleteFileUntilSuccess(file);
                }
            }

            //writeToLog("Identification des fichiers à supprimer...");
            for (int ii = 0; ii < remoteFilesList.Count; ii++)
            {
                string remoteFile = remoteFilesList[ii];
                if (!localFilesList.Contains(remoteFile))
                {
                    File.Delete(fileDBLocalPath + ".good");
                    File.Copy(fileDBLocalPath, fileDBLocalPath + ".good");
                    File.Delete(fileLogLocalPath + ".good");
                    File.Copy(fileLogLocalPath, fileLogLocalPath + ".good");
                    File.Create(workDir + @"\DbOp").Dispose();

                        //supprimer de la db
                        string path = fileDBLocalPath;
                        var oldLines = System.IO.File.ReadAllLines(path);
                        var newLines = oldLines.Where(l => !l.Contains(remoteFilesListFull[ii]));
                        System.IO.File.WriteAllLines(path, newLines);

                        //ajoute suppression au log
                        writer = new StreamWriter(fileLogLocalPath, true); //true = append
                        writer.WriteLine(backupTime + "|0|" + (@"\" + remoteFile.Split('|')[0]).Replace(@"\\", @"\") + "|" + remoteFile.Split('|')[1] + "|" + remoteFilesListFull[ii].Split('|')[2] + "|" + remoteFilesListFull[ii].Split('|')[3]);
                        writer.Close();

                        File.Create(filesChanged).Dispose();

                    File.Delete(workDir + @"\DbOp");

                    writeToLog(multiLangClass.getText(10)+" "+ remoteFile.Split('|')[0], Color.Fuchsia);
                }
            }

            ////taille du dossier distant
            ///////////////////////////////////////////
            //float folderSize=0;
            //try
            //{
            //    RemoteDirectoryInfo RFolders = session.ListDirectory(dest);
            //    for(int i=0;i<RFolders.Files.Count;i++)
            //    {
            //        folderSize += RFolders.Files[i].Length;
            //    }
            //    folderSize=folderSize/1024/1024/1024;
            //}
            //catch
            //{
	           // error();
	           // Environment.Exit(1);
            //}
            /////////////////////////////////////////////

            //writeToLog("Identification des nouveaux fichiers...");
            for(int ii=0;ii<localFilesList.Count;ii++)
            {
                string localFile = localFilesList[ii];


                if (!remoteFilesList.Contains(localFile))
                {
                    string localFilePath=source+localFile.Split('|')[0];
                    //float localFileSize= float.Parse(localFile.Split('|')[2])/1024/1024/1024;

                    if (!File.Exists(localFilePath)) continue;

                    //if (folderSize+localFileSize>maxBackupSize) {
                    //    error();
                    //    MessageBox.Show(multiLangClass.getText(19).Replace("$1",folderSize.ToString()));
                    //    Environment.Exit(1);
                    //}
                    //folderSize+=localFileSize;
                    
                    //ColoredConsoleWrite(System.ConsoleColor.Cyan, "Chiffrement du fichier: " + localFilePath);
                    writeToLog(multiLangClass.getText(11)+" " + localFilePath, Color.Lime);
                    encryptedFile="";
                   
                    encryptedFile = encryptFile(localFilePath, encryptionKey, uploadDir+"\\"+Guid.NewGuid() + ".7z");
                   
                    if (encryptedFile=="error") {
                        //writeToLog(localFilePath+" est utilisé par une autre application. Ignoré.");
                        continue;
                    }

                    File.Delete(fileDBLocalPath + ".good");
                    File.Copy(fileDBLocalPath, fileDBLocalPath + ".good");
                    File.Delete(fileLogLocalPath + ".good");
                    File.Copy(fileLogLocalPath, fileLogLocalPath + ".good");
                    File.Create(workDir + @"\DbOp").Dispose();

                        writer = new StreamWriter(fileDBLocalPath, true); //true = append
                        writer.WriteLine(localFilesList[ii].Split('|')[0].Replace(source, "") + "|"+ localFilesList[ii].Split('|')[1] + "|" + Path.GetFileName(encryptedFile) + "|" + localFilesList[ii].Split('|')[2] );
                        writer.Close();

                        writer = new StreamWriter(fileLogLocalPath, true); //true = append
                        writer.WriteLine(backupTime+"|1|"+ localFilesList[ii].Split('|')[0].Replace(source, "") + "|" + localFilesList[ii].Split('|')[1] + "|" + Path.GetFileName(encryptedFile) + "|" + localFilesList[ii].Split('|')[2] );
                        writer.Close();

                        File.Create(filesChanged).Dispose();

                    File.Delete(workDir + @"\DbOp");

                    fileTransferring= encryptedFile;
                    uploadFile(encryptedFile, session, sessionOptions);
                    writeToLog("", Color.Aqua);
                    tryDeleteFileUntilSuccess(encryptedFile);
                    
                }
            }

            //supression des vieux backups
            if (maxBackups!="0") {
                
                writeToLog(multiLangClass.getText(20), Color.Lime);

                List<double> backupsTimes = new List<double>();

                //log fichiers
                ////////////////////////// 
                List<string> logData = new List<string>();
                logReader = new StreamReader(fileLogLocalPath);
                while ((line = logReader.ReadLine()) != null)
                {
                    backupsTimes.Add(Convert.ToDouble((line.Split('|')[0])));
                    logData.Add(line);        
                }
                logReader.Close();
                ////////////////////////////

                //log dossiers
                //////////////////////////
                List<string> dirLogData = new List<string>();
                List<string> newDirLogData = new List<string>();
                logReader = new StreamReader(dirLogLocalPath);
                while ((line = logReader.ReadLine()) != null)
                {
                    backupsTimes.Add(Convert.ToDouble((line.Split('|')[0])));
                    dirLogData.Add(line);        
                }
                logReader.Close();
                ////////////////////////////

                backupsTimes = backupsTimes.Distinct().ToList();

                backupsTimes.Sort();
                
                if(backupsTimes.Count>int.Parse(maxBackups)) {
                    //writeToLog("Plus de backups que maxBackups");

                    //obtient la date du backup jusqu'a ou supprimer les fichiers et dossiers
                    double time = backupsTimes[backupsTimes.Count-int.Parse(maxBackups)-1];

                    //nettoyage fichiers
                    /////////////////////////////////////
                    for(int ii=0;ii<logData.Count;ii++) {
                        string logEntry = logData[ii];
                        if(double.Parse(logEntry.Split('|')[0])<=time){

                            //supprimme fichier sur serveur si supression
                            if(logEntry.Split('|')[1]=="0"){
                                try
                                {
                                    openSessionIfNeeded(session, sessionOptions);
                                    session.RemoveFiles(
                                        dest + @"/" + logEntry.Split('|')[4]
                                    );
                                }
                                catch
                                {
	                                error();
	                                Environment.Exit(1);
                                }
                            }

                            writeToLog(multiLangClass.getText(10)+" "+ dest + @"/" + logEntry.Split('|')[4], Color.Fuchsia);
                        }
                    }

                    logData = logData.Where(l => double.Parse(l.Split('|')[0])>time).ToList();
                    /////////////////////////////////////////

                    //écriture nouveau log fichier
                    File.Delete(fileLogLocalPath + ".good");
                    File.Copy(fileLogLocalPath, fileLogLocalPath + ".good");
                    File.Create(workDir + @"\DbOp").Dispose();
                        File.Delete(fileLogLocalPath);
                        StreamWriter logWriter = new StreamWriter(fileLogLocalPath,true);
                        for(int ii=0;ii<logData.Count;ii++) {
                            //writeToLog(logData[ii]);
                            logWriter.WriteLine(logData[ii]);
                        }
                        logWriter.Close();
                        File.Create(filesChanged).Dispose();
                    File.Delete(workDir + @"\DbOp");
                    
                    logData.Clear();

                    //nettoyage dossiers
                    /////////////////////////////////////
                    dirLogData = dirLogData.Where(l => double.Parse(l.Split('|')[0])>time ).ToList();     
                    /////////////////////////////////////////

                    //écriture nouveau log dossiers
                    
                    File.Delete(dirLogLocalPath + ".good");
                    File.Copy(dirLogLocalPath, dirLogLocalPath + ".good");
                    File.Create(workDir + @"\DbOp").Dispose();
                        File.Delete(dirLogLocalPath);
                        logWriter = new StreamWriter(dirLogLocalPath,true);
                        for(int ii=0;ii<dirLogData.Count;ii++) {
                            logWriter.WriteLine(dirLogData[ii]);
                        }
                        logWriter.Close();
                        File.Create(dirsChanged).Dispose();
                    File.Delete(workDir + @"\DbOp");
                    
                    dirLogData.Clear();

                } 
            }

            ////identification fichiers manquants dans le backup distant
            ////////////////////////////
            //    //liste avec: chemin relatif fichier|date modif|nom encrypté fichier
            //    remoteFilesListReader = new StreamReader(fileDBLocalPath);
            //    remoteFilesListFull.Clear();
            //    while ((line = remoteFilesListReader.ReadLine()) != null)
            //    {
            //        remoteFilesListFull.Add(line);
            //    }
            //    remoteFilesListReader.Close();

            //    //liste avec: fichier encryptés
            //    remoteFilesListEncryptedFiles.Clear();
            //    for(int i=0;i<remoteFilesListFull.Count;i++)
            //    {
            //        remoteFilesListEncryptedFiles.Add(remoteFilesListFull[i].Split('|')[2]);
            //    }
            
            //    writeToLog(multiLangClass.getText(13), Color.Lime);
            //    try
            //    {
            //        RemoteDirectoryInfo RFolders = session.ListDirectory(dest);
            //        List<string> remoteEncryptedFiles = new List<string>();
            //        for(int i=0;i<RFolders.Files.Count;i++)
            //        {
            //            remoteEncryptedFiles.Add(RFolders.Files[i].ToString());
            //        }
            //        for (int i=0;i<remoteFilesListFull.Count;i++) {
            //            if (!remoteEncryptedFiles.Contains(remoteFilesListEncryptedFiles[i]))
            //            {
            //                MessageBox.Show(multiLangClass.getText(14)+" "+remoteFilesListFull[i]);
            //            }
            //        }
            //    }
            //    catch
            //    {
	           //     error();
	           //     Environment.Exit(1);
            //    }
                
            ////////////////////////////

            //chiffrement dbs
            ////////////////////////
            if (File.Exists(filesChanged)){
                   
                encryptFile(fileDBLocalPath, encryptionKey, workDir+"\\"+fileDBArchiveName+ext);
                encryptFile(fileLogLocalPath, encryptionKey, workDir + "\\" + fileLogArchiveName +ext);
                   
            }

            if (File.Exists(dirsChanged))
            {
                encryptFile(dirDBLocalPath, encryptionKey, workDir + "\\" + dirDBArchiveName + ext);
                encryptFile(dirLogLocalPath, encryptionKey, workDir + "\\" + dirLogArchiveName + ext);
            } 
            /////////////////
            
            //upload dbs
            if ( File.Exists(filesChanged) || File.Exists(dirsChanged) )
            {
                try
                {
                    writeToLog(multiLangClass.getText(18), Color.Lime);

                    //on attend que la voie downloadUpload soit libre
                    getTicketClass.getTicket();

                    //upload dbs
                    //////////////////////
                        openSessionIfNeeded(session, sessionOptions);
                        session.RemoveFiles(newDBsRemote);

                        string upload;

                        if ( File.Exists(filesChanged) ) {
                        
                            //param
                            upload = fileDBArchiveName+ext;
                            //end param
                            fileTransferring=workDir+"\\"+ upload;
                            session.RemoveFiles(dest + "/"+ upload + ".filepart");
                            uploadFile(workDir+"\\"+ upload, session, sessionOptions);
                            writeToLog("", Color.Aqua);

                            //param
                            upload = fileLogArchiveName+ext;
                            //end param
                            fileTransferring=workDir+"\\"+ upload;
                            session.RemoveFiles(dest + "/" + upload + ".filepart");
                            uploadFile(workDir + "\\" + upload, session, sessionOptions);
                            writeToLog("", Color.Aqua);
                        }

                        if ( File.Exists(dirsChanged) ) {
                             //param
                            upload = dirDBArchiveName+ext;
                            //end param
                            fileTransferring=workDir+"\\"+ upload;
                            session.RemoveFiles(dest + "/" + upload + ".filepart");
                            uploadFile(workDir + "\\" + upload, session, sessionOptions);
                            writeToLog("", Color.Aqua);

                            //param
                            upload = dirLogArchiveName+ext;
                            //end param
                            fileTransferring=workDir+"\\"+ upload;
                            session.RemoveFiles(dest + "/" + upload + ".filepart");
                            uploadFile(workDir + "\\" + upload, session, sessionOptions);
                            writeToLog("", Color.Aqua);
                        }
                
                        //finalisation upload des db
                        File.Create(newDBsLocal).Dispose();
                        fileTransferring = newDBsLocal;
                        uploadFile(newDBsLocal, session, sessionOptions);
                        writeToLog("", Color.Aqua);
                    ///////////////////////////

                    //On signale que la voie downloadUpload est libre
                    ////////////////
                    getTicketClass.status=0;
                    ///////////////
                }
                catch
                {
	                error();
	                Environment.Exit(1);
                }
            }

            if (session.Opened) session.Close();

            //File.Create(nukeFile).Dispose();
            //nukeDirClass.nukeDir(workDir);
            //File.Delete(nukeFile);

            File.Delete(filesChanged);
            File.Delete(dirsChanged);

            //supression log winscp abandonnés
            foreach(string file in  Directory.GetFiles(workDir)) {
                if(file.Contains("winSCPLog")) {
                    File.Delete(file);
                }
            }

            writeToLog(multiLangClass.getText(16), Color.Lime);

            Thread.Sleep(1000);
        }
    }
}
