using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;

namespace TestMod
{   
    //Manages other PolygonCollisions
    public  class PolyCorrdinator:UpdatableAndDeletable
    {
        public List<Crate> Polys;
        public PolyCorrdinator() 
        {
            this.Polys = new List<Crate>();
        }
        public override void Update(bool eu)
        {
            base.Update(eu);



        }
    }
}
