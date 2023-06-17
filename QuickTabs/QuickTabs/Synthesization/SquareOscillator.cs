using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickTabs.Synthesization
{
    internal class SquareOscillator : NAudio.Wave.IWaveProvider
    {
        public WaveFormat WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        public int MidtickSamples { get; set; } = 0;
        public int SamplesWritten { get; private set; } = 0;
        public DateTime StartTime { get; private set; }

        private List<FrequencyTracker> frequencies = new List<FrequencyTracker>();
        private float[] floatBuffer;

        public PlayingFrequency AddFrequency(float frequency, float volume)
        {
            FrequencyTracker frequencyTracker = new FrequencyTracker();
            frequencyTracker.Frequency = frequency;
            frequencyTracker.SamplesPerOscillation = (int)Math.Round(WaveFormat.SampleRate / frequency / 2);
            frequencyTracker.Volume = volume;
            frequencies.Add(frequencyTracker);
            return frequencyTracker;
        }
        public PlayingFrequency AddFrequency(float startFrequency, float endFrequency, float pitchEnvelopeDuration, float startVolume, float endVolume, float volumeEnvelopeDuration)
        {
            FrequencyTracker frequencyTracker = new FrequencyTracker();
            frequencyTracker.Enveloped = true;
            frequencyTracker.Frequency = startFrequency;
            frequencyTracker.FrequencyEnd = endFrequency;
            frequencyTracker.Volume = startVolume;
            frequencyTracker.VolumeEnd = endVolume;
            frequencyTracker.FrequencyEnvelopeSampleDuration = (int)(WaveFormat.SampleRate * (pitchEnvelopeDuration / 1000F));
            frequencyTracker.VolumeEnvelopeSampleDuration = (int)(WaveFormat.SampleRate * (volumeEnvelopeDuration / 1000F));
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
                    float volume;
                    if (frequencyTracker.EnvelopeSamplesWritten >= frequencyTracker.VolumeEnvelopeSampleDuration)
                    {
                        volume = frequencyTracker.VolumeEnd;
                    } else
                    {
                        volume = interpolate(frequencyTracker.Volume, frequencyTracker.VolumeEnd, (float)frequencyTracker.EnvelopeSamplesWritten / frequencyTracker.VolumeEnvelopeSampleDuration);
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
                        sample += volume;
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
                        sample += frequencyTracker.Volume;
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
            public float Volume = 0F;
            public bool Enveloped = false;
            public float FrequencyEnd;
            public float VolumeEnd;
            public int FrequencyEnvelopeSampleDuration;
            public int VolumeEnvelopeSampleDuration;
            public int EnvelopeSamplesWritten = 0;
            public int SamplesPerOscillation = 0;
            public int OutputtedSinceLastSwitch = 0;
            public bool CurrentlyOutputting = true;
        }
    }
}
