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

        public virtual void remove()
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
        BigWar,                 // 0
        GoldBarer,              // 1
        IronBarer,              // 2
        Main,                   // 3
        MinerMaker,             // 4
        PlantMaker,             // 5
        PowerPlant,             // 6
        PurPurPurifier,         // 7
        Pylon,                  // 8
        SmallWar,               // 9
        StoneFiltrationStation, // 10
        University,             // 11
        WaterPurifier,          // 12
        Construction            // 13
    }

    public class BUnderConstruction : GBuilding
    {
        int maxcooldown, cooldown;
        private int[] ressourcesNeeded;
        GObjBuild gobjb;
        private GBuilding futurePlans;
        bool completedCollecting = false;

        public BUnderConstruction(GBuilding whenIGrowUpIllBecomeA, GObjBuild gobjbuild, int timeNeeded, int[] ressourcesNeeded)
        {
            this.position = whenIGrowUpIllBecomeA.position;
            this.hostile = whenIGrowUpIllBecomeA.hostile;
            this.type = EBuildingType.Construction;
            this.size = whenIGrowUpIllBecomeA.size;
            this.gobjb = gobjbuild;
            this.futurePlans = whenIGrowUpIllBecomeA;

            maxcooldown = timeNeeded;
            cooldown = timeNeeded;

            this.ressourcesNeeded = ressourcesNeeded;

            for (int i = 0; i < ressourcesNeeded.Length; i++)
            {
                for (int j = 0; j < ressourcesNeeded[i]; j++)
                {
                    TransportHandler.placeNeed((ERessourceType)i, new TransportBuildingHandle(this, this.position));
                }
            }
        }

        public override void update()
        {
            if (completedCollecting)
            {
                cooldown--;
            }

            if(cooldown == 0)
            {
                Master.buildings.Remove(this);
                Master.buildings.Add(futurePlans);
                gobjb.building = futurePlans;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[13 + size],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
        }

        public override void addRessource(ERessourceType rtype)
        {
            base.addRessource(rtype);

            bool ressourcesComplete = true;

            for (int i = 0; i < ressources.Length; i++)
            {
                if (ressourcesNeeded[i] != ressources[i])
                    ressourcesComplete = false;
            }

            if (ressourcesComplete)
                completedCollecting = true;
        }
    }

    public class BMainBuilding : GBuilding
    {
        const int maxcooldown = 60 * 10;
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

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
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
        const int maxcooldown = 60 * 15;
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

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
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
        int orderedStone = 0;
        ParticleSystem particleSystem = new ParticleSystem();

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

                if (cooldown % 15 == 0)
                {
                    particleSystem.addStoneSmokeParticle(position + new Vector2(.7f, -.4f));
                }
            }

            if (ressources[(int)ERessourceType.Stone] + orderedStone <= 5)
            {
                TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, position));
                orderedStone++;
            }

            if(cooldown <= 0 && ressources[(int)ERessourceType.Stone] > 0)
            {
                ressources[(int)ERessourceType.Stone]--;
                Master.addOffer(new Ressource(ERessourceType.Iron, position + new Vector2(0, 2)));
                cooldown = maxcooldown;
            }
        }

        public override void addRessource(ERessourceType rtype)
        {
            base.addRessource(rtype);

            if (rtype == ERessourceType.Stone)
            {
                orderedStone--;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[texcount >= maxtexcount / 2 ? 10 : 11],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));

            particleSystem.update(batch);

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
        }
    }

    public class BIronMelting : GStoppableBuilding
    {
        const int maxcooldown = 60 * 5;
        int cooldown = maxcooldown;
        int orderedCoal = 0, orderedIron = 0;
        ParticleSystem particleSystem = new ParticleSystem();

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

            if (ressources[(int)ERessourceType.Iron] > 1 && ressources[(int)ERessourceType.Coal] > 0 && cooldown > 0)
            {
                cooldown--;

                if(cooldown % 8 == 0)
                {
                    particleSystem.addDarkSmokeParticle(position + new Vector2(.15f,-1.2f));
                }
            }

            if (ressources[(int)ERessourceType.Iron] + orderedIron <= 10)
            {
                TransportHandler.placeNeed(ERessourceType.Iron, new TransportBuildingHandle(this, position));
                orderedIron++;
            }

            if (ressources[(int)ERessourceType.Coal] + orderedCoal <= 5)
            {
                TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
                orderedCoal++;
            }

            if (cooldown <= 0 && ressources[(int)ERessourceType.Iron] > 1 && ressources[(int)ERessourceType.Coal] > 0)
            {
                ressources[(int)ERessourceType.Iron]-=2;
                ressources[(int)ERessourceType.Coal]--;

                Master.addOffer(new Ressource(ERessourceType.IronBar, position + new Vector2(0, 2)));
                cooldown = maxcooldown;
            }
        }

        public override void addRessource(ERessourceType rtype)
        {
            base.addRessource(rtype);

            if (rtype == ERessourceType.Iron)
            {
                orderedIron--;
            }
            else if(rtype == ERessourceType.Coal)
            {
                orderedCoal--;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[2],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));

            particleSystem.update(batch);

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
        }
    }

    public class BGoldMelting : GStoppableBuilding
    {
        const int maxcooldown = 60 * 20;
        int cooldown = maxcooldown;
        int orderedCoal = 0, orderedGold = 0;
        ParticleSystem particleSystem = new ParticleSystem();

        public BGoldMelting(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.GoldBarer;
            this.size = 2;
        }

        public override void update()
        {
            if (stopped)
                return;

            if (ressources[(int)ERessourceType.Gold] > 0 && ressources[(int)ERessourceType.Coal] > 3 && cooldown > 0)
            {
                cooldown--;

                if(cooldown % 12 == 0)
                {
                    particleSystem.addDarkSmokeParticle(position + new Vector2(-.1f,-.3f));
                }
            }

            if (ressources[(int)ERessourceType.Gold] + orderedGold <= 5)
            {
                TransportHandler.placeNeed(ERessourceType.Gold, new TransportBuildingHandle(this, position));
                orderedGold++;
            }

            if (ressources[(int)ERessourceType.Coal] + orderedCoal <= 20)
            {
                TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
                orderedCoal++;
            }

            if (cooldown <= 0 && ressources[(int)ERessourceType.Gold] > 0 && ressources[(int)ERessourceType.Coal] > 3)
            {
                ressources[(int)ERessourceType.Gold]--;
                ressources[(int)ERessourceType.Coal]-=4;

                Master.addOffer(new Ressource(ERessourceType.GoldBar, position + new Vector2(0, 2)));
                cooldown = maxcooldown;
            }
        }

        public override void addRessource(ERessourceType rtype)
        {
            base.addRessource(rtype);

            if (rtype == ERessourceType.Gold)
            {
                orderedGold--;
            }
            else if (rtype == ERessourceType.Coal)
            {
                orderedCoal--;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[1],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));

            particleSystem.update(batch);

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
        }
    }

    public class BWaterPurifier : GPoweredBuilding
    {
        const int maxcooldown = 60 * 30;
        int cooldown = maxcooldown;
        int orderedStone = 0, orderedIce = 0;

        public BWaterPurifier(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.WaterPurifier;
            this.size = 2;
        }

        public override void update()
        {
            if (stopped)
                return;

            if (ressources[(int)ERessourceType.Ice] > 0 && ressources[(int)ERessourceType.Stone] > 1 && cooldown > 0)
            {
                if(powered)
                {
                    cooldown--;
                }

                cooldown--;
            }

            if (ressources[(int)ERessourceType.Ice] + orderedIce <= 5)
            {
                TransportHandler.placeNeed(ERessourceType.Ice, new TransportBuildingHandle(this, position));
                orderedIce++;
            }

            if (ressources[(int)ERessourceType.Stone] + orderedStone <= 10)
            {
                TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, position));
                orderedStone++;
            }

            if (cooldown <= 0 && ressources[(int)ERessourceType.Ice] > 0 && ressources[(int)ERessourceType.Stone] > 1)
            {
                ressources[(int)ERessourceType.Ice]--;
                ressources[(int)ERessourceType.Stone]-=2;

                Master.addOffer(new Ressource(ERessourceType.Water, position + new Vector2(0, 2)));
                cooldown = maxcooldown;
            }
        }

        public override void addRessource(ERessourceType rtype)
        {
            base.addRessource(rtype);

            if (rtype == ERessourceType.Ice)
            {
                orderedIce--;
            }
            else if (rtype == ERessourceType.Stone)
            {
                orderedStone--;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            if (!powered)
            {
                batch.Draw(Master.fxTextures[3], position, null, Color.White, 0f, new Vector2(5f), new Vector2(.0495f, .075f), SpriteEffects.None, 0f);
            }

            batch.Draw(Master.buildingTextures[13],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
        }
    }

    public class BPlantage : GPoweredBuilding
    {
        const int maxcooldown = 60 * 30;
        int cooldown = maxcooldown;
        int orderedStone = 0, orderedWater = 0;

        public BPlantage(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.PlantMaker;
            this.size = 2;
        }

        public override void update()
        {
            if (stopped || !powered)
                return;

            if (ressources[(int)ERessourceType.Water] > 0 && ressources[(int)ERessourceType.Stone] > 2 && cooldown > 0)
            {
                cooldown--;
            }

            if (ressources[(int)ERessourceType.Water] + orderedWater <= 5)
            {
                TransportHandler.placeNeed(ERessourceType.Water, new TransportBuildingHandle(this, position));
                orderedWater++;
            }

            if (ressources[(int)ERessourceType.Stone] + orderedStone <= 15)
            {
                TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, position));
                orderedStone++;
            }

            if (cooldown <= 0 && ressources[(int)ERessourceType.Water] > 0 && ressources[(int)ERessourceType.Stone] > 2)
            {
                ressources[(int)ERessourceType.Water]--;
                ressources[(int)ERessourceType.Stone] -= 3;

                Master.addOffer(new Ressource(ERessourceType.Food, position + new Vector2(0, 2)));
                cooldown = maxcooldown;
            }
        }

        public override void addRessource(ERessourceType rtype)
        {
            base.addRessource(rtype);

            if (rtype == ERessourceType.Water)
            {
                orderedWater--;
            }
            else if (rtype == ERessourceType.Stone)
            {
                orderedStone--;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            if (!powered)
            {
                batch.Draw(Master.fxTextures[3], position, null, Color.White, 0f, new Vector2(5f), new Vector2(.0495f, .075f), SpriteEffects.None, 0f);
            }

            batch.Draw(Master.buildingTextures[5],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
        }
    }

    public class BPurPurPurifier : GPoweredBuilding
    {
        const int maxcooldown = 60 * 30;
        int cooldown = maxcooldown;
        int orderedIce = 0, orderedRawPurPur = 0;

        public BPurPurPurifier(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.PurPurPurifier;
            this.size = 2;
        }

        public override void update()
        {
            if (stopped || !powered)
                return;

            if (ressources[(int)ERessourceType.RawPurPur] > 0 && ressources[(int)ERessourceType.Ice] > 2 && cooldown > 0)
            {
                cooldown--;
            }

            if (ressources[(int)ERessourceType.RawPurPur] + orderedRawPurPur <= 5)
            {
                TransportHandler.placeNeed(ERessourceType.RawPurPur, new TransportBuildingHandle(this, position));
                orderedRawPurPur++;
            }

            if (ressources[(int)ERessourceType.Ice] + orderedIce <= 15)
            {
                TransportHandler.placeNeed(ERessourceType.Ice, new TransportBuildingHandle(this, position));
                orderedIce++;
            }

            if (cooldown <= 0 && ressources[(int)ERessourceType.RawPurPur] > 0 && ressources[(int)ERessourceType.Ice] > 2)
            {
                ressources[(int)ERessourceType.RawPurPur]--;
                ressources[(int)ERessourceType.Ice] -= 3;

                Master.addOffer(new Ressource(ERessourceType.PurPur, position + new Vector2(0, 2)));
                cooldown = maxcooldown;
            }
        }

        public override void addRessource(ERessourceType rtype)
        {
            base.addRessource(rtype);

            if (rtype == ERessourceType.RawPurPur)
            {
                orderedRawPurPur--;
            }
            else if (rtype == ERessourceType.Ice)
            {
                orderedIce--;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            if (!powered)
            {
                batch.Draw(Master.fxTextures[3], position, null, Color.White, 0f, new Vector2(5f), new Vector2(.0495f, .075f), SpriteEffects.None, 0f);
            }

            batch.Draw(Master.buildingTextures[7],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
        }
    }

    public class GPoweredBuilding : GStoppableBuilding
    {
        public bool powered = false;
        public List<GPoweredBuilding> connectedBuildings = new List<GPoweredBuilding>();

        public void addToConnectedBuildings(GPoweredBuilding gpb)
        {
            if(!connectedBuildings.Contains(gpb))
                connectedBuildings.Add(gpb);
        }

        public override void remove()
        {
            for (int i = 0; i < Master.buildings.Count; i++)
            {
                if(Master.buildings[i] is GPoweredBuilding)
                {
                    ((GPoweredBuilding)Master.buildings[i]).connectedBuildings.Remove(this);
                }
            }

            base.remove();
        }
    }

    public class BPowerPlant : GPoweredBuilding
    {
        const int maxcooldown = 60 * 20;
        int cooldown = maxcooldown;
        int orderedWater = 0, orderedCoal = 0;

        ParticleSystem particleSystem = new ParticleSystem();

        public BPowerPlant(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.PowerPlant;
            this.size = 2;
        }

        public override void update()
        {
            if (stopped)
            {
                powered = false;

                return;
            }

            if (ressources[(int)ERessourceType.Coal] > 3 && ressources[(int)ERessourceType.Water] > 0 && cooldown > 0)
            {
                cooldown--;

                if (cooldown % 9 == 0)
                    particleSystem.addLightSmokeParticles(position + new Vector2(.3f,-.2f));
            }

            if (ressources[(int)ERessourceType.Coal] > 3 && ressources[(int)ERessourceType.Water] > 0)
            {
                powered = true;
            }
            else
            {
                powered = false;
            }

            if (ressources[(int)ERessourceType.Coal] + orderedCoal <= 20)
            {
                TransportHandler.placeNeed(ERessourceType.Coal, new TransportBuildingHandle(this, position));
                orderedCoal++;
            }

            if (ressources[(int)ERessourceType.Water] + orderedWater <= 5)
            {
                TransportHandler.placeNeed(ERessourceType.Water, new TransportBuildingHandle(this, position));
                orderedWater++;
            }

            if (cooldown <= 0 && ressources[(int)ERessourceType.Coal] > 3 && ressources[(int)ERessourceType.Water] > 0)
            {
                ressources[(int)ERessourceType.Coal]-=4;
                ressources[(int)ERessourceType.Water]--;
                cooldown = maxcooldown;
                powered = true;
            }
        }

        public override void addRessource(ERessourceType rtype)
        {
            base.addRessource(rtype);

            if (rtype == ERessourceType.Coal)
            {
                orderedCoal--;
            }
            else if (rtype == ERessourceType.Water)
            {
                orderedWater--;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            if (!powered)
            {
                batch.Draw(Master.fxTextures[3], position, null, Color.White, 0f, new Vector2(5f), new Vector2(.0495f, .075f), SpriteEffects.None, 0f);
            }

            batch.Draw(Master.buildingTextures[6],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));

            particleSystem.update(batch);

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.52f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.5f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
        }
    }

    public class BPylon : GPoweredBuilding
    {
        int powerlevel = 0;
        const int maxpowerlevel = 30;

        public BPylon(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.Pylon;
            this.size = 1;
        }

        public override void update()
        {
            if (stopped)
            {
                powered = false;

                for (int i = 0; i < connectedBuildings.Count; i++)
                {
                    if (!(connectedBuildings[i] is BPowerPlant || connectedBuildings[i] is BPylon))
                        connectedBuildings[i].powered = false;
                }

                return;
            }

            connectedBuildings.Clear();

            for (int i = 0; i < Master.buildings.Count; i++)
            {
                if (Master.buildings[i] is GPoweredBuilding && Master.buildings[i].hostile == hostile && (position - Master.buildings[i].position).Length() <= Master.powerRange)
                {
                    connectedBuildings.Add((GPoweredBuilding)Master.buildings[i]);
                }
            }

            powered = false;
            powerlevel--;

            for (int i = 0; i < connectedBuildings.Count; i++)
            {
                if (connectedBuildings[i] is BPowerPlant)
                {
                    if (connectedBuildings[i].powered)
                    {
                        powered = true;
                        powerlevel = maxpowerlevel;
                        break;
                    }
                }
                else if (connectedBuildings[i] is BPylon)
                {
                    if (((BPylon)connectedBuildings[i]).powerlevel > powerlevel)
                        powerlevel = ((BPylon)connectedBuildings[i]).powerlevel - 1;
                }
            }

            if (powerlevel > 0)
            {
                powered = true;
            }


            for (int i = 0; i < connectedBuildings.Count; i++)
            {
                if (connectedBuildings[i] is BPowerPlant || connectedBuildings[i] is BPylon)
                    continue;

                connectedBuildings[i].powered = powered;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            if (!powered)
            {
                batch.Draw(Master.fxTextures[3], position, null, Color.White, 0f, new Vector2(5f), new Vector2(.0495f, .075f), SpriteEffects.None, 0f);
            }

            batch.Draw(Master.buildingTextures[8],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));
        }
    }

    public class BUniversity : GBuilding
    {
        int maxcooldown = 0;
        int cooldown = 0;
        int[] ressourcesNeeded = new int[11];
        bool completedCollecting = false;
        ETechnology developingTechnology = ETechnology.None;

        public BUniversity(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.University;
            this.size = 2;
        }

        public override void update()
        {
            if (completedCollecting && cooldown > 0)
            {
                cooldown--;
            }

            if (completedCollecting && cooldown <= 0)
            {
                Master.DevelopedTechnologies |= developingTechnology;
                ressourcesNeeded = new int[11];
            }
        }

        public override void addRessource(ERessourceType rtype)
        {
            base.addRessource(rtype);

            bool ressourcesComplete = true;

            for (int i = 0; i < ressources.Length; i++)
            {
                if (ressourcesNeeded[i] != ressources[i])
                    ressourcesComplete = false;
            }

            if (ressourcesComplete)
                completedCollecting = true;
        }

        /// <summary>
        /// COST: 20 GOLDBARS, 50 IRONBARS, 50 STONE, 20 FOOD
        /// </summary>
        public void developBigWarStation()
        {
            if(cooldown <= 0 && (Master.discoveryStarted & ETechnology.BigWarStation) == 0)
            {
                ressourcesNeeded[(int)ERessourceType.GoldBar] += 20;
                ressourcesNeeded[(int)ERessourceType.IronBar] += 50;
                ressourcesNeeded[(int)ERessourceType.Stone] += 50;
                ressourcesNeeded[(int)ERessourceType.Food] += 20;

                for (int i = 0; i < 20; i++)
                {
                    TransportHandler.placeNeed(ERessourceType.GoldBar, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.Food, new TransportBuildingHandle(this, this.position));
                }

                for (int i = 0; i < 50; i++)
                {
                    TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, this.position));
                }

                cooldown = 240 * 60;
                maxcooldown = 240 * 60;
            }
        }

        /// <summary>
        /// COST: 20 GOLDBARS, 20 IRONBARS, 20 STONE, 20 FOOD, 20 RAWPURPUR
        /// </summary>
        public void developPurPurPurifier()
        {
            if (cooldown <= 0 && (Master.discoveryStarted & ETechnology.PurPurPurifier) == 0)
            {
                ressourcesNeeded[(int)ERessourceType.GoldBar] += 20;
                ressourcesNeeded[(int)ERessourceType.IronBar] += 20;
                ressourcesNeeded[(int)ERessourceType.Stone] += 20;
                ressourcesNeeded[(int)ERessourceType.Food] += 20;
                ressourcesNeeded[(int)ERessourceType.RawPurPur] += 20;

                for (int i = 0; i < 20; i++)
                {
                    TransportHandler.placeNeed(ERessourceType.GoldBar, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.Food, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.RawPurPur, new TransportBuildingHandle(this, this.position));
                }

                cooldown = 120 * 60;
                maxcooldown = 120 * 60;
            }
        }

        /// <summary>
        /// COST: 5 GOLDBARS, 25 IRONBARS, 25 STONE, 10 FOOD, 25 IRON
        /// </summary>
        public void developBiggerFighter()
        {
            if (cooldown <= 0 && (Master.discoveryStarted & ETechnology.BiggerFighter) == 0)
            {
                ressourcesNeeded[(int)ERessourceType.GoldBar] += 5;
                ressourcesNeeded[(int)ERessourceType.IronBar] += 25;
                ressourcesNeeded[(int)ERessourceType.Stone] += 25;
                ressourcesNeeded[(int)ERessourceType.Food] += 10;
                ressourcesNeeded[(int)ERessourceType.Iron] += 25;

                for (int i = 0; i < 25; i++)
                {
                    TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.Iron, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.Stone, new TransportBuildingHandle(this, this.position));
                }

                for (int i = 0; i < 10; i++)
                {
                    TransportHandler.placeNeed(ERessourceType.Food, new TransportBuildingHandle(this, this.position));
                }

                for (int i = 0; i < 5; i++)
                {
                    TransportHandler.placeNeed(ERessourceType.GoldBar, new TransportBuildingHandle(this, this.position));
                }

                cooldown = 90 * 60;
                maxcooldown = 90 * 60;
            }
        }

        /// <summary>
        /// COST: 50 GOLDBARS, 50 FOOD, 25 IRONBARS, 25 PURPUR
        /// </summary>
        public void developCannonTank()
        {
            ressourcesNeeded[(int)ERessourceType.GoldBar] += 50;
            ressourcesNeeded[(int)ERessourceType.Food] += 50;
            ressourcesNeeded[(int)ERessourceType.IronBar] += 25;
            ressourcesNeeded[(int)ERessourceType.PurPur] += 25;

            if (cooldown <= 0 && (Master.discoveryStarted & ETechnology.BigCanonTank) == 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    TransportHandler.placeNeed(ERessourceType.GoldBar, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.Food, new TransportBuildingHandle(this, this.position));
                }

                for (int i = 0; i < 25; i++)
                {
                    TransportHandler.placeNeed(ERessourceType.IronBar, new TransportBuildingHandle(this, this.position));
                    TransportHandler.placeNeed(ERessourceType.PurPur, new TransportBuildingHandle(this, this.position));
                }

                cooldown = 480 * 60;
                maxcooldown = 480 * 60;
            }
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[(int)type],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));

            if (cooldown != maxcooldown && cooldown > 0)
            {
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.Black, 0f, new Vector2(.5f), new Vector2(.80f, .2f), SpriteEffects.None, 0.01f);
                batch.Draw(Master.pixel, position - new Vector2(0, -.8f), null, Color.White, 0f, new Vector2(.5f), new Vector2(.78f * (1f - (float)cooldown / (float)maxcooldown), .18f), SpriteEffects.None, 0.009f);
            }
        }

        internal void discoverSoftMiner()
        {
            if ((Master.discoveryStarted & ETechnology.Softminer) != ETechnology.Softminer)
            {
                Master.discoveryStarted |= ETechnology.Softminer;
            }
        }
    }
}