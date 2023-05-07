using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestMod
{
    // Code taken from https://github.com/casheww/RW-ToolBox/blob/master/SpriteLabel.cs with permission from casheww on Github (tysm!!!)
    class DebugSpriteText : CosmeticSprite
    {
        public DebugSpriteText(Vector2 vec2)
        {
            this.vec2 = vec2;
        }

        public override void Update(bool eu)
        {
            if (owner.room?.abstractRoom?.name != Plugin.CurrentRoomName || owner.slatedForDeletetion)
            {
                Destroy();
            }

            base.Update(eu);
        }

        public override void Destroy()
        {
            label.isVisible = false;
            label.text = "";
            base.Destroy();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            // set label background properties (sans color)
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
            Vector2 pos = vec2;
            sLeaser.sprites[0].SetPosition(pos);
            label.SetPosition(pos);
            sLeaser.sprites[0].isVisible = true;
            label.isVisible = true;

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            base.AddToContainer(sLeaser, rCam, null);
            label.RemoveFromContainer();
            sLeaser.sprites[0].RemoveFromContainer();
            rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
            rCam.ReturnFContainer("Foreground").AddChild(label);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            // set colour of label background
            Color color = Color.red;
            color.a = 0.7f;
            sLeaser.sprites[0].color = color;
        }

        readonly Vector2 vec2;
        readonly UpdatableAndDeletable owner;
        readonly FLabel label;
    }
}
