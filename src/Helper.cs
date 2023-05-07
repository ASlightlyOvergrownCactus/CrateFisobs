using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector2 = UnityEngine.Vector2;
namespace TestMod
{
    public static class Helper
    {

        public static bool IsBetween(float a, float b,float c)
        {
            return (a <= b && b <= c)|| (a >= b && b >= c);
        }

        public static bool AABB(Vector2 Line1a,Vector2 Line1b,Vector2 Line2a,Vector2 Line2b,Vector2 p)
        {
            //some small margin #BandageFix
            float Margin = 1;
            Vector2 TopLeftA = new Vector2(Math.Min(Line1a.x, Line1b.x)- Margin, Math.Max(Line1a.y,Line1b.y)+ Margin);
            Vector2 RightDownA=new Vector2(Math.Max(Line1a.x, Line1b.x)+ Margin, Math.Min(Line1a.y, Line1b.y)+ Margin);

            Vector2 TopLeftB = new Vector2(Math.Min(Line2a.x, Line2b.x)- Margin, Math.Max(Line2a.y, Line2b.y)+ Margin);
            Vector2 RightDownB = new Vector2(Math.Max(Line2a.x, Line2b.x)+ Margin, Math.Min(Line2a.y, Line2b.y)- Margin);

            bool X= IsBetween(TopLeftA.x, p.x, RightDownA.x) && IsBetween(TopLeftB.x,p.x,RightDownB.x);
            bool Y= IsBetween(RightDownA.y, p.y, TopLeftA.y) && IsBetween(RightDownB.y, p.y, TopLeftB.y);

            return X && Y;
        }
    }
}
