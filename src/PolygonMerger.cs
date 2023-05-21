using IL.RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Custom=RWCustom.Custom;
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
                for (int p = i; p < OutLine.Count; p++)
                {
                    if (p == i) { continue; }
                    if (OutLine[i].p2 == OutLine[p].p1 && OutLine[i].p2 != OutLine[i+1].p1)
                    {

                        (OutLine[i + 1], OutLine[p]) = (OutLine[p], OutLine[i + 1]);
                        break;
                    }
                }

            }

    
            List<Vector2> vector2s = new List<Vector2>();
            
            for(int i=0;i<OutLine.Count;i++)
            {
                vector2s.Add(OutLine[i].p1);
                vector2s.Add(OutLine[i].p2);
               
            }

            List<List<Vector2>> SplitedTiles = new List<List<Vector2>>();

            int LastShapeindex = 0;
            for (int i = 0; i < vector2s.Count - 1; i++)
            {
                if (Custom.Dist(vector2s[i], vector2s[i + 1]) > 30 || i + 1 == vector2s.Count-1)//(Math.Abs(vector2s[i].x - vector2s[i+1].x)>25|| Math.Abs(vector2s[i].y - vector2s[i + 1].y) > 25)
                {

                    SplitedTiles.Add(new List<Vector2>());
                    for (int j = LastShapeindex; j <= i; j++)
                    {
                        SplitedTiles[SplitedTiles.Count - 1].Add(vector2s[j]);
                    }
                    LastShapeindex = i+1;
                }

            }

            List<TilePolygon> tp = new List<TilePolygon>();

            foreach (List<Vector2> l2 in SplitedTiles)
            {



                //for (int i = 0; i < l2.Count; i++)
                //{
                //    List<Vector2> SameX = new List<Vector2>();
                //    List<Vector2> SameY = new List<Vector2>();

                //    int num = 1;
                //    while (l2[(i + num) % l2.Count].x == l2[i].x)
                //    {
                //        SameX.Add(l2[(i + num) % l2.Count]);
                //        num++;

                //    }

                //    if (SameX.Count >= 1) SameX.RemoveAt(SameX.Count - 1);
                //    num = 1;
                //    while (l2[(i + num) % l2.Count].y == l2[i].y)
                //    {

                //        SameY.Add(l2[(i + num) % l2.Count]);
                //        num++;

                //    }
                //    if (SameY.Count >= 1) SameY.RemoveAt(SameY.Count - 1);
                //    if (SameX.Count >= 1 || SameY.Count >= 1)
                //    {

                //        foreach (Vector2 v in SameX.Count >= 1 ? SameX : SameY)
                //        {
                //            l2.Remove(v);
                //        }
                //      //  i = 0;
                //    }
                //}

                tp.Add(new TilePolygon(center, TilePolygon.DefaultShape.others, l2.ToArray()));

            }

           // tp =  new List<TilePolygon>() { new TilePolygon(center, TilePolygon.DefaultShape.others, vector2s.ToArray()) };

           

                return tp;

        }
    }
}
 


