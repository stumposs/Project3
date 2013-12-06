using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class Pies
    {
        /// <summary>
        /// Game that uses this pie
        /// </summary>
        private PrisonGame game;

        /// <summary>
        /// The game's player
        /// </summary>
        private Player player;

        /// <summary>
        /// The pie transformation matrix. Places the pies where they need to be.
        /// </summary>
        private Matrix transform;

        /// <summary>
        /// The pie move rate in centimeters per second
        /// </summary>
        private float moveRate = 3000;

        /// <summary>
        /// Collision spheres for each pie
        /// </summary>
        private BoundingSphere pie1Collision;
        private BoundingSphere pie2Collision;
        private BoundingSphere pie3Collision;

        /// <summary>
        /// How many pies in this pie object have been fired.
        /// </summary>
        private int piesFired = 0;
        public int PiesFired { get { return piesFired; } }

        /// <summary>
        /// enum for pie states
        /// </summary>
        public enum pieState { loaded, firing, hitWall, hitAlien, eaten };

        /// <summary>
        /// Which pies are currently shooting through the air.
        /// </summary>
        private pieState[] pieStates = { pieState.loaded, pieState.loaded, pieState.loaded };
        public pieState[] PieStates { get { return pieStates; } }

        /// <summary>
        /// which enemy each pie has hit
        /// </summary>
        private Enemy[] hitEnemy = { new Enemy(), new Enemy(), new Enemy()};
        public Enemy[] HitEnemy { get { return hitEnemy; } }

        /// <summary>
        /// The vector in which each pie is being fired.
        /// </summary>
        private Matrix[] drawLoc = { new Matrix(), new Matrix(), new Matrix() };

        /// <summary>
        /// Tells us if this pie object is loaded in the bazooka
        /// </summary>
        private bool loaded = false;
        public bool Loaded { get { return loaded; } set { loaded = value; } }


        /// <summary>
        /// The model for the pies
        /// </summary>
        private AnimatedModel pies;
        public AnimatedModel PieModel { get { return pies; } }

        public Pies(PrisonGame game)
        {
            this.game = game;
            player = game.Player;
            pies = new AnimatedModel(game, "pies");
        }

        public void LoadContent(ContentManager content)
        {
            pies.LoadContent(content);

            pies.AbsoTransforms[pies.Model.Bones["Pie1"].Index] = Matrix.CreateTranslation(0, -20, 0);

            pies.AbsoTransforms[pies.Model.Bones["Pie2bot01"].Index] = Matrix.CreateTranslation(0, -20, 0);
            pies.AbsoTransforms[pies.Model.Bones["Pie2top01"].Index] = Matrix.CreateTranslation(0, -20, 0);

            pies.AbsoTransforms[pies.Model.Bones["Pie3bot"].Index] = Matrix.CreateTranslation(0, -20, 0);
            pies.AbsoTransforms[pies.Model.Bones["Pie3top"].Index] = Matrix.CreateTranslation(0, -20, 0);

            pie1Collision = pies.Model.Meshes[0].BoundingSphere;
            pie2Collision = pies.Model.Meshes[4].BoundingSphere;
            pie3Collision = pies.Model.Meshes[1].BoundingSphere;

        }

        public void Update(GameTime gameTime)
        {
            double delta = gameTime.ElapsedGameTime.TotalSeconds;
            //transform = game.BazTransform;
            transform = Matrix.CreateTranslation(0, 0, 0);

            Vector3 direction;

            //////////////////////////////////
            //Pie 1

            if (pieStates[0] == pieState.loaded)
            {
                if (game.Player.WieldBazooka && !game.NeedPies)
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie1"].Index] = game.BazTransform;
                }
                else
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie1"].Index] = Matrix.CreateTranslation(0, 0, 0);
                }
            }
            else if (pieStates[0] == pieState.firing)
            {
                direction = Vector3.TransformNormal(new Vector3(0, 0, 1), drawLoc[0]);
                pies.AbsoTransforms[pies.Model.Bones["Pie1"].Index].Translation += direction * moveRate * (float)delta;

                pie1Collision = pies.Model.Meshes[0].BoundingSphere;
                pie1Collision = pie1Collision.Transform(pies.AbsoTransforms[pies.Model.Bones["Pie1"].Index]);

                if (game.Dalek.EnemyCollision.TestForCollision(pie1Collision))
                {
                    game.Score += 15;
                    pieStates[0] = pieState.hitAlien;
                    hitEnemy[0] = game.Dalek;
                    game.Dalek.EatPie(this, 1);
                }

                if (game.Alien.EnemyCollision.TestForCollision(pie1Collision))
                {
                    game.Score += 15;
                    pieStates[0] = pieState.hitAlien;
                    hitEnemy[0] = game.Alien;
                    game.Alien.EatPie(this, 3);
                }
            }
            else if (pieStates[0] == pieState.hitAlien)
            {
                if (hitEnemy[0] == game.Dalek)
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie1"].Index] = hitEnemy[0].EnemyModel.AbsoTransforms[hitEnemy[0].EnemyModel.Model.Bones["PlungerArm"].Index] * Matrix.CreateTranslation(0, 0, 80) * Matrix.CreateRotationX(-hitEnemy[0].SpitTimer / 6) * game.Dalek.Transform;
                }

                if (hitEnemy[0] == game.Alien)
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie1"].Index] = hitEnemy[0].EnemyModel.AbsoTransforms[hitEnemy[0].EnemyModel.Model.Bones["Bip01 L Hand"].Index] * game.Alien.Transform;
                }
            }
            else if (pieStates[0] == pieState.eaten)
            {
                pies.AbsoTransforms[pies.Model.Bones["Pie1"].Index] = transform;
            }

            ////////////////////////////////////
            //Pie 2

            if (pieStates[1] == pieState.loaded)
            {
                if (game.Player.WieldBazooka && !game.NeedPies)
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie2bot01"].Index] = game.BazTransform;
                    pies.AbsoTransforms[pies.Model.Bones["Pie2top01"].Index] = game.BazTransform;
                }
                else
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie2bot01"].Index] = Matrix.CreateTranslation(0, 0, 0);
                    pies.AbsoTransforms[pies.Model.Bones["Pie2top01"].Index] = Matrix.CreateTranslation(0, 0, 0);
                }
            }
            else if (pieStates[1] == pieState.firing)
            {
                direction = Vector3.TransformNormal(new Vector3(0, 0, 1), drawLoc[1]);
                pies.AbsoTransforms[pies.Model.Bones["Pie2bot01"].Index].Translation += direction * moveRate * (float)delta;
                pies.AbsoTransforms[pies.Model.Bones["Pie2top01"].Index].Translation += direction * moveRate * (float)delta;

                pie2Collision = pies.Model.Meshes[4].BoundingSphere;
                pie2Collision = pie2Collision.Transform(pies.AbsoTransforms[pies.Model.Bones["Pie2bot01"].Index]);

                if (game.Dalek.EnemyCollision.TestForCollision(pie2Collision))
                {
                    game.Score += 15;
                    pieStates[1] = pieState.hitAlien;
                    hitEnemy[1] = game.Dalek;
                    game.Dalek.EatPie(this, 2);
                }

                if (game.Alien.EnemyCollision.TestForCollision(pie2Collision))
                {
                    game.Score += 15;
                    pieStates[1] = pieState.hitAlien;
                    hitEnemy[1] = game.Alien;
                    game.Alien.EatPie(this, 3);
                }

            }
            else if (pieStates[1] == pieState.hitAlien)
            {
                if (hitEnemy[1] == game.Dalek)
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie2bot01"].Index] = hitEnemy[1].EnemyModel.AbsoTransforms[hitEnemy[1].EnemyModel.Model.Bones["PlungerArm"].Index] * Matrix.CreateTranslation(0, 0, 80) * Matrix.CreateRotationX(-hitEnemy[1].SpitTimer / 6) * game.Dalek.Transform;
                    pies.AbsoTransforms[pies.Model.Bones["Pie2top01"].Index] = hitEnemy[1].EnemyModel.AbsoTransforms[hitEnemy[1].EnemyModel.Model.Bones["PlungerArm"].Index] * Matrix.CreateTranslation(0, 0, 80) * Matrix.CreateRotationX(-hitEnemy[1].SpitTimer / 6) * game.Dalek.Transform;
                }

                if (hitEnemy[1] == game.Alien)
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie2bot01"].Index] = hitEnemy[1].EnemyModel.AbsoTransforms[hitEnemy[1].EnemyModel.Model.Bones["Bip01 L Hand"].Index] * game.Alien.Transform;
                    pies.AbsoTransforms[pies.Model.Bones["Pie2top01"].Index] = hitEnemy[1].EnemyModel.AbsoTransforms[hitEnemy[1].EnemyModel.Model.Bones["Bip01 L Hand"].Index] * game.Alien.Transform;
                }
            }
            else if (pieStates[1] == pieState.eaten)
            {
                pies.AbsoTransforms[pies.Model.Bones["Pie2bot01"].Index] = transform;
                pies.AbsoTransforms[pies.Model.Bones["Pie2top01"].Index] = transform;
            }

            ///////////////////////////////////
            //Pie 3

            if (pieStates[2] == pieState.loaded)
            {
                if (game.Player.WieldBazooka && !game.NeedPies)
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie3bot"].Index] = game.BazTransform;
                    pies.AbsoTransforms[pies.Model.Bones["Pie3top"].Index] = game.BazTransform;
                }
                else
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie3bot"].Index] = Matrix.CreateTranslation(0, 0, 0);
                    pies.AbsoTransforms[pies.Model.Bones["Pie3top"].Index] = Matrix.CreateTranslation(0, 0, 0);
                }
            }
            else if (pieStates[2] == pieState.firing)
            {
                direction = Vector3.TransformNormal(new Vector3(0, 0, 1), drawLoc[2]);
                pies.AbsoTransforms[pies.Model.Bones["Pie3bot"].Index].Translation += direction * moveRate * (float)delta;
                pies.AbsoTransforms[pies.Model.Bones["Pie3top"].Index].Translation += direction * moveRate * (float)delta;

                pie3Collision = pies.Model.Meshes[1].BoundingSphere;
                pie3Collision = pie3Collision.Transform(pies.AbsoTransforms[pies.Model.Bones["Pie3bot"].Index]);

                if (game.Dalek.EnemyCollision.TestForCollision(pie3Collision))
                {
                    game.Score += 15;
                    pieStates[2] = pieState.hitAlien;
                    hitEnemy[2] = game.Dalek;
                    game.Dalek.EatPie(this, 3);
                }

                if (game.Alien.EnemyCollision.TestForCollision(pie3Collision))
                {
                    game.Score += 15;
                    pieStates[2] = pieState.hitAlien;
                    hitEnemy[2] = game.Alien;
                    game.Alien.EatPie(this, 3);
                }

            }
            else if (pieStates[2] == pieState.hitAlien)
            {
                if (hitEnemy[2] == game.Dalek)
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie3bot"].Index] = hitEnemy[2].EnemyModel.AbsoTransforms[hitEnemy[2].EnemyModel.Model.Bones["PlungerArm"].Index] * Matrix.CreateTranslation(0, 0, 80) * Matrix.CreateRotationX(-hitEnemy[2].SpitTimer / 6) * game.Dalek.Transform;
                    pies.AbsoTransforms[pies.Model.Bones["Pie3top"].Index] = hitEnemy[2].EnemyModel.AbsoTransforms[hitEnemy[2].EnemyModel.Model.Bones["PlungerArm"].Index] * Matrix.CreateTranslation(0, 0, 80) * Matrix.CreateRotationX(-hitEnemy[2].SpitTimer / 6) * game.Dalek.Transform;
                }

                if (hitEnemy[2] == game.Alien)
                {
                    pies.AbsoTransforms[pies.Model.Bones["Pie3bot"].Index] = hitEnemy[2].EnemyModel.AbsoTransforms[hitEnemy[2].EnemyModel.Model.Bones["Bip01 L Hand"].Index] * game.Alien.Transform;
                    pies.AbsoTransforms[pies.Model.Bones["Pie3top"].Index] = hitEnemy[2].EnemyModel.AbsoTransforms[hitEnemy[2].EnemyModel.Model.Bones["Bip01 L Hand"].Index] * game.Alien.Transform;
                }
            }
            else if (pieStates[2] == pieState.eaten)
            {
                pies.AbsoTransforms[pies.Model.Bones["Pie3bot"].Index] = transform;
                pies.AbsoTransforms[pies.Model.Bones["Pie3top"].Index] = transform;
            }

        }

        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
                pies.Draw(graphics, gameTime, transform);
        }

        public void LoadPiesInBazooka()
        {
            loaded = true;
        }

        public void FirePie()
        {
            piesFired++;
            if (game.TotalPiesFired < 10)
            {
                game.TotalPiesFired++;
            }

            if (game.TotalPiesFired < 10)
            {
                switch (piesFired)
                {

                    case 1:
                        pieStates[0] = pieState.firing;
                        drawLoc[0] = game.BazTransform;
                        break;

                    case 2:
                        pieStates[1] = pieState.firing;
                        drawLoc[1] = game.BazTransform;
                        break;

                    case 3:
                        pieStates[2] = pieState.firing;
                        drawLoc[2] = game.BazTransform;
                        break;

                    default:
                        //piesFired = 0;
                        break;
                }
            }
            else
            {
                game.NeedPies = true;
            }
        }

        public bool PieCheckReload()
        {
            bool reload = false;

            if (piesFired >= 3
                && (pieStates[0] == pieState.hitWall || pieStates[0] == pieState.hitAlien || pieStates[0] == pieState.eaten)
                && (pieStates[1] == pieState.hitWall || pieStates[1] == pieState.hitAlien || pieStates[1] == pieState.eaten)
                && (pieStates[2] == pieState.hitWall || pieStates[2] == pieState.hitAlien || pieStates[2] == pieState.eaten)
                && game.TotalPiesFired < 10)
            {
                reload = true;
            }

            return reload;
        }

        public void PieHitWall(int pie)
        {
            pie--;
            pieStates[pie] = pieState.hitWall;
        }
    }
}
