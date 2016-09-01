using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nukeDirLib
{
    public class nukeDirClass
    {
        //stack overflow si trop de fichiers et dossiers à cause d'appel recursif.
        public static void nukeDir(string path)
        {
            try
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
                foreach (string dir in Directory.GetDirectories(path))
                {
                    try
                    {
                        nukeDir(dir);
                    }
                    catch { }
                }
                Directory.Delete(path);
            }
            catch { }
        }
    }
}
