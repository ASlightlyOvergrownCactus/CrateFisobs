using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector2 = UnityEngine.Vector2;
namespace TestMod
{
    public static class PolygonMerger
    {
        public static List<TilePolygon> Merge(List<TilePolygon> tiles)

        {   List<TilePolygon> r= new List<TilePolygon>();

            Vector2 center= Vector2.zero;

            foreach(TilePolygon tpp in tiles)
            {
                center += tpp.center;
            }
            center /=tiles.Count;
            List<TilePolygon.Edge> OutLine = new List<TilePolygon.Edge>();
            for (int i = 0; i < tiles.Count; i++)
            {
                for (int t = 0; t < tiles[i].edges.Count; t++)
                {
                    OutLine.Add(tiles[i].edges[t]);
                }

            }

            for (int i = 0; i < OutLine.Count; i++)
            {
                TilePolygon.Edge ip = OutLine[i];
                for (int p = 0; p < OutLine.Count; p++)
                {
                    TilePolygon.Edge pp = OutLine[p];
                    if (p != i)
                    {
                        if (OutLine[i].Equal(OutLine[p]))
                        {
                            OutLine.Remove(ip);
                            OutLine.Remove(pp);
                            i = 0;
                            break;

                        }
                    }
                }
            }

            for (int i = 0; i < OutLine.Count - 1; i++)
            {
                for (int p = 0; p < OutLine.Count; p++)
                {
                    if (p == i) { continue; }
                    if (OutLine[i].p2 == OutLine[p].p1)
                    {
                        TilePolygon.Edge e = OutLine[i + 1];
                        OutLine[i + 1] = OutLine[p];
                        OutLine[p] = e;

                        break;
                    }
                }

            }

            //for (int i = 0; i < OutLine.Count; i++)
            //{
            //    TilePolygon.Edge e = OutLine[i];
            //    int xCounter = 1; int yCounter = 1;
            //    for (int p = i + 1; i < OutLine.Count - 3; p++)
            //    {
            //        if (e.p1.x == OutLine[p].p1.x)
            //        {
            //            xCounter++;
            //        }
            //        if (e.p1.y == OutLine[p].p1.y)
            //        {
            //            yCounter++;
            //        }
            //        if (!e.Equal(OutLine[p]))
            //        {
            //            if (xCounter >= 3 && yCounter >= 3)
            //            {
            //                for (int o = i + 1; o < i + (xCounter >= 3 ? xCounter : yCounter) - 1; o++)
            //                {
            //                    OutLine.RemoveAt(o);
            //                }
            //            }
            //            break;
            //        }
            //    }
            //}
            Vector2[] vector2s = new Vector2[OutLine.Count*2];
            int index=0;
            for(int i=0;i<OutLine.Count;i++)
            {
                vector2s[index] = OutLine[i].p1;
                index++;
                vector2s[index] = OutLine[i].p2;
                index++;
            }

            List<TilePolygon> tp =  new List<TilePolygon>() { new TilePolygon(center, TilePolygon.DefaultShape.others, vector2s) };


            return tp;

        }
    }
}
 


