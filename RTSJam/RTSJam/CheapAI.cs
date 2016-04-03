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
    public static class CheapAI
    {
        public static int counter = 0;
        public const int reactionTime = 5 * 60;
        public static int slowtick = 0;
        public const int slowtick_MAX = 3;
        public static Vector2 basePosition = Vector2.Zero;
        public static int searchingRange = 4;
        public static int gameState = 0;

        public static bool everythingDoneBuilding = true;
        public static int minCooldown = 120 * 40;

        public static ETechnology DevelopedTechnologies = ETechnology.None;
        public static ETechnology discoveryStarted = ETechnology.None;

        public static List<Vector2> OrePositions = new List<Vector2>();

        public static int buildingsThisTurn = 0;
        public static int buildingsThisTurnPowered = 0;

        public static void initialize(Vector2 basePosition)
        {
            CheapAI.basePosition = basePosition;
            gameState = 0;

            searchingRange += 3;
        }

        public static void update()
        {
            minCooldown--;

            if (minCooldown < 0)
                minCooldown = 0;

            counter++;

            if(counter > reactionTime)
            {
                slowtick++;

                if (slowtick >= slowtick_MAX)
                    slowtick = 0;

                counter = 0;
                int minercount = 0;
                int softminercount = 0;

                for (int i = 0; i < Master.units.Count; i++)
                {
                    if (Master.units[i].hostile)
                    {
                        if(Master.units[i] is GMiner)
                        {
                            GMiner currentMiner = ((GMiner)Master.units[i]);

                            if (currentMiner.currentAction == EMinerAction.None)
                            {
                                if (currentMiner.softmine)
                                {
                                    int num = (int)(Master.rand.NextDouble() * (OrePositions.Count + .5f));
                                    currentMiner.doAction(EActionType.SelectRegion, OrePositions[num], OrePositions[num]);
                                    softminercount++;
                                }
                                else
                                {
                                    minercount++;
                                    Vector2 pos = currentMiner.position + Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * 20;
                                    currentMiner.doAction(EActionType.SelectRegion, pos, pos);
                                }
                            }
                        }
                        else
                        {
                            GFighter currentFighter = (GFighter)Master.units[i];

                            if(currentFighter.currentAction == EFighterAction.None)
                            {
                                currentFighter.dontcareabouteverything = false;

                                currentFighter.fightMode = (EGFighterFightMode)((int)(Master.rand.NextDouble() * (float)((Enum.GetValues(typeof(EGFighterFightMode)).Length) - .1f)));

                                for (int j = 0; j < Master.units.Count; j++)
                                {
                                    if (!Master.units[j].hostile)
                                    {
                                        float dist = (Master.units[j].position - currentFighter.position).Length();

                                        if (dist < currentFighter.range * 2)
                                        {
                                            if(dist < currentFighter.range)
                                            {
                                                currentFighter.currentAction = EFighterAction.Fight;
                                            }
                                            else
                                            {
                                                currentFighter.doAction(EActionType.SelectRegion, Master.units[j].position, Master.units[j].position);
                                            }
                                            break;
                                        }
                                    }
                                }

                                if(currentFighter.currentAction == EFighterAction.None)
                                {
                                    Vector2 pos = currentFighter.position + Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * 50;
                                    currentFighter.doAction(EActionType.ClickPosition, pos, null);
                                }
                            }
                            else if(currentFighter.currentAction == EFighterAction.Fight)
                            {
                                for (int j = 0; j < Master.units.Count; j++)
                                {
                                    if (Master.units[j].hostile && Master.units[j] is GFighter && (((GFighter)Master.units[j]).currentAction == EFighterAction.None || (((GFighter)Master.units[j]).currentAction == EFighterAction.Move && !(((GFighter)Master.units[j]).fightAtLocation))))
                                    {
                                        Master.units[j].doAction(EActionType.SelectRegion, currentFighter.selectedEnemy.position, currentFighter.selectedEnemy.position);
                                    }
                                }
                            }
                        }
                    }
                }

                everythingDoneBuilding = true;

                for (int i = 0; i < Master.buildings.Count; i++)
                {
                    if (!Master.buildings[i].hostile)
                        continue;

                    if(Master.buildings[i] is BMinerFactory)
                    {
                        if (softminercount + ((BMinerFactory)Master.buildings[i]).minersLeft.Count < 0 + (gameState - 1) * 4)
                        {
                            ((BMinerFactory)Master.buildings[i]).buildSoftMiner();
                        }
                        else if (minercount + ((BMinerFactory)Master.buildings[i]).minersLeft.Count < 6 + gameState * 10)
                        {
                            ((BMinerFactory)Master.buildings[i]).buildMiner();
                        }
                    }
                    else if(Master.buildings[i] is BMainBuilding)
                    {
                        int transporters = 0;

                        if (slowtick == 0)
                        {
                            for (int j = 0; j < Master.transports.Count; j++)
                            {
                                if (Master.transports[i].hostile)
                                {
                                    transporters++;
                                }
                            }

                            while (transporters + (((BMainBuilding)Master.buildings[i]).transportsLeft) < 9 + gameState * gameState * 1.5f)
                            {
                                ((BMainBuilding)Master.buildings[i]).buildTransporter();
                                transporters++;
                            }
                        }
                    }
                    else if (Master.buildings[i] is BSmallWar)
                    {
                        if(slowtick == 0)
                        {
                            BSmallWar bsmw = (BSmallWar)Master.buildings[i];

                            if (bsmw.fightersLeft.Count > 3)
                                continue;

                            if ((DevelopedTechnologies & ETechnology.BetterFighter) == ETechnology.BetterFighter)
                                bsmw.buildBetterFighter();
                            else
                                bsmw.buildRegularFighter();
                        }
                    }
                    else if (Master.buildings[i] is BBigWar)
                    {
                        if(slowtick == 0)
                        {
                            BBigWar bbw = (BBigWar)Master.buildings[i];

                            if (bbw.fightersLeft.Count > 3)
                                continue;

                            if ((DevelopedTechnologies & ETechnology.BigCanonTank) == ETechnology.BigCanonTank)
                                bbw.buildBetterFighter();
                            else
                                bbw.buildRegularFighter();
                        }
                    }
                    else if(Master.buildings[i] is BUnderConstruction && Master.buildings[i].hostile)
                    {
                        everythingDoneBuilding = false;
                    }
                }

                if(minCooldown == 0 && everythingDoneBuilding)
                {
                    if(gameState == 0) // after >= 2min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        buildNewBuilding(EBuildingType.StoneFiltrationStation);
                        buildNewBuilding(EBuildingType.StoneFiltrationStation);
                        buildNewBuilding(EBuildingType.IronBarer);

                        minCooldown = 120 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 1) // after >= 4min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        buildNewBuilding(EBuildingType.MinerMaker);

                        buildNewBuilding(EBuildingType.StoneFiltrationStation);
                        buildNewBuilding(EBuildingType.IronBarer);

                        minCooldown = 240 * 60;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 2) // after >= 8min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        for (int i = 0; i < Master.buildings.Count; i++)
                        {
                            if(Master.buildings[i].hostile && Master.buildings[i] is BMinerFactory)
                            {
                                ((BMinerFactory)Master.buildings[i]).discoverSoftMiner();
                                break;
                            }
                        }

                        buildNewBuilding(EBuildingType.StoneFiltrationStation);
                        buildNewBuilding(EBuildingType.IronBarer);

                        minCooldown = 240 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 3) // after >= 12min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        buildNewBuilding(EBuildingType.WaterPurifier);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.WaterPurifier);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.PowerPlant);

                        minCooldown = 360 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 4) // after >= 18min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        buildNewBuilding(EBuildingType.GoldBarer);
                        buildNewBuilding(EBuildingType.PlantMaker);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.PlantMaker);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.WaterPurifier);
                        buildNewBuilding(EBuildingType.MinerMaker);

                        minCooldown = 540 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 5) // after >= 27min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        buildNewBuilding(EBuildingType.University);
                        buildNewBuilding(EBuildingType.GoldBarer);
                        buildNewBuilding(EBuildingType.SmallWar);

                        minCooldown = 180 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 6) // after >= 30min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        for (int i = 0; i < Master.buildings.Count; i++)
                        {
                            if (Master.buildings[i].hostile && Master.buildings[i] is BUniversity)
                            {
                                ((BUniversity)Master.buildings[i]).developBiggerFighter();
                                break;
                            }
                        }
                        buildNewBuilding(EBuildingType.GoldBarer);
                        buildNewBuilding(EBuildingType.MinerMaker);

                        minCooldown = 240 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 7) // after >= 32min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        for (int i = 0; i < Master.buildings.Count; i++)
                        {
                            if (Master.buildings[i].hostile && Master.buildings[i] is BUniversity)
                            {
                                ((BUniversity)Master.buildings[i]).developPurPurPurifier();
                                break;
                            }
                        }
                        buildNewBuilding(EBuildingType.SmallWar);

                        minCooldown = 480 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 8) // after >= 40min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        for (int i = 0; i < Master.buildings.Count; i++)
                        {
                            if (Master.buildings[i].hostile && Master.buildings[i] is BUniversity)
                            {
                                ((BUniversity)Master.buildings[i]).developBigWarStation();
                                break;
                            }
                        }

                        buildNewBuilding(EBuildingType.PurPurPurifier);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.PowerPlant);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.PlantMaker);

                        minCooldown = 360 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 9) // after >= 46min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;

                        buildNewBuilding(EBuildingType.BigWar);

                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.WaterPurifier);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.PlantMaker);

                        buildNewBuilding(EBuildingType.MinerMaker);

                        for (int i = 0; i < Master.buildings.Count; i++)
                        {
                            if (Master.buildings[i].hostile && Master.buildings[i] is BUniversity)
                            {
                                ((BUniversity)Master.buildings[i]).developCannonTank();
                                break;
                            }
                        }

                        minCooldown = 480 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else if (gameState == 10) // after >= 54min
                    {
                        buildingsThisTurn = 0;
                        buildingsThisTurnPowered = 0;
                        
                        buildNewBuilding(EBuildingType.PurPurPurifier);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.PlantMaker);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.WaterPurifier);

                        buildNewBuilding(EBuildingType.GoldBarer);
                        buildNewBuilding(EBuildingType.BigWar);
                        buildNewBuilding(EBuildingType.SmallWar);

                        minCooldown = 240 * 6;
                        everythingDoneBuilding = false;
                        gameState++;
                    }
                    else // after >60min
                    {
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.PurPurPurifier);
                        buildNewBuilding(EBuildingType.Pylon);
                        buildNewBuilding(EBuildingType.PurPurPurifier);

                        buildNewBuilding(EBuildingType.GoldBarer);
                        buildNewBuilding(EBuildingType.GoldBarer);

                        // ENDGAME REACHED
                    }
                }
            }
        }

        private static void buildNewBuilding(EBuildingType buildingType)
        {
            if(buildingType == EBuildingType.PlantMaker || buildingType == EBuildingType.PowerPlant ||
                buildingType == EBuildingType.Pylon || buildingType == EBuildingType.WaterPurifier || 
                buildingType == EBuildingType.PurPurPurifier)
            {
                int posX = (int)basePosition.X + 3 + buildingsThisTurnPowered, posY = (int)basePosition.Y + 11 - 2 * gameState, xobj, yobj, chunk;
                Master.getCollisionExists(out chunk, out xobj, out yobj, posX, posY);

                switch(buildingType)
                {
                    case EBuildingType.PlantMaker:
                        Master.AddBuilding(new BPlantage(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.PowerPlant:
                        Master.AddBuilding(new BPowerPlant(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.Pylon:
                        Master.AddBuilding(new BPylon(new Vector2(posX, posY), true), chunk, xobj, yobj, false);
                        break;


                    case EBuildingType.WaterPurifier:
                        Master.AddBuilding(new BWaterPurifier(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.PurPurPurifier:
                        Master.AddBuilding(new BPurPurPurifier(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;
                        

                    default:
#if DEBUG
                        throw new Exception("THIS BUILDING DOES NOT BELONG HERE OR YOU FORGOT SOMETING SOMEHOW");
#endif
                        break;
                }

                if (buildingType == EBuildingType.Pylon)
                    buildingsThisTurnPowered++;
                else
                    buildingsThisTurnPowered += 2;
            }
            else
            {
                int posX = (int)basePosition.X - 2 - buildingsThisTurn, posY = (int)basePosition.Y - 11 + 2 * gameState, xobj, yobj, chunk;
                Master.getCollisionExists(out chunk, out xobj, out yobj, posX, posY);

                switch (buildingType)
                {
                    case EBuildingType.BigWar:
                        Master.AddBuilding(new BBigWar(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.GoldBarer:
                        Master.AddBuilding(new BGoldMelting(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.IronBarer:
                        Master.AddBuilding(new BIronMelting(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.Main:
                        Master.AddBuilding(new BMainBuilding(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.MinerMaker:
                        Master.AddBuilding(new BMinerFactory(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.SmallWar:
                        Master.AddBuilding(new BSmallWar(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.StoneFiltrationStation:
                        Master.AddBuilding(new BStoneFiltration(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.University:
                        Master.AddBuilding(new BUniversity(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    case EBuildingType.WaterPurifier:
                        Master.AddBuilding(new BWaterPurifier(new Vector2(posX, posY), true), chunk, xobj, yobj, true);
                        break;


                    default:
#if DEBUG
                        throw new Exception("THIS BUILDING DOES NOT BELONG HERE OR YOU FORGOT SOMETING SOMEHOW");
#endif
                        break;
                }

                buildingsThisTurn += 2;
            }
        }
    }
}
