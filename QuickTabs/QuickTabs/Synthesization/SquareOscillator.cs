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
            BeforeBufferFill?.Invoke();
            int channelCount = WaveFormat.Channels;
            int floatCount = count / 4;
            for (int i = 0; i < floatCount; i += channelCount)
            {
                float sample = getNextSample();
                for (int ii = i; ii < i + channelCount; ii++)
                {
                    
                    floatBuffer[ii] = sample;
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
                if (frequencyTracker == null) // one time it crashed with a null reference exception on the next if statement and i was never able to reproduce it. i think its a threading thing :/ for now im just putting this here (until it inevitably happens again somewhere else and i find the root problem)
                {
                    continue;
                }
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
            return sample;
        }

        public abstract class PlayingFrequency { }

        private class FrequencyTracker : PlayingFrequency
        {
            public float Frequency = 0;
            public int SamplesPerOscillation = 0;
            public int OutputtedSinceLastSwitch = 0;
            public bool CurrentlyOutputting = true;
            public float Volume = 0F;
        }
    }
}
