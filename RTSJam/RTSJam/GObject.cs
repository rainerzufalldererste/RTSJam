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
    public class GObject
    {
        public Vector2 position;
        public int texture = 1;
    }

    public class GStone : GObject
    {
        public ERessourceType stoneType = ERessourceType.Stone;
        public int health = 2000, maxhealth = 2000;
    }

    // Not used anymore! Rest in Pizza, dude!
    public enum EStoneType
    {
        Stone,
        Coal,
        Ice,
        Gold,
        Pizza,
    }


    public class GGround : GObject
    {
    }

    public class GObjBuild : GObject
    {
        public GBuilding building;
        public List<GObject> connectedPositions = new List<GObject>();

        public GObjBuild(GBuilding building, List<GObject> connectedPositions)
        {
            this.building = building;
            this.connectedPositions = connectedPositions;
        }

        public void draw(SpriteBatch batch)
        {
            building.draw(batch);
        }

        public void remove()
        {
            for (int i = 0; i < connectedPositions.Count; i++)
            {
                connectedPositions[i] = new GGround() { texture = 1, position = connectedPositions[i].position };
            }

            connectedPositions = null;
            building.remove();
        }
    }

    public class G_NOT_FREE_FOR_BUILDING
    {
        public GObjBuild thisIsWhereIBelongTo;

        public G_NOT_FREE_FOR_BUILDING(GObjBuild thisIsWhereIBelongTo)
        {
            this.thisIsWhereIBelongTo = thisIsWhereIBelongTo;
        }
    }
}