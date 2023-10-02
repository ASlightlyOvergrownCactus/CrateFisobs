using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
namespace TestMod
{
    public class DebugSpr : CosmeticSprite
    {
        public PhysicalObject owner;

        public List<Vector2> NumberOfPoint;
        public int NumberOfSpr
        {
            get
            {
                return 10;
            }

        }

        public DebugSpr()
        {
            NumberOfPoint = new List<Vector2>
            {
                new Vector2(),
                new Vector2(),
            };
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];

            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", true);
                sLeaser.sprites[i].scale = 4;
                sLeaser.sprites[i].color = i == 0 ? UnityEngine.Color.cyan : UnityEngine.Color.green;
            }

            AddToContainer(sLeaser, rCam, null);
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
        {
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[i].SetPosition(NumberOfPoint[i] - camPos);

            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {



            foreach (var i in sLeaser.sprites)
            {
                i.RemoveFromContainer();
                rCam.ReturnFContainer("HUD").AddChild(i);
            }

            //base.AddToContainer(sLeaser, rCam, newContatiner);

        }



    }
}