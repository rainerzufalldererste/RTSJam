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
    public class Chunk
    {
        public GObject[][] gobjects = new GObject[Master.chunknum][];
        public Rectangle boundaries;

        public Chunk(int x, int y)
        {
            for (int i = 0; i < Master.chunknum; i++)
            {
                gobjects[i] = new GObject[Master.chunknum];
            }

            boundaries = new Rectangle(x * Master.chunknum, y * Master.chunknum, Master.chunknum, Master.chunknum);
        }
    }
}
