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
        
        public static readonly int[] stoneDropNum = { 60, 90, 120, 120, 150 };

        public static SpriteFont pixelFont;
        public static Texture2D[] objectTextures = new Texture2D[9];
        public static Texture2D[] unitTextures = new Texture2D[8];
        public static Texture2D[] ressourceTextures = new Texture2D[11];
        public static Texture2D[] buildingTextures = new Texture2D[14];

        public static Texture2D pixel;

        public static readonly Vector2 scaler = new Vector2(1f / 30f, 1f / (30f * .66f));

        public static List<Ressource> ressources = new List<Ressource>();
        public static List<GBuilding> buildings = new List<GBuilding>();
        public static List<GUnit> units = new List<GUnit>();
        public static List<GTransport> transports = new List<GTransport>();

        public static float calculateDepth(float YPosition)
        {
            return 0.5f + .01f * (camera.AimPos.Y - YPosition) / (2 * camera.zoom.Y);
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
        public static void DrawLine(SpriteBatch batch, Vector2 point1, Vector2 point2, Color coly, float depth)
        {
            if (point1.X > point2.X)
            {
                Vector2 tmp = point2;
                point2 = point1;
                point1 = tmp;
            }

            Vector2 c = new Vector2(point1.X - point2.X, point1.Y - point2.Y);
            batch.Draw(pixel,
                new Rectangle((int)point1.X, (int)point1.Y, (int)Math.Sqrt((c.X * c.X + c.Y * c.Y)) + 1, 1),
                null, coly, (float)Math.Atan((point2.Y - point1.Y) / (point2.X - point1.X)),
                new Vector2(0f, 0.5f), SpriteEffects.None, depth);
        }

        public static void DrawLine(SpriteBatch batch, Vector2 point1, Vector2 point2, Color coly, float width, float depth)
        {
            if (point1.X > point2.X)
            {
                Vector2 tmp = point2;
                point2 = point1;
                point1 = tmp;
            }

            Vector2 c = new Vector2(point1.X - point2.X, point1.Y - point2.Y);
            batch.Draw(pixel,
                point1,
                null, coly, (float)Math.Atan((point2.Y - point1.Y) / (point2.X - point1.X)),
                new Vector2(0f, 0.5f), new Vector2((float)Math.Sqrt((c.X * c.X + c.Y * c.Y)) + .001f, width), SpriteEffects.None, depth);
        }

        public static void getCoordsInChunk(out int xobj, out int yobj, int chunk, int x, int y)
        {
            xobj = x - loadedChunks[chunk].boundaries.X;
            yobj = y - loadedChunks[chunk].boundaries.Y;
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

        internal static void replaceGObject(Vector2 pos, GObject gObject)
        {
            Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, 1, 1);

            for (int i = 0; i < Master.loadedChunks.Length; i++)
            {
                if (loadedChunks[i].boundaries.Intersects(rect))
                {
                    int xobj, yobj;

                    Master.getCoordsInChunk(out xobj, out yobj, i, rect.X, rect.Y);

                    loadedChunks[i].gobjects[xobj][yobj] = gObject;
                }
            }
        }

        internal static void notify(string message, Vector2 position)
        {
            // TODO: OPTIONAL: notify player
        }

        public static GObject getGObjAt(Vector2 pos)
        {
            Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, 1, 1);

            for (int i = 0; i < Master.loadedChunks.Length; i++)
            {
                if(loadedChunks[i].boundaries.Intersects(rect))
                {
                    int xobj, yobj;

                    Master.getCoordsInChunk(out xobj, out yobj, i, rect.X, rect.Y);

                    return loadedChunks[i].gobjects[xobj][yobj];
                }
            }

            return null;
        }

        public static void drop(Ressource res)
        {
            Master.ressources.Add(res);
            TransportHandler.placeOffer(res.type, new TransportRessourceHandle(res, res.position));
        }

        internal static void updateUnitsBuildingsTransporters(SpriteBatch batch)
        {
            for (int i = 0; i < Master.buildings.Count; i++)
            {
                buildings[i].update();
            }

            for (int i = 0; i < Master.units.Count; i++)
            {
                units[i].update();
                units[i].draw(batch);
            }

            for (int i = 0; i < Master.transports.Count; i++)
            {
                transports[i].update();
                transports[i].draw(batch);
            }

            for (int i = 0; i < ressources.Count; i++)
            {
                batch.Draw(ressourceTextures[(int)ressources[i].type], ressources[i].position, null, new Color(1f, 1f, 1f, 1f), 0f, new Vector2(5), new Vector2(.02f, .033f), SpriteEffects.None, Master.calculateDepth(ressources[i].position.Y + .5f));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="units"></param>
        /// <param name="position"></param>
        /// <param name="goDoSth">Mine or Attack</param>
        public static void sendUnitsTo(List<GUnit> units, Vector2 position, bool goDoSth)
        {
            for (int i = 0; i < units.Count; i++)
            {
                units[i].doAction(goDoSth ? EActionType.SelectRegion : EActionType.ClickPosition, position, goDoSth ? (Vector2?)position : null);
            }
        }
    }

    public struct Ressource
    {
        public ERessourceType type;
        public Vector2 position;

        public Ressource(ERessourceType t, Vector2 v) : this()
        {
            type = t;
            position = v;
        }
    }
}
