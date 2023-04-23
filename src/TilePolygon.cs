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
        private List<Vector2> edges = new List<Vector2>();

        // For tiles
        public TilePolygon(Vector2 center)
        {
            //Debug.Log("reached tile polygon cons");
            this.center = center;
            corners = new Vector2[4] { new Vector2(center.x - 10, center.y - 10), new Vector2(center.x - 10, center.y + 10), new Vector2(center.x + 10, center.y + 10), new Vector2(center.x + 10, center.y - 10) };
            //Debug.Log("Finished tile cons");
            edges = new List<Vector2>();


            // Define edges
            BuildEdges();
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
