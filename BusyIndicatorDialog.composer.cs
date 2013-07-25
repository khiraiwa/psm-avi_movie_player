// AUTOMATICALLY GENERATED CODE

using System;
using System.Collections.Generic;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.HighLevel.UI;

namespace Avi_Movie_Player
{
    partial class BusyIndicatorDialog
    {
        Label MessageLabel;
        BusyIndicator BusyIndicator_1;

        private void InitializeWidget()
        {
            InitializeWidget(LayoutOrientation.Horizontal);
        }

        private void InitializeWidget(LayoutOrientation orientation)
        {
            MessageLabel = new Label();
            MessageLabel.Name = "MessageLabel";
            BusyIndicator_1 = new BusyIndicator(true);
            BusyIndicator_1.Name = "BusyIndicator_1";

            // MessageLabel
            MessageLabel.TextColor = new UIColor(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            MessageLabel.Font = new UIFont(FontAlias.System, 25, FontStyle.Regular);
            MessageLabel.LineBreak = LineBreak.Character;

            // BusyIndicatorDialog
            this.AddChildLast(MessageLabel);
            this.AddChildLast(BusyIndicator_1);
            this.ShowEffect = new BunjeeJumpEffect()
            {
            };
            this.HideEffect = new TiltDropEffect();

            SetWidgetLayout(orientation);

            UpdateLanguage();
        }

        private LayoutOrientation _currentLayoutOrientation;
        public void SetWidgetLayout(LayoutOrientation orientation)
        {
            switch (orientation)
            {
                case LayoutOrientation.Vertical:
                    this.SetPosition(0, 0);
                    this.SetSize(544, 960);
                    this.Anchors = Anchors.None;

                    MessageLabel.SetPosition(-11, 30);
                    MessageLabel.SetSize(214, 36);
                    MessageLabel.Anchors = Anchors.None;
                    MessageLabel.Visible = true;

                    BusyIndicator_1.SetPosition(97, 68);
                    BusyIndicator_1.SetSize(48, 48);
                    BusyIndicator_1.Anchors = Anchors.Height | Anchors.Width;
                    BusyIndicator_1.Visible = true;

                    break;

                default:
                    this.SetPosition(0, 0);
                    this.SetSize(400, 100);
                    this.Anchors = Anchors.None;

                    MessageLabel.SetPosition(103, 28);
                    MessageLabel.SetSize(253, 36);
                    MessageLabel.Anchors = Anchors.None;
                    MessageLabel.Visible = true;

                    BusyIndicator_1.SetPosition(35, 22);
                    BusyIndicator_1.SetSize(48, 48);
                    BusyIndicator_1.Anchors = Anchors.Height | Anchors.Width;
                    BusyIndicator_1.Visible = true;

                    break;
            }
            _currentLayoutOrientation = orientation;
        }

        public void UpdateLanguage()
        {
            MessageLabel.Text = "しばらくお待ち下さい";
        }

        private void onShowing(object sender, EventArgs e)
        {
            switch (_currentLayoutOrientation)
            {
                case LayoutOrientation.Vertical:
                    break;

                default:
                    break;
            }
        }

        private void onShown(object sender, EventArgs e)
        {
            switch (_currentLayoutOrientation)
            {
                case LayoutOrientation.Vertical:
                    break;

                default:
                    break;
            }
        }

    }
}
