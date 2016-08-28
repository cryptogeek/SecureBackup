using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SecureBackupExecution
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static int previousPos = 0;

        public static void writeToLog(string text, Color color)
        {
            //hideselection to false to get autoscroll

            richTextBox1.AppendText(text, color);
            richTextBox1.AppendText(Environment.NewLine);

            previousPos = richTextBox1.Text.Length;
        }

        public static void writeToSameLine(string text, Color color)
        {
            //if (richTextBox1.Lines.Length != 0)
            //{
            //    //supprime derniere ligne
            //    int lastLineNumber = richTextBox1.Lines.Length - 1;
            //    int start = richTextBox1.GetFirstCharIndexFromLine(lastLineNumber);
            //    int offset = richTextBox1.Lines[lastLineNumber].Length;
            //    richTextBox1.Select(start, offset);
            //    MessageBox.Show("line:"+lastLineNumber +" start:"+ start+" offset:"+ offset+ " selection:"+ richTextBox1.SelectedText);
            //    richTextBox1.SelectedText = "";
            //}

            richTextBox1.Select(previousPos, richTextBox1.SelectionStart);
            richTextBox1.SelectedText = "";

            previousPos = richTextBox1.SelectionStart;

            richTextBox1.AppendText(text, color);
        }

        void hideWindowWhenMainGUIClosed() {  
            while (true) {
                Process[] proc = Process.GetProcessesByName("SecureBackupMainGUI");
                if (proc.Length == 0) {
                   this.Hide();  
                }else {
                   this.Show();
                }
                Thread.Sleep(500);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            this.Text=args[9];   
            Task.Run( ()=>hideWindowWhenMainGUIClosed() );
        }
    }

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
