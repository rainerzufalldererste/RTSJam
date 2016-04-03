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
    public class GUnit : G_DamagableObject
    { 
        public EActionType actionType = EActionType.ClickPosition;

        public virtual void doAction(EActionType actionType, Vector2 pos1, Vector2? pos2)
        {

        }

        public virtual void update()
        {

        }

        /// <summary>
        /// DO NOT CALL THIS DIRECTLY - ONLY VIA Master.dealDamageTo
        /// </summary>
        /// <param name="damage"></param>
        public override void takeDamage(int damage, G_DamagableObject sender)
        {
            health -= damage;

            if(health <= 0)
            {
                health = 0;

                Master.removeUnit(this);
            }
        }

        public virtual void draw(SpriteBatch batch)
        {
            baseDraw(batch);
        }

        public void baseDraw(SpriteBatch batch)
        {
            if (health < maxhealth)
            {
                if (this is GTank)
                {
                    batch.Draw(Master.pixel, position + new Vector2(.25f, .25f), null,
                        Color.Black,
                        0f, new Vector2(0f, .5f), new Vector2(.5f, .05f), SpriteEffects.None, 0.1f);

                    batch.Draw(Master.pixel, position + new Vector2(.25f, .25f), null,
                        new Color(
                        1f - (health > maxhealth / 2 ? ((health - (float)maxhealth / 2) / ((float)maxhealth / 2)) : 0f),
                        health > maxhealth / 2 ? 1f : (health / (maxhealth / 2f)), 0f, 1f),
                        0f, new Vector2(0f,.5f), new Vector2(.5f * ((float)health / (float)maxhealth), .05f), SpriteEffects.None, 0f);
                }
                else
                {
                    batch.Draw(Master.pixel, position - new Vector2(.25f, .1f), null,
                        Color.Black,
                        0f, new Vector2(0f, .5f), new Vector2(.5f, .05f), SpriteEffects.None, 0.1f);
                    
                    batch.Draw(Master.pixel, position - new Vector2(.25f, .1f), null,
                        new Color(
                        1f - (health > maxhealth / 2 ? ((health - (float)maxhealth / 2) / ((float)maxhealth / 2)) : 0f),
                        health > maxhealth / 2 ? 1f : (health / (maxhealth / 2f)), 0f, 1f),
                        0f, new Vector2(0f, .5f), new Vector2(.5f * ((float)health / (float)maxhealth), .05f), SpriteEffects.None, 0f);
                }
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

        int doingNothingIn = doingNothingInMAX;
        const int doingNothingInMAX = 10;

        public ParticleSystem particleSystem = new ParticleSystem();

        public GMiner(Vector2 pos, bool hostile, bool softminer)
        {
            position = pos;
            actionType = EActionType.ClickPosition | EActionType.SelectRegion;
            this.hostile = hostile;
            this.softmine = softminer;
            health = softmine ? 3000 : 2000;
            maxhealth = health;
            speed = softmine ? .025f : .04f;
        }

        public override void doAction(EActionType actionType, Vector2 pos1, Vector2? pos2)
        {
            if (actionType == EActionType.ClickPosition)
            {
                nextPos = pos1;
                mineAtLocation = false;
                currentAction = EMinerAction.Move;
                doingNothingIn = doingNothingInMAX;
            }
            else
            {
                if (!pos2.HasValue)
                    throw new Exception("What the heck are you doing?!");

                nextPos = pos1;
                LastSelection = new Selection(pos1, pos2.Value);

                mineAtLocation = true;
                currentAction = EMinerAction.Move;
                doingNothingIn = doingNothingInMAX;
            }
        }

        public override void update()
        {
            switch (currentAction)
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

                    if (found)
                    {
                        position -= Master.VectorFromAngle(angle) * speed;

                        if (mineAtLocation/* && difference.Length() < 20f * speed*/)
                        {
                            currentAction = EMinerAction.Mine;
                        }
                        else
                        {
                            currentAction = EMinerAction.None;
                        }

                        doingNothingIn--;

                        if(doingNothingIn <= 0)
                        {
                            currentAction = EMinerAction.None;
                        }
                    }
                    else
                    {
                        doingNothingIn = doingNothingInMAX;
                        particleSystem.addDustParticle(position);
                    }

                    /*if(!found)
                    {
                        for (int i = 0; i < Master.units.Count; i++)
                        {
                            if(Master.units[i] != this && positionRect.Intersects(new Rectangle((int)Master.units[i].position.X, (int)Master.units[i].position.Y, 1,1)))
                            {
                                float xx = Master.units[i].position.X - position.X, yy = Master.units[i].position.Y - position.Y;

                                if(Math.Sqrt(xx * xx + yy * yy) < .4f)
                                {
                                    Vector2 diff = new Vector2(xx, yy);
                                    diff /= diff.Length();
                                    position += diff * speed * .1f;
                                }
                            }
                        }
                    }*/

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
                        if (selectedStone != null && !selectedStone.removed)
                        {
                            selectedStone.removed = true;
                            particleSystem.addHeavyDustParticles(selectedStone.position);

                            Master.replaceGObject(selectedStone.position, new GGround() { position = selectedStone.position, texture = 1 });
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
                                        float xx = Master.loadedChunks[list[i]].gobjects[j][k].position.X - position.X, yy = Master.loadedChunks[list[i]].gobjects[j][k].position.Y - position.Y;

                                        float ldist = (float)Math.Sqrt(xx * xx + yy * yy);

                                        if (Math.Abs(ldist) < Math.Abs(dist))
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
                                                if (!softmine)
                                                {
                                                    selectedStone = (GStone)Master.loadedChunks[list[i]].gobjects[j][k];
                                                    dist = ldist;
                                                }
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

                            if (position != nextPos)
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

                        if (selectedStone.health % Master.stoneDropNum[(int)selectedStone.texture] == 0)
                        {
                            Master.drop(new Ressource(selectedStone.stoneType, selectedStone.position), hostile);
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            particleSystem.update(batch);

            if (currentAction == EMinerAction.Move)
            {
                if (nextPos.X > position.X)
                {
                    drivingRight = true;
                }
                else
                {
                    drivingRight = false;
                }
            }

            if (softmine)
            {
                batch.Draw(hostile ? Master.HOSTILEunitTextures[1] : Master.unitTextures[1], position, null, Color.White,
                    0f, new Vector2(Master.unitTextures[1].Width / 2f, Master.unitTextures[1].Height / 2f),
                    Master.scaler, drivingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    Master.calculateDepth(position.Y + 1f));
            }
            else
            {
                batch.Draw(hostile ? Master.HOSTILEunitTextures[5] : Master.unitTextures[5], position, null, Color.White,
                    0f, new Vector2(Master.unitTextures[1].Width / 2f, Master.unitTextures[5].Height / 2f),
                    Master.scaler, drivingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    Master.calculateDepth(position.Y + 1f));
            }

            if (currentAction == EMinerAction.Mine && selectedStone != null)
            {
                Master.DrawLine(batch, position + new Vector2(0, .1f), selectedStone.position, new Color(.8f, .8f, .8f, .5f), .05f, Master.calculateDepth(Math.Max(position.Y, selectedStone.position.Y)));

                batch.Draw(Master.fxTextures[5], selectedStone.position, null,
                    new Color(
                    1f - (selectedStone.health > selectedStone.maxhealth / 2 ? ((selectedStone.health - (float)selectedStone.maxhealth / 2) / ((float)selectedStone.maxhealth / 2)) : 0f),
                    selectedStone.health > selectedStone.maxhealth / 2 ? 1f : (selectedStone.health / (selectedStone.maxhealth / 2f)), 0f, .25f),
                        0f, new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, 0f);
            }

            base.draw(batch);
        }
    }

    public class GFighter : GUnit
    {
        public Selection LastSelection;
        public Vector2 nextPos = Vector2.Zero;
        public EFighterAction currentAction = EFighterAction.None;
        public bool fightAtLocation = false;
        public int countdown = 0;
        public float speed = .05f;
        public float radius = 5f;
        public bool betterfighter = false;
        public G_DamagableObject selectedEnemy = null;
        public bool drivingRight = true;

        public EGFighterFightMode fightMode = EGFighterFightMode.ClosestDistance;

        public float range;
        public int damage;

        public int doingNothingIn = doingNothingInMAX;
        public const int doingNothingInMAX = 10;

        public ParticleSystem particleSystem = new ParticleSystem();
        public bool dontcareabouteverything = false;

        public GFighter(Vector2 pos, bool hostile, bool better_fightr)
        {
            position = pos;
            actionType = EActionType.ClickPosition | EActionType.SelectRegion;
            this.hostile = hostile;
            this.betterfighter = better_fightr;
            health = betterfighter ? 6500 : 4500;
            maxhealth = health;
            speed = betterfighter ? .05f : .075f;
            range = betterfighter ? 7.5f : 5f;
            damage = betterfighter ? 3 : 2;
        }

        public override void doAction(EActionType actionType, Vector2 pos1, Vector2? pos2)
        {
            if (actionType == EActionType.ClickPosition)
            {
                nextPos = pos1;
                fightAtLocation = false;
                currentAction = EFighterAction.Move;

                doingNothingIn = doingNothingInMAX;
            }
            else
            {
                if (!pos2.HasValue)
                    throw new Exception("What the heck are you doing?!");

                nextPos = pos1;
                LastSelection = new Selection(pos1, pos2.Value);

                fightAtLocation = true;
                currentAction = EFighterAction.Move;

                doingNothingIn = doingNothingInMAX;
            }
        }

        public override void update()
        {
            switch (currentAction)
            {
                case EFighterAction.Move:

                    Vector2 difference = nextPos - position;
                    float angle = Master.angleOfVector(difference);
                    position += Master.VectorFromAngle(angle) * speed;
                    selectedEnemy = null;

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

                    if (found)
                    {
                        position -= Master.VectorFromAngle(angle) * speed;

                        if (fightAtLocation/* && difference.Length() < 20f * speed*/)
                        {
                            currentAction = EFighterAction.Fight;
                        }
                        else
                        {
                            currentAction = EFighterAction.None;
                        }

                        doingNothingIn--;

                        if(doingNothingIn <= 0)
                        {
                            currentAction = EFighterAction.None;
                        }
                    }
                    else
                    {
                        particleSystem.addFadingDustParticle(position);
                        doingNothingIn = doingNothingInMAX;
                    }

                    /*if(!found)
                    {
                        for (int i = 0; i < Master.units.Count; i++)
                        {
                            if(Master.units[i] != this && positionRect.Intersects(new Rectangle((int)Master.units[i].position.X, (int)Master.units[i].position.Y, 1,1)))
                            {
                                float xx = Master.units[i].position.X - position.X, yy = Master.units[i].position.Y - position.Y;

                                if(Math.Sqrt(xx * xx + yy * yy) < .4f)
                                {
                                    Vector2 diff = new Vector2(xx, yy);
                                    diff /= diff.Length();
                                    position += diff * speed * .1f;
                                }
                            }
                        }
                    }*/

                    if (Math.Abs(Math.Sqrt(difference.X * difference.X + difference.Y * difference.Y)) < speed)
                    {
                        position = nextPos;

                        if (fightAtLocation)
                        {
                            currentAction = EFighterAction.Fight;
                        }
                        else
                        {
                            currentAction = EFighterAction.None;
                        }
                    }

                    break;

                case EFighterAction.Fight:
                    
                    if(selectedEnemy == null || selectedEnemy.health <= 0 || (selectedEnemy is GBuilding && ((GBuilding)selectedEnemy).doesNotExist))
                    {
                        selectedEnemy = null;

                        float mindist = float.MaxValue;
                        float minhp = int.MaxValue;
                        int maxhp = int.MinValue;

                        float lrange, lminhp;

                        for (int i = 0; i < Master.units.Count; i++)
                        {
                            if (Master.units[i].hostile == hostile)
                                continue;

                            lrange = (Master.units[i].position - position).Length();

                            if (lrange < range)
                            {
                                if(fightMode == EGFighterFightMode.ClosestDistance)
                                {
                                    if(lrange < mindist)
                                    {
                                        selectedEnemy = Master.units[i];
                                        mindist = lrange;
                                    }
                                }
                                else if (fightMode == EGFighterFightMode.MinimumHPPercentage)
                                {
                                    lminhp = (float)Master.units[i].health / (float)Master.units[i].maxhealth;

                                    if (lminhp < minhp)
                                    {
                                        minhp = lminhp;
                                        selectedEnemy = Master.units[i];
                                    }
                                }
                                else if (fightMode == EGFighterFightMode.MaximumTotalHP)
                                {
                                    if(maxhp < Master.units[i].maxhealth)
                                    {
                                        selectedEnemy = Master.units[i];
                                        maxhealth = Master.units[i].maxhealth;
                                    }
                                }
                            }
                        }

                        if(selectedEnemy == null)
                        {
                            for (int i = 0; i < Master.buildings.Count; i++)
                            {
                                if (Master.buildings[i].hostile == hostile)
                                    continue;

                                lrange = (Master.buildings[i].position - position).Length();

                                if (lrange < range)
                                {
                                    if (lrange < mindist)
                                    {
                                        selectedEnemy = Master.buildings[i];
                                        mindist = lrange;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if ((selectedEnemy.position - position).Length() <= range)
                            Master.dealDamageTo(selectedEnemy, damage, this);
                        else if (dontcareabouteverything)
                            selectedEnemy = null;
                        else
                            Master.moveUnitToPosition(this, selectedEnemy.position, true);
                    }

                    break;

                default:
                    break;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            particleSystem.update(batch);

            if (currentAction == EFighterAction.Move)
            {
                if (nextPos.X > position.X)
                {
                    drivingRight = true;
                }
                else
                {
                    drivingRight = false;
                }
            }

            if (betterfighter)
            {
                batch.Draw(hostile ? Master.HOSTILEunitTextures[3] : Master.unitTextures[3], position, null, Color.White,
                    0f, new Vector2(Master.unitTextures[1].Width / 2f, Master.unitTextures[1].Height / 2f),
                    Master.scaler, drivingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    Master.calculateDepth(position.Y + 1f));
            }
            else
            {
                batch.Draw(hostile ? Master.HOSTILEunitTextures[4] : Master.unitTextures[4], position, null, Color.White,
                    0f, new Vector2(Master.unitTextures[1].Width / 2f, Master.unitTextures[5].Height / 2f),
                    Master.scaler, drivingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    Master.calculateDepth(position.Y + 1f));
            }

            if (currentAction == EFighterAction.Fight && selectedEnemy != null)
            {
                Master.DrawLine(batch, position + new Vector2(0, .1f), selectedEnemy.position, hostile ? new Color(.8f, .2f, .2f, .25f) : new Color(.2f, .4f, .8f, .25f), .1f, Master.calculateDepth(Math.Max(position.Y, selectedEnemy.position.Y)));
                Master.DrawLine(batch, position + new Vector2(0, .1f), selectedEnemy.position, hostile ? new Color(.8f, .2f, .2f, .5f) : new Color(.2f, .4f, .8f, .5f), .05f, Master.calculateDepth(Math.Max(position.Y, selectedEnemy.position.Y)));

                batch.Draw(Master.fxTextures[4], selectedEnemy.position, null,
                    new Color(
                    1f - (selectedEnemy.health > selectedEnemy.maxhealth / 2 ? ((selectedEnemy.health - (float)selectedEnemy.maxhealth / 2) / ((float)selectedEnemy.maxhealth / 2)) : 0f),
                    selectedEnemy.health > selectedEnemy.maxhealth / 2 ? 1f : (selectedEnemy.health / (selectedEnemy.maxhealth / 2f)), 0f, .25f),
                        0f, new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, 0f);
            }

            base.draw(batch);
        }

        public override void takeDamage(int damage, G_DamagableObject sender)
        {
            base.takeDamage(damage, sender);

            if (!dontcareabouteverything)
            {
                if (currentAction == EFighterAction.None && sender.hostile != this.hostile)
                {
                    if ((sender.position - position).Length() > range)
                    {
                        Master.moveUnitToPosition(this, sender.position, true);
                    }
                    else
                    {
                        currentAction = EFighterAction.Fight;
                        selectedEnemy = sender;
                    }
                }
            }
        }
    }


    public class GTank : GFighter
    {
        G_DamagableObject[] selectedEnemies = new G_DamagableObject[4];

        public GTank(Vector2 pos, bool hostile, bool better_fightr) : base(pos, hostile, better_fightr)
        {
            position = pos;
            actionType = EActionType.ClickPosition | EActionType.SelectRegion;
            this.hostile = hostile;
            this.betterfighter = better_fightr;
            health = betterfighter ? 65000 : 45000;
            maxhealth = health;
            speed = betterfighter ? .03f : .035f;
            range = betterfighter ? 12f : 8f;
            damage = betterfighter ? 4 : 6;
            dontcareabouteverything = false;

            if(better_fightr)
                selectedEnemies = new G_DamagableObject[8];
        }

        public override void doAction(EActionType actionType, Vector2 pos1, Vector2? pos2)
        {
            if (actionType == EActionType.ClickPosition)
            {
                nextPos = pos1;
                fightAtLocation = false;
                currentAction = EFighterAction.Move;

                doingNothingIn = doingNothingInMAX;
            }
            else
            {
                if (!pos2.HasValue)
                    throw new Exception("What the heck are you doing?!");

                nextPos = pos1;
                LastSelection = new Selection(pos1, pos2.Value);

                fightAtLocation = true;
                currentAction = EFighterAction.Move;

                doingNothingIn = doingNothingInMAX;
            }
        }

        public override void update()
        {
            switch (currentAction)
            {
                case EFighterAction.Move:

                    Vector2 difference = nextPos - position;
                    float angle = Master.angleOfVector(difference);
                    position += Master.VectorFromAngle(angle) * speed;
                    selectedEnemy = null;

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

                    if (found)
                    {
                        position -= Master.VectorFromAngle(angle) * speed;

                        if (fightAtLocation/* && difference.Length() < 20f * speed*/)
                        {
                            currentAction = EFighterAction.Fight;
                        }
                        else
                        {
                            currentAction = EFighterAction.None;
                        }

                        doingNothingIn--;

                        if (doingNothingIn <= 0)
                        {
                            currentAction = EFighterAction.None;
                        }
                    }
                    else
                    {
                        particleSystem.addDust1Particle(position);
                        doingNothingIn = doingNothingInMAX;
                    }

                    /*if(!found)
                    {
                        for (int i = 0; i < Master.units.Count; i++)
                        {
                            if(Master.units[i] != this && positionRect.Intersects(new Rectangle((int)Master.units[i].position.X, (int)Master.units[i].position.Y, 1,1)))
                            {
                                float xx = Master.units[i].position.X - position.X, yy = Master.units[i].position.Y - position.Y;

                                if(Math.Sqrt(xx * xx + yy * yy) < .4f)
                                {
                                    Vector2 diff = new Vector2(xx, yy);
                                    diff /= diff.Length();
                                    position += diff * speed * .1f;
                                }
                            }
                        }
                    }*/

                    if (Math.Abs(Math.Sqrt(difference.X * difference.X + difference.Y * difference.Y)) < speed)
                    {
                        position = nextPos;

                        if (fightAtLocation)
                        {
                            currentAction = EFighterAction.Fight;
                        }
                        else
                        {
                            currentAction = EFighterAction.None;
                        }
                    }

                    break;

                case EFighterAction.Fight:

                    for (int xx = 0; xx < selectedEnemies.Length; xx++)
                    {
                        if (selectedEnemies[xx] == null || selectedEnemies[xx].health <= 0 || (selectedEnemies[xx] is GBuilding && ((GBuilding)selectedEnemies[xx]).doesNotExist))
                        {
                            selectedEnemies[xx] = null;

                            float mindist = float.MaxValue;
                            float minhp = int.MaxValue;
                            int maxhp = int.MinValue;

                            float lrange = 0, lminhp = 0;

                            for (int i = 0; i < Master.units.Count; i++)
                            {
                                if (Master.units[i].hostile == hostile || Master.units[i].health <= 0 || enemyIsTheSame(Master.units[i], selectedEnemies))
                                    continue;

                                lrange = (Master.units[i].position - position).Length();

                                if (lrange < range)
                                {
                                    if (fightMode == EGFighterFightMode.ClosestDistance)
                                    {
                                        if (lrange < mindist)
                                        {
                                            selectedEnemies[xx] = Master.units[i];
                                            mindist = lrange;
                                        }
                                    }
                                    else if (fightMode == EGFighterFightMode.MinimumHPPercentage)
                                    {
                                        lminhp = (float)Master.units[i].health / (float)Master.units[i].maxhealth;

                                        if (lminhp < minhp)
                                        {
                                            minhp = lminhp;
                                            selectedEnemies[xx] = Master.units[i];
                                        }
                                    }
                                    else if (fightMode == EGFighterFightMode.MaximumTotalHP)
                                    {
                                        if (maxhp < Master.units[i].maxhealth)
                                        {
                                            selectedEnemies[xx] = Master.units[i];
                                            maxhealth = Master.units[i].maxhealth;
                                        }
                                    }
                                }
                            }

                            if (selectedEnemies[xx] == null)
                            {
                                for (int i = 0; i < Master.buildings.Count; i++)
                                {
                                    if (Master.buildings[i].hostile == hostile && !selectedEnemies.Contains<G_DamagableObject>(Master.buildings[i]))
                                        continue;

                                    lrange = (Master.buildings[i].position - position).Length();

                                    if (lrange < range)
                                    {
                                        if (lrange < mindist)
                                        {
                                            selectedEnemies[xx] = Master.buildings[i];
                                            mindist = lrange;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if ((selectedEnemies[xx].position - position).Length() <= range)
                                Master.dealDamageTo(selectedEnemies[xx], damage, this);
                            else
                                selectedEnemies[xx] = null;
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        private bool enemyIsTheSame(GUnit gUnit, G_DamagableObject[] selectedEnemies)
        {
            for (int i = 0; i < selectedEnemies.Length; i++)
            {
                if(selectedEnemies[i] != null && (gUnit.position == selectedEnemies[i].position && gUnit.health == selectedEnemies[i].health))
                {
                    return true;
                }
            }

            return false;
        }

        public override void draw(SpriteBatch batch)
        {
            particleSystem.update(batch);

            if (currentAction == EFighterAction.Move)
            {
                if (nextPos.X > position.X)
                {
                    drivingRight = true;
                }
                else
                {
                    drivingRight = false;
                }
            }

            if (betterfighter)
            {
                batch.Draw(hostile ? Master.HOSTILEunitTextures[2] : Master.unitTextures[2], position, null, Color.White,
                    0f, new Vector2(Master.unitTextures[1].Width / 2f, Master.unitTextures[1].Height / 2f),
                    Master.scaler, drivingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    Master.calculateDepth(position.Y + 2f));
            }
            else
            {
                batch.Draw(hostile ? Master.HOSTILEunitTextures[0] : Master.unitTextures[0], position, null, Color.White,
                    0f, new Vector2(Master.unitTextures[1].Width / 2f, Master.unitTextures[5].Height / 2f),
                    Master.scaler, drivingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    Master.calculateDepth(position.Y + 2f));
            }

            if (currentAction == EFighterAction.Fight)
            {
                for (int i = 0; i < selectedEnemies.Length; i++)
                {
                    if (selectedEnemies[i] != null)
                    {
                        Master.DrawLine(batch, position + new Vector2(0.5f, .7f), selectedEnemies[i].position, hostile ? new Color(.8f, .2f, .2f, .25f) : new Color(.2f, .4f, .8f, .25f), .125f, Master.calculateDepth(Math.Max(position.Y, selectedEnemies[i].position.Y)));
                        Master.DrawLine(batch, position + new Vector2(0.5f, .7f), selectedEnemies[i].position, hostile ? new Color(.8f, .2f, .2f, .5f) : new Color(.2f, .4f, .8f, .5f), .1f, Master.calculateDepth(Math.Max(position.Y, selectedEnemies[i].position.Y)));

                        batch.Draw(Master.fxTextures[4], selectedEnemies[i].position, null,
                            new Color(
                            1f - (selectedEnemies[i].health > selectedEnemies[i].maxhealth / 2 ? ((selectedEnemies[i].health - (float)selectedEnemies[i].maxhealth / 2) / ((float)selectedEnemies[i].maxhealth / 2)) : 0f),
                            selectedEnemies[i].health > selectedEnemies[i].maxhealth / 2 ? 1f : (selectedEnemies[i].health / (selectedEnemies[i].maxhealth / 2f)), 0f, .25f),
                                0f, new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, 0f);
                    }
                }
            }

            baseDraw(batch);
        }

        public override void takeDamage(int damage, G_DamagableObject sender)
        {
            base.takeDamage(damage, sender);

            if (!dontcareabouteverything)
            {
                if (currentAction == EFighterAction.None && sender.hostile != this.hostile)
                {
                    if ((sender.position - position).Length() > range)
                    {
                        Master.moveUnitToPosition(this, sender.position, true);
                    }
                    else
                    {
                        currentAction = EFighterAction.Fight;
                        selectedEnemy = sender;
                    }
                }
            }
        }
    }

    // ============================================================================================================================================================================================================================
    // ============================================================================================================================================================================================================================
    // ============================================================================================================================================================================================================================
    // ============================================================================================================================================================================================================================
    // ============================================================================================================================================================================================================================
    // ============================================================================================================================================================================================================================

    public class GTransport
    {
        public int textureNum = 0;

        public Transaction activeTransaction;
        public ETransporterState currentState = ETransporterState.None;

        public Vector2 position;
        public float speed = .05f;
        public const int maxTexNum = 16;

        public bool hostile = false;

        public GTransport(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;

            TransportHandler.addFreeTransport(this);
        }


        public void update()
        {
            if (currentState != ETransporterState.None && activeTransaction.buildingID.doesNotExist)
            {
                currentState = ETransporterState.None;

                if (currentState == ETransporterState.MoveToDestination)
                {
                    // drop ressource
                    Master.drop(new Ressource(activeTransaction.type, position), hostile);
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

            batch.Draw(hostile ? Master.HOSTILEunitTextures[textureNum < maxTexNum / 2 ? 6 : 7] : Master.unitTextures[textureNum < maxTexNum / 2 ? 6 : 7], position, null, Color.White,
                0f, new Vector2(Master.unitTextures[6].Width / 2f, Master.unitTextures[6].Height / 2f),
                Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 2f));

            if(currentState == ETransporterState.MoveToDestination)
            {
                batch.Draw(Master.ressourceTextures[(int)activeTransaction.type], position - new Vector2(.05f,0), null, Color.White,
                    0f, new Vector2(5), new Vector2(.02f, .033f), SpriteEffects.None, Master.calculateDepth(position.Y + 2f));
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

    public enum EFighterAction
    {
        None,
        Move,
        Fight
    }

    public enum EGFighterFightMode
    {
        ClosestDistance = 0,
        MinimumHPPercentage = 1,
        MaximumTotalHP = 2,
        OnlyBuildings = 3
    }
}
