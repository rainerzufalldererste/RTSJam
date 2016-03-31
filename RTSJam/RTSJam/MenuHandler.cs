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

        public MenuHandler()
        {
        }

        public void update(KeyboardState ks, KeyboardState lks, MouseState ms, MouseState lms, ref List<GUnit> selectedUnits, ref bool selectionContainsTroops, GBuilding selectedBuilding, ref int placeBuilding, ref int buildingSize)
        {
            this.ks = ks;
            this.lks = lks;

            if (ks.IsKeyDown(Keys.Escape) && lks.IsKeyUp(Keys.Escape) && (selectedUnits.Count > 0 || selectedBuilding != null || placeBuilding != -1))
            {
                selectedUnits.Clear();
                selectionContainsTroops = false;
                placeBuilding = -1;
            }
            else
            {
                // if building
                if(selectedBuilding != null)
                {
                    if(selectedBuilding.type == EBuildingType.Main)
                    {
                        outString = "MAIN BUILDING\n[1] Build A Transporter (1 IronBar, 2 Iron)\n[2] Build 5 Transporters (5 IronBars, 10 Iron)";

                        if (numTrigger(NumTrigger._1))
                        {
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                        }
                        else if(numTrigger(NumTrigger._2))
                        {
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                            ((BMainBuilding)selectedBuilding).buildTransporter();
                        }
                    }
                    else if(selectedBuilding.type == EBuildingType.MinerMaker)
                    {
                        outString = "MINER FACTORY\n[1] Build a Miner for Stone & Coal (2 IronBars, 3 Coal)\n";

                        if (numTrigger(NumTrigger._1))
                        {
                            ((BMinerFactory)selectedBuilding).buildMiner();
                        }

                        if((Master.discoveryStarted & ETechnology.Softminer) != ETechnology.Softminer)
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
                }
                // if unit
                else if(selectedUnits.Count > 0)
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
                    outString = "";
                }
            }
        }

        public void draw(SpriteBatch batch, int width, int height)
        {
            batch.DrawString(Master.pixelFont, outString, new Vector2(10, height - 80), Color.White);
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
            }

            return false;
        }

        enum NumTrigger
        {
            _0, _1, _2, _3, _4, _5, _6, _7, _8, _9
        }
    }
}