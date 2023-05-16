using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using System.Reflection;
using UnityEngine.PlayerLoop;

namespace TestMod
{

    class PolygonDebugSprite : CosmeticSprite
    {
        public Plugin.PolygonCollisionResult result;
        public Vector2[] ColliSquare;

        public Polygon Owner;

        public List<TilePolygon> Tiles;
        public int TileSpriteCount = 150;
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
           for(int i = 0; i < Timer.Count; i++) 
            {
                Timer[i].Update();
                if (Timer[i].RemainTime <= 0) { Timer.RemoveAt(i); }
            }


            base.Update(eu);
        }

        public override void Destroy()
        {

            base.Destroy();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {

            sLeaser.sprites = new FSprite[TileColliDetectCount + TileLineCount + ColliLineCount + ColliResultCount + ExtraDebugPixelCount];

            for (int i = TileColliDetectSpr; i < TileColliDetectSpr + TileColliDetectCount; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", true);
            }

            for (int i = TileSpr; i < TileSpr + TileLineCount; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", true);
                sLeaser.sprites[i].color = Color.green;
            }

            for (int i = ColliLineSpr; i < ColliLineSpr + ColliLineCount; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", true);
                sLeaser.sprites[i].color = Color.yellow;
            }

            for (int i = ColliResultSpr; i < ColliResultSpr + ColliResultCount; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", true);
                sLeaser.sprites[i].color = Color.cyan;
                sLeaser.sprites[i].scale = 10;
            }

            for (int i = ExtraDebugPixelSpr; i < ExtraDebugPixelSpr + ExtraDebugPixelCount; i++)
            {
                sLeaser.sprites[i] = new FSprite("pixel", true);
                sLeaser.sprites[i].color = Color.blue;

            }

            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (ColliSquare != null)
            {
                for (int i = TileColliDetectSpr; i < TileColliDetectSpr + TileColliDetectCount; i++)
                {
                    sLeaser.sprites[i].SetPosition(ColliSquare[i] + (ColliSquare[(i + 1) % TileColliDetectCount] - ColliSquare[i]) / 2 - camPos);
                    sLeaser.sprites[i].scaleX = 2;
                    sLeaser.sprites[i].scaleY = Custom.Dist(ColliSquare[i], ColliSquare[(i + 1) % TileColliDetectCount]);
                    sLeaser.sprites[i].rotation = (i + 1) % 2 != 0 ? 0 : 90;
                }
            }

            if (Tiles != null)
            {
                int index = 0;
                for (int i = TileSpr; i < TileSpr + TileLineCount; i = i + 4)
                {
                    if (index < Tiles.Count)
                    {
                        for (int j = 0; j < Tiles[index].corners.Length; j++)
                        {

                            if (index < Tiles.Count)
                            {
                                sLeaser.sprites[i + j].SetPosition(Tiles[index].corners[j] - camPos + (Tiles[index].corners[(j + 1) % Tiles[index].corners.Length] - Tiles[index].corners[j]) / 2);
                                sLeaser.sprites[i + j].scaleX = 3;
                                sLeaser.sprites[i + j].scaleY = Custom.Dist(Tiles[index].corners[(j + 1) % Tiles[index].corners.Length], Tiles[index].corners[j]);
                                sLeaser.sprites[i + j].rotation = Custom.AimFromOneVectorToAnother(Tiles[index].corners[j], Tiles[index].corners[(j + 1) % Tiles[index].corners.Length]);
                                sLeaser.sprites[i + j].isVisible = true;
                            }
                            else
                            {
                                sLeaser.sprites[i + j].isVisible = false;
                            }
                        }
                    }
                    index++;
                }
                Tiles = null;
            }
            else
            {
                for (int i = TileSpr; i < TileSpr + TileSpriteCount; i++)
                {
                    sLeaser.sprites[i].isVisible = false;
                }
            }

            if (result.Intersect)
            {

                sLeaser.sprites[this.ColliLineSpr].SetPosition(result.line1[0] + (result.line1[1] - result.line1[0]) / 2 - camPos);
                sLeaser.sprites[this.ColliLineSpr].scaleX = 1;
                sLeaser.sprites[this.ColliLineSpr].scaleY = Custom.Dist(result.line1[1], result.line1[0]);
                sLeaser.sprites[this.ColliLineSpr].rotation = Custom.AimFromOneVectorToAnother(result.line1[0], result.line1[1]);
                sLeaser.sprites[this.ColliLineSpr].isVisible = true;

                sLeaser.sprites[this.ColliLineSpr + 1].SetPosition(result.line2[0] + (result.line2[1] - result.line2[0]) / 2 - camPos);
                sLeaser.sprites[this.ColliLineSpr + 1].scaleX = 1;
                sLeaser.sprites[this.ColliLineSpr + 1].scaleY = Custom.Dist(result.line2[1], result.line2[0]);
                sLeaser.sprites[this.ColliLineSpr + 1].rotation = Custom.AimFromOneVectorToAnother(result.line2[0], result.line2[1]);
                sLeaser.sprites[this.ColliLineSpr + 1].isVisible = true;


                sLeaser.sprites[ColliResultSpr].SetPosition(result.CollisionPos - camPos);
                sLeaser.sprites[ColliResultSpr].isVisible = true;

            } else
            {
                for (int i = ColliLineSpr; i < ColliLineSpr + ColliLineCount + ColliResultCount; i++)
                {
                    sLeaser.sprites[i].isVisible = false;
                }
            }
            
            for(int i = 0; i<Timer.Count;i++)
            {
                sLeaser.sprites[ExtraDebugPixelSpr+i].SetPosition(Timer[i].Pos);
                sLeaser.sprites[ExtraDebugPixelSpr + i].rotation=Timer[i].Rotation;
                sLeaser.sprites[ExtraDebugPixelSpr + i].scaleY = Timer[i].YScale;
                sLeaser.sprites[ExtraDebugPixelSpr + i].scaleX = 2;
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

        public void DrawADot(Vector2 pos, float Duration = 0)
        {
            if (Timer.Count>ExtraDebugPixelCount) { return; }
            Timer.Add(new EXDebugSprite(pos));
        }

        public void DrawADot(Vector2 pos1,Vector2 pos2, float Duration = 0)
        {
            if (Timer.Count > ExtraDebugPixelCount) { return; }
            Timer.Add(new EXDebugSprite(pos1,pos2));
        }

        public List<EXDebugSprite> Timer = new List<EXDebugSprite>();
        public int TileColliDetectCount { get { return 4; } }
        public int TileColliDetectSpr { get { return 0; } }

        public int TileLineCount { get { return TileSpriteCount * 4; } }
        public int TileSpr { get { return TileColliDetectSpr + TileColliDetectCount; } }

        public int ColliLineCount { get { return 2; } }
        public int ColliLineSpr { get { return TileSpr + TileLineCount; } }

        public int ColliResultCount { get { return 1; } }
        public int ColliResultSpr { get { return ColliLineSpr + ColliLineCount; } }

        public int ExtraDebugPixelCount { get { return 10; } }
        public int ExtraDebugPixelSpr { get { return ColliResultSpr + ColliResultCount; } }


        public class EXDebugSprite
        {   
            public int RemainTime=40*3;
            public Vector2 pos1;
            public Vector2 pos2=Vector2.zero;
            public float Rotation
            {
                get
                {
                    if(pos2!=Vector2.zero) 
                    {
                        return 0;
                    }else
                    {
                        return Custom.AimFromOneVectorToAnother(pos1,pos2);
                    }    
                }
            }
            public float YScale
            {
                get
                {
                    if(pos2==Vector2.zero) { return 1; }
                    else
                    {
                        return Custom.Dist(pos1,pos2);
                    }
                }
            }
            public Vector2 Pos
            {
                get
                {
                    if(pos2==Vector2.zero)
                    {
                        return pos1;
                    }else
                    {
                        return pos1 + (pos2 - pos1) / 2;
                    }
                }
            }
            public EXDebugSprite(Vector2 p1) 
            {
                pos1= p1;
            }
            public EXDebugSprite(Vector2 p1,Vector2 p2)
            {
                pos1 = p1;
                pos2 = p2;
            }

            public void Update()
            {
                RemainTime--;
            }


        }
    }
}