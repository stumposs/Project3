using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class Ground
    {
        /// <summary>
        /// Current position
        /// </summary>
        private Vector3 position = Vector3.Zero;
        private PrisonGame game;
        private int scale;
        private Model model;

        public Vector3 Position { get { return position; } set { position = value; } }
        public int Scale { get { return scale; } set { scale = value; } }

        public Ground(PrisonGame inGame)
        {
            game = inGame;
        }

        public void Initialize()
        {
        }

        public void Update(GameTime gameTime) { }

        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Camera inCamera)
        {
            DrawModel(graphics, model, Matrix.CreateTranslation(position), gameTime, inCamera);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world, GameTime gameTime, Camera inCamera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    effect.View = inCamera.View;
                    effect.Projection = inCamera.Projection;
                }
                mesh.Draw();
            }

        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("ground");
        }


    }
}