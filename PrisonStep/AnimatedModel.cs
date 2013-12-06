using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XnaAux;

namespace PrisonStep
{
    public class AnimatedModel
    {
        /// <summary>
        /// Reference to the game that uses this class
        /// </summary>
        private PrisonGame game;

        /// <summary>
        /// The XNA model we will be animating
        /// </summary>
        private Model model;

        public Model Model { get { return model; } }

        /// <summary>
        /// The effect to use on the objects and player
        /// </summary>
        private Effect objectEffect;
        public Effect ObjectEffect { get { return objectEffect; } set { objectEffect = value; } }

        /// <summary>
        /// The number of skinning matrices in SkinnedEffect.fx. This must
        /// match the number in SkinnedEffect.fx.
        /// </summary>
        public const int NumSkinBones = 57;


        private Matrix[] bindTransforms;
        private Matrix[] boneTransforms;
        private Matrix[] absoTransforms;
        private Matrix[] skinTransforms = null;
        private List<int> skelToBone = null;
        private Matrix[] inverseBindTransforms = null;

        private Matrix rootMatrixRaw = Matrix.Identity;
        private Matrix deltaMatrix = Matrix.Identity;

        public Matrix DeltaMatrix { get { return deltaMatrix; } }
        public Vector3 DeltaPosition;
        public Matrix RootMatrix { get { return inverseBindTransforms[skelToBone[0]] * rootMatrixRaw; } }

        public Matrix[] AbsoTransforms { get { return absoTransforms; } }
        public Matrix[] BoneTransforms { get { return boneTransforms; } }
        public Matrix[] BindTransforms { get { return bindTransforms; } }

        /// <summary>
        /// Name of the asset we are going to load
        /// </summary>
        private string asset;

        /// <summary>
        /// Access the current animation player
        /// </summary>
        public AnimationPlayer Player { get { return player; } }

        /// <summary>
        /// This class describes a single animation clip we load from
        /// an asset.
        /// </summary>
        private class AssetClip
        {
            public AssetClip(string name, string asset)
            {
                Name = name;
                Asset = asset;
                TheClip = null;
            }

            public string Name { get; set; }
            public string Asset { get; set; }
            public AnimationClips.Clip TheClip { get; set; }
        }

        /// <summary>
        /// A dictionary that allows us to look up animation clips
        /// by name. 
        /// </summary>
        private Dictionary<string, AssetClip> assetClips = new Dictionary<string, AssetClip>();


        public AnimatedModel(PrisonGame game, string asset)
        {
            this.game = game;
            this.asset = asset;

            skinTransforms = new Matrix[57];
            for (int i = 0; i < skinTransforms.Length; i++)
            {
                skinTransforms[i] = Matrix.Identity;
            }
        }


        /// <summary>
        /// This function is called to load content into this component
        /// of our game.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>(asset);

            int boneCnt = model.Bones.Count;
            bindTransforms = new Matrix[boneCnt];
            boneTransforms = new Matrix[boneCnt];
            absoTransforms = new Matrix[boneCnt];

            model.CopyBoneTransformsTo(bindTransforms);
            model.CopyBoneTransformsTo(boneTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);

            AnimationClips clips = model.Tag as AnimationClips;
            if (clips != null && clips.SkelToBone.Count > 0)
            {
                skelToBone = clips.SkelToBone;

                inverseBindTransforms = new Matrix[boneCnt];
                skinTransforms = new Matrix[NumSkinBones];

                model.CopyAbsoluteBoneTransformsTo(inverseBindTransforms);

                for (int b = 0; b < inverseBindTransforms.Length; b++)
                    inverseBindTransforms[b] = Matrix.Invert(inverseBindTransforms[b]);

                for (int i = 0; i < skinTransforms.Length; i++)
                    skinTransforms[i] = Matrix.Identity;
            }

            foreach (AssetClip clip in assetClips.Values)
            {
                Model clipmodel = content.Load<Model>(clip.Asset);
                AnimationClips modelclips = clipmodel.Tag as AnimationClips;
                clip.TheClip = modelclips.Clips["Take 001"];
            }
        }

