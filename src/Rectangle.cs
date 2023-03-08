using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestMod
{
    class Rectangle
    {
        public Vector2 center;
        public Vector2[] corners;
        public float width;
        public float height;
        public float angleRad;

        public Rectangle(Vector2 center, float width, float height)
        {
            this.center = center;
            this.width = width;
            this.height = height;
            corners = new Vector2[4];
            UpdateCornerPointsWithAngle(90f);
        }

        

        public void Move(Vector2 velocity)
        {
            center += velocity * Time.deltaTime;
        }

        public void UpdateCornerPoints()
        {
            //Debug.Log(center);
            // Define the corner points of the rectangle
            corners[0] = new Vector2(center.x - width / 2f, center.y - height / 2f); // Bottom Left
            corners[1] = new Vector2(center.x + width / 2f, center.y - height / 2f); // Bottom Right
            corners[2] = new Vector2(center.x + width / 2f, center.y + height / 2f); // Top Left
            corners[3] = new Vector2(center.x - width / 2f, center.y + height / 2f); // Top Right

            // Define the rotation matrix (Doesnt work right now)
            //Matrix4x4 rotationMatrix = new();
            //rotationMatrix.SetColumn(0, new Vector4(Mathf.Cos(angleRad), -Mathf.Sin(angleRad), 0f, 0f));
            //rotationMatrix.SetColumn(1, new Vector4(Mathf.Sin(angleRad), Mathf.Cos(angleRad), 0f, 0f));
            //rotationMatrix.SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
            //rotationMatrix.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));

            // Apply the rotation matrix to each corner point
            //for (int i = 0; i < 4; i++)
            //{
            //    corners[i] = rotationMatrix * new Vector4(corners[i].x, corners[i].y, 0f, 1f);
            //}
        }

        public void UpdateCornerPointsWithAngle(float angle)
        {
            // Define the corner points of the rectangle
            //Debug.Log(center);
            corners[0] = new Vector2(center.x - width / 2f, center.y - height / 2f); // Bottom Left
            
            corners[1] = new Vector2(center.x + width / 2f, center.y - height / 2f); // Bottom Right
            
            corners[2] = new Vector2(center.x - width / 2f, center.y + height / 2f); // Top Left
            
            corners[3] = new Vector2(center.x + width / 2f, center.y + height / 2f); // Top Right
            

            // Convert the rotation angle from degrees to radians
            angleRad = angle * Mathf.Deg2Rad;

            // Define the rotation matrix (doesnt work right now)
            Matrix4x4 rotationMatrix = new();
            rotationMatrix.SetColumn(0, new Vector4(Mathf.Cos(angleRad), -Mathf.Sin(angleRad), 0f, 0f));
            rotationMatrix.SetColumn(1, new Vector4(Mathf.Sin(angleRad), Mathf.Cos(angleRad), 0f, 0f));
            rotationMatrix.SetColumn(2, new Vector4(0f, 0f, 1f, 0f));
            rotationMatrix.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));

            // Apply the rotation matrix to each corner point
            //for (int i = 0; i < 4; i++)
            //{
            //    corners[i] = rotationMatrix * new Vector4(corners[i].x, corners[i].y, 0f, 1f);
            //}

            //Debug.Log(corners[0]);
            //Debug.Log(corners[1]);
            //Debug.Log(corners[2]);
            //Debug.Log(corners[3]);
        }
    }
}
