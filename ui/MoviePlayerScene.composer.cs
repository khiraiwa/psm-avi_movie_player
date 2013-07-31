// AUTOMATICALLY GENERATED CODE

using System;
using System.Collections.Generic;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.HighLevel.UI;

namespace Avi_Movie_Player
{
    partial class MoviePlayerScene
    {
        EditableText UriText;
        Label UriLabel;
        Button Button_Play;
        Button Button_Stop;
        Button Button_Resume;
        Button Button_Pause;

        private void InitializeWidget()
        {
            InitializeWidget(LayoutOrientation.Horizontal);
        }

        private void InitializeWidget(LayoutOrientation orientation)
        {
            UriText = new EditableText();
            UriText.Name = "UriText";
            UriLabel = new Label();
            UriLabel.Name = "UriLabel";
            Button_Play = new Button();
            Button_Play.Name = "Button_Play";
            Button_Stop = new Button();
            Button_Stop.Name = "Button_Stop";
            Button_Resume = new Button();
            Button_Resume.Name = "Button_Resume";
            Button_Pause = new Button();
            Button_Pause.Name = "Button_Pause";

            // UriText
            UriText.TextColor = new UIColor(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            UriText.Font = new UIFont(FontAlias.System, 25, FontStyle.Regular);
            UriText.LineBreak = LineBreak.Character;

            // UriLabel
            UriLabel.TextColor = new UIColor(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            UriLabel.Font = new UIFont(FontAlias.System, 25, FontStyle.Regular);
            UriLabel.LineBreak = LineBreak.Character;

            // Button_Play
            Button_Play.TextColor = new UIColor(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            Button_Play.TextFont = new UIFont(FontAlias.System, 25, FontStyle.Regular);

            // Button_Stop
            Button_Stop.TextColor = new UIColor(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            Button_Stop.TextFont = new UIFont(FontAlias.System, 25, FontStyle.Regular);

            // Button_Resume
            Button_Resume.TextColor = new UIColor(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            Button_Resume.TextFont = new UIFont(FontAlias.System, 25, FontStyle.Regular);

            // Button_Pause
            Button_Pause.TextColor = new UIColor(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f);
            Button_Pause.TextFont = new UIFont(FontAlias.System, 25, FontStyle.Regular);

            // MoviePlayerScene
            this.RootWidget.AddChildLast(UriText);
            this.RootWidget.AddChildLast(UriLabel);
            this.RootWidget.AddChildLast(Button_Play);
            this.RootWidget.AddChildLast(Button_Stop);
            this.RootWidget.AddChildLast(Button_Resume);
            this.RootWidget.AddChildLast(Button_Pause);

            SetWidgetLayout(orientation);

            UpdateLanguage();
        }

        private LayoutOrientation _currentLayoutOrientation;
        public void SetWidgetLayout(LayoutOrientation orientation)
        {
            switch (orientation)
            {
                case LayoutOrientation.Vertical:
                    this.DesignWidth = 544;
                    this.DesignHeight = 960;

                    UriText.SetPosition(43, 22);
                    UriText.SetSize(360, 56);
                    UriText.Anchors = Anchors.None;
                    UriText.Visible = true;

                    UriLabel.SetPosition(-46, 35);
                    UriLabel.SetSize(214, 36);
                    UriLabel.Anchors = Anchors.None;
                    UriLabel.Visible = true;

                    Button_Play.SetPosition(769, 56);
                    Button_Play.SetSize(214, 56);
                    Button_Play.Anchors = Anchors.None;
                    Button_Play.Visible = true;

                    Button_Stop.SetPosition(798, 7);
                    Button_Stop.SetSize(214, 56);
                    Button_Stop.Anchors = Anchors.None;
                    Button_Stop.Visible = true;

                    Button_Resume.SetPosition(699, 90);
                    Button_Resume.SetSize(214, 56);
                    Button_Resume.Anchors = Anchors.None;
                    Button_Resume.Visible = true;

                    Button_Pause.SetPosition(738, 124);
                    Button_Pause.SetSize(214, 56);
                    Button_Pause.Anchors = Anchors.None;
                    Button_Pause.Visible = true;

                    break;

                default:
                    this.DesignWidth = 960;
                    this.DesignHeight = 544;

                    UriText.SetPosition(72, 7);
                    UriText.SetSize(589, 54);
                    UriText.Anchors = Anchors.None;
                    UriText.Visible = true;

                    UriLabel.SetPosition(10, 16);
                    UriLabel.SetSize(62, 36);
                    UriLabel.Anchors = Anchors.None;
                    UriLabel.Visible = true;

                    Button_Play.SetPosition(686, 7);
                    Button_Play.SetSize(84, 54);
                    Button_Play.Anchors = Anchors.None;
                    Button_Play.Visible = true;

                    Button_Stop.SetPosition(868, 7);
                    Button_Stop.SetSize(84, 54);
                    Button_Stop.Anchors = Anchors.None;
                    Button_Stop.Visible = true;

                    Button_Resume.SetPosition(667, 7);
                    Button_Resume.SetSize(103, 54);
                    Button_Resume.Anchors = Anchors.Height;
                    Button_Resume.Visible = false;

                    Button_Pause.SetPosition(777, 7);
                    Button_Pause.SetSize(84, 54);
                    Button_Pause.Anchors = Anchors.None;
                    Button_Pause.Visible = true;

                    break;
            }
            _currentLayoutOrientation = orientation;
        }

        public void UpdateLanguage()
        {
            UriLabel.Text = "URI:";

            Button_Play.Text = "Play";

            Button_Stop.Text = "Stop";

            Button_Resume.Text = "Resume";

            Button_Pause.Text = "Pause";
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
