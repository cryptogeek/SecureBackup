using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace memStorageLib
{
    public class memStorageClass
    {
        public static StringBuilder getMem(MemoryMappedViewAccessor accessor)
        {
            byte byteValue;
            int index = 0;
            StringBuilder message = new StringBuilder();
            do
            {
                byteValue = accessor.ReadByte(index);
                if (byteValue != 0)
                {
                    char asciiChar = (char)byteValue;
                    message.Append(asciiChar);
                }
                index++;
            } while (byteValue != 0);
            return message;
        }
    }
}
