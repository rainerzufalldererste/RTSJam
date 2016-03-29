using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSJam
{
    public class GBuilding
    {
        public EBuildingType type;
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
}