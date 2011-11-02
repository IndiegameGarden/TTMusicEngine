// (c) 2010-2011 TranceTrance.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TTMusicEngine
{
    /** 
     * utilities for Beat/Time conversion and music Beat calculations
     */
    public class Beat
    {
         /**
         * convert time value to a 'beat ruler' value (Acid Music studio like)
         */
        static public double TimeToBeat(double time, double bpm)
        {
            double b = time * bpm / 60.0;
            double bfl = Math.Floor(b);
            double bfr = b - bfl;
            bfr *= 0.4;

            // convert to measure.beat value
            double m = Math.Floor(b / 4.0);
            double m_rem = b - m * 4.0;

            //return (bfl + bfr);
            // acid pro beat ruler starts with 1.1 (at time 0.0!)
            return 1 + m + ( (1+m_rem) / 10.0);
        }

        static public double BeatToTime(double beatRulerCount, double bpm)
        {
            double b = Math.Floor(beatRulerCount) - 1;
            double bfr = beatRulerCount - (b + 1);
            bfr -= 0.1;
            bfr *= 2.5; // scale from 0.0-0.3999 range to 0-1
            return ((b*4 + bfr*4) * 60.0 / bpm);
        }

    
    }
}
