using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using RWCustom;
namespace TestMod
{
    public static class Helper
    {
        public static bool IsEven(int n)
        {
            return n%2== 0;
        }
        public static bool IsBetween(float a, float b,float c)
        {
            return (a <= b && b <= c)|| (c <= b && b <= a);
        }

        public static bool AABB(Vector2 Line1a,Vector2 Line1b,Vector2 Line2a,Vector2 Line2b,Vector2 p)
        {
            //some small margin #BandageFix
            float Margin = 0f;
            Vector2 TopLeftA = new Vector2(Math.Min(Line1a.x, Line1b.x)- Margin, Math.Max(Line1a.y,Line1b.y)+ Margin);
            Vector2 RightDownA=new Vector2(Math.Max(Line1a.x, Line1b.x)+ Margin, Math.Min(Line1a.y, Line1b.y)- Margin);

            Vector2 TopLeftB = new Vector2(Math.Min(Line2a.x, Line2b.x)- Margin, Math.Max(Line2a.y, Line2b.y)+ Margin);
            Vector2 RightDownB = new Vector2(Math.Max(Line2a.x, Line2b.x)+ Margin, Math.Min(Line2a.y, Line2b.y)- Margin);

            bool X= IsBetween(TopLeftA.x, p.x, RightDownA.x) && IsBetween(TopLeftB.x,p.x,RightDownB.x);
            bool Y= IsBetween(RightDownA.y, p.y, TopLeftA.y) && IsBetween(RightDownB.y, p.y, TopLeftB.y);

            return X && Y;
        }

        public static Vector2 LineCollide(Vector2 line_r1s,Vector2 line_r1e,Vector2 line_r2s, Vector2 line_r2e)
        {
            Vector2 CollisionPoint;

            float x1 = line_r1s.x;
            float x2 = line_r1e.x;

            float y1 = line_r1s.y;
            float y2 = line_r1e.y;

            float x3 = line_r2s.x;
            float x4 = line_r2e.x;

            float y3 = line_r2s.y;
            float y4 = line_r2e.y;

            float d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

            float px = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / d;
            float py = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / d;
            CollisionPoint = new Vector2(px, py);




           // if (d == 0) { return Vector2.positiveInfinity; }

            return CollisionPoint;
        }

        public static int VeloInSlopequadrant(Vector2 p1,Vector2 p2,Vector2 velo)
        {
            float slope = (p2.y -p1.y) / (p2.x-p1.x);
            float CompareY = velo.x * slope;
            if(velo.y>CompareY) { return 1; }
            if(velo.y<CompareY) { return -1; }
            return 0;
        }

