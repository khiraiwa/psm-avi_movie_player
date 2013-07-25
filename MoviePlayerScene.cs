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
        public MoviePlayerScene()
        {
            InitializeWidget();
            this.Button_Play.TouchEventReceived += new EventHandler<TouchEventArgs>(playButtonClick);
            this.Button_Resume.TouchEventReceived += new EventHandler<TouchEventArgs>(resumeButtonClick);
            this.Button_Pause.TouchEventReceived += new EventHandler<TouchEventArgs>(pauseButtonClick);
            this.Button_Stop.TouchEventReceived += new EventHandler<TouchEventArgs>(stopButtonClick);
            this.UriText.Text = "http://localhost/sample.avi";
        }

        bool isPlay;
        bool isResume;
        bool isPause;
        bool isStop;
        private void playButtonClick(object sender, TouchEventArgs e) {
      //      if (e.TouchEventType == TouchEventType.Down) {
            if (!isPlay) {
                isPlay = true;
                isResume = false;
                isPause = false;
                isStop = false;
                this.Button_Resume.Visible = true;
                this.Button_Play.Visible = false;
             //   Sample.PlayerRunner.play(this.EditableText_1.Text);
            }
    //        }

       //     dialog = new BusyIndicatorDialog();
       //     dialog.Show();
           // Sample.PlayerRunner.play();
        }

        private void resumeButtonClick(object sender, TouchEventArgs e) {
            if (!isResume) {
                isPlay = false;
                isResume = true;
                isPause = false;
                isStop = false;
           //     Sample.PlayerRunner.resume();
            }

        }

        private void pauseButtonClick(object sender, TouchEventArgs e) {
            if (isStop) {                return;
            }
            if (!isPause) {
                isPlay = false;
                isResume = false;
                isPause = true;
                isStop = false;
            //    Sample.PlayerRunner.pause();
            }
        }

        private void stopButtonClick(object sender, TouchEventArgs e) {
            if (!isStop) {
                isPlay = false;
                isResume = false;
                isPause = false;
                isStop = true;
                this.Button_Play.Visible = true;
                this.Button_Resume.Visible = false;
            //    Sample.PlayerRunner.stop();
            }
        }

    }
}
