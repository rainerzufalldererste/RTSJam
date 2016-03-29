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

namespace RTSJam
{
    public static class Master
    {
        public static Camera camera = new Camera();

        public static Chunk[] loadedChunks;

        public const int chunknum = 28;

        public static Texture2D[] textures = new Texture2D[9];

        public static int[] stoneDropNum = { 60, 90, 120, 120, 150 };

        public static SpriteFont pixelFont;

        public static float calculateDepth(float YPosition)
        {
            return 0.5f + .01f * (camera.position.Y - YPosition) / (2 * camera.zoom.Y);
        }
        public static float angleOfVector(Vector2 v)
        {
            float ret = (float)Math.Atan((-v.Y) / (-v.X));

            if (v.X < 0)
            {
                ret -= (float)Math.PI;
            }

            return ret;
        }

        public static Vector2 VectorFromAngle(float angle)
        {
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public static void getCoordsInChunk(out int xobj, out int yobj, int chunk, int x, int y)
        {
            xobj = loadedChunks[chunk].boundaries.X - (x);
            yobj = loadedChunks[chunk].boundaries.Y - (y);
        }

        public static bool getCollisionExists(out int chunk, out int xobj, out int yobj, int x, int y)
        {
            Rectangle rect = new Rectangle(x - 1, y - 1, 3, 3);

            for (int ii = 0; ii < loadedChunks.Length; ii++)
            {
                if (loadedChunks[ii].boundaries.Intersects(rect))
                {
                    getCoordsInChunk(out xobj, out yobj, ii, x, y);

                    if (xobj >= 0 && xobj < chunknum && yobj >= 0 && yobj < chunknum)
                    {
                        chunk = ii;
                        return true;
                    }
                }
            }

                    chunk = -1;
            xobj = -1;
            yobj = -1;
            return false;
        }

        public static List<int> getChunk(int x, int y)
        {
            Rectangle rect = new Rectangle(x - 1, y - 1, 3, 3);
            List<int> ret = new List<int>();

            for (int ii = 0; ii < loadedChunks.Length; ii++)
            {
                if (loadedChunks[ii].boundaries.Intersects(rect))
                {
                    ret.Add(ii);
                }
            }

            return ret;
        }

        public static T Clamp<T>(T a, T min, T max) where T : IComparable<T>
        {
            if(a.CompareTo(min) < 0)
            {
                return min;
            }
            else if(a.CompareTo(max) > 0)
            {
                return max;
            }
            return a;
        }

        public static int upClamp(int val, int max)
        {
            if(val > max)
            {
                return max;
            }
            return val;
        }

        public static int downClamp(int val, int min)
        {
            if (val < min)
            {
                return min;
            }
            return val;
        }

        internal static void drop(EStoneType stoneType, Vector2 position)
        {
            // TODO: drop stuff
        }

        internal static void notify(string message, Vector2 position)
        {
            // TODO: OPTIONAL: notify player
        }
    }
}
