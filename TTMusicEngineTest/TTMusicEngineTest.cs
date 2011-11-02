// (c) 2010-2011 TranceTrance.com. Distributed under the FreeBSD license in LICENSE.txt
using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using TTMusicEngine;
using TTMusicEngine.Soundevents;

namespace TTMusicEngine.Test
{
    public class TTMusicEngineTest : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spBatch;
        SpriteFont spFont;
        MusicEngine musicEngine;
        SoundEvent soundScript;
        List<SoundEvent> soundScripts = new List<SoundEvent>();
        int testIndex = 0;
        double timeLastSelected = 0;
        RenderParams rp = new RenderParams();

        public TTMusicEngineTest()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "TTMusicEngineTestContent";
        }

        protected override void Initialize()
        {
            // open the TTMusicEngine
            musicEngine = MusicEngine.GetInstance();
            musicEngine.AudioPath = "../../../../test-audio";
            if (!musicEngine.Initialize())
                throw new Exception(musicEngine.StatusMsg);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spBatch = new SpriteBatch(GraphicsDevice);

            // font
            spFont = Content.Load<SpriteFont>(@"SpriteFont1");

            TestScripts test = new TestScripts();            
            //soundScript = test.Test_Script1();

            // iterate all test methods
            MethodInfo[] am = test.GetType().GetMethods();
            for (int i = 0; i < am.Length; i++ )
            {
                if (am[i].Name.StartsWith("Test_"))
                {
                    soundScripts.Add( (SoundEvent) test.GetType().InvokeMember(am[i].Name,BindingFlags.InvokeMethod,null,test,new object[]{}) );
                }
            }
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // User can select one of the test methods to play
            if ( gameTime.TotalGameTime.TotalSeconds > timeLastSelected + 0.5)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.PageUp))
                {
                    testIndex++;
                    if (testIndex >= soundScripts.Count)
                        testIndex = 0;
                    timeLastSelected = gameTime.TotalGameTime.TotalSeconds;
                    rp.Time = 0;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.PageDown))
                {
                    testIndex--;
                    if (testIndex < 0)
                        testIndex = soundScripts.Count - 1;
                    timeLastSelected = gameTime.TotalGameTime.TotalSeconds;
                    rp.Time = 0;
                }
            }

            // select a sound script and render it.            
            rp.Time += gameTime.ElapsedGameTime.TotalSeconds;
            soundScript = soundScripts[testIndex];
            musicEngine.Render(soundScript,rp);

            // call base update
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            string msg = "Playing test: " + soundScript.Name + "   Time: " + Math.Round(rp.Time, 3) 
                + "  \n(ESC=exit, PgUp/PgDown=select script)";
            spBatch.Begin();
            spBatch.DrawString(spFont, msg, new Vector2(70.0f, 50.0f), Color.AntiqueWhite);
            spBatch.End();

            // call parent class draw
            base.Draw(gameTime);
        }
    }
}
