// (c) 2010-2011 TranceTrance.com. Distributed under the FreeBSD license in LICENSE.txt
using System;
using System.Collections.Generic;
using TTMusicEngine;
using TTMusicEngine.Impl;

namespace TTMusicEngine.Soundevents
{
    public class SampleSoundEvent: SoundEvent
    {
        AudioSample _audio = null;
        int _audioRepeats = 1;
        List<FMOD.DSP> _dspList = new List<FMOD.DSP>();

         /**
         * construct event that specifies an audio sample, use for
         * new (not yet loaded) audio samples typically.
         * <exception cref="ContentLoadException">if the file could not be loaded</exception>
         */
        public SampleSoundEvent(string fn) : base()
        {
            _audio = new AudioSample(fn);
            UpdateDuration(_audio.Duration);
        }

        /**
         * constructor that saves memory by re-using the internal AudioSample obj
         * of another SampleSoundEvent
         */
        public SampleSoundEvent(SampleSoundEvent ev) : base(ev)
        {
            _audio = ev._audio;
            UpdateDuration(_audio.Duration);
        }

        public override int Repeat
        {
            set
            {
                //base.Repeat = value;
                UpdateDuration(value * _duration);
                _repeats = 1;
                _audioRepeats = value;
            }
        }

        internal void AddDSP(FMOD.DSP dsp)
        {
            _dspList.Add(dsp);
        }

        public double CurrentPlayTime
        {
            get
            {
                return _audio.CurrentPlayTime;
            }
        }

        public override bool Render(RenderParams parentRp, RenderCanvas canvas)
        {
            if (!Active) return false;
            _rp = new RenderParams(parentRp);
            AdaptRenderParams(_rp);
            //Util.Log("Render HID=" + rp.HierarchyID + " ID=" + this.ID + "\n");

            // make a new canvas - anything painted on here by children, will
            // be used in this SOundEvent.
            RenderCanvas myCanvas = new RenderCanvas();
            // Render to rp/myCanvas, to retrieve adapted render-params based on possible
            // effects attached to me as child nodes.
            RenderChildren(_rp, myCanvas);
            
            // render my audio ... if within the right time
            if (_rp.Time >= 0 && _rp.Time < Duration ) 
            {
                _audio.Render(_rp, myCanvas,_dspList, _audioRepeats);
                canvas.TimeMarker = myCanvas.TimeMarker;
                return true;
            }
            return false;
        }

    }
}
