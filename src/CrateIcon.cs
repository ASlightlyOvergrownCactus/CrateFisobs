using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fisobs.Core;
using UnityEngine;

namespace TestMod
{
    sealed class CrateIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return apo is CrateAbstract crate ? (int)(crate.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }

        public override string SpriteName(int data)
        {
            // Fisobs autoloads the embedded resource named `icon_{Type}` automatically
            // For Crates, this is `icon_Crate`
            return "icon_Crate";
        }
    }
}
