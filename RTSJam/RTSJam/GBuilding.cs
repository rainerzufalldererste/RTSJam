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
        private bool v;
        private Vector2 zero;

        public BMainBuilding(Vector2 position, bool hostile)
        {
            this.position = position;
            this.hostile = hostile;
            this.type = EBuildingType.Main;
        }

        public override void draw(SpriteBatch batch)
        {
            batch.Draw(Master.buildingTextures[5],
                position, null, Color.White, 0f,
                new Vector2(15f, 22.5f), Master.scaler, SpriteEffects.None, Master.calculateDepth(position.Y + 1.1f));
        }
    }
}