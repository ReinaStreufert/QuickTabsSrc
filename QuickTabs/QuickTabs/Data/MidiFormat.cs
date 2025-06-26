using NAudio.Midi;
using QuickTabs.Songwriting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Data
{
    /*public class MidiFormat : FileFormat // note: midi is biiiiiig endian
    {
        public override string Extension => ".mid";

        public override string Name => "MIDI Tab File";

        public override Song Open(string fileName, out bool failed)
        {
            MidiFile midiFile;
            try
            {
                midiFile = new MidiFile(fileName);
            } catch
            {
                failed = true;
                return null;
            }
            if (midiFile.FileFormat != 0 || midiFile.Tracks > 1)
            {
                throw new Exception();
                failed = true;
                return null;
            }
            if (midiFile.DeltaTicksPerQuarterNote != 2)
            {
                throw new Exception();
                failed = true;
                return null;
            }

            Song song = new Song();
            Tab tab = song.Tab;
            tab.SetLength(1);
            bool tuningSet = false;
            bool nameSet = false;
            bool tsSet = false;
            bool tempoSet = false;
            int selectedString = 0;
            foreach (MidiEvent midiEvent in midiFile.Events[0])
            {
                tab.SetLength(tab.Count + midiEvent.DeltaTime);
                if (midiEvent.CommandCode == MidiCommandCode.NoteOn)
                {
                    if (!tuningSet || !nameSet || !tsSet || !tempoSet)
                    {
                        throw new Exception();
                        failed = true;
                        return null;
                    }
                    NoteOnEvent noteOnEvent = (NoteOnEvent)midiEvent;
                    Beat currentBeat = (Beat)tab[tab.Count - 1];
                    Note stringNote = tab.Tuning.GetMusicalNote(selectedString);
                    if (noteOnEvent.NoteNumber < stringNote.MidiNumber)
                    {
                        throw new Exception();
                        failed = true;
                        return null;
                    }
                    Note eventNote = new Note(noteOnEvent.NoteName);
                    int fret = Note.ToSemitones(stringNote, eventNote);
                    currentBeat[new Fret(selectedString, fret)] = true;
                    currentBeat.SustainTimeEighthNotes = noteOnEvent.NoteLength;
                } else if (midiEvent.CommandCode == MidiCommandCode.ControlChange)
                {
                    ControlChangeEvent controlChangeEvent = (ControlChangeEvent)midiEvent;
                    if (controlChangeEvent.Controller == (MidiController)88)
                    {
                        if (controlChangeEvent.ControllerValue >= 0 && controlChangeEvent.ControllerValue < tab.Tuning.Count)
                        {
                            selectedString = controlChangeEvent.ControllerValue;
                        } else
                        {
                            throw new Exception();
                            failed = true;
                            return null;
                        }
                    }
                } else if (midiEvent.CommandCode == MidiCommandCode.MetaEvent)
                {
                    MetaEvent metaEvent = (MetaEvent)midiEvent;
                    if (metaEvent.MetaEventType == MetaEventType.Marker)
                    {
                        TextEvent markerEvent = (TextEvent)metaEvent;
                        SectionHead sh = new SectionHead();
                        sh.Name = markerEvent.Text;
                        tab[tab.Count - 1] = sh;
                        tab.SetLength(tab.Count + 1);
                    } else if (metaEvent.MetaEventType == MetaEventType.TextEvent)
                    {
                        TextEvent textEvent = (TextEvent)metaEvent;
                        if (textEvent.Text.Substring(0, 8) == "tuning: ")
                        {
                            if (tuningSet)
                            {
                                throw new Exception();
                                failed = true;
                                return null;
                            }
                            string tuningString = textEvent.Text.Substring(8);
                            string[] tuningNotes = tuningString.Split(' ').Reverse().ToArray();
                            tab.Tuning = new Tuning(tuningNotes);
                            tuningSet = true;
                        }
                    } else if (metaEvent.MetaEventType == MetaEventType.SequenceTrackName)
                    {
                        if (nameSet)
                        {
                            throw new Exception();
                            failed = true;
                            return null;
                        }
                        TextEvent sequenceName = (TextEvent)metaEvent;
                        song.Name = sequenceName.Text;
                        nameSet = true;
                    } else if (metaEvent.MetaEventType == MetaEventType.SetTempo)
                    {
                        if (tempoSet)
                        {
                            throw new Exception();
                            failed = true;
                            return null;
                        }
                        TempoEvent setTempo = (TempoEvent)metaEvent;
                        song.Tempo = msecPerQuarterToBPM(setTempo.MicrosecondsPerQuarterNote);
                        tempoSet = true;
                    } else if (metaEvent.MetaEventType == MetaEventType.TimeSignature)
                    {
                        if (tsSet)
                        {
                            throw new Exception();
                            failed = true;
                            return null;
                        }
                        TimeSignatureEvent timeSignature = (TimeSignatureEvent)metaEvent;
                        song.TimeSignature = new TimeSignature(timeSignature.Numerator, tsMidiDenominatorToQTDenominator(timeSignature.Denominator));
                        tsSet = true;
                    } else if (metaEvent.MetaEventType == MetaEventType.EndTrack)
                    {
                        tab.SetLength(tab.Count - 1);
                        break;
                    }
                }
            }
            failed = false;
            return song;
        }
        public override void Save(Song song, string fileName)
        {
            MidiEventCollection midiEvents = new MidiEventCollection(0, 2);

            Tuning tuning = song.Tab.Tuning;
            string tuningText = "tuning:";
            for (int i = tuning.Count - 1; i >= 0; i--)
            {
                tuningText += " " + tuning.GetMusicalNote(i).ToString();
            }
            TextEvent tuningEvent = new TextEvent(tuningText, MetaEventType.TextEvent, 0);
            midiEvents.AddEvent(tuningEvent, 0);
            TextEvent nameEvent = new TextEvent(song.Name, MetaEventType.SequenceTrackName, 0);
            midiEvents.AddEvent(nameEvent, 0);
            TempoEvent tempoEvent = new TempoEvent(bpmToMsecsPerQuarter(song.Tempo), 0);
            midiEvents.AddEvent(tempoEvent, 0);
            TimeSignatureEvent tsEvent = new TimeSignatureEvent(0, song.TimeSignature.T1, tsQTDenominatorToMidiDenominator(song.TimeSignature.T2), 2, 8);
            midiEvents.AddEvent(tsEvent, 0);

            long absoluteTime = 0;
            int selectedString = 0;
            List<PlayingBeat> playingBeats = new List<PlayingBeat>();
            foreach (Step step in song.Tab)
            {
                if (step.Type == Enums.StepType.Beat)
                {
                    foreach (PlayingBeat playingBeat in playingBeats.ToArray())
                    {
                        playingBeat.timeLeft--;
                        if (playingBeat.timeLeft <= 0)
                        {
                            playingBeats.Remove(playingBeat);
                            foreach (Fret fret in playingBeat.beat)
                            {
                                Note stringNote = tuning.GetMusicalNote(fret.String);
                                NoteEvent noteOff = new NoteEvent(absoluteTime, 1, MidiCommandCode.NoteOff, stringNote.MidiNumber + fret.Space, 127);
                                midiEvents.AddEvent(noteOff, 0);
                            }
                        }
                    }
                    Beat beat = (Beat)step;
                    bool empty = true;
                    foreach (Fret fret in beat)
                    {
                        empty = false;
                        if (selectedString != fret.String)
                        {
                            ControlChangeEvent controlChangeEvent = new ControlChangeEvent(absoluteTime, 1, (MidiController)88, fret.String);
                            midiEvents.AddEvent(controlChangeEvent, 0);
                            selectedString = fret.String;
                        }
                        Note stringNote = tuning.GetMusicalNote(fret.String);
                        NoteEvent noteOn = new NoteEvent(absoluteTime, 1, MidiCommandCode.NoteOn, stringNote.MidiNumber + fret.Space, 127);
                        midiEvents.AddEvent(noteOn, 0);
                    }
                    if (!empty)
                    {
                        PlayingBeat playingBeat = new PlayingBeat();
                        playingBeat.beat = beat;
                        playingBeat.timeLeft = beat.SustainTimeEighthNotes;
                        playingBeats.Add(playingBeat);
                    }
                    absoluteTime++;
                } else if (step.Type == Enums.StepType.SectionHead)
                {
                    SectionHead sectionHead = (SectionHead)step;
                    TextEvent markerEvent = new TextEvent(sectionHead.Name, MetaEventType.Marker, absoluteTime);
                    midiEvents.AddEvent(markerEvent, 0);
                }
            }
            foreach (PlayingBeat playingBeat in playingBeats.ToArray())
            {
                foreach (Fret fret in playingBeat.beat)
                {
                    Note stringNote = tuning.GetMusicalNote(fret.String);
                    NoteEvent noteOff = new NoteEvent(absoluteTime, 1, MidiCommandCode.NoteOff, stringNote.MidiNumber + fret.Space, 127);
                    midiEvents.AddEvent(noteOff, 0);
                }
            }
            playingBeats.Clear();
            MetaEvent endOfTrack = new MetaEvent(MetaEventType.EndTrack, 0, absoluteTime);
            midiEvents.AddEvent(endOfTrack, 0);

            MidiFile.Export(fileName, midiEvents);
        }

        private int msecPerQuarterToBPM(int msecs)
        {
            int minuteMsecs = 60 * 1000000;
            return (int)Math.Round(minuteMsecs / (double)msecs);
        }
        private int bpmToMsecsPerQuarter(int bpm)
        {
            double beatsPerSec = bpm / 60F;
            double secPerBeat = 1 / beatsPerSec;
            return (int)Math.Round(secPerBeat * 1000000);
        }
        private int tsMidiDenominatorToQTDenominator(int denom)
        {
            return 1 << denom;
        }
        private int tsQTDenominatorToMidiDenominator(int denom)
        {
            int shifts = 0;
            while (denom > 0)
            {
                denom >>= 1;
                shifts++;
            }
            return shifts - 1;
        }
        private class PlayingBeat
        {
            public Beat beat;
            public int timeLeft;
        }
    }*/
}
