using System;
using System.IO;

namespace Avi_Movie_Player
{
    public class BinaryUtil
    {
        public BinaryUtil ()
        {
        }

        public static void BinaryWrite(FileStream binFileStream, long address, ref byte[] data)
        {
            if (binFileStream != null && address > -1) {
                BinaryWriter binWriter = new BinaryWriter(binFileStream);
                binFileStream.Seek(address, SeekOrigin.Begin);
                binWriter.Write(data);
            }
        }
    }
}

