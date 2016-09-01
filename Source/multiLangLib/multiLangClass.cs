using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace multiLangLib
{
    public class multiLangClass
    {
        static List<string> wordsList = new List<string>();
        public static string getText(int id)
        {
            foreach (string word in wordsList)
            {
                if (id.ToString() == word.Split('|')[0])
                {
                    return word.Split('|')[1];
                }
            }
            return "???";
        }

        public static string lang;
        public static string currentLangPath;
        public static void translate()
        {
            string execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string execName = Assembly.GetEntryAssembly().GetName().Name;

            currentLangPath = execDir + @"\lang\currentLang.txt";
            StreamReader reader = new StreamReader(currentLangPath);
            lang = reader.ReadLine();
            reader.Close();

            reader = new StreamReader(execDir + @"\lang\" + execName + ".exe-" + lang + ".txt");
            string line;
            wordsList.Clear();
            while ((line = reader.ReadLine()) != null)
            {
                wordsList.Add(line);
            }
            reader.Close();
        }
    }
}
