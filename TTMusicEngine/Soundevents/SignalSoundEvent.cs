// (c) 2010-2011 TranceTrance.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTMusicEngine.Soundevents
{
    public class SignalSoundEvent : SoundEvent
    {
        /** Modifier enum defines how a signal may modify a playing (parent) SoundEvent */
        public enum Modifier : int
        { NONE, AMPLITUDE, PAN, AMPLITUDE_ADDITIVE, DSP_PARAM };


        // For signal-containing events - a signal goes here
        Signal _sig = null;
        // Indicates what signal _sig intends to modify during rendering
        SignalSoundEvent.Modifier _modif = SignalSoundEvent.Modifier.NONE;
        int _paramNumber = 0;
        // ref to dsp parent in case this event tweaks DSP parameters during render
        DSPSoundEvent _parentsDSP = null;
        OscSoundEvent _parentsOsc = null;

         /**
         * create an event containing a signal, which modifies properties of the parent
         * event during rendering. Modifier types to be chosen from SignalSoundEvent.Modifier
         */
        public SignalSoundEvent(SignalSoundEvent.Modifier modifierType , Signal sig): base()
        {
            _sig = sig;
            _modif = modifierType;
            UpdateDuration(_sig.Duration);
        }

        /**
        * create an event containing a signal, which modifies a parameter property of a DSP
        * event during rendering. Modifier type must be SignalSoundEvent.Modifier.PARAM !
        */
        public SignalSoundEvent(SignalSoundEvent.Modifier modifierType, Signal sig, int paramNumber)
            : base()
        {
            _sig = sig;
            _modif = modifierType;
            _paramNumber = paramNumber;
            UpdateDuration(_sig.Duration);
        }

        public SignalSoundEvent(SignalSoundEvent ev)
            : base(ev)
        {
            _sig = ev._sig;
            _modif = ev._modif;
            _paramNumber = ev._paramNumber;
            _parentsDSP = ev._parentsDSP;
            _parentsOsc = ev._parentsOsc;
        }

        internal override void NotifyNewParent(SoundEvent parent)
        {
            base.NotifyNewParent(parent);
            if ( _modif == Modifier.DSP_PARAM )
            {
                if (parent is DSPSoundEvent )
                    _parentsDSP = (DSPSoundEvent)parent;
                if (parent is OscSoundEvent)
                    _parentsOsc = (OscSoundEvent)parent;
            }
            // else TODO

        }

        public override bool Render(RenderParams parentRp, RenderCanvas canvas)
        {
            if (!Active) return false; // skip if not active
            RenderParams rp = new RenderParams(parentRp);
            AdaptRenderParams(rp);
            RenderChildren(rp, canvas);

            if (rp.Time >= 0 && rp.Time < Duration && _sig != null)
            {
                double s = _sig.Render(rp.Time);
                switch (_modif)
                {
                    case Modifier.DSP_PARAM:
                        if (_parentsDSP != null)
                        {
                            _parentsDSP.SetParameter(_paramNumber, s);
                        }
                        if (_parentsOsc != null)
                        {
                            _parentsOsc.SetParameter(_paramNumber, s);
                        }

                        break;
                    case Modifier.NONE:
                        break;
                    case Modifier.AMPLITUDE:
                        canvas.AmplMultiply *= s;
                        break;
                    case Modifier.AMPLITUDE_ADDITIVE:
                        canvas.AmplMultiply += s;
                        break;
                    case Modifier.PAN:
                        canvas.Pan += s;
                        break;
                }
                return true;
            }
            return false;

        } // end method

    }
}
