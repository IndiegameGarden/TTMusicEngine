// (c) 2010-2013 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TTMusicEngine;
using TTMusicEngine.Soundevents;

namespace TTMusicEngine
{
    /** <summary>Different testing sound scripts. Each Test_*() method contains one specific test script</summary>
     */
    public class TestScripts
    {

        /**
         * simple test of generic Repeat feature - on a SoundEvent. Not directly applied on SampleSoundEvent here.
         */
        public SoundEvent Test_Repeat()
        {
            SoundEvent soundScript = new SoundEvent("Test_Repeat");
            // try a once event
            SampleSoundEvent evDing = new SampleSoundEvent("ding.wav");
            evDing.Repeat = 1;
            soundScript.AddEvent(1, evDing);

            SampleSoundEvent evDing2 = new SampleSoundEvent(evDing);
            SoundEvent dingHolderEv = new SoundEvent();
            dingHolderEv.AddEvent(0, evDing2);
            dingHolderEv.Repeat = 10;
            soundScript.AddEvent(5, dingHolderEv);

            soundScript.UpdateDuration(60);
            return soundScript;
        }

        /**
         * simple test of Repeat feature for SampleSoundEvent - using the better audio-engine looping instead!
         * there is an audible difference - this one better than Test_Repeat.
         */
        public SoundEvent Test_RepeatForSampleSoundEvents()
        {
            SoundEvent soundScript = new SoundEvent("Test_RepeatForSampleSoundEvents");
            // try a once event
            SampleSoundEvent evDing = new SampleSoundEvent("ding.wav");
            evDing.Repeat = 1;
            soundScript.AddEvent(1, evDing);

            SampleSoundEvent evDing2 = new SampleSoundEvent(evDing);
            evDing2.Repeat = 10;
            soundScript.AddEvent(5, evDing2);

            soundScript.UpdateDuration(60);
            return soundScript;
        }

        /**
         * every 2 seconds, one out of 3 different sounds should play
         */
        public SoundEvent Test_MorphingSoundEvent()
        {
            SoundEvent soundScript = new SoundEvent("Test_MorphingSoundEvent");
            MorphingSoundEvent mEv = new MorphingSoundEvent();

            // component 1 
            SampleSoundEvent evTwoPing = new SampleSoundEvent("synthetic_twoPing.wav");
            // component 2 
            SampleSoundEvent evDing = new SampleSoundEvent("ding.wav");
            evDing.Amplitude = 0.5;
            // comp3 
            SampleSoundEvent evEmerald = new SampleSoundEvent("tail_emerald2.wav");
            evEmerald.Amplitude = 0.2;

            // add comps to the morphing event
            mEv.AddEvent(0, evTwoPing);
            mEv.AddEvent(0, evDing);
            mEv.AddEvent(0, evEmerald);

            // repeat this event lots of times to test
            for(double t= 0; t < 300 ; t+=0.5 )
                soundScript.AddEvent(t, new MorphingSoundEvent(mEv ));

            return soundScript;
        }

        public SoundEvent Test_OverlapInTime()
        {
            SoundEvent soundScript = new SoundEvent("Test_OverlapInTime");
            SampleSoundEvent evDing = new SampleSoundEvent("ding.wav");
            for (double t = 0; t < 300; t += 0.333333)
                soundScript.AddEvent(t, new SampleSoundEvent(evDing));
            return soundScript;
        }

        public SoundEvent Test_Speed()
        {
            SoundEvent soundScript = new SoundEvent("Test_Speed");
            SampleSoundEvent evDing = new SampleSoundEvent("synthetic_twoPing.wav");
            for (double t = 0; t < 100; t += 2.0)
                soundScript.AddEvent(t, new SampleSoundEvent(evDing));
            soundScript.Speed = 2.0; // twice as fast
            return soundScript;
        }

        /** oscillator with varying frequency */
        public SoundEvent Test_OscFrequency()
        {
            SoundEvent soundScript = new SoundEvent("Test_OscFrequency");
            OscSoundEvent oscEv = new OscSoundEvent(320);
            oscEv.SetOscType(OscSoundEvent.OscType.SINE);
            //oscEv.SetParameter((int)FMOD.DSP_OSCILLATOR.TYPE, 1);
            oscEv.Amplitude = 0.15;
            oscEv.UpdateDuration(6);
            SignalSoundEvent sEv = new SignalSoundEvent(SignalSoundEvent.Modifier.AMPLITUDE,
                new Signal(new List<double>() { 0, 0,  5.5,  1.0, 6.0, 0, 7.0, 0 }));
            SignalSoundEvent sEv2 = new SignalSoundEvent(SignalSoundEvent.Modifier.DSP_PARAM,
                new Signal(new List<double>() { 0, 320, 1.0, 543, 2, 109, 2.5, 737.3, 6.0, 210 }), (int)FMOD.DSP_OSCILLATOR.RATE);
            oscEv.AddEvent(0.0, sEv);
            oscEv.AddEvent(0.0, sEv2);
            soundScript.AddEvent(1.0, oscEv);
            //soundScript.AddEvent(3.0, new OscSoundEvent(oscEv));
            soundScript.UpdateDuration(10);
            return soundScript;
        }

