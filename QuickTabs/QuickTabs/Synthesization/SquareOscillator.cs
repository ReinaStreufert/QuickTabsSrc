using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    public class SquareOscillator : NAudio.Wave.IWaveProvider
    {
        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        public int MidtickSamples { get; set; } = 0;
        public int SamplesWritten { get; private set; } = 0;
        public DateTime StartTime { get; private set; }

        private List<FrequencyTracker> frequencies = new List<FrequencyTracker>();
        private float[] floatBuffer;

        public PlayingFrequency AddFrequency(float frequency, float level, IVolumeProvider volume)
        {
            FrequencyTracker frequencyTracker = new FrequencyTracker();
            frequencyTracker.Frequency = frequency;
            frequencyTracker.SamplesPerOscillation = (int)Math.Round(WaveFormat.SampleRate / frequency / 2);
            frequencyTracker.Level = level;
            frequencyTracker.Volume = volume;
            frequencies.Add(frequencyTracker);
            return frequencyTracker;
        }
        public PlayingFrequency AddFrequency(float startFrequency, float endFrequency, float pitchEnvelopeDuration, float startLevel, float endLevel, float levelEnvelopeDuration, IVolumeProvider volume)
        {
            FrequencyTracker frequencyTracker = new FrequencyTracker();
            frequencyTracker.Enveloped = true;
            frequencyTracker.Frequency = startFrequency;
            frequencyTracker.FrequencyEnd = endFrequency;
            frequencyTracker.Level = startLevel;
            frequencyTracker.LevelEnd = endLevel;
            frequencyTracker.FrequencyEnvelopeSampleDuration = (int)(WaveFormat.SampleRate * (pitchEnvelopeDuration / 1000F));
            frequencyTracker.LevelEnvelopeSampleDuration = (int)(WaveFormat.SampleRate * (levelEnvelopeDuration / 1000F));
            frequencyTracker.Volume = volume;
            frequencies.Add(frequencyTracker);
            return frequencyTracker;
        }
        public void ClearFrequency(PlayingFrequency frequency)
        {
            frequencies.Remove((FrequencyTracker)frequency);
        }
        public void ClearAllFrequencies()
        {
            frequencies.Clear();
        }
        public void InitializeBuffer(int samplesPerBuffer)
        {
            floatBuffer = new float[samplesPerBuffer * WaveFormat.Channels];
        }
        public event Action BeforeBufferFill;

        public int Read(byte[] buffer, int offset, int count)
        {
            if (SamplesWritten < 1)
            {
                StartTime = DateTime.Now;
            }
            BeforeBufferFill?.Invoke();
            int channelCount = WaveFormat.Channels;
            int floatCount = count / 4;
            int midtickFinalFloatCount = floatCount;
            if (MidtickSamples > 0)
            {
                floatCount = MidtickSamples * channelCount;
            }
            for (int i = 0; i < floatCount; i += channelCount)
            {
                float sample = getNextSample();
                for (int ii = i; ii < i + channelCount; ii++)
                {
                    floatBuffer[ii] = sample;
                }
            }
            if (MidtickSamples > 0)
            {
                MidtickSamples = 0;
                //System.Diagnostics.Debug.WriteLine("midtick");
                BeforeBufferFill?.Invoke();
                for (int i = floatCount; i < midtickFinalFloatCount; i += channelCount)
                {
                    float sample = getNextSample();
                    for (int ii = i; ii < i + channelCount; ii++)
                    {
                        floatBuffer[ii] = sample;
                    }
                }
            }
            Buffer.BlockCopy(floatBuffer, 0, buffer, offset, count);
            return count;
        }

        private float getNextSample()
        {
            float sample = 0F;
            foreach (FrequencyTracker frequencyTracker in frequencies)
            {
                if (frequencyTracker.Enveloped)
                {
                    float level;
                    if (frequencyTracker.EnvelopeSamplesWritten >= frequencyTracker.LevelEnvelopeSampleDuration)
                    {
                        level = frequencyTracker.LevelEnd;
                    } else
                    {
                        level = interpolate(frequencyTracker.Level, frequencyTracker.LevelEnd, (float)frequencyTracker.EnvelopeSamplesWritten / frequencyTracker.LevelEnvelopeSampleDuration);
                    }
                    float pitch;
                    if (frequencyTracker.EnvelopeSamplesWritten >= frequencyTracker.FrequencyEnvelopeSampleDuration)
                    {
                        pitch = frequencyTracker.FrequencyEnd;
                    }
                    else
                    {
                        pitch = interpolate(frequencyTracker.Frequency, frequencyTracker.FrequencyEnd, (float)frequencyTracker.EnvelopeSamplesWritten / frequencyTracker.FrequencyEnvelopeSampleDuration);
                    }
                    int samplesPerOscillation = (int)Math.Round(WaveFormat.SampleRate / pitch / 2);
                    frequencyTracker.EnvelopeSamplesWritten++;
                    if (frequencyTracker.CurrentlyOutputting)
                    {
                        sample += level * frequencyTracker.Volume.Volume;
                    }
                    frequencyTracker.OutputtedSinceLastSwitch++;
                    if (frequencyTracker.OutputtedSinceLastSwitch >= samplesPerOscillation)
                    {
                        frequencyTracker.OutputtedSinceLastSwitch = 0;
                        frequencyTracker.CurrentlyOutputting = !frequencyTracker.CurrentlyOutputting;
                    }
                } else
                {
                    if (frequencyTracker.CurrentlyOutputting)
                    {
                        sample += frequencyTracker.Level * frequencyTracker.Volume.Volume;
                    }
                    frequencyTracker.OutputtedSinceLastSwitch++;
                    if (frequencyTracker.OutputtedSinceLastSwitch >= frequencyTracker.SamplesPerOscillation)
                    {
                        frequencyTracker.OutputtedSinceLastSwitch = 0;
                        frequencyTracker.CurrentlyOutputting = !frequencyTracker.CurrentlyOutputting;
                    }
                }
            }
            SamplesWritten++;
            return sample;
        }

        private float interpolate(float start, float end, float x)
        {
            return start + (end - start) * x;
        }

        public abstract class PlayingFrequency { }

        private class FrequencyTracker : PlayingFrequency
        {
            public float Frequency = 0;
            public float Level = 0F;
            public bool Enveloped = false;
            public float FrequencyEnd;
            public float LevelEnd;
            public IVolumeProvider Volume;
            public int FrequencyEnvelopeSampleDuration;
            public int LevelEnvelopeSampleDuration;
            public int EnvelopeSamplesWritten = 0;
            public int SamplesPerOscillation = 0;
            public int OutputtedSinceLastSwitch = 0;
            public bool CurrentlyOutputting = true;
        }
    }
}
