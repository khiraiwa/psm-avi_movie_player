using System;
using System.Collections.Generic;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.HighLevel.UI;

namespace Avi_Movie_Player
{
    public partial class MoviePlayerScene : Scene
    {
        private MoviePlayer player;

        public MoviePlayerScene(MoviePlayer player)
        {
            InitializeWidget();
            this.player = player;
            this.Button_Play.TouchEventReceived += new EventHandler<TouchEventArgs>(playButtonClick);
            this.Button_Resume.TouchEventReceived += new EventHandler<TouchEventArgs>(resumeButtonClick);
            this.Button_Pause.TouchEventReceived += new EventHandler<TouchEventArgs>(pauseButtonClick);
            this.Button_Stop.TouchEventReceived += new EventHandler<TouchEventArgs>(stopButtonClick);
            this.UriText.Text = "file:///Documents/contents/output.avi";
        }

        private void playButtonClick(object sender, TouchEventArgs e) {
            MoviePlayer.State status = player.Status;
            if (status != MoviePlayer.State.Play) {
                this.Button_Resume.Visible = true;
                this.Button_Play.Visible = false;
                Uri uri = new Uri(this.UriText.Text);
                player.Play(uri);
            }
        }

        private void resumeButtonClick(object sender, TouchEventArgs e) {
            MoviePlayer.State status = player.Status;
            if (status != MoviePlayer.State.Resume) {
                player.Resume();
            }

        }

        private void pauseButtonClick(object sender, TouchEventArgs e) {
            MoviePlayer.State status = player.Status;
            if (status == MoviePlayer.State.Stop) {
                return;
            }
            if (status != MoviePlayer.State.Pause) {
                player.Pause();
            }
        }

        private void stopButtonClick(object sender, TouchEventArgs e) {
            MoviePlayer.State status = player.Status;
            if (status != MoviePlayer.State.Stop) {
                this.Button_Play.Visible = true;
                this.Button_Resume.Visible = false;
                player.Stop();
            }
        }

    }
}
