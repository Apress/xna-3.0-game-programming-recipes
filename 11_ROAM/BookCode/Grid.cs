using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace BookCode
{
    class Grid
    {
        public bool[,] criticalPoints;
        int resolution;

        public Grid(int resolution)
        {
            this.resolution = resolution;
            criticalPoints = new bool[resolution, resolution];

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    criticalPoints[x, y] = true;
                }
            }
            //criticalPoints[3, 0] = true;
          //  criticalPoints[2, 4] = true;
        }

        public void UpdateCriticalPoints(int res, BoundingFrustum bf, float[,] heights)
        {
            InScreen(new Vector2(0, 0), res, bf, heights);
            //criticalPoints[30, 33] = true;
            //criticalPoints[3, 3] = true;
        }

        public void InScreen(Vector2 p, int size, BoundingFrustum bf, float[,] heights)
        {
            Vector3 tlPoint = new Vector3(p.X, heights[(int)p.X, (int)p.Y + size], -(p.Y + size));
            Vector3 trPoint = new Vector3(p.X + size, heights[(int)p.X + size, (int)p.Y + size], -(p.Y + size));
            Vector3 blPoint = new Vector3(p.X, heights[(int)p.X, (int)p.Y], -p.Y);
            Vector3 brPoint = new Vector3(p.X + size, heights[(int)p.X + size, (int)p.Y], -p.Y);
            Vector3 midPoint = new Vector3(p.X + size / 2, heights[(int)p.X + size / 2, (int)p.Y + size / 2], -(p.Y + size / 2));            

            bool inSight = InScreen(tlPoint, bf);
            inSight |= InScreen(trPoint, bf);
            inSight |= InScreen(blPoint, bf);
            inSight |= InScreen(brPoint, bf);

            if (inSight)
            {
                criticalPoints[(int)p.X, (int)p.Y] = true;
                criticalPoints[(int)p.X + size, (int)p.Y] = true;
                criticalPoints[(int)p.X, (int)p.Y + size] = true;
                criticalPoints[(int)p.X + size, (int)p.Y + size] = true;

                if (size > 2)
                {
                    InScreen(new Vector2(p.X, p.Y + size / 2), size / 2, bf, heights);
                    InScreen(new Vector2(p.X + size / 2, p.Y + size / 2), size / 2, bf, heights);
                    InScreen(new Vector2(p.X + size / 2, p.Y), size / 2, bf, heights);
                    InScreen(new Vector2(p.X, p.Y), size / 2, bf, heights);
                }
            }

        }

        public bool InScreen(Vector3 p, BoundingFrustum bf)
        {
            if (bf.Contains(p) != ContainmentType.Disjoint)
                return true;
            else
                return false;
        }

        public void PropagateCriticals(int resolution)
        {
            Propagate(new Vector2(0, 0), resolution-1);
        }

        private void Propagate(Vector2 p, int size)
        {
            if (size > 2)
            {                
                Propagate(p + new Vector2(0, size / 2), size / 2);
                Propagate(p + new Vector2(size / 2, size / 2), size / 2);                
                Propagate(p + new Vector2(size / 2, 0), size / 2);
                Propagate(p, size / 2);
            }

            bool isCritical = criticalPoints[(int)(p.X + size / 2), (int)(p.Y + size / 2)];            
            
            isCritical |= criticalPoints[(int)(p.X), (int)(p.Y + size/2)];
            isCritical |= criticalPoints[(int)(p.X + size / 2), (int)(p.Y + size)];
            isCritical |= criticalPoints[(int)(p.X + size), (int)(p.Y + size/2)];
            isCritical |= criticalPoints[(int)(p.X + size/2), (int)(p.Y)];            

            if (isCritical)
            {
                /*criticalPoints[(int)(p.X), (int)(p.Y)] = true;
                criticalPoints[(int)(p.X + size), (int)(p.Y)] = true;
                criticalPoints[(int)(p.X), (int)(p.Y + size)] = true;
                criticalPoints[(int)(p.X + size), (int)(p.Y + size)] = true;*/
                criticalPoints[(int)(p.X + size/2), (int)(p.Y + size/2)] = true;
            }
        }
    }
}
