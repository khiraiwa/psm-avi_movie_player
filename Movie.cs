using System;
using System.IO;
using System.Collections.Generic;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Audio;
using Sample;

namespace Avi_Movie_Player
{
    public class Movie : IDisposable
    {
        private Uri targetUri;
        private String fileName;
        private HttpRequestUtil requestUtil;
        private static String MOVIE_FILE_DIR = "/Documents";

        private int width;
        private int height;
        private int microSecPerFrame;
        private int totalFrames;
        private int movi_index;
        private static List<AviOldIndexEntry> audioEntryList;
        private static List<AviOldIndexEntry> videoEntryList;

        private DateTime m_BaseTime;

        private GraphicsContext sm_GraphicsContext = null;
        private Texture2D sm_Texture2D = null;
        private SampleSprite sm_SampleSprite = null;
        private Bgm bgm;
        private BgmPlayer bgmPlayer;

        // Buffer for Sprite (Atmic)
        static object sm_LockObjectForBuffer = new object();
        static byte[] sm_Buffer = null;
        static byte[] Buffer {
            get {
                lock (sm_LockObjectForBuffer) {
                    return sm_Buffer;
                }
            }
            set {
                lock (sm_LockObjectForBuffer) {
                    sm_Buffer = value;
                }
            }
        }

        // 同期関連
        private bool isInitialized;
        public enum State
        {
            None,
            Play,
            Pause,
            Resume,
            Stop
        }
        public State state = State.None;


        public Movie (Uri uri)
        {
            this.targetUri = uri;
            this.fileName = uri.Segments[uri.Segments.Length - 1];
            this.requestUtil = new HttpRequestUtil();
        }

        public void Init(GraphicsContext graphicsContext) {
            sm_GraphicsContext = graphicsContext;
            InitSampleDraw();
        }

        public void Update()
        {
            if (!isInitialized && state == State.Play) {
                InitTexture2D();
                InitSampleSprite();
                isInitialized = true;
            }

            if (state == State.Play) {
                SampleDraw.Update();
                UpdateTexture2D();
            }
        }

        public void Render()
        {
            if (state == State.Play) {
                RenderSprite();
            }
        }

        public void Term()
        {
            TermTexture2D();
            TermSampleSprite();
            TermSampleDraw();
        }

        public void Dispose()
        {
            Term();
        }
        ////////////////////////////////////////////////////////////////
        public void InitSampleDraw()
        {
            if (null == sm_GraphicsContext) {
                return;
            }
            SampleDraw.Init(sm_GraphicsContext);
        }
    
        public void TermSampleDraw()
        {
            SampleDraw.Term();
        }
        ////////////////////////////////////////////////////////////////
        public void InitTexture2D()
        {
            TermTexture2D();
            sm_Texture2D = new Texture2D(width, height, false, PixelFormat.Rgba);
            // Rgba8888 : 4 bytes per pixel
            // Rgb565   : 2 bytes per pixel
            var bytesCount = width * height * 4;
            Buffer = new byte[bytesCount];
            sm_Texture2D.SetPixels(0, Buffer);
        }

        public void UpdateTexture2D()
        {
            if (null == sm_Texture2D)
            {
                return;
            }
            if (null == Buffer)
            {
                return;
            }
            DateTime now = DateTime.Now;
    
            long ival_100ns = now.Ticks - m_BaseTime.Ticks;
            int index = (int)((ival_100ns / 10000000.0) / (microSecPerFrame / 1000000.0));
            Console.WriteLine(index);
            if (totalFrames <= index) {
                index = totalFrames -1;
            }
            Console.WriteLine(index);

            AviOldIndexEntry entry = videoEntryList[index];
            int size = entry.size;
            int offset = entry.offset;
            BinaryReader reader = new BinaryReader(File.OpenRead(MOVIE_FILE_DIR + "/" + fileName));
            reader.BaseStream.Seek(movi_index + 4 + 4 + offset, SeekOrigin.Begin);
            byte[] tmp = reader.ReadBytes(size);
            reader.Close();
            reader.Dispose();

            if (tmp.Length != 0) {
                Image img = new Image(tmp);
                img.Decode();
                Buffer = img.ToBuffer();
                sm_Texture2D.SetPixels(0, Buffer);
                img.Dispose();
            }
        }

