// (c) 2010-2013 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt
﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace TTMusicEngine.Impl
{
    /**
     * utility functions collection
     */
    internal static class Util
    {
        public static bool Logging = false;

        private static TextWriter logTw = null;        

        /**
         * log msg to file, if Util.Logging = true
         */
        internal static void Log(string s)
        {
            if (!Util.Logging) return;
            if(logTw==null)
                logTw = new StreamWriter("log_TTMusicEngine.txt");
            logTw.Write(s);
            logTw.Flush();
        }

        /**
         * apply a simple Hash function to two values
         * taken and modified from http://en.wikipedia.org/wiki/Jenkins_hash_function
         */
        internal static uint HashValues(uint h1, uint h2)
        {
            uint hash = h1;
            hash += (hash << 10);
            hash ^= (hash >> 6);

	        hash += h2;
            hash += (hash << 10);
            hash ^= (hash >> 6);

            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            return hash;
        }

        /**
         * FMOD error checking util function
        * @return true if error found
        */
        internal static bool ERRCHECK(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                Util.Log("FMOD ERR: " + result + " - " + ERRMSG(result) + "\n" );
                return true;
            }
            return false;
        }

        internal static String ERRMSG(FMOD.RESULT result)
        {
            if (result != FMOD.RESULT.OK)
            {
                return FMOD.Error.String(result);
            }
            return "";
        }

    }
}
