using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Fisobs.Core;

using BepInEx;
using System.Security.Permissions;
using static TestMod.Plugin;
using TestMod;

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

		public static bool DEBUGMODE = true;
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

		
		// Structure that stores the results of the PolygonCollision function
		public struct PolygonCollisionResult
		{
			// Are the polygons going to intersect forward in time?
			public bool WillIntersect;
			// Are the polygons currently intersecting?
			public bool Intersect;
			// The translation to apply to the first polygon to push the polygons apart.
			public Vector2 MinimumTranslationVector;
			// Contains the tile collided with (i dinked)
			public Vector2 collisionTile;
			// Contains the number that corresponds to the side collided.
			public int collidedSide;

			public Vector2[] line1;
			
			public Vector2[] line2;
			

			public Vector2 CollisionPos;
			public Vector2 ReflectedNormal;

			public Vector2 Push;
        }



		// Uses DIAGONAL method of collision detection
		public PolygonCollisionResult PolygonCollisionTile(Polygon polygonA,
							  TilePolygon polygonTile, Vector2 velocity)
		{
			PolygonCollisionResult result = new PolygonCollisionResult();
			Polygon poly1 = polygonA;
			TilePolygon polyTile = polygonTile;
			
			

			for (int shape = 1; shape <= 2; shape++)
            {
				if (shape == 1)
				{
					// Check diagonals of polygon...
					for (int p = 0; p < poly1.corners.Length; p++)
					{
						Vector2 line_r1s = poly1.center.pos;
						Vector2 line_r1e = poly1.corners[p];


						// ... against edges of other polygon
						for (int q = 0; q < polyTile.corners.Length; q++)
						{
							Vector2 line_r2s = polyTile.corners[q];
							Vector2 line_r2e = polyTile.corners[(q + 1) % polyTile.corners.Length];

							//Find two line intersect then filter out with AABB check :{}

							Vector2 CollisionPoint = Helper.LineCollide(line_r1s, line_r1e, line_r2s, line_r2e);
							if (Helper.AABB(line_r1s, line_r1e, line_r2s, line_r2e, CollisionPoint))
							{
								result.Intersect = true;
								result.collisionTile = polyTile.center;
								result.collidedSide = p;

								result.line1 = new Vector2[2] { line_r1s, line_r1e };
								result.line2 = new Vector2[2] { line_r2s, line_r2e };
								result.CollisionPos = CollisionPoint;
								result.Push = Helper.PolyPushOutOfLine(line_r1s,line_r1s,line_r1e,line_r2s, line_r2e, polygonTile, velocity);
								//result.Xdifference = Mathf.Abs((line_r1s.x < polyTile.corners[1].x ? line_r1s.x : line_r1e.x) - polyTile.corners[1].x);
								result.ReflectedNormal=new Vector2(-(line_r2e-line_r2s).y,(line_r2e - line_r2s).x).normalized;
								if(result.Push!=Vector2.zero)return result;
							}

						}
					}
				}
				else if (shape == 20)
				{
					// Check diagonals of polygon...
					for (int p = 0; p < polyTile.corners.Length; p++)
					{
                        Vector2 line_r1s = polyTile.center;
                        Vector2 line_r1e = polyTile.corners[(p + 1) % polyTile.corners.Length];

                        for (int q = 0; q < poly1.corners.Length; q++)
						{
							Vector2 line_r2s = poly1.corners[q];
							Vector2 line_r2e = poly1.corners[(q + 1) % poly1.corners.Length];
							//Find two line intersect then filter out with AABB check :)

							Vector2 CollisionPoint = Helper.LineCollide(line_r1s, line_r1e, line_r2s, line_r2e);
							if (Helper.AABB(line_r1s, line_r1e, line_r2s, line_r2e, CollisionPoint))
							{
								result.Intersect = true;
								result.collisionTile = polyTile.center;
								result.collidedSide = p;

								result.line1 = new Vector2[2] { line_r1s, line_r1e };
								result.line2 = new Vector2[2] { line_r2s, line_r2e };
								result.CollisionPos = CollisionPoint;
								
								//result.Ydifference = Math.Abs(py - poly1.corners[q].y);
								return result;
							}

							//https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection

						}
					}
				}
			
				
            }

			return result;
		}

		private void BodyChunk_Update(On.BodyChunk.orig_Update orig, BodyChunk self)
		{
			
			if (self.owner is Crate)
            {
				//Debug.Log("Starting tile initialization Loop");
				var crate = self.owner as Crate;

				float[] colRectDimensions = new float[4];

				colRectDimensions[0] = crate.rect.corners[0].x; // X value for collision rect start point
				colRectDimensions[1] = crate.rect.corners[0].y; // Y value for collision rect start point
				colRectDimensions[2] = crate.rect.corners[0].x; // X value for collision rect X length
				colRectDimensions[3] = crate.rect.corners[0].y; // Y value for collision rect Y length

				// X value most left
                for (int i = 0; i < crate.rect.corners.Length; i++)
                {
					if (crate.rect.corners[i].x < colRectDimensions[0])
                    {
						colRectDimensions[0] = crate.rect.corners[i].x;
                    }
                }

				// Y value most up
				for (int i = 0; i < crate.rect.corners.Length; i++)
				{
					if (crate.rect.corners[i].y > colRectDimensions[1])
					{
						colRectDimensions[1] = crate.rect.corners[i].y;
					}
				}

				// X value most right
				for (int i = 0; i < crate.rect.corners.Length; i++)
				{
					if (crate.rect.corners[i].x > colRectDimensions[2])
					{
						colRectDimensions[2] = crate.rect.corners[i].x;
					}
				}

				// Y value most down
				for (int i = 0; i < crate.rect.corners.Length; i++)
				{
					if (crate.rect.corners[i].y < colRectDimensions[3])
					{
						colRectDimensions[3] = crate.rect.corners[i].y;
					}
				}

				colRectDimensions[0] -= 80f;//x y x y
				colRectDimensions[1] += 80f;
				colRectDimensions[2] += 80f;
				colRectDimensions[3] -= 80f;

				if (DEBUGMODE) 
				{
					(self.owner as Crate).DebugSpr.ColliSquare = new Vector2[4]
					{
						new Vector2(colRectDimensions[0],colRectDimensions[3]),
						new Vector2(colRectDimensions[0],colRectDimensions[1]),
						new Vector2(colRectDimensions[2],colRectDimensions[1]),
						new Vector2(colRectDimensions[2],colRectDimensions[3]),

					};
									
				}
				RWCustom.IntVector2 startPoint = self.owner.room.GetTilePosition(new Vector2(colRectDimensions[0], colRectDimensions[1]));
				RWCustom.IntVector2 dimensions = self.owner.room.GetTilePosition(new Vector2(colRectDimensions[2] - colRectDimensions[0], colRectDimensions[1] - colRectDimensions[3]));
				//Debug.Log(startPoint);
				//Debug.Log(dimensions);


				Rect collisionDetector = new(startPoint.x, startPoint.y, dimensions.x, dimensions.y);
				crate.rect.collisionContainer.Clear();
				List<TilePolygon> AllTile = new List<TilePolygon>();
				 
				for (int i = 0; i < collisionDetector.height; i++)
                {
					for (int a = 0; a < collisionDetector.width; a++)
                    {
						RWCustom.IntVector2 TilePos= new RWCustom.IntVector2(a + (int)collisionDetector.x, (int)collisionDetector.y - i);

						if (self.owner.room.GetTile(TilePos).Terrain == Room.Tile.TerrainType.Solid)
						{
							bool flag = false;

							foreach (TilePolygon p in crate.rect.collisionContainer)
							{
								//Debug.Log("Got into tile check");
								if (TilePos.x*20+10 == p.center.x && TilePos.y*20+10 == p.center.y)
								{
									//Debug.Log("Matching Tile");
									flag = true;
									break;
								}
							}
							if (!flag)
							{

								//Debug.Log("Tile added to list");
								AllTile.Add( new TilePolygon(TilePos.ToVector2()*20 + new Vector2(10, 10), TilePolygon.DefaultShape.Square) ) ;
							}
						}
					}
                }
				if (AllTile.Count > 0)
				{
					AllTile = PolygonMerger.Merge(AllTile);
					foreach (TilePolygon p in AllTile) { crate.rect.collisionContainer.Add(p); };
				}
				

                if (DEBUGMODE)
				{
					(self.owner as Crate).DebugSpr.Tiles = crate.rect.collisionContainer;

                }
               
                //if (crate.rect.collisionContainer.Count > 0)
                //{
                //	for (int i = 0; i < crate.rect.collisionContainer.Count; i++)
                //	{
                //		TilePolygon temp = crate.rect.collisionContainer[i];
                //		Vector2 check = temp.center/20 ;
                //		if (!collisionDetector.Contains(check))
                //		{
                //			//Debug.Log("removing");
                //			crate.rect.collisionContainer.RemoveAt(i);
                //			//Debug.Log("Finished removing");
                //		}

                //	}
                //}


                // Only use this log for debugging!!! This lags a LOT!!!!!
                /*
				Debug.Log(crate.rect.center / 20f);
				Debug.Log(crate.rect.collisionContainer.Count);
				foreach (TilePolygon p in crate.rect.collisionContainer)
				{
					Debug.Log("X: " + p.center.x + "   Y: " + p.center.y);
				}
				*/
            }
            orig(self);
        }


		private void BodyChunk_CheckHorizontalCollision(On.BodyChunk.orig_CheckHorizontalCollision orig, BodyChunk self)
		{
			if (self.owner is Crate)
			{
				//self.CheckVerticalCollision();
				//var crate = self.owner as Crate;

				//for (int i = 0; i < crate.rect.collisionContainer.Count; i++)
				//{
				//	PolygonCollisionResult polygonCollisionResult = PolygonCollisionTile(crate.rect, crate.rect.collisionContainer[i], self.vel);

				//	if (polygonCollisionResult.Intersect)
				//	{
				//		//Debug.Log("Currently Colliding!!! With " + polygonCollisionResult.collisionTile);
				//	}
				//	else if (polygonCollisionResult.WillIntersect)
				//	{
				//		Debug.Log("Will Collide!!!");
				//	}
				//}
			
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
                bool willbounced = false;
                PolygonCollisionResult polygonCollisionResult = new PolygonCollisionResult();
                Crate crate = self.owner as Crate;

				for (int i = 0; i < crate.rect.collisionContainer.Count; i++)
				{
                    
                     polygonCollisionResult = PolygonCollisionTile(crate.rect, crate.rect.collisionContainer[i], self.vel);

                    if(DEBUGMODE) crate.DebugSpr.result = polygonCollisionResult;
                    if (polygonCollisionResult.Intersect)
					{	
						//willbounced = true;
						//Debug.Log(polygonCollisionResult.Push);
						//self.pos += polygonCollisionResult.Push;

						//self.HardSetPosition(self.pos);
											
						//crate.rect.UpdateCornerPoints();
						

						//Debug.Log("Currently Colliding!!! With " + polygonCollisionResult.collisionTile);
						//self.pos = self.lastPos;
						if (polygonCollisionResult.collidedSide == 0)
                        {
							//Debug.Log("Left collide tile");
							
							
                        }
						else if (polygonCollisionResult.collidedSide == 1)
						{
							//Debug.Log("Up collide tile");
                            
                        }
						else if (polygonCollisionResult.collidedSide == 2)
						{
							//Debug.Log("Right collide tile");
                          
                        }
						else if (polygonCollisionResult.collidedSide == 3)
						{
							//Debug.Log("Down collide tile");
                           
                        }
						//return;  //breaks when found one colli
					}
					else if (polygonCollisionResult.WillIntersect)
					{
						Debug.Log("Will Collide!!!");
                      
                    }
				}

                if (willbounced)
                {

                    self.vel = self.vel.magnitude * Vector2.Reflect(self.vel.normalized, polygonCollisionResult.ReflectedNormal) * 1f;//Vector2.Reflect(self.vel,polygonCollisionResult.ReflectedNormal)*0.6f ;

                    //                    if (polygonCollisionResult.Push.y != 0)
                    //self.vel.y *= -1  * 0.5f;
                    //                    if (polygonCollisionResult.Push.x != 0)
                    //                        self.vel.x *= -1  * 0.5f;

                    //self.vel*= -polygonCollisionResult.Push.normalized*0.4f;
                   
                }
            }
			else
			{
				orig(self);
			}
		}

		public static string CurrentRoomName { get; private set; }
		public int x = 500;
		public int y = 500;
	}
}