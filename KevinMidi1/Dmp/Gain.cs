﻿using Jacobi.Vst.Core;
using Jacobi.Vst.Framework;
namespace KevinMidi1.Dmp
{
    internal sealed class Gain
    {
        private static readonly string ParameterCategoryName = "Gain";

        private VstParameterManager _gainMgr;

        private Plugin _plugin;

        public Gain(Plugin plugin)
        {
            _plugin = plugin;

            InitializeParameters();
        }

        private void InitializeParameters()
        {
            // all parameter definitions are added to a central list.
            VstParameterInfoCollection parameterInfos = _plugin.PluginPrograms.ParameterInfos;

            // retrieve the category for all delay parameters.
            VstParameterCategory paramCategory =
                _plugin.PluginPrograms.GetParameterCategory(ParameterCategoryName);

            // delay time parameter
            VstParameterInfo paramInfo = new VstParameterInfo();
            paramInfo.Category = paramCategory;
            paramInfo.CanBeAutomated = true;
            paramInfo.Name = "Gain";
            paramInfo.Label = "Db";
            paramInfo.ShortLabel = "Db";
            paramInfo.MinInteger = -100;
            paramInfo.MaxInteger = 100;
            paramInfo.LargeStepFloat = 20.0f;
            paramInfo.SmallStepFloat = 1.0f;
            paramInfo.StepFloat = 10.0f;
            paramInfo.DefaultValue = 0.0f;
            _gainMgr = new VstParameterManager(paramInfo);
            VstParameterNormalizationInfo.AttachTo(paramInfo);

            parameterInfos.Add(paramInfo);
        }

        public VstMidiEvent ProcessEvent(VstMidiEvent inEvent)
        {
            if (!MidiHelper.IsNoteOff(inEvent.Data) && !MidiHelper.IsNoteOn(inEvent.Data))
            {
                return inEvent;
            }

            byte[] outData = new byte[4];
            inEvent.Data.CopyTo(outData, 0);

            outData[2] += (byte)_gainMgr.CurrentValue;

            if (outData[2] > 127)
            {
                outData[2] = 127;
            }

            if (outData[2] < 0)
            {
                outData[2] = 0;
            }

            VstMidiEvent outEvent = new VstMidiEvent(
                inEvent.DeltaFrames, inEvent.NoteLength, inEvent.NoteOffset,
                outData, inEvent.Detune, inEvent.NoteOffVelocity);

            return outEvent;
        }
    }
}
