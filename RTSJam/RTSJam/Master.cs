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
        
        public static readonly int[] stoneDropNum = 
            {
                3 * 60, // stone0
                0,      // ground0
                3 * 90, // coal0
                3 * 120,// ice0
                3 * 120,// gold0
                3 * 90, // stone1
                3 * 120,// stone2
                3 * 960,// stone3
                3 * 150 // purpur
            };

        public static readonly int[] constructionTimes =
        {
            240 * 60,// bigwar
            45 * 60, // gold
            30 * 60, // iron
            240 * 60,// main
            45 * 60, // miner
            60 * 60, // plant
            120 * 60,// powerplant
            90 * 60, // purpur
            20 * 60, // pylon
            90 * 60, // smallwar
            30 * 60, // stonefilter
            120 * 60,// university
            30 * 60, // water
        };

        public static readonly int[][] constructionRessources =
        {
            new int[] 
            {
                100, // stone
                0, // coal
                0, // iron
                100, // ironbar
                0, // ice
                0, // water
                10, // food
                0, // gold
                50, // goldbar
                0, // rawpurpur
                25, // purpur

            },// bigwar


            new int[]
            {
                25, // stone
                0, // coal
                0, // iron
                25, // ironbar
                0, // ice
                0, // water
                0, // food
                0, // gold
                0, // goldbar
                0, // rawpurpur
                0, // purpur

            }, // gold


            new int[]
            {
                10, // stone
                0, // coal
                0, // iron
                0, // ironbar
                0, // ice
                0, // water
                0, // food
                0, // gold
                0, // goldbar
                0, // rawpurpur
                0, // purpur

            }, // iron


            new int[]
            {
                25, // stone
                0, // coal
                0, // iron
                25, // ironbar
                0, // ice
                0, // water
                25, // food
                0, // gold
                50, // goldbar
                0, // rawpurpur
                25, // purpur

            },// main


            new int[]
            {
                15, // stone
                0, // coal
                0, // iron
                0, // ironbar
                0, // ice
                0, // water
                0, // food
                0, // gold
                0, // goldbar
                0, // rawpurpur
                0, // purpur

            }, // miner


            new int[]
            {
                20, // stone
                0, // coal
                0, // iron
                10, // ironbar
                0, // ice
                0, // water
                0, // food
                0, // gold
                0, // goldbar
                0, // rawpurpur
                0, // purpur

            }, // plant


            new int[]
            {
                50, // stone
                0, // coal
                0, // iron
                25, // ironbar
                0, // ice
                0, // water
                0, // food
                0, // gold
                0, // goldbar
                0, // rawpurpur
                0, // purpur

            },// powerplant


            new int[]
            {
                25, // stone
                0, // coal
                0, // iron
                25, // ironbar
                0, // ice
                0, // water
                10, // food
                0, // gold
                25, // goldbar
                0, // rawpurpur
                0, // purpur

            }, // purpur


            new int[]
            {
                10, // stone
                0, // coal
                0, // iron
                5, // ironbar
                0, // ice
                0, // water
                0, // food
                0, // gold
                0, // goldbar
                0, // rawpurpur
                0, // purpur

            }, // pylon


            new int[]
            {
                50, // stone
                0, // coal
                0, // iron
                25, // ironbar
                0, // ice
                0, // water
                0, // food
                0, // gold
                10, // goldbar
                0, // rawpurpur
                0, // purpur

            }, // smallwar


            new int[]
            {
                10, // stone
                0, // coal
                0, // iron
                0, // ironbar
                0, // ice
                0, // water
                0, // food
                0, // gold
                0, // goldbar
                0, // rawpurpur
                0, // purpur

            }, // stonefilter


            new int[]
            {
                50, // stone
                0, // coal
                0, // iron
                50, // ironbar
                0, // ice
                0, // water
                25, // food
                0, // gold
                25, // goldbar
                0, // rawpurpur
                25, // purpur

            },// university


            new int[]
            {
                20, // stone
                0, // coal
                0, // iron
                5, // ironbar
                0, // ice
                0, // water
                0, // food
                0, // gold
                0, // goldbar
                0, // rawpurpur
                0, // purpur

            }, // water
        };

        public static SpriteFont pixelFont;
        public static Texture2D[] objectTextures = new Texture2D[9];
        public static Texture2D[] unitTextures = new Texture2D[8];
        public static Texture2D[] ressourceTextures = new Texture2D[11];
        public static Texture2D[] buildingTextures = new Texture2D[16];
        public static Texture2D[] fxTextures = new Texture2D[8];
        public static Effect lightEffect;

        public static Texture2D pixel;

        public static readonly Vector2 scaler = new Vector2(1f / 30f, 1f / (30f * .66f));

        public static List<Ressource> ressources = new List<Ressource>();
        public static List<GBuilding> buildings = new List<GBuilding>();
        public static List<GUnit> units = new List<GUnit>();
        public static List<GTransport> transports = new List<GTransport>();

        public static Random rand = new Random();

        public static ETechnology DevelopedTechnologies = ETechnology.None;
        public static ETechnology discoveryStarted = ETechnology.None;

        public const float powerRange = 4.5f;
        public const float TwoPI = (float)(Math.PI * 2d);
        private static bool drawRessources = false;

        public static readonly Color transparentColor = new Color(.45f,.45f,.45f,.45f);

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
        internal static GObject getGObjAt(Vector2 pos, out int chunk, out int xobj, out int yobj)
        {
            Rectangle rect = new Rectangle((int)pos.X, (int)pos.Y, 1, 1);

            for (int i = 0; i < Master.loadedChunks.Length; i++)
            {
                if (loadedChunks[i].boundaries.Intersects(rect))
                {
                    int xobj_, yobj_;

                    Master.getCoordsInChunk(out xobj_, out yobj_, i, rect.X, rect.Y);

                    chunk = i;
                    xobj = xobj_;
                    yobj = yobj_;

                    return loadedChunks[i].gobjects[xobj_][yobj_];
                }
            }
            chunk = -1;
            xobj = -1;
            yobj = -1;
            return null;
        }

        public static void drop(Ressource res)
        {
            Master.ressources.Add(res);
            TransportHandler.placeOffer(res.type, new TransportRessourceHandle(res, res.position));
        }

        internal static void updateUnitsBuildingsTransporters(SpriteBatch batch)
        {
            Rectangle dispRect = new Rectangle(
                (int)(Math.Round(Master.camera.currentPos.X - 1 * MainGame.width / (Master.camera.zoom.X * 2) - 2)),
                (int)(Math.Round(Master.camera.currentPos.Y - 1 * MainGame.height / (Master.camera.zoom.Y * 2)) - 2),
                (int)(Math.Round(1 * MainGame.width / (Master.camera.zoom.X) + 4)),
                (int)(Math.Round(1 * MainGame.height / (Master.camera.zoom.Y)) + 4));

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

                /*if ((transports[i].position.X > dispRect.X || transports[i].position.X < dispRect.X + dispRect.Width) &&
                      (transports[i].position.Y > dispRect.Y || transports[i].position.Y < dispRect.Y + dispRect.Height))*/
                {
                    transports[i].draw(batch);
                }
            }

            if (drawRessources)
            {
                for (int i = 0; i < ressources.Count; i++)
                {
                    batch.Draw(ressourceTextures[(int)ressources[i].type], ressources[i].position, null, new Color(1f, 1f, 1f, 1f), 0f, new Vector2(5), new Vector2(.02f, .033f), SpriteEffects.None, Master.calculateDepth(ressources[i].position.Y + .5f));
                }
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

        internal static void addTransport(GTransport gTransport)
        {
            transports.Add(gTransport);
        }

        internal static void addUnit(GMiner gMiner)
        {
            units.Add(gMiner);
        }

        internal static void addOffer(Ressource res)
        {
            TransportHandler.placeOffer(res.type, new TransportRessourceHandle(res, res.position));
            ressources.Add(res);
        }

        internal static void AddBuilding(GBuilding gb, int i, int xobj, int yobj, bool sizeIsTwo)
        {
            GObjBuild gobj = new GObjBuild(gb, sizeIsTwo ? new List<GObject>() {
                                Master.getGObjAt(new Vector2(xobj, yobj + 1)),
                                Master.getGObjAt(new Vector2(xobj + 1, yobj)),
                                Master.getGObjAt(new Vector2(xobj + 1, yobj + 1))
                            } : new List<GObject>());

            GBuilding constr = new BUnderConstruction(gb, gobj, Master.constructionTimes[(int)gb.type], Master.constructionRessources[(int)gb.type]);

            gobj.building = constr;

            buildings.Add(constr);

            loadedChunks[i].gobjects[xobj][yobj] = gobj;
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

    [Flags]
    public enum ETechnology
    {
        None = 0,
        Softminer = 1,
        BiggerFighter = 2,
        PurPurPurifier = 4,
        BigWarStation = 8,
        BigCanonTank = 0x10
    }
}
