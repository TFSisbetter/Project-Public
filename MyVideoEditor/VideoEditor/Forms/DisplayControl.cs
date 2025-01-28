﻿using VideoEditor.Static;

namespace VideoEditor.Forms;

public partial class DisplayControl : UserControl
{
    public DisplayControl()
    {
        InitializeComponent();
        Engine.DisplayControl = this;
    }

    private void DisplayControl_Resize(object sender, EventArgs e)
    {
        //if (Engine == null) return;

        var width = ClientRectangle.Width;
        var height = ClientRectangle.Height;

        var screenWidthBasedOnHeight = height * Engine.Timeline.VideoInfo.Resolution.Width / Engine.Timeline.VideoInfo.Resolution.Height;
        var screenHeightBasedOnWidth = width * Engine.Timeline.VideoInfo.Resolution.Height / Engine.Timeline.VideoInfo.Resolution.Width;

        if (height > screenHeightBasedOnWidth)
        {
            height = screenHeightBasedOnWidth;
        }
        if (width > screenWidthBasedOnHeight)
        {
            width = screenWidthBasedOnHeight;
        }

        var offsetX = (ClientRectangle.Width - width) / 2;
        var offsetY = (ClientRectangle.Height - height) / 2;

        videoControl.Top = offsetY;
        videoControl.Left = offsetX;
        videoControl.Width = width;
        videoControl.Height = height;

        //videoElement.Height = videoElement.Width * Engine.Timeline.Settings.Resolution.Height / Engine.Timeline.Settings.Resolution.Width;

        //if (videoElement.Height > Convert.ToInt32((ClientRectangle.Height - Constants.Margin * 3) * VerdelingY))
        //{
        //    videoElement.Height = Convert.ToInt32((ClientRectangle.Height - Constants.Margin * 3) * VerdelingY);
        //    videoElement.Width = videoElement.Height * Engine.Timeline.Settings.Resolution.Width / Engine.Timeline.Settings.Resolution.Height;
        //    videoElement.Left = (ClientRectangle.Width - Constants.Margin * 2 - videoElement.Width) / 2 + Constants.Margin;
        //}
    }
}
