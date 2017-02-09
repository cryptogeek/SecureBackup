using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cleanPathLib
{
    public class cleanPathClass
    {
        public static string cleanPath(string path)
        {
            if (path.Substring(path.Length - 1) == @"\")
                path = path.Replace(@"\", @"\\");
            return path;
        }
    }
}
