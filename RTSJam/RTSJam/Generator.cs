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
using System.Threading;


namespace RTSJam
{
    public static class Generator
    {
        public static Random random;

        public static void generateWorld(int? seed, int scenario)
        {
            TransportHandler.stopTransportHandler();

            List<Chunk> chunks = new List<Chunk>();
            Master.buildings = new List<GBuilding>();
            Master.loadedChunks = null;
            Master.ressources = new List<Ressource>();
            Master.transports = new List<GTransport>();
            Master.units = new List<GUnit>();

            Vector2 possibleAiPos = new Vector2(0, (1.25f * Master.chunknum));

            if (seed.HasValue)
            {
                random = new Random(seed.Value);
            }
            else
            {
                random = new Random();
            }

            // Step 1: Place all Chunks

            for (int x = -Master.chunknum / 4; x < Master.chunknum / 4; x++)
            {
                for (int y = -Master.chunknum / 4; y < Master.chunknum / 4; y++)
                {

                    float x__ = x + .5f, y__ = y + .5f;

                    if (Math.Sqrt(x__ * x__ + y__ * y__) < Master.chunknum / 4)
                    {
                        chunks.Add(new Chunk(x, y));

                        for (int xx = 0; xx < Master.chunknum; xx++)
                        {
                            for (int yy = 0; yy < Master.chunknum; yy++)
                            {
                                float x_ = x * Master.chunknum + xx, y_ = y * Master.chunknum + yy;

                                double dd = Math.Sqrt(x_ * x_ + y_ * y_);

                                if (dd < Master.chunknum * 2f + random.NextDouble() * 20)
                                {
                                    chunks[chunks.Count - 1].gobjects[xx][yy] = new GStone() { position = new Microsoft.Xna.Framework.Vector2(x_, y_), texture = 0 };
                                }
                                else if (dd < Master.chunknum * 4 + random.NextDouble() * 22)
                                {
                                    chunks[chunks.Count - 1].gobjects[xx][yy] = new GStone() { position = new Microsoft.Xna.Framework.Vector2(x_, y_), texture = 5 };
                                    ((GStone)(chunks[chunks.Count - 1].gobjects[xx][yy])).maxhealth = 3000;
                                    ((GStone)(chunks[chunks.Count - 1].gobjects[xx][yy])).health = 3000;
                                }
                                else if (dd < Master.chunknum * 5.5f + random.NextDouble() * 20)
                                {
                                    chunks[chunks.Count - 1].gobjects[xx][yy] = new GStone() { position = new Microsoft.Xna.Framework.Vector2(x_, y_), texture = 6 };
                                    ((GStone)(chunks[chunks.Count - 1].gobjects[xx][yy])).maxhealth = 6000;
                                    ((GStone)(chunks[chunks.Count - 1].gobjects[xx][yy])).health = 6000;
                                }
                                else
                                {
                                    chunks[chunks.Count - 1].gobjects[xx][yy] = new GStone() { position = new Microsoft.Xna.Framework.Vector2(x_, y_), texture = 7 };
                                    ((GStone)(chunks[chunks.Count - 1].gobjects[xx][yy])).maxhealth = 15000;
                                    ((GStone)(chunks[chunks.Count - 1].gobjects[xx][yy])).health = 15000;
                                }
                            }
                        }
                    }
                }
            }

            // Step 2: Create Start Hole

            if (scenario == 0)
            {
                Rectangle middleCircle = new Rectangle(-Master.chunknum, -Master.chunknum, 2 * Master.chunknum, 2 * Master.chunknum);

                for (int i = 0; i < chunks.Count; i++)
                {
                    if (chunks[i].boundaries.Intersects(middleCircle))
                    {
                        for (int xx = 0; xx < Master.chunknum; xx++)
                        {
                            for (int yy = 0; yy < Master.chunknum; yy++)
                            {
                                //if (chunks[i].gobjects[xx][yy] == null) continue;

                                Vector2 v = chunks[i].gobjects[xx][yy].position;
                                double d = v.Length();

                                if (d < (double)(Master.chunknum / 2f))
                                {
                                    chunks[i].gobjects[xx][yy] = new GGround() { position = chunks[i].gobjects[xx][yy].position };
                                }
                            }
                        }
                    }
                }
            }
            else if (scenario == 1)
            {
                Rectangle middleCircle = new Rectangle(-Master.chunknum, -(int)(1.25f * Master.chunknum) - Master.chunknum, 2 * Master.chunknum, 2 * Master.chunknum);

                for (int i = 0; i < chunks.Count; i++)
                {
                    if (chunks[i].boundaries.Intersects(middleCircle))
                    {
                        for (int xx = 0; xx < Master.chunknum; xx++)
                        {
                            for (int yy = 0; yy < Master.chunknum; yy++)
                            {
                                //if (chunks[i].gobjects[xx][yy] == null) continue;

                                Vector2 v = chunks[i].gobjects[xx][yy].position;
                                double d = (v - new Vector2(middleCircle.X + middleCircle.Width / 2, middleCircle.Y + middleCircle.Height / 2)).Length();

                                if (d < (double)(Master.chunknum / 2f))
                                {
                                    chunks[i].gobjects[xx][yy] = new GGround() { position = chunks[i].gobjects[xx][yy].position };
                                }
                            }
                        }
                    }
                }


                middleCircle = new Rectangle(-Master.chunknum, (int)(1.25f * Master.chunknum) - Master.chunknum, 2 * Master.chunknum, 2 * Master.chunknum);

                for (int i = 0; i < chunks.Count; i++)
                {
                    if (chunks[i].boundaries.Intersects(middleCircle))
                    {
                        for (int xx = 0; xx < Master.chunknum; xx++)
                        {
                            for (int yy = 0; yy < Master.chunknum; yy++)
                            {
                                //if (chunks[i].gobjects[xx][yy] == null) continue;

                                Vector2 v = chunks[i].gobjects[xx][yy].position;
                                double d = (v - new Vector2(middleCircle.X + middleCircle.Width / 2, middleCircle.Y + middleCircle.Height / 2)).Length();

                                if (d < (double)(Master.chunknum / 2f))
                                {
                                    chunks[i].gobjects[xx][yy] = new GGround() { position = chunks[i].gobjects[xx][yy].position };
                                }
                            }
                        }
                    }
                }
            }


            // Step 3: Place Minerals
            {
                int size = (int)(random.NextDouble() * 20d + 130d);
                Vector2[] positions = new Vector2[size];

                for (int i = 0; i < size; i++)
                {
                    positions[i] = Master.VectorFromAngle((float)(random.NextDouble() * (Master.TwoPI)))
                        * (float)(random.NextDouble() * (Master.chunknum * 12) - Master.chunknum * 6f);
                }

                for (int i = 0; i < size; i++)
                {
                    int length = (int)(random.NextDouble() * 15 + 35);

                    Rectangle stoneRect = new Rectangle((int)positions[i].X - length * 8, (int)positions[i].Y - length * 2, length * 16, length * 4);

                    for (int ii = 0; ii < chunks.Count; ii++)
                    {
                        if (chunks[ii].boundaries.Intersects(stoneRect))
                        {
                            for (int x = 0; x < length; x++)
                            {
                                for (int y = -(int)(x > length / 2 ? length - x : x) / 5; y < (int)(x > length / 2 ? length - x : x) / 5; y++)
                                {
                                    /*int xobj = chunks[ii].boundaries.X - ((int)positions[i].X) + x,
                                        yobj = chunks[ii].boundaries.Y - ((int)positions[i].Y) + y;*/

                                    int xobj = (((int)positions[i].X) + x) - chunks[ii].boundaries.X,
                                    yobj = (((int)positions[i].Y) + y) - chunks[ii].boundaries.Y;

                                    if (xobj >= 0 && xobj < Master.chunknum && yobj >= 0 && yobj < Master.chunknum && chunks[ii].gobjects[xobj][yobj] is GStone)
                                    {
                                        chunks[ii].gobjects[xobj][yobj].texture = 2;
                                        chunks[ii].gobjects[xobj][yobj].position = new Vector2(chunks[ii].boundaries.X + xobj, chunks[ii].boundaries.Y + yobj);
                                        ((GStone)(chunks[ii].gobjects[xobj][yobj])).stoneType = ERessourceType.Coal;
                                        ((GStone)(chunks[ii].gobjects[xobj][yobj])).maxhealth = 2500;
                                        ((GStone)(chunks[ii].gobjects[xobj][yobj])).health = 2500;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Step 4: Place Rare Minerals
            {
                int size = (int)(random.NextDouble() * 20d + 180d);
                Vector2[] positions = new Vector2[size];
                float[] length = new float[size];

                for (int i = 0; i < size; i++)
                {
                    positions[i] = new Vector2((float)(random.NextDouble() * (Master.chunknum * 10) - Master.chunknum * 5f),
                        (float)(random.NextDouble() * (Master.chunknum * 6) - Master.chunknum * 3f));

                    length[i] = (float)(random.NextDouble() * 1.5f + 1);
                }

                for (int i = 0; i < size; i++)
                {
                    int tex = 3;

                    if (random.NextDouble() < .35f)
                    {
                        tex = 4;
                    }

                    for (int ii = 0; ii < chunks.Count; ii++)
                    {
                        if (chunks[ii].boundaries.Intersects(new Rectangle((int)positions[i].X - (int)length[i] - 6, (int)positions[i].Y - (int)length[i] - 6, (int)length[i] * (int)2 + 12, (int)length[i] * (int)2 + 12)))
                        {
                            for (int xx = 0; xx < Master.chunknum; xx++)
                            {
                                for (int yy = 0; yy < Master.chunknum; yy++)
                                {
                                    if (!(chunks[ii].gobjects[xx][yy] is GStone)) continue;

                                    if (scenario != 0 && (chunks[ii].gobjects[xx][yy].position - possibleAiPos).Length() < 4 * Master.chunknum)
                                        CheapAI.OrePositions.Add(chunks[ii].gobjects[xx][yy].position);

                                    Vector2 v = chunks[ii].gobjects[xx][yy].position;

                                    v -= positions[i];

                                    double d = Math.Sqrt((v.X * v.X + v.Y * v.Y));

                                    if (d < (double)(length[i]) + (float)(random.NextDouble() * 3))
                                    {
                                        chunks[ii].gobjects[xx][yy].texture = tex;
                                        chunks[ii].gobjects[xx][yy].position = new Vector2(chunks[ii].boundaries.X + xx, chunks[ii].boundaries.Y + yy);
                                        ((GStone)(chunks[ii].gobjects[xx][yy])).stoneType = tex == 3 ? ERessourceType.Ice : ERessourceType.Gold;
                                        ((GStone)(chunks[ii].gobjects[xx][yy])).maxhealth = 3000;
                                        ((GStone)(chunks[ii].gobjects[xx][yy])).health = 3000;
                                    }
                                }
                            }
                        }
                    }
                }

                size = (int)(random.NextDouble() * 10 + 80);
                positions = new Vector2[size];
                length = new float[size];

                for (int i = 0; i < size; i++)
                {
                    positions[i] = Master.VectorFromAngle((float)(random.NextDouble() * (float)(Master.chunknum * 2)))
                        * (float)(random.NextDouble() * (Master.chunknum * 6) - Master.chunknum * 3f);

                    length[i] = (float)(random.NextDouble() * 1.5f + 2);
                }

                for (int i = 0; i < size; i++)
                {

                    for (int ii = 0; ii < chunks.Count; ii++)
                    {
                        if (chunks[ii].boundaries.Intersects(new Rectangle((int)positions[i].X - (int)length[i] - 6, (int)positions[i].Y - (int)length[i] - 6, (int)length[i] * (int)2 + 12, (int)length[i] * (int)2 + 12)))
                        {
                            for (int xx = 0; xx < Master.chunknum; xx++)
                            {
                                for (int yy = 0; yy < Master.chunknum; yy++)
                                {
                                    if (!(chunks[ii].gobjects[xx][yy] is GStone)) continue;

                                    if (scenario != 0 && (chunks[ii].gobjects[xx][yy].position - possibleAiPos).Length() < 4 * Master.chunknum)
                                        CheapAI.OrePositions.Add(chunks[ii].gobjects[xx][yy].position);

                                    float xxx = chunks[i].boundaries.X + xx, yyy = chunks[i].boundaries.X + xx;

                                    if (Math.Sqrt(xxx * xxx + yyy * yyy) < Master.chunknum * 3.5f) continue;

                                    Vector2 v = chunks[ii].gobjects[xx][yy].position;

                                    v -= positions[i];

                                    double d = Math.Sqrt((v.X * v.X + v.Y * v.Y));

                                    if (d < (float)(random.NextDouble() * length[i]))
                                    {
                                        chunks[ii].gobjects[xx][yy].texture = 8;
                                        chunks[ii].gobjects[xx][yy].position = new Vector2(chunks[ii].boundaries.X + xx, chunks[ii].boundaries.Y + yy);
                                        ((GStone)(chunks[ii].gobjects[xx][yy])).stoneType = ERessourceType.RawPurPur;
                                        ((GStone)(chunks[ii].gobjects[xx][yy])).maxhealth = 5000;
                                        ((GStone)(chunks[ii].gobjects[xx][yy])).health = 5000;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================

            Master.loadedChunks = chunks.ToArray();


            TransportHandler.initialize();

            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================
            /// ========================================================================================================================================================================================================================================================

            // Step 5: Scenario Specific
            {
                if (scenario == 0)
                {
                    Master.youCanWin = false;
                    Master.WinningState = EWinningState.None;

                    Rectangle rect = new Rectangle(-1, -1, 1, 1);

                    for (int i = 0; i < Master.loadedChunks.Length; i++)
                    {
                        if (Master.loadedChunks[i].boundaries.Contains(rect))
                        {
                            int xobj, yobj;

                            Master.getCoordsInChunk(out xobj, out yobj, i, -1, -1);

                            GBuilding gb = new BMainBuilding(new Vector2(-1, -1), false);

                            Master.AddFinishedBuilding(gb, i, xobj, yobj, true);

                            break;
                        }
                    }

                    Master.addUnit(new GMiner(new Vector2(0, 2), false, false));
                    Master.addUnit(new GMiner(new Vector2(1, 2), false, false));
                    Master.addUnit(new GMiner(new Vector2(0, 3), false, false));
                    Master.addUnit(new GMiner(new Vector2(1, 3), false, false));

                    Master.addUnit(new GFighter(new Vector2(3, 3), false, false));
                    Master.addUnit(new GFighter(new Vector2(3, 4), false, false));

                    Master.addTransport(new GTransport(new Vector2(-1, -1), false));
                    Master.addTransport(new GTransport(new Vector2(-1, -2), false));
                    Master.addTransport(new GTransport(new Vector2(-1, -3), false));
                    Master.addTransport(new GTransport(new Vector2(-2, -1), false));
                    Master.addTransport(new GTransport(new Vector2(-2, -2), false));
                    Master.addTransport(new GTransport(new Vector2(-2, -3), false));
                    Master.addTransport(new GTransport(new Vector2(-3, -1), false));
                    Master.addTransport(new GTransport(new Vector2(-3, -2), false));
                    Master.addTransport(new GTransport(new Vector2(-3, -3), false));


                    /*Ressource res = new Ressource(ERessourceType.IronIngot, Vector2.Zero);
                    for (int i = 0; i < 15; i++)
                    {
                        Master.addOffer(res);
                    }

                    res = new Ressource(ERessourceType.Iron, Vector2.Zero);
                    Ressource res2 = new Ressource(ERessourceType.Iron, Vector2.Zero);

                    for (int i = 0; i < 20; i++)
                    {
                        Master.addOffer(res);
                        Master.addOffer(res2);
                    }*/

                    /*for (int i = 0; i < 2000; i++)
                    {
                        Master.addOffer(new Ressource(ERessourceType.Coal, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.Food, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.Gold, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.GoldIngot, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.Ice, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.Iron, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.IronIngot, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.PurPur, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.RawPurPur, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.Stone, Vector2.Zero));
                        Master.addOffer(new Ressource(ERessourceType.Water, Vector2.Zero));
                    }*/
                }

                // ===================================================================================================================================================================
                // ===================================================================================================================================================================
                // ===================================================================================================================================================================
                // ===================================================================================================================================================================
                // ===================================================================================================================================================================
                // ===================================================================================================================================================================

                else if (scenario == 1)
                {
                    Master.youCanWin = true;
                    Master.WinningState = EWinningState.None;

                    bool hostile = false;
                    int startX = 0, startY = -(int)(1.25f * Master.chunknum);

                    Master.camera.currentPos = new Vector2(startX, startY);
                    Master.camera.AimPos = Master.camera.currentPos;

                    Rectangle rect = new Rectangle(startX + -1, startY + -1, 1, 1);
                    Vector2 basepos = new Vector2(startX + -1, startY + -1);

                    for (int i = 0; i < Master.loadedChunks.Length; i++)
                    {
                        if (Master.loadedChunks[i].boundaries.Contains(rect))
                        {
                            int xobj, yobj;

                            Master.getCoordsInChunk(out xobj, out yobj, i, (int)basepos.X, (int)basepos.Y);

                            GBuilding gb = new BMainBuilding(basepos, false);

                            Master.AddFinishedBuilding(gb, i, xobj, yobj, hostile);

                            break;
                        }
                    }

                    basepos += new Vector2(1);

                    Master.addUnit(new GMiner(basepos + new Vector2(0, 2), hostile, false));
                    Master.addUnit(new GMiner(basepos + new Vector2(1, 2), hostile, false));
                    Master.addUnit(new GMiner(basepos + new Vector2(0, 3), hostile, false));
                    Master.addUnit(new GMiner(basepos + new Vector2(1, 3), hostile, false));

                    Master.addUnit(new GFighter(basepos + new Vector2(3, 3), hostile, false));
                    Master.addUnit(new GFighter(basepos + new Vector2(3, 4), hostile, false));

                    Master.addTransport(new GTransport(basepos + new Vector2(-1, -1), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-1, -2), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-1, -3), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-2, -1), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-2, -2), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-2, -3), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-3, -1), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-3, -2), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-3, -3), hostile));

                    // =====================================================================================================

                    hostile = true;
                    startY = (int)(1.25f * Master.chunknum);

                    // =====================================================================================================

                    rect = new Rectangle(startX + -1, startY + -1, 1, 1);
                    basepos = new Vector2(startX + -1, startY + -1);

                    CheapAI.initialize(basepos);

                    for (int i = 0; i < Master.loadedChunks.Length; i++)
                    {
                        if (Master.loadedChunks[i].boundaries.Contains(rect))
                        {
                            int xobj, yobj;

                            Master.getCoordsInChunk(out xobj, out yobj, i, (int)basepos.X, (int)basepos.Y);

                            GBuilding gb = new BMainBuilding(basepos, hostile);

                            Master.AddFinishedBuilding(gb, i, xobj, yobj, false);

                            break;
                        }
                    }

                    basepos += new Vector2(1);

                    Master.addUnit(new GMiner(basepos + new Vector2(0, 2), hostile, false));
                    Master.addUnit(new GMiner(basepos + new Vector2(1, 2), hostile, false));
                    Master.addUnit(new GMiner(basepos + new Vector2(0, 3), hostile, false));
                    Master.addUnit(new GMiner(basepos + new Vector2(1, 3), hostile, false));

                    Master.addUnit(new GFighter(basepos + new Vector2(3, 3), hostile, false));
                    Master.addUnit(new GFighter(basepos + new Vector2(3, 4), hostile, false));

                    Master.addTransport(new GTransport(basepos + new Vector2(-1, -1), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-1, -2), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-1, -3), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-2, -1), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-2, -2), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-2, -3), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-3, -1), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-3, -2), hostile));
                    Master.addTransport(new GTransport(basepos + new Vector2(-3, -3), hostile));
                }
            }
        }
    }
}
