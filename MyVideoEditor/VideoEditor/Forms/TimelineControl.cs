﻿using System.Diagnostics;
using VideoEditor.Enums;
using VideoEditor.Static;
using VideoEditor.Types;

namespace VideoEditor.Forms;

public partial class TimelineControl : UserControl
{
    public TimelineControl()
    {
        InitializeComponent();
        Engine.TimelineControl = this;
    }

    int OldSmallScrollDelta { get; set; } = 0;
    int TotalBigScrollDelta { get; set; } = 0;

    bool Moving { get; set; }
    Point MovingStartPoint { get; set; }
    TimelinePosition MovingStartPosition { get; set; }
    List<File> DragAndDrop_Files { get; } = new List<File>();
    List<TimelineClipVideo> DragAndDrop_TimelineClipVideos { get; } = new List<TimelineClipVideo>();
    List<TimelineClipAudio> DragAndDrop_TimelineClipAudios { get; } = new List<TimelineClipAudio>();

    Timeline Timeline => Engine.Timeline;
    Rectangle TimelineRectangle => new Rectangle(
        ClientRectangle.Left,
        ClientRectangle.Top,
        ClientRectangle.Width,
        ClientRectangle.Height - scrollBarControl.Height);
    IEnumerable<ITimelineClip> TempTimelineClips =>
        DragAndDrop_TimelineClipVideos
            .Select(a => a as ITimelineClip)
            .Concat(DragAndDrop_TimelineClipAudios);

