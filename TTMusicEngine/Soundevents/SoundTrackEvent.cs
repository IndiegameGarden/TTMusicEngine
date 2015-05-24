// (c) 2010-2013 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTMusicEngine.Soundevents
{
    /**
     * a soundevent that supports creating a music/sound track,
     * playing at a specified BPM rate. BPM is used to convert
     * 'beat time' into real time.
     */
    public class SoundTrackEvent : SoundEvent
    {
        double bpm = 0.0;

        public SoundTrackEvent(double bpm)
        {
            this.bpm = bpm;
        }

        /** 
         * get the current BPM setting of this soundtrack
         */
        public double BPM { get { return bpm; } }

        /**
         * add a new soundevent to this track at specified beat count using ACID Music Studio Beat RUler notation
         * (shorthand notation method name)
         */
        public double b(double beatCount, SoundEvent ev)
        {
            // translate beats to time
            double t = Beat.BeatToTime(beatCount, bpm);
            // add event to track
            this.AddEvent(t, ev);
            return t;
        }

        /**
         * add a new soundevent to this track at specified real time (shorthand notation method name)
         */
        public double t(double time, SoundEvent ev)
        {
            this.AddEvent(time, ev);
            return time;
        }
    }
}
