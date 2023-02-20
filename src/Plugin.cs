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

#pragma warning disable CS0618 // Do not remove the following line.
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace TestMod
    {

	// Add target_game_version and youtube_trailer_id to modinfo.json if applicable.
	// See https://rainworldmodding.miraheze.org/wiki/Downpour_Reference/Mod_Directories

	[BepInPlugin("cactus.testMod", "Test Mod - Crate", "0.1.0")]
	sealed class Plugin : BaseUnityPlugin
	{
		public Rect rect;
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
			On.BodyChunk.CheckHorizontalCollision += BodyChunk_CheckHorizontalCollision;
			On.BodyChunk.CheckVerticalCollision += BodyChunk_CheckVerticalCollision;
		}

		private void BodyChunk_ctor(On.BodyChunk.orig_ctor orig, BodyChunk self, PhysicalObject owner, int index, Vector2 pos, float rad, float mass)
		{
			orig(self, owner, index, pos, rad, mass);

			if (self.owner is Crate)
			{
				Debug.Log("Loading Crate BodyChunk ctor!");
				rect = new Rect(new Vector2(-rad, rad), new Vector2(rad * 2, rad * 2));
				rotationInDegrees = 45f;
				pivot = new Vector2(rect.center.x, rect.center.y);
			}

		}
		private void BodyChunk_Update(On.BodyChunk.orig_Update orig, BodyChunk self)
		{
			orig(self);
			if (self.owner is Crate)
            {
				rect.center = self.pos;
				Debug.Log("Rect Pos: " + rect.center);
				Debug.Log("Circle Pos: " + self.pos);
            }
		}

		private void BodyChunk_CheckHorizontalCollision(On.BodyChunk.orig_CheckHorizontalCollision orig, BodyChunk self)
		{
			if (self.owner is Crate)
			{
				Debug.Log("Running Crate Collision (Horizontal)");
				self.contactPoint.x = 0;

				// Used to know how far per pixel to Lerp from one Vector2 to the next when calculating number of points for collision detection.
				// Smaller values are very likely to get laggy
				Vector2 tileSize = new(1, 1);

				// Get the corner points of the rectangle
				Vector2 topLeft = new Vector2(rect.xMin, rect.yMax);
				Vector2 topRight = new Vector2(rect.xMax, rect.yMax);
				Vector2 bottomLeft = new Vector2(rect.xMin, rect.yMin);
				Vector2 bottomRight = new Vector2(rect.xMax, rect.yMin);

				// Calculate the number of points along each edge of the rectangle
				int pointsAlongTop = Mathf.RoundToInt(rect.width / tileSize.x);
				int pointsAlongBottom = Mathf.RoundToInt(rect.width / tileSize.x);
				int pointsAlongLeft = Mathf.RoundToInt(rect.height / tileSize.y);
				int pointsAlongRight = Mathf.RoundToInt(rect.height / tileSize.y);

				// Initialize collision normal to zero
				Vector2 collisionNormal = Vector2.zero;

				// Generate the points along the top edge of the rectangle
				for (int i = 0; i <= pointsAlongTop; i++)
				{
					Vector2 point = Vector2.Lerp(topLeft, topRight, (float)i / pointsAlongTop);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						rect = HandleCollisionResponse(rect, collisionNormal, 0, self);
						self.pos = rect.center;


						break;
					}
				}

				// Generate the points along the bottom edge of the rectangle
				for (int i = 0; i <= pointsAlongBottom; i++)
				{
					Vector2 point = Vector2.Lerp(bottomLeft, bottomRight, (float)i / pointsAlongBottom);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						rect = HandleCollisionResponse(rect, collisionNormal, 0, self);
						self.pos = rect.center;


						break;
					}
				}

				// Generate the points along the left edge of the rectangle
				for (int i = 0; i <= pointsAlongLeft; i++)
				{
					Vector2 point = Vector2.Lerp(bottomLeft, topLeft, (float)i / pointsAlongLeft);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						rect = HandleCollisionResponse(rect, collisionNormal, 0, self);
						self.pos = rect.center;


						break;
					}
				}

				// Generate the points along the right edge of the rectangle
				for (int i = 0; i <= pointsAlongRight; i++)
				{
					Vector2 point = Vector2.Lerp(bottomRight, topRight, (float)i / pointsAlongRight);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						rect = HandleCollisionResponse(rect, collisionNormal, 0, self);
						self.pos = rect.center;


						break;
					}
				}



				Quaternion rotation = Quaternion.FromToRotation(Vector3.up, collisionNormal);

				Vector2 position = rect.center + collisionNormal * (rect.width / 2f);

				rotationInDegrees = rotation.ToEulerAngles().z;
				rotationInDegrees %= 360;
				rect.position = position;
			}
			else
			{
				orig(self);
			}
		}

		private void BodyChunk_CheckVerticalCollision(On.BodyChunk.orig_CheckVerticalCollision orig, BodyChunk self)
		{
			if (self.owner is Crate)
			{
				Debug.Log("Running Crate Collision (Vertical)");
				self.contactPoint.x = 0;

				// Used to know how far per pixel to Lerp from one Vector2 to the next when calculating number of points for collision detection.
				// Smaller values are very likely to get laggy
				Vector2 tileSize = new(1, 1);

				// Get the corner points of the rectangle
				Vector2 topLeft = new Vector2(rect.xMin, rect.yMax);
				Vector2 topRight = new Vector2(rect.xMax, rect.yMax);
				Vector2 bottomLeft = new Vector2(rect.xMin, rect.yMin);
				Vector2 bottomRight = new Vector2(rect.xMax, rect.yMin);

				// Calculate the number of points along each edge of the rectangle
				int pointsAlongTop = Mathf.RoundToInt(rect.width / tileSize.x);
				int pointsAlongBottom = Mathf.RoundToInt(rect.width / tileSize.x);
				int pointsAlongLeft = Mathf.RoundToInt(rect.height / tileSize.y);
				int pointsAlongRight = Mathf.RoundToInt(rect.height / tileSize.y);

				// Initialize collision normal to zero
				Vector2 collisionNormal = Vector2.zero;

				// Generate the points along the top edge of the rectangle
				for (int i = 0; i <= pointsAlongTop; i++)
				{
					Vector2 point = Vector2.Lerp(topLeft, topRight, (float)i / pointsAlongTop);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						rect = HandleCollisionResponse(rect, collisionNormal, 0, self);
						self.pos = rect.center;

						break;
					}
				}

				// Generate the points along the bottom edge of the rectangle
				for (int i = 0; i <= pointsAlongBottom; i++)
				{
					Vector2 point = Vector2.Lerp(bottomLeft, bottomRight, (float)i / pointsAlongBottom);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						rect = HandleCollisionResponse(rect, collisionNormal, 0, self);
						self.pos = rect.center;

						break;
					}
				}

				// Generate the points along the left edge of the rectangle
				for (int i = 0; i <= pointsAlongLeft; i++)
				{
					Vector2 point = Vector2.Lerp(bottomLeft, topLeft, (float)i / pointsAlongLeft);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						rect = HandleCollisionResponse(rect, collisionNormal, 0, self);
						self.pos = rect.center;

						break;
					}
				}

				// Generate the points along the right edge of the rectangle
				for (int i = 0; i <= pointsAlongRight; i++)
				{
					Vector2 point = Vector2.Lerp(bottomRight, topRight, (float)i / pointsAlongRight);
					// Check for collision with tiles at the point
					if (CheckTileCollision(point, self, rect))
					{
						// Calculate the collision vector from the point to the center of the rectangle
						Vector2 collisionVector = rect.center - point;
						// Calculate the surface normal using the cross product of the collision vector and "up" direction
						collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

						rect = HandleCollisionResponse(rect, collisionNormal, 0, self);
						self.pos = rect.center;

						break;
					}
				}

				Quaternion rotation = Quaternion.FromToRotation(Vector3.up, collisionNormal);

				Vector2 position = rect.center + collisionNormal * (rect.width / 2f);

				rotationInDegrees = rotation.ToEulerAngles().z;
				rotationInDegrees %= 360;
				rect.position = position;
			}
			else
			{
				orig(self);
			}
		}

		// Checks for collision between point provided and rectangle
		public bool CheckTileCollision(Vector2 pointToCheck, BodyChunk self, Rect rectangle)
		{
			// Debug.Log("Checking Tile Collision"); (This Log lags the game out lmao)
			// Definitely gets to at least this point in the stack though (meaning the object does get loaded in the game and checks for collision)
			RWCustom.IntVector2 tilePosition = self.owner.room.GetTilePosition(self.lastPos);
			RWCustom.IntVector2 tilePos = self.owner.room.GetTilePosition(pointToCheck);
			//Debug.Log(tilePos); (log lags a lot)
			if (self.owner.room.GetTile(tilePos.x, tilePos.y).Terrain == Room.Tile.TerrainType.Solid /*&& self.owner.room.GetTile(tilePos.x - 1, tilePos.y).Terrain != Room.Tile.TerrainType.Solid && (tilePosition.x < tilePos.x || self.owner.room.GetTile(self.lastPos).Terrain == Room.Tile.TerrainType.Solid)*/)
			{
				// Passes first if statement, stops here with the rectangle.Contains method (not sure why, should be working?).
				if (rectangle.Contains(new Vector2(tilePos.x * 20, tilePos.y * 20)))
				{
					return true;
				}
			}
			return false;
		}

		// Something's up with the collision method here {as in, it don't work ): }, possibly need a second opinion on this
		private Rect HandleCollisionResponse(Rect rectangle, Vector2 surfaceNormal, float penetrationDepth, BodyChunk self)
		{
			Debug.Log("Handling Collision Response!");
			// Calculate the angle between the surface normal and the upward-facing vector
			float angle = Mathf.Atan2(surfaceNormal.x, surfaceNormal.y) * Mathf.Rad2Deg;

			// Rotate the rect by the opposite of the collision angle
			rectangle = RotateRect(rectangle, -angle, self);

			// Move the rect out of the collision by the penetration depth
			rectangle.position += surfaceNormal * penetrationDepth;
			self.pos = rectangle.position;

			// Rotate the rect back to its original orientation
			rectangle = RotateRect(rectangle, angle, self);

			// Update the position and rotation of the game object based on the new rect position and rotation
			transform.position = new Vector3(rectangle.x, rectangle.y, transform.position.z);
			transform.rotation = Quaternion.Euler(0f, 0f, angle);

			return rectangle;
		}

		// Rotates the rectangle to the desiered angle
		private Rect RotateRect(Rect rectangle, float angle, BodyChunk self)
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
	}
}