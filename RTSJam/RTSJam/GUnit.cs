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
    public class GUnit
    {
        public Vector2 position;
        public EActionType actionType = EActionType.ClickPosition;

        public virtual void doAction(EActionType actionType, Vector2 pos1, Vector2? pos2)
        {

        }

        public virtual void update()
        {

        }

        public virtual void draw(SpriteBatch batch)
        {

        }
    }

    public class GMiner : GUnit
    {
        public Selection LastSelection;
        public Vector2 nextPos = Vector2.Zero;
        public EMinerAction currentAction = EMinerAction.None;
        public bool mineAtLocation = false;
        public int countdown = 0;
        public float speed = .25f;
        public float radius = 5 * .25f;
        public bool softmine = false;
        public GStone selectedStone = null;
        
        public GMiner(Vector2 pos)
        {
            position = pos;
            actionType = EActionType.ClickPosition | EActionType.SelectRegion;
        }

        public override void doAction(EActionType actionType, Vector2 pos1, Vector2? pos2)
        {
            if(actionType == EActionType.ClickPosition)
            {
                nextPos = pos1;
                mineAtLocation = false;
            }
            else
            {
                if (!pos2.HasValue)
                    throw new Exception("What the heck are you doing?!");

                nextPos = pos1;
                LastSelection = new Selection(pos1, pos2.Value);

                mineAtLocation = true;
            }
        }

        public override void update()
        {
            switch(currentAction)
            {
                case EMinerAction.Move:

                    Vector2 difference = position - nextPos;
                    float angle = Master.angleOfVector(difference);
                    position += Master.VectorFromAngle(angle) * speed;

                    Rectangle positionRect = new Rectangle((int)position.X - 1, (int)position.Y - 1, 2, 2);
                    bool found = false;

                    for (int i = 0; i < Master.loadedChunks.Length; i++)
                    {
                        if (Master.loadedChunks[i].boundaries.Intersects(positionRect))
                        {
                            int xobj = Master.loadedChunks[i].boundaries.X - ((int)position.X),
                                yobj = Master.loadedChunks[i].boundaries.Y - ((int)position.Y);

                            if (xobj >= 0 && xobj < Master.chunknum && yobj >= 0 && yobj < Master.chunknum)
                            {
                                if (Master.loadedChunks[i].gobjects[xobj][yobj] is GStone)
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if(found)
                    {
                        if(mineAtLocation && difference.Length() < 3f * speed)
                        {
                            currentAction = EMinerAction.Mine;
                        }
                        else
                        {
                            currentAction = EMinerAction.None;
                        }
                    }

                    if (Math.Abs(Math.Sqrt(difference.X * difference.X + difference.Y * difference.Y)) < speed)
                    {
                        position = nextPos;

                        if (mineAtLocation)
                        {
                            currentAction = EMinerAction.Mine;
                        }
                        else
                        {
                            currentAction = EMinerAction.None;
                        }
                    }

                    break;

                case EMinerAction.Mine:

                    if (selectedStone == null || selectedStone.health <= 0)
                    {
                        selectedStone = null; // GC, DO YOUR THING!

                        var list = Master.getChunk((int)position.X, (int)position.Y);

                        int xo1, yo1, xo2, yo2;
                        float dist = float.MaxValue;

                        for (int i = 0; i < list.Count; i++)
                        {
                            Master.getCoordsInChunk(out xo1, out yo1, list[i], (int)position.X - (int)speed, (int)position.Y - (int)speed);
                            Master.getCoordsInChunk(out xo2, out yo2, list[i], (int)position.X + (int)speed, (int)position.Y + (int)speed);

                            xo1 = Master.Clamp<int>(xo1, 0, Master.chunknum);
                            xo2 = Master.Clamp<int>(xo2, 0, Master.chunknum);
                            yo1 = Master.Clamp<int>(yo1, 0, Master.chunknum);
                            yo2 = Master.Clamp<int>(yo2, 0, Master.chunknum);

                            for (int j = xo1; j < xo2; j++)
                            {
                                for (int k = yo1; k < yo2; k++)
                                {
                                    if (Master.loadedChunks[i].gobjects[j][k] is GStone)
                                    {
                                        float xx = position.X - j, yy = position.Y - k;
                                        float ldist = (float)Math.Sqrt(xx * xx + yy * yy);

                                        if (ldist < dist)
                                        {
                                            if (softmine && (((GStone)Master.loadedChunks[i].gobjects[j][k]).texture == 3 || ((GStone)Master.loadedChunks[i].gobjects[j][k]).texture == 4 || ((GStone)Master.loadedChunks[i].gobjects[j][k]).texture == 8))
                                            {
                                                selectedStone = (GStone)Master.loadedChunks[i].gobjects[j][k];
                                                dist = ldist;
                                            }
                                            else
                                            {
                                                selectedStone = (GStone)Master.loadedChunks[i].gobjects[j][k];
                                                dist = ldist;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // we found nothing? :O
                        if (dist == float.MaxValue)
                        {
                            currentAction = EMinerAction.None;

                            Master.notify("This Miner has nothing to mine here!", position);
                        }
                    }
                    else
                    {
                        selectedStone.health -= 1;

                        if(selectedStone.health % Master.stoneDropNum[(int)selectedStone.stoneType] == 0)
                        {
                            Master.drop(selectedStone.stoneType, selectedStone.position);
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            // TODO: draw!
        }
    }

    [Flags]
    public enum EActionType
    {
        ClickPosition, SelectRegion
    }
    public enum EMinerAction
    {
        None, Move, Mine
    }

    public struct Selection
    {
        Vector2 pos1, pos2;

        public Selection(Vector2 pos1, Vector2 pos2)
        {
            this.pos1 = pos1;
            this.pos2 = pos2;
        }
    }
}
