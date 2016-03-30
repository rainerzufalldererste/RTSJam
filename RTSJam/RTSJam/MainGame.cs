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

        RenderTarget2D rt;
        Rectangle dispRect;

        public static int width = 640*2, height = 400*2;
        float scale = 1f;

        Rectangle selectionA, selectionA_, selectionB, selectionB_;

        List<GUnit> selectedUnits = new List<GUnit>();
        bool selectionContainsTroops = false;

        MenuHandler menuHandler = new MenuHandler();

        MouseState ms, lms;
        KeyboardState ks, lks;
        Vector2 previousSize;

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

            Window.Title = "RTS Game on Mars | MiniLD #66 | #integerMasterRace";
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
            TransportHandler.stopTransportHandler();
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            width = (int)(Window.ClientBounds.Width / scale);
            height = (int)(Window.ClientBounds.Height / scale);

            if(width < 100 || height < 100)
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
            Master.pixel = Content.Load<Texture2D>("pixel");

            Master.pixelFont = Content.Load<SpriteFont>("pxfnt");

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

            TransportHandler.initialize();

            Generator.generateWorld(null, 0);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            lks = ks;
            lms = ms;

            ks = Keyboard.GetState();
            ms = Mouse.GetState();

            if(ks.IsKeyDown(Keys.Q))
            {
                Master.camera.zoomAim *= 1.05f;
            }

            if (ks.IsKeyDown(Keys.E))
            {
                Master.camera.zoomAim *= 0.95f;
            }

            if(ks.IsKeyDown(Keys.W))
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

            if (ms.LeftButton == ButtonState.Pressed && lms.LeftButton == ButtonState.Released && ms.RightButton == ButtonState.Released)
            {
                selectionA.X = (int)(Math.Round(Master.camera.AimPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X)));
                selectionA.Y = (int)(Math.Round(Master.camera.AimPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y)));

                selectionA_.X = (int)(Math.Round(Master.camera.AimPos.X * 100 - (100 * width) / (Master.camera.zoom.X * 2) + (ms.X * 100 / scale) / (Master.camera.zoom.X)));
                selectionA_.Y = (int)(Math.Round(Master.camera.AimPos.Y * 100 - (100 * height) / (Master.camera.zoom.Y * 2) + (ms.Y * 100 / scale) / (Master.camera.zoom.Y)));
            }
            else if (ms.LeftButton == ButtonState.Released && lms.LeftButton == ButtonState.Pressed && ms.RightButton == ButtonState.Released)
            {
                selectionA.Width = selectionA.X - (int)(Math.Round(Master.camera.AimPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X)));
                selectionA.Height = selectionA.Y - (int)(Math.Round(Master.camera.AimPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y)));

                selectionA_.Width = selectionA_.X - (int)(Math.Round(Master.camera.AimPos.X * 100 - (100 * width) / (Master.camera.zoom.X * 2) + (ms.X * 100 / scale) / (Master.camera.zoom.X)));
                selectionA_.Height = selectionA_.Y - (int)(Math.Round(Master.camera.AimPos.Y * 100 - (100 * height) / (Master.camera.zoom.Y * 2) + (ms.Y * 100 / scale) / (Master.camera.zoom.Y)));

                // TUT: Press shift to multiselect
                if(ks.IsKeyUp(Keys.LeftShift))
                {
                    selectedUnits.Clear();
                    selectionContainsTroops = false;
                }

                for (int i = 0; i < Master.units.Count; i++)
                {
                    if(Master.units[i].hostile)
                        continue;

                    if (!selectionA_.Contains(new Rectangle((int)Master.units[i].position.X * 100, (int)Master.units[i].position.Y * 100, 1, 1)))
                        continue;

                    if(Master.units[i] is GMiner && !selectionContainsTroops)
                    {
                        selectedUnits.Add(Master.units[i]);
                    }
                    else if(!(Master.units[i] is GMiner))
                    {
                        if(selectionContainsTroops)
                        {
                            selectedUnits.Add(Master.units[i]);
                        }
                        else
                        {
                            selectionContainsTroops = true;

                            for (int j = selectedUnits.Count - 1; j >= 0 ; j--)
                            {
                                if(selectedUnits[i] is GMiner)
                                {
                                    selectedUnits.RemoveAt(i);
                                }
                            }

                            selectedUnits.Add(Master.units[i]);
                        }
                    }
                }
            }
            else if (ms.RightButton == ButtonState.Pressed && lms.RightButton == ButtonState.Released && ms.LeftButton == ButtonState.Released)
            {
                selectionB.X = (int)(Math.Round(Master.camera.AimPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X)));
                selectionB.Y = (int)(Math.Round(Master.camera.AimPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y)));

                selectionB_.X = (int)(Math.Round(Master.camera.AimPos.X * 100 - (100 * width) / (Master.camera.zoom.X * 2) + (ms.X * 100 / scale) / (Master.camera.zoom.X)));
                selectionB_.Y = (int)(Math.Round(Master.camera.AimPos.Y * 100 - (100 * height) / (Master.camera.zoom.Y * 2) + (ms.Y * 100 / scale) / (Master.camera.zoom.Y)));
            }
            else if (ms.RightButton == ButtonState.Released && lms.RightButton == ButtonState.Pressed && ms.LeftButton == ButtonState.Released)
            {
                selectionB.Width = selectionB.X - (int)(Math.Round(Master.camera.AimPos.X - (1 * width) / (Master.camera.zoom.X * 2) + (ms.X / scale) / (Master.camera.zoom.X)));
                selectionB.Height = selectionB.Y - (int)(Math.Round(Master.camera.AimPos.Y - (1 * height) / (Master.camera.zoom.Y * 2) + (ms.Y / scale) / (Master.camera.zoom.Y)));

                selectionB_.Width = selectionB_.X - (int)(Math.Round(Master.camera.AimPos.X * 100 - (100 * width) / (Master.camera.zoom.X * 2) + (ms.X * 100 / scale) / (Master.camera.zoom.X)));
                selectionB_.Height = selectionB_.Y - (int)(Math.Round(Master.camera.AimPos.Y * 100 - (100 * height) / (Master.camera.zoom.Y * 2) + (ms.Y * 100 / scale) / (Master.camera.zoom.Y)));

                if(selectionA_.Width + selectionA_.Height < 50)
                {
                    Master.sendUnitsTo(selectedUnits, new Vector2(selectionB_.X - selectionA_.X, selectionB_.Y - selectionA_.Y) / 100f, false);
                }
                else
                // GO DO STH DON'T ONLY JUST WALK, DUDE!!!
                {
                    Master.sendUnitsTo(selectedUnits, new Vector2(selectionB_.X - selectionA_.X, selectionB_.Y - selectionA_.Y) / 100f, true);
                }
            }

            menuHandler.update(ks, lks, ms, lms, ref selectedUnits, ref selectionContainsTroops);

            TransportHandler.assignTransporters();


            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
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

                for (int x = 0; x < Master.chunknum; x++)
                {
                    for (int y = 0; y < Master.chunknum; y++)
                    {
                        //if(Master.loadedChunks[i].gobjects[x][y] != null)
                        spriteBatch.Draw(Master.objectTextures[(Master.loadedChunks[i].gobjects[x][y]).texture],
                            Master.loadedChunks[i].gobjects[x][y].position, null, Color.White, 0f,
                            new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(Master.loadedChunks[i].gobjects[x][y].position.Y));

                        if(Master.loadedChunks[i].gobjects[x][y] is GObjBuild)
                        {
                            ((GObjBuild)Master.loadedChunks[i].gobjects[x][y]).draw(spriteBatch);
                        }
                    }
                }
            }

            Master.updateUnitsBuildingsTransporters(spriteBatch);

            //spriteBatch.Draw(Master.pixel, new Vector2(selectionA_.X, selectionA_.Y)/100f, null, Color.White, 0f, new Vector2(.5f), 1f, SpriteEffects.None, 0f);

            spriteBatch.End();

            spriteBatch.Begin(0, null, SamplerState.PointClamp, null, null);
            spriteBatch.DrawString(Master.pixelFont, "Hello World ( " + selectionA.X + " | " + selectionA.Y + " )", Vector2.Zero, Color.White);
            menuHandler.draw(spriteBatch, width, height);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, null, null);

            spriteBatch.Draw(rt, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
