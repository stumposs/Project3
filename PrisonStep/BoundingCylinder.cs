using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Linq;

namespace PrisonStep
{
    public class BoundingCylinder
    {
        /// <summary>
        /// The radius of our collision cylinder
        /// </summary>
        private float radius = 50;

        private Vector3 location;

        public BoundingCylinder(PrisonGame game, Vector3 location)
        {
            this.location = location;
        }

        public void Update(GameTime gameTime, Vector3 location)
        {
            this.location = location;
        }

        public bool TestForCollision(BoundingSphere sphere)
        {
            bool collision = false;
            float totalLength = (new Vector2(sphere.Center.X - location.X, sphere.Center.Z - location.Z)).Length();

            if (totalLength < radius + sphere.Radius)
            {
                collision = true;
            }

            return collision;
        }
    }
}