    private void TimelineControl_Load(object sender, EventArgs e)
    {
        SetupScrollbar();
    }
    private void TimelineControl_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Black);

        // Teken tijdsmarkeringen
        DrawTimeMarkers(g);

        // Teken clips
        DrawVideoClips(g);
    }
    private void TimelineControl_MouseWheel(object? sender, MouseEventArgs e)
    {
        var delta = GetScrollDelta(e);
        var clientPoint = new Point(e.X, e.Y);
        var currentTime = GetCurrentTime(clientPoint);
        if (currentTime == null) return;
        if ((ModifierKeys & Keys.Control) == Keys.Control)
        {
            ZoomX(delta, clientPoint, currentTime.Value);
        }
        else if ((ModifierKeys & Keys.Shift) == Keys.Shift)
        {
            ZoomY(delta, clientPoint, currentTime.Value);
        }
        else
        {
            ScrollY(delta, clientPoint, currentTime.Value);
        }
        Invalidate();
        SetupScrollbar();
    }
    private void ScrollBarControl_Scroll(object sender, ScrollEventArgs e)
    {
        Timeline.VisibleStart = e.NewValue;
        Invalidate();
    }
    private void TimelineControl_Resize(object sender, EventArgs e)
    {
        scrollBarControl.Left = 0;
        scrollBarControl.Top = ClientRectangle.Height - scrollBarControl.Height;
        scrollBarControl.Width = ClientRectangle.Width;
        Invalidate();
    }

    private void TimelineControl_DragEnter(object sender, DragEventArgs e)
    {
        var fullNames = GetDragAndDropFiles(e);
        if (fullNames.Length == 0)
        {
            ClearTempFilesAndClips();
            return;
        }

        var applicationPoint = new Point(e.X, e.Y);
        var timelinePosition = TranslateToTimelinePosition(applicationPoint);
        if (timelinePosition == null)
        {
            ClearTempFilesAndClips();
            return;
        }

        e.Effect = DragDropEffects.Copy;

        AddFilesToDragAndDrop(fullNames, timelinePosition.Value);
        Invalidate();
    }
    private void TimelineControl_DragOver(object sender, DragEventArgs e)
    {
        var fullNames = GetDragAndDropFiles(e);
        if (fullNames.Length == 0)
        {
            ClearTempFilesAndClips();
            return;
        }

        var applicationPoint = new Point(e.X, e.Y);
        var timelinePosition = TranslateToTimelinePosition(applicationPoint);
        if (timelinePosition == null)
        {
            ClearTempFilesAndClips();
            return;
        }

        MoveDragAndDrop(fullNames, timelinePosition);

        Invalidate();
        SetupScrollbar();
    }
    private void TimelineControl_DragDrop(object sender, DragEventArgs e)
    {
        var fullNames = GetDragAndDropFiles(e);
        if (fullNames.Length == 0) return;

        Timeline.VideoClips.AddRange(DragAndDrop_TimelineClipVideos);
        Timeline.AudioClips.AddRange(DragAndDrop_TimelineClipAudios);
        Timeline.AllClips.AddRange(DragAndDrop_TimelineClipVideos);
        Timeline.AllClips.AddRange(DragAndDrop_TimelineClipAudios);

        TimelineControl_DragLeave(sender, e);
    }
    private void TimelineControl_DragLeave(object sender, EventArgs e)
    {
        ClearTempFilesAndClips();
        Invalidate();
        SetupScrollbar();
    }

    private void TimelineControl_MouseDown(object sender, MouseEventArgs e)
    {
        var applicationPoint = new Point(e.X, e.Y);
        var position = TranslateToTimelinePosition(applicationPoint);
        if (position == null) return;

        var selectedClips = new List<ITimelineClip>();
        foreach (var clip in Timeline.AllClips)
        {
            var rect = CalculateRectangle(clip);
            if (rect.Left < e.X && e.X < rect.Right &&
                rect.Top < e.Y && e.Y < rect.Bottom &&
                !selectedClips.Contains(clip))
            {
                selectedClips.Add(clip);

                foreach (var clip2 in Timeline.AllClips)
                {
                    if (clip2.StreamInfo.File == clip.StreamInfo.File && 
                        !selectedClips.Contains(clip2))
                    {
                        selectedClips.Add(clip2);
                    }
                }
            }
        }

        if (selectedClips.Any(a => Timeline.SelectedClips.Any(b => b.Equals(a))))
        {
            Moving = true;
            MovingStartPoint = new Point(e.X, e.Y);
            var startposition = TranslateToTimelinePosition(MovingStartPoint);
            if (startposition == null) return;
            MovingStartPosition = startposition.Value;
            foreach ( var clip in selectedClips)
            {
                clip.OldLayer = clip.Layer;
                clip.OldTimelineStartInSeconds = clip.TimelineStartInSeconds;
                clip.OldTimelineEndInSeconds = clip.TimelineEndInSeconds;
            }
            return;
        }

        Timeline.SelectedClips.Clear();
        Timeline.SelectedClips.AddRange(selectedClips);

        Invalidate();
    }
    private void TimelineControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (!Moving) return;

        var movingEndPoint = new Point(e.X, e.Y);
        var movingEndPosition = TranslateToTimelinePosition(movingEndPoint);
        if (movingEndPosition == null) return;

        if (movingEndPosition.Value.Layer != MovingStartPosition.Layer ||
            movingEndPosition.Value.CurrentTime != MovingStartPosition.CurrentTime)
        {
            var l = movingEndPosition.Value.Layer - MovingStartPosition.Layer;
            var w = movingEndPosition.Value.CurrentTime - MovingStartPosition.CurrentTime;
            foreach (var clip in Timeline.SelectedClips)
            {
                clip.Layer = clip.OldLayer + l;
                clip.TimelineStartInSeconds = clip.OldTimelineStartInSeconds + w;
                clip.TimelineEndInSeconds = clip.OldTimelineEndInSeconds + w;
            }
        }

        var x = movingEndPoint.X - MovingStartPoint.X;
        var y = movingEndPoint.Y - MovingStartPoint.Y;
        Debug.WriteLine($"Dragging {x}x{y}");

        Invalidate();
    }
    private void TimelineControl_MouseUp(object sender, MouseEventArgs e)
    {
        Moving = false;
    }

    private void AddFilesToDragAndDrop(string[] fullNames, TimelinePosition timelinePosition)
    {
        var currentTime = timelinePosition.CurrentTime;
        var layerIndex = timelinePosition.Layer;
        foreach (var fullName in fullNames)
        {
            var file = new File(fullName);
            if (file.Duration == null) continue;

            var start = currentTime;
            currentTime += file.Duration.Value;
            var layer = layerIndex;
            foreach (var videoStream in file.VideoStreams.OrderBy(a => a.Index))
            {
                var clip = new TimelineClipVideo(Timeline, videoStream)
                {
                    Layer = layer,
                    TimelineStartInSeconds = start,
                    TimelineEndInSeconds = currentTime,
                    ClipStartInSeconds = 0,
                    ClipEndInSeconds = file.Duration.Value
                };
                DragAndDrop_TimelineClipVideos.Add(clip);
                layer++;
            }

            layer = 0;
            foreach (var audioStream in file.AudioStreams.OrderBy(a => a.Index))
            {
                var clip = new TimelineClipAudio(Timeline, audioStream)
                {
                    Layer = layer,
                    TimelineStartInSeconds = start,
                    TimelineEndInSeconds = currentTime,
                    ClipStartInSeconds = 0,
                    ClipEndInSeconds = file.Duration.Value
                };
                DragAndDrop_TimelineClipAudios.Add(clip);
                layer++;
            }

            DragAndDrop_Files.Add(file);
        }
    }
    private void MoveDragAndDrop(string[] fullNames, TimelinePosition? timelinePosition)
    {
        var currentTime = timelinePosition.Value.CurrentTime;
        var layerIndex = timelinePosition.Value.Layer;
        foreach (var fullName in fullNames)
        {
            var file = DragAndDrop_Files.FirstOrDefault(a => a.FullName == fullName);
            if (file == null) continue;
            if (file.Duration == null) continue;

            var start = currentTime;
            currentTime += file.Duration.Value;
            var layer = layerIndex;
            foreach (var videoStream in file.VideoStreams.OrderBy(a => a.Index))
            {
                var cachedVideoStream = DragAndDrop_TimelineClipVideos
                    .FirstOrDefault(a => a.StreamInfo.EqualTo(videoStream));

                if (cachedVideoStream != null)
                {
                    cachedVideoStream.Layer = layer;
                    cachedVideoStream.TimelineStartInSeconds = start;
                    cachedVideoStream.TimelineEndInSeconds = currentTime;
                    layer++;
                }
            }

            layer = layerIndex;
            foreach (var audioStream in file.AudioStreams.OrderBy(a => a.Index))
            {
                var cachedAudioStream = DragAndDrop_TimelineClipAudios
                    .FirstOrDefault(a => a.StreamInfo.EqualTo(audioStream));

                if (cachedAudioStream != null)
                {
                    cachedAudioStream.Layer = layer;
                    cachedAudioStream.TimelineStartInSeconds = start;
                    cachedAudioStream.TimelineEndInSeconds = currentTime;
                    layer++;
                }
            }
        }
    }
    private void DrawTimeMarkers(Graphics g)
    {
        using var pen = new Pen(Color.FromArgb(0, 0, 128), 1);
        using var pen2 = new Pen(Color.FromArgb(64, 0, 0), 1);
        using var font = new Font("Arial", Constants.TextSize);
        using var brush = new SolidBrush(Color.White);

        int middle = (ClientRectangle.Height - scrollBarControl.Height) / 2;
        int videoBlockHeight = (middle - Constants.MiddleOffset) / Timeline.VisibleVideoLayers;
        int audioBlockHeight = (middle - Constants.MiddleOffset) / Timeline.VisibleAudioLayers;

        for (var i = 0; i < Timeline.VisibleVideoLayers; i++)
        {
            var y = middle - i * videoBlockHeight - Constants.MiddleOffset;
            g.DrawLine(pen2, 0, y, Width, y);

            var text = $"{i + Timeline.FirstVisibleVideoLayer}";
            var textSize = g.MeasureString(text, font);
            var textY = y - videoBlockHeight / 2 - (int)(textSize.Height / 2);
            g.DrawString(text, font, brush, 2, textY);
        }
        for (var i = 0; i < Timeline.VisibleAudioLayers; i++)
        {
            var y = middle + i * audioBlockHeight + Constants.MiddleOffset;
            g.DrawLine(pen2, 0, y, Width, y);

            var text = $"{i + Timeline.FirstVisibleAudioLayer}";
            var textSize = g.MeasureString(text, font);
            var textY = y + videoBlockHeight / 2 - (int)(textSize.Height / 2);
            g.DrawString(text, font, brush, 2, textY);
        }

        var timeIncrease = 0.01D;
        var decimals = 2;
        while (Width / Timeline.VisibleWidth * timeIncrease < 50)
        {
            timeIncrease *= 10;
            decimals--;
        }
        if (decimals < 0) decimals = 0;

        for (var sec = 0D; sec < double.MaxValue; sec += timeIncrease)
        {
            var x = Convert.ToInt32((sec - Timeline.VisibleStart) / Timeline.VisibleWidth * Width);
            if (x >= Width) break;
            g.DrawLine(pen, x, 0, x, Height);

            var text = $"{sec.ToString("F" + decimals)}s";
            var textSize = g.MeasureString(text, font);
            var textY = middle - (int)(textSize.Height / 2);
            g.DrawString(text, font, brush, x + 2, textY);
        }

    }
    private void DrawVideoClips(Graphics g)
    {
        var clips = Timeline.AllClips.Concat(TempTimelineClips);
        foreach (var clip in clips)
        {
            var rect = CalculateRectangle(clip);

            if (rect.Left > ClientRectangle.Width || rect.Right < 0) continue; // Clip buiten zichtbare range
            if (rect.Top > ClientRectangle.Height || rect.Bottom < 0) continue; // Clip buiten zichtbare range

            var selected = Timeline.SelectedClips.Contains(clip);
            var fillBrush = selected ? Brushes.Red : clip.IsVideoClip ? Brushes.DarkBlue : Brushes.Aqua;
            var borderPen = Pens.White;

            g.FillRectangle(fillBrush, rect);
            g.DrawRectangle(borderPen, rect);
        }
    }
    private int GetScrollDelta(MouseEventArgs e)
    {
        TotalBigScrollDelta += e.Delta;

        if (TotalBigScrollDelta / 120 == OldSmallScrollDelta)
            return 0;

        var delta = TotalBigScrollDelta / 120 - OldSmallScrollDelta;
        OldSmallScrollDelta = TotalBigScrollDelta / 120;
        return delta;
    }
    private void ScrollY(int delta, Point clientPoint, double currentTime)
    {
        int Middle = (ClientRectangle.Height - scrollBarControl.Height) / 2;
        if (clientPoint.Y < Middle)
        {
            // Video
            Timeline.FirstVisibleVideoLayer += delta;
            if (Timeline.FirstVisibleVideoLayer < 0) Timeline.FirstVisibleVideoLayer = 0;
        }
        if (clientPoint.Y > Middle)
        {
            // Audio
            Timeline.FirstVisibleAudioLayer += delta;
            if (Timeline.FirstVisibleAudioLayer < 0) Timeline.FirstVisibleAudioLayer = 0;
        }
    }
    private void ZoomY(int delta, Point clientPoint, double currentTime)
    {
        int Middle = (ClientRectangle.Height - scrollBarControl.Height) / 2;
        if (clientPoint.Y < Middle)
        {
            // Video
            Timeline.VisibleVideoLayers += delta;
            if (Timeline.VisibleVideoLayers < 1) Timeline.VisibleVideoLayers = 1;
        }
        if (clientPoint.Y > Middle)
        {
            // Audio
            Timeline.VisibleAudioLayers += delta;
            if (Timeline.VisibleAudioLayers < 1) Timeline.VisibleAudioLayers = 1;
        }
    }
    private void ZoomX(int delta, Point clientPoint, double currentTime)
    {
        if (delta > 0)
        {
            for (int i = 0; i < delta; i++)
            {
                Timeline.VisibleWidth -= Timeline.VisibleWidth / 10;
            }
        }
        if (delta < 0)
        {
            for (int i = 0; i < delta * -1; i++)
            {
                Timeline.VisibleWidth += Timeline.VisibleWidth / 10;
            }
        }
    }
    private void SetupScrollbar()
    {
        var max = Timeline.AudioClips.Any() ? Timeline.AudioClips.Max(a => a.TimelineEndInSeconds) : Timeline.VisibleStart + Timeline.VisibleWidth;
        max = Math.Max(max, Timeline.VisibleStart + Timeline.VisibleWidth);
        scrollBarControl.Minimum = 0;
        scrollBarControl.Maximum = Convert.ToInt32(Math.Ceiling(max));
        scrollBarControl.SmallChange = 1;
        scrollBarControl.LargeChange = Convert.ToInt32(Math.Floor(Timeline.VisibleWidth));
    }
    private void ClearTempFilesAndClips()
    {
        DragAndDrop_Files.Clear();
        DragAndDrop_TimelineClipAudios.Clear();
        DragAndDrop_TimelineClipVideos.Clear();
    }
    private string[] GetDragAndDropFiles(DragEventArgs e)
    {
        if (e == null || e.Data == null) return [];
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return [];
        var filesobj = e.Data.GetData(DataFormats.FileDrop);
        if (filesobj == null) return [];
        var files = (string[])filesobj;
        if (files == null) return [];
        return CheckFileType.Filter(files)
            .OrderBy(a => a)
            .ToArray();
    }
    private TimelinePosition? TranslateToTimelinePosition(Point applicationPoint)
    {
        var clientPoint = PointToClient(applicationPoint);
        var currentTime = GetCurrentTime(clientPoint);
        if (currentTime == null) return null;
        var layerMediaFormat = GetLayerMediaFormat(clientPoint);
        if (layerMediaFormat == null) return null;
        return new TimelinePosition(layerMediaFormat.Value.MediaFormat, currentTime.Value, layerMediaFormat.Value.Layer);
    }
    private LayerMediaFormat? GetLayerMediaFormat(Point clientPoint)
    {
        // Bepaal de hoogte van de video- en audiotijdlijn
        var timelineHeight = TimelineRectangle.Height;
        var videoHeight = timelineHeight / 2;  // Bovenste helft is voor video
        var audioHeight = timelineHeight - videoHeight;  // Onderste helft is voor audio

        // Bepaal of de muis boven de videolaag of de audiolaag is
        if (clientPoint.Y < videoHeight)
        {
            // Video tijdlijn
            var videoLayerHeight = videoHeight / Timeline.VisibleVideoLayers;
            var layerIndex = Timeline.FirstVisibleVideoLayer + (int)(Timeline.VisibleVideoLayers - clientPoint.Y / videoLayerHeight - 1); // Bepaal de laag op de video tijdlijn
            return new LayerMediaFormat(MediaFormat.Video, layerIndex);
        }
        else if (clientPoint.Y >= videoHeight && clientPoint.Y < timelineHeight)
        {
            // Audio tijdlijn
            var audioLayerHeight = audioHeight / Timeline.VisibleAudioLayers;
            var layerIndex = Timeline.FirstVisibleAudioLayer + (int)((clientPoint.Y - videoHeight) / audioLayerHeight); // Bepaal de laag op de audio tijdlijn
            return new LayerMediaFormat(MediaFormat.Audio, layerIndex);
        }

        return null;
    }
    private double? GetCurrentTime(Point clientPoint)
    {
        return Timeline.VisibleStart + Timeline.VisibleWidth * clientPoint.X / ClientRectangle.Width;
    }
    private Rectangle CalculateRectangle(ITimelineClip clip)
    {
        var timeline = clip.Timeline;
        int Middle = TimelineRectangle.Height / 2;
        int VideoBlockHeight = (Middle - Constants.MiddleOffset) / timeline.VisibleVideoLayers;
        int AudioBlockHeight = (Middle - Constants.MiddleOffset) / timeline.VisibleAudioLayers;

        int x1 = Convert.ToInt32((clip.TimelineStartInSeconds - timeline.VisibleStart) / timeline.VisibleWidth * TimelineRectangle.Width);
        int x2 = Convert.ToInt32((clip.TimelineEndInSeconds - timeline.VisibleStart) / timeline.VisibleWidth * TimelineRectangle.Width);
        int width = x2 - x1;

        int layer;
        int y;
        if (clip.IsVideoClip)
        {
            layer = clip.Layer - timeline.FirstVisibleVideoLayer;
            y = Middle - Constants.MiddleOffset - VideoBlockHeight - layer * VideoBlockHeight;
        }
        else
        {
            layer = clip.Layer - timeline.FirstVisibleAudioLayer;
            y = Middle + Constants.MiddleOffset + (clip.Layer - timeline.FirstVisibleAudioLayer) * AudioBlockHeight;
        }

        var rect = new Rectangle(x1, y + Constants.Margin / 2, width, VideoBlockHeight - Constants.Margin);
        return rect;
    }

}
