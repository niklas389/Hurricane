﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using Hurricane.Music.MusicDatabase.EventArgs;
using Hurricane.Music.Track;

namespace Hurricane.Music.Playlist
{
    [Serializable, XmlType(TypeName = "Playlist")]
    public class NormalPlaylist : PlaylistBase
    {
        private string _name;
        public override string Name
        {
            get { return _name; }
            set
            {
                SetProperty(value, ref _name);
            }
        }

        public async Task AddFiles(EventHandler<TrackImportProgressChangedEventArgs> progresschanged, params string[] paths)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                FileInfo fi = new FileInfo(paths[i]);
                if (fi.Exists)
                {
                    if (progresschanged != null) progresschanged(this, new TrackImportProgressChangedEventArgs(i, paths.Length, fi.Name));
                    var t = new LocalTrack() { Path = fi.FullName };
                    if (!await t.LoadInformation()) continue;
                    t.TimeAdded = DateTime.Now;
                    AddTrack(t);
                }
            }
            AsyncTrackLoader.Instance.RunAsync(new List<NormalPlaylist> {this});
        }

        public async Task AddFiles(params string[] paths)
        {
            await AddFiles(null, paths);
        }

        public async Task ReloadTrackInformation(EventHandler<TrackImportProgressChangedEventArgs> progresschanged)
        {
            foreach (PlayableBase t in Tracks)
            {
                if (progresschanged != null) progresschanged(this, new TrackImportProgressChangedEventArgs(Tracks.IndexOf(t), Tracks.Count, t.ToString()));
                if (t.TrackExists)
                {
                    await t.LoadInformation();
                }
            }
        }

        public override void AddTrack(PlayableBase track)
        {
            base.AddTrack(track);
            Tracks.Add(track);
            ShuffleList.Add(track);

            track.IsAdded = true;
            DispatcherTimer tmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            tmr.Tick += (s, e) =>
            {
                track.IsAdded = false;
                tmr.Stop();
            };
            tmr.Start();
        }

        public override void RemoveTrack(PlayableBase track)
        {
            base.RemoveTrack(track);
            ShuffleList.Remove(track);
            track.IsRemoving = true;
            DispatcherTimer tmr = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            tmr.Tick += (s, e) =>
            {
                if (!track.TrackExists)
                {
                    for (int i = 0; i < Tracks.Count; i++)
                    {
                        if (Tracks[i].AuthenticationCode == track.AuthenticationCode)
                        {
                            Tracks.RemoveAt(i);
                            break;
                        }
                    }
                }
                else { Tracks.Remove(track); }
                
                tmr.Stop();
                track.IsRemoving = false; //The track could be also in another playlist
            };
            tmr.Start();
        }

        public override void Clear()
        {
            Tracks.Clear();
            ShuffleList.Clear();
        }

        public override bool CanEdit
        {
            get { return true; }
        }
    }
}
