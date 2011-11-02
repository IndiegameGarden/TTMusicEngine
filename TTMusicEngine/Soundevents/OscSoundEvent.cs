// (c) 2010-2011 TranceTrance.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTMusicEngine.Soundevents
{
    public class OscSoundEvent : SoundEvent 
    {

        public enum OscType : int
            { SINE, SQUARE, SAW_UP, SAW_DOWN, TRIANGLE, NOISE } ;

        FMOD.DSP _dsp = null;
        List<FMOD.DSP> _dspList = new List<FMOD.DSP>();
        double _freq = 0;
        OscType _type = OscType.SINE;

        /** static class-wide list of currently playing sample instances */
        protected static Dictionary<uint, FMOD.Channel> nowPlayingList = new Dictionary<uint, FMOD.Channel>();

        public OscSoundEvent(double freq)
            : base()
        {
            _freq = freq;
            FMOD.RESULT r = MusicEngine.AudioEngine.createDSPByType(FMOD.DSP_TYPE.OSCILLATOR, ref _dsp);
            if (r == FMOD.RESULT.OK)
            {
                _dsp.setParameter((int)FMOD.DSP_OSCILLATOR.RATE, (float)freq); // freq in Hz.
            }
        }

        public OscSoundEvent(OscSoundEvent other)
            : base(other)
        {
            FMOD.RESULT r = MusicEngine.AudioEngine.createDSPByType(FMOD.DSP_TYPE.OSCILLATOR, ref _dsp);
            if (r == FMOD.RESULT.OK)
            {
                _dsp.setParameter((int)FMOD.DSP_OSCILLATOR.RATE, (float) other._freq); // initial freq in Hz.
                SetOscType(other._type);
            }
        }

        public void SetOscType(OscType t)
        {
            SetParameter((int)FMOD.DSP_OSCILLATOR.TYPE, (int)t);
        }

        public void SetParameter(int param, double value)
        {
            _dsp.setParameter(param, (float) value);
        }

        /**
         * adapt a running sample according to renderparams and canvas result
         */
        internal void AdaptChannelSettings(FMOD.Channel channel, RenderParams rp, RenderCanvas canvas)
        {
            double a = rp.Ampl * canvas.AmplMultiply + canvas.AmplAdd;
            double p = rp.Pan + canvas.Pan;
            channel.setVolume((float)a);
            channel.setPan((float)p);
        }

        internal void RenderOsc(RenderParams rp, RenderCanvas canvas)
        {
            //Util.Log("RenOsc HID=" + rp.HierarchyID + " T=" + Math.Round(rp.Time, 3) + " AbsT=" + Math.Round(rp.AbsTime, 3) + " A=" + Math.Round(rp.Ampl, 3) + "\n");
            bool wasPlaying = nowPlayingList.ContainsKey(rp.HierarchyID);
            FMOD.Channel channel = null;
            FMOD.RESULT r;

            if (wasPlaying)
            {
                channel = nowPlayingList[rp.HierarchyID];
                // check if still playing now
                bool isPlayingNow = false;
                r = channel.isPlaying(ref isPlayingNow);
                if (isPlayingNow)
                {   
                    // check if should be stopped now
                    if (rp.Time >= Duration)
                    {
                        channel.stop();
                        nowPlayingList.Remove(rp.HierarchyID);
                    }
                    else
                    {
                        // if still validly playing, adapt channel properties only.
                        AdaptChannelSettings(channel, rp, canvas);
                        //Util.Log("     rp.A=" + Math.Round(rp.Ampl, 3) + " canv.A=" + canvas.AmplMultiply + "\n");
                    }
                }
                else
                {   // if not anymore, remove from list
                    nowPlayingList.Remove(rp.HierarchyID);
                }
            }
            else
            {   // was not playing but should be rendered - hence, initiate playing now
                if (rp.Time < Duration - 0.100) // extra safety margin - do not start if close to end.
                {

                    channel = PlayOsc(rp, canvas);
                    if (channel != null)
                    {                        
                        // store playing sound in the table
                        nowPlayingList[rp.HierarchyID] = channel;
                    }
                    else
                    {
                        //
                    }

                }
            }
        }

        FMOD.Channel PlayOsc(RenderParams rp, RenderCanvas canvas)
        {
            FMOD.Channel channel = null;
            FMOD.RESULT r = MusicEngine.AudioEngine.playDSP(FMOD.CHANNELINDEX.FREE, _dsp, true, ref channel);
            if (r == FMOD.RESULT.OK && channel != null)
            {
                // set regular play properties 
                AdaptChannelSettings(channel, rp, canvas);

                // set optional DSP unit(s) on channel
                if (r == FMOD.RESULT.OK)
                {
                    FMOD.DSPConnection conn = null;
                    foreach (FMOD.DSP d in _dspList)
                    {
                        r = channel.addDSP(d, ref conn); // TODO errcheck
                    }

                    // go - start playing
                    if (r == FMOD.RESULT.OK)
                    {
                        r = channel.setPaused(false);
                    }

                }
            } // if

            return channel;
        }

        public override bool Render(RenderParams parentRp, RenderCanvas canvas)
        {
            if (!Active) return false;
            RenderParams rp = new RenderParams(parentRp);
            AdaptRenderParams(rp);
            RenderCanvas myCanvas = new RenderCanvas(); 
            RenderChildren(rp, myCanvas);

            // render my audio or signal or ... if within the right time
            if (rp.Time >= 0 && (rp.Time < (Duration + RENDER_SAFETY_MARGIN_SEC)) && _dsp != null)
            {
                RenderOsc(rp, myCanvas);
                return true;
            }
            return false;
        }

    }
}
