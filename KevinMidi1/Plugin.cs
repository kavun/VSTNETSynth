using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
using Jacobi.Vst.Framework.Plugin;

namespace KevinMidi1
{
    /// <summary>
    /// The Plugin root object.
    /// </summary>
    internal sealed class Plugin : VstPluginWithInterfaceManagerBase
    {

        private static readonly int UniquePluginId = new FourCharacterCode("4643").ToInt32();

        private static readonly string PluginName = "KevinMidi1";

        private static readonly string ProductName = "KevinMidi";

        private static readonly string VendorName = "Kevin Reed";
        /// <summary>
        /// ex: 1.0.0.0
        /// </summary>
        private static readonly int PluginVersion = 1000;

        /// <summary>
        /// Initializes the one an only instance of the Plugin root object.
        /// </summary>
        public Plugin()
            : base(PluginName,
            new VstProductInfo(ProductName, VendorName, PluginVersion),
                // Making a synth?
                VstPluginCategory.Synth,
                VstPluginCapabilities.NoSoundInStop,
                // initial delay: number of samples your plugin lags behind.
                0,
                UniquePluginId)
        {
            _synth = new Synth();
        }

        public Synth _synth { get; private set; }
        /// <summary>
        /// Gets the audio processor object.
        /// </summary>
        public AudioProcessor AudioProcessor
        {
            get { return GetInstance<AudioProcessor>(); }
        }

        /// <summary>
        /// Gets the midi processor object.
        /// </summary>
        public MidiProcessor MidiProcessor
        {
            get { return GetInstance<MidiProcessor>(); }
        }

        /// <summary>
        /// Gets the plugin editor object.
        /// </summary>
        public PluginEditor PluginEditor
        {
            get { return GetInstance<PluginEditor>(); }
        }

        /// <summary>
        /// Gets the plugin programs object.
        /// </summary>
        public PluginPrograms PluginPrograms
        {
            get { return GetInstance<PluginPrograms>(); }
        }

        /// <summary>
        /// Implement this when you do audio processing.
        /// </summary>
        /// <param name="instance">A previous instance returned by this method. 
        /// When non-null, return a thread-safe version (or wrapper) for the object.</param>
        /// <returns>Returns null when not supported by the plugin.</returns>
        protected override IVstPluginAudioProcessor CreateAudioProcessor(IVstPluginAudioProcessor instance)
        {
            // Dont expose an AudioProcessor if Midi is output in the MidiProcessor
            //if (!MidiProcessor.SyncWithAudioProcessor) return null;

            if (instance == null)
            {
                return new AudioProcessor(this);
            }

            // TODO: implement a thread-safe wrapper.
            return base.CreateAudioProcessor(instance);
        }

        /// <summary>
        /// Implement this when you do midi processing.
        /// </summary>
        /// <param name="instance">A previous instance returned by this method. 
        /// When non-null, return a thread-safe version (or wrapper) for the object.</param>
        /// <returns>Returns null when not supported by the plugin.</returns>
        protected override IVstMidiProcessor CreateMidiProcessor(IVstMidiProcessor instance)
        {
            if (instance == null)
            {
                return new MidiProcessor(this);
            }

            // TODO: implement a thread-safe wrapper.
            return base.CreateMidiProcessor(instance);
        }

        /// <summary>
        /// Implement this when you output midi events to the host.
        /// </summary>
        /// <param name="instance">A previous instance returned by this method. 
        /// When non-null, return a thread-safe version (or wrapper) for the object.</param>
        /// <returns>Returns null when not supported by the plugin.</returns>
        protected override IVstPluginMidiSource CreateMidiSource(IVstPluginMidiSource instance)
        {
            // we implement this interface on out midi processor.
            return (IVstPluginMidiSource)MidiProcessor;
        }

        /// <summary>
        /// Implement this when you need a custom editor (UI).
        /// </summary>
        /// <param name="instance">A previous instance returned by this method. 
        /// When non-null, return a thread-safe version (or wrapper) for the object.</param>
        /// <returns>Returns null when not supported by the plugin.</returns>
        protected override IVstPluginEditor CreateEditor(IVstPluginEditor instance)
        {
            if (instance == null)
            {
                return new PluginEditor(this);
            }

            // TODO: implement a thread-safe wrapper.
            return base.CreateEditor(instance);
        }

        /// <summary>
        /// Implement this when you implement plugin programs or presets.
        /// </summary>
        /// <param name="instance">A previous instance returned by this method. 
        /// When non-null, return a thread-safe version (or wrapper) for the object.</param>
        /// <returns>Returns null when not supported by the plugin.</returns>
        protected override IVstPluginPrograms CreatePrograms(IVstPluginPrograms instance)
        {
            if (instance == null)
            {
                return new PluginPrograms(this);
            }

            return base.CreatePrograms(instance);
        }

        /// <summary>
        /// Returns the channel count as reported by the host
        /// </summary>
        public int ChannelCount
        {
            get
            {
                IVstMidiProcessor midiProcessor = null;

                if (Host != null)
                {
                    midiProcessor = Host.GetInstance<IVstMidiProcessor>();
                }


                if (midiProcessor != null)
                {
                    return midiProcessor.ChannelCount;
                }

                return 0;
            }
        }
    }
}
