using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Avi_Movie_Player
{
    public class RIFFParser
    {
        private long totalOffset = 0;
        private AviMainHeader mainHeader;
        private AviStreamHeader videoStreamHeader;
        private BitmapInfo bitmapInfo;
        private AviStreamHeader audioStreamHeader;
        private WaveFormatEX waveFormatEX;
        private AviOldIndex aviOldIndex;
        private long movi_index;

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
                mainHeader = new AviMainHeader(ref bs, index);
                searchIndex(ref bs, size, ref index, "strh", ref reader);
                                Console.WriteLine(index + totalOffset);
                int strhSize = 56;
                shiftIndex(ref bs, size, ref index, strhSize, ref reader);
                videoStreamHeader = new AviStreamHeader(ref bs, index);
                searchIndex(ref bs, size, ref index, "strf", ref reader);
                                Console.WriteLine(index + totalOffset);
                int bitmapInfoSize = 41;
                shiftIndex(ref bs, size, ref index, bitmapInfoSize, ref reader);
                bitmapInfo = new BitmapInfo(ref bs, index);
                searchIndex(ref bs, size, ref index, "strh", ref reader);
                                Console.WriteLine(index + totalOffset);
                shiftIndex(ref bs, size, ref index, strhSize, ref reader);
                audioStreamHeader = new AviStreamHeader(ref bs, index);
                searchIndex(ref bs, size, ref index, "strf", ref reader);
                                Console.WriteLine(index + totalOffset);
                int waveFormatSize = 24;
                shiftIndex(ref bs, size, ref index, waveFormatSize, ref reader);
                waveFormatEX = new WaveFormatEX(ref bs, index);

            //    mainHeader.print();
            //    videoStreamHeader.print();
            //    bitmapInfo.print();
            //    audioStreamHeader.print();
            //    waveFormatEX.print();
                searchIndex(ref bs, size, ref index, "movi", ref reader);
                this.movi_index = index + totalOffset;

                FileInfo info = new FileInfo(filePath);
                long num = info.Length;
                reader.BaseStream.Seek(num - size, SeekOrigin.Begin);
                bs = reader.ReadBytes(size);
                totalOffset = num - size;
                index = 0;

                searchIndexFromBack(ref bs, size, ref index, "idx1", ref reader);
                int idxSize = 8;
                shiftIndex(ref bs, size, ref index, idxSize, ref reader);
                aviOldIndex = new AviOldIndex(ref bs, index);
                index += idxSize;
                for (;;) {
                    bool end = shiftIndex(ref bs, size, ref index, 16, ref reader);
                    if (!end) {
                         break;
                    }
                    aviOldIndex.addEntrys(ref bs, index);
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
        public AviMainHeader getAviMainHeader() {
            return this.mainHeader;
        }
        public AviStreamHeader getAviVideoStreamHeader() {
            return this.videoStreamHeader;
        }
        public AviStreamHeader getAviAudioStreamHeader() {
            return this.audioStreamHeader;
        }
        public BitmapInfo getBitmapInfo() {
            return this.bitmapInfo;
        }
        public WaveFormatEX getWaveFormatEx() {
            return this.waveFormatEX;
        }
        public AviOldIndex getAviOldIndex() {
            return this.aviOldIndex;
        }
        public long getMoviIndex() {
            return this.movi_index;
        }
    }

    public class AviMainHeader {
        public String fcc;
        public int cb;
        public int microSecPerFrame;
        public int maxBytesPerSec;
        public int paddingGranularity;
        public int flags;
        public int totalFrames;
        public int initialFrames;
        public int streams;
        public int suggestedBufferSize;
        public int width;
        public int height;

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

            fcc = Encoding.ASCII.GetString(dwfcc);
            cb = BitConverter.ToInt32(dwcb, 0);
            microSecPerFrame = BitConverter.ToInt32(dwMicroSecPerFrame, 0);
            maxBytesPerSec = BitConverter.ToInt32(dwMaxBytesPerSec, 0);
            paddingGranularity = BitConverter.ToInt32(dwPaddingGranularity, 0);
            flags = BitConverter.ToInt32(dwFlags, 0);
            totalFrames = BitConverter.ToInt32(dwTotalFrames, 0);
            initialFrames = BitConverter.ToInt32(dwInitialFrames, 0);
            streams = BitConverter.ToInt32(dwStreams, 0);
            suggestedBufferSize = BitConverter.ToInt32(dwSuggestedBufferSize, 0);
            width = BitConverter.ToInt32(dwWidth, 0);
            height = BitConverter.ToInt32(dwHeight, 0);
        }

        public void print() {
            Console.WriteLine("fcc = " + fcc);
            Console.WriteLine("cb = " + cb);
            Console.WriteLine("microSecPerFrame = " + microSecPerFrame);
            Console.WriteLine("maxBytesPerSec = " + maxBytesPerSec);
            Console.WriteLine("paddingGranularity = " + paddingGranularity);
            Console.WriteLine("flags = " + flags);
            Console.WriteLine("totalFrames = " + totalFrames);
            Console.WriteLine("itnitalFrames = " + initialFrames);
            Console.WriteLine("streams = " + streams);
            Console.WriteLine("suggestedBufferSize = " + suggestedBufferSize);
            Console.WriteLine("width = " + width);
            Console.WriteLine("height = " + height);
        }
    }

    public class AviStreamHeader {
        private String fcc;
        private int cb;
        private String fccType;
        private int fccHandler;
        private int flags;
        private short priority;
        private short language;
        private int initialFrames;
        private int scale;
        private int rate;
        private int start;
        private int length;
        private int suggetedBufferSize;
        private int quality;
        private int sampleSize;
        private RcFrame rcFrame;
        class RcFrame {
            private short left;
            private short top;
            private short right;
            private short buttom;
            public RcFrame(ref byte[] contents, long offset) {
                byte[] wleft = {contents[offset++], contents[offset++]};
                byte[] wtop = {contents[offset++], contents[offset++]};
                byte[] wright = {contents[offset++], contents[offset++]};
                byte[] wbottom = {contents[offset++], contents[offset++]};
                left = BitConverter.ToInt16(wleft, 0);
                top = BitConverter.ToInt16(wtop, 0);
                right = BitConverter.ToInt16(wright, 0);
                buttom = BitConverter.ToInt16(wbottom, 0);
            }
            public void print() {
                Console.WriteLine("left = " + left);
                Console.WriteLine("top = " + top);
                Console.WriteLine("right = " + right);
                Console.WriteLine("buttom = " + buttom);
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
            this.rcFrame = new RcFrame(ref contents, offset);

            fcc = Encoding.ASCII.GetString(dwfcc);
            cb = BitConverter.ToInt32(dwcb, 0);
            fccType = Encoding.ASCII.GetString(dwfccType);
            fccHandler = BitConverter.ToInt32(dwfccHandler, 0);
            flags = BitConverter.ToInt32(dwFlags, 0);
            priority = BitConverter.ToInt16(wPriority, 0);
            language = BitConverter.ToInt16(wLanguage, 0);
            initialFrames = BitConverter.ToInt32(dwInitialFrames, 0);
            scale = BitConverter.ToInt32(dwScale, 0);
            rate = BitConverter.ToInt32(dwRate, 0);
            start = BitConverter.ToInt32(dwStart, 0);
            length = BitConverter.ToInt32(dwLength, 0);
            suggetedBufferSize = BitConverter.ToInt32(dwSuggestedBufferSize, 0);
            quality = BitConverter.ToInt32(dwQuality, 0);
            sampleSize = BitConverter.ToInt32(dwSampleSize, 0);
        }
        public void print() {
            Console.WriteLine("fcc = " + fcc);
            Console.WriteLine("cb = " + cb);
            Console.WriteLine("fccType = " + fccType);
            Console.WriteLine("fccHandler = " + fccHandler);
            Console.WriteLine("flags = " + flags);
            Console.WriteLine("priority = " + priority);
            Console.WriteLine("language = " + language);
            Console.WriteLine("initialFrames = " + initialFrames);
            Console.WriteLine("scale = " + scale);
            Console.WriteLine("rate = " + rate);
            Console.WriteLine("start = " + start);
            Console.WriteLine("length = " + length);
            Console.WriteLine("suggetedBufferSize = " + suggetedBufferSize);
            Console.WriteLine("quality = " + quality);
            Console.WriteLine("sampleSize = " + sampleSize);
            this.rcFrame.print();
        }
    }

    /**
     * バグってる
     */
    public class BitmapInfo {
        private BitmapInfoHeader bitmapHeader;
        private RGBQuad rgb;
        class BitmapInfoHeader {
            private int size;
            private int width;
            private int height;
            private short planes;
            private short bitCount;
            private int compression;
            private int sizeImage;
            private int xPelsPerMeter;
            private int yPelsPerMeter;
            private int clrUserd;
            private int clrImportant;

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
                size = BitConverter.ToInt32(biSize, 0);
                width = BitConverter.ToInt32(biWidth, 0);
                height = BitConverter.ToInt32(biHeight, 0);
                planes = BitConverter.ToInt16(biPlanes, 0);
                bitCount = BitConverter.ToInt16(biBitCount, 0);
                compression = BitConverter.ToInt32(biCompression, 0);
                sizeImage = BitConverter.ToInt32(biSizeImage, 0);
                xPelsPerMeter = BitConverter.ToInt32(biXPelsPerMeter, 0);
                yPelsPerMeter = BitConverter.ToInt32(biYPelsPerMeter, 0);
                clrUserd = BitConverter.ToInt32(biClrUsed, 0);
                clrImportant = BitConverter.ToInt32(biClrImportant, 0);
            }

            public void print() {
                Console.WriteLine("size = " + size);
                Console.WriteLine("width = " + width);
                Console.WriteLine("height = " + height);
                Console.WriteLine("planes = " + planes);
                Console.WriteLine("bitCount = " + bitCount);
                Console.WriteLine("compression = " + compression);
                Console.WriteLine("sizeImage = " + sizeImage);
                Console.WriteLine("xPelsPerMeter = " + xPelsPerMeter);
                Console.WriteLine("yPelsPerMeter = " + yPelsPerMeter);
                Console.WriteLine("clrUserd = " + clrUserd);
                Console.WriteLine("clrImportant = " + clrImportant);
            }
        }
        class RGBQuad {
            private byte rgbBlue;
            private byte rgbGreen;
            private byte rgbRed;
            private byte rgbReserved;
            public RGBQuad(ref byte[] contents, long offset) {
                rgbBlue = contents[offset++];
                rgbGreen = contents[offset++];
                rgbRed = contents[offset++];
                rgbReserved = contents[offset++];
            }
            public void print() {
                Console.WriteLine("rgbBlue = " + rgbBlue);
                Console.WriteLine("rgbGreen = " + rgbGreen);
                Console.WriteLine("rgbRed = " + rgbRed);
                Console.WriteLine("rgbReserved = " + rgbReserved);

            }
        }
        public BitmapInfo(ref byte[] contents, long offset) {
            bitmapHeader = new BitmapInfoHeader(ref contents, offset);
            rgb = new RGBQuad(ref contents, offset);
        }
        public void print() {
            bitmapHeader.print();
            rgb.print();
        }
    }

    /**
     * バグってる
     */
    public class WaveFormatEX {
        private short formatTag;
        private short channels;
        private int samplesPerSec;
        private int avgBytesPerSec;
        private short blockAlign;
        private short bitsPerSample;
        private short cbSize;

        public WaveFormatEX(ref byte[] contents, long offset) {
            byte[] wFormatTag = {contents[offset++], contents[offset++]};
            byte[] nChannels = {contents[offset++], contents[offset++]};
            byte[] nSamplesPerSec = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] nAvgBytesPerSec = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] nBlockAlign = {contents[offset++], contents[offset++]};
            byte[] wBitsPerSample = {contents[offset++], contents[offset++]};
            byte[] wcbSize = {contents[offset++], contents[offset++]};

            formatTag = BitConverter.ToInt16(wFormatTag, 0);
            channels = BitConverter.ToInt16(nChannels, 0);
            samplesPerSec = BitConverter.ToInt32(nSamplesPerSec, 0);
            avgBytesPerSec = BitConverter.ToInt32(nAvgBytesPerSec, 0);
            blockAlign = BitConverter.ToInt16(nBlockAlign, 0);
            bitsPerSample = BitConverter.ToInt16(wBitsPerSample, 0);
            cbSize = BitConverter.ToInt16(wcbSize, 0);
        }
        public void print() {
            Console.WriteLine("formatTag = " + formatTag);
            Console.WriteLine("channels = " + channels);
            Console.WriteLine("samplesPerSec = " + samplesPerSec);
            Console.WriteLine("avgBytesPerSec = " + avgBytesPerSec);
            Console.WriteLine("blockAlign = " + blockAlign);
            Console.WriteLine("bitsPerSample = " + bitsPerSample);
            Console.WriteLine("cbSize = " + cbSize);
        }
    }

    public class AviOldIndex {
        private string fcc;
        private int cb;
        public List<AviOldIndexEntry> videoEntry = new List<AviOldIndexEntry>();
        public List<AviOldIndexEntry> audioEntry = new List<AviOldIndexEntry>();

        public AviOldIndex(ref byte[] contents, long offset) {
            byte[] dwfcc = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwcb = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            this.fcc = Encoding.ASCII.GetString(dwfcc);
            this.cb = BitConverter.ToInt32(dwcb, 0);
        }

        public void addEntrys(ref byte[] contents, long offset) {
            AviOldIndexEntry aviOldIndexEntry = new AviOldIndexEntry(ref contents, offset);
            if (aviOldIndexEntry.chunkId.Equals("00dc")) {
                videoEntry.Add(aviOldIndexEntry);
            } else if (aviOldIndexEntry.chunkId.Equals("01wb")) {
                audioEntry.Add(aviOldIndexEntry);
            }
        }

        public void print() {
            Console.WriteLine("fcc = " + fcc);
            Console.WriteLine("cb = " + cb);
            /*
            foreach(AviOldIndexEntry entry in videoEntry) {
                entry.print();
            }
            foreach(AviOldIndexEntry entry in audioEntry) {
                entry.print();
            }
            */
        }

    }

    public class AviOldIndexEntry {
        public String chunkId;
        public int flags;
        public int offset;
        public int size;

        public AviOldIndexEntry(ref byte[] contents, long offset) {
            byte[] dwChunkId = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwFlags = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwOffset = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};
            byte[] dwSize = {contents[offset++], contents[offset++], contents[offset++], contents[offset++]};

            this.chunkId = Encoding.ASCII.GetString(dwChunkId);
            this.flags = BitConverter.ToInt32(dwFlags, 0);
            this.offset = BitConverter.ToInt32(dwOffset, 0);
            this.size = BitConverter.ToInt32(dwSize, 0);
        }

        public void print() {
            Console.WriteLine("chunkId = " + chunkId);
            Console.WriteLine("flags = " + flags);
            Console.WriteLine("offset = " + offset);
            Console.WriteLine("size = " + size);
        }
    }
}