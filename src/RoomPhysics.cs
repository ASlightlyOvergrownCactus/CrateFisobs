using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestMod
{
    public class RoomPhysics
    {
        public const float PIXELS_PER_UNIT = 20f;

        private static readonly Dictionary<Room, RoomPhysics> _systems = new();

        private readonly Room _room;
        private readonly Scene _scene;
        private readonly PhysicsScene2D _physics;
        private readonly Dictionary<UpdatableAndDeletable, GameObject> _linkedObjects = new();
        public static Crate c;

        #region Hooks and Helpers
        public static void AddHooks()
        {
            On.Room.Update += Room_Update;
            On.RainWorldGame.ShutDownProcess += RainWorldGame_ShutDownProcess;
            On.AbstractRoom.Abstractize += AbstractRoom_Abstractize;
        }

        private static void Room_Update(On.Room.orig_Update orig, Room self)
        {
            if(_systems.TryGetValue(self, out var system))
            {
                system.Update();
            }

            orig(self);
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

            // Create a scene with physics independent from the main scene
            _scene = SceneManager.CreateScene($"Physics System {room.abstractRoom.name}", new CreateSceneParameters() { localPhysicsMode = LocalPhysicsMode.Physics2D });
            _physics = _scene.GetPhysicsScene2D();

            // Add room tiles as a collider
            RefreshTiles();
        }

        public bool TryGetObject(UpdatableAndDeletable owner, out GameObject gameObj)
        {
            return _linkedObjects.TryGetValue(owner, out gameObj);
        }

        public GameObject CreateObject(UpdatableAndDeletable owner, Crate crate)
        {
            c = crate;
            var obj = new GameObject();
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

        private void RefreshTiles()
        {
            var obj = new GameObject("Room Geometry");
            SceneManager.MoveGameObjectToScene(obj, _scene);

            var rb2d = obj.AddComponent<Rigidbody2D>();
            rb2d.bodyType = RigidbodyType2D.Static;
            var compositeCollider = obj.AddComponent<CompositeCollider2D>();
            compositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;
            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Outlines;

            var width = _room.TileWidth;
            var height = _room.TileHeight;
            var tiles = _room.Tiles;

            // Make polygons
            for(int y = 0; y < height; y++)
            {
                int? startX = null;

                for(int x = 0; x <= width; x++)
                {
                    var tile = x < width ? tiles[x, y] : null;
                    if (tile != null && tile.Solid)
                    {
                        if(startX == null)
                        {
                            startX = x;
                        }
                    }
                    else
                    {
                        if(startX != null)
                        {
                            var col = obj.AddComponent<BoxCollider2D>();
                            col.size = new Vector2(x - startX.Value, 1f) * 20f / PIXELS_PER_UNIT;
                            col.offset = new Vector2(startX.Value, y) * 20f / PIXELS_PER_UNIT + col.size / 2f;
                            col.usedByComposite = true;

                            startX = null;
                        }
                    }
                }
            }

            // Merge them
            compositeCollider.GenerateGeometry();

            // Get rid of the original colliders
            foreach(var col in obj.GetComponents<BoxCollider2D>())
            {
                UnityEngine.Object.Destroy(col);
            }
        }

        private void Update()
        {
            foreach(var pair in _linkedObjects)
            {
                if(pair.Key.slatedForDeletetion || pair.Key.room != _room)
                {
                    UnityEngine.Object.Destroy(pair.Value);
                    _linkedObjects.Remove(pair.Key);
                }
            }

            var oldGrav = Physics2D.gravity;
            Physics2D.gravity = new Vector2(0f, -80f);
            _physics.Simulate(1f / 40f);
            Physics2D.gravity = oldGrav;
        }

        private void Dispose()
        {
            SceneManager.UnloadSceneAsync(_scene);
        }
    }
}