        /// <summary>
        /// This function is called to update this component of our game
        /// to the current game time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(double delta)
        {
            //double delta = gameTime.ElapsedGameTime.TotalSeconds;

            if (player != null)
            {
                // Update the clip
                player.Update(delta);

                for (int b = 0; b < player.BoneCount; b++)
                {
                    AnimationPlayer.Bone bone = player.GetBone(b);
                    if (!bone.Valid)
                        continue;

                    Vector3 scale = new Vector3(bindTransforms[b].Right.Length(),
                        bindTransforms[b].Up.Length(),
                        bindTransforms[b].Backward.Length());

                    boneTransforms[b] = Matrix.CreateScale(scale) *
                        Matrix.CreateFromQuaternion(bone.Rotation) *
                        Matrix.CreateTranslation(bone.Translation);
                }

                if (skelToBone != null)
                {
                    int rootBone = skelToBone[0];

                    deltaMatrix = Matrix.Invert(rootMatrixRaw) * boneTransforms[rootBone];
                    DeltaPosition = boneTransforms[rootBone].Translation - rootMatrixRaw.Translation;

                    rootMatrixRaw = boneTransforms[rootBone];

                    boneTransforms[rootBone] = bindTransforms[rootBone];
                }

                if (skelToBone != null)
                {
                    int rootBone = skelToBone[0];

                    boneTransforms[rootBone] = bindTransforms[rootBone];
                }

                model.CopyBoneTransformsFrom(boneTransforms);
            }

            //if (game.Player.Aiming)
            //{
            //    boneTransforms[6] = Matrix.CreateRotationX(game.Player.AimOrient - game.Player.AimAngle) * bindTransforms[6];
            //}
            game.Player.PlayerAim();

            model.CopyBoneTransformsFrom(boneTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);
        }

        private AnimationPlayer player = null;

        /// <summary>
        /// Play an animation clip on this model.
        /// </summary>
        /// <param name="name"></param>
        public AnimationPlayer PlayClip(string name)
        {
            player = null;

            if (name != "Take 001")
            {
                player = new AnimationPlayer(this, assetClips[name].TheClip);
                Update(0);
                return player;
            }

            AnimationClips clips = model.Tag as AnimationClips;
            if (clips != null)
            {
                player = new AnimationPlayer(this, clips.Clips[name]);
                Update(0);
            }

            return player;
        }

        /// <summary>
        /// This function is called to draw this game component.
        /// </summary>
        /// <param name="graphics">Device to draw the model on.</param>
        /// <param name="gameTime">Current game time.</param>
        /// <param name="transform">Transform that puts the model where we want it.</param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix transform)
        {
            DrawModel(graphics, model, transform);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world)
        {
            if (skelToBone != null)
            {
                for (int b = 0; b < skelToBone.Count; b++)
                {
                    int n = skelToBone[b];
                    skinTransforms[b] = inverseBindTransforms[n] * absoTransforms[n];
                }
            }

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    Matrix temp = absoTransforms[mesh.ParentBone.Index] * world;
                    effect.Parameters["World"].SetValue(temp);
                    effect.Parameters["View"].SetValue(game.Camera.View);
                    effect.Parameters["Projection"].SetValue(game.Camera.Projection);

                    if (skelToBone != null)
                    {
                        effect.Parameters["Bones"].SetValue(skinTransforms);
                    } 
                }
                mesh.Draw();
            }

        }

        /// <summary>
        /// Add an asset clip to the dictionary.
        /// </summary>
        /// <param name="name">Name we will use for the clip</param>
        /// <param name="asset">The FBX asset to load</param>
        public void AddAssetClip(string name, string asset)
        {
            assetClips[name] = new AssetClip(name, asset);
        }

        /// <summary>
        /// Replace the model effect with a new effect we load ourselves
        /// </summary>
        public void SetEffect()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    //BasicEffect bEffect = part.Effect as BasicEffect;
                    Vector3 diff = part.Effect.Parameters["DiffuseColor"].GetValueVector3();
                    part.Effect = objectEffect.Clone();

                    Matrix temp = Matrix.CreateTranslation(game.Camera.Center);

                    part.Effect.Parameters["DiffuseColor"].SetValue(diff);
                    part.Effect.Parameters["World"].SetValue(temp);
                    part.Effect.Parameters["View"].SetValue(game.Camera.View);
                    part.Effect.Parameters["Projection"].SetValue(game.Camera.Projection);
                    part.Effect.Parameters["Slime"].SetValue(game.SlimeLevel);
                }
            }
        }
    }
}
