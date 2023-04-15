using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Drawing.Drawing2D;

namespace TestMod
{
    class Rectangle
    {
        public UnityEngine.Vector2 center;
        public UnityEngine.Vector2[] corners;
        public float width;
        public float height;
        public float angleDeg;
        public List<RWCustom.IntVector2> collisionContainer; 
        private Vector2[] originalCorners;

        public Rectangle(UnityEngine.Vector2 center, float width, float height, Vector2[] origCorners)
        {
            this.center = center;
            this.width = width;
            this.height = height;
            originalCorners = origCorners;
            corners = new UnityEngine.Vector2[originalCorners.Length];
            UpdateCornerPointsWithAngle(0f);
            angleDeg = 0f;

            collisionContainer = new List<RWCustom.IntVector2>();
            collisionContainer.Add(new RWCustom.IntVector2(0, 0));
        }

        

        public void Move(UnityEngine.Vector2 velocity)
        {
            center += velocity * Time.deltaTime;
        }

        public void UpdateCornerPoints()
        {
            //Debug.Log(center);
            // Define the corner points of the shape

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = originalCorners[i];
            }

            // Loop through each corner point
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = RWCustom.Custom.RotateAroundOrigo(corners[i], 45f);
                corners[i] += center;
            }
        }

        public void UpdateCornerPointsWithAngle(float angleAdded)
        {
            // Define the corner points of the shape

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = originalCorners[i];
            }

            // Loop through each corner point
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = RWCustom.Custom.RotateAroundOrigo(corners[i], 45f);
                corners[i] += center;
            }

        }
    }
}
