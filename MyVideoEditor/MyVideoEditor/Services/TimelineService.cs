﻿using MyVideoEditor.DTOs;
using MyVideoEditor.Enums;
using MyVideoEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVideoEditor.Services
{
    public class TimelineService
    {
        #region Props 

        MainForm MainForm { get; }

        ProjectService ProjectService => MainForm.ProjectService;
        MediaContainerService MediaContainerService => MainForm.MediaContainerService;
        TimeStampService TimeStampService => MainForm.TimeStampService;

        Project? Project => MainForm.Project;
        Timeline? Timeline => MainForm.Timeline;

        #endregion

        public TimelineService(MainForm mainForm)
        {
            MainForm = mainForm;
        }

        public Timeline? GetCurrentTimeline()
        {
            if (Project == null) return null;
            return Project.Timelines.First(a => a.Id == Project.CurrentTimelineId);
        }

        public bool IsPlaying()
        {
            return false;
        }
        public bool IsPaused()
        {
            return true;
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Forward()
        {
            throw new NotImplementedException();
        }

        public void Backward()
        {
            throw new NotImplementedException();
        }
    }
}
