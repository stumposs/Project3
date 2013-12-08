using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    /// <summary>
    /// This class describes our player in the game. 
    /// </summary>
    public class Player
    {
        #region Fields

        private Camera camera;
        public Camera Camera { get { return camera; } }

        /// <summary>
        /// Game that uses this player
        /// </summary>
        private PrisonGame game;

        //
        // Player location information.  We keep a x/z location (y stays zero)
        // and an orientation (which way we are looking).
        //

        /// <summary>
        /// Player location in the prison. Only x/z are important. y still stay zero
        /// unless we add some flying or jumping behavior later on.
        /// </summary>
        private Vector3 location = new Vector3(0, 0, 0);
        public Vector3 Location { get { return location; } set { location = value; } }

        /// <summary>
        /// The player orientation as a simple angle
        /// </summary>
        private float orientation = 1.6f;

        private float upDownAngle = 0.0f;

        /// <summary>
        /// The player transformation matrix. Places the player where they need to be.
        /// </summary>
        private Matrix transform;
        public Matrix Transform { get { return transform; } }

        /// <summary>
        /// The rotation rate in radians per second when player is rotating
        /// </summary>
        private float panRate = 2;

        /// <summary>
        /// Keeps track of the last game pad state
        /// </summary>
        GamePadState lastGPS;

        private enum States { Start, StanceStart, Stance, WalkStart, WalkLoopStart, WalkLoop}
        private States state = States.Start;

        /// <summary>
        /// Our animated model
        /// </summary>
        private AnimatedModel animatedModel;

        private string playerRegion;

        /// <summary>
        /// The collision cylinder for the player
        /// </summary>
        private BoundingCylinder playerCollision;
        public BoundingCylinder PlayerCollision { get { return playerCollision; } }



        #endregion


        public Player(PrisonGame game, Camera inCamera)
        {
            this.game = game;
            this.camera = inCamera;
            animatedModel = new AnimatedModel(game, "dalek");
            SetPlayerTransform();

            playerCollision = new BoundingCylinder(game, location);
        }

        public void Initialize()
        {
            lastGPS = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>
        /// Set the value of transform to match the current location
        /// and orientation.
        /// </summary>
        private void SetPlayerTransform()
        {
            transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;
        }


        public void LoadContent(ContentManager content)
        {
            animatedModel.LoadContent(content);
        }

        public string TestRegion(Vector3 v3)
        {
            // Convert to a 2D Point
            float x = v3.X;
            float y = v3.Z;

            return "";
        }

        public void Update(GameTime gameTime)
        {
            double deltaTotal = gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            float speed = 0;
            float pan = 0;
            float strafe = 0;
            float newOrientation;
            float deltaAngle;

            do
            {
                double delta = deltaTotal;

                //
                // State machine
                //

                switch (state)
                {
                    case States.Start:
                        state = States.StanceStart;
                        delta = 0;
                        break;

                    case States.StanceStart:
                        //animatedModel.PlayClip("stance");
                        location.Y = 0;
                        state = States.Stance;
                        break;

                    case States.Stance:
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);

                        if (speed > 0)
                        {
                            // We need to leave the stance state and start walking
                            //animatedModel.PlayClip("walkloop");
                            animatedModel.Player.Speed = speed;
                            state = States.WalkLoop;
                        }

                        if (pan != 0)
                        {
                            animatedModel.Player.Speed = pan;
                            state = States.TurnLoopStart;
                        }

                        break;

                    case States.TurnLoopStart:
                        if (pan > 0)
                        {
                            animatedModel.PlayClip("rightturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoop;
                            break;
                        }
                        else if (pan < 0)
                        {
                            animatedModel.PlayClip("leftturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoop;
                            break;
                        }
                        state = States.TurnLoop;

                        break;

                    case States.TurnLoop:
                        if (delta > animatedModel.Player.Clip.Duration - animatedModel.Player.Time)
                        {
                            delta = animatedModel.Player.Clip.Duration - animatedModel.Player.Time;

                            // The clip is done after this update
                            state = States.TurnLoopStart;
                        }

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                        if (pan == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            animatedModel.Player.Speed = pan;
                        }
                        break;

                    case States.WalkStart:
                        if (delta > animatedModel.Player.Clip.Duration - animatedModel.Player.Time)
                        {
                            delta = animatedModel.Player.Clip.Duration - animatedModel.Player.Time;

                            // The clip is done after this update
                            state = States.WalkLoopStart;
                        }

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);

                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            animatedModel.Player.Speed = speed;
                        }

                        break;
                    case States.WalkLoop:
                        location.Y = 0;
                        if (delta > animatedModel.Player.Clip.Duration - animatedModel.Player.Time)
                        {
                            delta = animatedModel.Player.Clip.Duration - animatedModel.Player.Time;

                            // The clip is done after this update
                            state = States.WalkLoopStart;
                        }

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            animatedModel.Player.Speed = speed;
                        }

                        break;

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                        if (pan == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            animatedModel.Player.Speed = pan;
                        }
                        break;

                    case States.WalkStartBazooka:
                        if (delta > animatedModel.Player.Clip.Duration - animatedModel.Player.Time)
                        {
                            delta = animatedModel.Player.Clip.Duration - animatedModel.Player.Time;

                            // The clip is done after this update
                            animatedModel.PlayClip("walkstartbazooka");
                            state = States.WalkLoopStartBazooka;
                        }

                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);

                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            animatedModel.Player.Speed = speed;
                        }

                        break;

                    case States.WalkLoopStartBazooka:
                        animatedModel.PlayClip("walkloopbazooka").Speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        state = States.WalkLoopBazooka;
                        break;

                    case States.WalkLoopBazooka:
                        location.Y = 0;
                        if (delta > animatedModel.Player.Clip.Duration - animatedModel.Player.Time)
                        {
                            delta = animatedModel.Player.Clip.Duration - animatedModel.Player.Time;

                            // The clip is done after this update
                            state = States.WalkLoopStartBazooka;
                        }

                        strafe = GetDesiredStrafe(ref keyboardState, ref gamePadState);
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            animatedModel.Player.Speed = speed;
                        }

                        break;
                }

                // 
                // State update
                //

                animatedModel.Update(delta);

                //
                // Part 1:  Compute a new orientation
                //

                Matrix deltaMatrix = animatedModel.DeltaMatrix;
                deltaAngle = (float)Math.Atan2(deltaMatrix.Backward.X, deltaMatrix.Backward.Z);
                newOrientation = orientation + deltaAngle;

                //
                // Part 2:  Compute a new location
                //

                // We are likely rotated from the angle the model expects to be in
                // Determine that angle.
                Matrix rootMatrix = animatedModel.RootMatrix;
                float actualAngle = (float)Math.Atan2(rootMatrix.Backward.X, rootMatrix.Backward.Z);
                Vector3 newLocation = location + Vector3.TransformNormal(animatedModel.DeltaPosition + new Vector3(strafe, 0, 0),
                               Matrix.CreateRotationY(newOrientation - actualAngle));

                //
                // Update the orientation
                //

                orientation = newOrientation;

                //
                // Update the location
                //


                bool collision = false;     // Until we know otherwise

                //string region = TestRegion(newLocation);
                string region = "lol";
                playerRegion = region;

                if (region == "")
                {
                    // If not in a region, we have stepped out of bounds
                    collision = true;
                }

                if (!collision)
                {
                    location = newLocation;
                }

                SetPlayerTransform();

                bool collisionCamera = false;
                camera.Center = location + new Vector3(0,100,0);
                Vector3 newCameraLocation = location + new Vector3(300, 100, 0);
                camera.Eye = newCameraLocation;
                string regionCamera = TestRegion(newCameraLocation);

                /*if (regionCamera == "")
                {
                    // If not in a region, we have stepped out of bounds
                    collisionCamera = true;
                }*/

                /*if (!collisionCamera)
                {
                    game.Camera.Eye = newCameraLocation;
                }
                else
                {
                    int cameraDistance = 0;
                    regionCamera = "playerLoc";
                    while (regionCamera != "")
                    {
                        newCameraLocation = Vector3.Transform(new Vector3(0, 180, cameraDistance), transform);
                        cameraDistance -= 1;
                        regionCamera = TestRegion(newCameraLocation);
                    }
                    game.Camera.Eye = newCameraLocation;
                }*/
                deltaTotal -= delta;
            } while (deltaTotal > 0);

 
            //do other keyboard based actions

            if (keyboardState.IsKeyDown(Keys.D1) && lastKeyboardState.IsKeyUp(Keys.D1))
            {
                if (wieldBazooka)
                {
                    wieldBazooka = false;
                }
                else if (!wieldBazooka)
                {
                    wieldBazooka = true;
                }
            }

            playerCollision.Update(gameTime, location);

            lastKeyboardState = keyboardState;
        }


        /// <summary>
        /// This function is called to draw the player.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Camera inCamera)
        {
            Matrix transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;

            animatedModel.Draw(graphics, gameTime, transform, inCamera.View, inCamera.Projection);

        }

        private float GetDesiredSpeed(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.W))
                return 1;
            if (keyboardState.IsKeyDown(Keys.S))
                return -1;

            float speed = gamePadState.ThumbSticks.Right.Y;

            return speed;
        }

        private float GetDesiredTurnRate(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                return panRate;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                return -panRate;
            }

            return -gamePadState.ThumbSticks.Right.X * panRate;
        }

        private float GetDesiredStrafe(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.A))
                return 1;

            if (keyboardState.IsKeyDown(Keys.D))
                return -1;

            float speed = gamePadState.ThumbSticks.Right.Y;

            if (speed < 0)
                speed = 0;

            return speed;
        }

        public void PlayerAim()
        {
        }
    }
}
