using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class Spit
    {
        /// <summary>
        /// Game that uses this spit
        /// </summary>
        private PrisonGame game;

        /// <summary>
        /// The game's player
        /// </summary>
        private Enemy enemy;

        private BoundingSphere spitCollision;

        /// <summary>
        /// Our animated model
        /// </summary>
        private AnimatedModel spitModel;
        public AnimatedModel SpitModel { get { return spitModel; } }

        /// <summary>
        /// The spit transformation matrix. Places the spits where they need to be.
        /// </summary>
        private Matrix transform;
        public Matrix Transform { get { return transform; } }

        /// <summary>
        /// tells us if the spit is firing
        /// </summary>
        private bool firing = false;
        public bool Firing { get { return firing; } set { firing = value; } }

        /// <summary>
        /// The spit move rate in centimeters per second
        /// </summary>
        private float moveRate = 600;

        public Spit(PrisonGame game, Enemy npc)
        {
            this.game = game;
            enemy = npc;
            transform = enemy.Transform;
            spitModel = new AnimatedModel(game, "Spit");
        }

        public void LoadContent(ContentManager content)
        {
            spitModel.LoadContent(content);

            spitCollision = spitModel.Model.Meshes[0].BoundingSphere;
        }

        public void Update(GameTime gameTime)
        {
            double delta = gameTime.ElapsedGameTime.TotalSeconds;
            Vector3 translateVector = new Vector3((float)Math.Sin(enemy.Facing), 0, (float)Math.Cos(enemy.Facing));

            if (firing)
            {
                transform.Translation += translateVector * moveRate * (float)delta;
            }
            else
            {
                transform = enemy.Transform;
                transform *= Matrix.CreateTranslation(0, 130, 0);
            }

            spitCollision = spitModel.Model.Meshes[0].BoundingSphere;
            spitCollision = spitCollision.Transform(transform);

            if (game.Player.PlayerCollision.TestForCollision(spitCollision) && !game.Player.Crouch)
            {
                if (!game.Slimed)
                {
                    game.Score -= 50;
                    game.Slimed = true;
                }
            }
        }
    }
}
