using delEmptyDirsLib;
using getTicketLib;
using multiLangLib;
using nukeDirLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

//libraire SFTP
using WinSCP;

namespace SecureBackupExplorer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //chemin de l'executable
        static string execDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        static string workDir;

        static string ip;
        static int port;
        static string key;
        static string user;
        static string pass;
        static string source;
        static string dest;
        static string encryptionKey;
        static string backupName;

        static string fileTransferring="";
        private static void SessionFileTransferProgress(object sender, FileTransferProgressEventArgs e)
        {
           //affichage progrès pour winscp
            //textBoxLog.Text = textBoxLog.Text.Remove(textBoxLog.Text.LastIndexOf(Environment.NewLine));
            textBoxLog.AppendText(e.FileProgress*100+"% "+e.CPS/1000+"KB/s "+fileTransferring);
            textBoxLog.AppendText(Environment.NewLine);
            //Console.Write("\r{0:P0} {1}KB/s", e.FileProgress, e.CPS/1000);
            //Form1.textBoxLog.Invoke(new MethodInvoker(delegate
            //{
            //    Form1.textBoxLog.AppendText(e.FileProgress.ToString());
            //    //Form1.progressBar1.Value = (int)((double)totalBytesRead / (size) * 100);
            //}));
        }

        static void decryptFile(string file,string destDir, string pass)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.FileName = execDir + @"\7-Zip\7z.exe";
            startInfo.Arguments = "e \"" + file + "\" -p" + pass + " -o\"" + destDir + "\" -y";
            var process = Process.Start(startInfo);
            process.WaitForExit();
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        static List<double> backupsTimes;
        static List<string> dbItems;
        static List<string> tempView;
        static List<string> logData;
        static List<string> dirs;
        static List<string> dirsLog;
        static List<string> foldersInSelectedBackup;

        static string ext;

        void getBackups()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 7) System.Environment.Exit(0);

            ip = args[1];
            port = Convert.ToInt32(args[2]);
            key = args[3];
            user = args[4];
            pass = args[5];
            source = args[6];
            dest = args[7];
            encryptionKey = args[8];
            backupName = args[9];

            workDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SecureBackupWorkDirRestore " + backupName;

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

            string dirsChanged = workDir + @"\dirsChanged";
            string filesChanged = workDir + @"\filesChanged";

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

            //progrès des transferts
            session.FileTransferProgress += SessionFileTransferProgress;

            //session.ExecutablePath = execDir+@"\WinSCP\WinSCP.exe";

            textBoxLog.AppendText(multiLangClass.getText(1));
            textBoxLog.AppendText(Environment.NewLine);
            session.Open(sessionOptions);

            getTicketClass.getTicket();
            
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

            //download des dbs
            ///////////////////////////////
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
                    decryptFile(localArchive,Path.GetDirectoryName(localArchive),encryptionKey);
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
                    decryptFile(localArchive,Path.GetDirectoryName(localArchive),encryptionKey);
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
                    decryptFile(localArchive,Path.GetDirectoryName(localArchive),encryptionKey);
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
                    decryptFile(localArchive,Path.GetDirectoryName(localArchive),encryptionKey);
                }
                else
                {
                    File.Create(localFile).Dispose();
                    File.Create(changeFile).Dispose();
                }
            ///////////////////////////////

            //On signale que la voie downloadUpload est libre
            getTicketClass.status =0;

            session.Close();

            //charge db dossiers en mémoire
            dirs = new List<string>();
            StreamReader reader = new StreamReader(dirDBLocalPath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                dirs.Add(line);
            }
            reader.Close();

            backupsTimes = new List<double>();

            //charge log dossiers en mémoire
            dirsLog = new List<string>();
            reader = new StreamReader(dirLogLocalPath);
            while ((line = reader.ReadLine()) != null)
            {
                dirsLog.Add(line);
                backupsTimes.Add(Convert.ToDouble((line.Split('|')[0])));
            }
            reader.Close();

            dirsLog.Reverse();

            //chargement db fichiers en mémoire
            dbItems = new List<string>();
            StreamReader remoteFilesListReader = new StreamReader(fileDBLocalPath);
            while ((line = remoteFilesListReader.ReadLine()) != null)
            {
                dbItems.Add(line);
            }
            remoteFilesListReader.Close();

            //chargement log fichiers et dates backups en mémoire
            logData = new List<string>();
            StreamReader logReader = new StreamReader(fileLogLocalPath);
            while ((line = logReader.ReadLine()) != null)
            {
                logData.Add(line);
                backupsTimes.Add(Convert.ToDouble((line.Split('|')[0])));
            }
            logReader.Close();

            logData.Reverse();

            backupsTimes = backupsTimes.Distinct().ToList();

            backupsTimes.Sort();

            backupsTimes.Reverse();

            for(int i=0;i<backupsTimes.Count;i++) {
                DateTime date = UnixTimeStampToDateTime(backupsTimes[i]);
                comboBoxBackups.Items.Add(date);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            getBackups();
        }

        private void loadView(string view)
        {
            textBoxLocation.Text = view;

            listBoxView.Items.Clear();

            if (view != "\\") listBoxView.Items.Add("..\\");

            List<string> locationView = new List<string>();

            //ajoute les fichiers dans l'interface
            for(int i=0;i<tempView.Count;i++) {
                //MessageBox.Show("chemin:"+item);
                
                //chemin après la vue demandée
                string path = tempView[i].Split('|')[0];
                int startIndex = path.IndexOf(view);
                if (startIndex != 0) continue;
                string pathAfterView = path.Substring(view.Length);

                //MessageBox.Show("startIndex:"+startIndex);
                //MessageBox.Show("pathAfterView:"+pathAfterView);

                //nom fichier ou dossier
                int endIndex = pathAfterView.IndexOf("\\");
                string name;
                if (endIndex != -1) //c'est un dossier
                {
                    name = pathAfterView.Substring(0, endIndex+1);
                }
                else
                {
                    name = pathAfterView;
                }

                //MessageBox.Show("name:"+name);

                locationView.Add(name);
            }

            //ajoute les dossiers dans l'interface
            for (int i = 0; i < foldersInSelectedBackup.Count; i++)
            {
                if (foldersInSelectedBackup[i].StartsWith(view))
                {
                    string name = foldersInSelectedBackup[i].Substring(view.Length);
                    if (name.Contains("\\")) name = name.Substring(0, name.IndexOf("\\"));
                    name = name + "\\";
                    locationView.Add(name);
                }
            }

            locationView = locationView.Distinct().ToList();

            locationView.Sort();

            for (int i=0;i<locationView.Count;i++) listBoxView.Items.Add(locationView[i]);
        }

        private void comboBoxBackups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxBackups.SelectedIndex != -1)
            {
                int index = comboBoxBackups.SelectedIndex;

                //fichiers dans le backups séléctionné
                ///////////////////////////////////////////////////
                    tempView = new List<string>(); 
                    for(int i=0;i<dbItems.Count;i++) tempView.Add(dbItems[i]);

                    List<string> toRemove = new List<string>();
                    for(int i=0;i<logData.Count;i++)
                    {
                        string logEntry = logData[i];
                        if (double.Parse(logEntry.Split('|')[0]) > backupsTimes[index])
                        {
                            //MessageBox.Show("entrée plus récente que temps du backup");

                            if (logEntry.Split('|')[1] == "1")
                            {
                                string nameAndDate = logEntry.Split('|')[2]+"|"+logEntry.Split('|')[3];
                                string size = logEntry.Split('|')[5];
                                tempView.RemoveAll(l => l.Contains(nameAndDate) && l.Split('|')[3]==size );
                                //toRemove.Add(nameAndDate);
                            }else{
                                tempView.Add( logEntry.Split('|')[2] + "|"+logEntry.Split('|')[3] + "|"+logEntry.Split('|')[4] + "|"+logEntry.Split('|')[5] );
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    //retire les items de la liste
                    //tempView = tempView.Where(l => toRemove.FirstOrDefault(l.Contains)==null ).ToList();
                ///////////////////////////////////////////////////

                //dossier dans le backups séléctionné
                ///////////////////////////////////////////////////
                    foldersInSelectedBackup = new List<string>(); 
                    for(int i=0;i<dirs.Count;i++) foldersInSelectedBackup.Add(dirs[i]);

                    for(int i=0;i<dirsLog.Count;i++)
                    {
                        string logEntry = dirsLog[i];
                        if (double.Parse(logEntry.Split('|')[0]) > backupsTimes[index])
                        {
                            //MessageBox.Show("plus vieux");

                            if (logEntry.Split('|')[1] == "1")
                            {
                                foldersInSelectedBackup.Remove(logEntry.Split('|')[2]);
                            }else{
                                foldersInSelectedBackup.Add(logEntry.Split('|')[2]);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                ///////////////////////////////////////////////////


                loadView("\\");
            }
        }

        private void listBoxView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //double clic sur un item
            int index = this.listBoxView.IndexFromPoint(e.Location);
            if (index != System.Windows.Forms.ListBox.NoMatches)
            {
                string item = listBoxView.Items[index].ToString();
                if (item == "..\\")
                {
                    //on remonte
                    index = textBoxLocation.Text.LastIndexOf("\\");
                    //MessageBox.Show(index.ToString());
                    index = textBoxLocation.Text.LastIndexOf("\\",index-1)+1;
                    //MessageBox.Show(index.ToString());
                    //MessageBox.Show(textBoxLocation.Text.Substring(0, index));
                    loadView(textBoxLocation.Text.Substring(0,index));
                }
                else
                {
                    if (item.Contains("\\"))
                    {
                        //c'est un dossier
                        textBoxLocation.Text = textBoxLocation.Text + listBoxView.Items[index].ToString();
                        loadView(textBoxLocation.Text);
                    }
                }
            }
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            loadView(textBoxLocation.Text);
        }

        static List<string> localFilesList = new List<string>();

        //fonction pour obtenir liste recursive des fichiers et dossiers
        public static void customGetFiles(string root)
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
                    Console.WriteLine(e.Message);
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(currentDir);
                }

                catch (UnauthorizedAccessException e)
                {

                    Console.WriteLine(e.Message);
                    continue;
                }

                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                // Perform the required action on each file here.
                // Modify this block to perform your required task.
                foreach (string file in files)
                {
                    localFilesList.Add(file);
                }

                // Push the subdirectories onto the stack for traversal.
                // This could also be done before handing the files.
                foreach (string str in subDirs) {
                    dirs.Push(str);
                }
            }
        }


        private void restore()
        {
            buttonRestore.Enabled = false;

            //FolderBrowserDialog fd = new FolderBrowserDialog();
            //if (fd.ShowDialog() == DialogResult.OK) 
            //{
            //    MessageBox.Show(fd.SelectedPath);
            //}

            var dialog = new selectFolder {
                InitialDirectory = @"C:\",
                Title = ""
            };
            if (dialog.Show(Handle)) {
                //MessageBox.Show(dialog.FileName);

                var confirmResult = MessageBox.Show(multiLangClass.getText(3)+" \""+dialog.FileName+"\" ?",multiLangClass.getText(4),MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (confirmResult == DialogResult.No)
                {
                    buttonRestore.Enabled = true; 
                    return;
                }

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

                Session session = new Session();

                session.FileTransferProgress += SessionFileTransferProgress;

                //session.ExecutablePath = execDir+@"\WinSCP\WinSCP.exe";

                List<string> tempViewFreezed = new List<string>();
                foreach (string item in tempView) tempViewFreezed.Add(item);

                List<string> SelectedItemsFreezed = new List<string>();
                foreach (var item in listBoxView.SelectedItems) SelectedItemsFreezed.Add(item.ToString());

                string loc = textBoxLocation.Text;
               
                //restaure les fichiers séléctionnés
                foreach (string item in SelectedItemsFreezed)
                {
                    if (item.ToString() != "..\\")
                    foreach (string file in tempViewFreezed)
                    {
                            if (file.IndexOf(loc+item) == 0)
                            {
                                //a restaurer
                                //MessageBox.Show(file);
                                string destFile = dialog.FileName + file.Split('|')[0];

                                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                                if (File.Exists(destFile) && DateTimeToUnixTimestamp(File.GetLastWriteTime(destFile)).ToString() == file.Split('|')[1] && new System.IO.FileInfo(destFile).Length.ToString()==file.Split('|')[3] ) continue;

                                File.Delete(destFile);

                                if (!session.Opened) session.Open(sessionOptions);

                                
                                //textBoxLog.AppendText("Téléchargement du fichier: "+destFile);
                                string archiveName = file.Split('|')[2];
                                string downloadedArchive = Path.GetDirectoryName(destFile) + "\\" + archiveName;
                                fileTransferring=destFile;
                                session.GetFiles(
                                    dest + "/" + archiveName,
                                    downloadedArchive
                                );
                                textBoxLog.AppendText(multiLangClass.getText(5)+" "+destFile);
                                textBoxLog.AppendText(Environment.NewLine);
                                //textBoxLog.AppendText(Environment.NewLine);
                                decryptFile(downloadedArchive, Path.GetDirectoryName(destFile), encryptionKey);
                                File.Delete(downloadedArchive);

                            }
                    }
                }

                //supression des fichiers pas contenus dans le backup
                confirmResult = MessageBox.Show(multiLangClass.getText(6),"",MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (confirmResult == DialogResult.Yes)
                {
                    localFilesList.Clear();
                    //textBoxLog.AppendText("Génération liste locale des fichiers...");
                    //textBoxLog.AppendText(Environment.NewLine);
                    customGetFiles(dialog.FileName);

                    //textBoxLog.AppendText("Supression des fichiers...");
                    //textBoxLog.AppendText(Environment.NewLine);

                    List<string> localFileListRelative = new List<string>();
                    for (int i = 0; i < localFilesList.Count; i++) localFileListRelative.Add(localFilesList[i].Replace(dialog.FileName, "").ToLower());

                    List<string> tempViewFreezedOnlyRelativePath = new List<string>();
                    for (int i = 0; i < tempViewFreezed.Count; i++) tempViewFreezedOnlyRelativePath.Add(tempViewFreezed[i].Split('|')[0].ToLower());

                    for(int i=0;i<localFilesList.Count;i++)
                    {
                        //textBoxLog.AppendText("fichier: "+file);
                        //textBoxLog.AppendText(Environment.NewLine);
                        string file = localFilesList[i];
                        if(tempViewFreezedOnlyRelativePath.FirstOrDefault(s => s==localFileListRelative[i]) == null)
                        {
                            //envoi fichier dans poubelle
                            //reference ajouté dans propriété du projet vers Microsoft.VisualBasic
                            textBoxLog.AppendText(multiLangClass.getText(7)+" "+file);
                            textBoxLog.AppendText(Environment.NewLine);
                            try
                            {
                                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                                    file,
                                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin,
                                    Microsoft.VisualBasic.FileIO.UICancelOption.ThrowException
                                );
                            }
                            catch { };
                        }

                    }

                    //supression dossiers vides
                    //textBoxLog.AppendText("Supression des dossiers vides...");
                    //textBoxLog.AppendText(Environment.NewLine);
                    delEmptyDirsClass.delEmptyDirs(dialog.FileName);
                }

                //textBoxLog.AppendText("Création des dossiers présents dans le backup...");
                //textBoxLog.AppendText(Environment.NewLine);
                List<string> foldersInSelectedBackupFreezed = new List<string>();
                for (int i = 0; i < foldersInSelectedBackup.Count; i++) foldersInSelectedBackupFreezed.Add(foldersInSelectedBackup[i]);
                foreach (string folder in foldersInSelectedBackupFreezed)
                {
                    //textBoxLog.AppendText(folder);
                    //textBoxLog.AppendText(Environment.NewLine);
                    if (SelectedItemsFreezed.FirstOrDefault(l => (folder+"\\").IndexOf(loc+l) == 0)!=null) {
                        //textBoxLog.AppendText("Création: "+dialog.FileName+folder);
                        //textBoxLog.AppendText(Environment.NewLine);
                        Directory.CreateDirectory(dialog.FileName+folder);
                    }
                }
                

                if (session.Opened) session.Close();
                textBoxLog.AppendText(multiLangClass.getText(8));
                textBoxLog.AppendText(Environment.NewLine);
            }
            buttonRestore.Enabled = true; 
        }

        //restauration
        private void button1_Click(object sender, EventArgs e)
        {
           Task.Run( () => restore() ); //async
           //restore();
        }

        //sélectionner tout les items dans la liste si ctrl+a
        private void listBoxView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                listBoxView.SelectionMode = SelectionMode.MultiSimple;
                for (int i = 0; i < listBoxView.Items.Count; i++)
                    listBoxView.SetSelected(i, true);
                listBoxView.SelectionMode = SelectionMode.MultiExtended;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            nukeDirClass.nukeDir(workDir);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            multiLangClass.translate();

            //traduction GUI
            label1.Text = multiLangClass.getText(9);
            buttonGo.Text = multiLangClass.getText(10);
            buttonRestore.Text = multiLangClass.getText(11);
        }
    }
}
