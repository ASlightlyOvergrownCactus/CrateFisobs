using Rewired.UI.ControlMapper;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestMod
{
    public class RoomPhysics
    {
        public const float PIXELS_PER_UNIT = 20f;

        public const float OBJECT_LAYER = 1 << 2;

        private float water_level = 0;
        private float WATER_LEVEL { get { return water_level / PIXELS_PER_UNIT; } set { water_level = value; } }
        private static readonly Dictionary<Room, RoomPhysics> _systems = new();
        public static int sceneNumber = 0;

        private readonly Room _room;
        private readonly Scene _scene;
        private readonly PhysicsScene2D _physics;
        private readonly Dictionary<UpdatableAndDeletable, GameObject> _linkedObjects = new();

        #region Hooks and Helpers
        public static void AddHooks()
        {
            On.Room.Update += Room_Update;
            On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
            On.AbstractRoom.Abstractize += AbstractRoom_Abstractize;
            On.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;
            On.ArenaSitting.SessionEnded += ArenaSitting_SessionEnded;
            On.ArenaSitting.ArenaPlayer.Reset += ArenaPlayer_Reset;
            On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;
            On.AbstractPhysicalObject.Abstractize += AbstractPhysicalObject_Abstractize;
            On.AbstractCreature.Realize += AbstractCreature_Realize;
            On.AbstractCreature.Abstractize += AbstractCreature_Abstractize;
            // Fix your typos rain world!!!!!!! wraghhhhhh!!!!! (wrath of 1000 slugcats)
            //On.Room.GetTile_int_int += Room_GetTile_int_int;
            //On.PhysicalObject.IsTileSolid += PhysicalObject_IsTileSolid;
        }

        private static void AbstractCreature_Abstractize(On.AbstractCreature.orig_Abstractize orig, AbstractCreature self, WorldCoordinate coord)
        {
            if (RoomPhysics.Get(self.realizedObject.room).TryGetObject(self.realizedObject, out var obj))
            {
                UnityEngine.Object.Destroy(obj);
            }
            orig(self, coord);
        }

        private static void AbstractCreature_Realize(On.AbstractCreature.orig_Realize orig, AbstractCreature self)
        {
            orig(self);
            var obj = new GameObject();
            obj.layer = 1 << 3;
            SceneManager.MoveGameObjectToScene(obj, RoomPhysics.Get(self.realizedObject.room)._scene);
            try
            {
                RoomPhysics.Get(self.realizedObject.room)._linkedObjects.Add(self.realizedObject, obj);
                obj.tag = "RW";
            }
            catch
            {
                UnityEngine.Object.Destroy(obj);
                throw;
            }
            var rb2d = obj.AddComponent<Rigidbody2D>();
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.drag = 0f;
            rb2d.gravityScale = 0.5f;
            rb2d.WakeUp();

            // This is where the actual shape is made
            for (int i = 0; i < self.realizedObject.bodyChunks.Length; i++)
            {
                var circle = obj.AddComponent<CircleCollider2D>();
                circle.radius = self.realizedObject.bodyChunks[i].rad / 20f;
            }
        }

        private static void AbstractPhysicalObject_Abstractize(On.AbstractPhysicalObject.orig_Abstractize orig, AbstractPhysicalObject self, WorldCoordinate coord)
        {
            if (RoomPhysics.Get(self.realizedObject.room).TryGetObject(self.realizedObject, out var obj))
            {
                UnityEngine.Object.Destroy(obj);
            }
            orig(self, coord);
        }

        private static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
        {
            orig(self);
            var obj = new GameObject();
            obj.layer = 1 << 3;
            SceneManager.MoveGameObjectToScene(obj, RoomPhysics.Get(self.realizedObject.room)._scene);
            try
            {
                RoomPhysics.Get(self.realizedObject.room)._linkedObjects.Add(self.realizedObject, obj);
            }
            catch
            {
                UnityEngine.Object.Destroy(obj);
                throw;
            }
            var rb2d = obj.AddComponent<Rigidbody2D>();
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.drag = 0f;
            rb2d.gravityScale = 0.5f;
            rb2d.WakeUp();

            // This is where the actual shape is made
            for (int i = 0; i < self.realizedObject.bodyChunks.Length; i++)
            {
                var circle = obj.AddComponent<CircleCollider2D>();
                circle.radius = self.realizedObject.bodyChunks[i].rad / 20f;
            }


        }

        private static void ArenaPlayer_Reset(On.ArenaSitting.ArenaPlayer.orig_Reset orig, ArenaSitting.ArenaPlayer self)
        {
            foreach (var system in _systems.Values)
            {
                system.Dispose();
            }

            _systems.Clear();

            orig(self);
        }

        private static void ArenaSitting_SessionEnded(On.ArenaSitting.orig_SessionEnded orig, ArenaSitting self, ArenaGameSession session)
        {
            foreach (var system in _systems.Values)
            {
                system.Dispose();
            }

            _systems.Clear();

            orig(self, session);
        }
        private static void ProcessManager_PostSwitchMainProcess(On.ProcessManager.orig_PostSwitchMainProcess orig, ProcessManager self, ProcessManager.ProcessID ID)
        {
            if (self.currentMainLoop != null && self.currentMainLoop is RainWorldGame)
            {
                RainWorldGame rainWorldGame = self.currentMainLoop as RainWorldGame;

                if (rainWorldGame.IsArenaSession)
                {
                    _systems.TryGetValue(rainWorldGame.GetArenaGameSession.room.abstractRoom.realizedRoom, out var system);
                    system.Dispose();
                }
            }

            orig(self, ID);

        }

        private static bool PhysicalObject_IsTileSolid(On.PhysicalObject.orig_IsTileSolid orig, PhysicalObject self, int bChunk, int relativeX, int relativeY)
        {
            if (orig(self, bChunk, relativeX, relativeY)) return true;
            if (self.room.ReadyForPlayer)
            {
                foreach (KeyValuePair<UpdatableAndDeletable, GameObject> item in RoomPhysics.Get(self.room)._linkedObjects)
                {
                    if (RoomPhysics.Get(self.room).PointInRb(item.Value, self.bodyChunks[bChunk].pos + new Vector2(relativeX * 20, relativeY * 20)))
                    {

                        return true;

                    }
                }
            }
            return false;
        }

        public static Room.Tile Room_GetTile_int_int(On.Room.orig_GetTile_int_int orig, Room self, int x, int y)
        {

            if (self.ReadyForPlayer)
            {
                var obj = RoomPhysics.Get(self);
                foreach (KeyValuePair<UpdatableAndDeletable, GameObject> item in obj._linkedObjects)
                {
                    if (obj.PointInRb(item.Value, new Vector2(x * 20, y * 20)))
                    {
                        Room.Tile tile = new Room.Tile(x, y, Room.Tile.TerrainType.Solid, false, false, false, 0, 0);
                        return tile;
                    }
                }
            }
            return orig(self, x, y);
        }

        private static void Room_Update(On.Room.orig_Update orig, Room self)
        {


            if (_systems.TryGetValue(self, out var system))
            {
                system.Update();
                orig(self);
                system.LateUpdate();
            }
            else
            {
                orig(self);
            }



        }

        private static void RainWorldGame_ShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
        {
            orig(self);

            foreach (var system in _systems.Values)
            {
                system.Dispose();
            }

            _systems.Clear();
        }

        private static void AbstractRoom_Abstractize(On.AbstractRoom.orig_Abstractize orig, AbstractRoom self)
        {
            if (self.realizedRoom != null && _systems.TryGetValue(self.realizedRoom, out var system))
            {
                system.Dispose();
                _systems.Remove(self.realizedRoom);
            }

            orig(self);
        }

        public static RoomPhysics Get(Room room)
        {
            if (!_systems.TryGetValue(room, out var system))
            {
                _systems[room] = system = new RoomPhysics(room);
            }

            return system;
        }
        #endregion Hooks and Helpers

        private RoomPhysics(Room room)
        {
            _room = room;
            this.water_level = room.floatWaterLevel;

            // Create a scene with physics independent from the main scene
            _scene = SceneManager.CreateScene($"Physics System {room.abstractRoom.name}" + sceneNumber, new CreateSceneParameters() { localPhysicsMode = LocalPhysicsMode.Physics2D });
            _physics = _scene.GetPhysicsScene2D();

            // Add room tiles as a collider
            RefreshTiles();

            sceneNumber++;
        }

        public bool TryGetObject(UpdatableAndDeletable owner, out GameObject gameObj)
        {
            return _linkedObjects.TryGetValue(owner, out gameObj);
        }

        public GameObject CreateObject(UpdatableAndDeletable owner)
        {
            var obj = new GameObject();
            obj.layer = 1 << 2;
            SceneManager.MoveGameObjectToScene(obj, _scene);
            try
            {
                _linkedObjects.Add(owner, obj);
            }
            catch
            {
                UnityEngine.Object.Destroy(obj);
                throw;
            }

            return obj;
        }



        //Layer 1 is Floor , Layer 2 is Unity Objects, Layer 3 is RW Objects
        private void RefreshTiles()
        {
            var obj = new GameObject("Room Geometry");
            obj.layer = (1 << 1);
            obj.isStatic = true;
            SceneManager.MoveGameObjectToScene(obj, _scene);

            // Makes rw objects ignore eachothers unity collision and the unity level collision
            Physics2D.IgnoreLayerCollision(1 << 3, 1 << 3);
            Physics2D.IgnoreLayerCollision(1 << 3, 1 << 1);

            var rb2d = obj.AddComponent<Rigidbody2D>();
            rb2d.bodyType = RigidbodyType2D.Static;

            var compositeCollider = obj.AddComponent<CompositeCollider2D>();
            compositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;
            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Outlines;

            var width = _room.TileWidth;
            var height = _room.TileHeight;
            var tiles = _room.Tiles;

            // Make polygons
            for (int y = 0; y < height; y++)
            {
                int? startX = null;

                for (int x = 0; x <= width; x++)
                {
                    var tile = x < width ? tiles[x, y] : null;
                    if (tile != null && tile.Terrain == Room.Tile.TerrainType.Solid)
                    {
                        if (startX == null)
                        {
                            startX = x;
                        }
                    }
                    else
                    {
                        if (startX != null)
                        {
                            var col = obj.AddComponent<BoxCollider2D>();

                            col.size = new Vector2(x - startX.Value, 1f) * 20f / PIXELS_PER_UNIT;
                            col.offset = new Vector2(startX.Value, y) * 20f / PIXELS_PER_UNIT + col.size / 2f;
                            col.usedByComposite = true;

                            startX = null;
                        }
                    }
                    Room.SlopeDirection slope = _room.IdentifySlope(new IntVector2(x, y));
                    if (tile != null && slope != Room.SlopeDirection.Broken && tiles[x, y].Terrain == Room.Tile.TerrainType.Slope)
                    {

                        if (slope == Room.SlopeDirection.UpRight)
                        {
                            var col = obj.AddComponent<PolygonCollider2D>();

                            col.points = new Vector2[3] { new Vector2(-1, 1) * 20f / PIXELS_PER_UNIT, new Vector2(-1, -1) * 20f / PIXELS_PER_UNIT, new Vector2(1, -1) * 20f / PIXELS_PER_UNIT };
                            col.offset = new Vector2(x, y) * 20f / PIXELS_PER_UNIT + Vector2.one * 10 / PIXELS_PER_UNIT;
                            col.usedByComposite = true;
                        }
                        else if (slope == Room.SlopeDirection.UpLeft)
                        {
                            var col = obj.AddComponent<PolygonCollider2D>();

                            col.points = new Vector2[3] { new Vector2(1, 1) * 20f / PIXELS_PER_UNIT, new Vector2(-1, -1) * 20f / PIXELS_PER_UNIT, new Vector2(1, -1) * 20f / PIXELS_PER_UNIT };
                            col.offset = new Vector2(x, y) * 20f / PIXELS_PER_UNIT + Vector2.one * 10 / PIXELS_PER_UNIT;
                            col.usedByComposite = true;
                        }
                        else if (slope == Room.SlopeDirection.DownRight)
                        {
                            var col = obj.AddComponent<PolygonCollider2D>();

                            col.points = new Vector2[3] { new Vector2(-1, 1) * 20f / PIXELS_PER_UNIT, new Vector2(-1, -1) * 20f / PIXELS_PER_UNIT, new Vector2(1, 1) * 20f / PIXELS_PER_UNIT };
                            col.offset = new Vector2(x, y) * 20f / PIXELS_PER_UNIT + Vector2.one * 10 / PIXELS_PER_UNIT;
                            col.usedByComposite = true;
                        }
                        else if (slope == Room.SlopeDirection.DownLeft)
                        {
                            var col = obj.AddComponent<PolygonCollider2D>();

                            col.points = new Vector2[3] { new Vector2(1, 1) * 20f / PIXELS_PER_UNIT, new Vector2(-1, 1) * 20f / PIXELS_PER_UNIT, new Vector2(-1, -1) * 20f / PIXELS_PER_UNIT };
                            col.offset = new Vector2(x, y) * 20f / PIXELS_PER_UNIT + Vector2.one * 10 / PIXELS_PER_UNIT;
                            col.usedByComposite = true;
                        }




                    }
                }
            }

            // Merge them
            compositeCollider.GenerateGeometry();

            // Get rid of the original colliders
            foreach (var col in obj.GetComponents<BoxCollider2D>())
            {
                UnityEngine.Object.Destroy(col);
            }
            foreach (var col in obj.GetComponents<PolygonCollider2D>())
            {
                UnityEngine.Object.Destroy(col);
            }


        }

        private void Update()
        {
            this.water_level = _room.floatWaterLevel;
            #region UnityObjUpdate
            foreach (var pair in _linkedObjects)
            {
                if (pair.Key.slatedForDeletetion || pair.Key.room != _room)
                {
                    UnityEngine.Object.Destroy(pair.Value);
                    _linkedObjects.Remove(pair.Key);
                }
            }

            var oldGrav = Physics2D.gravity;
            Physics2D.gravity = new Vector2(0f, -80f);
            _physics.Simulate(1f / 40f);
            Physics2D.gravity = oldGrav;
            #endregion


        }

        // Called from Room_Update, Used for collision to bodyChunks/floating water
        private void LateUpdate()
        {
            WaterFloatrb();
            CheckBodyChunkAgainstrb();

        }


        private void WaterFloatrb()
        {

            foreach (var item in _linkedObjects.ToList())
            {
                if (item.Value.GetComponent<Rigidbody2D>().position.y < WATER_LEVEL)
                {
                    Vector2 float_ = Vector2.up * (WATER_LEVEL * 4 - item.Value.GetComponent<Rigidbody2D>().position.y);
                    Debug.Log(float_);
                    item.Value.GetComponent<Rigidbody2D>().AddForce(float_);
                }
            }
        }

        // Collision between bodyChunks and RigidBodies
        private void CheckBodyChunkAgainstrb()
        {
            // Foreach error, check exceptionLog
            foreach (var obj in _room.updateList.ToList())
            {
                if (obj is PhysicalObject Pobj && Pobj.bodyChunks != null && !_linkedObjects.ContainsKey(obj))
                {
                    foreach (BodyChunk b in Pobj.bodyChunks.ToList())
                    {

                        foreach (var item in _linkedObjects.ToList())
                        {

                            ContactFilter2D CF = new ContactFilter2D();
                            CF.useLayerMask = true;
                            CF.layerMask = ~(1 << 2);
                            
                            //_physics.OverlapCircle((b.pos) / PIXELS_PER_UNIT, (b.rad + b.TerrainRad) / PIXELS_PER_UNIT, CF, result);


                                if (item.Key is Crate)
                                {
                                    Vector2 oldPos = b.pos;
                                    Crate c = item.Key as Crate;
                                    var phys = RoomPhysics.Get(c.room);
                                    if (phys.TryGetObject(c, out var obje))
                                    {
                                        

                                    Vector2[] hitPoint = ClosestPointToRb(obje.gameObject, b.pos, b.vel, b.rad, b.TerrainRad);

                                    (item.Key as Crate).debugSpr.NumberOfPoint[0] = hitPoint[0];
                                    (item.Key as Crate).debugSpr.NumberOfPoint[1] = hitPoint[1];

                                    // Corners of polygon (remove later)
                                    (item.Key as Crate).debugSpr.NumberOfPoint[2] = obje.GetComponent<Rigidbody2D>().position;
                                    (item.Key as Crate).debugSpr.NumberOfPoint[3] = obje.GetComponent<PolygonCollider2D>().points[1];
                                    (item.Key as Crate).debugSpr.NumberOfPoint[4] = obje.GetComponent<PolygonCollider2D>().points[2];
                                    (item.Key as Crate).debugSpr.NumberOfPoint[5] = obje.GetComponent<PolygonCollider2D>().points[3];

                                    // Use Cast() and the IsChunkTouchingGameObject() methods to check for collision.
                                    foreach (PolygonCollider2D polygon in obje.GetComponents<PolygonCollider2D>())
                                    {
                                        Vector2 temp = CastGameObject(polygon, b);
                                        if (temp != b.pos)
                                           {
                                            b.pos = temp;
                                           }
                                    }
                                }
                                }                           
                        }
                    }
                }
            }
        }
        public Dictionary<UpdatableAndDeletable, GameObject> ObjList { get { return this._linkedObjects; } }

        public Vector2 CastGameObject( PolygonCollider2D polygon, BodyChunk b )
        {
            Vector2 origPos = b.pos;
            Vector2 UniPos = b.pos / PIXELS_PER_UNIT;
            Vector2 UniVel = b.vel / PIXELS_PER_UNIT;
            Vector2 castB = b.pos + b.vel;
            Collider2D collider = IsChunkTouchingGameObject(polygon.gameObject, castB, b.rad);

            if (collider != null && collider is PolygonCollider2D && collider as PolygonCollider2D == polygon)
            {
                // Need to now correct velocity properly
                // Replace with Unity CircleCast https://docs.unity3d.com/ScriptReference/PhysicsScene2D.CircleCast.html
                //Vector2 point = _physics.CircleCast((UniPos), b.rad / PIXELS_PER_UNIT, UniVel.normalized, Vector2.Distance(UniPos - UniVel, UniPos) + 20f).point * PIXELS_PER_UNIT;
                Vector2 point = _physics.Raycast((UniPos - UniVel), UniVel.normalized, Vector2.Distance(UniPos - UniVel, UniPos) + 20f).point * PIXELS_PER_UNIT;
                if (Mathf.Abs(origPos.y - point.y) > 0.01f)
                {
                    int dir = Math.Sign(origPos.y - point.y);
                    if (Mathf.Abs(b.vel.y) >= b.owner.impactTreshhold) b.owner.TerrainImpact(b.index, new IntVector2(0, dir), Mathf.Abs(b.vel.y), b.contactPoint.y != dir);
                    if (b.contactPoint.y == 0) b.contactPoint.y = dir;
                    b.vel.y = Mathf.Abs(b.vel.y) * Mathf.Sign(b.pos.y - collider.transform.position.y) * b.owner.bounce;
                    b.vel.x *= Mathf.Clamp01(b.owner.surfaceFriction * 2f);
                    if (b.index == 1 && b.owner is Player ply)
                    {
                        ply.feetStuckPos = ply.bodyChunks[1].pos;
                    }
                }
                if (point != Vector2.zero ) 
                {
                    return point;
                }
            }

            return origPos;
        }
        public bool IsPointInRb(GameObject obj, Vector2 p)
        {
            p = obj.transform.InverseTransformPoint(p / PIXELS_PER_UNIT);
            float width = Vector2.Distance(obj.GetComponent<PolygonCollider2D>().GetPath(0)[0], obj.GetComponent<PolygonCollider2D>().GetPath(0)[1]) / 2;
            float height = Vector2.Distance(obj.GetComponent<PolygonCollider2D>().GetPath(0)[1], obj.GetComponent<PolygonCollider2D>().GetPath(0)[2]) / 2;
            if (Math.Abs(p.x) < width || Math.Abs(p.y) < height)
            {
                return true;
            }
            return false;
        }
        public bool PointInRb(GameObject obj, Vector2 p)
        {
            p = obj.transform.InverseTransformPoint(p / PIXELS_PER_UNIT);
            float width = Vector2.Distance(obj.GetComponent<PolygonCollider2D>().GetPath(0)[0], obj.GetComponent<PolygonCollider2D>().GetPath(0)[1]) / 2;
            float height = Vector2.Distance(obj.GetComponent<PolygonCollider2D>().GetPath(0)[1], obj.GetComponent<PolygonCollider2D>().GetPath(0)[2]) / 2;
            if (Math.Abs(p.x) < width && Math.Abs(p.y) < height)
            {
                return true;
            }
            return false;
        }
        public bool IsPointInAnyRb(Vector2 p)
        {
            foreach (var item in this._linkedObjects)
            {
                Vector2 point = item.Value.transform.InverseTransformPoint(p / PIXELS_PER_UNIT);
                float width = Vector2.Distance(item.Value.GetComponent<PolygonCollider2D>().GetPath(0)[0], item.Value.GetComponent<PolygonCollider2D>().GetPath(0)[1]) / 2;
                float height = Vector2.Distance(item.Value.GetComponent<PolygonCollider2D>().GetPath(0)[1], item.Value.GetComponent<PolygonCollider2D>().GetPath(0)[2]) / 2;
                if (Math.Abs(point.x) < width && Math.Abs(point.y) < height)
                {
                    return true;
                }
            }
            return false;
        }

        public Vector2[] ClosestPointToRb(GameObject obj, Vector2 p, Vector2 pVel, float rad, float terrainRad)
        {

            Vector2 relativePoint = obj.transform.InverseTransformPoint((p) / PIXELS_PER_UNIT);
            terrainRad /= PIXELS_PER_UNIT;
            rad /= PIXELS_PER_UNIT;
            Vector2[] hitPoint = new Vector2[2];

            float width = Vector2.Distance(obj.GetComponent<PolygonCollider2D>().GetPath(0)[0], obj.GetComponent<PolygonCollider2D>().GetPath(0)[1]) / 2;
            float height = Vector2.Distance(obj.GetComponent<PolygonCollider2D>().GetPath(0)[1], obj.GetComponent<PolygonCollider2D>().GetPath(0)[2]) / 2;
            if (Math.Abs(relativePoint.x) < width || Math.Abs(relativePoint.y) < height)
            {

                if (width - Math.Abs(relativePoint.x) < height - Math.Abs(relativePoint.y))
                {
                    hitPoint[0] = new Vector2(width * Math.Sign(relativePoint.x), relativePoint.y);
                    hitPoint[1] = new Vector2(relativePoint.x - (rad + terrainRad) * Math.Sign(relativePoint.x), relativePoint.y);
                }
                else
                {

                    hitPoint[0] = new Vector2(relativePoint.x, height * Math.Sign(relativePoint.y));

                    hitPoint[1] = new Vector2(relativePoint.x, relativePoint.y - (rad + terrainRad) * Math.Sign(relativePoint.y));
                }
            }
            else
            {
                hitPoint[0] = new Vector2(width * Math.Sign(relativePoint.x), height * Math.Sign(relativePoint.y));
                hitPoint[1] = relativePoint + pVel.normalized * rad;

            }
            hitPoint[0] = obj.transform.TransformPoint(hitPoint[0]) * PIXELS_PER_UNIT;
            hitPoint[1] = obj.transform.TransformPoint(hitPoint[1]) * PIXELS_PER_UNIT;
            return hitPoint;
        }
        //test
        public Collider2D IsChunkTouchingGameObject(GameObject obj, Vector2 p, float rad)
        {
            ContactFilter2D CF = new ContactFilter2D();
            CF.useLayerMask = true;
            CF.layerMask = ~(1 << 2);

            return _physics.OverlapCircle(p / PIXELS_PER_UNIT, rad / PIXELS_PER_UNIT, CF);
        }
        private void Dispose()
        {
            SceneManager.UnloadSceneAsync(_scene);
        }
    }
}