        public void TermTexture2D()
        {
            if (null == sm_Texture2D)
            {
                return;
            }
    
            sm_Texture2D.Dispose();
            sm_Texture2D = null;
        }

        ////////////////////////////////////////////////////////////////
        public void InitSampleSprite()
        {
            if (null == sm_Texture2D) {
                return;
            }
            if (0 >= sm_Texture2D.Width || 0 >= sm_Texture2D.Height) {
                return;
            }
    
            TermSampleSprite();
    
            var positionX = (SampleDraw.Width - sm_Texture2D.Width) / 2;
            var positionY = (SampleDraw.Height - sm_Texture2D.Height) / 2;
            var scale =
                Math.Min(
                    (float)SampleDraw.Width / sm_Texture2D.Width,
                    (float)SampleDraw.Height / sm_Texture2D.Height);
    
            sm_SampleSprite = new SampleSprite(sm_Texture2D, positionX, positionY, 0, scale);
        }

        public void RenderSprite()
        {
            if (null == sm_GraphicsContext) {
                return;
            }
            if (null == sm_SampleSprite) {
                return;
            }

            SampleDraw.DrawSprite(sm_SampleSprite);
        }

        public void TermSampleSprite()
        {
            if (null == sm_SampleSprite) {
                return;
            }

            sm_SampleSprite.Dispose();
            sm_SampleSprite = null;
        }
        ////////////////////////////////////////////////////////////////

        private void ReadLocalMovie(String filePath)
        {
            if (!File.Exists(filePath)) {
                Console.WriteLine("{0} does not exist.", filePath);
                return;
            }
            Console.WriteLine("Tick: " + DateTime.Now.Ticks);
            RIFFParser parser = new RIFFParser();
            parser.parse(filePath);
            Console.WriteLine("Tick: " + DateTime.Now.Ticks);
            AviMainHeader mainHeader = parser.getAviMainHeader();
            microSecPerFrame = mainHeader.microSecPerFrame;
            totalFrames = mainHeader.totalFrames;
            width = mainHeader.width;
            height = mainHeader.height;
            AviOldIndex aviOldIndex = parser.getAviOldIndex();
            videoEntryList = aviOldIndex.videoEntry;
            audioEntryList = aviOldIndex.audioEntry;
            movi_index = (int)parser.getMoviIndex();

            Console.WriteLine("Tick: " + DateTime.Now.Ticks);
            WriteMp3Data(MOVIE_FILE_DIR + "/" + fileName);

        }

        private void WriteMp3Data(String filePath)
        {
            FileStream wfs = null;
            byte[] musicBytes = new byte[0];
            BinaryReader reader = new BinaryReader(File.OpenRead(filePath));
            long byteSum = 0;
            try {
                wfs = new FileStream(MOVIE_FILE_DIR + "/" + fileName + ".mp3", FileMode.OpenOrCreate, FileAccess.Write);

                foreach(AviOldIndexEntry entry in audioEntryList) {
                    int size = entry.size;
                    int dataOffset = entry.offset;

                    reader.BaseStream.Seek(movi_index + 4 + 4 + dataOffset, SeekOrigin.Begin);
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

        public void Play()
        {
            if (state != State.Play) {
                if (this.targetUri.Scheme == "http") {
                    this.requestUtil.Completed += this.RequestCallBack;
                    this.requestUtil.downloadFile(targetUri, MOVIE_FILE_DIR + "/" + fileName);
                } else if (this.targetUri.Scheme == "file") {
                    RequestCallBack(this, EventArgs.Empty);
                }
            }
        }

        public void RequestCallBack(object sender, EventArgs e) {
            ReadLocalMovie(MOVIE_FILE_DIR + "/" + fileName);
            bgm = new Bgm(MOVIE_FILE_DIR + "/" + fileName + ".mp3");
            bgmPlayer = bgm.CreatePlayer();
            bgmPlayer.Volume = 1.0F;
            bgmPlayer.Play();
            m_BaseTime = DateTime.Now;
            state = State.Play;
        }

        public void Pause()
        {
            state = State.Pause;
        }

        public void Resume()
        {

            state = State.Resume;
        }

        public void Stop()
        {
            state = State.Stop;
        }
    }
}

