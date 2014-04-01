// (c) 2010-2013 IndiegameGarden.com. Distributed under the FreeBSD license in LICENSE.txt
using System;
using System.Runtime.InteropServices;

namespace TTMusicEngine.Test
{
    static class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
        
        /// <summary>
        /// The main entry point for the test application.
        /// </summary>
        static void Main(string[] args)
        {
            using (TTMusicEngineTest game = new TTMusicEngineTest())
            {
                try
                {
                    game.Run();
                }
                catch (Exception e)
                {
                    MessageBox(new IntPtr(0), "Error - " + e.ToString(), "TTMusicEngineTest", 0);
                }
            }

        }

    }
}

