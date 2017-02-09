using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace delEmptyDirsLib
{
    public class delEmptyDirsClass
    {
        //supression dossiers vides
        public static void delEmptyDirs(string startLocation)
        {
            try
            {
                foreach (var directory in Directory.GetDirectories(startLocation))
                {
                    delEmptyDirs(directory);

                    try
                    {
                        if (Directory.GetFiles(directory).Length == 0 &&
                        Directory.GetDirectories(directory).Length == 0)
                        {
                            Directory.Delete(directory, false);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
