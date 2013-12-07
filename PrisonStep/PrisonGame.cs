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

        /// <summary>
        /// Splash screen states
        /// </summary>
        public enum GameState { splash, game, results };
        GameState current = GameState.splash;

        /// <summary>
        /// Stores the last keyboard state for the game.
        /// </summary>
        KeyboardState lastKeyboardState;

        /// <summary>
        /// The player in your game is modeled with this class
        /// </summary>
        private Player player;
        public Player Player { get { return player; } }

        /// <summary>
        /// The Dalek NPC
        /// </summary>
        private Dalek dalek;
        public Dalek Dalek { get { return dalek; } }

        /// <summary>
        /// The Alien NPC
        /// </summary>
        private Alien alien;
        public Alien Alien { get { return alien; } }

        /// <summary>
        /// This is the actual model we are using for the prison
        /// </summary>
        private List<PrisonModel> phibesModels = new List<PrisonModel>();

        private PSLineDraw lineDraw;

        #endregion

        #region Properties

        /// <summary>
        /// The game camera
        /// </summary>
        public Camera Camera { get { return camera; } }

        public PSLineDraw LineDraw { get { return lineDraw; } }

        public List<PrisonModel> PhibesModels
        {
            get { return phibesModels; }
        }
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
        /// The pies in the game.
        /// </summary>
        private Pies pies;
        public Pies Pies { get { return pies; } }

        /// <summary>
        /// list of old pie objects that have been fired
        /// </summary>
        private List<Pies> oldPies = new List<Pies>();

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

            // Create objects for the parts of the ship
            for(int i=1;  i<=6;  i++)
            {
                phibesModels.Add(new PrisonModel(this, i));
            }

            // Create a player object
            player = new Player(this);
            pies = new Pies(this);
            dalek = new Dalek(this);
            alien = new Alien(this);

            //Particle system
            smokePlume = new SmokeParticleSystem3d(9);

            // Some basic setup for the display window
            this.IsMouseVisible = true;
			this.Window.AllowUserResizing = true;
			this.graphics.PreferredBackBufferWidth = 1024;
			this.graphics.PreferredBackBufferHeight = 768;

            // Basic camera settings
            camera = new Camera(graphics);
            camera.Eye = new Vector3(800, 180, 1053);
            camera.Center = new Vector3(275, 90, 1053);
            camera.FieldOfView = MathHelper.ToRadians(42);

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
            player.Initialize();

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
            pies.LoadContent(Content);
            pies.LoadPiesInBazooka();
            player.LoadContent(Content);
            dalek.LoadContent(Content);
            alien.LoadContent(Content);

            smokePlume.LoadContent(Content);

            foreach (PrisonModel model in phibesModels)
            {
                model.LoadContent(Content);
            }

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
                /*
                lineDraw.Crosshair(new Vector3(0, 100, 0), 20, Color.White);

                lineDraw.Begin();
                lineDraw.Vertex(new Vector3(0, 150, 0), Color.White);
                lineDraw.Vertex(new Vector3(50, 100, 0), Color.Red);
                lineDraw.End();*/

                pies.Update(gameTime);
                foreach (Pies pie in oldPies)
                {
                    pie.Update(gameTime);
                }

                player.Update(gameTime);
                dalek.Update(gameTime);
                alien.Update(gameTime);

                if (pies.PieCheckReload())
                {
                    oldPies.Add(pies);
                    pies = new Pies(this);
                    pies.LoadContent(Content);
                    pies.LoadPiesInBazooka();
                }

                foreach (PrisonModel model in phibesModels)
                {
                    model.Update(gameTime);
                }

                camera.Update(gameTime);

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

                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

                graphics.GraphicsDevice.Clear(Color.Black);

                foreach (PrisonModel model in phibesModels)
                {
                    model.Draw(graphics, gameTime);
                }

                pies.Draw(graphics, gameTime);
                foreach (Pies pie in oldPies)
                {
                    pie.Draw(graphics, gameTime);
                }

                player.Draw(graphics, gameTime);
                dalek.Draw(graphics, gameTime);
                alien.Draw(graphics, gameTime);

                smokePlume.Draw(GraphicsDevice, camera);

                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                dalek.Spit.SpitModel.Draw(graphics, gameTime, dalek.Spit.Transform);
                alien.Spit.SpitModel.Draw(graphics, gameTime, alien.Spit.Transform);
                GraphicsDevice.BlendState = BlendState.Opaque;

                base.Draw(gameTime);

                //Show score and pies in bazooka
                spriteBatch.Begin();
                spriteBatch.DrawString(UIFont, "Pies: " + (10 - totalPiesFired).ToString(), new Vector2(10, 10), Color.White);
                spriteBatch.DrawString(UIFont, "Score: " + score.ToString(), new Vector2(10, 25), Color.White);
                spriteBatch.End();
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            }
            else if (current == GameState.results)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(UIFont, "You are dead and all your friends are dead.", new Vector2(10, 10), Color.White);
                spriteBatch.End();
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
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

                /*foreach (Effect effect in mesh.Effects)
                {
                    mesh.Effects = Content.Load<Effect>("PhibesEffect2");
                    Matrix temp = world;
                    effect.Parameters["World"].SetValue(temp);
                    effect.Parameters["View"].SetValue(Camera.View);
                    effect.Parameters["Projection"].SetValue(Camera.Projection);
                }
                mesh.Draw();*/
            }
        }

        public void ResetPies()
        {
            totalPiesFired = 0;
            needPies = false;

            oldPies.Add(pies);
            pies = new Pies(this);
            pies.LoadContent(Content);
            pies.LoadPiesInBazooka();
        }
    }
}
