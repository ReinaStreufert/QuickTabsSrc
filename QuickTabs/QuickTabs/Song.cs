using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuickTabs.Songwriting;

namespace QuickTabs
{
    public class Song : IJsonSaveable
    {
        public string Name { get; set; } = "Untitled tab";
        public int Tempo { get; set; } = 120;
        public TimeSignature TimeSignature { get; set; } = new TimeSignature(4, 4);
        public Tab FocusedTab
        {
            get
            {
                return Tracks[FocusedTrackIndex].Tab;
            }
        }
        public int FocusedTrackIndex { get; set; } = 0;
        public Track FocusedTrack
        {
            get
            {
                return Tracks[FocusedTrackIndex];
            }
            set
            {
                FocusedTrackIndex = Tracks.IndexOf(value);
            }
        }
        public List<Track> Tracks { get; set; } = new List<Track>() { new Track() };

        public Section[] GetSectionGraph()
        {
            List<Section> result = new List<Section>();
            for (int i = 0; i < FocusedTab.Count; i++)
            {
                Step step = FocusedTab[i];
                if (step.Type == Enums.StepType.SectionHead)
                {
                    SectionHead sh = (SectionHead)step;
                    MusicalTimespan firstBeatTime = FocusedTab.FindBeatTime((Beat)FocusedTab[i + 1]);
                    result.Add(generateSection(sh.Name, firstBeatTime));
                }
            }
            return result.ToArray();
        }
        private Section generateSection(string name, MusicalTimespan firstBeatTime)
        {
            Section section = new Section();
            section.SectionName = name;
            section.FirstBeatPosition = firstBeatTime;
            Dictionary<Track, Beat[]> sectionContent = new Dictionary<Track, Beat[]>();
            section.Content = sectionContent;

            foreach (Track track in Tracks)
            {
                Tab trackTab = track.Tab;
                MusicalTimespan throwaway;
                int sectionStart = trackTab.FindClosestBeatIndexToTime(firstBeatTime, out throwaway);
                int sectionBeatLength = 0;
                for (int i = sectionStart; i < trackTab.Count; i++)
                {
                    if (trackTab[i].Type == Enums.StepType.Beat)
                    {
                        sectionBeatLength++;
                    } else
                    {
                        break;
                    }
                }
                Beat[] sectionBeats = new Beat[sectionBeatLength];
                int sectionBeatsI = 0;
                for (int i = sectionStart; i < trackTab.Count; i++)
                {
                    if (trackTab[i].Type == Enums.StepType.Beat)
                    {
                        sectionBeats[sectionBeatsI] = (Beat)trackTab[i];
                        sectionBeatsI++;
                    }
                    else
                    {
                        break;
                    }
                }
                sectionContent[track] = sectionBeats;
            }

            return section;
        } 

        public JObject SaveAsJObject(Song Song)
        {
            JObject songJson = new JObject();
            songJson.Add("name", Name);
            songJson.Add("tempo", Tempo);
            JArray tsJson = new JArray();
            tsJson.Add(TimeSignature.T1);
            tsJson.Add(TimeSignature.T2);
            songJson.Add("ts", tsJson);
            JArray tracksJson = new JArray();
            foreach (Track track in Tracks)
            {
                Tab tab = track.Tab;
                JObject trackJson = new JObject();
                trackJson.Add("name", track.Name);
                trackJson.Add("mute", track.Mute);
                trackJson.Add("solo", track.Solo);
                trackJson.Add("volume", track.Volume);
                trackJson.Add("namedbyuser", track.NamedByUser);
                JArray tuningJson = new JArray();
                for (int i = track.Tab.Tuning.Count - 1; i >= 0; i--)
                {
                    tuningJson.Add(tab.Tuning.GetMusicalNote(i).ToString());
                }
                trackJson.Add("tuning", tuningJson);
                JArray stepsJson = new JArray();
                foreach (Step step in tab)
                {
                    stepsJson.Add(step.SaveAsJObject(this));
                }
                trackJson.Add("steps", stepsJson);
                tracksJson.Add(trackJson);
            }
            songJson.Add("tracks", tracksJson);
            return songJson;
        }
    }
}
