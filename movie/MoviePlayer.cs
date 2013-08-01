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

        public bool Play(String uri) {
            if (Status != State.Play) {
                Status = State.Play;
                movie.Play(uri);
                return true;
            }
            return false;
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

