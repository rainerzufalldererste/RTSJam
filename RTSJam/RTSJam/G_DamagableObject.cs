using Microsoft.Xna.Framework;

namespace RTSJam
{
    public abstract class G_DamagableObject
    {
        public int health, maxhealth;
        public Vector2 position;
        public bool hostile = false;

        public abstract void takeDamage(int amount, G_DamagableObject sender);
    }
}