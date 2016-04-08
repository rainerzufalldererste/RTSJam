using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RTSJam
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        RenderTarget2D rt, lrt;
        Rectangle dispRect;

        public static int width = 1271, height = 676;
        float scale = 1f;

        Rectangle selectionA, selectionA_, selectionB, selectionB_;

        List<GUnit> selectedUnits = new List<GUnit>();
        bool selectionContainsTroops = false;
        GBuilding selectedBuilding = null;

        MenuHandler menuHandler = new MenuHandler();

        MouseState ms, lms;
        KeyboardState ks, lks;
        Vector2 previousSize;
        int placeBuilding = -1;
        int buildingSize = 2;
        public static bool pause = false;
        int frames = 0;
        long seconds = 0;

        public int menu = 0;

        public MainGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Data";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            previousSize = new Vector2(width / scale, height / scale);
            graphics.PreferredBackBufferWidth = (int)(width * scale);
            graphics.PreferredBackBufferHeight = (int)(height * scale);
            graphics.PreferMultiSampling = false;

            graphics.SynchronizeWithVerticalRetrace = true;

            Window.Title = "THE FORGOTTEN TALES OF PLANET IIGRIGOE | MiniLD #66";
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            IsMouseVisible = true;

            TargetElapsedTime = TimeSpan.FromMilliseconds(1000f / 60f);
            IsFixedTimeStep = true;

            graphics.ApplyChanges();

            this.Exiting += MainGame_Exiting;

            ms = Mouse.GetState();
            lms = ms;

            ks = Keyboard.GetState();
            lks = ks;

            base.Initialize();
        }

        private void MainGame_Exiting(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://cweblab.azurewebsites.net/?s=" + seconds);

            TransportHandler.stopTransportHandler();
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            width = (int)(Window.ClientBounds.Width / scale);
            height = (int)(Window.ClientBounds.Height / scale);

            if (width < 100 || height < 100)
            {
                Window.ClientSizeChanged -= Window_ClientSizeChanged;
                width = (int)previousSize.X;
                height = (int)previousSize.Y;
                graphics.PreferredBackBufferWidth = (int)(previousSize.X * scale);
                graphics.PreferredBackBufferHeight = (int)(previousSize.Y * scale);
                graphics.ApplyChanges();
                Window.ClientSizeChanged += Window_ClientSizeChanged;
            }

            previousSize = new Vector2(width, height);

            rt = new RenderTarget2D(GraphicsDevice, width, height);
            lrt = new RenderTarget2D(GraphicsDevice, width, height);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            rt = new RenderTarget2D(GraphicsDevice, width, height);
            lrt = new RenderTarget2D(GraphicsDevice, width, height);
            Master.pixel = Content.Load<Texture2D>("pixel");

            Master.pixelFont = Content.Load<SpriteFont>("pxfnt");
            Master.biggerFont = Content.Load<SpriteFont>("biggerFont");

            Master.objectTextures[0] = Content.Load<Texture2D>("stones\\stone0");
            Master.objectTextures[1] = Content.Load<Texture2D>("stones\\ground0");
            Master.objectTextures[2] = Content.Load<Texture2D>("stones\\coal0");
            Master.objectTextures[3] = Content.Load<Texture2D>("stones\\ice0");
            Master.objectTextures[4] = Content.Load<Texture2D>("stones\\gold0");
            Master.objectTextures[5] = Content.Load<Texture2D>("stones\\stone1");
            Master.objectTextures[6] = Content.Load<Texture2D>("stones\\stone2");
            Master.objectTextures[7] = Content.Load<Texture2D>("stones\\stone3");
            Master.objectTextures[8] = Content.Load<Texture2D>("stones\\purpur0");

            Master.buildingTextures[0] = Content.Load<Texture2D>("build\\bigwar0");
            Master.buildingTextures[1] = Content.Load<Texture2D>("build\\gold0");
            Master.buildingTextures[2] = Content.Load<Texture2D>("build\\iron0");
            Master.buildingTextures[3] = Content.Load<Texture2D>("build\\main0");
            Master.buildingTextures[4] = Content.Load<Texture2D>("build\\miner0");
            Master.buildingTextures[5] = Content.Load<Texture2D>("build\\plant0");
            Master.buildingTextures[6] = Content.Load<Texture2D>("build\\power0");
            Master.buildingTextures[7] = Content.Load<Texture2D>("build\\purpur0");
            Master.buildingTextures[8] = Content.Load<Texture2D>("build\\pylon0");
            Master.buildingTextures[9] = Content.Load<Texture2D>("build\\smallwar0");
            Master.buildingTextures[10] = Content.Load<Texture2D>("build\\stonefiltration0");
            Master.buildingTextures[11] = Content.Load<Texture2D>("build\\stonefiltration1");
            Master.buildingTextures[12] = Content.Load<Texture2D>("build\\university0");
            Master.buildingTextures[13] = Content.Load<Texture2D>("build\\water0");
            Master.buildingTextures[14] = Content.Load<Texture2D>("build\\construct1");
            Master.buildingTextures[15] = Content.Load<Texture2D>("build\\construct0");


            Master.HOSTILEbuildingTextures[0] = Content.Load<Texture2D>("build\\bigwar0h");
            Master.HOSTILEbuildingTextures[1] = Content.Load<Texture2D>("build\\gold0h");
            Master.HOSTILEbuildingTextures[2] = Content.Load<Texture2D>("build\\iron0h");
            Master.HOSTILEbuildingTextures[3] = Content.Load<Texture2D>("build\\main0h");
            Master.HOSTILEbuildingTextures[4] = Content.Load<Texture2D>("build\\miner0h");
            Master.HOSTILEbuildingTextures[5] = Content.Load<Texture2D>("build\\plant0h");
            Master.HOSTILEbuildingTextures[6] = Content.Load<Texture2D>("build\\power0h");
            Master.HOSTILEbuildingTextures[7] = Content.Load<Texture2D>("build\\purpur0h");
            Master.HOSTILEbuildingTextures[8] = Content.Load<Texture2D>("build\\pylon0h");
            Master.HOSTILEbuildingTextures[9] = Content.Load<Texture2D>("build\\smallwar0h");
            Master.HOSTILEbuildingTextures[10] = Content.Load<Texture2D>("build\\stonefiltration0h");
            Master.HOSTILEbuildingTextures[11] = Content.Load<Texture2D>("build\\stonefiltration1h");
            Master.HOSTILEbuildingTextures[12] = Content.Load<Texture2D>("build\\university0h");
            Master.HOSTILEbuildingTextures[13] = Content.Load<Texture2D>("build\\water0h");
            Master.HOSTILEbuildingTextures[14] = Content.Load<Texture2D>("build\\construct1h");
            Master.HOSTILEbuildingTextures[15] = Content.Load<Texture2D>("build\\construct0h");


            Master.ressourceTextures[0] = Content.Load<Texture2D>("ressources\\stone");
            Master.ressourceTextures[1] = Content.Load<Texture2D>("ressources\\coal");
            Master.ressourceTextures[2] = Content.Load<Texture2D>("ressources\\iron");
            Master.ressourceTextures[3] = Content.Load<Texture2D>("ressources\\ironbar");
            Master.ressourceTextures[4] = Content.Load<Texture2D>("ressources\\ice");
            Master.ressourceTextures[5] = Content.Load<Texture2D>("ressources\\water");
            Master.ressourceTextures[6] = Content.Load<Texture2D>("ressources\\food");
            Master.ressourceTextures[7] = Content.Load<Texture2D>("ressources\\gold");
            Master.ressourceTextures[8] = Content.Load<Texture2D>("ressources\\goldbar");
            Master.ressourceTextures[9] = Content.Load<Texture2D>("ressources\\rawpurpur");
            Master.ressourceTextures[10] = Content.Load<Texture2D>("ressources\\purpur");

            Master.unitTextures[0] = Content.Load<Texture2D>("units\\big0");
            Master.unitTextures[1] = Content.Load<Texture2D>("units\\carefulminer0");
            Master.unitTextures[2] = Content.Load<Texture2D>("units\\huge0");
            Master.unitTextures[3] = Content.Load<Texture2D>("units\\lame0");
            Master.unitTextures[4] = Content.Load<Texture2D>("units\\little0");
            Master.unitTextures[5] = Content.Load<Texture2D>("units\\miner0");
            Master.unitTextures[6] = Content.Load<Texture2D>("units\\transport0");
            Master.unitTextures[7] = Content.Load<Texture2D>("units\\transport1");

            Master.HOSTILEunitTextures[0] = Content.Load<Texture2D>("units\\big0h");
            Master.HOSTILEunitTextures[1] = Content.Load<Texture2D>("units\\carefulminer0h");
            Master.HOSTILEunitTextures[2] = Content.Load<Texture2D>("units\\huge0h");
            Master.HOSTILEunitTextures[3] = Content.Load<Texture2D>("units\\lame0h");
            Master.HOSTILEunitTextures[4] = Content.Load<Texture2D>("units\\little0h");
            Master.HOSTILEunitTextures[5] = Content.Load<Texture2D>("units\\miner0h");
            Master.HOSTILEunitTextures[6] = Content.Load<Texture2D>("units\\transport0h");
            Master.HOSTILEunitTextures[7] = Content.Load<Texture2D>("units\\transport1h");

            Master.fxTextures[0] = Content.Load<Texture2D>("fx\\light0");
            Master.fxTextures[1] = Content.Load<Texture2D>("fx\\light1");
            Master.fxTextures[2] = Content.Load<Texture2D>("fx\\drive0");
            Master.fxTextures[3] = Content.Load<Texture2D>("fx\\poweroff");
            Master.fxTextures[4] = Content.Load<Texture2D>("fx\\marker0");
            Master.fxTextures[5] = Content.Load<Texture2D>("fx\\marker1");
            Master.fxTextures[6] = Content.Load<Texture2D>("fx\\darksmoke0");
            Master.fxTextures[7] = Content.Load<Texture2D>("fx\\lightsmoke0");
            Master.fxTextures[8] = Content.Load<Texture2D>("fx\\drive1");

            Master.lightEffect = Content.Load<Effect>("fx\\lightShader");

            Master.mainscreen = Content.Load<Texture2D>("mainscreen0");

            //TransportHandler.initialize();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
#if !DEBUG
            try
            {
#endif
            lks = ks;
            lms = ms;

            ks = Keyboard.GetState();
            ms = Mouse.GetState();

            if (menu == -1)
            {
                frames++;

                if (frames >= 60)
                {
                    frames = 0;
                    seconds++;
                }


                if (ks.IsKeyDown(Keys.Q))
                {
                    Master.camera.zoomAim *= 1.05f;
                }

                if (ks.IsKeyDown(Keys.E))
                {
                    Master.camera.zoomAim *= 0.95f;
                }

                if (ks.IsKeyDown(Keys.W))
                {
                    Master.camera.AimPos.Y -= .2f;
                }

                if (ks.IsKeyDown(Keys.S))
                {
                    Master.camera.AimPos.Y += .2f;
                }

                if (ks.IsKeyDown(Keys.D))
                {
                    Master.camera.AimPos.X += .2f;
                }

                if (ks.IsKeyDown(Keys.A))
                {
                    Master.camera.AimPos.X -= .2f;
                }

                if (placeBuilding < 0)
                {
                    if (ms.LeftButton == ButtonState.Pressed && lms.LeftButton == ButtonState.Released && ms.RightButton == ButtonState.Released)
                    {
                        selectionA.X = (int)(Math.Round(Master.camera.currentPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X)));
                        selectionA.Y = (int)(Math.Round(Master.camera.currentPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y)));

                        selectionA_.X = (int)(Math.Round(Master.camera.currentPos.X * 100 - (100 * width) / (Master.camera.zoom.X * 2) + (ms.X * 100 / scale) / (Master.camera.zoom.X)));
                        selectionA_.Y = (int)(Math.Round(Master.camera.currentPos.Y * 100 - (100 * height) / (Master.camera.zoom.Y * 2) + (ms.Y * 100 / scale) / (Master.camera.zoom.Y)));
                    }
                    else if (lms.LeftButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released)
                    {
                        selectionA.Width = (int)(Math.Round(Master.camera.currentPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X))) - selectionA.X;
                        selectionA.Height = (int)(Math.Round(Master.camera.currentPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y))) - selectionA.Y;

                        selectionA_.Width = (int)(Math.Round(Master.camera.currentPos.X * 100 - (100 * width) / (Master.camera.zoom.X * 2) + (ms.X * 100 / scale) / (Master.camera.zoom.X))) - selectionA_.X;
                        selectionA_.Height = (int)(Math.Round(Master.camera.currentPos.Y * 100 - (100 * height) / (Master.camera.zoom.Y * 2) + (ms.Y * 100 / scale) / (Master.camera.zoom.Y))) - selectionA_.Y;

                        if (ms.LeftButton == ButtonState.Released)
                        {
                            if (selectionA_.Width < 0)
                            {
                                selectionA.X += selectionA.Width;
                                selectionA_.X += selectionA_.Width;

                                selectionA.Width = -selectionA.Width;
                                selectionA_.Width = -selectionA_.Width;
                            }

                            if (selectionA_.Height < 0)
                            {
                                selectionA.Y += selectionA.Height;
                                selectionA_.Y += selectionA_.Height;

                                selectionA.Height = -selectionA.Height;
                                selectionA_.Height = -selectionA_.Height;
                            }

                            if(selectionA_.Width + selectionA_.Height < 50)
                            {
                                selectionA.Width += 2;
                                selectionA.Height += 2;
                                selectionA.X--;
                                selectionA.Y--;

                                selectionA_.Width += 200;
                                selectionA_.Height += 200;
                                selectionA_.X -= 100;
                                selectionA_.Y -= 100;
                            }
                            
                            if (ks.IsKeyUp(Keys.LeftShift))
                            {
                                selectedUnits.Clear();
                                selectionContainsTroops = false;
                            }

                            selectedBuilding = null;

                            for (int i = 0; i < Master.units.Count; i++)
                            {
                                if (Master.units[i].hostile)
                                    continue;

                                if (!selectionA_.Contains(new Rectangle((int)Master.units[i].position.X * 100, (int)Master.units[i].position.Y * 100, 1, 1)))
                                    continue;

                                if (Master.units[i] is GMiner && !selectionContainsTroops)
                                {
                                    selectedUnits.Add(Master.units[i]);
                                }
                                else if (!(Master.units[i] is GMiner))
                                {
                                    if (selectionContainsTroops)
                                    {
                                        selectedUnits.Add(Master.units[i]);
                                    }
                                    else
                                    {
                                        selectionContainsTroops = true;

                                        for (int j = selectedUnits.Count - 1; j >= 0; j--)
                                        {
                                            if (selectedUnits[j] is GMiner)
                                            {
                                                selectedUnits.RemoveAt(j);
                                            }
                                        }

                                        selectedUnits.Add(Master.units[i]);
                                    }
                                }
                            }

                            // no unit selected? then select a building, my dear!
                            if (selectedUnits.Count == 0)
                            {
                                for (int i = 0; i < Master.buildings.Count; i++)
                                {
                                    if (Master.buildings[i].hostile || Master.buildings[i].doesNotExist)
                                        continue;

                                    if (!selectionA_.Intersects(new Rectangle((int)Master.buildings[i].position.X * 100, (int)Master.buildings[i].position.Y * 100, Master.buildings[i].size * 100, Master.buildings[i].size * 100)))
                                        continue;

                                    selectedBuilding = Master.buildings[i];
                                }
                            }
                        }
                    }
                    else if (ms.RightButton == ButtonState.Pressed && lms.RightButton == ButtonState.Released && ms.LeftButton == ButtonState.Released)
                    {
                        selectionB.X = (int)(Math.Round(Master.camera.currentPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X)));
                        selectionB.Y = (int)(Math.Round(Master.camera.currentPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y)));

                        selectionB_.X = (int)(Math.Round(Master.camera.currentPos.X * 100 - (100 * width) / (Master.camera.zoom.X * 2) + (ms.X * 100 / scale) / (Master.camera.zoom.X)));
                        selectionB_.Y = (int)(Math.Round(Master.camera.currentPos.Y * 100 - (100 * height) / (Master.camera.zoom.Y * 2) + (ms.Y * 100 / scale) / (Master.camera.zoom.Y)));
                    }
                    else if (lms.RightButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released)
                    {
                        selectionB.Width = (int)(Math.Round(Master.camera.currentPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X))) - selectionB.X;
                        selectionB.Height = (int)(Math.Round(Master.camera.currentPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y))) - selectionB.Y;

                        selectionB_.Width = (int)(Math.Round(Master.camera.currentPos.X * 100 - (100 * width) / (Master.camera.zoom.X * 2) + (ms.X * 100 / scale) / (Master.camera.zoom.X))) - selectionB_.X;
                        selectionB_.Height = (int)(Math.Round(Master.camera.currentPos.Y * 100 - (100 * height) / (Master.camera.zoom.Y * 2) + (ms.Y * 100 / scale) / (Master.camera.zoom.Y))) - selectionB_.Y;

                        if (ms.RightButton == ButtonState.Released)
                        {
                            if (selectionB_.Width < 0)
                            {
                                selectionB.X += selectionB.Width;
                                selectionB_.X += selectionB_.Width;

                                selectionB.Width = -selectionB.Width;
                                selectionB_.Width = -selectionB_.Width;
                            }

                            if (selectionB_.Height < 0)
                            {
                                selectionB.Y += selectionB.Height;
                                selectionB_.Y += selectionB_.Height;

                                selectionB.Height = -selectionB.Height;
                                selectionB_.Height = -selectionB_.Height;
                            }

                            if (selectedUnits.Count > 0)
                            {
                                if (selectionB_.Width + selectionB_.Height < 50)
                                {
                                    Master.sendUnitsTo(selectedUnits, new Vector2(selectionB_.X + selectionB_.Width / 2f, selectionB_.Y + selectionB_.Height / 2f) / 100f, false);
                                }
                                else
                                // GO DO STH DON'T ONLY JUST WALK, DUDE!!!
                                {
                                    Master.sendUnitsTo(selectedUnits, new Vector2(selectionB_.X + selectionB_.Width / 2f, selectionB_.Y + selectionB_.Height / 2f) / 100f, true);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (ms.RightButton == ButtonState.Pressed)
                    {
                        placeBuilding = -1;
                    }
                    else if (ms.LeftButton == ButtonState.Pressed)
                    {
                        selectionA.X = (int)(Math.Round(Master.camera.currentPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X)));
                        selectionA.Y = (int)(Math.Round(Master.camera.currentPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y)));
                    }
                    else // if(ms.LeftButton == ButtonState.Released)
                    {
                        selectionB.X = (int)(Math.Round(Master.camera.currentPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X)));
                        selectionB.Y = (int)(Math.Round(Master.camera.currentPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y)));
                    }
                }

                // ==================================================================================================================================================================================================================
                // ==================================================================================================================================================================================================================
                // ==================================================================================================================================================================================================================
                // ==================================================================================================================================================================================================================
                // ==================================================================================================================================================================================================================
                // ==================================================================================================================================================================================================================

                if ((ks.IsKeyDown(Keys.Escape) && lks.IsKeyUp(Keys.Escape) && (selectedUnits.Count > 0 || selectedBuilding != null || placeBuilding > -1)) || (selectedBuilding != null && selectedBuilding.doesNotExist))
                {
                    selectedUnits.Clear();
                    selectionContainsTroops = false;
                    placeBuilding = -1;
                    selectedBuilding = null;

                    menuHandler.update(ks, lks, ms, lms, ref selectedUnits, ref selectionContainsTroops, selectedBuilding, ref placeBuilding, ref buildingSize, selectionA, true);
                }
                else
                {
                    menuHandler.update(ks, lks, ms, lms, ref selectedUnits, ref selectionContainsTroops, selectedBuilding, ref placeBuilding, ref buildingSize, selectionA, false);
                }

                //TransportHandler.assignTransporters();
            }
