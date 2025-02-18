﻿namespace VideoEditor;

public class TimelineClipAudio : TimelineClip, ITimelineClip
{
    public TimelineClipAudio(Timeline timeline, StreamInfo streamInfo) : base(timeline, streamInfo)
    {
    }


    public double TimelineStartInSeconds
    {
        get => TimelineStartIndex / Timeline.SampleRate;
        set => TimelineStartIndex = Convert.ToInt64(value * Timeline.SampleRate);
    }
    public double TimelineEndInSeconds
    {
        get => TimelineEndIndex / Timeline.SampleRate;
        set => TimelineEndIndex = Convert.ToInt64(value * Timeline.SampleRate);
    }
    public double ClipStartInSeconds
    {
        get => ClipStartIndex / StreamInfo.SampleRate!.Value;
        set => ClipStartIndex = Convert.ToInt64(value * StreamInfo.SampleRate!.Value);
    }
    public double ClipEndInSeconds
    {
        get => ClipEndIndex / StreamInfo.SampleRate!.Value;
        set => ClipEndIndex = Convert.ToInt64(value * StreamInfo.SampleRate!.Value);
    }

    public bool IsVideoClip => false;
    public bool IsAudioClip => true;
}

