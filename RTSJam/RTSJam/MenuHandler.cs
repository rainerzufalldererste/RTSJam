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
        public MenuHandler()
        {
        }

        public void update(KeyboardState ks, KeyboardState lks, MouseState ms, MouseState lms, ref List<GUnit> selectedUnits, ref bool selectionContainsTroops, GBuilding selectedBuilding)
        {
            if (ks.IsKeyDown(Keys.Escape) && lks.IsKeyUp(Keys.Escape) && (selectedUnits.Count > 0 || selectedBuilding != null))
            {
                selectedUnits.Clear();
                selectionContainsTroops = false;
            }
        }

        public void draw(SpriteBatch batch, int width, int height)
        {
            batch.DrawString(Master.pixelFont, "I am a Menu.", new Vector2(20, height - 80), Color.White);
        }
    }
}