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
        public Vector2 size = new Vector2(1);
        public int texture = 1;
    }

    public class GStone : GObject
    {
        public EStoneType stoneType = EStoneType.Stone;
        public int health = 2000;
    }

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
}