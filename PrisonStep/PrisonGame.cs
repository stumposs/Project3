using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using PrisonStep;

namespace PrisonStep
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PrisonGame : Microsoft.Xna.Framework.Game
    {

        #region Fields

        /// <summary>
        /// This graphics device we are drawing on in this assignment
        /// </summary>
        GraphicsDeviceManager graphics;

        /// <summary>
        /// The camera we use
        /// </summary>
        private Camera camera;

        private Camera camera2;

        /// <summary>
        /// Splash screen states
        /// </summary>
        public enum GameState { splash, game, results };
        GameState current = GameState.splash;

        /// <summary>
        /// Stores the last keyboard state for the game.
        /// </summary>
        KeyboardState lastKeyboardState;

        private Ground ground;
        public Ground Ground { get { return ground; } }

        /// <summary>
        /// The player in your game is modeled with this class
        /// </summary>
        private Player player;
        public Player Player { get { return player; } }

        private Player player2;
        public Player Player2 { get { return player; } }

        private PSLineDraw lineDraw;

        #endregion

        #region Properties

        /// <summary>
        /// The game camera
        /// </summary>
        public Camera Camera { get { return camera; } }

        public Camera Camera2 { get { return camera2; } }

        public PSLineDraw LineDraw { get { return lineDraw; } }

        #endregion

        private bool slimed = false;
        public bool Slimed { get { return slimed; } set { slimed = value; } }

        private float slimeLevel = 1.0f;
        public float SlimeLevel { get { return slimeLevel; } }

        /// <summary>
        /// The bazooka victoria carries
        /// </summary>
        private AnimatedModel bazooka;
        public AnimatedModel Bazooka { get { return bazooka; } }

        /// <summary>
        /// the transformation matrix for the bazooka
        /// </summary>
        private Matrix bazTransform;
        public Matrix BazTransform { get { return bazTransform; } set { bazTransform = value; } }

        /// <summary>
        /// Tells us how many pies have been fired total before we must return to the control room
        /// </summary>
        private int totalPiesFired = 0;
        public int TotalPiesFired { get { return totalPiesFired; } set { totalPiesFired = value; } }

        /// <summary>
        /// the player's score
        /// </summary>
        private int score = 0;
        public int Score {get {return score; } set { score = value; } }

        /// <summary>
        /// Tells us if we need to return to the control room for more pies
        /// </summary>
        private bool needPies = false;
        public bool NeedPies { get { return needPies; } set { needPies = value; } }

        /// <summary>
        /// random number generator
        /// </summary>
        private Random randNum = new Random();
        public Random RandNum { get { return randNum; } }

        /// <summary>
        /// Score and UI fonts
        /// </summary>
        private SpriteFont UIFont;
        SpriteBatch spriteBatch;

        /// <summary>
        /// Particle system business. Game components will use the particle system by accessing the game's copy of the particle effects.
        /// </summary>
        private SmokeParticleSystem3d smokePlume = null;
        public SmokeParticleSystem3d SmokePlume { get { return smokePlume; } }


        /// <summary>
        /// Constructor
        /// </summary>
        public PrisonGame()
        {
            // XNA startup
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Camera settings

            camera = new Camera(graphics);
            camera.Eye = new Vector3(800, 180, 1053);
            camera.Center = new Vector3(275, 90, 1053);
            camera.FieldOfView = MathHelper.ToRadians(42);

            camera2 = new Camera(graphics);
            camera.Eye = new Vector3(800, 180, 1053);
            camera.Center = new Vector3(275, 90, 1053);
            camera.FieldOfView = MathHelper.ToRadians(42);

            ground = new Ground(this);

            // Create a player object
            player = new Player(this, camera);
            player2 = new Player(this, camera2);

            //Particle system
            smokePlume = new SmokeParticleSystem3d(9);

            // Some basic setup for the display window
            this.IsMouseVisible = true;
			//this.Window.AllowUserResizing = true;
			this.graphics.PreferredBackBufferWidth = 1024;
			this.graphics.PreferredBackBufferHeight = 768;

            lineDraw = new PSLineDraw(this, Camera);
            this.Components.Add(lineDraw);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            camera.Initialize();
            camera2.Initialize();
            player.Initialize();
            ground.Initialize();

            //This section is lifted from the learning XNA 4.0 book. Partition the screen into two disjoint halves.
            Viewport vp1 = GraphicsDevice.Viewport;
            Viewport vp2 = GraphicsDevice.Viewport;
            vp1.Height = (GraphicsDevice.Viewport.Height / 2) - 1;

            vp2.Y = vp1.Height;
            vp2.Height = vp1.Height;

            camera.Viewport = vp1;
            camera2.Viewport = vp2;

            base.Initialize();

            
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //bazooka = Content.Load<Model>("PieBazooka");
            bazooka = new AnimatedModel(this, "PieBazooka");
            bazooka.LoadContent(Content);
            player.LoadContent(Content);
            player2.LoadContent(Content);
            ground.LoadContent(Content);

            smokePlume.LoadContent(Content);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            UIFont = Content.Load<SpriteFont>("UIFont");

            //bazooka.ObjectEffect = Content.Load<Effect>("PhibesEffect1");
            //bazooka.SetEffect();
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            base.Update(gameTime);

            KeyboardState keyboardState = Keyboard.GetState();

            //
            // Update game components
            //
            if (current == GameState.splash)
            {
                if (keyboardState.IsKeyDown(Keys.Tab) && lastKeyboardState.IsKeyUp(Keys.Tab))
                    current = GameState.game;

            }
            else if (current == GameState.game)
            {
                if (keyboardState.IsKeyDown(Keys.Tab) && lastKeyboardState.IsKeyUp(Keys.Tab))
                    current = GameState.results;

                lineDraw.Clear();

                player.Update(gameTime);

                ground.Update(gameTime);


                camera.Update(gameTime);
                camera2.Update(gameTime);

                //particle systems
                smokePlume.Update(gameTime.ElapsedGameTime.TotalSeconds);

                // Amount to change slimeLevel in one second
                float slimeRate = 2.5f;

                if (slimed && slimeLevel >= -1.5)
                {
                    slimeLevel -= (float)gameTime.ElapsedGameTime.TotalSeconds * slimeRate;
                }
                else if (!slimed && slimeLevel < 1)
                {
                    slimeLevel += (float)gameTime.ElapsedGameTime.TotalSeconds * slimeRate;
                }
            }
            else if (current == GameState.results)
            {
                if (keyboardState.IsKeyDown(Keys.Tab) && lastKeyboardState.IsKeyUp(Keys.Tab))
                    current = GameState.splash;
            }

            lastKeyboardState = keyboardState;

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            if (current == GameState.splash)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(UIFont, "If the aliens win, everyone will die.", new Vector2(10, 10), Color.White);
                spriteBatch.End();
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
            else if (current == GameState.game)
            {
                graphics.GraphicsDevice.Clear(Color.Black);
                GraphicsDevice.Viewport = camera.Viewport;
                DrawGame(gameTime, camera);
                GraphicsDevice.Viewport = camera2.Viewport;
                DrawGame(gameTime, camera2);
                
            }
            else if (current == GameState.results)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(UIFont, "You are dead and all your friends are dead.", new Vector2(10, 10), Color.White);
                spriteBatch.End();
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
        }

        public void DrawGame(GameTime gameTime, Camera inCamera)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            ground.Draw(graphics, gameTime, inCamera);

            player.Draw(graphics, gameTime, inCamera);
            player2.Draw(graphics, gameTime, inCamera);

            smokePlume.Draw(GraphicsDevice, inCamera);

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.BlendState = BlendState.Opaque;

            base.Draw(gameTime);

            //Show score and pies in bazooka
            spriteBatch.Begin();
            spriteBatch.DrawString(UIFont, "Pies: " + (10 - totalPiesFired).ToString(), new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(UIFont, "Score: " + score.ToString(), new Vector2(10, 25), Color.White);
            spriteBatch.End();
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        }

        public void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }
    }
}
