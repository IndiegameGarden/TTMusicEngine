// (c) 2010-2013 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTMusicEngine.Soundevents
{
    /**
     * a SoundEvent that morphs (changes itself) each time it is rendered.
     * Used to eg to play a slightly different sample each time the event
     * is rendered.
     * WARNING only works if one or different instances are called non-overlapping in time !!!
     */
    public class MorphingSoundEvent : SoundEvent
    {

        SoundEvent _childSelected = null;

        public MorphingSoundEvent()
            : base()
        {
        }

        /** copy constructor */
        public MorphingSoundEvent(MorphingSoundEvent other)
            : base(other)
        {
        }

        public override void AddEvent(double t, SoundEvent child)
        {
            if (t > 0) throw new Exception("MorphingSoundEvent does not support t > 0");
            base.AddEvent(t, child);
            // specific behaviour: _all_ child events are as long as the longest child already there.
            double longestChildDuration = this.Duration; // as a side effect of AddEvent, this.Duration is updated.

            // update _all_ children now
            foreach (KeyValuePair<double, List<SoundEvent>> pair in _children)
            {
                foreach (SoundEvent c in pair.Value)
                {
                    c.UpdateDuration(longestChildDuration);
                }
            }
        }

        /**
        * called internally from Render() method, by any event that needs/wants to render its child events
        */
        internal override bool RenderChildren(RenderParams rp, RenderCanvas canvas)
        {
            if (_children.Count() == 0)
                return false;

            // check if a child for rendering is already selected: if not, select one
            if (_childSelected == null)
            {
                // select one child effect random or in sequence, and see if it has to be played now
                double myRpTime = rp.Time;
                Random rnd = new Random();
                int idx = rnd.Next(_children.Count());
                KeyValuePair<double, List<SoundEvent>> kvPair = _children.ElementAt(idx);
                List<SoundEvent> evs = kvPair.Value;
                idx = rnd.Next(evs.Count());
                _childSelected = evs.ElementAt(idx);
            }

            SoundEvent ev = _childSelected;
            
            // check if we are in the time range where the effect can work
            double evEndTime = 0.0 + ev.Duration; ///_timeSpeedupFactor ;
            if (evEndTime + RENDER_SAFETY_MARGIN_SEC > rp.Time)  // if end time lies in the future...
            {
                // --render the child effect, shifted in time/pan/amplitude by use of rp.
                RenderParams rpChild = new RenderParams(rp);
                rpChild.Time = (rp.Time - 0.0);  // only time is set for each child separately. Rest is same.
                bool wasActive= ev.Render(rpChild, canvas);
                if (!wasActive)
                {
                    _childSelected = null; // reset back - next time, another event may be selected into _childSelected
                }
                return wasActive;
            }
            else
            {
                _childSelected = null; // reset back - next time, another event may be selected into _childSelected
            }
            return false;
        } // end method


    }
}
