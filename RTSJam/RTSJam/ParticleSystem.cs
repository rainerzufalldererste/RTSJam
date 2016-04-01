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
    public class ParticleSystem
    {
        public List<Particle> particles = new List<Particle>();

        public void update(SpriteBatch batch)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle particle = particles[i];
                particle.alpha *= particle.alphaFactor;

                if (particle.alpha <= .04)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                particle.position += particle.velocity;
                particle.velocity *= particle.velocityFactor;

                particle.size += particle.sizeAdd;
                particle.sizeAdd *= particle.sizeAddCooldown;

                particle.rotation += particle.rotationAdd;
                particle.rotationAdd *= particle.rotationAddCooldown;

                batch.Draw(particle.texture, particle.position, null, new Color(particle.alpha, particle.alpha, particle.alpha, particle.alpha), particle.rotation, new Vector2(5), particle.size, SpriteEffects.None, 0f);

                particles[i] = particle;
            }
        }

        public void addDustParticle(Vector2 pos)
        {
            pos.Y += .3f;

            pos += Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .4f;

            particles.Add(new Particle(Master.fxTextures[2], pos, Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .015f, .925f, .02f, .0015f, .999f, (float)Master.rand.NextDouble() * Master.TwoPI, ((float)Master.rand.NextDouble() - .5f) * .15f, .985f, .1f, .99f));
        }

        internal void addHeavyDustParticles(Vector2 pos)
        {
            addHeavyDustParticle(pos);
            addHeavyDustParticle(pos);
            addHeavyDustParticle(pos);
            addHeavyDustParticle(pos);
            addHeavyDustParticle(pos);
        }

        private void addHeavyDustParticle(Vector2 pos)
        {
            pos += Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .3f;

            particles.Add(new Particle(Master.fxTextures[2], pos, Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .025f, .925f, .02f, .0015f, .999f, (float)Master.rand.NextDouble() * Master.TwoPI, ((float)Master.rand.NextDouble() - .5f) * .15f, .985f, .3f, .99f));
        }

        internal void addLightSmokeParticles(Vector2 pos)
        {
            pos += Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .3f;

            particles.Add(new Particle(Master.fxTextures[7], pos, new Vector2(0,-.025f) + Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .00125f, .999f, .06f, .001f, .999f, (float)Master.rand.NextDouble() * Master.TwoPI, ((float)Master.rand.NextDouble() - .5f) * .15f, .985f, .2f, .99f));
        }

        internal void addDarkSmokeParticle(Vector2 pos)
        {
            pos += Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .15f;

            particles.Add(new Particle(Master.fxTextures[6], pos, new Vector2(0, -.03f) + Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .000125f, .999f,
                .05f, .00075f, .999f,
                (float)Master.rand.NextDouble() * Master.TwoPI, ((float)Master.rand.NextDouble() - .5f) * .15f, .985f,
                .25f, .99f));
        }

        internal void addStoneSmokeParticle(Vector2 pos)
        {
            pos += Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .15f;

            particles.Add(new Particle(Master.fxTextures[2], pos, new Vector2(0, -.03f) + Master.VectorFromAngle((float)Master.rand.NextDouble() * Master.TwoPI) * .000125f, .999f,
                .05f, .00125f, .999f,
                (float)Master.rand.NextDouble() * Master.TwoPI, ((float)Master.rand.NextDouble() - .5f) * .25f, .85f,
                .25f, .99f));
        }
    }

    public struct Particle
    {
        public Texture2D texture;
        public Vector2 position, velocity;
        public float velocityFactor;
        public float size, sizeAdd, sizeAddCooldown;
        public float rotation, rotationAdd, rotationAddCooldown;
        public float alpha, alphaFactor;

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity, float velocityFactor, float size, float sizeAdd, float sizeAddCooldown, float rotation, float rotationAdd, float rotationAddCooldown, float alpha, float alphaFactor) : this()
        {
            this.texture = texture;
            this.position = position;
            this.velocity = velocity;
            this.velocityFactor = velocityFactor;
            this.size = size;
            this.sizeAdd = sizeAdd;
            this.sizeAddCooldown = sizeAddCooldown;
            this.rotation = rotation;
            this.rotationAdd = rotationAdd;
            this.rotationAddCooldown = rotationAddCooldown;
            this.alpha = alpha;
            this.alphaFactor = alphaFactor;
        }
    }
}