        /** 2 overlapping oscillators - does not work */
        public SoundEvent Test_TwoOscillators()
        {
            SoundEvent soundScript = new SoundEvent("Test_TwoOscillators");
            OscSoundEvent oscEv = new OscSoundEvent(320);
            oscEv.Amplitude = 0.15;
            //oscEv.UpdateDuration(6);
            SignalSoundEvent sEv = new SignalSoundEvent(SignalSoundEvent.Modifier.AMPLITUDE,
                new Signal(new List<double>() { 0, 0, 0.5, 1.0, 6.5, 1.0, 7.0, 0 }));
            SignalSoundEvent sEv2 = new SignalSoundEvent(SignalSoundEvent.Modifier.DSP_PARAM,
                new Signal(new List<double>() { 0, 170, 7.0, 450 }), (int)FMOD.DSP_OSCILLATOR.RATE);
            oscEv.AddEvent(0.0, sEv);
            oscEv.AddEvent(0.0, sEv2);
            OscSoundEvent oscEv2 = new OscSoundEvent(oscEv);
            oscEv2.AddEvent(0.0, new SignalSoundEvent(sEv));
            oscEv2.AddEvent(0.0, new SignalSoundEvent(sEv2));
            soundScript.AddEvent(1.0, oscEv);
            soundScript.AddEvent(4.0, oscEv2);
            
            return soundScript;
        }

        /**
         * complex script sequence for testing
         */
        public SoundEvent Test_Script1()
        {
            SoundEvent soundScript = new SoundEvent("Test_Script1");
            SampleSoundEvent evDing = new SampleSoundEvent("ding.wav");
            SampleSoundEvent evOrgan = new SampleSoundEvent("hammond-loop.wav");

            // dsp effects
            SoundEvent evEcho = new DSPSoundEvent(FMOD.DSP_TYPE.ECHO);
            //evEmerald.AddEvent(0,echo1);
            SoundEvent evChorus = new DSPSoundEvent(FMOD.DSP_TYPE.CHORUS);
            evChorus.UpdateDuration(10.0);
            evOrgan.AddEvent(evOrgan.Duration, evChorus);            
            DSPSoundEvent evLP = new DSPSoundEvent(FMOD.DSP_TYPE.LOWPASS);
            evLP.UpdateDuration(10.0);
            Signal sigLP = new Signal(new List<double>() { 0,300, 3,1500, 7,300, 20,15050 });
            SignalSoundEvent evLPsig = new SignalSoundEvent(SignalSoundEvent.Modifier.DSP_PARAM, sigLP, (int)FMOD.DSP_LOWPASS.CUTOFF);
            evLP.AddEvent(0, evLPsig);
            evOrgan.AddEvent(0, evLP);

            evOrgan.Amplitude = 0.4;
            evOrgan.Repeat = 10;
            
            soundScript.AddEvent(0.5, evOrgan);

            // create an event defining a signal, which modifies amplitude - linear fade in!
            Signal sig = new Signal(new List<double>() {0,0, 4,1  } );
            SignalSoundEvent evsig = new SignalSoundEvent(SignalSoundEvent.Modifier.AMPLITUDE, sig);
            evOrgan.AddEvent(0.0, evsig);

            // composite event
            SoundEvent evc = new SoundEvent();
            evc.AddEvent(0, evDing);
            evDing = new SampleSoundEvent(evDing); evDing.Amplitude = 0.8; evDing.Pan = -0.5;
            evc.AddEvent(0.33, evDing);
            evDing = new SampleSoundEvent(evDing); evDing.Amplitude = 0.6; evDing.Pan = 0.5;
            evc.AddEvent(0.66, evDing);
            evDing = new SampleSoundEvent(evDing); evDing.Amplitude = 0.4; evDing.Pan = 0.0;
            evc.AddEvent(1.0, evDing);
            evDing = new SampleSoundEvent(evDing); evDing.Amplitude = 0.3;
            evc.AddEvent(1.33, evDing);
            evDing = new SampleSoundEvent(evDing); evDing.Amplitude = 0.25;
            evc.AddEvent(1.66, evDing);

            // add it to script
            soundScript.AddEvent(2.0, evc);
            soundScript.AddEvent(2.5, evc );

            evDing = new SampleSoundEvent(evDing); evDing.Amplitude = 0.91; evDing.Pan = -0.9;
            soundScript.AddEvent(0, evDing);
            evDing = new SampleSoundEvent(evDing); evDing.Amplitude = 0.92; evDing.Pan = +1;
            soundScript.AddEvent(0.5, evDing);
            evDing = new SampleSoundEvent(evDing); evDing.Amplitude = 0.93; evDing.Pan = -1;
            soundScript.AddEvent(1.0, evDing);

            // try oscillator
            OscSoundEvent oscEv = new OscSoundEvent(320);
            oscEv.Amplitude = 0.1;
            SignalSoundEvent sEv = new SignalSoundEvent( SignalSoundEvent.Modifier.AMPLITUDE , 
                new Signal( new List<double>() { 0,0 , 0.5,1 , 2,1 , 2.5,0 , 3.0,0 } ) );
            SignalSoundEvent sEv2 = new SignalSoundEvent(SignalSoundEvent.Modifier.DSP_PARAM,
                new Signal(new List<double>() { 0, 320, 0.5, 543, 2, 129, 2.5, 192.3, 3.0, 410 }), (int) FMOD.DSP_OSCILLATOR.RATE);
            oscEv.AddEvent(0.0, sEv);
            oscEv.AddEvent(0.0, sEv2);
            soundScript.AddEvent(3.0, oscEv);
            soundScript.AddEvent(7.0, oscEv);
            soundScript.AddEvent(11.0, oscEv);
            soundScript.AddEvent(15.0, oscEv);

            return soundScript;

        }     
    }
}
