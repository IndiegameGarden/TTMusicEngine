// (c) 2010-2013 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TTMusicEngine.Impl;

namespace TTMusicEngine.Soundevents
{
    /** SoundEvent is the working horse class of TTMusicEngine. It describes single events
     * like a sound playing, but also composite events and even entire sound scripts.
     * SoundEvents can be rendered by the MusicEngine class.
     * FMOD-specific effects can be attached to a SoundEvent as well.
     */
    public class SoundEvent
    {
        // internal vars per instance
        protected RenderParams _rp = null;
        protected double _ampl = 1.0,
                        _duration = 0.0,
                        _speed = 1.0,
                        _pan = 0.0;
        protected bool _isActive = true;
        protected uint _id;
        protected String _name;
        protected int _repeats = 1;

        protected Multimap<double, SoundEvent> _children = new Multimap<double, SoundEvent>();
        protected SoundEvent _parent = null;
        
        // class global vars
        static uint nextID = 1;
        // this defines a 'render margin' for which an event is still Render()ed even after its
        // duration has expired. Purpose: to be able eg for the SoundEvent to switch its state
        // to 'off' or to do some last tasks, before it is not called anymore. Now used for DSP type
        // effects which need to switch off.
        protected const double RENDER_SAFETY_MARGIN_SEC = 0.2;

        void CreateID()
        {
            _id = nextID++;
        }

        /**
         * construct event inst that plays nothing (to be used as a node to add children to)
         */
        public SoundEvent()
        {
            CreateID();
        }

        /**
         * construct event inst that plays nothing, with a given name
         */
        public SoundEvent(String name)
        {
            CreateID();
            Name = name;
        }


        /**
         * copy constructor - single level is copied (children not copied)
         */
        public SoundEvent(SoundEvent ev)
        {
            CreateID();
            _ampl = ev._ampl;
            _children = ev._children.Clone();
            _duration = ev._duration;
            _isActive = ev._isActive;
            _pan = ev._pan;
            _repeats = ev._repeats;
        }

        /** unique id of this SoundEvent */
        public uint ID
        {
            get { return _id; }
        }

        /** human-readable name (optional) */
        public String Name
        {
            get
            {
                if (_name == null)
                    return "SoundEvent" + ID;
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public RenderParams LastRenderParams
        {
            get
            {
                return _rp;
            }
        }

        /** amplitude factor typically 0...1 */
        public double Amplitude
        {
            get { return _ampl; }
            set { _ampl = value ; }
        }

        /** audio panning value -1...0...+1*/
        public double Pan
        {
            get { return _pan; }
            set { _pan = value; }
        }

        /** speed value multiplier, where 1.0 is normal*/
        public double Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        /** whether event is active , true or false*/
        public bool Active
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        /** WARNING try to always use UpdateDuration() to change duration safely! */
        public double Duration
        {
            get { return _duration * ((double)_repeats) / _speed ; }
            // set { _duration = value; }
        }

        /** number of repeats of event. FIXME Repeats is flawed- only works for samples
         * currently and if not started _inside_ the repeat period
         */
        public virtual int Repeat
        {
            get { return _repeats; }
            set {
                _repeats = value;
                // note: duration is auto-updated, see Duration attribute.
            }
        }

        public List<SoundEvent> Children
        {
            get
            {
                List<SoundEvent> l = new List<SoundEvent>();
                IEnumerator<List<SoundEvent>> e = _children.Values.GetEnumerator();
                while (e.MoveNext())
                {
                    l.AddRange(e.Current);
                }
                return l;
            }
        }

        /**
         * Update duration of event to include at least a duration d.
         * If duration was already longer, do nothing (does not shorten)
         * See also Duration attribute
         */
        public bool UpdateDuration(double d)
        {
            if (d > Duration )
            {
                _duration = d / _repeats * _speed ;
                // recursively update my parent's duration as well
                // TODO _parent.UpdateDuration(this.Duration + begin-time);
                return true;
            }
            return false;
        }

        /** add a SoundEvent as a child at specified relative Time t */
        public virtual void AddEvent(double t, SoundEvent child)
        {
            _children.Add(t, child);
            child.NotifyNewParent(this);            
            UpdateDuration(t + child.Duration);            

        }

        /**
         * called internally when adding this event as a child to a parent event 
         */
        internal virtual void NotifyNewParent(SoundEvent parent)
        {
            _parent = parent;
        }

        /**
         * adapt the RenderParams using specific fixed param fields defined in this event.
         * Such as pan, volume, Time (in case of repeats) and a new hierarchy-id.
         */
        internal void AdaptRenderParams(RenderParams rp)
        {
            // adapt rendering-params UID for rendering/children
            rp.HierarchyID = Util.HashValues(rp.HierarchyID, _id);

            // adapt render params with instructions from this event (also passed along to children via rp)
            rp.Pan += _pan;
            rp.Ampl *= _ampl;
            rp.Time *= _speed;

            // if repeats are active, 'wind back' time where needed so that event is played again and again
            if (_repeats > 1 && rp.Time < Duration)
            {
                double Nwindbacks = Math.Floor(rp.Time / _duration);
                rp.Time -= _duration * Nwindbacks;
            }

        }

        /**
         * Render() method entry point for any SoundEvent.
         * returns the adapted canvas, upon which events may 'draw' their output. (e.g. modifiers)
         * Child classes will typically override Render() and have their own implementation.
         * Return true if effect was active and activated at this time, false if not active or activated at this time.
         */
        public virtual bool Render(RenderParams parentRp, RenderCanvas canvas)
        {
            if (!_isActive) return false; // skip if not active
            _rp = new RenderParams(parentRp);
            AdaptRenderParams(_rp);
            return RenderChildren(_rp, canvas);
        }

        /**
         * called internally from Render() method, by any event that needs/wants to render its child events
         */
        internal virtual bool RenderChildren(RenderParams rp, RenderCanvas canvas)
        {
            if (_children.Count() == 0)
                return false;

            // loop all child effects and see if they have to be played now            
            bool wasActive = false;
            foreach (KeyValuePair<double, List<SoundEvent>> pair in _children)
            {
                double evStartTime = pair.Key ;
                
                // check if effect lies in the future. In this case, we can break now. All further child effects
                // will be even later in time so we do not need to iterate these further
                if (evStartTime > rp.Time + rp.RenderAheadTime ) break;

                // loop all events at that specific time 't'
                foreach (SoundEvent ev in pair.Value)
                {
                    // check if we are in the time range where the effect can work
			        double evEndTime = evStartTime + ev.Duration; ///_timeSpeedupFactor ;
			        if (evEndTime + RENDER_SAFETY_MARGIN_SEC > rp.Time )  // if end time lies in the future...
                    {			
				        // --render the child effect, shifted in time/pan/amplitude by use of rp.
                        RenderParams rpChild = new RenderParams(rp);
                        rpChild.Time = (rp.Time - evStartTime);  // only time is set for each child separately. Rest is same.
                        bool wasChildActive = ev.Render(rpChild, canvas);
                        if (wasChildActive)
                            wasActive = true;
			        }
                }
            }// foreach over children
            return wasActive;
        } // end method

    }
}
