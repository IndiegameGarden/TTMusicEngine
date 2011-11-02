// (c) 2010-2011 TranceTrance.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TTMusicEngine
{
    /**
     * Sinal with linear interpolation
     */
    public class Signal
    {
        protected SortedDictionary<double, double> map = new SortedDictionary<double,double>();

        /** create empty signal */
        public Signal()
        {
        }

        /** create signal from list of (time,value) pairs */
        public Signal(List<double> timeValuePairs)
        {
            int sz = timeValuePairs.Count;
            if ((sz / 2) * 2 != sz)
                throw new Exception("Signal(List<double>) requires even number of double values");
            for (int i = 0; i < sz; i+=2 )
            {
                Set(timeValuePairs[i], timeValuePairs[i + 1]);
            }
        }

        public double Duration
        {
            get { return map.Last().Key; }
        }

        public void Set(double t, double v)
        {
            map[t] = v;
        }

        public double Render(double t)
        {
            double v = Double.NaN;
            double t_prev = map.First().Key;
            double v_prev = map.First().Value;
            foreach (KeyValuePair<double, double> p in map)
            {
                if (t_prev <= t && p.Key >= t)
                {
                    double f = (p.Key - t) / (p.Key - t_prev); // a fraction 0-1
                    v = p.Value * (1-f) + v_prev * f ; // linear interpolation
                    /*if ( t >= 2.5 )
                        Util.Log("sig(" + t + ")=" + v + "\n"); */
                    return v;
                }
                v_prev = p.Value;
                t_prev = p.Key;
            }            
            return v;
        }

    }
}
