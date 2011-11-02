// (c) 2010-2011 TranceTrance.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTMusicEngine
{
    /**
     * the RenderCanvas is an object that SoundEvents can 'paint' to. SoundEvents higher up in the hierarchy
     * can then use the results in the canvas, while rendering themselves. 
     * For example: a SignalSoundEvent sigEv that modifies sound Amplitude, writes that modified amplitude into
     * the RenderCanvas which is then used by the parent of sigEv which is a SampleSoundEvent, to modify its
     * amplitude.
     */
    public class RenderCanvas
    {
        public double   AmplMultiply = 1.0,
                        AmplAdd = 0.0,
                        Pan = 0.0,
                        TimeAdd = 0.0;
        // a marker to let an event feedback its real playing position back to the app
        public double TimeMarker = 0.0;

        public RenderCanvas()
        {
        }

    }
}
