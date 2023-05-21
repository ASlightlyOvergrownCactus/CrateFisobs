using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace TestMod
{
    public class TilePolygon : UpdatableAndDeletable, IDrawable
    {
        public UnityEngine.Vector2 center;
        public UnityEngine.Vector2[] corners;
        public List<Edge> edges = new List<Edge>();

        // For tiles
        public TilePolygon(Vector2 center,DefaultShape DF, Vector2[]corners=null)
        {
            //Debug.Log("reached tile polygon cons");
            this.center = center;
            
      
            switch(DF)
            {
                case DefaultShape.LeftUp:
                    this.corners = new Vector2[3] { new Vector2(center.x - 10, center.y - 10), new Vector2(center.x + 10, center.y + 10), new Vector2(center.x + 10, center.y - 10) };
                    break;

                case DefaultShape.RightUp:
                    this.corners = new Vector2[3] { new Vector2(center.x - 10, center.y - 10), new Vector2(center.x - 10,center.y+10), new Vector2(center.x + 10, center.y - 10) };
                    break;

                case DefaultShape.LeftDown:
                    this.corners = new Vector2[3] {  new Vector2(center.x - 10, center.y + 10), new Vector2(center.x + 10, center.y + 10), new Vector2(center.x + 10, center.y - 10) };
                    break;
                case DefaultShape.RightDown:
                    this.corners = new Vector2[3] { new Vector2(center.x - 10, center.y - 10), new Vector2(center.x - 10, center.y + 10),  new Vector2(center.x + 10, center.y - 10) };
                    break;
                case DefaultShape.Square:
                    this.corners = new Vector2[4] { new Vector2(center.x - 10, center.y - 10), new Vector2(center.x - 10, center.y + 10), new Vector2(center.x + 10, center.y + 10), new Vector2(center.x + 10, center.y - 10) };
                    break;
                case DefaultShape.others:
                        this.corners = corners;
                        break;
            }
            //Debug.Log("Finished tile cons");
            edges = new List<Edge>();


            // Define edges
            BuildEdges();
        }

        public void BuildEdges()
        {
            Vector2 p1;
            Vector2 p2;
            Edges.Clear();
            for (int i = 0; i < corners.Length; i++)
            {
                p1 = corners[i];
                p2= corners[(i+1)%corners.Length];
                edges.Add(new Edge(p1,p2));
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[corners.Length];

            for (int i = 0; i < corners.Length; i++)
                sLeaser.sprites[i] = new FSprite("Circle4");

            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < corners.Length; i++)
            {
                var spr = sLeaser.sprites[i];
                spr.SetPosition((corners[i]) - camPos);
                spr.scale = 10f;
            }

            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();

            for (int i = 0; i < corners.Length; i++)
            {
                sLeaser.sprites[i].color = Color.blue;
            }


        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            foreach (var sprite in sLeaser.sprites)
                sprite.color = palette.blackColor;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
                newContainer.AddChild(fsprite);
        }

        public List<Edge> Edges
        {
            get { return edges; }
        }

        public enum DefaultShape
        {
            Square,
            LeftUp,
            RightUp,
            LeftDown,
            RightDown,
            others
        }
        public class Edge
        {
           public Vector2 p1;
            public Vector2 p2;
            public Edge(Vector2 p1, Vector2 p2) { this.p1 = p1; this.p2 = p2;}
            public bool Equal (Edge e)
            {
                if ((e.p1 == this.p1&&e.p2==this.p2)||(e.p2==this.p1&&e.p1==this.p2))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
