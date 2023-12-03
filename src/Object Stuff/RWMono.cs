using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TestMod.Object_Stuff
{
    internal class RWMono : MonoBehaviour
    {
        public Rigidbody2D rb2d;
        public CircleCollider2D[] circles;
        public AbstractPhysicalObject physObj;
        void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
            circles = GetComponents<CircleCollider2D>();
        }

        public void SetPhysObj(AbstractPhysicalObject physObj)
        {
            this.physObj = physObj;
        }
        public void Update()
        {
            rb2d.position = physObj.realizedObject.firstChunk.pos / RoomPhysics.PIXELS_PER_UNIT;
            for (int i = 0; i < circles.Length; i++)
            {
                UnityEngine.Vector2 offset = rb2d.position - (physObj.realizedObject.bodyChunks[i].pos / RoomPhysics.PIXELS_PER_UNIT);
                circles[i].offset = offset;
            }
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("Collision at " + collision.contacts[0].point * RoomPhysics.PIXELS_PER_UNIT);
        }
    }
}
