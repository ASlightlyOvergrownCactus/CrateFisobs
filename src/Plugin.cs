using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Fisobs.Core;

using BepInEx;
using System.Security.Permissions;

// IMPORTANT
// This requires Fisobs to work!
// Big thx to Dual-Iron (on github) for help with Fisobs!
// This code was based off of Dual-Iron's Centishield as practice, I didn't make parts of this!

// Plugin class has (or at least im attempting) to get custom rectangular collision. Definitely a WIP rn

// Problem with update method, need to either make compatible with game or make a new update method entirely

#pragma warning disable CS0618 // Do not remove the following line.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TestMod
    {

	// Add target_game_version and youtube_trailer_id to modinfo.json if applicable.
	// See https://rainworldmodding.miraheze.org/wiki/Downpour_Reference/Mod_Directories

	[BepInPlugin("cactus.testMod", "Test Mod - Crate", "0.1.0")]
	sealed class Plugin : BaseUnityPlugin
	{
		public Vector2 pivot;
		public float rotationInDegrees;

		public void OnEnable()
		{
			// How to make a hook:
			{
				/*
			On.RainWorld.OnModsInit += Init;

				// Register hooks

				On.Rock.ctor += Rock_ctor;
				*/
			}

			Content.Register(new CrateFisobs());

			On.BodyChunk.ctor += BodyChunk_ctor;
			On.BodyChunk.Update += BodyChunk_Update;
			//On.RoomCamera.SpriteLeaser.ctor += SpriteLeaser_ctor;
			On.BodyChunk.CheckHorizontalCollision += BodyChunk_CheckHorizontalCollision;
			On.BodyChunk.CheckVerticalCollision += BodyChunk_CheckVerticalCollision;
		}

        private void BodyChunk_ctor(On.BodyChunk.orig_ctor orig, BodyChunk self, PhysicalObject owner, int index, Vector2 pos, float rad, float mass)
		{
			orig(self, owner, index, pos, rad, mass);

			if (self.owner is Crate)
			{
				rotationInDegrees = 45f;
			}

		}
		private void BodyChunk_Update(On.BodyChunk.orig_Update orig, BodyChunk self)
		{
			orig(self);
			if (self.owner is Crate)
            {
			}
		}


		private void BodyChunk_CheckHorizontalCollision(On.BodyChunk.orig_CheckHorizontalCollision orig, BodyChunk self)
		{
			/*if (self.owner is Crate)
			{
				var crate = self.owner as Crate;
				//Debug.Log("Running Crate Collision (Horizontal)");
				self.contactPoint.x = 0;

				// Used to know how far per pixel to Lerp from one Vector2 to the next when calculating number of points for collision detection.
				// Smaller values are very likely to get laggy
				Vector2 tileSize = new(1, 1);

				// Calculate the number of points along each edge of the rectangle
				int pointsAlongTop = Mathf.RoundToInt(crate.rect.width / tileSize.x);
				int pointsAlongBottom = Mathf.RoundToInt(crate.rect.width / tileSize.x);
				int pointsAlongLeft = Mathf.RoundToInt(crate.rect.height / tileSize.y);
				int pointsAlongRight = Mathf.RoundToInt(crate.rect.height / tileSize.y);

				// Initialize collision normal to zero
				Vector2 collisionNormal = Vector2.zero;

				// Generate the points along the top edge of the rectangle
				for (int i = 0; i <= pointsAlongTop; i++)
				{
					Vector2 point = Vector2.Lerp(crate.rect.corners[2], crate.rect.corners[3], (float)i / pointsAlongTop);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, crate.rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = crate.rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, 0, self);
						self.pos = crate.rect.center;


						break;
					}
				}

				// Generate the points along the bottom edge of the rectangle
				for (int i = 0; i <= pointsAlongBottom; i++)
				{
					Vector2 point = Vector2.Lerp(crate.rect.corners[0], crate.rect.corners[1], (float)i / pointsAlongBottom);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, crate.rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = crate.rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, 0, self);
						self.pos = crate.rect.center;


						break;
					}
				}

				// Generate the points along the left edge of the rectangle
				for (int i = 0; i <= pointsAlongLeft; i++)
				{
					Vector2 point = Vector2.Lerp(crate.rect.corners[0], crate.rect.corners[2], (float)i / pointsAlongLeft);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, crate.rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = crate.rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, 0, self);


						break;
					}
				}

				// Generate the points along the right edge of the rectangle
				for (int i = 0; i <= pointsAlongRight; i++)
				{
					Vector2 point = Vector2.Lerp(crate.rect.corners[1], crate.rect.corners[3], (float)i / pointsAlongRight);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, crate.rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = crate.rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, 0, self);
						self.pos = crate.rect.center;


						break;
					}
				}



				Quaternion rotation = Quaternion.FromToRotation(Vector3.up, collisionNormal);

				Vector2 position = crate.rect.center + collisionNormal * (crate.rect.width / 2f);

				rotationInDegrees = rotation.ToEulerAngles().z;
				rotationInDegrees %= 360;
				crate.rect.center = position;
			}
			else*/
			{
				orig(self);
			}
		}

		private void BodyChunk_CheckVerticalCollision(On.BodyChunk.orig_CheckVerticalCollision orig, BodyChunk self)
		{

			/*if (self.owner is Crate)
			{
				var crate = self.owner as Crate;
				//Debug.Log("Running Crate Collision (Horizontal)");
				self.contactPoint.x = 0;

				// Used to know how far per pixel to Lerp from one Vector2 to the next when calculating number of points for collision detection.
				// Smaller values are very likely to get laggy
				Vector2 tileSize = new(1, 1);

				// Calculate the number of points along each edge of the rectangle
				int pointsAlongTop = Mathf.RoundToInt(crate.rect.width / tileSize.x);
				int pointsAlongBottom = Mathf.RoundToInt(crate.rect.width / tileSize.x);
				int pointsAlongLeft = Mathf.RoundToInt(crate.rect.height / tileSize.y);
				int pointsAlongRight = Mathf.RoundToInt(crate.rect.height / tileSize.y);

				// Initialize collision normal to zero
				Vector2 collisionNormal = Vector2.zero;

				// Generate the points along the top edge of the rectangle
				for (int i = 0; i <= pointsAlongTop; i++)
				{
					Vector2 point = Vector2.Lerp(crate.rect.corners[2], crate.rect.corners[3], (float)i / pointsAlongTop);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, crate.rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = crate.rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, 0, self);
						self.pos = crate.rect.center;


						break;
					}
				}

				// Generate the points along the bottom edge of the rectangle
				for (int i = 0; i <= pointsAlongBottom; i++)
				{
					Vector2 point = Vector2.Lerp(crate.rect.corners[0], crate.rect.corners[1], (float)i / pointsAlongBottom);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, crate.rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = crate.rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, 0, self);
						self.pos = crate.rect.center;


						break;
					}
				}

				// Generate the points along the left edge of the rectangle
				for (int i = 0; i <= pointsAlongLeft; i++)
				{
					Vector2 point = Vector2.Lerp(crate.rect.corners[0], crate.rect.corners[2], (float)i / pointsAlongLeft);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, crate.rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = crate.rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, 0, self);


						break;
					}
				}

				// Generate the points along the right edge of the rectangle
				for (int i = 0; i <= pointsAlongRight; i++)
				{
					Vector2 point = Vector2.Lerp(crate.rect.corners[1], crate.rect.corners[3], (float)i / pointsAlongRight);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, crate.rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = crate.rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, 0, self);
						self.pos = crate.rect.center;


						break;
					}
				}



				Quaternion rotation = Quaternion.FromToRotation(Vector3.up, collisionNormal);

				Vector2 position = crate.rect.center + collisionNormal * (crate.rect.width / 2f);

				rotationInDegrees = rotation.ToEulerAngles().z;
				rotationInDegrees %= 360;
				crate.rect.center = position;
			}
			else*/
			{
				orig(self);
			}
		}

		// Checks for collision between point provided and rectangle
		public bool CheckTileCollision(Vector2 pointToCheck, BodyChunk self, Rectangle rectangle)
		{
			// Debug.Log("Checking Tile Collision"); (This Log lags the game out lmao)
			RWCustom.IntVector2 tilePosition = self.owner.room.GetTilePosition(self.lastPos);
			RWCustom.IntVector2 tilePos = self.owner.room.GetTilePosition(pointToCheck);
			//Debug.Log(tilePos); //(log lags a lot)
			if (self.owner.room.GetTile(tilePos.x, tilePos.y).Terrain == Room.Tile.TerrainType.Solid /*&& self.owner.room.GetTile(tilePos.x - 1, tilePos.y).Terrain != Room.Tile.TerrainType.Solid && (tilePosition.x < tilePos.x || self.owner.room.GetTile(self.lastPos).Terrain == Room.Tile.TerrainType.Solid)*/)
			{
				
				// Note for modders: GetTile returns the position of the tile on the grid! The actual position of the object is in pixels, so the tilePos of it is it's position in pixels * 20!

					return true;

			}
			return false;
		}

		// Something's up with the collision method here {as in, it don't work ): }, possibly need a second opinion on this
		private Rectangle HandleCollisionResponse(Rectangle rectangle, Vector2 surfaceNormal, float penetrationDepth, BodyChunk self)
		{
			Debug.Log("Handling Collision Response!");

			// Calculate the angle between the surface normal and the upward-facing vector
			float angle = Mathf.Atan2(surfaceNormal.x, surfaceNormal.y) * Mathf.Rad2Deg;

			// Rotate the rect by the opposite of the collision angle
			rectangle.UpdateCornerPointsWithAngle(-angle);

			// Move the rect out of the collision by the penetration depth
			rectangle.Move(surfaceNormal * penetrationDepth);
			//self.pos = rectangle.center;

			// Rotate the rect back to its original orientation
			rectangle.UpdateCornerPointsWithAngle(angle);

			// Update the position and rotation of the game object based on the new rect position and rotation
			transform.position = new Vector3(rectangle.center.x, rectangle.center.y, transform.position.z);
			transform.rotation = Quaternion.Euler(0f, 0f, angle);

			return rectangle;
		}

		// Rotates the rectangle to the desiered angle
		/*
		private Rect RotateRect(Rectangle rectangle, float angle, BodyChunk self)
		{
			rotationInDegrees = angle;
			Debug.Log("Rotating Rectangle");
			rectangle.center = self.pos;
			Vector2[] corners = new Vector2[4];
			corners[0] = new Vector2(rectangle.x, rectangle.y); // Top-left corner
			corners[1] = new Vector2(rectangle.x + rectangle.width, rectangle.y); // Top-right corner
			corners[2] = new Vector2(rectangle.x + rectangle.width, rectangle.y + rectangle.height); // Bottom-right corner
			corners[3] = new Vector2(rectangle.x, rectangle.y + rectangle.height); // Bottom-left corner

			for (int i = 0; i < corners.Length; i++)
			{
				Vector2 offset = corners[i] - pivot;
				float radians = rotationInDegrees * Mathf.Deg2Rad;
				float rotatedX = offset.x * Mathf.Cos(radians) - offset.y * Mathf.Sin(radians);
				float rotatedY = offset.x * Mathf.Sin(radians) + offset.y * Mathf.Cos(radians);
				corners[i] = new Vector2(rotatedX, rotatedY) + pivot;
			}
			rectangle.position = corners[0];
			rectangle.width = corners[1].x - corners[0].x;
			rectangle.height = corners[3].y - corners[0].y;

			return rectangle;
		}
		// Rock bounce thingy (DO NOT)
		/*private void Rock_ctor(On.Rock.orig_ctor orig, Rock self, AbstractPhysicalObject abstractPhysicalObject, World world)
            {
                orig(self, abstractPhysicalObject, world);
                self.gravity = 0.1f;
                self.bounce = 3.0f;
            }*/
		public static string CurrentRoomName { get; private set; }
		public int x = 500;
		public int y = 500;
	}
}