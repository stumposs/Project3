using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class Dalek : Enemy
    {
        public Dalek(PrisonGame game)
        {
            this.game = game;
            enemy = new AnimatedModel(game, "Dalek");
            location = new Vector3(760, 0, -360);
            enemyCollision = new BoundingCylinder(game, location);
            spit = new Spit(game, this);
            SetEnemyTransform();
        }

        public void LoadContent(ContentManager content)
        {
            enemy.LoadContent(content);
            spit.LoadContent(content);
            regions = game.Player.Regions;
        }

        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //
            // Part 1:  Compute a new orientation
            //
            if (!eating)
            {

                float newOrientation;

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
                location += translateVector * moveRate * (float)delta;

                SetEnemyTransform();

                //
                // Make the Head and disentagrator point toward the player
                //

                Vector3 playerVec = game.Player.Location - location;
                Vector3 forwardVec = transform.Backward;
                float deltaAngle = (float)Math.Atan2(playerVec.Z - forwardVec.Z, playerVec.X - forwardVec.X);

                enemy.BoneTransforms[enemy.Model.Bones["Head"].Index] = Matrix.CreateRotationZ(-deltaAngle + 1.6f - orientation) * enemy.BindTransforms[enemy.Model.Bones["Head"].Index];

                //if ((orientation - deltaAngle) >= -1.6f && (orientation - deltaAngle) <= 1.6f)
                //{
                enemy.BoneTransforms[enemy.Model.Bones["Arm2"].Index] = Matrix.CreateRotationZ(-deltaAngle + 1.6f - orientation) * enemy.BindTransforms[enemy.Model.Bones["Arm2"].Index];
                //}

                enemy.BoneTransforms[enemy.Model.Bones["PlungerArm"].Index] = new Matrix() * enemy.BindTransforms[enemy.Model.Bones["PlungerArm"].Index];

                facing = -deltaAngle + 1.6f;

            }
            else
            {
                SetEnemyTransform();
                enemy.BoneTransforms[enemy.Model.Bones["PlungerArm"].Index] = Matrix.CreateRotationX(-spitTimer / 3) * enemy.BindTransforms[enemy.Model.Bones["PlungerArm"].Index];
            }

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
            if (spitRegion == "")
            {
                spit.Firing = false;
            }

            game.Camera2.Center = location + new Vector3(0, 100, -300);
            game.Camera2.Eye = location + new Vector3(0, 100, 0);

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
