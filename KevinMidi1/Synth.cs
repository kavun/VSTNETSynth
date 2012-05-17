using System;
using System.Collections.Generic;
using System.Text;

using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;


namespace KevinMidi1
{
    class Synth
    {
        private StereoBuffer _synthBuffer { get; set; }
        public int SampleCount { get; set; }
        public Synth()
        {
            _synthBuffer = new StereoBuffer(0);
        }
        /// <summary>
        /// Starts synthesizing the current note.
        /// </summary>
        /// <param name="noteNo">The midi note number.</param>
        public void ProcessNoteOnEvent(byte noteNo)
        {
            _synthBuffer = new StereoBuffer(noteNo);
            if (_player == null)
            {
                _player = new SamplePlayer(_synthBuffer);
                _player.Synthesize();
                _player.isOn = true;
            }
            else
            {
                if (_player.isOn == true)
                {
                    if (_player.SynthBuffer.NoteNo != noteNo)
                    {
                        
                        _player.attackTime = 44100 / 20;
                        _player.SynthBuffer = _synthBuffer;
                        //_player = new SamplePlayer(_synthBuffer);
                        _player.Synthesize();
                    }
                }
                else
                {
                    _player.attackTime = 44100 / 5;
                    _player.SynthBuffer = _synthBuffer;
                    _player.Synthesize();
                    _player.isOn = true;
                }
            }
        }

        /// <summary>
        /// Stops synthesizing the current note.
        /// </summary>
        /// <param name="noteNo">The midi note number.</param>
        public void ProcessNoteOffEvent(byte noteNo)
        {
            if (_player.isOn && _player.SynthBuffer.NoteNo == noteNo)
            {
                _player.shouldEnd = true;
            }
        }


        private SamplePlayer _player;
        /// <summary>
        /// Indicates if a sythesized sample buffer is being played back
        /// </summary>
        public bool IsPlaying
        {
            get { if (_player == null) return false; else return _player.isOn; }
        }

        /// <summary>
        /// Plays back the current sample buffer
        /// </summary>
        /// <param name="outChannels">Output buffers. Must not be null.</param>
        public void PlayAudio(VstAudioBuffer[] outChannels)
        {
            if (IsPlaying)
            {
                _player.Play(outChannels[0], outChannels[1]);
            }
        }


        /// <summary>
        /// Manages playing back a sample buffer
        /// </summary>
        public class SamplePlayer
        {
            public bool isOn { get; set; }
            public SamplePlayer(StereoBuffer synthBuffer)
            {
                SynthBuffer = synthBuffer;
            }

            public void Synthesize() // create one period for given frequency
            {
                releaseIndex = releaseTime;
                time = 0;
                shouldEnd = false;
                float freq = Frequency(SynthBuffer.NoteNo);
                float samples = _samplerate / freq;
                float _time;
                float clip = 0;
                float prevClip = 0;
                int i = 0;
                bool notDone = true;
                while(notDone)
                {
                    _time = (float)i / samples;
                    clip = Sine(_time, freq);
                    if (i != 0 & clip >= 0 & prevClip < 0) // if sine is 0 and increasing
                        notDone = false;
                    SynthBuffer.LeftSamples.Add(clip);
                    SynthBuffer.RightSamples.Add(clip);
                    prevClip = clip;
                    i++;
                }
            }
            float Sine(float time, float freq)
            {
                return (float)Math.Sin(time * 2.0f * Math.PI);
            }
            float Square(float time, float freq)
            {
                return Math.Sign(Sine(time, freq));
            }
            float Saw(float time)
            {
                return (float) (1 - 2 * time);
            }
            // convert MIDI note number to A440 based frequency
            float Frequency(int noteNo)
            {
                if (noteNo >= 0 & noteNo <= 119)
                    return 440.0f * (float)Math.Pow((float)2, (float)(noteNo-69) / 12.0f);
                else
                    return -1;
            } 

            private static float _samplerate = 44100; // samples per second
            private int _bufferIndex = 0;

            public StereoBuffer SynthBuffer { get; set; }
            public StereoBuffer toCombine { get; set; }
            public int bufferOffset { get { return _bufferOffset; } set { _bufferOffset = value; } }
            public int _bufferOffset = 0;
            public int time = 0;
            public bool shouldEnd = false;

            
            public float attackTime = _samplerate / 5; // 0.20 s
            public static float releaseTime = _samplerate / 5;
            public float releaseIndex = releaseTime;
            public void Play(VstAudioBuffer left, VstAudioBuffer right)
            {
                _bufferIndex = 0;
                float clip;
                //System.IO.File.AppendAllText(@"C:\Users\Kavun\School\SPRING 12\CSC498\VST\KevinMidi1\KevinMidi1\bin\Debug\debug.txt", "PreLoad\n" + "_bufferIndex: " + _bufferIndex + "\n" + "bufferOffset: " + bufferOffset + "\n" + "Synth samples: " + SynthBuffer.LeftSamples.Count + "\n" + "OutputBufferSize: " + left.SampleCount + "\n\n");
                while (_bufferIndex + SynthBuffer.LeftSamples.Count < left.SampleCount)
                {
                    for (int index = bufferOffset; index < SynthBuffer.LeftSamples.Count; index++)
                    {
                        clip = SynthBuffer.LeftSamples[index];
                        
                        if (time < attackTime) // attack envelope
                        {
                            clip = time / attackTime * clip; 
                        }
                        else if (shouldEnd) // forced release envelope
                        {
                            
                            if (releaseIndex == 0)
                            {
                                clip = 0;
                                isOn = false;
                            }
                            else
                            {
                                clip = clip * releaseIndex / releaseTime;
                                releaseIndex--;
                            }
                        }
                        left[_bufferIndex] = clip;
                        right[_bufferIndex] = clip;
                        _bufferIndex++;
                        time++;
                    }
                    if (bufferOffset != 0)
                        bufferOffset = 0;
                }
                bufferOffset = left.SampleCount - _bufferIndex;
                for (int index = 0; index < bufferOffset; index++)
                {

                    
                        clip = SynthBuffer.LeftSamples[index];
                    
                    if (time < attackTime)
                    {
                        clip = time / attackTime * clip; // attack envelope
                    }
                    else if (shouldEnd) // forced release envelope
                    {

                        if (releaseIndex == 0)
                        {
                            clip = 0;
                            isOn = false;
                        }
                        else
                        {
                            clip = clip * releaseIndex / releaseTime;
                            releaseIndex--;
                        }
                    }
                    left[_bufferIndex] = clip;
                    right[_bufferIndex] = clip;
                    _bufferIndex++;
                    time++;
                }
                //System.IO.File.AppendAllText(@"C:\Users\Kavun\School\SPRING 12\CSC498\VST\KevinMidi1\KevinMidi1\bin\Debug\debug.txt", "PostLoad\n" + "_bufferIndex: " + _bufferIndex + "\n" + "bufferOffset: " + bufferOffset + "\n" + "Synth samples: " + SynthBuffer.LeftSamples.Count + "\n" + "OutputBufferSize: " + left.SampleCount + "\n\n");
            }
        }

        /// <summary>
        /// Manages a stereo buffer
        /// </summary>
        public class StereoBuffer
        {
            public StereoBuffer(byte noteNo)
            {
                NoteNo = noteNo;
            }

            public byte NoteNo;
            public List<float> LeftSamples = new List<float>();
            public List<float> RightSamples = new List<float>();
        }

    }
}
