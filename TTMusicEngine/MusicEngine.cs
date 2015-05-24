// (c) 2010-2013 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt

using System;
using TTMusicEngine.Soundevents;

namespace TTMusicEngine
{
    /**
     * main class
     */
    public class MusicEngine
    {
        // use an audio engine
        private static FMOD.System _fmodEngine = null;
        // singleton
        private static MusicEngine _instance = null;
        // where to load samples from, can be set by user
        string _audioPath = ".";
        string errMsg = "";
        FMOD.RESULT errResult = FMOD.RESULT.OK;
        bool initOk = false;

        public static MusicEngine GetInstance()
        {
            if (_instance == null)
                _instance = new MusicEngine();
            return _instance;
        }

        /**
         * let a client check if engine is properly initialized or not
         * if not, use StatusMsg to retrieve error msg
         */
        public bool Initialized
        {
            get
            {
                return initOk;
            }
        }

        /**
         * retrieve the error message to check what's wrong when Initialized==false
         */
        public string StatusMsg
        {
            get
            {
                return errMsg;
            }
        }

        protected MusicEngine()
        {
            // do nothing
        }

        public bool Initialize()
        {
            initOk = InitAudioEngine();
            return initOk;
        }

        ~MusicEngine()
        {
            Dispose();
        }

        #region Properties
        /**
         * get/set the default load path for audio files
         */
        public string AudioPath
        {
            get { return _audioPath; }
            set { _audioPath = value; }
        }

        /**
         * static global ref to the specific (FMOD) audio-engine
         */
        internal static FMOD.System AudioEngine {
            get { return _fmodEngine; }
        }
        #endregion

        /**
         * render audio in the script 'ev', with parameters 'rp'. Multiple rendered scripts may run
         * in parrallel.
         * Call for example once per frame per script, but no firm requirements posed for this.
         * See also: Update()
         */
        public RenderCanvas Render(SoundEvent ev, RenderParams rp)
        {
            rp.AbsTime = rp.Time;
            RenderCanvas canvas = new RenderCanvas(); // TODO option to recycle objects here, avoid creation?
            ev.Render(rp,canvas);
            
            return canvas;
        }

        /**
         * Engine update, call once per frame
         */
        public void Update()
        {
            if(_fmodEngine != null )
                ERRCHECK(_fmodEngine.update()); // FMOD required update each frame
        }

        private bool InitAudioEngine()
        {
            uint            version = 0;
            FMOD.RESULT     result ;

            try
            {
                result = FMOD.Factory.System_Create(ref _fmodEngine);
                if (ERRCHECK(result)) return false;
            }
            catch (DllNotFoundException)
            {
                errResult = FMOD.RESULT.ERR_PLUGIN_MISSING;
                errMsg = "fmodex.dll not found";
                return false;
            }

            result = _fmodEngine.getVersion(ref version);
            if (ERRCHECK(result)) return false;
            if (version < FMOD.VERSION.number)
            {
                errMsg += "You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + FMOD.VERSION.number.ToString("X") + ".";
                return false;
            }

            // init the FMOD engine
            result = _fmodEngine.init(32, FMOD.INITFLAGS.NORMAL, (IntPtr)null);
            if (ERRCHECK(result)) return false;

            return true;
        }

        public void Dispose()
        {
            if (_fmodEngine != null)
            {
                _fmodEngine.release();
                _fmodEngine = null;
            }
        }

        internal bool ERRCHECK(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                errResult = result;
                errMsg += "FMOD ERR: " + result + " - " + FMOD.Error.String(result) + ". ";
                return true;
            }
            return false;
        }

    }
}
