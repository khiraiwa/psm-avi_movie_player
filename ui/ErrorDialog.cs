using System;
using System.Collections.Generic;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.HighLevel.UI;

namespace Avi_Movie_Player
{
    public partial class ErrorDialog : Dialog
    {
        public ErrorDialog()
            : base(null, null)
        {
            InitializeWidget();
            this.Button_1.TouchEventReceived += new EventHandler<TouchEventArgs>(closeDialog);
        }

        public void SetText(String message) {
            this.Label_1.Text = message;
        }

        public void OpenDialog() {

            FadeInEffect fadeInEffect = new FadeInEffect(this, 500, FadeInEffectInterpolator.Linear);
            //fadeInEffect.Start();
            this.Show(fadeInEffect);
        }

        private void closeDialog(object sender, TouchEventArgs e) {
            FadeOutEffect fadeOutEffect = new FadeOutEffect(
                    this, 500, FadeOutEffectInterpolator.Linear);
            fadeOutEffect.Start();
            this.Hide(fadeOutEffect);
        }
    }
}
