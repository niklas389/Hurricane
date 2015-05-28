﻿using System;
using System.Collections.ObjectModel;

namespace Hurricane.Model.AudioEngine
{
    public interface ISoundOutProvider : IDisposable
    {
        ObservableCollection<ISoundOutMode> SoundOutModes { get; }
        bool IsAvailable { get; }
        ISoundOutDevice CurrentSoundOutDevice { get; set; }
    }
}