﻿using multiLangLib;
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
using System.Threading.Tasks;
using System.Windows.Forms;

//libraire SFTP
using WinSCP;

namespace SecureBackup
{
    public partial class createBackup : Form
    {
        public createBackup()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", "SshHostKeyFingerprint-"+ multiLangClass.lang+".txt");
        }

        //sauvegarde paramètres du backup
        private void button2_Click(object sender, EventArgs e)
        {
            if(encKeyForm.Text.Length<20) {
                MessageBox.Show(multiLangClass.getText(31)+" "+(20-encKeyForm.Text.Length));
                return;
            }

            if(encKeyForm.Text!=encKeyForm2.Text) {
                    MessageBox.Show(multiLangClass.getText(32));
                    return;
            }

            if (sshForm.Text == "") sshForm.Text = "Any";
           
            if (sftpPassForm.Text == "") sftpPassForm.Text = "********************";

            if (!Directory.Exists("backupParams"))
            {
                Directory.CreateDirectory("backupParams");
            }
            string file = "backupParams\\" + textBox1.Text + ".txt";
            if (!File.Exists(file)) File.Create(file).Dispose();

            StreamWriter writer = new StreamWriter(file, false); //true = append
            writer.WriteLine(ipForm.Text+"|"+portForm.Text + "|" + sshForm.Text + "|" + userForm.Text + "|" + sftpPassForm.Text + "|" +sourceForm.Text + "|" +destForm.Text + "|" +encKeyForm.Text + "|" + intervalForm.Text + "|" + autoBackup.Checked + "|" + textBoxMaxBackups.Text+ "|<uploadLimitParam>" + textBoxuploadSpeedLimit.Text+ "<uploadLimitParam>" + "<ignoreSSHFingerprint>" + checkBox1.Checked + "<ignoreSSHFingerprint>" + "<SSHprivateKey>" + textBox2.Text + "<SSHprivateKey>");
            writer.Close();

            //signal queue to recheck auto jobs
            string memFileName = "SecureBackupQueue-Recheck-z34b5923z5";
            int memFileBytes = 10000000;
            MemoryMappedFile mmf;
            mmf = MemoryMappedFile.CreateOrOpen(memFileName, memFileBytes);
            MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
            byte[] asciiBytes = Encoding.ASCII.GetBytes("1");
            accessor.WriteArray(0, asciiBytes, 0, asciiBytes.Length);

            Form1.refresh = true;

            this.Close();
        }

        private void buttonSource_Click(object sender, EventArgs e)
        {
            var dialog = new selectFolder {
                InitialDirectory = @"C:\",
                Title = multiLangClass.getText(33)
            };
            if (dialog.Show(Handle)) {
                sourceForm.Text=dialog.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // options sftp
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = ipForm.Text,
                UserName = userForm.Text,
                Password = sftpPassForm.Text,
                PortNumber = int.Parse(portForm.Text)
            };


            if (checkBox1.Checked)
            {
                sessionOptions.GiveUpSecurityAndAcceptAnySshHostKey = true;
            }
            else
            {
                sessionOptions.SshHostKeyFingerprint = "ssh-rsa 2048 " + sshForm.Text;
            }

            //ssh private key
            if (textBox2.Text != "")
            {
                sessionOptions.SshPrivateKeyPath = textBox2.Text;
            }

            //connexion sftp
            Session session = new Session();

            try
            {
                if (!session.Opened) session.Open(sessionOptions);
            }
            catch
            {
                MessageBox.Show(multiLangClass.getText(34));
                if (session.Opened) session.Close();
                return;
            }

            if (session.Opened) session.Close();
            MessageBox.Show(multiLangClass.getText(35));
        }

        private void createBackup_Load(object sender, EventArgs e)
        {
            //traduction GUI
            label1.Text = multiLangClass.getText(15);
            label2.Text = multiLangClass.getText(16);
            label3.Text = multiLangClass.getText(17);
            label4.Text = multiLangClass.getText(18);
            label5.Text = multiLangClass.getText(19);
            label6.Text = multiLangClass.getText(20);
            label7.Text = multiLangClass.getText(21);
            label8.Text = multiLangClass.getText(22);
            label9.Text = multiLangClass.getText(23);
            label14.Text = multiLangClass.getText(23);
            label10.Text = multiLangClass.getText(24);
            label11.Text = multiLangClass.getText(25);
            label12.Text = multiLangClass.getText(26);
            label13.Text = multiLangClass.getText(27);
            button2.Text = multiLangClass.getText(28);
            button3.Text = multiLangClass.getText(29);
            this.Text = multiLangClass.getText(30);
            label15.Text = multiLangClass.getText(36);
            label16.Text = multiLangClass.getText(27);
            checkBox1.Text = multiLangClass.getText(37);
            label17.Text = multiLangClass.getText(38);
            button5.Text = multiLangClass.getText(39);
        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (FileDialog fileDialog = new OpenFileDialog())
            {
                if (DialogResult.OK == fileDialog.ShowDialog())
                {
                    textBox2.Text = fileDialog.FileName;
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "puttygen.exe";
            var process = Process.Start(startInfo);
        }
    }
}
