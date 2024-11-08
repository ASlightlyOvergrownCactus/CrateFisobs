using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TestMod.Object_Stuff;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

namespace TestMod
{
    internal class UnityObject
    {
        public Rigidbody2D rb2d;
        public CircleCollider2D circle;
        public GameObject obj;
        public bool isUniObject;
        public UpdatableAndDeletable owner;
        public AbstractPhysicalObject physObj;
        
        // For Unity Objects
        public UnityObject(RoomPhysics phys, UnityEngine.Vector2[] shape, RigidbodyType2D type, float drag, float gravityScale, UpdatableAndDeletable owner) 
        {
            Debug.Log("beginning unity object creation");
            obj = new GameObject();
            rb2d = obj.AddComponent<Rigidbody2D>();
            rb2d.bodyType = type;
            rb2d.drag = drag;
            rb2d.gravityScale = gravityScale;
            rb2d.WakeUp();

            var polygon = obj.AddComponent<PolygonCollider2D>();
            // SetPath is used to define the shape of the collider. The index value is which part of it we're setting. Check https://docs.unity3d.com/ScriptReference/PolygonCollider2D.SetPath.html for more info.
            polygon.points = shape;

            this.owner = owner;

            obj.layer = 1 << 2;
            SceneManager.MoveGameObjectToScene(obj, phys._scene);

            try
            {
                phys._linkedObjects.Add(owner, obj);
            }
            catch
            {
                UnityEngine.Object.Destroy(obj);
                throw;
            }

            isUniObject = true;
        }

        // For Rainworld Objects
        public UnityObject(RoomPhysics phys, RigidbodyType2D type, float drag, float gravityScale, AbstractPhysicalObject physObj)
        {
            Debug.Log("beginning rw object creation");
            obj = new GameObject();

            rb2d = obj.AddComponent<Rigidbody2D>();
            rb2d.bodyType = type;
            rb2d.drag = drag;
            rb2d.gravityScale = gravityScale;
            rb2d.WakeUp();
            this.physObj = physObj;
            owner = physObj.realizedObject;

            // Circles for each bodyChunk
            for (int i = 0; i < physObj.realizedObject.bodyChunks.Length; i++)
            {
                circle = obj.AddComponent<CircleCollider2D>();
                circle.radius = physObj.realizedObject.bodyChunks[i].rad / RoomPhysics.PIXELS_PER_UNIT;
                Debug.Log("Collider added");
            }

            RWMono rwMono = obj.AddComponent<RWMono>();
            rwMono.SetPhysObj(physObj);

            obj.layer = 1 << 3;

            try
            {
                phys._linkedObjects.Add(owner, obj);
            }
            catch
            {
                UnityEngine.Object.Destroy(obj);
                throw;
            }

            isUniObject = false;
        }
    }
}
