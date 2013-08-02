using System;
using System.Threading;
using System.IO;
using Sce.PlayStation.HighLevel.UI;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Imaging;
using Sample;

namespace Avi_Movie_Player
{
    public enum State
    {
        None,
        Play,
        Pause,
        Resume,
        Stop
    }

    public class StateEventArgs : EventArgs
    {
        public State Status;
    }

    public class ErrorEventArgs : EventArgs
    {
        public String Message;
    }

    public class MoviePlayer : IDisposable
    {
        // singleton
        private static MoviePlayer instance = new MoviePlayer();
        private static Movie movie;

        public State Status = State.None;

        private Uri targetUri;
        private String fileName;
        private HttpRequestUtil requestUtil;

        private DateTime m_BaseTime;
        private DateTime m_PauseTime;

        private GraphicsContext sm_GraphicsContext = null;
        private Texture2D sm_Texture2D = null;
        private SampleSprite sm_SampleSprite = null;

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

        public delegate void StateEventHandler(object sender, StateEventArgs e);
        public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
        public event StateEventHandler StateChanged;
        public event ErrorEventHandler ErrorOccurred;


        public static MoviePlayer getInstance(Movie movie) {
            MoviePlayer.movie = movie;
            return instance;
        }

        private MoviePlayer ()
        {
            requestUtil = new HttpRequestUtil();
            requestUtil.Completed += this.startMovie;
            requestUtil.ErrorOccurred += this.applyNetworkError;
        }


        public void Dispose() {
            Term();
        }

        public void Play(String uri) {
            if (Status != State.Play) {
                try {
                    this.targetUri = new Uri(uri);
                } catch(System.UriFormatException ure) {
                    sendErrorOccurred(ure.Message);
                    return;
                }
                this.fileName = targetUri.Segments[targetUri.Segments.Length - 1];
                if (Status == State.None || Status == State.Stop) {
                    String outputDir    = movie.OutputDir;
                    if (targetUri.Scheme == "http") {
                        movie.MovieFileDir = outputDir;
                        requestUtil.DownloadFile(targetUri, outputDir + "/" + fileName);
                    } else if (this.targetUri.Scheme == "file") {
                        String filePath = targetUri.AbsolutePath;
                        int fIndex = filePath.LastIndexOf("/" + fileName);
                        movie.MovieFileDir = filePath.Substring(0, fIndex);
                        Thread thread = new Thread(new ThreadStart(startMovie));
                        thread.Start();
                    } else {
                        sendErrorOccurred("This scheme is unknown.: " + targetUri.Scheme);
                        return;
                    }
                }
            }
            return ;
        }

        public void Stop() {
            if (Status != State.Stop && Status != State.None) {
                movie.AudioStop();
                TermSampleSprite();
                TermTexture2D();

                isInitialized = false;
                Status = State.Stop;
                sendStatusChanged();
            }

            return ;
        }

        public void Pause() {
            if (Status == State.Play || Status == State.Resume) {
                movie.AudioPause();
                m_PauseTime = DateTime.Now;
                Status = State.Pause;
                sendStatusChanged();
            }
            return;
        }

        public void Resume() {
            if (Status == State.Pause) {
                movie.AudioResume();
                m_BaseTime += DateTime.Now - m_PauseTime;
                Status = State.Resume;
                sendStatusChanged();
            }
            return;
        }

        public void Init(GraphicsContext graphicsContext) {
            Status = State.None;
            sm_GraphicsContext = graphicsContext;
            InitSampleDraw();
        }

        public void Update()
        {
            MovieThreadUtil.Run();
            if (!isInitialized && Status == State.Play) {
                InitTexture2D();
                InitSampleSprite();
                movie.AudioPlay();
                isInitialized = true;
            }

            if (Status == State.Play || Status == State.Resume) {
                SampleDraw.Update();
                UpdateTexture2D();
            }
        }

        public void Render()
        {
            if (Status == State.Play || Status == State.Resume || Status == State.Pause) {
                RenderSprite();
            }
        }

        public void Term()
        {
            TermTexture2D();
            TermSampleSprite();
            TermSampleDraw();
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
            sm_Texture2D = new Texture2D(movie.Width, movie.Height, false, PixelFormat.Rgba);
            // Rgba8888 : 4 bytes per pixel
            // Rgb565   : 2 bytes per pixel
            var bytesCount = movie.Width * movie.Height * 4;
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
            int index = (int)((ival_100ns / 10000000.0) / (movie.MicroSecPerFrame / 1000000.0));
            if (movie.TotalFrames <= index) {
                index = movie.TotalFrames -1;
            }

            AviOldIndexEntry entry = movie.VideoEntryList[index];
            int size = entry.Size;
            int offset = entry.Offset;
            BinaryReader reader = new BinaryReader(File.OpenRead(movie.MovieFileDir + "/" + fileName));
            reader.BaseStream.Seek(movie.MoviIndex + 4 + 4 + offset, SeekOrigin.Begin);
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

        private void startMovie(object sender, EventArgs e) {
            startMovie();
        }

        private void applyNetworkError(object sender, ErrorEventArgs e) {
            sendErrorOccurred(e.Message);
        }

        private void startMovie() {
            bool isRead = movie.ReadLocalMovie(movie.MovieFileDir + "/" + fileName);
            if (!isRead) {
                sendErrorOccurred("Local movie file is not found.");
                return;
            }
            m_BaseTime = DateTime.Now;
            Status = State.Play;
            sendStatusChanged();
        }

        private void sendStatusChanged() {
            StateEventArgs e = new StateEventArgs();
            e.Status = Status;
            MovieThreadUtil.InvokeLator(OnStateChanged, e);
        }

        private void sendErrorOccurred(String message) {
            ErrorEventArgs e = new ErrorEventArgs();
            e.Message = message;
            MovieThreadUtil.InvokeLator(OnErrorOccurred, e);
        }

        private void OnStateChanged(StateEventArgs e) {
            if (StateChanged != null) {
                StateChanged(this, e);
            }
        }

        private void OnErrorOccurred(ErrorEventArgs e) {
            if (ErrorOccurred != null) {
                ErrorOccurred(this, e);
            }
        }
    }
}


