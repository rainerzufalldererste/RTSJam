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
    internal class MenuHandler
    {
        private string outString = "";
        private KeyboardState ks;
        private KeyboardState lks;
        int menuState = 0;
        private string resString = "";

        public MenuHandler()
        {
        }

        public void update(KeyboardState ks, KeyboardState lks, MouseState ms, MouseState lms, ref List<GUnit> selectedUnits, ref bool selectionContainsTroops, GBuilding selectedBuilding, ref int placeBuilding, ref int buildingSize, Rectangle selectionA)
        {
            this.ks = ks;
            this.lks = lks;
            setResToGlobalTransportVolumes();

            if (ks.IsKeyDown(Keys.Escape) && lks.IsKeyUp(Keys.Escape) && (selectedUnits.Count > 0 || selectedBuilding != null || placeBuilding > -1))
            {
                selectedUnits.Clear();
                selectionContainsTroops = false;
                placeBuilding = -1;
                selectedBuilding = null;
            }
            else
            {
                // if placing building
                if(placeBuilding >= 0)
                {
                    if(ms.LeftButton == ButtonState.Pressed && lms.LeftButton == ButtonState.Released)
                    {
                        if(buildingSize == 1)
                        {
                            int chunk, xobj, yobj;
                            GObject gobj = Master.getGObjAt(new Vector2(selectionA.X, selectionA.Y), out chunk, out xobj, out yobj);

                            if(gobj is GGround)
                            {
                                // place building

                                switch(placeBuilding)
                                {
                                    // ONLY SINGLE SIZE BUILDINGS!!!

                                    default:
                                        throw new Exception("ONLY SINGLE SIZE BUILDINGS, PLEASE!");
                                }

                                placeBuilding = -1;
                            }
                        }
                        else if(buildingSize == 2)
                        {
                            int chunk, xobj, yobj;
                            GObject gobj = Master.getGObjAt(new Vector2(selectionA.X, selectionA.Y), out chunk, out xobj, out yobj);

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
                                                case 2:
                                                    Master.AddBuilding(new BIronMelting(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    break;

                                                case 4:
                                                    Master.AddBuilding(new BMinerFactory(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    break;

                                                case 10:
                                                case 11:
                                                    Master.AddBuilding(new BStoneFiltration(new Vector2(selectionA.X, selectionA.Y), false), chunk, xobj, yobj, true);
                                                    break;

                                                default:
                                                    throw new Exception("ONLY DOUBLE SIZE BUILDINGS, PLEASE!");
                                            }

                                            placeBuilding = -1;
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            throw new Exception("What are you doing?!");
                        }
                    }
                }
                // if building selected
                else if(selectedBuilding != null)
                {
                    if (selectedBuilding.type == EBuildingType.Main)
                    {
                        outString = "MAIN BUILDING\n[1] Build A Transporter (1 IronBar, 2 Iron)\n[2] Build 5 Transporters (5 IronBars, 10 Iron)";

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
                        outString = "MINER FACTORY\n[1] Build a Miner for Stone & Coal (2 IronBars, 3 Coal)\n";

                        if (numTrigger(NumTrigger._1))
                        {
                            ((BMinerFactory)selectedBuilding).buildMiner();
                        }

                        if ((Master.discoveryStarted & ETechnology.Softminer) != ETechnology.Softminer)
                        {
                            outString += "[2] Develop Special Miner for Rare Ores (8 IronBars, 4 Coal, 2 Stone)";

                            if (numTrigger(NumTrigger._2))
                            {
                                ((BMinerFactory)selectedBuilding).discoverSoftMiner();
                            }
                        }
                        else
                        {
                            outString += "[2] Build Special Miner for Rare Ores (4 IronBars, 2 Stone)";

                            if (numTrigger(NumTrigger._2))
                            {
                                ((BMinerFactory)selectedBuilding).buildSoftMiner();
                            }
                        }
                    }
                    else
                    {
                        outString = "[THIS BUILDING HAS NO SPECIAL ABILITIES]";
                    }

                    if (selectedBuilding is GStoppableBuilding) // if something without special texts
                    {
                        outString += "\n [0] " + (((GStoppableBuilding)selectedBuilding).stopped ? "Start Working Again" : "Stop Working");

                        if (numTrigger(NumTrigger._0))
                            ((GStoppableBuilding)selectedBuilding).stopped = !((GStoppableBuilding)selectedBuilding).stopped;
                    }

                    displayRessources(selectedBuilding.ressources);
                }
                // if unit selected
                else if (selectedUnits.Count > 0)
                {
                    outString = selectedUnits.Count.ToString() + " Units selected.\n[1] Select First\n[2] Select Half\n[3] Select Low HP (< 30%)";

                    if (numTrigger(NumTrigger._1))
                    {
                        selectedUnits.RemoveRange(1, selectedUnits.Count - 1);
                    }
                    else if(numTrigger(NumTrigger._2))
                    {
                        selectedUnits.RemoveRange(selectedUnits.Count / 2, selectedUnits.Count - selectedUnits.Count / 2); // for odd and even numbers
                    }
                    else if(numTrigger(NumTrigger._3))
                    {
                        for (int i = selectedUnits.Count - 1; i >= 0 ; i--)
                        {
                            if(selectedUnits[i].health / selectedUnits[i].maxHealth >/*=*/ .3f) // well... <= 30% not < 30%
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
                        outString = "[1] Basic Economy Buildings\n[2] High-Tech Economy Buildings\n[3] War Buildings\n[4] Current Ressources";

                        if (numTrigger(NumTrigger._1))
                            menuState = 1;

                        if (numTrigger(NumTrigger._2))
                            menuState = 2;

                        if (numTrigger(NumTrigger._3))
                            menuState = 3;

                        if (numTrigger(NumTrigger._4))
                            menuState = 4;
                    }
                    else if(menuState == 1)
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
                            ;
                    }
                    else if (menuState == 2)
                    {
                        outString = "[1] Water Purification | [2] Plantages \n[3] Power Plant | [4] Pylon \n[5] University | [6] PurPur-Purification\n[ESC] back\n";

                        if (numTrigger(NumTrigger._ESC))
                            menuState = 0;
                    }
                    else if (menuState == 3)
                    {
                        outString = "[1] Small Fighter Factory\n[2] Big Tank Factory\n[ESC] back\n";

                        if (numTrigger(NumTrigger._ESC))
                            menuState = 0;
                    }
                    else if (menuState == 4)
                    {
                        outString = "blah stats \n[ESC] back\n";

                        if (numTrigger(NumTrigger._ESC))
                            menuState = 0;
                    }
                    else
                    {
                        outString = "";
                    }
                }
            }
        }

        private void setResToGlobalTransportVolumes()
        {
            resString = "";

            for (int i = 0; i < TransportHandler.OfferCount.Length; i++)
            {
                resString += "[" + ((ERessourceType[])Enum.GetValues(typeof(ERessourceType)))[i] + "] : " + TransportHandler.NeedCount[i].ToString() + " / " + TransportHandler.OfferCount[i].ToString() + "    ";

                if (i % 3 == 2)
                {
                    resString += "\n";
                }
            }
        }

        private void displayRessources(int[] ressources)
        {
            resString = "";

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
            batch.DrawString(Master.pixelFont, outString, new Vector2(10, height - 80), Color.White);
            batch.DrawString(Master.pixelFont, resString, new Vector2(width - 500, height - 80), Color.White);
        }

        private bool numTrigger(NumTrigger trigger)
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
                    return ((ks.IsKeyDown(Keys.Escape) || ks.IsKeyDown(Keys.Back)) && lks.IsKeyUp(Keys.Escape) && lks.IsKeyUp(Keys.Back));
            }

            return false;
        }

        enum NumTrigger
        {
            _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _ESC
        }
    }
}