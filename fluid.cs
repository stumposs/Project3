using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
/* This class was modeled after Owen's AnimatedMesh class
* the the Draw function still uses his normals not the normals
* and tangents calculated. 
*
* TODO: add more specular lighting, remove dead code (I didn't delete
* because I wanted to see something react
*/
namespace PrisonStep
{
    public class Fluid
    {
        private PrisonGame game;



        private Effect effect;
        //TODO: numPerDimension set according to flooded area
        // flooded area MUST be a rectangle (right now)
        private const int numPerDimension = 100;
        
        // default values
        private const float xMin = -100;
        private const float xMax = 100;
        private const float zMin = -100;
        private const float zMax = 100;
        private const float y = 5;

        private VertexPositionNormalTexture[] vertices;
        // vectors for the previous and current positions of the vertices
        // since the x and z locations remain constant, we need only y value
        private float[] vertexYPositions1;
        private float[] vertexYPositions2;
        private Vector3[] tangents;
        private Vector3[] normals;
        private float[][] positionsBuffer = new float[2][];
      
   
        private int renderBuffer = 0;
        int width;
        int height;
        //distance between vertices
        private float distance=0;
        private float k1, k2, k3;
        private float timeStep = 0.016f, waveSpeed = 0.5f, viscosity = 1;
        private int[] indices;
        // constrain to rectangular region for now to simplify things
        private float[] region = null;
        public float[] Region { get { return region; } set { region = value; } }
        private const float TOL = 2f;

        public Fluid(PrisonGame game)
        {
            this.game = game;
            height = numPerDimension;
            width = numPerDimension;
        }
        public void LoadContent(ContentManager Content)
        {
            effect = Content.Load<Effect>("VertexLights");
            effect.Parameters["Light1Location"].SetValue(new Vector3(100, 100, 100));
            CreateMesh();
        }
        public void SetConstants(float speed, float viscosity, float time,float spaceStep)
        {
            timeStep = time;
            waveSpeed = speed;
            this.viscosity = viscosity;

            if (distance != 0)
                spaceStep = distance;
            // protect from divergence!
            if(speed<0)
            {
                speed = -speed;
            }
            float limit = (float)Math.Sqrt(viscosity*time+2)*distance/(2*time);
            if(speed>limit)
            {
                speed -= (speed - limit - 0.01f);
            }
            float f1 = time * time * speed * speed / (spaceStep * spaceStep);
            float f2 = 1f/ (viscosity * time + 2);
            k1 = (4f - 8f * f1) *f2;
            k2 = (viscosity * time - 2f) * f2;
            k3 = 2f * f1 * f2;
        }
        private void CreateMesh()
        {
            //
            // Create the vertices (initialize location; normals, tangents)
            //
         
            vertices = new VertexPositionNormalTexture[height*width];
            normals = new Vector3[height * width];
            tangents = new Vector3[height * width];

            vertexYPositions1 = new float[height * width];
            vertexYPositions2 = new float[height * width];
            
            positionsBuffer[0] = vertexYPositions1;
            positionsBuffer[1] = vertexYPositions2;
            
            for (int i = 0; i < numPerDimension; i++)
            {
            
            //if a region was not specified, set to default values
                if (region == null)
                {
                    region = new float[4];

                    region[0] = zMin;
                    region[1] = zMax;
                    region[2] = xMin;
                    region[3] = xMax;

                }
                float z = region[0] + (float)(i) / (float)(numPerDimension - 1) * (region[1] - region[0]);

                for (int j = 0; j < numPerDimension; j++)
                {
                    float x = region[2] + (float)(j) / (float)(numPerDimension - 1) * (region[3] - region[2]);

                    vertices[i * numPerDimension + j].Position = new Vector3(x, y, -z);
                    vertexYPositions1[i * numPerDimension + j] = y;
                    vertexYPositions2[i * numPerDimension + j] = y;
                    vertices[i * numPerDimension + j].Normal = new Vector3(0, 1, 0);
                    
                }


            }
            
            //get distance between vertices
            //-- should be uniform so doesn't matter which vertices we use
            float diffX = vertices[0].Position.X - vertices[numPerDimension + 1].Position.X;
            float diffZ = vertices[0].Position.Z - vertices[numPerDimension + 1].Position.Z;
            distance = (float)Math.Sqrt(diffX * diffX + diffZ * diffZ);
            
            // set normals 
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    normals[i * height + j] = new Vector3(0, 2 * distance, 0);
                    tangents[i * height + j] = new Vector3(2 * distance, 0, 0);
                }
            }
            //
            // Create the indices
            //

            indices = new int[(numPerDimension - 1) * (numPerDimension - 1) * 6];

