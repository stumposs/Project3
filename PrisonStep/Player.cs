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

        /// <summary>
        /// This is a range from the door center that is considered
        /// to be under the door.  This much either side.
        /// </summary>
        private const float DoorUnderRange = 40;

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
        private Vector3 location = new Vector3(1000, 0, -1000);//new Vector3(275, 0, 1053);
        public Vector3 Location { get { return location; } }

        /// <summary>
        /// The player orientation as a simple angle
        /// </summary>
        private float orientation = 1.6f;

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
        /// The player move rate in centimeters per second
        /// </summary>
        private float moveRate = 500;

        /// <summary>
        /// Id for a door we are opening or 0 if none.
        /// </summary>
        private int openDoor = 0;

        /// <summary>
        /// Keeps track of the last game pad state
        /// </summary>
        GamePadState lastGPS;

        /// <summary>
        /// Tells if the player has chosen to wield the bazooka or not.
        /// </summary>
        private bool wieldBazooka = false;
        public bool WieldBazooka { get { return wieldBazooka; } }

        /// <summary>
        /// Tells if the player has chosen to crouch;
        /// </summary>
        private bool crouch = false;
        public bool Crouch { get { return crouch; } }

        /// <summary>
        /// Tells us if the bazooka is raised and we are aiming
        /// </summary>
        private bool aiming = false;
        public bool Aiming { get { return aiming; } }

        /// <summary>
        /// Stores the angle we are in when we start aiming
        /// </summary>
        private float aimOrient = 0;
        public float AimOrient { get { return aimOrient; } }

        /// <summary>
        /// Stores the angle we are aiming at;
        /// </summary>
        private float aimAngle = 0;
        public float AimAngle { get { return aimAngle; } }

        /// <summary>
        /// The previous keyboard state
        /// </summary>
        KeyboardState lastKeyboardState;

        private enum States { Start, StanceStart, Stance, Turn, TurnLoopStart, TurnLoop, WalkStart, WalkLoopStart, WalkLoop, 
            StanceRaised, CrouchBazooka, WalkStartBazooka, WalkLoopStartBazooka, WalkLoopBazooka, TurnBazooka, TurnLoopStartBazooka, TurnLoopBazooka, Aim }
        private States state = States.Start;

        /// <summary>
        /// Our animated model
        /// </summary>
        private AnimatedModel victoria;

        private AnimatedModel pies;

        //private AnimatedModel bazooka;

        private Dictionary<string, List<Vector2>> regions = new Dictionary<string, List<Vector2>>();
        public Dictionary<string, List<Vector2>> Regions { get { return regions; } }

        private string playerRegion;

        /// <summary>
        /// The collision cylinder for the player
        /// </summary>
        private BoundingCylinder playerCollision;
        public BoundingCylinder PlayerCollision { get { return playerCollision; } }



        #endregion


        public Player(PrisonGame game)
        {
            this.game = game;
            victoria = new AnimatedModel(game, "Victoria");
            //bazooka = new AnimatedModel(game, "PieBazooka");

            victoria.AddAssetClip("dance", "Victoria-dance");
            victoria.AddAssetClip("stance", "Victoria-stance");
            victoria.AddAssetClip("walk", "Victoria-walk");
            victoria.AddAssetClip("walkstart", "Victoria-walkstart");
            victoria.AddAssetClip("walkloop", "Victoria-walkloop");
            victoria.AddAssetClip("leftturn", "Victoria-leftturn");
            victoria.AddAssetClip("rightturn", "Victoria-rightturn");

            victoria.AddAssetClip("crouchbazooka", "Victoria-crouchbazooka");
            victoria.AddAssetClip("lowerbazooka", "Victoria-lowerbazooka");
            victoria.AddAssetClip("raisebazooka", "Victoria-raisebazooka");
            victoria.AddAssetClip("walkloopbazooka", "Victoria-walkloopbazooka");
            victoria.AddAssetClip("walkstartbazooka", "Victoria-walkstartbazooka");
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
            //if(!aiming)
                transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;
        }


        public void LoadContent(ContentManager content)
        {
            victoria.LoadContent(content);
            //bazooka.LoadContent(content);

            Model model = content.Load<Model>("AntonPhibesCollision");

            Matrix[] M = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(M);

            foreach (ModelMesh mesh in model.Meshes)
            {
                // For accumulating the triangles for this mesh
                List<Vector2> triangles = new List<Vector2>();

                // Loop over the mesh parts
                foreach(ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // 
                    // Obtain the vertices for the mesh part
                    //

                    int numVertices = meshPart.VertexBuffer.VertexCount;
                    VertexPositionColorTexture[] verticesRaw = new VertexPositionColorTexture[numVertices];
                    meshPart.VertexBuffer.GetData<VertexPositionColorTexture>(verticesRaw);

                    //
                    // Obtain the indices for the mesh part
                    //

                    int numIndices = meshPart.IndexBuffer.IndexCount;
                    short [] indices = new short[numIndices];
                    meshPart.IndexBuffer.GetData<short>(indices);

                    //
                    // Build the list of triangles
                    //

                    for (int i = 0; i < meshPart.PrimitiveCount * 3; i++)
                    {
                        // The actual index is relative to a supplied start position
                        int index = i + meshPart.StartIndex;

                        // Transform the vertex into world coordinates
                        Vector3 v = Vector3.Transform(verticesRaw[indices[index] + meshPart.VertexOffset].Position, M[mesh.ParentBone.Index]);
                        triangles.Add(new Vector2(v.X, v.Z));
                    }

                }

                regions[mesh.Name] = triangles;
            }

            //AnimationPlayer player = victoria.PlayClip("walk");
        }

        public string TestRegion(Vector3 v3)
        {
            // Convert to a 2D Point
            float x = v3.X;
            float y = v3.Z;

            foreach (KeyValuePair<string, List<Vector2>> region in regions)
            {
                // For now we ignore the walls
                if (region.Key.StartsWith("W") && playerRegion != "R_Section6" && playerRegion != "R_Section5" && playerRegion != "R_Door4" && playerRegion != "R_Door5")
                    continue;

                for (int i = 0; i < region.Value.Count; i += 3)
                {
                    float x1 = region.Value[i].X;
                    float x2 = region.Value[i + 1].X;
                    float x3 = region.Value[i + 2].X;
                    float y1 = region.Value[i].Y;
                    float y2 = region.Value[i + 1].Y;
                    float y3 = region.Value[i + 2].Y;

                    float d = 1.0f / ((x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3));
                    float l1 = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) * d;
                    if (l1 < 0)
                        continue;

                    float l2 = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) * d;
                    if (l2 < 0)
                        continue;

                    float l3 = 1 - l1 - l2;
                    if (l3 < 0)
                        continue;

                    return region.Key;
                }
            }

            return "";
        }

        #region oldUpdate

        /*public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            victoria.Update(gameTime.ElapsedGameTime.TotalSeconds);

            //
            // Part 1:  Compute a new orientation
            //

            Matrix deltaMatrix = victoria.DeltaMatrix;
            float deltaAngle = (float)Math.Atan2(deltaMatrix.Backward.X, deltaMatrix.Backward.Z);
            float newOrientation = orientation + deltaAngle;

            //
            // Part 2:  Compute a new location
            //

            // We are likely rotated from the angle the model expects to be in
            // Determine that angle.
            Matrix rootMatrix = victoria.RootMatrix;
            float actualAngle = (float)Math.Atan2(rootMatrix.Backward.X, rootMatrix.Backward.Z);
            Vector3 newLocation = location + Vector3.TransformNormal(victoria.DeltaPosition,
                               Matrix.CreateRotationY(newOrientation - actualAngle));

            //
            // I'm just taking these here.  You'll likely want to add something 
            // for collision detection instead.
            //

            location = newLocation;
            orientation = newOrientation;
            SetPlayerTransform();

            // How much we will move the player
            float translation = 0;
            float rotation = 0;
            
            /*

            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                rotation += panRate * delta;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                rotation -= panRate * delta;
            }



            if (keyboardState.IsKeyDown(Keys.Up))
            {
                translation += moveRate * delta;
            }

            if (keyboardState.IsKeyDown(Keys.Down))
            {
                translation -= moveRate * delta;
            }

            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            rotation += -gamePadState.ThumbSticks.Right.X * panRate * delta;
            translation += gamePadState.ThumbSticks.Right.Y * moveRate * delta;

            end comment here

            //
            // Update the orientation
            //

            orientation += rotation;

            //
            // Update the location
            //

            Vector3 translateVector = new Vector3((float)Math.Sin(orientation), 0, (float)Math.Cos(orientation));
            translateVector *= translation;

            //Vector3 newLocation = location + translateVector;

            bool collision = false;     // Until we know otherwise

            string region = TestRegion(newLocation);
            
            // Slimed support
            if (!game.Slimed && region == "R_Section6")
            {
                game.Slimed = true;
            }
            else if (game.Slimed && region == "R_Section1")
            {
                game.Slimed = false;
            }

            if (region == "")
            {
                // If not in a region, we have stepped out of bounds
                collision = true;
            }
            else if (region.StartsWith("R_Door"))   // Are we in a door region
            {
                // What is the door number for the region we are in?
                int dnum = int.Parse(region.Substring(6));

                // Are we currently facing the door or walking through a 
                // door?

                bool underDoor;
                if (DoorShouldBeOpen(dnum, location, transform.Backward, out underDoor))
                {
                    SetOpenDoor(dnum);
                }
                else
                {
                    SetOpenDoor(0);
                }

                if (underDoor)
                {
                    // is the door actually open right now?
                    bool isOpen = false;
                    foreach (PrisonModel model in game.PhibesModels)
                    {
                        if (model.DoorIsOpen(dnum))
                        {
                            isOpen = true;
                            break;
                        }
                    }

                    if (!isOpen)
                        collision = true;
                }
            }
            else if (openDoor > 0)
            {
                // Indicate none are open
                SetOpenDoor(0);
            }

            if (!collision)
            {
                location = newLocation;
            }

            SetPlayerTransform();

            //
            // Make the camera follow the player
            //

            //game.Camera.Eye = location + new Vector3(0, 180, 0);
            //game.Camera.Center = game.Camera.Eye + transform.Backward + new Vector3(0, -0.1f, 0);

            // Retain the game pad state
            lastGPS = gamePadState;


        }*/

        #endregion oldUpdate

        public void Update(GameTime gameTime)
        {
            double deltaTotal = gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            float speed = 0;
            float pan = 0;
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
                        victoria.PlayClip("stance");
                        location.Y = 0;
                        if (!wieldBazooka)
                        {
                            state = States.Stance;
                        }
                        else if (wieldBazooka)
                        {
                            victoria.PlayClip("raisebazooka");
                            state = States.StanceRaised;
                        }
                        break;

                    case States.Stance:
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);

                        if (speed > 0)
                        {
                            // We need to leave the stance state and start walking
                            victoria.PlayClip("walkstart");
                            victoria.Player.Speed = speed;
                            state = States.WalkStart;
                        }

                        if (pan != 0)
                        {
                            victoria.Player.Speed = pan;
                            state = States.TurnLoopStart;
                        }

                        break;

                    case States.TurnLoopStart:
                        if (pan > 0)
                        {
                            victoria.PlayClip("rightturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoop;
                            break;
                        }
                        else if (pan < 0)
                        {
                            victoria.PlayClip("leftturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoop;
                            break;
                        }
                        state = States.TurnLoop;

                        break;

                    case States.TurnLoop:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.TurnLoopStart;
                        }

                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                        if (pan == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = pan;
                        }
                        break;

                    case States.WalkStart:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.WalkLoopStart;
                        }

                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);

                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = speed;
                        }

                        break;

                    case States.WalkLoopStart:
                        victoria.PlayClip("walkloop").Speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        state = States.WalkLoop;
                        break;

                    case States.WalkLoop:
                        location.Y = 0;
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.WalkLoopStart;
                        }

                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = speed;
                        }

                        break;

                    case States.StanceRaised:
                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);

                        location.Y = 0;

                        if (speed > 0)
                        {
                            // We need to leave the stance state and start walking
                            victoria.PlayClip("lowerbazooka");
                            victoria.Player.Speed = speed;
                            state = States.WalkStartBazooka;
                        }

                        if (pan != 0 && !aiming)
                        {
                            victoria.Player.Speed = pan;
                            victoria.PlayClip("lowerbazooka");
                            state = States.TurnLoopStartBazooka;
                        }

                        if(aiming)
                        {
                            pan = 0;
                            state = States.Aim;
                        }

                        if (!wieldBazooka)
                        {
                            victoria.PlayClip("lowerbazooka");
                            state = States.Stance;
                        }

                        if (crouch)
                        {
                            victoria.PlayClip("crouchbazooka");
                            state = States.CrouchBazooka;
                        }

                        break;

                    case States.Aim:
                        if (!keyboardState.IsKeyDown(Keys.LeftShift))
                        {
                            pan = 0;
                            state = States.StanceRaised;
                        }
                        else
                        {
                            //int spineInd = victoria.Model.Bones["Bip01 Spine1"].Index;
                            //victoria.AbsoTransforms[spineInd].Backward = new Vector3(orientation, 0, 0);
                            pan = 0;
                            state = States.Aim;
                        }

                        break;

                    case States.CrouchBazooka:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            crouch = false;
                            victoria.PlayClip("raisebazooka");
                            state = States.StanceRaised;
                        }

                        break;

                    case States.TurnLoopStartBazooka:
                        if (pan > 0)
                        {
                            victoria.PlayClip("rightturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoopBazooka;
                            break;
                        }
                        else if (pan < 0)
                        {
                            victoria.PlayClip("leftturn").Speed = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                            state = States.TurnLoopBazooka;
                            break;
                        }
                        state = States.TurnLoopBazooka;

                        break;

                    case States.TurnLoopBazooka:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.TurnLoopStartBazooka;
                        }

                        pan = GetDesiredTurnRate(ref keyboardState, ref gamePadState);
                        if (pan == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = pan;
                        }
                        break;

                    case States.WalkStartBazooka:
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            victoria.PlayClip("walkstartbazooka");
                            state = States.WalkLoopStartBazooka;
                        }

                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);

                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = speed;
                        }

                        break;

                    case States.WalkLoopStartBazooka:
                        victoria.PlayClip("walkloopbazooka").Speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        state = States.WalkLoopBazooka;
                        break;

                    case States.WalkLoopBazooka:
                        location.Y = 0;
                        if (delta > victoria.Player.Clip.Duration - victoria.Player.Time)
                        {
                            delta = victoria.Player.Clip.Duration - victoria.Player.Time;

                            // The clip is done after this update
                            state = States.WalkLoopStartBazooka;
                        }

                        speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                        if (speed == 0)
                        {
                            delta = 0;
                            state = States.StanceStart;
                        }
                        else
                        {
                            victoria.Player.Speed = speed;
                        }

                        break;
                }

                // 
                // State update
                //

                if (!aiming)
                {
                    orientation += GetDesiredTurnRate(ref keyboardState, ref gamePadState) * (float)delta;
                }
                else
                {
                    aimAngle += GetDesiredTurnRate(ref keyboardState, ref gamePadState) * (float)delta;
                }

                victoria.Update(delta);

                //
                // Part 1:  Compute a new orientation
                //

                Matrix deltaMatrix = victoria.DeltaMatrix;
                deltaAngle = (float)Math.Atan2(deltaMatrix.Backward.X, deltaMatrix.Backward.Z);
                newOrientation = orientation + deltaAngle;

                if (keyboardState.IsKeyDown(Keys.LeftShift) && lastKeyboardState.IsKeyDown(Keys.LeftShift) && (state == States.Aim || state == States.StanceRaised))
                {
                    if (!aiming)
                    {
                        aimOrient = orientation;
                        aimAngle = orientation;
                    }
                    aiming = true;

                    if (aimOrient - aimAngle > 1.222f)
                    {
                        aimAngle = aimOrient - 1.222f;
                    }

                    if (aimOrient - aimAngle < -1.222f)
                    {
                        aimAngle = aimOrient + 1.222f;
                    }

                }
                else
                {
                    aiming = false;
                    int spineInd = victoria.Model.Bones["Bip01 Spine1"].Index;
                    victoria.AbsoTransforms[spineInd].Backward = new Vector3(1, 0, 0);
                }

                //
                // Part 2:  Compute a new location
                //

                // We are likely rotated from the angle the model expects to be in
                // Determine that angle.
                Matrix rootMatrix = victoria.RootMatrix;
                float actualAngle = (float)Math.Atan2(rootMatrix.Backward.X, rootMatrix.Backward.Z);
                Vector3 newLocation = location + Vector3.TransformNormal(victoria.DeltaPosition,
                               Matrix.CreateRotationY(newOrientation - actualAngle));

                //
                // Update the orientation
                //

                orientation = newOrientation;

                //
                // Update the location
                //

                //Vector3 newLocation = location + translateVector;

                bool collision = false;     // Until we know otherwise

                string region = TestRegion(newLocation);
                playerRegion = region;

                // Slimed support
                //if (!game.Slimed && region == "R_Section6")
                //{
                //    game.Slimed = true;
                //}
                
                if (game.Slimed && region == "R_Section1")
                {
                    game.Slimed = false;
                }

                if (game.NeedPies && region == "R_Section1")
                {
                    game.ResetPies();
                }

                if (region == "")
                {
                    // If not in a region, we have stepped out of bounds
                    collision = true;
                }
                else if (region.StartsWith("R_Door"))   // Are we in a door region
                {
                    // What is the door number for the region we are in?
                    int dnum = int.Parse(region.Substring(6));

                    // Are we currently facing the door or walking through a 
                    // door?

                    bool underDoor;
                    if (DoorShouldBeOpen(dnum, location, transform.Backward, out underDoor))
                    {
                        SetOpenDoor(dnum);
                    }
                    else
                    {
                        SetOpenDoor(0);
                    }

                    if (underDoor)
                    {
                        // is the door actually open right now?
                        bool isOpen = false;
                        foreach (PrisonModel model in game.PhibesModels)
                        {
                            if (model.DoorIsOpen(dnum))
                            {
                                isOpen = true;
                                break;
                            }
                        }

                        if (!isOpen)
                            collision = true;
                    }
                }
                else if (openDoor > 0)
                {
                    // Indicate none are open
                    SetOpenDoor(0);
                }

                if (!collision)
                {
                    location = newLocation;
                }

                SetPlayerTransform();

                //get the new camera location relative to the player
                //Vector3 newCameraLocation = location + new Vector3(0, 180, -40);
                //game.Camera.Center = game.Camera.Eye + transform.Backward + new Vector3(0, -0.1f, 0);

                Vector3 newCameraLocation = Vector3.Transform(new Vector3(0, 180, -400), transform);
                game.Camera.Center = location + new Vector3(0, 100, 0);

                bool collisionCamera = false;
                string regionCamera = TestRegion(newCameraLocation);

                if (regionCamera == "")
                {
                    // If not in a region, we have stepped out of bounds
                    collisionCamera = true;
                }

                if (!collisionCamera)
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
                }

                deltaTotal -= delta;
            } while (deltaTotal > 0);

            //check the current pie locations

            Vector3 pieLocation = game.Pies.PieModel.AbsoTransforms[game.Pies.PieModel.Model.Bones["Pie1"].Index].Translation;
            string regionPie = TestRegion(pieLocation);
            if (regionPie == "" && game.Pies.PieStates[0] == Pies.pieState.firing)
            {
                game.Pies.PieHitWall(1);
            }

            pieLocation = game.Pies.PieModel.AbsoTransforms[game.Pies.PieModel.Model.Bones["Pie2top01"].Index].Translation;
            regionPie = TestRegion(pieLocation);
            if (regionPie == "" && game.Pies.PieStates[1] == Pies.pieState.firing)
            {
                game.Pies.PieHitWall(2);
            }

            pieLocation = game.Pies.PieModel.AbsoTransforms[game.Pies.PieModel.Model.Bones["Pie3top"].Index].Translation;
            regionPie = TestRegion(pieLocation);
            if (regionPie == "" && game.Pies.PieStates[2] == Pies.pieState.firing)
            {
                game.Pies.PieHitWall(3);
            }


            //do other keyboard based actions

            if (keyboardState.IsKeyDown(Keys.W) && lastKeyboardState.IsKeyUp(Keys.W))
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

            if (keyboardState.IsKeyDown(Keys.LeftControl) && lastKeyboardState.IsKeyUp(Keys.LeftControl) && wieldBazooka)
            {
                if (crouch)
                {
                    crouch = false;
                }
                else if (!crouch)
                {
                    crouch = true;
                }
            }

            if (keyboardState.IsKeyDown(Keys.Space) && lastKeyboardState.IsKeyUp(Keys.Space) && aiming)
            {
                game.Pies.FirePie();
            }

            playerCollision.Update(gameTime, location);

            lastKeyboardState = keyboardState;
        }


        /// <summary>
        /// This function is called to draw the player.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            Matrix transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;

            if (wieldBazooka)
            {
                int handInd;
                Matrix boneMat;

                handInd = victoria.Model.Bones["Bip01 R Hand"].Index;

                boneMat = victoria.AbsoTransforms[handInd] * transform;

                Matrix bazTransform =
                    Matrix.CreateRotationX(MathHelper.ToRadians(109.5f)) *
                    Matrix.CreateRotationY(MathHelper.ToRadians(9.7f)) *
                    Matrix.CreateRotationZ(MathHelper.ToRadians(72.9f)) *
                    Matrix.CreateTranslation(-9.6f, 11.85f, 21.1f) *
                    boneMat;

                game.BazTransform = bazTransform;
                //game.DrawModel(graphics, game.Bazooka, bazTransform);


            }
            else
            {
                game.BazTransform = Matrix.CreateTranslation(0, -100, 0);
            }

            victoria.Draw(graphics, gameTime, transform);
            game.Bazooka.Draw(graphics, gameTime, game.BazTransform);

        }

        /// <summary>
        /// This is the logic that determines if a door should be open.  This is 
        /// based on a position and a direction we are traveling.  
        /// </summary>
        /// <param name="dnum">Door number we are interested in (1-5)</param>
        /// <param name="loc">A location near the door</param>
        /// <param name="dir">Direction we are currently facing as a vector.</param>
        /// <param name="doorVector">A vector pointing throught the door.</param>
        /// <param name="doorCenter">The center of the door.</param>
        /// <param name="under">Return value - indicates we are under the door</param>
        /// <returns>True if we are under the door</returns>
        private bool DoorShouldBeOpen(int dnum, Vector3 loc, Vector3 dir, out bool under)
        {
            Vector3 doorCenter;
            Vector3 doorVector;

            // I need to know information about the doors.  This 
            // is the location and a vector through the door for each door.
            switch (dnum)
            {
                case 1:
                    doorCenter = new Vector3(218, 0, 1023);
                    doorVector = new Vector3(1, 0, 0);
                    break;

                case 2:
                    doorCenter = new Vector3(-11, 0, -769);
                    doorVector = new Vector3(0, 0, 1);
                    break;

                case 3:
                    doorCenter = new Vector3(587, 0, -999);
                    doorVector = new Vector3(1, 0, 0);
                    break;

                case 4:
                    doorCenter = new Vector3(787, 0, -763);
                    doorVector = new Vector3(0, 0, 1);
                    break;

                case 5:
                default:
                    doorCenter = new Vector3(1187, 0, -1218);
                    doorVector = new Vector3(0, 0, 1);
                    break;
            }

            // I want the door vector to indicate the direction we are doing through the
            // door.  This depends on the side of the center we are on.
            Vector3 toDoor = doorCenter - loc;
            if (Vector3.Dot(toDoor, doorVector) < 0)
            {
                doorVector = -doorVector;
            }


            // Determine if we are under the door
            // Determine points after the center where we are 
            // considered to be under the door
            Vector3 doorBefore = doorCenter - doorVector * DoorUnderRange;
            Vector3 doorAfter = doorCenter + doorVector * DoorUnderRange;
            under = false;

            // If we have passed the point before the door, a vector 
            // to our position from that point will be pointing within 
            // 90 degrees of the door vector.  
            if (Vector3.Dot(loc - doorAfter, doorVector) <= 0 &&
                Vector3.Dot(loc - doorBefore, doorVector) >= 0)
            {
                under = true;
                return true;
            }

            // Are we facing the door?
            if (Vector3.Dot(dir, doorVector) >= 0)
            {
                // We are, so the door should be open
                return true;
            }

            return false;
        }


        /// <summary>
        /// Set the current open/opening door
        /// </summary>
        /// <param name="dnum">Door to set open or 0 if none</param>
        private void SetOpenDoor(int dnum)
        {
            // Is this already indicated?
            if (openDoor == dnum)
                return;

            // Is a door other than this already open?
            // If so, make it close
            if (openDoor > 0 && openDoor != dnum)
            {
                foreach (PrisonModel model in game.PhibesModels)
                {
                    model.SetDoor(openDoor, false);
                }
            }

            // Make this the open door and flag it as open
            openDoor = dnum;
            if (openDoor > 0)
            {
                foreach (PrisonModel model in game.PhibesModels)
                {
                    model.SetDoor(openDoor, true);
                }
            }
        }

        private float GetDesiredSpeed(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Up))
                return 1;

            float speed = gamePadState.ThumbSticks.Right.Y;

            // I'm not allowing you to walk backwards
            if (speed < 0)
                speed = 0;

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

        public void PlayerAim()
        {
            if (aiming)
            {
                victoria.BoneTransforms[victoria.Model.Bones["Bip01 Spine1"].Index] = Matrix.CreateRotationX(aimOrient - aimAngle) * victoria.BindTransforms[victoria.Model.Bones["Bip01 Spine1"].Index];
            }
        }
    }
}
