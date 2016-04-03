using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace RTSJam
{
    public class MenuHandler
    {
        private string outString = "";
        private static KeyboardState ks;
        private static KeyboardState lks;
        int menuState = 0;
        private string resString = "";

        public MenuHandler()
        {
        }

        public void update(KeyboardState ks, KeyboardState lks, MouseState ms, MouseState lms, ref List<GUnit> selectedUnits, ref bool selectionContainsTroops, GBuilding selectedBuilding, ref int placeBuilding, ref int buildingSize, Rectangle selectionA, bool hack)
        {
            MenuHandler.ks = ks;
            MenuHandler.lks = lks;
            setResToGlobalTransportVolumes();

            if ((ks.IsKeyDown(Keys.Escape) && lks.IsKeyUp(Keys.Escape) && (selectedUnits.Count > 0 || selectedBuilding != null || placeBuilding > -1)) || hack)
            {
                selectedUnits.Clear();
                selectionContainsTroops = false;
                placeBuilding = -1;
                selectedBuilding = null;
            }
            else
            {
                // if placing building
                if (placeBuilding >= 0)
                {
                    if (ms.LeftButton == ButtonState.Pressed && lms.LeftButton == ButtonState.Released)
                    {
                        if (buildingSize == 1)
                        {
                            int chunk, xobj, yobj;
                            GObject gobj = Master.getGObjAt(new Vector2(selectionA.X, selectionA.Y), out chunk, out xobj, out yobj);
                            
                            // if powered
                            if ((placeBuilding == 5 || placeBuilding == 7 || placeBuilding == 8 || placeBuilding == 13))
                            {
                                for (int i = 0; i < Master.buildings.Count; i++)
                                {
                                    if ((Master.buildings[i] is BPylon || Master.buildings[i] is BPowerPlant || 
                                        (Master.buildings[i] is BUnderConstruction &&
                                                (((BUnderConstruction)Master.buildings[i]).futurePlans.type == EBuildingType.Pylon ||
                                                ((BUnderConstruction)Master.buildings[i]).futurePlans.type == EBuildingType.PowerPlant)))
                                        && (new Vector2(selectionA.X, selectionA.Y) - Master.buildings[i].position).Length() <= Master.powerRange)
                                        goto SUFFICIENT_POWER;
                                }

                                goto NOT_WORKED_POWER;
                            }

                            SUFFICIENT_POWER:

                            if (gobj is GGround)
                            {
                                // place building

                                switch (placeBuilding)
                                {
                                    // ONLY SINGLE SIZE BUILDINGS!!!
                                    case 8:
                                        Master.AddBuilding(new BPylon(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                        outString = "A Pylon distributes Energy created by a PowerPlant.";
                                        break;

                                    default:
                                        throw new Exception("ONLY SINGLE SIZE BUILDINGS, PLEASE!");
                                }

                                placeBuilding = -1;
                            }
                        }
                        else if (buildingSize == 2)
                        {
                            int chunk, xobj, yobj;
                            GObject gobj = Master.getGObjAt(new Vector2(selectionA.X, selectionA.Y), out chunk, out xobj, out yobj);

                            // if powered
                            for (int i = 0; i < Master.buildings.Count; i++)
                            {
                                if ((Master.buildings[i] is BPylon || Master.buildings[i] is BPowerPlant ||
                                    (Master.buildings[i] is BUnderConstruction &&
                                            (((BUnderConstruction)Master.buildings[i]).futurePlans.type == EBuildingType.Pylon ||
                                            ((BUnderConstruction)Master.buildings[i]).futurePlans.type == EBuildingType.PowerPlant)))
                                    && (new Vector2(selectionA.X, selectionA.Y) - Master.buildings[i].position).Length() <= Master.powerRange)
                                    goto SUFFICIENT_POWER;
                            }

                            SUFFICIENT_POWER:

                            // UGLY CODE INCOMING, DUDE

                            if (gobj is GGround)
                            {
                                gobj = Master.getGObjAt(new Vector2(selectionA.X, selectionA.Y + 1));

                                if (gobj is GGround)
                                {
                                    gobj = Master.getGObjAt(new Vector2(selectionA.X + 1, selectionA.Y));

                                    if (gobj is GGround)
                                    {
                                        gobj = Master.getGObjAt(new Vector2(selectionA.X + 1, selectionA.Y + 1));

                                        if (gobj is GGround)
                                        {
                                            // place building

                                            switch (placeBuilding)
                                            {
                                                case 0:
                                                    Master.AddBuilding(new BBigWar(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A Big Tank Manifacturing Station is able to build huge Battleships.";
                                                    break;

                                                case 1:
                                                    Master.AddBuilding(new BGoldMelting(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A Gold Smelter uses Gold and Coal to produce GoldIngots.";
                                                    break;

                                                case 2:
                                                    Master.AddBuilding(new BIronMelting(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A Gold Smelter uses Gold and Iron to produce IronIngots.";
                                                    break;

                                                case 3:
                                                    Master.AddBuilding(new BMainBuilding(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A Main Building is used to produce Transporters.";
                                                    break;

                                                case 4:
                                                    Master.AddBuilding(new BMinerFactory(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A Miner Factory produces regular and special Miners. Special Miners can (only) mine Rare Ores but have to be developed first.";
                                                    break;

                                                case 5:
                                                    Master.AddBuilding(new BPlantage(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A Plantage produces Food out of Water and Stones. THIS BUILDING NEEDS ENERGY TO WORK.";
                                                    break;

                                                case 6:
                                                    Master.AddBuilding(new BPowerPlant(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A PowerPlant produces Energy out of Water and Coal. Use Pylons to distribute it's Energy.";
                                                    break;

                                                case 7:
                                                    Master.AddBuilding(new BPurPurPurifier(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A PurPur Purifier produces PurPur out of Raw PurPur and Ice. THIS BUILDING NEEDS ENERGY TO WORK.";
                                                    break;

                                                    // 8 is single sized: pylon

                                                case 9:
                                                    Master.AddBuilding(new BSmallWar(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A Fighter Workshop is used to build Fighters.";
                                                    break;

                                                case 10:
                                                case 11:
                                                    Master.AddBuilding(new BStoneFiltration(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A StonePurifier transforms Stone into Iron.";
                                                    break;

                                                case 12:
                                                    Master.AddBuilding(new BUniversity(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A University is used to build better Technology such as PurPur Purifiers, bigger or better Fighters.";
                                                    break;

                                                case 13:
                                                    Master.AddBuilding(new BWaterPurifier(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    outString = "A WaterPurifier produces Water out of Ice and Coal. THIS BUILDING WORKS BETTER WITH ENERGY.";
                                                    break;

                                                default:
                                                    throw new Exception("ONLY DOUBLE SIZE BUILDINGS, PLEASE!");
                                            }

                                            placeBuilding = -1;
                                        }
                                    }

                                }
                            }

                            if(placeBuilding > -1)
                            {
                                goto NOT_WORKED_GENERAL;
                            }
                        }
                        else
                        {
                            throw new Exception("What are you doing?!");
                        }
                    }
                }
                // if building selected
                else if (selectedBuilding != null)
                {
                    switch (((BUnderConstruction)selectedBuilding).futurePlans.type)
                    {
                        case EBuildingType.BigWar:
                            outString = "Big Tank Manifacturing Station | Used to build huge flying War Tanks";
                            break;

                        case EBuildingType.GoldIngoter:
                            outString = "Gold Smelter | Transforms Gold and Coal into GoldIngots";
                            break;

                        case EBuildingType.IronIngoter:
                            outString = "Iron Smelter | Transforms Iron and Coal into IronIngots";
                            break;

                        case EBuildingType.Main:
                            outString = "Main Building | Used to build Transporters";
                            break;

                        case EBuildingType.MinerMaker:
                            outString = "Miner Factory | Used to build Miners\nRegular Miners only mine Stone and Coal, Special Miners only mine Rare Ores";
                            break;

                        case EBuildingType.PlantMaker:
                            outString = "Plantage | Transforms Water and Stones into Food";
                            break;

                        case EBuildingType.PowerPlant:
                            outString = "Power Plant | Transforms Coal and Water into Energy\nUse pylons to make the energy available to other Buildings.";
                            break;

                        case EBuildingType.PurPurPurifier:
                            outString = "PurPur Purifier | Transforms Raw PurPur and Ice into PurPur";
                            break;

                        case EBuildingType.Pylon:
                            outString = "Pylon | Distributes power to other Buildings.";
                            break;

                        case EBuildingType.SmallWar:
                            outString = "Fighter Workshop | Used to build Fighters";
                            break;

                        case EBuildingType.StoneFiltrationStation:
                            outString = "Stone Filtrator | Filtrates Iron out of Stones";
                            break;

                        case EBuildingType.University:
                            outString = "University | Used to develop better Technologies";
                            break;

                        case EBuildingType.WaterPurifier:
                            outString = "Water Purifier | Transforms Ice and Coal into Water";
                            break;

                        default:
                            outString = "Strange Building | Is useless and doesn't work";
                            break;
                    }

                    if (selectedBuilding.type == EBuildingType.Main)
                    {
                        outString += "\n[1] Build A Transporter (1 IronIngot, 2 Iron)\n[2] Build 5 Transporters (5 IronIngots, 10 Iron)";

                        if (numTrigger(NumTrigger._1))
                        {
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                        }
                        else if (numTrigger(NumTrigger._2))
                        {
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                        }
                    }
                    else if (selectedBuilding.type == EBuildingType.MinerMaker)
                    {
                        outString = "MINER FACTORY\n[1] Build a Miner for Stone & Coal (2 IronIngots, 3 Coal)\n";

                        if (numTrigger(NumTrigger._1))
                        {
                            ((BMinerFactory)selectedBuilding).buildMiner();
                        }

                        if ((Master.discoveryStarted & ETechnology.Softminer) != ETechnology.Softminer)
                        {
                            outString += "[2] Develop Special Miner for Rare Ores (8 IronIngots, 4 Coal, 2 Stone)";

                            if (numTrigger(NumTrigger._2))
                            {
                                ((BMinerFactory)selectedBuilding).discoverSoftMiner();
                            }
                        }
                        else
                        {
                            outString += "[2] Build Special Miner for Rare Ores (4 IronIngots, 2 Stone)";

                            if (numTrigger(NumTrigger._2))
                            {
                                ((BMinerFactory)selectedBuilding).buildSoftMiner();
                            }
                        }
                    }
                    else if(selectedBuilding.type == EBuildingType.WaterPurifier)
                    {
                        outString += "\nWater Purifiers also work if there is no pylon nearby to supply power.\nThey just take much longer to produce water.";
                    }
                    else if (selectedBuilding.type == EBuildingType.PowerPlant)
                    {
                        outString += "\nYou need a Pylon to distribute the Power generated by this Power Plant.";
                    }
                    else if(selectedBuilding.type == EBuildingType.University)
                    {
                        outString += "\n";
                        int num = 1;

                        if ((Master.discoveryStarted & ETechnology.BetterFighter) == 0)
                        {
                            outString += "[" + num++ + "] Develop A Better Fighter (5 GoldIngots, 25 IronIngots, 25 Stone, 25 Iron, 10 Food)\n";

                            if (numTrigger((NumTrigger)(num - 1)))
                            {
                                ((BUniversity)selectedBuilding).developBiggerFighter();
                            }
                        }

                        if ((Master.discoveryStarted & ETechnology.PurPurPurifier) == 0)
                        {
                            outString += "[" + num++ + "] Develop A Purifier for PurPur (20 GoldIngots, 20 IronIngots, 20 Stone, 20 RawPurpur, 20 Food)\n";

                            if(numTrigger((NumTrigger)(num - 1)))
                            {
                                ((BUniversity)selectedBuilding).developPurPurPurifier();
                            }
                        }

                        if ((Master.discoveryStarted & ETechnology.BigWarStation) == 0)
                        {
                            outString += "[" + num++ + "] Develop A Building To Manifacture Bigger Fighting-Units (20 GoldIngots, 50 IronIngots, 50 Stone, 20 Food)\n";

                            if (numTrigger((NumTrigger)(num - 1)))
                            {
                                ((BUniversity)selectedBuilding).developBigWarStation();
                            }
                        }
                        else if((Master.discoveryStarted & ETechnology.BigCanonTank) == 0 && (Master.DevelopedTechnologies & ETechnology.BigWarStation) != 0)
                        {
                            outString += "[" + num++ + "] Develop A Massive Laser-Cannon-Tank (50 GoldIngots, 25 IronIngots, 50 Food, 25 Purpur)\n";

                            if (numTrigger((NumTrigger)(num - 1)))
                            {
                                ((BUniversity)selectedBuilding).developCannonTank();
                            }
                        }
                    }
                    else if (selectedBuilding.type == EBuildingType.SmallWar)
                    {
                        outString += "\n[1] Build a Regular Fast Fighter (6 IronIngots, 4 GoldIngots, 2 Food)\n";

                        if (numTrigger(NumTrigger._1))
                        {
                            ((BSmallWar)selectedBuilding).buildRegularFighter();
                        }

                        if ((Master.DevelopedTechnologies & ETechnology.BetterFighter) == ETechnology.BetterFighter)
                        {
                            outString += "[2] Build a Regular Fast Fighter (6 IronIngots, 6 GoldIngots, 4 Food)";

                            if (numTrigger(NumTrigger._2))
                            {
                                ((BSmallWar)selectedBuilding).buildBetterFighter();
                            }
                        }
                    }
                    else if (selectedBuilding.type == EBuildingType.BigWar)
                    {
                        outString = "\n[1] Build a big flying Tank (25 IronIngots, 15 GoldIngots, 10 Food, 5 PurPur)\n";

                        if (numTrigger(NumTrigger._1))
                        {
                            ((BBigWar)selectedBuilding).buildRegularFighter();
                        }

                        if ((Master.DevelopedTechnologies & ETechnology.BigCanonTank) == ETechnology.BigCanonTank)
                        {
                            outString += "[2] Build a HUGE flying Tank (25 IronIngots, 25 GoldIngots, 15 Food, 15 PurPur)";

                            if (numTrigger(NumTrigger._2))
                            {
                                ((BBigWar)selectedBuilding).buildBetterFighter();
                            }
                        }
                    }
                    else if (selectedBuilding.type == EBuildingType.Construction)
                    {
                        outString = "This ";

                        switch(((BUnderConstruction)selectedBuilding).futurePlans.type)
                        {
                            case EBuildingType.BigWar:
                                outString += "Big Tank Manifacturing Station";
                                break;

                            case EBuildingType.GoldIngoter:
                                outString += "Gold Smelter";
                                break;

                            case EBuildingType.IronIngoter:
                                outString += "Iron Smelter";
                                break;

                            case EBuildingType.Main:
                                outString += "Main Building";
                                break;

                            case EBuildingType.MinerMaker:
                                outString += "Miner Factory";
                                break;

                            case EBuildingType.PlantMaker:
                                outString += "Plantage";
                                break;

                            case EBuildingType.PowerPlant:
                                outString += "Power Plant";
                                break;

                            case EBuildingType.PurPurPurifier:
                                outString += "PurPur Purifier";
                                break;

                            case EBuildingType.Pylon:
                                outString += "Pylon";
                                break;

                            case EBuildingType.SmallWar:
                                outString += "Fighter Workshop";
                                break;

                            case EBuildingType.StoneFiltrationStation:
                                outString += "Stone Filtrator";
                                break;

                            case EBuildingType.University:
                                outString += "University";
                                break;

                            case EBuildingType.WaterPurifier:
                                outString += "Water Purifier";
                                break;

                            default:
                                outString += "Strange Building";
                                break;
                        }

                        outString += " is currently under Construction.\n";
                    }
                    else
                    {
                        outString = "[THIS BUILDING HAS NO SPECIAL ABILITIES]";
                    }

                    if (selectedBuilding is GStoppableBuilding) // if something without special texts
                    {
                        outString += "\n[0] " + (((GStoppableBuilding)selectedBuilding).stopped ? "Start Working Again" : "Stop Working");

                        if (numTrigger(NumTrigger._0))
                            ((GStoppableBuilding)selectedBuilding).stopped = !((GStoppableBuilding)selectedBuilding).stopped;
                    }

                    outString += "\n[SHIFT + DELETE] Destroy Building";

                    if(numTrigger(NumTrigger._SHIFT_DELETE))
                    {
                        Master.removeBuilding(selectedBuilding);
                        
                        selectedBuilding = null;
                        return;
                    }

                    displayRessources(selectedBuilding.ressources);
                }
                // if unit selected
                else if (selectedUnits.Count > 0)
                {
                    outString = selectedUnits.Count.ToString() + " Units selected.\n[1] Select First\n[2] Select Half\n[3] Select Low HP (< 30%)\n";

                    List<int> Types = new List<int>();
                    int num = 4;

                    if (selectionContainsTroops)
                    {
                        outString += "[4] Ignore Enemy Attacks (" + (((GFighter)selectedUnits[0]).dontcareabouteverything ? "ON" : "OFF") + ")  |  [5] Attacking Mode: ";

                        switch(((GFighter)selectedUnits[0]).fightMode)
                        {
                            case EGFighterFightMode.ClosestDistance:
                                outString += "<Closest Distance>\n";
                                break;

                            case EGFighterFightMode.MaximumTotalHP:
                                outString += "<Maximum Total HP>\n";
                                break;

                            case EGFighterFightMode.MinimumHPPercentage:
                                outString += "<Minimum HP Percentage>\n";
                                break;

                            case EGFighterFightMode.OnlyBuildings:
                                outString += "<Only Attack Buildings>\n";
                                break;
                        }

                        if(numTrigger(NumTrigger._4))
                        {
                            bool value = !((GFighter)selectedUnits[0]).dontcareabouteverything;

                            for (int i = 0; i < selectedUnits.Count; i++)
                            {
                                ((GFighter)selectedUnits[i]).dontcareabouteverything = value;
                            }
                        }

                        if (numTrigger(NumTrigger._5))
                        {
                            EGFighterFightMode value = (((GFighter)selectedUnits[0]).fightMode + 1);

                            if ((int)value >= ((EGFighterFightMode[])Enum.GetValues(typeof(EGFighterFightMode))).Length)
                            {
                                value = 0;
                            }

                            for (int i = 0; i < selectedUnits.Count; i++)
                            {
                                ((GFighter)selectedUnits[i]).fightMode = value;
                            }
                        }

                        num = 6;

                        // 2 = normal fighter
                        // 3 = better fighter
                        // 4 = normal tank
                        // 5 = better tank

                        for (int i = 0; i < selectedUnits.Count; i++)
                        {
                            if(selectedUnits[i] is GTank)
                            {
                                if (((GTank)selectedUnits[i]).betterfighter)
                                {
                                    if (!Types.Contains(4))
                                    {
                                        outString += "[" + num++ + "] Only Select Big Tanks    " + (num % 2 == 0 ? "\n" : "");
                                        Types.Add(4);

                                        if (numTrigger((NumTrigger)(num - 1)))
                                        {
                                            for (int j = selectedUnits.Count - 1; j >= 0; j--)
                                            {
                                                if (!(selectedUnits[j] is GTank) || ((GTank)selectedUnits[j]).betterfighter)
                                                    selectedUnits.RemoveAt(j);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!Types.Contains(5))
                                    {
                                        outString += "[" + num++ + "] Only Select HUGE Tanks    " + (num % 2 == 0 ? "\n" : "");
                                        Types.Add(5);

                                        if (numTrigger((NumTrigger)(num - 1)))
                                        {
                                            for (int j = selectedUnits.Count - 1; j >= 0; j--)
                                            {
                                                if (!(selectedUnits[j] is GTank) || !((GTank)selectedUnits[j]).betterfighter)
                                                    selectedUnits.RemoveAt(j);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (!((GFighter)selectedUnits[i]).betterfighter)
                            {
                                if (!Types.Contains(2))
                                {
                                    outString += "[" + num++ + "] Only Select Regular Fighters    " + (num % 2 == 0 ? "\n" : "");
                                    Types.Add(2);

                                    if (numTrigger((NumTrigger)(num - 1)))
                                    {
                                        for (int j = selectedUnits.Count - 1; j >= 0; j--)
                                        {
                                            if (((GFighter)selectedUnits[j]).betterfighter || (selectedUnits[j] is GTank))
                                                selectedUnits.RemoveAt(j);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!Types.Contains(3))
                                {
                                    outString += "[" + num++ + "] Only Select Better Fighters    " + (num % 2 == 0 ? "\n" : "");
                                    Types.Add(3);

                                    if (numTrigger((NumTrigger)(num - 1)))
                                    {
                                        for (int j = selectedUnits.Count - 1; j >= 0; j--)
                                        {
                                            if (!((GFighter)selectedUnits[j]).betterfighter || (selectedUnits[j] is GTank))
                                                selectedUnits.RemoveAt(j);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // 0 = normal miner
                        // 1 = softminer

                        for (int i = 0; i < selectedUnits.Count; i++)
                        {
                            if (!((GMiner)selectedUnits[i]).softmine)
                            {
                                if (!Types.Contains(0))
                                {
                                    outString += "[" + num++ + "] Only Select Regular Miners (They only mine Stone and Coal)\n";
                                    Types.Add(0);

                                    if (numTrigger((NumTrigger)(num - 1)))
                                    {
                                        for (int j = selectedUnits.Count - 1; j >= 0; j--)
                                        {
                                            if (((GMiner)selectedUnits[j]).softmine)
                                                selectedUnits.RemoveAt(j);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!Types.Contains(1))
                                {
                                    outString += "[" + num++ + "] Only Select Rare-Ore Miners (They only mine Rare Ores)\n";
                                    Types.Add(1);

                                    if (numTrigger((NumTrigger)(num - 1)))
                                    {
                                        for (int j = selectedUnits.Count - 1; j >= 0; j--)
                                        {
                                            if (!((GMiner)selectedUnits[j]).softmine)
                                                selectedUnits.RemoveAt(j);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (numTrigger(NumTrigger._1))
                    {
                        selectedUnits.RemoveRange(1, selectedUnits.Count - 1);
                    }
                    else if (numTrigger(NumTrigger._2))
                    {
                        selectedUnits.RemoveRange(selectedUnits.Count / 2, selectedUnits.Count - selectedUnits.Count / 2); // for odd and even numbers
                    }
                    else if (numTrigger(NumTrigger._3))
                    {
                        for (int i = selectedUnits.Count - 1; i >= 0; i--)
                        {
                            if (selectedUnits[i].health / selectedUnits[i].maxhealth >/*=*/ .3f) // well... <= 30% not < 30%
                            {
                                selectedUnits.RemoveAt(i);
                            }
                        }
                    }
                }
                //if nothing selected
                else
                {
                    if (menuState == 0)
                    {
                        outString = "[1] Basic Economy Buildings\n[2] High-Tech Economy Buildings\n[3] War Buildings";

                        if (numTrigger(NumTrigger._1))
                            menuState = 1;

                        if (numTrigger(NumTrigger._2))
                            menuState = 2;

                        if (numTrigger(NumTrigger._3))
                            menuState = 3;
                    }
                    else if (menuState == 1)
                    {
                        outString = "[1] Stone Filtration | [2] Miner Factory \n[3] Iron Smelter | [4] Gold Smelter\n[ESC] back\n";

                        if (numTrigger(NumTrigger._ESC))
                            menuState = 0;

                        if (numTrigger(NumTrigger._1))
                        {
                            placeBuilding = 10;
                            buildingSize = 2;
                        }

                        if (numTrigger(NumTrigger._2))
                        {
                            placeBuilding = 4;
                            buildingSize = 2;
                        }

                        if (numTrigger(NumTrigger._3))
                        {
                            placeBuilding = 2;
                            buildingSize = 2;
                        }

                        if (numTrigger(NumTrigger._4))
                        {
                            placeBuilding = 1;
                            buildingSize = 2;
                        }
                    }
                    else if (menuState == 2)
                    {
                        outString = "[1] Water Purification | [2] Plantages \n[3] Power Plant | [4] Pylon \n[5] University";

                        if ((Master.DevelopedTechnologies & ETechnology.PurPurPurifier) == ETechnology.PurPurPurifier)
                            outString += " | [6] PurPur-Purifier";

                        outString += "\n[ESC] back\n";

                        if (numTrigger(NumTrigger._ESC))
                            menuState = 0;

                        if (numTrigger(NumTrigger._1))
                        {
                            placeBuilding = 13;
                            buildingSize = 2;
                        }

                        if (numTrigger(NumTrigger._2))
                        {
                            placeBuilding = 5;
                            buildingSize = 2;
                        }

                        if (numTrigger(NumTrigger._3))
                        {
                            placeBuilding = 6;
                            buildingSize = 2;
                        }

                        if (numTrigger(NumTrigger._4))
                        {
                            placeBuilding = 8;
                            buildingSize = 1;
                        }

                        if (numTrigger(NumTrigger._5))
                        {
                            placeBuilding = 12;
                            buildingSize = 2;
                        }

                        if (numTrigger(NumTrigger._6) && (Master.DevelopedTechnologies & ETechnology.PurPurPurifier) == ETechnology.PurPurPurifier)
                        {
                            placeBuilding = 7;
                            buildingSize = 2;
                        }
                    }

                    else if (menuState == 3)
                    {
                        outString = "[1] Small Fighter Factory\n";

                        if (numTrigger(NumTrigger._1))
                        {
                            placeBuilding = 9;
                            buildingSize = 2;
                        }

                        if ((Master.DevelopedTechnologies & ETechnology.BigWarStation) == ETechnology.BigWarStation)
                        {
                            if (numTrigger(NumTrigger._2))
                            {
                                placeBuilding = 0;
                                buildingSize = 2;
                            }

                            outString += "[2] Big Tank Factory\n";
                        }

                        outString += "[ESC] back\n";

                        if (numTrigger(NumTrigger._ESC))
                            menuState = 0;
                    }
                    else
                    {
                        outString = "";
                    }
                }

                goto END;

                NOT_WORKED_POWER:
                outString = "[NO POWER HERE. TRY PLACING THE BUILDING NEAR A POWERPLANT OR PYLON!]";
                goto END;

                NOT_WORKED_GENERAL:
                outString = "[YOU CAN'T PLACE THIS BUILDING HERE! THERE'S NOT ENIUGH ROOM.]";
                goto END;

                END:
                ;
            }
        }

        private void setResToGlobalTransportVolumes()
        {
            resString = "Total Current Ressources: (Offers - Needs)\n";

            for (int i = 0; i < TransportHandler.OfferCount.Length; i++)
            {
                int num = -TransportHandler.NeedCount[i] + TransportHandler.OfferCount[i];

                resString += "[" + ((ERessourceType[])Enum.GetValues(typeof(ERessourceType)))[i] + "] : " + num.ToString("+0;-0; 0") + "     ";

                if (i % 3 == 2)
                {
                    resString += "\n";
                }
            }
        }

        private void displayRessources(int[] ressources)
        {
            resString = "This Building currently has the following Ressources:\n";

            for (int i = 0; i < ressources.Length; i++)
            {
                resString += "[" + ((ERessourceType[])Enum.GetValues(typeof(ERessourceType)))[i] + "] : " + ressources[i].ToString() + "    ";

                if (i % 3 == 2)
                {
                    resString += "\n";
                }
            }
        }

        public void draw(SpriteBatch batch, int width, int height)
        {
            batch.DrawString(Master.pixelFont, outString, new Vector2(40, height - 120), Color.White);
            batch.DrawString(Master.pixelFont, resString, new Vector2(width - 525, height - 120), Color.White);

            if (Master.youCanWin && Master.WinningState == EWinningState.YouWon)
            {
                batch.DrawString(Master.biggerFont, "You won the game!", new Vector2(width / 2f - 145, 5), Color.White);
            }
            else if (Master.youCanWin && Master.WinningState == EWinningState.YouLost)
            {
                batch.DrawString(Master.biggerFont, "You lost the game!", new Vector2(width / 2f - 150, 5), Color.White);
                batch.DrawString(Master.pixelFont, "But there's no other way to the Main Menu\nthan to restart the whole game. Sorry.", new Vector2(width / 2f - 175, 35), Color.White);
            }
        }

        public static void setKS(KeyboardState ks, KeyboardState lks)
        {
            MenuHandler.ks = ks;
            MenuHandler.lks = lks;
        }

        public static bool numTrigger(NumTrigger trigger)
        {
            switch(trigger)
            {
                case NumTrigger._0:
                    return ((ks.IsKeyDown(Keys.D0) || ks.IsKeyDown(Keys.NumPad0) || ks.IsKeyDown(Keys.F10)) && lks.IsKeyUp(Keys.D0) && lks.IsKeyUp(Keys.NumPad0) && lks.IsKeyUp(Keys.F10));

                case NumTrigger._1:
                    return ((ks.IsKeyDown(Keys.D1) || ks.IsKeyDown(Keys.NumPad1) || ks.IsKeyDown(Keys.F1)) && lks.IsKeyUp(Keys.D1) && lks.IsKeyUp(Keys.NumPad1) && lks.IsKeyUp(Keys.F1));

                case NumTrigger._2:
                    return ((ks.IsKeyDown(Keys.D2) || ks.IsKeyDown(Keys.NumPad2) || ks.IsKeyDown(Keys.F2)) && lks.IsKeyUp(Keys.D2) && lks.IsKeyUp(Keys.NumPad2) && lks.IsKeyUp(Keys.F2));

                case NumTrigger._3:
                    return ((ks.IsKeyDown(Keys.D3) || ks.IsKeyDown(Keys.NumPad3) || ks.IsKeyDown(Keys.F3)) && lks.IsKeyUp(Keys.D3) && lks.IsKeyUp(Keys.NumPad3) && lks.IsKeyUp(Keys.F3));

                case NumTrigger._4:
                    return ((ks.IsKeyDown(Keys.D4) || ks.IsKeyDown(Keys.NumPad4) || ks.IsKeyDown(Keys.F4)) && lks.IsKeyUp(Keys.D4) && lks.IsKeyUp(Keys.NumPad4) && lks.IsKeyUp(Keys.F4));

                case NumTrigger._5:
                    return ((ks.IsKeyDown(Keys.D5) || ks.IsKeyDown(Keys.NumPad5) || ks.IsKeyDown(Keys.F5)) && lks.IsKeyUp(Keys.D5) && lks.IsKeyUp(Keys.NumPad5) && lks.IsKeyUp(Keys.F5));

                case NumTrigger._6:
                    return ((ks.IsKeyDown(Keys.D6) || ks.IsKeyDown(Keys.NumPad6) || ks.IsKeyDown(Keys.F6)) && lks.IsKeyUp(Keys.D6) && lks.IsKeyUp(Keys.NumPad6) && lks.IsKeyUp(Keys.F6));

                case NumTrigger._7:
                    return ((ks.IsKeyDown(Keys.D7) || ks.IsKeyDown(Keys.NumPad7) || ks.IsKeyDown(Keys.F7)) && lks.IsKeyUp(Keys.D7) && lks.IsKeyUp(Keys.NumPad7) && lks.IsKeyUp(Keys.F7));

                case NumTrigger._8:
                    return ((ks.IsKeyDown(Keys.D8) || ks.IsKeyDown(Keys.NumPad8) || ks.IsKeyDown(Keys.F8)) && lks.IsKeyUp(Keys.D8) && lks.IsKeyUp(Keys.NumPad8) && lks.IsKeyUp(Keys.F8));

                case NumTrigger._9:
                    return ((ks.IsKeyDown(Keys.D9) || ks.IsKeyDown(Keys.NumPad9) || ks.IsKeyDown(Keys.F9)) && lks.IsKeyUp(Keys.D9) && lks.IsKeyUp(Keys.NumPad9) && lks.IsKeyUp(Keys.F9));

                case NumTrigger._ESC:
                    return ((ks.IsKeyDown(Keys.Escape) || ks.IsKeyDown(Keys.Back)) && lks.IsKeyUp(Keys.Escape) && lks.IsKeyUp(Keys.Back) && ks.IsKeyUp(Keys.RightShift) && ks.IsKeyUp(Keys.LeftShift));

                case NumTrigger._SHIFT_DELETE:
                    return ((ks.IsKeyDown(Keys.LeftShift) || ks.IsKeyDown(Keys.RightShift)) && (ks.IsKeyDown(Keys.Back) || ks.IsKeyDown(Keys.Delete)) &&
                        lks.IsKeyUp(Keys.Back) && lks.IsKeyUp(Keys.Delete));
            }

            return false;
        }

        public enum NumTrigger
        {
            _0 = 10, _1 = 1, _2 = 2, _3 = 3, _4 = 4, _5 = 5, _6 = 6, _7 = 7, _8 = 8, _9 = 9, _ESC = -1, _SHIFT_DELETE = -2
        }
    }

    public enum EWinningState
    {
        YouWon,
        None,
        YouLost
    }
}