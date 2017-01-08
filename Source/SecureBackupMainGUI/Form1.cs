using memStorageLib;
using multiLangLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecureBackup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void disableButtons()
        {
            buttonDel.Enabled = false;
            buttonEdit.Enabled = false;
            buttonExec.Enabled = false;
            buttonExplo.Enabled = false;
            buttonLog.Enabled = false;

        }

        public void refreshList()
        {
            listBox1.Items.Clear();
            foreach (string file in Directory.GetFiles("backupParams"))
            {
                string path = Path.GetFileName(file);
                path = path.Remove(path.Length - 4);
                listBox1.Items.Add(path);
            }
            disableButtons();
        }

        public static bool refresh = false;
        void cbp_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (refresh) { refreshList(); refresh = false; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            createBackup cbp = new createBackup();
            cbp.FormClosed += new FormClosedEventHandler(cbp_FormClosed);
            cbp.Show();
        }

        private void refreshQueue() {
            string memFileName = "SecureBackupQueue-queue-z34b5923z5";
            //int memFileBytes = 10000000;
            MemoryMappedFile mmf;
            while(true) {
                try
                {
                    mmf = MemoryMappedFile.OpenExisting(memFileName);
                    MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
            
                    //lecture
                    StringBuilder message = memStorageClass.getMem(accessor);

                    //ecriture
                    string messageS = message.Replace("/", "").ToString();
                    labelQueue.Text = messageS;
                }
                catch{}
            
                Thread.Sleep(1000);
            }
        }

        void setGuiLang()
        {
            //traduction Form1
            buttonCreateBackup.Text = multiLangClass.getText(1);
            buttonEdit.Text = multiLangClass.getText(2);
            buttonExec.Text = multiLangClass.getText(3);
            buttonExplo.Text = multiLangClass.getText(4);
            buttonLog.Text = multiLangClass.getText(5);
            buttonDel.Text = multiLangClass.getText(6);
            label2.Text = multiLangClass.getText(7);
            button1.Text = multiLangClass.getText(47);
            button2.Text = multiLangClass.getText(50);
        }

        List<string> availableLangsList = new List<string>();
        static string execDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private void Form1_Load(object sender, EventArgs e)
        {
            //quite si interface déja ouverte
            Process[] processes = Process.GetProcessesByName("SecureBackupMainGUI");
            if (processes.Length > 1)
            {
                Environment.Exit(0);
            }

            multiLangClass.translate();

            setGuiLang();

            //selection langue
            string availableLangsPath = execDir + @"\lang\availableLangs.txt";
            StreamReader reader = new StreamReader(availableLangsPath);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                comboBoxLang.Items.Add(line.Split('|')[0]);
                availableLangsList.Add(line);
            }
            reader.Close();

            foreach (string availableLang in availableLangsList) {
                //MessageBox.Show(availableLang.Split('|')[1] +"=="+ lang);
                if (availableLang.Split('|')[1] == multiLangClass.lang) {
                    comboBoxLang.SelectedItem = availableLang.Split('|')[0];
                }
            }
            
            //démarre la queue si pas demarée
            processes = Process.GetProcessesByName("SecureBackupQueue");
            if (processes.Length < 1)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "SecureBackupQueue.exe";
                var process = Process.Start(startInfo);
            }

            refreshList();

            //Task.Run( () => showConsoles() );

            Task.Run( () => refreshQueue() );
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex!=-1) {
                buttonDel.Enabled = true;
                buttonEdit.Enabled = true;
                buttonExec.Enabled = true;
                buttonExplo.Enabled = true;
                buttonLog.Enabled = true;
            }
            
        }


        private string getParamValue(String line,string paramName)
        {
            int startOfString = line.IndexOf(paramName);
            if (startOfString > -1)
            {
                int lengthOfString = line.LastIndexOf(paramName) - startOfString;
                return line.Substring(startOfString, lengthOfString).Replace(paramName, "");
            }else
            {
                return "<notFound>";
            }
        }

        

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            createBackup cbp = new createBackup();
            cbp.FormClosed += new FormClosedEventHandler(cbp_FormClosed);
            cbp.Show();

            cbp.encKeyForm.Enabled=false;
            cbp.encKeyForm2.Enabled=false;

            StreamReader paramReader = new StreamReader("backupParams\\" + listBox1.SelectedItem.ToString() + ".txt");
            string line;
            while ((line = paramReader.ReadLine()) != null)
            {
                cbp.textBox1.Text = listBox1.SelectedItem.ToString();
                cbp.ipForm.Text = line.Split('|')[0];
                cbp.portForm.Text = line.Split('|')[1];
                cbp.sshForm.Text = line.Split('|')[2];
                cbp.userForm.Text = line.Split('|')[3];
                cbp.sftpPassForm.Text = line.Split('|')[4];
                cbp.sourceForm.Text = line.Split('|')[5];
                cbp.destForm.Text = line.Split('|')[6];
                cbp.encKeyForm.Text = line.Split('|')[7];
                cbp.encKeyForm2.Text = line.Split('|')[7];
                cbp.intervalForm.Text = line.Split('|')[8];
                cbp.autoBackup.Checked = Convert.ToBoolean(line.Split('|')[9]);
                cbp.textBoxMaxBackups.Text=line.Split('|')[10];

                //upload limit
                if (getParamValue(line, "<uploadLimitParam>") != "<notFound>")
                {
                    cbp.textBoxuploadSpeedLimit.Text = getParamValue(line, "<uploadLimitParam>");
                }else
                {
                    cbp.textBoxuploadSpeedLimit.Text = "0";
                }
            }
            paramReader.Close();
 

        }

        private void buttonExec_Click(object sender, EventArgs e)
        {
            string memFileName = "SecureBackupQueue-queue-z34b5923z5";
            int memFileBytes = 1000;
            MemoryMappedFile mmf=null;
            bool ok=false;
            while (!ok)
            {
                try
                {
                    mmf = MemoryMappedFile.CreateOrOpen(memFileName, memFileBytes);
                    ok = true;
                }
                catch
                {
                    //MessageBox.Show("creating new queue memory file...");
                    //mmf = MemoryMappedFile.CreateNew(memFileName, memFileBytes);
                    Thread.Sleep(100);
                }
            }
            MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
            //lecture
            StringBuilder message = memStorageClass.getMem(accessor);
            
            //ecriture
            string messageS = message.Replace("/","") + "|"+listBox1.SelectedItem.ToString()+"|";
            byte[] asciiBytes = Encoding.ASCII.GetBytes(messageS);
            accessor.WriteArray(0, asciiBytes, 0, asciiBytes.Length);
        }

        private void buttonDel_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(multiLangClass.getText(12),multiLangClass.getText(13),MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                File.Delete("backupParams\\" + listBox1.SelectedItem.ToString() + ".txt");
                refreshList();
            }
            
        }

        private void buttonLog_Click(object sender, EventArgs e)
        {
            StreamReader paramReader = new StreamReader("backupParams\\" + listBox1.SelectedItem.ToString() + ".txt");
            string line;
            line = paramReader.ReadLine();
            paramReader.Close();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "SecureBackupShowLog.exe";
            startInfo.Arguments = line.Split('|')[0] + " " + line.Split('|')[1] + " " + line.Split('|')[2] + " " + line.Split('|')[3] + " " + line.Split('|')[4] + " \"" + line.Split('|')[5] + "\" \"" + line.Split('|')[6] + "\" " + line.Split('|')[7] + " \"" + listBox1.SelectedItem.ToString() + "\"";
            var process = Process.Start(startInfo);          
        }

        private void buttonExplo_Click(object sender, EventArgs e)
        {
            StreamReader paramReader = new StreamReader("backupParams\\" + listBox1.SelectedItem.ToString() + ".txt");
            string line;
            line = paramReader.ReadLine();
            paramReader.Close();

            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "SecureBackupExplorer.exe";
            startInfo.Arguments = line.Split('|')[0] + " " + line.Split('|')[1] + " " + line.Split('|')[2] + " " + line.Split('|')[3] + " " + line.Split('|')[4] + " \"" + line.Split('|')[5] + "\" \"" + line.Split('|')[6] + "\" " + line.Split('|')[7] + " \"" + listBox1.SelectedItem.ToString() + "\"";
            var process = Process.Start(startInfo);
        }

        
        int langChanged = 0;
        private void comboBoxLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (langChanged > 0)
            {
                foreach (string availableLang in availableLangsList)
                {
                    if (availableLang.Split('|')[0] == comboBoxLang.SelectedItem.ToString())
                    {
                        StreamWriter writer = new StreamWriter(multiLangClass.currentLangPath, false); //true = append
                        writer.WriteLine(availableLang.Split('|')[1]);
                        writer.Close();
                    }
                }

                multiLangClass.translate();

                setGuiLang();
            }
            langChanged++;
        }

        int servicesOn=1;
        private void button1_Click_1(object sender, EventArgs e)
        {
            if (servicesOn==1) {
                foreach (var process in Process.GetProcessesByName("SecureBackupQueue"))
                {
                    process.Kill();
                }
                foreach (var process in Process.GetProcessesByName("SecureBackupQueuedownloadUpload"))
                {
                    process.Kill();
                }
                foreach (var process in Process.GetProcessesByName("SecureBackupExecution"))
                {
                    process.Kill();
                }
                servicesOn=0;
                button1.Text = multiLangClass.getText(48);
            }
            else {
                
                //démarre la queue si pas demarée
                Process[] processes = Process.GetProcessesByName("SecureBackupQueue");
                if (processes.Length < 1)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "SecureBackupQueue.exe";
                    var process = Process.Start(startInfo);
                }
                servicesOn=1;
                button1.Text = multiLangClass.getText(47);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", "backupFailed.bat");
        }
    }
}
