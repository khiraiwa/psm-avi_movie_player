using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Audio;
using Sample;
using Sce.PlayStation.HighLevel.UI;

namespace Avi_Movie_Player
{
    public class Movie : IDisposable
    {
        private Uri targetUri;
        private String fileName;
        private HttpRequestUtil requestUtil;
        private static String movieFileDir = "/Documents";
        private static String outputDir = "/Documents";

        private int width;
        private int height;
        private int microSecPerFrame;
        private int totalFrames;
        private int movi_index;
        private static List<AviOldIndexEntry> audioEntryList;
        private static List<AviOldIndexEntry> videoEntryList;

        private DateTime m_BaseTime;
        private DateTime m_PauseTime;

        private GraphicsContext sm_GraphicsContext = null;
        private Texture2D sm_Texture2D = null;
        private SampleSprite sm_SampleSprite = null;
        private Bgm bgm;
        private BgmPlayer bgmPlayer;
        private BusyIndicatorDialog dialog = null;
        private ErrorDialog errorDialog = null;

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

        // for synchronization
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
        private MoviePlayer player;

        public Movie ()
        {
            errorDialog = new ErrorDialog();
            requestUtil = new HttpRequestUtil();
            player = MoviePlayer.getInstance(this);
            player.Init();
        }

        public void Init(GraphicsContext graphicsContext) {
            sm_GraphicsContext = graphicsContext;
            InitSampleDraw();
        }

        public void Update()
        {
            MovieThreadUtility.Run();
            if (!isInitialized && state == State.Play) {
                InitTexture2D();
                InitSampleSprite();
                isInitialized = true;
            }

            if (state == State.Play || state == State.Resume) {
                SampleDraw.Update();
                UpdateTexture2D();
            }
        }

        public void Render()
        {
            if (state == State.Play || state == State.Resume || state == State.Pause) {
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
            int size = entry.Size;
            int offset = entry.Offset;
            BinaryReader reader = new BinaryReader(File.OpenRead(movieFileDir + "/" + fileName));
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

        private bool readLocalMovie(String filePath)
        {
            if (!File.Exists(filePath)) {
                Console.WriteLine("{0} does not exist.", filePath);
                showErrorDialog(filePath + " does not exist.");
                return false;
            }
            Console.WriteLine("Tick: " + DateTime.Now.Ticks);
            RIFFParser parser = new RIFFParser();
            parser.parse(filePath);
            Console.WriteLine("Tick: " + DateTime.Now.Ticks);
            AviMainHeader mainHeader = parser.AviMainHeader;
            microSecPerFrame = mainHeader.MicroSecPerFrame;
            totalFrames = mainHeader.TotalFrames;
            width = mainHeader.Width;
            height = mainHeader.Height;
            AviOldIndex aviOldIndex = parser.AviOldIndex;
            videoEntryList = aviOldIndex.VideoEntry;
            audioEntryList = aviOldIndex.AudioEntry;
            movi_index = (int)parser.MoviIndex;

            Console.WriteLine("Tick: " + DateTime.Now.Ticks);
            writeMp3Data(movieFileDir + "/" + fileName);
            return true;
        }

        private void writeMp3Data(String filePath)
        {
            FileStream wfs = null;
            byte[] musicBytes = new byte[0];
            BinaryReader reader = new BinaryReader(File.OpenRead(filePath));
            long byteSum = 0;
            try {
                wfs = new FileStream(outputDir + "/" + fileName + ".mp3", FileMode.OpenOrCreate, FileAccess.Write);

                foreach(AviOldIndexEntry entry in audioEntryList) {
                    int size = entry.Size;
                    int dataOffset = entry.Offset;

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

        public void Play(String uri)
        {
            openDialog();
            try {
                this.targetUri = new Uri(uri);
            } catch(System.UriFormatException ure) {
                showErrorDialog(ure.Message);
                return;
            }
            this.fileName = targetUri.Segments[targetUri.Segments.Length - 1];
            if (state == State.None || state == State.Stop) {
                if (targetUri.Scheme == "http") {
                    movieFileDir = "/Documents";
                    requestUtil.Completed += this.startMovie;
                    requestUtil.DownloadFile(targetUri, outputDir + "/" + fileName);
                } else if (this.targetUri.Scheme == "file") {
                    fileName = this.targetUri.Segments[targetUri.Segments.Length - 1];
                    movieFileDir = targetUri.AbsolutePath.Replace("/" + fileName, "");
                    Thread thread = new Thread(new ThreadStart(startMovie));
                    thread.Start();
                } else {
                    showErrorDialog("This scheme is unknown.: " + targetUri.Scheme);
                    return;
                }
            }
        }

        private void showErrorDialog(String message) {
            MovieThreadUtility.InvokeLator(closeDialog);
            errorDialog.SetText(message);
            MovieThreadUtility.InvokeLator(errorDialog.OpenDialog);
            state = State.Stop;
        }

        private void startMovie(object sender, EventArgs e) {
            startMovie();
        }
        private void startMovie() {
            bool isRead = readLocalMovie(movieFileDir + "/" + fileName);
            if (!isRead) {
                return;
            }
            bgm = new Bgm(outputDir + "/" + fileName + ".mp3");
            bgmPlayer = bgm.CreatePlayer();
            bgmPlayer.Volume = 1.0F;
            bgmPlayer.Play();
            m_BaseTime = DateTime.Now;
            state = State.Play;
            MovieThreadUtility.InvokeLator(closeDialog);
        }

        private void openDialog() {
            dialog = new BusyIndicatorDialog();

            FadeInEffect fadeInEffect = new FadeInEffect(dialog, 500, FadeInEffectInterpolator.Linear);
            fadeInEffect.EffectStopped += handleFadeInEffectEffectStopped;

            //fadeInEffect.Start();
            dialog.Show(fadeInEffect);
        }

        private void closeDialog() {
            FadeOutEffect fadeOutEffect = new FadeOutEffect(
                    dialog, 500, FadeOutEffectInterpolator.Linear);
            fadeOutEffect.EffectStopped += handleFadeOutEffectEffectStopped;
            fadeOutEffect.Start();
            dialog.Hide(fadeOutEffect);
        }

        private void handleFadeOutEffectEffectStopped (object sender, EventArgs e) {
            /*
            bgmPlayer.Play();
            m_BaseTime = DateTime.Now;
            isPlayed = true;
            */
        }
    
        private void handleFadeInEffectEffectStopped (object sender, EventArgs e) {
            /*
            InitTexture2D();
            InitSampleSprite();
            bgm = new Bgm("/Documents/output.mp3");
            bgmPlayer = bgm.CreatePlayer();
            bgmPlayer.Volume = 1.0F;
            */
        }

        public void Pause()
        {
            if (state == State.Play) {
                bgmPlayer.Pause();
                m_PauseTime = DateTime.Now;
    
                state = State.Pause;
            }
        }

        public void Resume()
        {
            if (state == State.Pause) {
                bgmPlayer.Resume();
                m_BaseTime += DateTime.Now - m_PauseTime;

                state = State.Play;
            }
        }

        public void Stop()
        {
            if (state != State.Stop) {
                bgmPlayer.Stop();
                bgmPlayer.Dispose();
                bgm.Dispose();
    
                TermSampleSprite();
                TermTexture2D();
    
                isInitialized = false;
    
                state = State.Stop;
            }
        }

        public MoviePlayer CreatePlayer() {
            return player;
        }
    }
}