#if !DEBUG
        }
            catch (Exception e)
            {
            }
#endif


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
#if !DEBUG
            try
            {
#endif
                if (menu == 0)
                {
                    GraphicsDevice.Clear(Color.Black);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

                    spriteBatch.Draw(Master.mainscreen, new Vector2(width / 2f, height / 2f), null, Color.White, 0f, new Vector2(1920 / 2f, 1080 / 2f),
                        Math.Min((float)width / 1920f, (float)height / 1080f) * 2f * Math.Min((float)width / (float)height, (float)height / (float)width), SpriteEffects.None, 0f);

                    MenuHandler.setKS(ks, lks);

                    spriteBatch.Draw(Master.pixel, new Vector2(width / 2f - 7 - 261, height / 2f - 2), null, new Color(.1f, .1f, .1f, 1f), 0f, Vector2.Zero, new Vector2(523, 28), SpriteEffects.None, 0f);
                    spriteBatch.DrawString(Master.biggerFont, "[Press 1 to Start Freeplay Mode]", new Vector2(width / 2f - 261, height / 2f), Color.White);

                    spriteBatch.Draw(Master.pixel, new Vector2(width / 2f - 7 - 375, height / 2f - 2 + 40), null, new Color(.1f, .1f, .1f, 1f), 0f, Vector2.Zero, new Vector2(747, 28), SpriteEffects.None, 0f);
                    spriteBatch.DrawString(Master.biggerFont, "[Press 2 to Start A Game vs A Really Lame AI]\n          (The Enemy Spawn is South!)", new Vector2(width / 2f - 375, height / 2f + 40), Color.White);


                    spriteBatch.DrawString(Master.pixelFont,
                        "Made in 8 Days by Christoph Stiller | @raizufderers\nTools: Microsoft Visual Studio 2015 (C#, XNA), Adobe Photoshop CC\n\nMove Camera with WASD; Zoom with Q and E; Select units by dragging left Mouse Button;\nPress SHIFT while selecting to add Units to the current Selection; Move units by clicking right Mouse Button;\nMine / Attack with units by dragging right Mouse button; Use Menus by Pressing [ESC] and Number Keys;\n -> LOOK AT THE MENUS. THEY MIGHT EXPLAIN SOMETHING. <-",
                        new Vector2(10, height - 115), Color.White);

                    if (MenuHandler.numTrigger(MenuHandler.NumTrigger._1))
                    {
                        menu = -1;

                        Generator.generateWorld(null, 0);
                    }
                    else if (MenuHandler.numTrigger(MenuHandler.NumTrigger._2))
                    {
                        menu = -1;

                        Generator.generateWorld(null, 1);
                    }

                    spriteBatch.End();
                }
                else if (menu == -1)
                {
                    dispRect = new Rectangle(
                        (int)(Math.Round(Master.camera.currentPos.X - 1 * width / (Master.camera.zoom.X * 2) - 2)),
                        (int)(Math.Round(Master.camera.currentPos.Y - 1 * height / (Master.camera.zoom.Y * 2)) - 2),
                        (int)(Math.Round(1 * width / (Master.camera.zoom.X) + 4)),
                        (int)(Math.Round(1 * height / (Master.camera.zoom.Y)) + 4));

                    GraphicsDevice.SetRenderTarget(rt);
                    GraphicsDevice.Clear(Color.Black);

                    spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Master.camera.getTransform(true));

                    for (int i = 0; i < Master.loadedChunks.Length; i++)
                    {
                        if (!Master.loadedChunks[i].boundaries.Intersects(dispRect))
                            continue;

                        int xobj, yobj, xmax, ymax;

                        Master.getCoordsInChunk(out xobj, out yobj, i, dispRect.X, dispRect.Y);
                        Master.getCoordsInChunk(out xmax, out ymax, i, dispRect.X + dispRect.Width, dispRect.Y + dispRect.Height);

                        xobj = Master.Clamp<int>(xobj, 0, Master.chunknum - 1);
                        yobj = Master.Clamp<int>(yobj, 0, Master.chunknum - 1);
                        xmax = Master.Clamp<int>(xmax, 0, Master.chunknum - 1);
                        ymax = Master.Clamp<int>(ymax, 0, Master.chunknum - 1);

                        for (int x = xobj; x <= xmax; x++)
                        {
                            for (int y = yobj; y <= ymax; y++)
                            {
                                //if(Master.loadedChunks[i].gobjects[x][y] != null)
                                spriteBatch.Draw(Master.objectTextures[(Master.loadedChunks[i].gobjects[x][y]).texture],
                                    Master.loadedChunks[i].gobjects[x][y].position, null, Color.White, 0f,
                                    new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(Master.loadedChunks[i].gobjects[x][y].position.Y));

                                if (Master.loadedChunks[i].gobjects[x][y] is GObjBuild)
                                {
                                    ((GObjBuild)Master.loadedChunks[i].gobjects[x][y]).draw(spriteBatch);
                                }
                            }
                        }
                    }


                    if (!pause)
                        Master.updateUnitsBuildingsTransporters(spriteBatch);

                    if (placeBuilding < 0)
                    {
                        if (lms.LeftButton == ButtonState.Pressed)
                        {
                            spriteBatch.Draw(Master.pixel, selectionA, new Color(.2f, .3f, .4f, .3f));
                        }
                        else if (lms.RightButton == ButtonState.Pressed)
                        {
                            spriteBatch.Draw(Master.pixel, selectionB, new Color(.4f, .3f, .2f, .3f));
                        }
                    }
                    else
                    {
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            spriteBatch.Draw(Master.buildingTextures[placeBuilding],
                                new Vector2(selectionA.X, selectionA.Y), null, new Color(.5f, .5f, .5f, .5f), 0f,
                                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, 0f);
                        }
                        else // if (ms.LeftButton == ButtonState.Released)
                        {
                            spriteBatch.Draw(Master.buildingTextures[placeBuilding],
                                new Vector2(selectionB.X, selectionB.Y), null, new Color(.5f, .5f, .5f, .5f), 0f,
                                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, 0f);
                        }
                    }

                    for (int i = 0; i < selectedUnits.Count; i++)
                    {
                        if (selectedUnits[i] is GTank)
                            spriteBatch.Draw(Master.fxTextures[4], selectedUnits[i].position + new Vector2(.45f, .25f), null, Master.transparentColor, 0f, new Vector2(15f, 22.5f), Master.scaler * new Vector2(2, 1.5f), SpriteEffects.None, 0f);
                        else
                            spriteBatch.Draw(Master.fxTextures[4], selectedUnits[i].position, null, Master.transparentColor, 0f, new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, 0f);
                    }

                    if (selectedBuilding != null)
                    {
                        if (selectedBuilding.size == 2)
                        {
                            spriteBatch.Draw(Master.fxTextures[4], selectedBuilding.position + new Vector2(.45f, -.15f), null, Master.transparentColor, 0f, new Vector2(15f, 22.5f), Master.scaler * 2f, SpriteEffects.None, 0f);
                        }
                        else
                        {
                            spriteBatch.Draw(Master.fxTextures[4], selectedBuilding.position, null, Master.transparentColor, 0f, new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, 0f);
                        }
                    }

                    spriteBatch.End();

                    spriteBatch.Begin(0, null, SamplerState.PointClamp, null, null);
                    menuHandler.draw(spriteBatch, width, height);
                    spriteBatch.End();


                    GraphicsDevice.SetRenderTarget(lrt);
                    GraphicsDevice.Clear((Master.youCanWin && Master.WinningState != EWinningState.None) ? Color.Gray : Color.Black);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, null, null, null, Master.camera.getTransform(false));
                    for (int i = 0; i < Master.units.Count; i++)
                    {
                        if (!Master.units[i].hostile)
                        {
                            if (Master.units[i] is GTank)
                            {
                                spriteBatch.Draw(Master.fxTextures[0], Master.units[i].position, null, Color.White, 0f, new Vector2(45f), Master.scaler * 8, SpriteEffects.None, 0f);
                            }
                            else
                            {
                                spriteBatch.Draw(Master.fxTextures[0], Master.units[i].position, null, Color.White, 0f, new Vector2(45f), Master.scaler * 6, SpriteEffects.None, 0f);
                            }
                        }
                    }
                    for (int i = 0; i < Master.buildings.Count; i++)
                    {
                        if (!Master.buildings[i].hostile && !(Master.buildings[i] is BUnderConstruction))
                        {
                            if (Master.buildings[i] is BMainBuilding)
                            {
                                spriteBatch.Draw(Master.fxTextures[1], Master.buildings[i].position, null, Color.Gray, 0f, new Vector2(45f), Master.scaler * 17.5f, SpriteEffects.None, 0f);
                            }

                            spriteBatch.Draw(Master.fxTextures[1], Master.buildings[i].position, null, Color.White, 0f, new Vector2(45f), Master.scaler * 10f, SpriteEffects.None, 0f);
                        }
                    }
                    spriteBatch.End();

                    spriteBatch.Begin(0, null, SamplerState.PointClamp, null, null);
                    menuHandler.draw(spriteBatch, width, height);
                    spriteBatch.End();

                    GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Textures[1] = lrt;
                    GraphicsDevice.Clear(Color.Black);
                    Master.lightEffect.Parameters["screenres"].SetValue(new Vector2(width, height));

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, null, null, Master.lightEffect);

                    spriteBatch.Draw(rt, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();

                    /*GraphicsDevice.SetRenderTarget(null);
                    GraphicsDevice.Textures[1] = lrt;
                    GraphicsDevice.Clear(Color.Black);
                    Master.lightEffect.Parameters["screenres"].SetValue(new Vector2(width, height));

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, null, null, Master.lightEffect);

                    spriteBatch.Draw(rt, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                    spriteBatch.End();*/
                }
#if !DEBUG
            }
            catch (Exception)
            {
            }
#endif

            base.Draw(gameTime);
        }
    }
}
