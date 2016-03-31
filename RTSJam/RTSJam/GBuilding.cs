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
    public class GBuilding
    {
        public EBuildingType type;

        public int[] ressources = new int[11];
        public bool doesNotExist = false;
        public bool hostile = false;
        public Vector2 position;
        public int size = 2;

        public virtual void addRessource(ERessourceType rtype)
        {
            ressources[(int)rtype]++;
        }

        public void remove()
        {
            Master.buildings.Remove(this);
            doesNotExist = true;
        }

        public virtual void update() { }
        public virtual void draw(SpriteBatch batch) { }
    }

    public class GStoppableBuilding : GBuilding
    {
        public bool stopped = false;
    }

    public enum EBuildingType
    {
        BigWar,
        GoldBarer,
        IronBarer,
        Main,
        MinerMaker,
        PlantMaker,
        PowerPlant,
        PurPurPurifier,
        Pylon,
        SmallWar,
        StoneFiltrationStation,
        University,
        WaterPurifier
    }

    public class BMainBuilding : GBuilding
    {
        const int maxcooldown = 60 * 15;
        public int transportsLeft = 0, cooldown = maxcooldown;

        public BMainBuilding(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.Main;
            this.size = 2;
        }

        public override void update()
        {
            if (transportsLeft > 0 && cooldown > 0)
            {
                cooldown--;
            }

            if (ressources[(int)ERessourceType.Iron] >= 2 && ressources[(int)ERessourceType.IronBar] >= 1 && cooldown <= 0)
            {
                cooldown = maxcooldown;
                ressources[(int)ERessourceType.Iron] -= 2;
                ressources[(int)ERessourceType.IronBar] -= 1;
                transportsLeft--;

                Master.transports.Add(new GTransport(this.position, false));
            }
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[3],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));
        }

        internal void buildTransporter()
        {
            transportsLeft++;
            TransportHandler.placeNeed(ERessourceType.Iron, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.Iron, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
        }
    }

    public class BMinerFactory : GBuilding
    {
        const int maxcooldown = 60 * 20;
        public List<int> minersLeft = new List<int>();
        int cooldown = maxcooldown;

        public BMinerFactory(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.MinerMaker;
            this.size = 2;
        }

        public override void update()
        {
            if (minersLeft.Count > 0 && cooldown > 0)
            {
                cooldown--;
            }

            if (minersLeft.Count > 0 && cooldown <= 0)
            {
                if (minersLeft[0] == 0 && ressources[(int)ERessourceType.IronBar] >= 2 && ressources[(int)ERessourceType.Coal] >= 3)
                {
                    cooldown = maxcooldown;
                    ressources[(int)ERessourceType.IronBar] -= 2;
                    ressources[(int)ERessourceType.Coal] -= 3;
                    minersLeft.RemoveAt(0);

                    Master.units.Add(new GMiner(position + new Vector2(0, 2), false, false));
                }
                else if (minersLeft[0] == 1 && ressources[(int)ERessourceType.IronBar] >= 4 && ressources[(int)ERessourceType.Stone] >= 2)
                {
                    cooldown = maxcooldown;
                    ressources[(int)ERessourceType.IronBar] -= 4;
                    ressources[(int)ERessourceType.Stone] -= 2;
                    minersLeft.RemoveAt(0);

                    Master.units.Add(new GMiner(position + new Vector2(0, 2), false, true));
                }
                else if (minersLeft[0] == 2 && ressources[(int)ERessourceType.IronBar] >= 8 && ressources[(int)ERessourceType.Coal] >= 4 && ressources[(int)ERessourceType.Stone] >= 2)
                {
                    cooldown = maxcooldown;

                    ressources[(int)ERessourceType.IronBar] -= 8;
                    ressources[(int)ERessourceType.Coal] -= 4;
                    ressources[(int)ERessourceType.Stone] -= 2;
                    minersLeft.RemoveAt(0);

                    Master.DevelopedTechnologies |= ETechnology.Softminer;
                }
            }
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[4],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));
        }

        internal void buildMiner()
        {
            minersLeft.Add(0);
            TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
        }

        internal void buildSoftMiner()
        {
            minersLeft.Add(1);
            TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, position));
            TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, position));
        }

        internal void discoverSoftMiner()
        {
            if ((Master.discoveryStarted & ETechnology.Softminer) != ETechnology.Softminer)
            {
                Master.discoveryStarted |= ETechnology.Softminer;

                minersLeft.Add(2);
                TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, position));
                TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, position));
            }
        }
    }

    public class BStoneFiltration : GStoppableBuilding
    {
        const int maxcooldown = 60 * 5;
        int cooldown = maxcooldown;
        int texcount = 0;
        const int maxtexcount = 20;

        public BStoneFiltration(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.StoneFiltrationStation;
            this.size = 2;
        }

        public override void update()
        {
            if (stopped)
                return;

            texcount++;

            if (texcount > maxtexcount)
                texcount = 0;

            if (ressources[(int)ERessourceType.Stone] > 0 && cooldown > 0)
            {
                cooldown--;
            }

            if (ressources[(int)ERessourceType.Stone] <= 5)
            {
                TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, position));
            }

            if(cooldown <= 0 && ressources[(int)ERessourceType.Stone] > 0)
            {
                ressources[(int)ERessourceType.Stone]--;
                Master.addOffer(new Ressource(ERessourceType.Iron, position + new Vector2(0, 2)));
            }
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[texcount >= maxtexcount / 2 ? 10 : 11],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));
        }
    }

    public class BIronMelting : GStoppableBuilding
    {
        const int maxcooldown = 60 * 5;
        int cooldown = maxcooldown;

        public BIronMelting(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.IronBarer;
            this.size = 2;
        }

        public override void update()
        {
            if (stopped)
                return;

            if (ressources[(int)ERessourceType.Iron] > 0 && ressources[(int)ERessourceType.Coal] > 0 && cooldown > 0)
            {
                cooldown--;
            }

            if (ressources[(int)ERessourceType.Iron] <= 10)
            {
                TransportHandler.placeNeed(ERessourceType.Iron, new TransportBuildingHandle(this, position));
            }

            if (ressources[(int)ERessourceType.Coal] <= 5)
            {
                TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
            }

            if (cooldown <= 0 && ressources[(int)ERessourceType.Iron] > 1 && ressources[(int)ERessourceType.Coal] > 0)
            {
                ressources[(int)ERessourceType.Iron]-=2;
                ressources[(int)ERessourceType.Coal]--;

                Master.addOffer(new Ressource(ERessourceType.IronBar, position + new Vector2(0, 2)));
            }
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[2],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));
        }
    }
}