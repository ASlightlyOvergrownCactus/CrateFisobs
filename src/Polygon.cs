using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Drawing.Drawing2D;
using RWCustom;
namespace TestMod
{
    class Polygon
    {
        public BodyChunk center;
        public UnityEngine.Vector2[] corners;
        public UnityEngine.Vector2[] lastcorners;
        public UnityEngine.Vector2[] lastlastcorners;
        private List<Vector2> edges = new List<Vector2>();
        public float width;
        public float height;
        public float angleDeg;
        public List<TilePolygon> collisionContainer; 
        private Vector2[] originalCorners;

        public Polygon(BodyChunk center, float width, float height, Vector2[] origCorners)
        {
            this.center = center;
            this.width = width;
            this.height = height;
            originalCorners = origCorners;
            corners = new UnityEngine.Vector2[originalCorners.Length];
            lastcorners = new UnityEngine.Vector2[originalCorners.Length];
            lastlastcorners = new UnityEngine.Vector2[originalCorners.Length];
            edges = new List<Vector2>();
            UpdateCornerPoints();
            angleDeg = 45f;
            Debug.Log("Adding actual polygon list!");
            collisionContainer = new List<TilePolygon>();
            
        }
        public Polygon(BodyChunk center, float scale, int NumberOfPoint,float ang=0)
        {
            this.center = center;
            this.width = scale;
            this.height = scale;
            this.angleDeg = ang;
            
            originalCorners = new Vector2[NumberOfPoint];
            for(int i = 0;i < NumberOfPoint; i++) { originalCorners[i] = Custom.RotateAroundOrigo(Vector2.up*scale, (360/NumberOfPoint)*i); }
            this.corners = new Vector2[NumberOfPoint];
            this.lastcorners = new Vector2[NumberOfPoint];
            this.lastlastcorners = new Vector2[NumberOfPoint];


            UpdateCornerPoints();
            Debug.Log("Adding actual polygon list!");
            collisionContainer = new List<TilePolygon>();
            
        }

        public Vector2 vel(int corner)
        {
            return this.lastcorners[corner] -this.corners[corner];
        }

        public void Move(UnityEngine.Vector2 velocity)
        {
            center.vel += velocity * Time.deltaTime;
        }
        public void ChangeSize(float s)
        {
            this.width = s;
            this.height = s;
        }

        public void ChangeSize(float s,float t)
        {
            this.width = s;
            this.height = t;
        }
        public void UpdateCornerPoints()
        {
            this.lastcorners.CopyTo(this.lastlastcorners, 0);
            this.corners.CopyTo(this.lastcorners, 0);
            
            
            // Define the corner points of the shape

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = originalCorners[i]*new Vector2(width,height);
            }

            // Define edges
            BuildEdges();

            // Loop through each corner point
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Custom.RotateAroundOrigo(corners[i], angleDeg);
                corners[i] += center.pos+center.vel;
            }
           
        }

        public void UpdateCornerPointsWithAngle(float angleAdded)
        {
            this.lastcorners.CopyTo(this.lastlastcorners, 0);
            this.corners.CopyTo(this.lastcorners, 0);
            // Define the corner points of the shape
            angleDeg += angleAdded;
            angleDeg = angleDeg%359;
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = originalCorners[i] * new Vector2(width, height);
            }

            // Define Edges
            BuildEdges();

            // Loop through each corner point
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = RWCustom.Custom.RotateAroundOrigo(corners[i], angleDeg);
                corners[i] += center.pos + center.vel;
            }

        }

        public void BuildEdges()
        {
            Vector2 p1;
            Vector2 p2;
            Edges.Clear();
            for (int i = 0; i < corners.Length; i++)
            {
                p1 = new Vector2(corners[i].x, corners[i].y);
                if (i + 1 >= corners.Length)
                {
                    p2 = new Vector2(corners[0].x, corners[0].y);
                }
                else
                {
                    p2 = new Vector2(corners[i + 1].x, corners[i + 1].y);
                }
                edges.Add(p2 - p1);
            }
        }

        public List<Vector2> Edges
        {
            get { return edges; }
        }
    }
}
