using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestMod
{
    // Code taken from https://github.com/casheww/RW-ToolBox/blob/master/SpriteLabel.cs with permission from casheww on Github (tysm!!!)
    class DebugSprite : CosmeticSprite
    {
        public DebugSprite(UpdatableAndDeletable owner, Vector2? position)
        {
            this.owner = owner;
            if (position != null)
            this.position = position.Value;
        }

        public override void Update(bool eu)
        {
            if (owner.slatedForDeletetion)
            {
                Destroy();
            }

            base.Update(eu);
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("pixel", true)
            {
                scaleX = 8f,
                scaleY = 8f
            };
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetPosition(position);
            sLeaser.sprites[0].isVisible = true;
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            base.AddToContainer(sLeaser, rCam, null);
            sLeaser.sprites[0].RemoveFromContainer();
            rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            // set colour of label background
            Color color = Color.blue;
            color.a = 0.7f;
            sLeaser.sprites[0].color = color;
        }

        readonly UpdatableAndDeletable owner;
        readonly Vector2 position;
    }
}
