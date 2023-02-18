using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fisobs.Properties;

namespace TestMod
{
    sealed class CrateProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
        => throwable = false;

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            // The player should only be able to grab one Crate at a time

            if (player.grasps.Any(g => g?.grabbed is Crate))
            {
                grabability = Player.ObjectGrabability.CantGrab;
            }
            else
            {
                grabability = Player.ObjectGrabability.TwoHands;
            }
        }

    }
}
