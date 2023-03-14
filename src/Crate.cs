using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace TestMod
{
    sealed class Crate : PhysicalObject, IDrawable
    {
        private static float Rand => UnityEngine.Random.value;

        public float lastDarkness = -1f;
        public float rotation;
        public float lastRotation;
        public float rotVel;
        public float darkness;
        public Rectangle rect;
        public float rad;
        private readonly float rotationOffset;

        // Idea: Custom Code a square collision (very hard)



        public CrateAbstract Abstr { get; }
        public Crate(CrateAbstract abstr) : base(abstr)
        {
            rad = 30f;
            Debug.Log("Initializing Crate Object!");
            float mass = 10f;
            Abstr = abstr;
            var positions = new List<Vector2>();

            /*for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    positions.Add(new Vector2(x, y) * 20f);
                }
            }*/
            positions.Add(new Vector2(0, 0) * 20f);
            bodyChunks = new BodyChunk[positions.Count];

            // Create all body chunks
            for(int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i] = new BodyChunk(this, i, new Vector2(), rad, mass / bodyChunks.Length);
            }

            bodyChunkConnections = new BodyChunkConnection[bodyChunks.Length * (bodyChunks.Length - 1) / 2];
            int connection = 0;

            // Create all chunk connections
            for (int x = 0; x < bodyChunks.Length; x++)
            {
                for (int y = x + 1; y < bodyChunks.Length; y++)
                {
                    bodyChunkConnections[connection] = new BodyChunkConnection(bodyChunks[x], bodyChunks[y], Vector2.Distance(positions[x], positions[y]), BodyChunkConnection.Type.Normal, 0.6f, -1f);
                    connection++;
                }
            }


            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0.3f;
            surfaceFriction = 1f;
            collisionLayer = 1;
            waterFriction = 0.92f;
            buoyancy = 0.75f;
            GoThroughFloors = false;

            rotation = Rand * 360f;
            lastRotation = rotation;
            rotationOffset = Rand * 30 - 15;

            Debug.Log("Loading Crate BodyChunk ctor!");
            rect = new Rectangle(this.bodyChunks[0].pos, rad * 2, rad * 2);

        }

        public override void Grabbed(Creature.Grasp grasp)
        {
            base.Grabbed(grasp);

            this.grabbedBy.Add(grasp);
        }

        public override void Update(bool eu)
        {
            base.Update(eu);


            /*if (grabbedBy.Count == 0)
            {
                // Slows crate down to stop the "slipperyness" that it has when slippin' accross the floor
                bodyChunks[0].vel = new Vector2(bodyChunks[0].vel.x * 0.65f, bodyChunks[0].vel.y);
            }*/

            rect.center = firstChunk.pos - (new Vector2(firstChunk.rad, firstChunk.rad * 2));
            rect.UpdateCornerPoints();
            //Debug.Log(rect.center.x + " " + rect.center.y);


            var chunk = firstChunk;
            lastRotation = rotation;
            rotation += rotVel * Vector2.Distance(chunk.lastPos, chunk.pos);

            rotation %= 360;

            if (grabbedBy.Count == 0)
            {
                if (firstChunk.lastPos == firstChunk.pos)
                {
                    rotVel *= 0.9f;
                }
                else if (Mathf.Abs(rotVel) <= 0.01f)
                {
                    ResetVel((firstChunk.lastPos - firstChunk.pos).magnitude);
                }
            }
            else
            {
                var grabberChunk = grabbedBy[0].grabber.mainBodyChunk;
                rotVel *= 0.9f;
                rotation = Mathf.Lerp(rotation, grabberChunk.Rotation.GetAngle() + rotationOffset, 0.25f);
            }
            //rotation %= 360;

            // Need to get the angle between chunks 1, 3, 5, 7
            // Then calculate avg. angle between them all and the center chunk (chunk 4)

            // Crate has "rotation", but its quite unpredictable. Need to make it more accurate somehow...

            // Idea: Find angle between all chunks used for positioning (chunks 1, 3, 5, and 7) and the angle between them and the center chunk.
            // Then find expected sprite angle from each of those and average.

            /*float angle1 = Custom.AimFromOneVectorToAnother(bodyChunks[4].pos, bodyChunks[1].pos);
            float angle2 = Custom.AimFromOneVectorToAnother(bodyChunks[4].pos, bodyChunks[3].pos);
            float angle3 = Custom.AimFromOneVectorToAnother(bodyChunks[4].pos, bodyChunks[5].pos);
            float angle4 = Custom.AimFromOneVectorToAnother(bodyChunks[4].pos, bodyChunks[7].pos);
            float ang1 = Custom.AimFromOneVectorToAnother(bodyChunks[1].pos, bodyChunks[3].pos);
            float ang2 = Custom.AimFromOneVectorToAnother(bodyChunks[3].pos, bodyChunks[5].pos);
            float ang3 = Custom.AimFromOneVectorToAnother(bodyChunks[5].pos, bodyChunks[7].pos);
            float ang4 = Custom.AimFromOneVectorToAnother(bodyChunks[7].pos, bodyChunks[1].pos);


            rotation = (angle1 + angle2 + angle3 + angle4 + ang1 + ang2 + ang3 + ang4) / 8;

            rotation = (rotation * 2) % 360;*/

            /*if (firstChunk.lastPos == firstChunk.pos)
            {
                rotVel *= 0.9f;
            }
            else if (Mathf.Abs(rotVel) <= 0.01f)
            {
                ResetVel((firstChunk.lastPos - firstChunk.pos).magnitude);
            }

            if (!Custom.DistLess(chunk.lastPos, chunk.pos, 3f) && room.GetTile(chunk.pos).Solid && !room.GetTile(chunk.lastPos).Solid)
            {
                var firstSolid = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(chunk.lastPos), room.GetTilePosition(chunk.pos));
                if (firstSolid != null)
                {
                    FloatRect floatRect = Custom.RectCollision(chunk.pos, chunk.lastPos, room.TileRect(firstSolid.Value).Grow(2f));
                    chunk.pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
                    bool flag = false;
                    if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
                    {
                        chunk.vel.x = Mathf.Abs(chunk.vel.x) * 0.15f;
                        flag = true;
                    }
                    else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
                    {
                        chunk.vel.x = -Mathf.Abs(chunk.vel.x) * 0.15f;
                        flag = true;
                    }
                    else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
                    {
                        chunk.vel.y = Mathf.Abs(chunk.vel.y) * 0.15f;
                        flag = true;
                    }
                    else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
                    {
                        chunk.vel.y = -Mathf.Abs(chunk.vel.y) * 0.15f;
                        flag = true;
                    }
                    if (flag)
                    {
                        rotVel *= 0.8f;
                    }
                }
            }*/
            //rotation = chunk.pos.GetAngle();
        }

        private void ResetVel(float speed)
        {
            rotVel = Mathf.Lerp(-1f, 1f, Rand) * Custom.LerpMap(speed, 0f, 18f, 5f, 26f);
        }

        // Deals damage to whatever it hits if it's a creature and it deals more than 1 damage.
        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
                if (otherObject.bodyChunks[otherChunk].owner is Creature creature && !creature.dead)
                {
                float damage = (this.bodyChunks[myChunk].vel.x + this.bodyChunks[myChunk].vel.y) / 2;
                if (damage < 0)
                    {
                        damage *= -1;
                    }
                else if (damage < 1)
                    {
                        // Don't deal damage
                    }
                else
                    {
                        creature.Violence(this.bodyChunks[myChunk], this.bodyChunks[myChunk].vel, otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, damage, 5f);
                        //Debug.Log("Crate collision damage: " + damage);
                    }
                }

        }

        public override void PlaceInRoom(Room placeRoom)
        {
            Debug.Log("Placing Crate!");
            base.PlaceInRoom(placeRoom);

            Vector2 center = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
            bodyChunks[0].HardSetPosition(new Vector2(0, 0) * 20f + center);
            rect.UpdateCornerPoints();
        }

        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            base.TerrainImpact(chunk, direction, speed, firstContact);

            if (speed > 10)
            {
                room.PlaySound(SoundID.Spear_Fragment_Bounce, bodyChunks[chunk].pos, 0.35f, 2f);
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[bodyChunks.Length + rect.corners.Length];
            
            /*sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("icon_Crate", true);*/
            
            for(int i = 0; i < bodyChunks.Length; i++)
                sLeaser.sprites[i] = new FSprite("Circle20");

            for (int i = bodyChunks.Length; i < bodyChunks.Length + rect.corners.Length; i++)
                sLeaser.sprites[i] = new FSprite("Circle4");

            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            // Colors in collidors
            for(int i = 0; i < bodyChunks.Length; i++)
            {
                var spr = sLeaser.sprites[i];
                spr.SetPosition(Vector2.Lerp(bodyChunks[i].lastPos, bodyChunks[i].pos, timeStacker) - camPos);
                spr.scale = bodyChunks[i].rad / 10f;
            }


            var spr1 = sLeaser.sprites[bodyChunks.Length];
            spr1.SetPosition(rect.corners[0]);
            spr1.scale = 5f;

            var spr2 = sLeaser.sprites[bodyChunks.Length + 1];
            spr2.SetPosition(rect.corners[1]);
            spr2.scale = 5f;

            var spr3 = sLeaser.sprites[bodyChunks.Length + 2];
            spr3.SetPosition(rect.corners[2]);
            spr3.scale = 5f;

            var spr4 = sLeaser.sprites[bodyChunks.Length + 3];
            spr4.SetPosition(rect.corners[3]);
            spr4.scale = 5f;


            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();


            sLeaser.sprites[0].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            sLeaser.sprites[0].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), Color.blue, darkness);

            sLeaser.sprites[1].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            sLeaser.sprites[1].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), Color.red, darkness);

            sLeaser.sprites[2].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            sLeaser.sprites[2].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), Color.red, darkness);

            sLeaser.sprites[3].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            sLeaser.sprites[3].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), Color.red, darkness);

            sLeaser.sprites[4].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
            sLeaser.sprites[4].color = Color.Lerp(Custom.HSL2RGB(Abstr.hue, Abstr.saturation, 0.55f), Color.red, darkness);
            // TODO: Add the sprite for this back
            // Typically, Rain World objects use fully white sprites, then color them via code. This allows them to change based on the palette and animate in cool ways
            // Sprite angle can be gotten by taking the angle from the center chunk to one of the outside ones, or taking the angle for all of them and using some sort of average
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
    }
}
