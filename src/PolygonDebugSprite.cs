using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using System.Reflection;

namespace TestMod
{
   
    class PolygonDebugSprite : CosmeticSprite
    {
        public Plugin.PolygonCollisionResult result;
        public Vector2[] ColliSquare;

        public Polygon Owner;

        public List<TilePolygon> Tiles;
        public int TileSpriteCount = 250;
        public PolygonDebugSprite(Polygon owner)
        {
            this.Owner = owner;
          
            
        }

      

        public override void Update(bool eu)
        {
            //if (owner.room?.abstractRoom?.name != Plugin.CurrentRoomName || owner.slatedForDeletetion)
            //{
            //    Destroy();
            //}

            base.Update(eu);
        }

        public override void Destroy()
        {
          
            base.Destroy();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            
            sLeaser.sprites = new FSprite[TileColliDetectCount+TileLineCount+ColliLineCount+ColliResultCount];

            for (int i = TileColliDetectSpr; i<TileColliDetectSpr+TileColliDetectCount;i++)
            { 
                sLeaser.sprites[i] = new FSprite("pixel", true);
            }

            for (int i = TileSpr; i < TileSpr + TileLineCount; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", true);
                sLeaser.sprites[i].color = Color.green;
            }

            for (int i = ColliLineSpr ; i < ColliLineSpr + ColliLineCount; i++)
            { 
                sLeaser.sprites[i] = new FSprite("pixel", true);
                sLeaser.sprites[i].color = Color.yellow;
            }

            for (int i = ColliResultSpr; i < ColliResultSpr + ColliResultCount; i++)
            { 
                sLeaser.sprites[i] = new FSprite("pixel", true);
                sLeaser.sprites[i].color =Color.cyan;
                sLeaser.sprites[i].scale = 10;
            }

            AddToContainer(sLeaser, rCam, null);
        }
       
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (ColliSquare != null)
            {
                for (int i = TileColliDetectSpr; i < TileColliDetectSpr + TileColliDetectCount; i++)
                {
                    sLeaser.sprites[i].SetPosition(ColliSquare[i] + (ColliSquare[(i+1)%TileColliDetectCount]- ColliSquare[i])/2  -camPos);
                    sLeaser.sprites[i].scaleX = 2;
                    sLeaser.sprites[i].scaleY = Custom.Dist(ColliSquare[i], ColliSquare[(i + 1) % TileColliDetectCount]);
                    sLeaser.sprites[i].rotation = (i+1) % 2 != 0 ? 0:90 ;
                }
            }

            if (Tiles != null)
            {
                int index = 0;
                for (int i = TileSpr; i < TileSpr + TileLineCount; i=i+4)
                {
                    for (int j = 0; j < 4; j++) {

                         if (index < Tiles.Count)
                        {
                            sLeaser.sprites[i+j].SetPosition(Tiles[index].corners[j] + (Tiles[index].corners[(j + 1) % 4] - Tiles[index].corners[j])/2 -camPos);
                            sLeaser.sprites[i+j].scaleX = 1;
                            sLeaser.sprites[i+j].scaleY = Custom.Dist(Tiles[index].corners[(j + 1) % 4] , Tiles[index].corners[j]);
                            sLeaser.sprites[i+j].rotation = (j + 1) % 2 != 0 ? 0 : 90;
                            sLeaser.sprites[i + j].isVisible = true;
                        } else
                        {
                            sLeaser.sprites[i+j].isVisible = false;
                        }
                    }
                    index++;
                }
            }
            if (result.Intersect)
            {

                sLeaser.sprites[this.ColliLineSpr].SetPosition(result.line1[0] + (result.line1[1] - result.line1[0]) / 2 - camPos);
                sLeaser.sprites[this.ColliLineSpr].scaleX = 1;
                sLeaser.sprites[this.ColliLineSpr].scaleY = Custom.Dist(result.line1[1] , result.line1[0]);
                sLeaser.sprites[this.ColliLineSpr].rotation = Custom.AimFromOneVectorToAnother(result.line1[0], result.line1[1]);
                sLeaser.sprites[this.ColliLineSpr].isVisible = true;

                sLeaser.sprites[this.ColliLineSpr+1].SetPosition(result.line2[0] + (result.line2[1] - result.line2[0]) / 2 - camPos);
                sLeaser.sprites[this.ColliLineSpr+1].scaleX = 1;
                sLeaser.sprites[this.ColliLineSpr+1].scaleY = Custom.Dist(result.line2[1], result.line2[0]);
                sLeaser.sprites[this.ColliLineSpr + 1].rotation = Custom.AimFromOneVectorToAnother(result.line2[0], result.line2[1]);
                sLeaser.sprites[this.ColliLineSpr + 1].isVisible = true;


                sLeaser.sprites[ColliResultSpr].SetPosition(result.CollisionPos - camPos);
              
            }

            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {

            // base.AddToContainer(sLeaser, rCam, null);
            foreach (FSprite s in sLeaser.sprites)
            {
                s.RemoveFromContainer();
                rCam.ReturnFContainer("HUD").AddChild(s);
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            // set colour of label background
           
        }

        public int TileColliDetectCount { get { return 4; } }
        public int TileColliDetectSpr { get { return 0; } }

        public int TileLineCount { get { return TileSpriteCount * 4; } }
        public int TileSpr { get { return TileColliDetectSpr + TileColliDetectCount; } }

        public int ColliLineCount { get { return 2; } }
        public int ColliLineSpr { get { return TileSpr + TileLineCount; } }

        public int ColliResultCount { get { return 1; } }
        public int ColliResultSpr { get { return ColliLineSpr + ColliLineCount; } }
       
       
    }
}