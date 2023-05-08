using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestMod
{
   
    class PolygonDebugSprite : CosmeticSprite
    {
        public Plugin.PolygonCollisionResult result;
        public Vector2[] ColliSquare;

        public Polygon Target;

        public TilePolygon[] Tiles;
        public int TileSpriteCount = 50;
        public PolygonDebugSprite(Vector2 vec2)
        {
            
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
          
            base.Destroy();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            // set label background properties (sans color)
            sLeaser.sprites = new FSprite[1];
           



            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
          

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {

            base.AddToContainer(sLeaser, rCam, null);
           
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
       
    }
}