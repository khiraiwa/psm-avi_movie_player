using System;
using System.Threading;
using Sce.PlayStation.HighLevel.UI;

namespace Avi_Movie_Player
{
    public class MoviePlayer : IDisposable
    {
        private static MoviePlayer instance = new MoviePlayer();
        private static Movie movie;
        private BusyIndicatorDialog dialog = null;

        public enum State
        {
            None,
            Play,
            Pause,
            Resume,
            Stop
        }
        public State Status = State.None;

        public static MoviePlayer getInstance(Movie movie) {
            MoviePlayer.movie = movie;
            return instance;
        }

        private MoviePlayer ()
        {
        }

        public void Init() {
            Status = State.None;
        }
        public void Dispose() {

        }

        public bool Play(Uri uri) {
            if (Status != State.Play) {
                Status = State.Play;
               // openDialog();

                movie.Play(uri);

                return true;
            }
            return false;
        }

        private void startMovie(object sender, EventArgs e) {
            startMovie();
        }
        private void startMovie() {
            /*
            bool isReadSuccess = false;
            try {
                isReadSuccess = readLocalMovie(movieFileDir + "/" + fileName);
            } catch (System.IndexOutOfRangeException ioe) {
                Console.WriteLine(ioe);
                closeDialog();
                return;
            }
            if (!isReadSuccess) {
                closeDialog();
                return;
            }

            bgm = new Bgm(outputDir + "/" + fileName + ".mp3");
            bgmPlayer = bgm.CreatePlayer();
            bgmPlayer.Volume = 1.0F;
            bgmPlayer.Play();
            m_BaseTime = DateTime.Now;
            state = State.Play;
            */
        }

        private void openDialog() {
            dialog = new BusyIndicatorDialog();
            FadeInEffect fadeInEffect = new FadeInEffect(dialog, 500, FadeInEffectInterpolator.Linear);

            //fadeInEffect.Start();
            dialog.Show(fadeInEffect);
        }

        private void closeDialog() {
            FadeOutEffect fadeOutEffect = new FadeOutEffect(
                    dialog, 500, FadeOutEffectInterpolator.Linear);
            fadeOutEffect.Start();
            dialog.Hide(fadeOutEffect);
        }


        public bool Stop() {
            if (Status != State.Stop && Status != State.None) {
                Status = State.Stop;
                movie.Stop();
                return true;
            }
            return false;
        }

        public bool Pause() {
            if (Status == State.Play) {
                Status = State.Pause;
                movie.Pause();
                return true;
            }
            return false;
        }

        public bool Resume() {

            if (Status == State.Pause) {
                Status = State.Play;
                movie.Resume();
                return true;
            }

            return false;
        }
    }
}

