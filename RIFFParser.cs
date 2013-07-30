using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Avi_Movie_Player
{
    public class RIFFParser
    {
        private long totalOffset = 0;
        public AviMainHeader AviMainHeader
        {
            get;
            private set;
        }
        public AviStreamHeader VideoStreamHeader
        {
            get;
            private set;
        }
        public BitmapInfo BitmapInfo
        {
           　get;
            private set;
        }
        public AviStreamHeader AudioStreamHeader
        {
            get;
            private set;
        }
        public WaveFormatEX WaveFormatEX
        {
            get;
            private set;
        }
        public AviOldIndex AviOldIndex
        {
            get;
            private set;
        }
        public long MoviIndex
        {
            get;
            private set;
        }

        public RIFFParser ()
        {
        }

        private bool findStr(ref byte[] bytes, long index, String str) {
            long index2 = index;
            bool retVal = true;
            foreach (char c in str) {
                if (bytes[index2] != c) {
                    retVal = false;
                    break;
                }
                index2++;
            }
            return retVal;
        }

        private bool shiftIndex(ref byte[] bs, int bsize, ref long index, int targetLength, ref BinaryReader reader) {
            int readSize = 0;
            int size = bs.Length;
            if (size - index < targetLength) { // bsのindex + 残りがtargetよりも小さい場合、読み込み直す
                /*
                for (int i = 0; i < size - index; i++) {
                    bs[i] = bs[index + i];
                }
                 */
                reader.BaseStream.Seek(index + totalOffset, SeekOrigin.Begin);
                reader.BaseStream.Seek(0, SeekOrigin.Current);
                bs = reader.ReadBytes(size);

                readSize = bs.Length;
                totalOffset += (readSize - (size - index));
                if (readSize == 0) { // 終了判定
                    return false;
                } else {
                    Console.WriteLine("readSize = " + readSize);
                }
                index = 0;
            }
            return true;
        }

        private void searchIndex(ref byte[] bs, int bsize, ref long index, String target, ref BinaryReader reader) {
            int size = bs.Length;
            for(;;) {
                shiftIndex(ref bs, bsize, ref index, target.Length, ref reader);

                bool result = findStr(ref bs, index , target);
                if (result == true) {
                    break;
                }
                index++;
            }
        }

        private bool shiftIndexToForward(ref byte[] bs, int bsize, ref long index, int targetLength, ref BinaryReader reader) {
            int readSize = 0;
            int size = bs.Length;
            if (size - index < targetLength) { // bsのindex + 残りがtargetよりも小さい場合、読み込み直す
                reader.BaseStream.Seek(totalOffset - size + (size - index), SeekOrigin.Begin);
                bs = reader.ReadBytes(size);

                readSize = bs.Length;
                totalOffset -= readSize - (size - index);
                if (readSize == 0) { // 終了判定
                    return false;
                } else {
                    Console.WriteLine("readSize = " + readSize);
                }
                index = 0;
            }
            return true;
        }

        private void searchIndexFromBack(ref byte[] bs, int bsize, ref long index, String target, ref BinaryReader reader) {
            int size = bs.Length;
            for(;;) {
                shiftIndexToForward(ref bs, bsize, ref index, target.Length, ref reader);

                bool result = findStr(ref bs, index , target);
                if (result == true) {
                    break;
                }
                index++;
            }
        }

        public void parse(String filePath) {
            BinaryReader reader = null;
            int offset = 0;
            try {
                reader = new BinaryReader(File.OpenRead(filePath));
                int size = 0x1000;
                long index = 0;

                reader.BaseStream.Seek(index + totalOffset, SeekOrigin.Begin);
                byte[] bs = reader.ReadBytes(size);
                int readSize = bs.Length;
                if (readSize == 0) { // 終了判定
                    return;
                }

                offset = size;
                searchIndex(ref bs, size, ref index, "avih", ref reader);
                int avihSize = 56;
                shiftIndex(ref bs, size, ref index, avihSize, ref reader);
                AviMainHeader = new AviMainHeader(ref bs, index);
                searchIndex(ref bs, size, ref index, "strh", ref reader);
                                Console.WriteLine(index + totalOffset);
                int strhSize = 56;
                shiftIndex(ref bs, size, ref index, strhSize, ref reader);
                VideoStreamHeader = new AviStreamHeader(ref bs, index);
                searchIndex(ref bs, size, ref index, "strf", ref reader);
                                Console.WriteLine(index + totalOffset);
                int bitmapInfoSize = 44 + 8;
                shiftIndex(ref bs, size, ref index, bitmapInfoSize, ref reader);
                BitmapInfo = new BitmapInfo(ref bs, index + 8);
                searchIndex(ref bs, size, ref index, "strh", ref reader);
                                Console.WriteLine(index + totalOffset);
                shiftIndex(ref bs, size, ref index, strhSize, ref reader);
                AudioStreamHeader = new AviStreamHeader(ref bs, index);
                searchIndex(ref bs, size, ref index, "strf", ref reader);
                                Console.WriteLine(index + totalOffset);
                int waveFormatSize = 24 + 8;
                shiftIndex(ref bs, size, ref index, waveFormatSize, ref reader);
                WaveFormatEX = new WaveFormatEX(ref bs, index + 8);

                Console.WriteLine("videoheader start");
                VideoStreamHeader.Print();
                Console.WriteLine("videoheader end");
                Console.WriteLine("bitmap start");
                BitmapInfo.Print();
                Console.WriteLine("bitmap end");
                Console.WriteLine("audioheader start");
                AudioStreamHeader.Print();
                Console.WriteLine("audioheader end");
                Console.WriteLine("wave start");
                WaveFormatEX.Print();
                Console.WriteLine("wave end");

                searchIndex(ref bs, size, ref index, "movi", ref reader);
                this.MoviIndex = index + totalOffset;

                FileInfo info = new FileInfo(filePath);
                long num = info.Length;
                reader.BaseStream.Seek(num - size, SeekOrigin.Begin);
                bs = reader.ReadBytes(size);
                totalOffset = num - size;
                index = 0;

                searchIndexFromBack(ref bs, size, ref index, "idx1", ref reader);
                int idxSize = 8;
                shiftIndex(ref bs, size, ref index, idxSize, ref reader);
                AviOldIndex = new AviOldIndex(ref bs, index);
                index += idxSize;
                for (;;) {
                    bool end = shiftIndex(ref bs, size, ref index, 16, ref reader);
                    if (!end) {
                         break;
                    }
                    AviOldIndex.AddEntrys(ref bs, index);
                    index += 16;
                }

             //   aviOldIndex.print();
            } finally {
                if (reader != null) {
                    reader.Close();
                    reader.Dispose();
                }
            }

        }
    }

    public class AviMainHeader {
        public String Fcc
        {
            get;
            private set;
        }
        public int Cb
        {
            get;
            private set;
        }
        public int MicroSecPerFrame
        {
            get;
            private set;
        }
        public int MaxBytesPerSec
        {
            get;
            private set;
        }
        public int PaddingGranularity
        {
            get;
            private set;
        }
        public int Flags
        {
            get;
            private set;
        }
        public int TotalFrames
        {
            get;
            private set;
        }
        public int InitialFrames
        {
            get;
            private set;
        }
        public int Streams
        {
            get;
            private set;
        }
        public int SuggestedBufferSize
        {
            get;
            private set;
        }
        public int Width
        {
            get;
            private set;
        }
        public int Height
        {
            get;
            private set;
        }

        public AviMainHeader(ref byte[] contents, long offset) {
            byte[] dwfcc = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwcb = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwMicroSecPerFrame = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwMaxBytesPerSec = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwPaddingGranularity = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwFlags = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwTotalFrames = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwInitialFrames = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwStreams = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwSuggestedBufferSize = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwWidth = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwHeight = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};

            Fcc = Encoding.ASCII.GetString(dwfcc);
            Cb = BitConverter.ToInt32(dwcb, 0);
            MicroSecPerFrame = BitConverter.ToInt32(dwMicroSecPerFrame, 0);
            MaxBytesPerSec = BitConverter.ToInt32(dwMaxBytesPerSec, 0);
            PaddingGranularity = BitConverter.ToInt32(dwPaddingGranularity, 0);
            Flags = BitConverter.ToInt32(dwFlags, 0);
            TotalFrames = BitConverter.ToInt32(dwTotalFrames, 0);
            InitialFrames = BitConverter.ToInt32(dwInitialFrames, 0);
            Streams = BitConverter.ToInt32(dwStreams, 0);
            SuggestedBufferSize = BitConverter.ToInt32(dwSuggestedBufferSize, 0);
            Width = BitConverter.ToInt32(dwWidth, 0);
            Height = BitConverter.ToInt32(dwHeight, 0);
        }

        public void Print() {
            Console.WriteLine("fcc = " + Fcc);
            Console.WriteLine("cb = " + Cb);
            Console.WriteLine("microSecPerFrame = " + MicroSecPerFrame);
            Console.WriteLine("maxBytesPerSec = " + MaxBytesPerSec);
            Console.WriteLine("paddingGranularity = " + PaddingGranularity);
            Console.WriteLine("flags = " + Flags);
            Console.WriteLine("totalFrames = " + TotalFrames);
            Console.WriteLine("itnitalFrames = " + InitialFrames);
            Console.WriteLine("streams = " + Streams);
            Console.WriteLine("suggestedBufferSize = " + SuggestedBufferSize);
            Console.WriteLine("width = " + Width);
            Console.WriteLine("height = " + Height);
        }
    }

    public class AviStreamHeader {
        public String Fcc
        {
            get;
            private set;
        }
        public int Cb
        {
            get;
            private set;
        }
        public String FccType
        {
            get;
            private set;
        }
        public int FccHandler
        {
            get;
            private set;
        }
        public int Flags
        {
            get;
            private set;
        }
        public short Priority
        {
            get;
            private set;
        }
        public short Language
        {
            get;
            private set;
        }
        public int InitialFrames
        {
            get;
            private set;
        }
        public int Scale
        {
            get;
            private set;
        }
        public int Rate
        {
            get;
            private set;
        }
        public int Start
        {
            get;
            private set;
        }
        public int Length
        {
            get;
            private set;
        }
        public int SuggetedBufferSize
        {
            get;
            private set;
        }
        public int Quality
        {
            get;
            private set;
        }
        public int SampleSize
        {
            get;
            private set;
        }
        public RcFrame RCFrame
        {
            get;
            private set;
        }

        public class RcFrame {
            public short Left
            {
                get;
                private set;
            }
            public short Top
            {
                get;
                private set;
            }
            public short Right
            {
                get;
                private set;
            }
            public short Buttom
            {
                get;
                private set;
            }

            public RcFrame(ref byte[] contents, long offset) {
                byte[] wleft = {contents[offset++], contents[offset++]};
                byte[] wtop = {contents[offset++], contents[offset++]};
                byte[] wright = {contents[offset++], contents[offset++]};
                byte[] wbottom = {contents[offset++], contents[offset++]};
                Left = BitConverter.ToInt16(wleft, 0);
                Top = BitConverter.ToInt16(wtop, 0);
                Right = BitConverter.ToInt16(wright, 0);
                Buttom = BitConverter.ToInt16(wbottom, 0);
            }
            public void Print() {
                Console.WriteLine("left = " + Left);
                Console.WriteLine("top = " + Top);
                Console.WriteLine("right = " + Right);
                Console.WriteLine("buttom = " + Buttom);
            }
        }

        public AviStreamHeader(ref byte[] contents, long offset) {
            byte[] dwfcc = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwcb = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwfccType = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwfccHandler = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwFlags = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] wPriority = {contents[offset++], contents[offset++]};
            byte[] wLanguage = {contents[offset++], contents[offset++]};
            byte[] dwInitialFrames = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwScale = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwRate = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwStart = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwLength = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwSuggestedBufferSize = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwQuality = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwSampleSize = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            this.RCFrame = new RcFrame(ref contents, offset);

            Fcc = Encoding.ASCII.GetString(dwfcc);
            Cb = BitConverter.ToInt32(dwcb, 0);
            FccType = Encoding.ASCII.GetString(dwfccType);
            FccHandler = BitConverter.ToInt32(dwfccHandler, 0);
            Flags = BitConverter.ToInt32(dwFlags, 0);
            Priority = BitConverter.ToInt16(wPriority, 0);
            Language = BitConverter.ToInt16(wLanguage, 0);
            InitialFrames = BitConverter.ToInt32(dwInitialFrames, 0);
            Scale = BitConverter.ToInt32(dwScale, 0);
            Rate = BitConverter.ToInt32(dwRate, 0);
            Start = BitConverter.ToInt32(dwStart, 0);
            Length = BitConverter.ToInt32(dwLength, 0);
            SuggetedBufferSize = BitConverter.ToInt32(dwSuggestedBufferSize, 0);
            Quality = BitConverter.ToInt32(dwQuality, 0);
            SampleSize = BitConverter.ToInt32(dwSampleSize, 0);
        }

        public void Print() {
            Console.WriteLine("fcc = " + Fcc);
            Console.WriteLine("cb = " + Cb);
            Console.WriteLine("fccType = " + FccType);
            Console.WriteLine("fccHandler = " + FccHandler);
            Console.WriteLine("flags = " + Flags);
            Console.WriteLine("priority = " + Priority);
            Console.WriteLine("language = " + Language);
            Console.WriteLine("initialFrames = " + InitialFrames);
            Console.WriteLine("scale = " + Scale);
            Console.WriteLine("rate = " + Rate);
            Console.WriteLine("start = " + Start);
            Console.WriteLine("length = " + Length);
            Console.WriteLine("suggetedBufferSize = " + SuggetedBufferSize);
            Console.WriteLine("quality = " + Quality);
            Console.WriteLine("sampleSize = " + SampleSize);
            this.RCFrame.Print();
        }
    }


    public class BitmapInfo {
        public BitmapInfoHeader BitmapHeader
        {
            get;
            private set;
        }
        public RGBQuad Rgb
        {
            get;
            private set;
        }


        public class BitmapInfoHeader {
            public int Size
            {
                get;
                private set;
            }
            public int Width
            {
                get;
                private set;
            }
            public int Height
            {
                get;
                private set;
            }
            public short Planes
            {
                get;
                private set;
            }
            public short BitCount
            {
                get;
                private set;
            }
            public int Compression
            {
                get;
                private set;
            }
            public int SizeImage
            {
                get;
                private set;
            }
            public int XPelsPerMeter
            {
                get;
                private set;
            }
            public int YPelsPerMeter
            {
                get;
                private set;
            }
            public int ClrUserd
            {
                get;
                private set;
            }
            public int ClrImportant
            {
                get;
                private set;
            }

            public BitmapInfoHeader(ref byte[] contents, long offset) {
                byte[] biSize = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
                byte[] biWidth = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
                byte[] biHeight = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
                byte[] biPlanes = {contents[offset++], contents[offset++]};
                byte[] biBitCount = {contents[offset++], contents[offset++]};
                byte[] biCompression = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
                byte[] biSizeImage = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
                byte[] biXPelsPerMeter = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
                byte[] biYPelsPerMeter = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
                byte[] biClrUsed = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
                byte[] biClrImportant = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
                Size = BitConverter.ToInt32(biSize, 0);
                Width = BitConverter.ToInt32(biWidth, 0);
                Height = BitConverter.ToInt32(biHeight, 0);
                Planes = BitConverter.ToInt16(biPlanes, 0);
                BitCount = BitConverter.ToInt16(biBitCount, 0);
                Compression = BitConverter.ToInt32(biCompression, 0);
                SizeImage = BitConverter.ToInt32(biSizeImage, 0);
                XPelsPerMeter = BitConverter.ToInt32(biXPelsPerMeter, 0);
                YPelsPerMeter = BitConverter.ToInt32(biYPelsPerMeter, 0);
                ClrUserd = BitConverter.ToInt32(biClrUsed, 0);
                ClrImportant = BitConverter.ToInt32(biClrImportant, 0);
            }

            public void Print() {
                Console.WriteLine("size = " + Size);
                Console.WriteLine("width = " + Width);
                Console.WriteLine("height = " + Height);
                Console.WriteLine("planes = " + Planes);
                Console.WriteLine("bitCount = " + BitCount);
                Console.WriteLine("compression = " + Compression);
                Console.WriteLine("sizeImage = " + SizeImage);
                Console.WriteLine("xPelsPerMeter = " + XPelsPerMeter);
                Console.WriteLine("yPelsPerMeter = " + YPelsPerMeter);
                Console.WriteLine("clrUserd = " + ClrUserd);
                Console.WriteLine("clrImportant = " + ClrImportant);
            }
        }

        public class RGBQuad {
            public byte RgbBlue
            {
                get;
                private set;
            }
            public byte RgbGreen
            {
                get;
                private set;
            }
            public byte RgbRed
            {
                get;
                private set;
            }
            public byte RgbReserved
            {
                get;
                private set;
            }

            public RGBQuad(ref byte[] contents, long offset) {
                RgbBlue = contents[offset++];
                RgbGreen = contents[offset++];
                RgbRed = contents[offset++];
                RgbReserved = contents[offset++];
            }

            public void Print() {
                Console.WriteLine("rgbBlue = " + RgbBlue);
                Console.WriteLine("rgbGreen = " + RgbGreen);
                Console.WriteLine("rgbRed = " + RgbRed);
                Console.WriteLine("rgbReserved = " + RgbReserved);

            }
        }

        public BitmapInfo(ref byte[] contents, long offset) {
            BitmapHeader = new BitmapInfoHeader(ref contents, offset);
            Rgb = new RGBQuad(ref contents, offset);
        }

        public void Print() {
            BitmapHeader.Print();
            Rgb.Print();
        }
    }

    public class WaveFormatEX {
        public short FormatTag
        {
            get;
            private set;
        }
        public short Channels
        {
            get;
            private set;
        }
        public int SamplesPerSec
        {
            get;
            private set;
        }
        public int AvgBytesPerSec
        {
            get;
            private set;
        }
        public short BlockAlign
        {
            get;
            private set;
        }
        public short BitsPerSample
        {
            get;
            private set;
        }
        public short CbSize
        {
            get;
            private set;
        }

        public WaveFormatEX(ref byte[] contents, long offset) {
            byte[] wFormatTag = {contents[offset++], contents[offset++]};
            byte[] nChannels = {contents[offset++], contents[offset++]};
            byte[] nSamplesPerSec = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] nAvgBytesPerSec = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] nBlockAlign = {contents[offset++], contents[offset++]};
            byte[] wBitsPerSample = {contents[offset++], contents[offset++]};
            byte[] wcbSize = {contents[offset++], contents[offset++]};

            FormatTag = BitConverter.ToInt16(wFormatTag, 0);
            Channels = BitConverter.ToInt16(nChannels, 0);
            SamplesPerSec = BitConverter.ToInt32(nSamplesPerSec, 0);
            AvgBytesPerSec = BitConverter.ToInt32(nAvgBytesPerSec, 0);
            BlockAlign = BitConverter.ToInt16(nBlockAlign, 0);
            BitsPerSample = BitConverter.ToInt16(wBitsPerSample, 0);
            CbSize = BitConverter.ToInt16(wcbSize, 0);
        }
        public void Print() {
            Console.WriteLine("formatTag = " + FormatTag);
            Console.WriteLine("channels = " + Channels);
            Console.WriteLine("samplesPerSec = " + SamplesPerSec);
            Console.WriteLine("avgBytesPerSec = " + AvgBytesPerSec);
            Console.WriteLine("blockAlign = " + BlockAlign);
            Console.WriteLine("bitsPerSample = " + BitsPerSample);
            Console.WriteLine("cbSize = " + CbSize);
        }
    }

    public class AviOldIndex {
        public string Fcc
        {
            get;
            private set;
        }
        public int Cb
        {
            get;
            private set;
        }
        public List<AviOldIndexEntry> VideoEntry
        {
            get;
            private set;
        }
        public List<AviOldIndexEntry> AudioEntry
        {
            get;
            private set;
        }

        public AviOldIndex(ref byte[] contents, long offset) {
            this.VideoEntry = new List<AviOldIndexEntry>();
            this.AudioEntry = new List<AviOldIndexEntry>();
            byte[] dwfcc = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwcb = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            this.Fcc = Encoding.ASCII.GetString(dwfcc);
            this.Cb = BitConverter.ToInt32(dwcb, 0);
        }

        public void AddEntrys(ref byte[] contents, long offset) {
            AviOldIndexEntry aviOldIndexEntry = new AviOldIndexEntry(ref contents, offset);
            if (aviOldIndexEntry.ChunkId.Equals("00dc")) {
                VideoEntry.Add(aviOldIndexEntry);
            } else if (aviOldIndexEntry.ChunkId.Equals("01wb")) {
                AudioEntry.Add(aviOldIndexEntry);
            }
        }

        public void Print() {
            Console.WriteLine("fcc = " + Fcc);
            Console.WriteLine("cb = " + Cb);
            foreach(AviOldIndexEntry entry in VideoEntry) {
                entry.Print();
            }
            foreach(AviOldIndexEntry entry in AudioEntry) {
                entry.Print();
            }
        }
    }

    public class AviOldIndexEntry {
        public String ChunkId
        {
            get;
            private set;
        }
        public int Flags
        {
            get;
            private set;
        }
        public int Offset
        {
            get;
            private set;
        }
        public int Size
        {
            get;
            private set;
        }

        public AviOldIndexEntry(ref byte[] contents, long offset) {
            byte[] dwChunkId = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwFlags = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwOffset = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwSize = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};

            this.ChunkId = Encoding.ASCII.GetString(dwChunkId);
            this.Flags = BitConverter.ToInt32(dwFlags, 0);
            this.Offset = BitConverter.ToInt32(dwOffset, 0);
            this.Size = BitConverter.ToInt32(dwSize, 0);
        }

        public void Print() {
            Console.WriteLine("chunkId = " + ChunkId);
            Console.WriteLine("flags = " + Flags);
            Console.WriteLine("offset = " + Offset);
            Console.WriteLine("size = " + Size);
        }
    }
}