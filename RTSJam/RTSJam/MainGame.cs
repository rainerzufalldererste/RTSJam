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

        public static int width = 640, height = 400;
        float scale = 2.5f;

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

            base.Initialize();
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            width = (int)(Window.ClientBounds.Width / scale);
            height = (int)(Window.ClientBounds.Height / scale);
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

            Master.loadedChunks = Generator.generateWorld(null);
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            

            if(Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                Master.camera.zoomAim *= 1.05f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                Master.camera.zoomAim *= 0.95f;
            }

            if(Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Master.camera.position.Y -= .25f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Master.camera.position.Y += .25f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Master.camera.position.X += .25f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Master.camera.position.X -= .25f;
            }

            TransportHandler.assignTransporters();

            base.Update(gameTime);
        }

        Rectangle dispRect;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            dispRect = new Rectangle(
                (int)(Math.Round(Master.camera.position.X - 1 * width / (Master.camera.zoom.X * 2) - 2)),
                (int)(Math.Round(Master.camera.position.Y - 1 * height / (Master.camera.zoom.Y * 2)) - 2),
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
                            spriteBatch.Draw(Master.objectTextures[((Master.loadedChunks[i].gobjects[x][y])).texture],
                                Master.loadedChunks[i].gobjects[x][y].position, null, Color.LightGoldenrodYellow, 0f,
                                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(Master.loadedChunks[i].gobjects[x][y].position.Y));
                    }
                }
            }


            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(Master.pixelFont, "Hello World", Vector2.Zero, Color.White);
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
