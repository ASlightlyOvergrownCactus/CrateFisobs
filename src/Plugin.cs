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
				//Debug.Log("Starting Ray Loop");
				var crate = self.owner as Crate;
				// Raycaster
				// Sets radius ray will go to
				float rayRad = (self.rad * 2f) + 80f;
				//Debug.Log(rayRad);
				for (int degree = 1; degree <= 360; degree++)
                {
					// Rotates for rays on each degree around object
					Vector2 ray = RWCustom.Custom.RotateAroundOrigo(Vector2.right, degree);
					Vector2 rayDirection = ray;
					ray += self.pos;

					for (int pix = 1; pix <= rayRad; pix++)
                    {
						ray += rayDirection;

						RWCustom.IntVector2 tilePos = self.owner.room.GetTilePosition(ray);
						if (self.owner.room.GetTile(tilePos.x + 2, tilePos.y + 3).Terrain == Room.Tile.TerrainType.Solid)
                        {
							bool flag = false;

							foreach(RWCustom.IntVector2 v in crate.rect.collisionContainer)
                            {
								//Debug.Log("Got into tile check");
								if (tilePos.x == v.x && tilePos.y == v.y)
                                {
									//Debug.Log("Matching Tile");
									flag = true;
									break;
                                }
                            }
							if (!flag)
							{
								//Debug.Log("Tile added to list");
								crate.rect.collisionContainer.Add(tilePos);
							}
						}
					}
					
				}
				//Debug.Log("Reached removal");
				if (crate.rect.collisionContainer.Count > 0)
				{
					for (int i = 0; i < crate.rect.collisionContainer.Count; i++)
					{
						RWCustom.IntVector2 temp = crate.rect.collisionContainer[i];
						if (Vector2.Distance(new Vector2(temp.x * 20, temp.y * 20), self.pos) > rayRad)
						{
							//Debug.Log("removing");
							crate.rect.collisionContainer.RemoveAt(i);
							//Debug.Log("Finished removing");
						}

					}
				}
				// Only use this log for debugging!!! This lags a LOT!!!!!
				/*
				Debug.Log(crate.rect.collisionContainer.Count);
				foreach (RWCustom.IntVector2 v in crate.rect.collisionContainer)
                {
					Debug.Log(v.ToString());
                }
				*/
			}
		}


		private void BodyChunk_CheckHorizontalCollision(On.BodyChunk.orig_CheckHorizontalCollision orig, BodyChunk self)
		{
			if (self.owner is Crate)
			{
				var crate = self.owner as Crate;
				int x = 0;
				//Debug.Log("Running Crate Collision (Horizontal)");
				//self.contactPoint.x = 0;

				// Used to know how far per pixel to Lerp from one Vector2 to the next when calculating number of points for collision detection.
				// Smaller values are very likely to get laggy
				float tileSize = 10;

				if (self.vel.x > 0)
                {
					// For movement to right
					x = 1;
                }
				else if (self.vel.x < 0)
                {
					// For movement to left
					x = 2;
                }

				// Initialize collision normal to zero
				Vector2 collisionNormal = Vector2.zero;

				// Big ol' for loop calculates points on each side of shape, goes from corner[0] -> [1] ->[2] ... [x] -> [0]
				for (int a = 0; a < crate.rect.corners.Length; a++)
				{
					int points;
					if (a != crate.rect.corners.Length - 1)
					{
						points = Mathf.RoundToInt(Vector2.Distance(crate.rect.corners[a], crate.rect.corners[a + 1]) / tileSize);
						// Generate the points along the side
						for (int i = 0; i <= points; i++)
						{
							Vector2 point = Vector2.Lerp(crate.rect.corners[a], crate.rect.corners[a + 1], (float)i / points);
							// Check for collision with tiles at the point
							if (CheckTileCollision(point, self, crate.rect, x))
							{
								// Calculate the collision vector from the point to the center of the rectangle
								Vector2 collisionVector = crate.rect.center - point;
								// Calculate the surface normal using the cross product of the collision vector and "up" direction
								collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

								crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, self, x, point);

								break;
							}
						}
					}

					else
					{
						points = Mathf.RoundToInt(Vector2.Distance(crate.rect.corners[a], crate.rect.corners[0]) / tileSize);
						// Generate the points along the side
						for (int i = 0; i <= points; i++)
						{
							Vector2 point = Vector2.Lerp(crate.rect.corners[a], crate.rect.corners[0], (float)i / points);
							// Check for collision with tiles at the point
							if (CheckTileCollision(point, self, crate.rect, x))
							{
								// Calculate the collision vector from the point to the center of the rectangle
								Vector2 collisionVector = crate.rect.center - point;
								// Calculate the surface normal using the cross product of the collision vector and "up" direction
								collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

								crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, self, x, point);

								break;
							}
						}
					}

				}

				Quaternion rotation = Quaternion.FromToRotation(Vector3.up, collisionNormal);

				Vector2 position = crate.rect.center + collisionNormal * (crate.rect.width / 2f);

				rotationInDegrees = rotation.ToEulerAngles().z;
				rotationInDegrees %= 360;
				crate.rect.center = position;
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
				var crate = self.owner as Crate;
				int x = 0;
				//Debug.Log("Running Crate Collision (Horizontal)");
				//self.contactPoint.x = 0;

				// Used to know how far per pixel to Lerp from one Vector2 to the next when calculating number of points for collision detection.
				// Smaller values are very likely to get laggy
				float tileSize = 10;

				if (self.vel.y > 0)
                {
					// For collision upwards
					x = 3;
                }
				else if (self.vel.y < 0)
                {
					// For collision downwards
					x = 4;
                }

				// Initialize collision normal to zero
				Vector2 collisionNormal = Vector2.zero;

				// Big ol' for loop calculates points on each side of shape, goes from corner[0] -> [1] ->[2] ... [x] -> [0]
				for (int a = 0; a < crate.rect.corners.Length; a++)
                {
					int points;
					if (a != crate.rect.corners.Length - 1)
					{
						points = Mathf.RoundToInt(Vector2.Distance(crate.rect.corners[a], crate.rect.corners[a + 1]) / tileSize);
						// Generate the points along the side
						for (int i = 0; i <= points; i++)
						{
							Vector2 point = Vector2.Lerp(crate.rect.corners[a], crate.rect.corners[a + 1], (float)i / points);
							// Check for collision with tiles at the point
							if (CheckTileCollision(point, self, crate.rect, x))
							{
								// Calculate the collision vector from the point to the center of the rectangle
								Vector2 collisionVector = crate.rect.center - point;
								// Calculate the surface normal using the cross product of the collision vector and "up" direction
								collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

								crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, self, x, point);

								break;
							}
						}
					}

					else
					{
						points = Mathf.RoundToInt(Vector2.Distance(crate.rect.corners[a], crate.rect.corners[0]) / tileSize);
						// Generate the points along the side
						for (int i = 0; i <= points; i++)
						{
							Vector2 point = Vector2.Lerp(crate.rect.corners[a], crate.rect.corners[0], (float)i / points);
							// Check for collision with tiles at the point
							if (CheckTileCollision(point, self, crate.rect, x))
							{
								// Calculate the collision vector from the point to the center of the rectangle
								Vector2 collisionVector = crate.rect.center - point;
								// Calculate the surface normal using the cross product of the collision vector and "up" direction
								collisionNormal = Vector3.Cross(collisionVector, Vector3.up).normalized;

								crate.rect = HandleCollisionResponse(crate.rect, collisionNormal, self, x, point);

								break;
							}
						}
					}
					
				}

				Quaternion rotation = Quaternion.FromToRotation(Vector3.up, collisionNormal);

				Vector2 position = crate.rect.center + collisionNormal * (crate.rect.width / 2f);

				rotationInDegrees = rotation.ToEulerAngles().z;
				rotationInDegrees %= 360;
				crate.rect.center = position;
			}
			else
			{
				orig(self);
			}
		}

		// Checks for collision between point provided and rectangle
		public bool CheckTileCollision(Vector2 pointToCheck, BodyChunk self, Rectangle rectangle, int direction)
		{

			RWCustom.IntVector2 tilePos = self.owner.room.GetTilePosition(pointToCheck);

			Vector2 tileCenter = self.owner.room.MiddleOfTile(new RWCustom.IntVector2(tilePos.x, tilePos.y));

			// Code chunk here calculates which side the crate collides with by checking which normal the position collided is closest to (might need some work)

			Vector2 rightNormal = new(1f, 0);
			Vector2 leftNormal = new(-1f, 0);
			Vector2 upNormal = new(0, 1f);
			Vector2 downNormal = new(0, -1f);

			Vector2 sub = (tileCenter - pointToCheck).normalized;

			float rightDis = Vector2.Distance(sub, rightNormal);
			float leftDis = Vector2.Distance(sub, leftNormal);
			float upDis = Vector2.Distance(sub, upNormal);
			float downDis = Vector2.Distance(sub, downNormal);



			switch (direction)
            {
				case 0:

					break;
				case 1: // Right Collision
					if (self.owner.room.GetTile(tilePos.x + 2, tilePos.y + 3).Terrain == Room.Tile.TerrainType.Solid)
					{
						//Debug.Log(sub);
						//Debug.Log("Passed right!");
						if (rightDis < leftDis && rightDis < upDis && rightDis < downDis)
						{
							//Debug.Log("Right!");
							return true;
						}
					}
					break;
				case 2: // Left Collision
					if (self.owner.room.GetTile(tilePos.x - 2, tilePos.y + 3).Terrain == Room.Tile.TerrainType.Solid)
					{
						//Debug.Log(sub);
						//Debug.Log("Passed left!");
						if (leftDis < rightDis && leftDis < upDis && leftDis < downDis)
						{
							//Debug.Log("Left!");
							return true;
						}
					}
					break;
				case 3: // Up Collision
					if (self.owner.room.GetTile(tilePos.x + 2, tilePos.y + 3).Terrain == Room.Tile.TerrainType.Solid)
					{
						//Debug.Log(sub);
						//Debug.Log("Passed up!");
						if (upDis < rightDis && upDis < leftDis && upDis < downDis)
						{
							//Debug.Log("Up!");
							return true;
						}
					}
					break;
				case 4: // Down Collision
					if (self.owner.room.GetTile(tilePos.x + 2, tilePos.y + 3).Terrain == Room.Tile.TerrainType.Solid)
                    {
						//Debug.Log(sub);
						//Debug.Log("Passed down!");
						if (downDis < rightDis && downDis < upDis && downDis < leftDis)
						{
							//Debug.Log("Down!");
							return true;
						}
                    }
					break;
                default:
					Debug.Log("Invalid direction check called");
					break;
            }



			return false;
		}

		private Rectangle HandleCollisionResponse(Rectangle rectangle, Vector2 surfaceNormal, BodyChunk self, int collisionDirection, Vector2 point)
		{
			float offset;
			Vector2 offVec;

			RWCustom.IntVector2 tilePos = self.owner.room.GetTilePosition(new Vector2(point.x, point.y));

			// Determines angle of position vector
			float angleInRadians = (Mathf.PI / 180) * Vector2.Angle(self.pos, self.lastPos);
			Vector2 vec2 = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
			//Debug.Log(vec2);

			Vector2 tileCenter = self.owner.room.MiddleOfTile(tilePos.x, tilePos.y);



			switch (collisionDirection)
            {
				case 0: // No Velocity

					break;

				case 1: // Right Collision ->
					self.vel.x = -self.vel.x * self.owner.bounce;

					tileCenter += new Vector2(-10f, 0f);
					offset = tileCenter.x - point.x;
					//Debug.Log(tileCenter.y + " " + point.y);
					offVec = new Vector2(offset, 0f);
					//Debug.Log("Right" + offVec);
					self.pos += offVec;
					if (Mathf.Abs(self.vel.x) < 1f + 9f * (1f - self.owner.bounce))
					{
						self.vel.x = 0f;
					}
					rectangle.UpdateCornerPoints();
					break;

				case 2: // Left Collision <-
					self.vel.x = -self.vel.x * self.owner.bounce;

					tileCenter += new Vector2(10f, 0f);
					offset = tileCenter.x - point.x;
					//Debug.Log(tileCenter.y + " " + point.y);
					offVec = new Vector2(offset, 0f);
					//Debug.Log("Left" + offVec);
					self.pos += offVec;
					if (Mathf.Abs(self.vel.x) < 1f + 9f * (1f - self.owner.bounce))
					{
						self.vel.x = 0f;
					}
					rectangle.UpdateCornerPoints();
					break;

				case 3: // Up Collision ^
					self.vel.y = -self.vel.y * self.owner.bounce;

					tileCenter += new Vector2(0f, -10f);
					offset = tileCenter.y - point.y;
					//Debug.Log(tileCenter.y + " " + point.y);
					offVec = new Vector2(0f, offset);
					//Debug.Log("Up" + offVec);
					self.pos += offVec;
					if (Mathf.Abs(self.vel.y) < 1f + 9f * (1f - self.owner.bounce))
					{
						self.vel.y = 0f;
					}
					rectangle.UpdateCornerPoints();
					break;

				case 4: // Down Collision v
					self.vel.y = -self.vel.y * self.owner.bounce;

					tileCenter += new Vector2(0f, 10f);
					offset = tileCenter.y - point.y;
					//Debug.Log(tileCenter.y + " " + point.y);
					offVec = new Vector2(0f, offset);
					//Debug.Log("Down" + offVec);
					self.pos += offVec;
					if (self.vel.y < self.owner.gravity || self.vel.y < 1f + 9f * (1f - self.owner.bounce))
					{
						self.vel.y = 0f;
					}
					rectangle.UpdateCornerPoints();
					break;

				default:
					Debug.Log("Invalid Direction entered in collision response!");
					break;
            }
			//Debug.Log(self.pos);
			return rectangle;
		}

		public static string CurrentRoomName { get; private set; }
		public int x = 500;
		public int y = 500;
	}
}