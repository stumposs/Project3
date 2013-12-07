using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class Alien : Enemy
    {
        /// <summary>
        /// Our animated model
        /// </summary>
        //private AnimatedModel alien;
        //public AnimatedModel Alien { get { return alien; } }

        public Alien(PrisonGame game)
        {
            this.game = game;
            enemy = new AnimatedModel(game, "Alien");
            location = new Vector3(1100, 0, -1400);
            enemyCollision = new BoundingCylinder(game, location);
            spit = new Spit(game, this);

            enemy.AddAssetClip("stance", "Alien-stance");
            enemy.AddAssetClip("catcheat", "Alien-catcheat");
            enemy.AddAssetClip("walkloop", "Alien-walkloop");
            SetEnemyTransform();
        }

        public void LoadContent(ContentManager content)
        {
            enemy.LoadContent(content);
            spit.LoadContent(content);
            regions = game.Player.Regions;

            enemy.PlayClip("stance");
        }

        public void Update(GameTime gameTime)
        {
            float deltaTotal = (float)gameTime.ElapsedGameTime.TotalSeconds;

            do{
                double delta = deltaTotal;

                if (!eating)
                {

                    //delta = 0;

                    float newOrientation;

                    //
                    // Update the orientation
                    //

                    string region = TestRegion(location);
                    if (region == "" || region.StartsWith("R_Door"))
                    {
                        int turnDegree = game.RandNum.Next(-45, 45);

                        if (turnDegree < 0)
                        {
                            turnDegree -= 45;
                        }
                        else
                        {
                            turnDegree += 45;
                        }

                        newOrientation = (float)(Math.PI * turnDegree / 180.0);
                        location -= Vector3.Normalize(transform.Backward) * 3;
                    }
                    else
                    {
                        newOrientation = 0;
                    }

                    orientation += newOrientation;

                    //
                    // Update the location
                    //

                    Vector3 translateVector = new Vector3((float)Math.Sin(orientation), 0, (float)Math.Cos(orientation));

                    SetEnemyTransform();
                    location += translateVector * moveRate * (float)deltaTotal;

                    SetEnemyTransform();

                    //
                    // Make the Head and disentagrator point toward the player
                    //

                    Vector3 playerVec = game.Player.Location - location;
                    Vector3 forwardVec = transform.Backward;
                    float deltaAngle = (float)Math.Atan2(playerVec.Z - forwardVec.Z, playerVec.X - forwardVec.X);

                    //alien.BoneTransforms[alien.Model.Bones["Head"].Index] = Matrix.CreateRotationZ(-deltaAngle + 1.6f - orientation) * alien.BindTransforms[alien.Model.Bones["Head"].Index];

                    //if ((orientation - deltaAngle) >= -1.6f && (orientation - deltaAngle) <= 1.6f)
                    //{
                    //alien.BoneTransforms[alien.Model.Bones["Arm2"].Index] = Matrix.CreateRotationZ(-deltaAngle + 1.6f - orientation) * alien.BindTransforms[alien.Model.Bones["Arm2"].Index];
                    //}

                    facing = -deltaAngle + 1.6f;

                    if (delta > enemy.Player.Clip.Duration - enemy.Player.Time)
                    {
                        delta = enemy.Player.Clip.Duration - enemy.Player.Time;

                        enemy.PlayClip("walkloop");
                    }
                }
                else
                {
                    if (delta > enemy.Player.Clip.Duration - enemy.Player.Time)
                    {
                        delta = enemy.Player.Clip.Duration - enemy.Player.Time;

                        enemy.PlayClip("catcheat");
                    }
                }

                deltaTotal -= (float)delta;
            } while (deltaTotal > 0);
            

            //enemy.PlayClip("catcheat");

            enemy.Update(gameTime.ElapsedGameTime.TotalSeconds);

            //
            // Spit at the player every 5 seconds
            //

            spitTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (spitTimer >= 5.0f)
            {
                spit.Firing = true;
                spitTimer = 0;
                eating = false;
                if (heldPie != null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (heldPie.HitEnemy[i] == this)
                        {
                            heldPie.PieStates[i] = Pies.pieState.eaten;
                        }
                    }
                }
            }

            string spitRegion = TestRegion(spit.Transform.Translation);
            if (spitRegion == "") // OR HAS COLLIDED WITH PLAYER
            {
                spit.Firing = false;
            }

            spit.Update(gameTime);
            enemyCollision.Update(gameTime, location);

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

            enemy.Draw(graphics, gameTime, transform, inCamera.View, inCamera.Projection);
        }
    }
}