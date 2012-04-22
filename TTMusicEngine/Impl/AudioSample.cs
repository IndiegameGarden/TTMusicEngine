// (c) 2010-2011 TranceTrance.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using TTMusicEngine;

namespace TTMusicEngine.Impl
{
    /**
     * audio sample to be rendered to the FMOD engine. The class also keeps
     *  track of book-keeping of playing instances of all AudioSamples.
     */
    internal class AudioSample
    {
        /** ref to an FMOD playable sound */
        FMOD.Sound  _sound = null;
        //List<FMOD.DSP> _dspList = new List<FMOD.DSP>();
        double _soundDuration = 0.0;
        string _fn = ""; // filename, keep for the record

        /** currently playing sample instances */
        protected Dictionary<uint, FMOD.Channel> _nowPlayingList = new Dictionary<uint, FMOD.Channel>();
        
        /**
         * <exception cref="ContentLoadException">if the file could not be loaded</exception>
         */
        public AudioSample(string fn) 
        {
            if (MusicEngine.AudioEngine != null)
            {
                _fn = MusicEngine.GetInstance().AudioPath + "\\" + fn;
                createFMODSound(_fn);
            }
        }

        /**
         * <summary>copy constructor - uses same sample, but loads again </summary>
         * <exception cref="ContentLoadException">if the file could not be loaded</exception>
         */
        public AudioSample(AudioSample other)
        {
            this._fn = other._fn;
            createFMODSound(this._fn);
        }

        /** repeats are not taken into account in Duration. THat's for the SampleSoundEvent to do. */
        public double Duration {
            get { return _soundDuration; }
        }

        /** get the current play time OF THE FIRST (OLDEST) PLAYING INSTANCE of this AudioSample 
         * useful in case there is only one playing instance at a time of a sample.
         */
        public double CurrentPlayTime
        {
            get
            {
                if (_nowPlayingList.Count == 0)
                    return 0.0;
                // check playing time
                uint playPosMs = 0;
                FMOD.Channel channel = _nowPlayingList.First().Value;
                bool isPlaying = false;
                channel.isPlaying(ref isPlaying);
                if (!isPlaying)
                    return 0.0;
                channel.getPosition(ref playPosMs, FMOD.TIMEUNIT.MS);
                return (((double)playPosMs) / 1000.0);
            }

        }

        void createFMODSound(string fn)
        {        
            // load the _sound
            FMOD.RESULT r;
            r = MusicEngine.AudioEngine.createSound(fn, FMOD.MODE.SOFTWARE, ref _sound);
            
            if (Util.ERRCHECK(r))
            {
                throw new ContentLoadException(Util.ERRMSG(r));
            }
            else
            {
                if (_sound != null)
                {
                    r = _sound.setMode(FMOD.MODE.LOOP_NORMAL);  // enable loop functionality, 
                    Util.ERRCHECK(r);
                    r = _sound.setLoopCount(0);                 // but do not loop by default
                    Util.ERRCHECK(r);

                    // determine sample duration
                    uint l = 0;
                    r = _sound.getLength(ref l, FMOD.TIMEUNIT.MS);
                    if(!Util.ERRCHECK(r))
                        _soundDuration = ((double)l) / 1000.0;
                }
                else
                {
                    Util.Log("AudioSample.createFMODSound(): Error, _sound is null.");
                }
            }
        }

        FMOD.Channel PlaySample(RenderParams rp, RenderCanvas canvas, List<FMOD.DSP> dspList )
        {
            FMOD.Channel channel = null;
            FMOD.RESULT r  = MusicEngine.AudioEngine.playSound(FMOD.CHANNELINDEX.FREE, _sound, true, ref channel);
            if (r == FMOD.RESULT.OK && channel != null)
            {
                // set regular play properties 
                AdaptChannelSettings(channel, rp, canvas);

                // set play position
                uint tMs = (uint)Math.Round(rp.Time * 1000.0);
                if (tMs > 0)
                {  // only set position if there is a need
                    r = channel.setPosition(tMs, FMOD.TIMEUNIT.MS);
#if DEBUG
                    Util.Log("   setPos(" + tMs + ")");
#endif
                }

                // set optional DSP unit(s) on channel
                if (r == FMOD.RESULT.OK)
                {
                    FMOD.DSPConnection conn = null;
                    foreach (FMOD.DSP d in dspList)
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
            Util.ERRCHECK(r);
            return channel;
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

        /**
         * called when this sample/effect should render itself, possibly spawning a new
         * playing instance or modifying an already playing instance.
         */
        internal void Render(RenderParams rp, RenderCanvas canvas, List<FMOD.DSP> dspList , int audioRepeats)
        {
#if DEBUG
            Util.Log("Render HID=" + rp.HierarchyID + " T=" + Math.Round(rp.Time,3) + " AbsT=" + Math.Round(rp.AbsTime,3) + " A="+ Math.Round(rp.Ampl,3) + "\n");
#endif
            // check if duration is not exceeded
            if (rp.Time > _soundDuration * ((double)audioRepeats) )
                return;

            bool wasPlaying = _nowPlayingList.ContainsKey(rp.HierarchyID);
            FMOD.Channel channel = null;
            FMOD.RESULT r;

            if (wasPlaying)
            {
                channel = _nowPlayingList[rp.HierarchyID];
                // check if still playing now
                bool isPlayingNow = false;
                r = channel.isPlaying(ref isPlayingNow);
                //Util.ERRCHECK(r); // TODO is this needed?
                if(isPlayingNow)
                {   // if so, adapt sample properties only.
                    AdaptChannelSettings(channel, rp, canvas);
                    
                    // check playing time
                    uint playPosMs = 0;
                    uint idealPlayPosMs = (uint)(rp.Time * 1000.0);
                    channel.getPosition(ref playPosMs, FMOD.TIMEUNIT.MS);
                    
                    if (Math.Abs(playPosMs - idealPlayPosMs) > 5000 && idealPlayPosMs >= 0)  // FIXME specify error margin better, somewhere?
                    {
                        //FIXME HACK enable tracking when needed !!! below.
                        channel.setPosition(idealPlayPosMs, FMOD.TIMEUNIT.MS);
                        playPosMs = idealPlayPosMs;
                    }
                    // store current pos on canvas
                    if (canvas.TimeMarker == 0)
                    {
                        canvas.TimeMarker = ((double)playPosMs) / 1000.0;
                    }
                }
                else
                {   // if not anymore, remove from list
                    _nowPlayingList.Remove(rp.HierarchyID);
                }
            }
            else
            {   // was not playing but should be rendered - hence, initiate playing now
                if (rp.Time < _soundDuration - 0.050 ) // extra safety margin - do not start if close to end. TODO configurable time?
                {
                    channel = PlaySample(rp, canvas, dspList);
                    channel.setLoopCount(audioRepeats - 1);
                    if (channel != null)
                    {
#if DEBUG
                        Util.Log("Play   HID=" + rp.HierarchyID + " T=" + Math.Round(rp.Time, 3) + " AbsT=" + Math.Round(rp.AbsTime, 3) + " A=" + Math.Round(rp.Ampl, 3) + "\n");
#endif
                        // store playing sound in the table
                        _nowPlayingList[rp.HierarchyID] = channel;
                    }
                    else
                    {
                        Util.Log("Play FAILED rp.H-ID=" + rp.HierarchyID + " rp.Time=" + rp.Time + " rp.AbsTime=" + rp.AbsTime + "\n");
                    }

                }
            }
        }

    }
}
