using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class Enemy
    {
        /// <summary>
        /// Game that uses this player
        /// </summary>
        protected PrisonGame game;

        /// <summary>
        /// Our animated model
        /// </summary>
        protected AnimatedModel enemy;
        public AnimatedModel EnemyModel { get { return enemy; } }

        /// <summary>
        /// The projectile the alien shoots
        /// </summary>
        protected Spit spit;
        public Spit Spit { get { return spit; } }

        /// <summary>
        /// The timer used to tell when to spit slime at the player
        /// </summary>
        protected float spitTimer = 0;
        public float SpitTimer { get { return spitTimer; } } 

        //
        // Player location information.  We keep a x/z location (y stays zero)
        // and an orientation (which way we are looking).
        //

        /// <summary>
        /// Player location in the prison. Only x/z are important. y still stay zero
        /// unless we add some flying or jumping behavior later on.
        /// </summary>
        protected Vector3 location = new Vector3(275, 0, 1053);

        /// <summary>
        /// The player orientation as a simple angle
        /// </summary>
        protected float orientation = 0;

        /// <summary>
        /// Tells us if this enemy is eating
        /// </summary>
        protected bool eating = false;

        protected Pies heldPie;

        /// <summary>
        /// The player transformation matrix. Places the player where they need to be.
        /// </summary>
        protected Matrix transform;
        public Matrix Transform { get { return transform; } }

        /// <summary>
        /// The direction the head is facing
        /// </summary>
        protected float facing;
        public float Facing { get { return facing; } }

        /// <summary>
        /// The player move rate in centimeters per second
        /// </summary>
        protected float moveRate = 150;

        /// <summary>
        /// the collision cylinder for the enemy
        /// </summary>
        protected BoundingCylinder enemyCollision;
        public BoundingCylinder EnemyCollision { get { return enemyCollision; } }

        protected Dictionary<string, List<Vector2>> regions = new Dictionary<string, List<Vector2>>();

        /// <summary>
        /// Set the value of transform to match the current location
        /// and orientation.
        /// </summary>
        protected void SetEnemyTransform()
        {
            transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;
        }

        protected string TestRegion(Vector3 v3)
        {
            // Convert to a 2D Point
            float x = v3.X;
            float y = v3.Z;

            foreach (KeyValuePair<string, List<Vector2>> region in regions)
            {
                //if (region.Key.StartsWith("W"))
                //    continue;

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

        public void EatPie(Pies pies, int pieNum)
        {
            eating = true;
            heldPie = pies;
            spitTimer = 0;
        }

    }
}
