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
        private BusyIndicatorDialog busyDialog;
        private ErrorDialog errorDialog;

        public MoviePlayerScene(MoviePlayer player)
        {
            InitializeWidget();
            this.player = player;
            this.Button_Play.TouchEventReceived += new EventHandler<TouchEventArgs>(playButtonClick);
            this.Button_Resume.TouchEventReceived += new EventHandler<TouchEventArgs>(resumeButtonClick);
            this.Button_Pause.TouchEventReceived += new EventHandler<TouchEventArgs>(pauseButtonClick);
            this.Button_Stop.TouchEventReceived += new EventHandler<TouchEventArgs>(stopButtonClick);
            this.UriText.Text = "file:///Application/contents/output.avi";
            this.busyDialog = new BusyIndicatorDialog();
            this.player.StateChanged += applyStateChaged;
            this.errorDialog = new ErrorDialog();
            this.player.ErrorOccurred += applyErrorOccurred;
        }

        private void playButtonClick(object sender, TouchEventArgs e) {
            State status = player.Status;
            if (status != State.Play) {
                FadeInEffect fadeInEffect = new FadeInEffect(busyDialog, 500, FadeInEffectInterpolator.Linear);
                //fadeInEffect.Start();
                busyDialog.Show(fadeInEffect);

                this.Button_Resume.Visible = true;
                this.Button_Play.Visible = false;
                player.Play(this.UriText.Text);
            }
        }

        private void resumeButtonClick(object sender, TouchEventArgs e) {
            State status = player.Status;
            if (status != State.Resume) {
                player.Resume();
            }

        }

        private void pauseButtonClick(object sender, TouchEventArgs e) {
            State status = player.Status;
            if (status == State.Stop) {
                return;
            }
            if (status != State.Pause) {
                player.Pause();
            }
        }

        private void stopButtonClick(object sender, TouchEventArgs e) {
            State status = player.Status;
            if (status != State.Stop) {
                this.Button_Play.Visible = true;
                this.Button_Resume.Visible = false;
                player.Stop();
            }
        }

        private void applyStateChaged(object sender, StateEventArgs e) {
            if (e.Status == State.Play) {
                closeBusyDialog();
            }
        }

        private void applyErrorOccurred(object sender, ErrorEventArgs e) {
            closeBusyDialog();

            errorDialog.SetText(e.Message);
            errorDialog.OpenDialog();

            this.Button_Play.Visible = true;
            this.Button_Resume.Visible = false;
        }

        private void closeBusyDialog() {
            FadeOutEffect fadeOutEffect = new FadeOutEffect(
            busyDialog, 500, FadeOutEffectInterpolator.Linear);
            fadeOutEffect.Start();
            busyDialog.Hide(fadeOutEffect);
        }
    }
}
