﻿using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace KevinMidi1
{
    /// <summary>
    /// This object is a dummy AudioProcessor only to be able to output Midi during the Audio processing cycle.
    /// </summary>
    internal sealed class AudioProcessor : VstPluginAudioProcessorBase
    {
        // some defaults
        private static readonly int AudioInputCount = 2;
        private static readonly int AudioOutputCount = 2;
        private static readonly int InitialTailSize = 0;

        private Plugin _plugin;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AudioProcessor(Plugin plugin)
            : base(AudioInputCount, AudioOutputCount, InitialTailSize)
        {
            _plugin = plugin;
        }

        /// <summary>
        /// Called by the host to allow the plugin to process audio samples.
        /// </summary>
        /// <param name="inChannels">Never null.</param>
        /// <param name="outChannels">Never null.</param>
        public override void Process(VstAudioBuffer[] inChannels, VstAudioBuffer[] outChannels)
        {
            // calling the base class transfers input samples to the output channels unchanged (bypass).
            //base.Process(inChannels, outChannels);

            if (_plugin._synth.IsPlaying)
            {
                _plugin._synth.SampleCount = outChannels[0].SampleCount;
                _plugin._synth.PlayAudio(outChannels);
            }
            else // audio thru
            {
                VstAudioBuffer input = inChannels[0];
                VstAudioBuffer output = outChannels[0];

                for (int index = 0; index < output.SampleCount; index++)
                {
                    output[index] = input[index];
                }

                input = inChannels[1];
                output = outChannels[1];

                for (int index = 0; index < output.SampleCount; index++)
                {
                    output[index] = input[index];
                }
            }

            _plugin.MidiProcessor.ProcessCurrentEvents();

        }
    }
}
