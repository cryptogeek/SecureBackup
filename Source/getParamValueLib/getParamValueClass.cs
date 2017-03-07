using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace getParamValueLib
{
    public class getParamValueClass
    {
        public static string getParamValue(String line, string paramName)
        {
            int startOfString = line.IndexOf(paramName);
            if (startOfString > -1)
            {
                int lengthOfString = line.LastIndexOf(paramName) - startOfString;
                return line.Substring(startOfString, lengthOfString).Replace(paramName, "");
            }
            else
            {
                return "<notFound>";
            }
        }
    }
}
