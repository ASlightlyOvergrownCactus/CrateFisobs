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
        public float angleRad;

        public Rectangle(UnityEngine.Vector2 center, float width, float height)
        {
            this.center = center;
            this.width = width;
            this.height = height;
            corners = new UnityEngine.Vector2[4];
            UpdateCornerPointsWithAngle(90f);
        }

        

        public void Move(UnityEngine.Vector2 velocity)
        {
            center += velocity * Time.deltaTime;
        }

        public void UpdateCornerPoints()
        {
            //Debug.Log(center);
            // Define the corner points of the rectangle
            corners[0] = new UnityEngine.Vector2(0 - width / 2f, 0 - height / 2f); // Bottom Left
            corners[1] = new UnityEngine.Vector2(0 + width / 2f, 0 - height / 2f); // Bottom Right
            corners[2] = new UnityEngine.Vector2(0 + width / 2f, 0 + height / 2f); // Top Left
            corners[3] = new UnityEngine.Vector2(0 - width / 2f, 0 + height / 2f); // Top Right

            angleRad = 45f * Mathf.Deg2Rad;
            // Loop through each corner point
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = RWCustom.Custom.RotateAroundOrigo(corners[i], 45f);
                corners[i] += center;
            }
        }

        public void UpdateCornerPointsWithAngle(float angleAdded)
        {
            // Define the corner points of the rectangle
            //Debug.Log(center);
            corners[0] = new UnityEngine.Vector2(0 - width / 2f, 0 - height / 2f); // Bottom Left
            corners[1] = new UnityEngine.Vector2(0 + width / 2f, 0 - height / 2f); // Bottom Right
            corners[2] = new UnityEngine.Vector2(0 + width / 2f, 0 + height / 2f); // Top Left
            corners[3] = new UnityEngine.Vector2(0 - width / 2f, 0 + height / 2f); // Top Right

            // Loop through each corner point
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = RWCustom.Custom.RotateAroundOrigo(corners[i], 45f);
                corners[i] += center;
            }

        }
    }
}
