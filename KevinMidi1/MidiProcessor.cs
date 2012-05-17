using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;
using KevinMidi1.Dmp;

namespace KevinMidi1
{
    /// <summary>
    /// This object performs midi processing for your plugin.
    /// </summary>
    internal sealed class MidiProcessor : IVstMidiProcessor, IVstPluginMidiSource
    {
        private Plugin _plugin;
        private Gain _gain;
        private Transpose _transpose;

        public MidiProcessor(Plugin plugin)
        {
            _plugin = plugin;
            _gain = new Gain(plugin);
            _transpose = new Transpose(plugin);
        }

        public int ChannelCount
        {
            get { return _plugin.ChannelCount; }
        }

        /// <summary>
        /// Midi events are received from the host on this method.
        /// </summary>
        /// <param name="events">A collection with midi events. Never null.</param>
        /// <remarks>
        /// Note that some hosts will only receieve midi events during audio processing.
        /// See also <see cref="IVstPluginAudioProcessor"/>.
        /// </remarks>
        public void Process(VstEventCollection events)
        {
            CurrentEvents = events;
            ProcessCurrentEvents();
        }

        // cache of events (for when syncing up with the AudioProcessor).
        public VstEventCollection CurrentEvents { get; private set; }

        public void ProcessCurrentEvents()
        {
            if (CurrentEvents == null || CurrentEvents.Count == 0) return;
            
            // a plugin must implement IVstPluginMidiSource or this call will throw an exception.
            IVstMidiProcessor midiHost = _plugin.Host.GetInstance<IVstMidiProcessor>();

            // always expect some hosts not to support this.
            if (midiHost != null)
            {
                VstEventCollection outEvents = new VstEventCollection();

                // NOTE: other types of events could be in the collection!
                foreach (VstEvent evnt in CurrentEvents)
                {
                    if (evnt.EventType == VstEventTypes.MidiEvent)
                    {
                        VstMidiEvent midiEvent = (VstMidiEvent)evnt;
                        if ((midiEvent.Data[0] & 0xF0) == 0x80)
                        {
                            _plugin._synth.ProcessNoteOffEvent(midiEvent.Data[1]);
                        }
                        if ((midiEvent.Data[0] & 0xF0) == 0x90)
                        {
                            // note on with velocity = 0 is a note off
                            if (midiEvent.Data[2] == 0)
                            {
                                _plugin._synth.ProcessNoteOffEvent(midiEvent.Data[1]);
                            }
                            else
                            {
                                //midiEvent = _gain.ProcessEvent(midiEvent);
                                //midiEvent = _transpose.ProcessEvent(midiEvent);
                                _plugin._synth.ProcessNoteOnEvent(midiEvent.Data[1]);
                            }
                        }
                    }
                    else
                    {
                        // non VstMidiEvent
                        outEvents.Add(evnt);
                    }
                }
                //midiHost.Process(outEvents);
            }
        }

        #region IVstPluginMidiSource Members

        int IVstPluginMidiSource.ChannelCount
        {
            get { return 16; }
        }

        #endregion
    }
}
