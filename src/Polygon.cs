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
    public class Polygon
    {
        public BodyChunk center;
        public Vector2 PivotPoint;
        public UnityEngine.Vector2[] corners;
        public UnityEngine.Vector2[] lastcorners;
        public UnityEngine.Vector2[] lastlastcorners;
        private List<Polygon.Edge> edges = new List<Edge>();
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
            edges = new List<Edge>();
            UpdateCornerPoints();
            angleDeg = 1f;
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
           

            // Loop through each corner point
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Custom.RotateAroundOrigo(corners[i], angleDeg);
                corners[i] += center.pos+center.vel;
            }
            BuildEdges();
        }

        //public void UpdateCornerPointsGrip(Vector2 gripPos)
        //{
        //    this.lastcorners.CopyTo(this.lastlastcorners, 0);
        //    this.corners.CopyTo(this.lastcorners, 0);


        //    // Define the corner points of the shape

        //    for (int i = 0; i < corners.Length; i++)
        //    {
        //        corners[i] = originalCorners[i] * new Vector2(width, height);
        //    }

        //    // Loop through each corner point
        //    for (int i = 0; i < corners.Length; i++)
        //    {
        //        //corners[i] = Custom.RotateAroundOrigo(corners[i], angleDeg);
        //        corners[i] += center.pos + center.vel;
        //        corners[i]-=gripPos;
        //        corners[i] = Custom.RotateAroundOrigo(corners[i],angleDeg);
        //        corners[i] += center.pos + center.vel;
        //    }
        //    // center.pos += center.vel;
           

        //    //center.pos -= gripPos;
        //    //center.pos = Custom.RotateAroundOrigo(center.pos, angleDeg);
        //    //center.pos += gripPos;
        //    // center.pos +=  center.vel;

        //    BuildEdges();
        //}

        public void UpdateCornerPointsWithAngle(float angleAdded)
        {
            this.lastcorners.CopyTo(this.lastlastcorners, 0);
            this.corners.CopyTo(this.lastcorners, 0);
            // Define the corner points of the shape
            angleDeg += angleAdded;
            angleDeg = angleDeg%360;
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = originalCorners[i] * new Vector2(width, height);
            }

           

            // Loop through each corner point
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Custom.RotateAroundOrigo(corners[i], angleDeg);
                corners[i] += center.pos + center.vel;

            }
           
            BuildEdges();
        }

        public void BuildEdges()
        {
            edges.Clear();
            for (int i = 0; i < corners.Length; i++)
            {             
                edges.Add(new Edge(corners[i], corners[(i+1)% corners.Length]));
            }
        }

        public List<Edge> Edges
        {
            get { return edges; }
        }

        public class Edge
        {
            public Vector2 p1;
            public Vector2 p2;
            public Edge(Vector2 p1, Vector2 p2) { this.p1 = p1; this.p2 = p2; }
            public bool Equal(Edge e)
            {
                if ((e.p1 == this.p1 && e.p2 == this.p2) || (e.p2 == this.p1 && e.p1 == this.p2))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
