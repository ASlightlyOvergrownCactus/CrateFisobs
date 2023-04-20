using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Drawing.Drawing2D;

namespace TestMod
{
    class Polygon
    {
        public UnityEngine.Vector2 center;
        public UnityEngine.Vector2[] corners;
        private List<Vector> edges = new List<Vector>();
        public float width;
        public float height;
        public float angleDeg;
        public List<TilePolygon> collisionContainer; 
        private Vector2[] originalCorners;

        public Polygon(UnityEngine.Vector2 center, float width, float height, Vector2[] origCorners)
        {
            this.center = center;
            this.width = width;
            this.height = height;
            originalCorners = origCorners;
            corners = new UnityEngine.Vector2[originalCorners.Length];
            edges = new List<Vector>();
            UpdateCornerPointsWithAngle(0f);
            angleDeg = 0f;
            Debug.Log("Adding actual polygon list!");
            collisionContainer = new List<TilePolygon>();
            
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

            // Define edges
            BuildEdges();

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

            // Define Edges
            BuildEdges();

            // Loop through each corner point
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = RWCustom.Custom.RotateAroundOrigo(corners[i], 45f);
                corners[i] += center;
            }

        }

        public void BuildEdges()
        {
            Vector p1;
            Vector p2;
            Edges.Clear();
            for (int i = 0; i < corners.Length; i++)
            {
                p1 = new Vector(corners[i].x, corners[i].y);
                if (i + 1 >= corners.Length)
                {
                    p2 = new Vector(corners[0].x, corners[0].y);
                }
                else
                {
                    p2 = new Vector(corners[i + 1].x, corners[i + 1].y);
                }
                edges.Add(p2 - p1);
            }
        }

        public List<Vector> Edges
        {
            get { return edges; }
        }
    }
}
