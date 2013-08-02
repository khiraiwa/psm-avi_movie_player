using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Audio;
using Sce.PlayStation.HighLevel.UI;

namespace Avi_Movie_Player
{
    public class Movie : IDisposable
    {
        private MoviePlayer player;
        private String fileName;

        private object sm_LockObjectForBgm = new object();
        private Bgm sm_Bgm = null;
        private Bgm bgm {
            get {
                lock (sm_LockObjectForBgm) {
                    return sm_Bgm;
                }
            }
            set {
                lock (sm_LockObjectForBgm) {
                    sm_Bgm = value;
                }
            }
        }

        private object sm_LockObjectForBgmPlayer = new object();
        private BgmPlayer sm_BgmPlayer = null;
        private BgmPlayer bgmPlayer {
            get {
                lock (sm_LockObjectForBgmPlayer) {
                    return sm_BgmPlayer;
                }
            }
            set {
                lock (sm_LockObjectForBgmPlayer) {
                    sm_BgmPlayer = value;
                }
            }
        }

        public String MovieFileDir {
            get;
            set;
        }

        public String OutputDir {
            get;
            set;
        }

        public int Width {
            get;
            private set;
        }
        public int Height {
            get;
            private set;
        }
        public int MicroSecPerFrame {
            get;
            private set;
        }

        public int TotalFrames {
            get;
            private set;
        }
        public int MoviIndex {
            get;
            private set;
        }
        public List<AviOldIndexEntry> AudioEntryList {
            get;
            private set;
        }
        public List<AviOldIndexEntry> VideoEntryList {
            get;
            private set;
        }

        public Movie ()
        {
            MovieFileDir = "/Application/contents";
            OutputDir = "/Documents";
            player = MoviePlayer.getInstance(this);
        }

        public void Init() {

        }

        public void Dispose()
        {
        }


        public MoviePlayer CreatePlayer() {
            return player;
        }

        public bool ReadLocalMovie(String filePath)
        {
            if (!File.Exists(filePath)) {
                Console.WriteLine("{0} does not exist.", filePath);
                return false;
            }
            String[] splits = filePath.Split('/');
            fileName = splits[splits.Length -1];
            int fIndex = filePath.LastIndexOf("/" + fileName);
            MovieFileDir = filePath.Substring(0, fIndex);

            Console.WriteLine("Tick: " + DateTime.Now.Ticks);
            RIFFParser parser = new RIFFParser();
            parser.parse(filePath);
            Console.WriteLine("Tick: " + DateTime.Now.Ticks);
            AviMainHeader mainHeader = parser.AviMainHeader;
            MicroSecPerFrame = mainHeader.MicroSecPerFrame;
            TotalFrames = mainHeader.TotalFrames;
            Width = mainHeader.Width;
            Height = mainHeader.Height;
            AviOldIndex aviOldIndex = parser.AviOldIndex;
            VideoEntryList = aviOldIndex.VideoEntry;
            AudioEntryList = aviOldIndex.AudioEntry;
            MoviIndex = (int)parser.MoviIndex;

            Console.WriteLine("Tick: " + DateTime.Now.Ticks);
            writeMp3Data(MovieFileDir, fileName);
            return true;
        }

        private void writeMp3Data(String outputPath, String fileName)
        {
            FileStream wfs = null;
            byte[] musicBytes = new byte[0];
            BinaryReader reader = new BinaryReader(File.OpenRead(outputPath + "/" + fileName));
            long byteSum = 0;
            try {
                wfs = new FileStream(OutputDir + "/" + fileName + ".mp3", FileMode.OpenOrCreate, FileAccess.Write);

                foreach(AviOldIndexEntry entry in AudioEntryList) {
                    int size = entry.Size;
                    int dataOffset = entry.Offset;

                    reader.BaseStream.Seek(MoviIndex + 4 + 4 + dataOffset, SeekOrigin.Begin);
                    byte[] tmp = reader.ReadBytes(size);
                    BinaryUtil.BinaryWrite(wfs, byteSum, ref tmp);
                    byteSum += size;
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            } finally {
                if (wfs != null) {
                    wfs.Close();
                }
                if (reader != null) {
                    reader.Close();
                    reader.Dispose();
                    Console.WriteLine("endTick: " + DateTime.Now.Ticks);
                }
            }
        }

        public void AudioPlay() {
            bgm = new Bgm(OutputDir + "/" + fileName + ".mp3");
            bgmPlayer = bgm.CreatePlayer();
            bgmPlayer.Volume = 1.0F;
            bgmPlayer.Play();
        }

        public void AudioResume() {
            bgmPlayer.Resume();
        }

        public void AudioPause() {
            bgmPlayer.Pause();
        }

        public void AudioStop() {
            bgmPlayer.Stop();
            bgmPlayer.Dispose();
            bgm.Dispose();
        }
    }
}

