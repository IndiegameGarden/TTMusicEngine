// (c) 2010-2011 TranceTrance.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTMusicEngine
{
    /**
     * Set of sound/music rendering parameters, used to call the Render methods
     * in the MusicEngine. Also internally used. Only contains a very basic set of
     * parameters to influence sound (Ampl and Pan). Many others (eg DSP effects) are setup in a different way by
     * attaching custom modifier-SoundEvents to a SoundEvent.
     */
    public class RenderParams
    {
        // !!!!!!!! WARNING! WARNING! if adding local vars, update the SetTo method also !!!!
        public double   Time = 0.0,
                        Ampl = 1.0,
                        Pan = 0.0;
        internal double AbsTime = 0.0;
        /** render setting specifying how far ahead future effects should already be rendered. 0.0 by default. */
        public double RenderAheadTime = 0.0;

        /** an ID that uniquely identifies the hierarchy of events passed so far during rendering the effects tree */
        internal uint     HierarchyID = 1;


        /** create new inst with default fields */
        public RenderParams()
        {
        }

        public RenderParams(RenderParams other)
        {
            this.SetTo(other);
        }

        /** clone settings of another into this object! This is a way to avoid excessive new object creation. Supports object re-use */
        public void SetTo(RenderParams other)
        {
            this.Time = other.Time;
            this.AbsTime = other.AbsTime;
            this.Ampl = other.Ampl;
            this.Pan = other.Pan;
            this.HierarchyID = other.HierarchyID;
            this.RenderAheadTime = other.RenderAheadTime;
        }

    }
}
