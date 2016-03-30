﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSJam
{
    public class Camera
    {
        public Vector2 AimPos, currentPos;
        private float shake = 0f, shakefalloff = 0.9f;
        public float responsiveness = 0.05f;
        public float zoomResponsiveness = 0.6f;
        public Vector2 zoom = new Vector2(.06f, .06f * .66f);
        public Vector2 zoomAim = new Vector2(2, 2 * .66f);

        List<Vector2> positions = new List<Vector2>();

        public void CreateLookAt(Vector2 pos)
        {
            AimPos = pos;
        }

        public Matrix getTransform(bool doStuff)
        {
            if (doStuff)
            {
                currentPos = AimPos * responsiveness + currentPos * (1f - responsiveness);
                zoom = zoomAim * zoomResponsiveness + zoom * (1 - zoomResponsiveness);
            }

            return Matrix.CreateTranslation(new Vector3(-AimPos, 0.0f)) *
               //Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(zoom.X, zoom.Y, 1.0f) *
               Matrix.CreateTranslation(new Vector3(MainGame.width / 2f, MainGame.height / 2f, 0.0f));

        }
    }
}