            int k = 0;
            for (int i = 0; i < numPerDimension - 1; i++)
            {

                for (int j = 0; j < numPerDimension - 1; j++)
                {
                    int v1 = i * numPerDimension + j;
                    int v2 = i * numPerDimension + j + 1;
                    int v3 = (i + 1) * numPerDimension + j + 1;
                    int v4 = (i + 1) * numPerDimension + j;

                    indices[k++] = v1;
                    indices[k++] = v3;
                    indices[k++] = v2;

                    indices[k++] = v1;
                    indices[k++] = v4;
                    indices[k++] = v3;
                }
            }

        }

        KeyboardState lastKeyboard;

        public void Initialize()
        {

            lastKeyboard = Keyboard.GetState();
        }

        double time = 0;
        const float period = 20;
        const float speed = 20;
        const float amplitude = 5;

        public void Update(GameTime gameTime)
        {

            time += gameTime.ElapsedGameTime.TotalSeconds;
            Evaluate();

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Vector3 v = vertices[i * numPerDimension + j].Position;
                    v.Y = 0;
                    float distanceFromCenter = (v - Vector3.Zero).Length();

                    if (distanceFromCenter < 100)
                    {
                    //    v.Y = y + amplitude * (1 - distanceFromCenter / 100) * (float)Math.Sin(distanceFromCenter / period * Math.PI * 2 - time * speed / period * Math.PI * 2);
                        v.Y = positionsBuffer[renderBuffer][i * height + j];
                        vertices[i * numPerDimension + j].Position = v;
                    }

                }
            }

            ComputeNormals();
        }
        private void Evaluate()
        {
            //current location: z(i,j,k)
            float[] current = positionsBuffer[renderBuffer];
            //previous location: z(i,j,k-1)
            float[] prev = positionsBuffer[1 - renderBuffer];

            for(int i=1;i<height-1;i++)
            {
                for(int j=1;j<width-1;j++)
                {
       
                    float term3 = current[(i + 1) * height + j] + current[(i - 1) * height + j] + current[(i * height + j + 1)] + current[(i * height + j - 1)];

                    //prev will be current, so that's the one we update
                    prev[i * height + j] = k1 * current[i * height + j] + k2 * prev[i * height + j] + k3 * term3;

                }
           }
            //swap buffers
            renderBuffer = 1 - renderBuffer;

            float[] next = positionsBuffer[renderBuffer];

            //calculate normals and tangents
            for (int i = 1; i < height-1; i++)
            {
                for (int j = 1; j < width-1; j++)
                {
                    normals[i * height + j].X = next[(i-1) * height + j] - next[(i + 1) * height + j];
                    normals[i * height + j].Z = next[i * height + j - 1] - next[i * height + j + 1];
                    tangents[i * height + j].Y = next[(i + 1) * height + j] - next[(i - 1) * height + j];
                }
            }
        }
        /* manually displace affected vertices
        * (affected vertices are vertices within 'TOL' distance
        * on XZ plane of the disturbance
        *
        */
        public void Disturb(Vector3 point)
        {
            Vector2 p = new Vector2(point.X, point.Z);
            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    Vector2 v = new Vector2(vertices[i * height + j].Position.X, vertices[i * height + j].Position.Z);
                    float d = (float)Math.Sqrt((v.X-p.X)*(v.X-p.X)+(v.Y-p.Y)*(v.Y-p.Y));
                    
                    if (d < TOL)
                    {
                        positionsBuffer[renderBuffer][i * height + j] -= 1;
                    }
                }
            }
        }


        private void ComputeNormals()
        {

            for (int i = 0; i < numPerDimension; i++)
            {
                for (int j = 0; j < numPerDimension; j++)
                {
                    Vector3 below = i > 0 ? vertices[(i - 1) * numPerDimension + j].Position : vertices[i * numPerDimension + j].Position;
                    Vector3 above = i < numPerDimension - 1 ? vertices[(i + 1) * numPerDimension + j].Position : vertices[i * numPerDimension + j].Position;

                    Vector3 left = j > 0 ? vertices[i * numPerDimension + j - 1].Position : vertices[i * numPerDimension + j].Position;
                    Vector3 right = j < numPerDimension - 1 ? vertices[i * numPerDimension + j + 1].Position : vertices[i * numPerDimension + j].Position;

                    vertices[i * numPerDimension + j].Normal = Vector3.Normalize(Vector3.Cross(right - left, above - below));

                }
            }
        }

        

        public void Draw(GraphicsDevice graphics)
        {
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(game.Camera.View);
            effect.Parameters["Projection"].SetValue(game.Camera.Projection);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3);

            }

        }
    }
}
