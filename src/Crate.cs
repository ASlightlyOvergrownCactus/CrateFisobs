using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace TestMod
{
    public class Crate : PhysicalObject, IDrawable
    {
        public float lastDarkness = -2f;
        public float rotation;
        public float lastRotation;
        public float darkness;
        
        public CrateAbstract Abstr { get; }
        public Crate(CrateAbstract abstr) : base(abstr)
        {
            Abstr = abstr;

            bodyChunks = new BodyChunk[]
            {
                new BodyChunk(this, 0, new Vector2(), 50f, 10f)
            };

            bodyChunkConnections = new BodyChunkConnection[0];

            airFriction = 0.999f;
            gravity = 0.7f;
            bounce = 0.3f;
            surfaceFriction = 1f;
            collisionLayer = 1;
            waterFriction = 0.92f;
            buoyancy = 0.75f;
            CollideWithTerrain = false;
            CollideWithSlopes = false;
            CollideWithObjects = false;
            GoThroughFloors = true;

            rotation = 0f;
            lastRotation = rotation;
        }
        
        public override void Update(bool eu)
        {
            lastRotation = rotation;
            base.Update(eu);

            if (room == null || slatedForDeletetion)
                return;

            var phys = RoomPhysics.Get(room);
            if (!phys.TryGetObject(this, out var obj))
            {
                obj = phys.CreateObject(this);
                obj.transform.position = firstChunk.pos / RoomPhysics.PIXELS_PER_UNIT;

                var rb2d = obj.AddComponent<Rigidbody2D>();
                rb2d.bodyType = RigidbodyType2D.Dynamic;
                rb2d.drag = 0f;
                rb2d.gravityScale = 0.5f;

                var box = obj.AddComponent<BoxCollider2D>();
                box.size = Vector2.one * 2f * firstChunk.rad / RoomPhysics.PIXELS_PER_UNIT;
            }
            else
            {
                var rb2d = obj.GetComponent<Rigidbody2D>();
                firstChunk.pos = rb2d.position * RoomPhysics.PIXELS_PER_UNIT;
                firstChunk.vel = rb2d.velocity * 40f * RoomPhysics.PIXELS_PER_UNIT;
                rotation = -rb2d.rotation;

                if(Input.GetKey(KeyCode.B))
                {
                    rb2d.velocity += Custom.DirVec(rb2d.position * RoomPhysics.PIXELS_PER_UNIT, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f * 40f / RoomPhysics.PIXELS_PER_UNIT;
                }
            }
        }

        // Deals damage to whatever it hits if it's a creature and it deals more than 1 damage.
        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
            if (otherObject.bodyChunks[otherChunk].owner is Creature creature && !creature.dead)
            {
                float damage = bodyChunks[myChunk].vel.magnitude / 2f;
                if (damage > 1f)
                {
                    creature.Violence(bodyChunks[myChunk], bodyChunks[myChunk].vel, otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Blunt, damage, 5f);
                }
            }
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);

            Vector2 center = placeRoom.MiddleOfTile(abstractPhysicalObject.pos);
            firstChunk.HardSetPosition(center);
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
            sLeaser.sprites = new FSprite[]
            {
                new FSprite("pixel")
                {
                    anchorX = 0.5f,
                    anchorY = 0.5f,
                    scale = firstChunk.rad * 2f
                }
            };

            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetPosition(Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos);
            sLeaser.sprites[0].rotation = Mathf.LerpAngle(lastRotation, rotation, timeStacker);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            foreach (var sprite in sLeaser.sprites)
                sprite.color = palette.blackColor;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
                newContainer.AddChild(fsprite);
        }
    }
}
