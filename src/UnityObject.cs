using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestMod
{
    internal class UnityObject : MonoBehaviour
    {
        public GameObject obj;
        public bool isUniObject;
        public Rigidbody2D rb2d;
        public UpdatableAndDeletable owner;
        public AbstractPhysicalObject physObj;
        public CircleCollider2D[] circles;
        // For Unity Objects
        public UnityObject(RoomPhysics phys, UnityEngine.Vector2[] shape, RigidbodyType2D type, float drag, float gravityScale, UpdatableAndDeletable owner) 
        {
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
                Destroy(this);
                throw;
            }

            isUniObject = true;
        }

        // For Rainworld Objects
        public UnityObject(RoomPhysics phys, RigidbodyType2D type, float drag, float gravityScale, AbstractPhysicalObject physObj)
        {
            obj = new GameObject();
            rb2d = obj.AddComponent<Rigidbody2D>();
            rb2d.bodyType = type;
            rb2d.drag = drag;
            rb2d.gravityScale = gravityScale;
            rb2d.WakeUp();

            // Circles for each bodyChunk
            circles = new CircleCollider2D[physObj.realizedObject.bodyChunks.Length];
            for (int i = 0; i < physObj.realizedObject.bodyChunks.Length; i++)
            {
                circles[i] = obj.AddComponent<CircleCollider2D>();
                circles[i].radius = physObj.realizedObject.bodyChunks[i].rad / 20f;
            }
            this.physObj = physObj;

            obj.layer = 1 << 3;

            try
            {
                phys._linkedObjects.Add(owner, obj);
            }
            catch
            {
                UnityEngine.Object.Destroy(obj);
                Destroy(this);
                throw;
            }

            isUniObject = false;
        }


        public void Destroy(UnityObject obj)
        {
            MonoBehaviour.Destroy(obj);
        }

        public void Update()
        {
            if (isUniObject)
            {

            }

            else
            {
                rb2d.position.Set(physObj.realizedObject.firstChunk.pos.y / RoomPhysics.PIXELS_PER_UNIT, physObj.realizedObject.firstChunk.pos.y / RoomPhysics.PIXELS_PER_UNIT);
                for (int i = 0; i < circles.Length; i++)
                {
                    UnityEngine.Vector2 offset = rb2d.position - (physObj.realizedObject.bodyChunks[i].pos / RoomPhysics.PIXELS_PER_UNIT);
                    circles[i].offset = offset;
                }
            }
        }


    }
}