        public static int PInSlopequadrant(Vector2 p1, Vector2 p2, Vector2 p)
        {
            float slope = (p2.y - p1.y) / (p2.x - p1.x);
            float c = p1.y - slope*p1.x;
            float CompareY = p.x * slope+c;
            if (p.y > CompareY) { return 1; }
            if (p.y < CompareY) { return -1; }
            return 0;
        }
        public static Vector2 PolyPushOutOfLine(Vector2 polyCenter,Vector2 p1,Vector2 p2,Vector2 p3,Vector2 p4,TilePolygon tp,Vector2 velocity)
        {
           
            Vector2[] PushPoint = new Vector2[2] { Vector2.one*10000, Vector2.one * 10000 };

           //Vertical Slope=0 Horizontal Slope =2
            if (p3.x - p4.x == 0)
            {
                if (velocity.x > 0)
                {
                    if (p1.x > p2.x)
                    {
                        return new Vector2(p3.x - p1.x, 0);
                    }
                    else
                    {
                        return new Vector2(p3.x - p2.x, 0);
                    }
                }
                else if (velocity.x < 0)
                {
                    if (p1.x < p2.x)
                    {
                        return new Vector2(p3.x - p1.x, 0);
                    }
                    else
                    {
                        return new Vector2(p3.x - p2.x, 0);
                    }
                }

            }
            else if (p3.y - p4.y == 0)
            {
                if (velocity.y > 0)
                {
                    if (p1.y > p2.y)
                    {
                        return new Vector2(0, p3.y - p1.y);

                    }
                    else
                    {
                        return new Vector2(0, p3.y - p2.y);
                    }
                }
                else if (velocity.y < 0)
                {
                    if (p1.y < p2.y)
                    {
                        return new Vector2(0, p3.y - p1.y);

                    }
                    else
                    {
                        return new Vector2(0, p3.y - p2.y);
                    }
                }
            }

    
            int velocityQuadrant = VeloInSlopequadrant(p3, p4, velocity);


            switch (velocityQuadrant)
            {
                case 1:
                    if(PInSlopequadrant(p3,p4,p1)>0)
                    {
                        PushPoint[0] = p1;
                    }
                    if (PInSlopequadrant(p3, p4,p2)>0)
                    {
                        PushPoint[1] = p2;
                    }
                    break;
                case -1:
                    if (PInSlopequadrant(p3, p4, p1) < 0)
                    {
                        PushPoint[0] = p1;
                    }
                    if (PInSlopequadrant(p3, p4, p2) < 0)
                    {
                        PushPoint[1] = p2;
                    }
                    break;

            }
            int SameSide=0;

            for(int i=0; i<PushPoint.Length; i++)
            {
               if( PushPoint[i]!= Vector2.one * 10000) SameSide ++;
            }
            
            



            if (SameSide==2)
            {





                float m=(p3.y-p4.y)/(p3.x-p4.x);
                float c = p3.y - m * p3.x;

                double d1 = (m * PushPoint[0].x +c- PushPoint[0].y) / Math.Sqrt(1 + m * m);
                double d2 = (m * PushPoint[1].x +c- PushPoint[1].y) / Math.Sqrt(1 + m * m);

                if (d1 > d2)
                {
                    Vector2 d1PlusVelo = PushPoint[0]+velocity;
                    float d1Slope;

                    if (PushPoint[0].x - d1PlusVelo.x == 0)
                    {
                        d1Slope = 1;
                        return Vector2.zero;
                    }
                    else
                    {
                        d1Slope = (PushPoint[0].y - d1PlusVelo.y) / (PushPoint[0].x - d1PlusVelo.x);
                    }
                    float d1c = PushPoint[0].y-d1Slope * PushPoint[0].x ;
                    float projx = (c-d1c)/(d1Slope-m);
                    Vector2 proj = new Vector2(projx, m * projx + c);
                    Vector2 push = proj - PushPoint[0];
                    return push;
                }
                else
                {
                    Vector2 d2PlusVelo = PushPoint[1] + velocity;
                    float d2Slope;
                    if (PushPoint[1].x - d2PlusVelo.x == 0)
                    {
                        d2Slope = 1;
                        return Vector2.zero;
                    }
                    else
                    {
                        d2Slope = (PushPoint[1].y - d2PlusVelo.y) / (PushPoint[1].x - d2PlusVelo.x);
                    }
                    float d2c = PushPoint[1].y - d2Slope * PushPoint[1].x;
                    float projx = (c - d2c) / (d2Slope - m);
                    Vector2 proj = new Vector2(projx, m * projx + c);
                    Vector2 push = proj - PushPoint[1];
                    return push;
                   
                }

            }
            else if(SameSide == 1)
            {
                for (int i = 0; i < PushPoint.Length; i++)
                {
                    if (PushPoint[i] != Vector2.one * 10000)
                    {
                        float m = (p3.y - p4.y) / (p3.x - p4.x);
                        float c = p3.y - m * p3.x;

                       

                        Vector2 d1PlusVelo = PushPoint[i] + velocity;
                        float d1Slope;
                        if (PushPoint[i].x - d1PlusVelo.x == 0)
                        {
                            d1Slope = 1;
                            return Vector2.zero;
                        }
                        else
                        {
                            d1Slope = (PushPoint[i].y - d1PlusVelo.y) / (PushPoint[i].x - d1PlusVelo.x);
                        }
                        float d1c = PushPoint[i].y - d1Slope * PushPoint[i].x;
                        float projx = (c - d1c) / (d1Slope - m);
                        Vector2 proj = new Vector2(projx, m * projx + c);
                        Vector2 push = proj - PushPoint[i];
                        return push;
                    }
                }
            }
                return Vector2.zero;
            //if (velocity == Vector2.zero) return Vector2.zero;
            //Vector2 PushVector=Vector2.zero;
            ////1. figure out which side of line is inside the Line
            ////2. Find furtherest point inside the Line
            ////3. return Vec that Push point to Line using reverse velocity
            //// i love y=mx+c
            //int PushSign=VeloInSlopequadrant(p3,p4,velocity);
            //float slope = (p4.y - p3.y) / (p4.x-p3.x);
            //float c = p3.y -(p3.x* slope);
            //List<Vector2> PointInLine= new List<Vector2>();
            //for(int i=0; i<corners.Length; i++) 
            //{
            //    if (corners[i].x > tp.corners[0].x&& corners[i].x < tp.corners[2].x && corners[i].y > tp.corners[1].y&& corners[i].y < tp.corners[0].y)
            //    {
            //        switch (PushSign)
            //        {
            //            case 1:
            //                if (corners[i].x * slope + c > corners[i].y)
            //                {
            //                    PointInLine.Add(corners[i]);
            //                }
            //                break;
            //            case -1:
            //                if (corners[i].x * slope + c < corners[i].y)
            //                {
            //                    PointInLine.Add(corners[i]);
            //                }
            //                break;
            //        }
            //    }
            //}
            
            //if (PointInLine.Count == 0)
            //{
            //    Debug.Log("No Points are in Tile!");
            //    Vector2 point1 = collisionPoint+new Vector2(collisionPoint.y,-collisionPoint.x);
            //    foreach(Vector2 point in tp.corners)
            //    {
                     
            //    }
            //}

               
             

            //Vector2 ChosenPoint = Vector2.negativeInfinity;
            //float distance = 0;
            //foreach (Vector2 point in PointInLine) 
            //{
            //    float dp = 0;
            //    float d = RWCustom.Custom.Dist(p3, p4);
            //    if(d!=0)
            //    {
            //        float t = ((point.x - p3.x) * (p4.x - p3.x) + (point.y - p3.y) * (p4.x - p3.x)) / d;
            //        t = Math.Max(0, Math.Min(1, t));
            //        dp =(float) Math.Sqrt(Custom.Dist(point, new Vector2(p3.x + t * (p4.x - p3.x),p3.y+t*(p4.y-p3.y))) );
            //    }else
            //    {
            //        dp=(float)Math.Sqrt(Custom.Dist(point,p3));
            //    }

            //    if(dp>distance) 
            //    {
            //        distance = dp;
            //        ChosenPoint = point;
            //    }
            //}
           
            //PushVector=LineCollide(ChosenPoint,ChosenPoint-velocity,p3,p4)-ChosenPoint;
            
            //return PushVector;
        }

        //public Vector2 PointRelativeDir(Vector2 o,Vector2 c)
        //{
        //    if (c.x < o.x) return Vector2.left;

        //    return Vector2.zero;
        //}

        //public Vector2 PushOutOfTerrainVec(Vector2 colli,Vector2 Objectcenter,Vector2 PointInTerrain)
        //{
           
        //}
    }
}
