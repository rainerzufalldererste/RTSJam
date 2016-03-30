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
        public bool hostile = false;
        public int health = 100;
        public int maxHealth = 100;

        public virtual void doAction(EActionType actionType, Vector2 pos1, Vector2? pos2)
        {

        }

        public virtual void update()
        {

        }

        public virtual void draw(SpriteBatch batch)
        {
            if(health < maxHealth)
            {
                batch.Draw(Master.pixel, position - new Vector2(0f,1f), null,
                    new Color(health < maxHealth / 2 ? (health / (maxHealth / 2f)) : 0f, 1f - (health > maxHealth / 2 ? ((health - (float)maxHealth / 2) / ((float)maxHealth / 2)) : 1f), 0f, .75f),
                    0f, new Vector2(.5f), new Vector2(.05f,.5f*(health / maxHealth)), SpriteEffects.None, 0f);
            }
        }
    }

    public class GMiner : GUnit
    {
        public Selection LastSelection;
        public Vector2 nextPos = Vector2.Zero;
        public EMinerAction currentAction = EMinerAction.None;
        public bool mineAtLocation = false;
        public int countdown = 0;
        public float speed = .05f;
        public float radius = 5f;
        public bool softmine = false;
        public GStone selectedStone = null;
        public bool drivingRight = true;
        
        public GMiner(Vector2 pos, bool hostile, bool softminer)
        {
            position = pos;
            actionType = EActionType.ClickPosition | EActionType.SelectRegion;
            this.hostile = hostile;
            this.softmine = softminer;
            health = softmine ? 200 : 100;
            maxHealth = health;
            speed = softmine ? .02f : .035f;
        }

        public override void doAction(EActionType actionType, Vector2 pos1, Vector2? pos2)
        {
            if(actionType == EActionType.ClickPosition)
            {
                nextPos = pos1;
                mineAtLocation = false;
                currentAction = EMinerAction.Move;
            }
            else
            {
                if (!pos2.HasValue)
                    throw new Exception("What the heck are you doing?!");

                nextPos = pos1;
                LastSelection = new Selection(pos1, pos2.Value);

                mineAtLocation = true;
                currentAction = EMinerAction.Move;
            }
        }

        public override void update()
        {
            switch(currentAction)
            {
                case EMinerAction.Move:

                    Vector2 difference = nextPos - position;
                    float angle = Master.angleOfVector(difference);
                    position += Master.VectorFromAngle(angle) * speed;
                    selectedStone = null;

                    Rectangle positionRect = new Rectangle((int)position.X - 1, (int)position.Y - 1, 2, 2);
                    bool found = false;

                    for (int i = 0; i < Master.loadedChunks.Length; i++)
                    {
                        if (Master.loadedChunks[i].boundaries.Intersects(positionRect))
                        {
                            int xobj, yobj;

                            Master.getCoordsInChunk(out xobj, out yobj, i, (int)position.X, (int)position.Y);

                            if (xobj >= 0 && xobj < Master.chunknum && yobj >= 0 && yobj < Master.chunknum)
                            {
                                if (Master.loadedChunks[i].gobjects[xobj][yobj] is GStone)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            xobj++;
                            if (xobj >= 0 && xobj < Master.chunknum && yobj >= 0 && yobj < Master.chunknum)
                            {
                                if (Master.loadedChunks[i].gobjects[xobj][yobj] is GStone)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            yobj++;
                            if (xobj >= 0 && xobj < Master.chunknum && yobj >= 0 && yobj < Master.chunknum)
                            {
                                if (Master.loadedChunks[i].gobjects[xobj][yobj] is GStone)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            
                            xobj--;
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
                        position -= Master.VectorFromAngle(angle) * speed;

                        if (mineAtLocation/* && difference.Length() < 20f * speed*/) // TODO: does this work?
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
                        if(selectedStone != null)
                        {
                            Master.replaceGObject(selectedStone.position, new GObject() { position = selectedStone.position, texture = 1 });
                        }

                        selectedStone = null; // GC, DO YOUR THING!

                        var list = Master.getChunk((int)position.X, (int)position.Y);

                        int xo1, yo1, xo2, yo2;
                        float dist = float.MaxValue;

                        for (int i = 0; i < list.Count; i++)
                        {
                            Master.getCoordsInChunk(out xo1, out yo1, list[i], (int)position.X - (int)(radius), (int)position.Y - (int)(radius));
                            Master.getCoordsInChunk(out xo2, out yo2, list[i], (int)position.X + (int)(radius), (int)position.Y + (int)(radius));

                            xo1 = Master.Clamp<int>(xo1, 0, Master.chunknum - 1);
                            xo2 = Master.Clamp<int>(xo2, 0, Master.chunknum - 1);
                            yo1 = Master.Clamp<int>(yo1, 0, Master.chunknum - 1);
                            yo2 = Master.Clamp<int>(yo2, 0, Master.chunknum - 1);

                            for (int j = xo1; j <= xo2; j++)
                            {
                                for (int k = yo1; k <= yo2; k++)
                                {
                                    if (Master.loadedChunks[list[i]].gobjects[j][k] is GStone)
                                    {
                                        float xx = (float)Math.Abs(j) - Math.Abs(position.X), yy = (float)Math.Abs(k) - Math.Abs(position.Y);
                                        float ldist = (float)Math.Sqrt(xx * xx + yy * yy);

                                        if (ldist < dist)
                                        {
                                            if ((((GStone)Master.loadedChunks[list[i]].gobjects[j][k]).texture == 3 || ((GStone)Master.loadedChunks[list[i]].gobjects[j][k]).texture == 4 || ((GStone)Master.loadedChunks[list[i]].gobjects[j][k]).texture == 8))
                                            {
                                                if (softmine)
                                                {
                                                    selectedStone = (GStone)Master.loadedChunks[list[i]].gobjects[j][k];
                                                    dist = ldist;
                                                }
                                            }
                                            else
                                            {
                                                selectedStone = (GStone)Master.loadedChunks[list[i]].gobjects[j][k];
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

                            if(position != nextPos)
                            {
                                currentAction = EMinerAction.Move;
                            }
                            else
                            {
                                Master.notify("This Miner has nothing to mine here!", position);
                                currentAction = EMinerAction.None;
                            }
                        }
                    }
                    else
                    {
                        selectedStone.health -= 1;

                        if(selectedStone.health % Master.stoneDropNum[(int)selectedStone.stoneType] == 0)
                        {
                            Master.drop(new Ressource(selectedStone.stoneType, selectedStone.position));
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            if(currentAction == EMinerAction.Move)
            {
                if(nextPos.X > position.X)
                {
                    drivingRight = true;
                }
                else
                {
                    drivingRight = false;
                }
            }

            if(softmine)
            {
                batch.Draw(Master.unitTextures[1], position, null, Color.White,
                    0f, new Vector2(Master.unitTextures[1].Width/2f, Master.unitTextures[1].Height / 2f),
                    Master.scaler, drivingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    Master.calculateDepth(position.Y + 1f));
            }
            else
            {
                batch.Draw(Master.unitTextures[5], position, null, Color.White,
                    0f, new Vector2(Master.unitTextures[1].Width / 2f, Master.unitTextures[5].Height / 2f),
                    Master.scaler, drivingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    Master.calculateDepth(position.Y + 1f));
            }

            if(currentAction == EMinerAction.Mine && selectedStone != null)
            {
                Master.DrawLine(batch, position, selectedStone.position, new Color(.8f, .8f, .8f, .5f), .05f, Master.calculateDepth(Math.Max(position.Y, selectedStone.position.Y)));

                batch.Draw(Master.pixel, selectedStone.position, null,
                    new Color(0f + (selectedStone.health < selectedStone.maxhealth / 2f ? (selectedStone.health / selectedStone.maxhealth) * 2f : 0f),
                              1f - (selectedStone.health > selectedStone.maxhealth / 2f ? (selectedStone.health / (selectedStone.maxhealth / 2)) : 1f), 0f, .25f),
                        0f, new Vector2(.5f, .5f), .5f, SpriteEffects.None, 1f);
            }

            base.draw(batch);
        }
    }

    public class GTransport
    {
        public int textureNum = 0;

        public Transaction activeTransaction;
        public ETransporterState currentState = ETransporterState.None;

        public Vector2 position;
        public float speed = .1f;
        public const int maxTexNum = 30;

        public bool hostile = false;

        public GTransport(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
        }


        public void update()
        {
            if (currentState != ETransporterState.None && activeTransaction.buildingID.doesNotExist)
            {
                currentState = ETransporterState.None;

                if (currentState == ETransporterState.MoveToDestination)
                {
                    // drop ressource
                    Master.drop(new Ressource(activeTransaction.type, position));
                    TransportHandler.addFreeTransport(this);
                }
            }

            if (currentState == ETransporterState.MoveToDestination)
            {
                Vector2 difference = activeTransaction.destiation - position;
                float angle = Master.angleOfVector(difference);
                position += Master.VectorFromAngle(angle) * speed;

                if(difference.Length() < speed * 2)
                {
                    activeTransaction.buildingID.addRessource(activeTransaction.type);
                    currentState = ETransporterState.None;
                    TransportHandler.addFreeTransport(this);
                }
            }
            else if(currentState == ETransporterState.MoveToOrigin)
            {
                Vector2 difference = activeTransaction.origin - position;
                float angle = Master.angleOfVector(difference);
                position += Master.VectorFromAngle(angle) * speed;

                if (difference.Length() < speed * 2)
                {
                    currentState = ETransporterState.MoveToDestination;
                    Master.ressources.Remove(activeTransaction.ressourceID);
                }
            }
        }

        public void draw(SpriteBatch batch)
        {
            textureNum++;

            if(textureNum > maxTexNum)
            {
                textureNum = 0;
            }

            batch.Draw(Master.unitTextures[textureNum < maxTexNum ? 6 : 7 ], position, null, Color.White,
                0f, new Vector2(Master.unitTextures[6].Width, Master.unitTextures[6].Height),
                Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 2f));

            if(currentState == ETransporterState.MoveToDestination)
            {
                batch.Draw(Master.ressourceTextures[(int)activeTransaction.type], position, null, Color.White,
                    0f, new Vector2(Master.ressourceTextures[(int)activeTransaction.type].Width / 2f, Master.ressourceTextures[(int)activeTransaction.type].Height / 2f),
                    Master.scaler * .15f, SpriteEffects.None, Master.calculateDepth(position.Y + 2f));
            }
        }

        public void setTransport(Transaction transaction)
        {
            currentState = ETransporterState.MoveToOrigin;
            activeTransaction = transaction;
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

    public enum ETransporterState
    {
        None, MoveToDestination, MoveToOrigin
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
