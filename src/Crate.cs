using System.Collections.Generic;
using System.Linq;
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
        public float distance;
        public static List<Crate> crates = new List<Crate>();
        private Vector2 grabberPos;
        private Vector2 grabPoint;
        public bool canGrab;
        public int timer;

        #region Hooks and Helpers
        public static void AddHooks()
        {
            On.Player.Update += Player_Update;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.ReleaseObject += Player_ReleaseObject;
        }

        // Hooked to make a timer on release of the object to prevent the player from immediately grabbing again due to holding the button for like even a fraction of a second
        private static void Player_ReleaseObject(On.Player.orig_ReleaseObject orig, Player self, int grasp, bool eu)
        {
            int index = 0;
            if (self.grasps[grasp].grabbed is Crate)
            {
                for (int i = 0; i < Crate.crates.Count; i++)
                {
                    if ((self.grasps[grasp].grabbed as Crate) == Crate.crates.ElementAt(i))
                    {
                        index = i;
                    }
                }
                Crate.crates.ElementAt(index).canGrab = false;
                Crate.crates.ElementAt(index).timer = 20;

                orig(self, grasp, eu);
            }
            orig(self, grasp, eu);
            
        }

        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            if (self != null && self.grasps[0] != null)
            {
                if (self.grasps[0].grabbedChunk.owner is Crate)
                {

                    //Need:
                    // 1. If slugcat is far enough from object while grabbing, add force to object in slugcat's direction.
                    // 2. When grabbing, grabber position stays relative to the object and can't be moved from its relative position.

                    Vector2 a = Custom.DirVec(self.mainBodyChunk.pos, self.grasps[0].grabbedChunk.pos);
                    Vector2 c = Vector2.zero;
                    float dist = Vector2.Distance(self.mainBodyChunk.pos, self.grasps[0].grabbedChunk.pos);
                    float num = 5f + self.grasps[0].grabbedChunk.rad;

                    float num2 = self.grasps[0].grabbedChunk.mass / (self.mainBodyChunk.mass + self.grasps[0].grabbedChunk.mass);
                    if (self.enteringShortCut != null)
                    {
                        num2 = 0f;
                    }
                    else if (self.grasps[0].grabbed.TotalMass < self.TotalMass)
                    {
                        num2 /= 2f;
                    }

                    if (self.enteringShortCut == null || dist > num)
                    {
                        c = 2 * -a * (dist - num) * (1f - num2);
                    }

                    var phys = RoomPhysics.Get(self.room);
                    if (phys.TryGetObject(self.grasps[0].grabbedChunk.owner, out var obj))
                    {
                       var rb2d = obj.GetComponent<Rigidbody2D>();
                        if (self.bodyChunks[1].ContactPoint.y == -1)
                        {
                            (self.grasps[0].grabbedChunk.owner as Crate).grabberPos += c;
                            rb2d.AddForce(c * 10f);
                            rb2d.position += (c / RoomPhysics.PIXELS_PER_UNIT);
                        }

                        (self.grasps[0].grabbedChunk.owner as Crate).grabberPos = rb2d.transform.TransformPoint((self.grasps[0].grabbedChunk.owner as Crate).grabPoint) * RoomPhysics.PIXELS_PER_UNIT;
                    }
                }
            }
            orig(self, eu);
        }

        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            //Debug.Log("Reached start");
            if (self.input[0].pckp && self.grasps[0] == null && self.grasps[1] == null)
            {
                //Debug.Log("Got Past check");
                float dist = float.PositiveInfinity;
                Crate c = Crate.crates[0];
                for (int i = 0; i < crates.Count; i++)
                {
                    float temp = Vector2.Distance(self.bodyChunks[0].pos, crates.ElementAt(i).bodyChunks[0].pos);
                    if (temp < dist)
                    {
                        dist = temp;
                        c = crates.ElementAt(i);
                    }
                }
                c.GrabObj(self);
                //Debug.Log("Finished Grab");
            }
            //Debug.Log("Escaped if statement");

            orig(self, eu);
        }
        #endregion Hooks and Helpers
        public CrateAbstract Abstr { get; }
        public Crate(CrateAbstract abstr) : base(abstr)
        {
            Abstr = abstr;

            bodyChunks = new BodyChunk[]
                { new BodyChunk(this, 0, new Vector2(), 1f, 10f),
                new BodyChunk(this, 0, Vector2.zero, 5f, 1f)};

            
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
            grabberPos = Vector2.zero;
            canGrab = true;

            rotation = 0f;
            lastRotation = rotation;
            crates.Add(this);
        }

        public void GrabObj(Player player)
        {
            if (player.input[0].pckp && player.grasps[0] == null && player.grasps[1] == null)
            {
                var phys = RoomPhysics.Get(room);


                if (phys.TryGetObject(this, out var obj))
                {
                    
                    var rb2d = obj.GetComponent<Rigidbody2D>();
                    //Debug.Log("New grabber chunk position: " + bodyChunks[1].pos);
                    grabberPos = rb2d.ClosestPoint(player.bodyChunks[0].pos / RoomPhysics.PIXELS_PER_UNIT) * RoomPhysics.PIXELS_PER_UNIT;
                    distance = Vector2.Distance(grabberPos, firstChunk.pos);
                    
                    if (Vector2.Distance(grabberPos, player.bodyChunks[0].pos) < 20f && canGrab)
                    {
                        grabPoint = rb2d.transform.InverseTransformPoint(new Vector3(this.grabberPos.x, this.grabberPos.y, 0f) / RoomPhysics.PIXELS_PER_UNIT);
                        player.Grab(this, 0, 1, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 0.5f, true, true);
                    }
                }

            }
        }
        
        public override void Update(bool eu)
        {
            lastRotation = rotation;
            base.Update(eu);

            if (room == null || slatedForDeletetion)
            {
                crates.Remove(this);
                return;
            }

            bodyChunks[1].HardSetPosition(grabberPos);

            if (timer > 0)
            {
                canGrab = false;
                timer--;
            }
            else
            {
                canGrab = true;
            }

            var phys = RoomPhysics.Get(room);
            if (!phys.TryGetObject(this, out var obj))
            {
                obj = phys.CreateObject(this, this);
                obj.transform.position = firstChunk.pos / RoomPhysics.PIXELS_PER_UNIT;

                var rb2d = obj.AddComponent<Rigidbody2D>();
                rb2d.bodyType = RigidbodyType2D.Dynamic;
                rb2d.drag = 0f;
                rb2d.gravityScale = 0.5f;

                var box = obj.AddComponent<BoxCollider2D>();
                box.size = Vector2.one * 100f / RoomPhysics.PIXELS_PER_UNIT;
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
                    scale = 50f * 2f
                },

                new FSprite("pixel"){anchorX = 0.5f, anchorY = 0.5f, scale = 5f}
            };

            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            sLeaser.sprites[0].SetPosition(Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos);
            sLeaser.sprites[0].rotation = Mathf.LerpAngle(lastRotation, rotation, timeStacker);

            sLeaser.sprites[1].SetPosition(Vector2.Lerp(bodyChunks[1].lastPos, bodyChunks[1].pos, timeStacker) - camPos);
            sLeaser.sprites[1].rotation = Mathf.LerpAngle(lastRotation, rotation, timeStacker);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            //foreach (var sprite in sLeaser.sprites)
                //sprite.color = palette.blackColor;
                sLeaser.sprites[0].color = palette.blackColor;
            sLeaser.sprites[1].color = Color.red;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");

            foreach (FSprite fsprite in sLeaser.sprites)
                newContainer.AddChild(fsprite);
        }
    }
}
