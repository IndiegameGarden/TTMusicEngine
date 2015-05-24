// (c) 2010-2013 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTMusicEngine.Soundevents
{
    /**
     * create a FMOD DSP SoundEvent that can be attached as a child to a SampleSoundEvent
     */
    public class DSPSoundEvent: SoundEvent
    {
        FMOD.DSP _dsp = null;

        public DSPSoundEvent(FMOD.DSP_TYPE dspType)
            : base()
        {
            FMOD.RESULT r = MusicEngine.AudioEngine.createDSPByType(dspType, ref _dsp);
            _dsp.setBypass(true); // later turned on in Render method.
        }

        public DSPSoundEvent(DSPSoundEvent ev)
            : base(ev)
        {
            _dsp = ev._dsp;
        }

        public void SetParameter(int param, double value)
        {
            _dsp.setParameter(param, (float) value);
        }

        internal override void NotifyNewParent(SoundEvent parent)
        {
            base.NotifyNewParent(parent);
            if (parent is SampleSoundEvent)
            {
                // yes, we can attach effect here
                SampleSoundEvent ev = (SampleSoundEvent)parent;
                ev.AddDSP(_dsp);
            }
        }

        public override bool Render(RenderParams parentRp, RenderCanvas canvas)
        {
            if (!Active) return false;
            RenderParams rp = new RenderParams(parentRp);
            AdaptRenderParams(rp);
            RenderChildren(rp, canvas);

            // enable dsp effect only when in right time range
            if (_dsp != null)
            {
                if (rp.Time >= 0 && rp.Time < Duration)
                {
                    _dsp.setBypass(false);
                    return true;
                }
                else
                {
                    _dsp.setBypass(true);                    
                }
            }
            return false;
        }
    }
}
