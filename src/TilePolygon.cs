using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace TestMod
{
    class TilePolygon
    {
        public UnityEngine.Vector2 center;
        public UnityEngine.Vector2[] corners;
        private List<Vector> edges = new List<Vector>();

        // For tiles
        public TilePolygon(Vector2 center)
        {
            //Debug.Log("reached tile polygon cons");
            this.center = center;
            corners = new Vector2[4] { new Vector2(center.x - 5, center.y - 5), new Vector2(center.x - 5, center.y + 5), new Vector2(center.x + 5, center.y + 5), new Vector2(center.x + 5, center.y - 5) };
            //Debug.Log("Finished tile cons");
            edges = new List<Vector>();


            // Define edges
            BuildEdges();
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
