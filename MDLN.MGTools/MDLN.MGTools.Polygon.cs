using MDLN.Tools;
using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	/// <summary>
	/// Class to manage and draw a 2D polygon shape using the provided GraphicsDevice
	/// </summary>
	public class ConvexPolygon : ICollidable, IVisible
	{
		private CollisionRegion cCollisionList;
		private Color cLineClr;
		private Color cFillClr;
		private Texture2D cLineTexture;

		private Texture2D cFillTexture;
		private readonly GraphicsDevice cGraphDev;
		private readonly BasicEffect cBasicShader;
		private List<Vector2> caTexturePos;

		private Vector2 cvBaseOffset;
		private Vector2 cvScale;
		private Vector2 cvMove;
		private float cnRotation;
		private List<Vector2> cavBaseVertexList;


		/// <summary>
		/// Set true to draw perimeter lines around the shape
		/// </summary>
		public bool DrawOutline;

		/// <summary>
		/// Set true to fill the shape with a solid color
		/// </summary>
		public bool FillShape;

		/// <summary>
		/// Specify a line width in pixels for the perimeter line
		/// </summary>
		public int LineWidth;

		/// <summary>
		/// Used to set/get the color to fill the shape with
		/// </summary>
		public Color FillColor {
			get {
				return cFillClr;
			}

			set {
				//Save the color
				cFillClr = value;

				//Set it as the texture
				cFillTexture = new Texture2D(cGraphDev, 1, 1);
				cFillTexture.SetData(new[] { value });
			}
		}

		/// <summary>
		/// Used to set/get the color to use for the perimeter lines
		/// </summary>
		public Color LineColor {
			get {
				return cLineClr;
			}

			set {
				//Save the color
				cLineClr = value;

				//Set it as the texture
				cLineTexture = new Texture2D(cGraphDev, 1, 1);
				cLineTexture.SetData(new[] { value });
			}
		}

		public Texture2D FillTexture {
			set {
				cFillTexture = value;
			}
		}

		/// <summary>
		/// Specifies the base offset, meaning the starting position, to be used for all
		/// directional offsets
		/// </summary>
		public Vector2 BaseOffset {
			get {
				return cvBaseOffset;
			}

			set {
				cvBaseOffset = value;
			}
		}

		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="GraphDev">Graphics Device this object should use to draw though</param>
		public ConvexPolygon(GraphicsDevice GraphDev) {
			//Setup all class variables
			FillShape = true;
			DrawOutline = true;
			LineWidth = 1;
			cnRotation = 0;

			cavBaseVertexList = new List<Vector2>();

			cvScale = new Vector2(1, 1);
			cvMove = new Vector2(0, 0);
			cvBaseOffset = new Vector2(0, 0);

			caTexturePos = new List<Vector2>();

			cGraphDev = GraphDev;

			cLineClr = new Color(0, 0, 0, 0); //Black and fully transparent
			cFillClr = new Color(0, 0, 0, 0); //Black and fully transparent
			cCollisionList = new CollisionRegion {
				Type = CollideType.ConvexPolygon,
				Vertexes = new List<Vector2>()
			};

			UpdateGaphicsDevice(GraphDev);

			return;
		}

		/// <summary>
		/// Used to retrieve the regions this object occupies
		/// </summary>
		/// <returns></returns>
		public IEnumerable<CollisionRegion> GetCollisionRegions() {
			return new CollisionRegion[1] { cCollisionList };
		}

		/// <summary>
		/// Tests if this object overlaps or collides with another region
		/// </summary>
		/// <param name="TestRegions">List of regions to test for collisions with</param>
		/// <returns>True if a collisions has occurred, false otherwise</returns>
		public bool TestCollision(IEnumerable<CollisionRegion> TestRegions) {
			foreach (CollisionRegion CurrReg in TestRegions) {
				switch (CurrReg.Type) {
				case CollideType.ConvexPolygon:
					//See if this polygon is hitting the other one
					foreach (Vector2 Vertex in cCollisionList.Vertexes) {
						if (MGMath.PointInConvexPolygon(Vertex, CurrReg.Vertexes) == true) {
							return true;
						}
					}

					//See if the other polygon is inside this one
					foreach (Vector2 Vertex in CurrReg.Vertexes) {
						if (MGMath.PointInConvexPolygon(Vertex, cCollisionList.Vertexes) == true) {
							return true;
						}
					}

					//Need a test for when edges completely span the other polygon.  
					//Vertexes are all outside, edges intersect

					return false;
				case CollideType.Rectangle:
					Point Coord;
					Vector2 Corner;

					//See if this polygon is hitting the rectangle
					foreach (Vector2 Vertex in cCollisionList.Vertexes) {
						Coord.X = (int)Vertex.X;
						Coord.Y = (int)Vertex.Y;
						if (MGMath.IsPointInRect(Coord, CurrReg.RectOffsets) == true) {
							return true;
						}
					}

					Corner.X = CurrReg.RectOffsets.X;
					Corner.Y = CurrReg.RectOffsets.Y;
					if (MGMath.PointInConvexPolygon(Corner, cCollisionList.Vertexes) == true) {
						return true;
					}

					Corner.X = CurrReg.RectOffsets.X + CurrReg.RectOffsets.Width;
					Corner.Y = CurrReg.RectOffsets.Y;
					if (MGMath.PointInConvexPolygon(Corner, cCollisionList.Vertexes) == true) {
						return true;
					}

					Corner.X = CurrReg.RectOffsets.X;
					Corner.Y = CurrReg.RectOffsets.Y + CurrReg.RectOffsets.Height;
					if (MGMath.PointInConvexPolygon(Corner, cCollisionList.Vertexes) == true) {
						return true;
					}

					Corner.X = CurrReg.RectOffsets.X + CurrReg.RectOffsets.Width;
					Corner.Y = CurrReg.RectOffsets.Y + CurrReg.RectOffsets.Height;
					if (MGMath.PointInConvexPolygon(Corner, cCollisionList.Vertexes) == true) {
						return true;
					}

					return false;
				case CollideType.Circle:
					//Need to work out this math.  
					//If any vertex of the polygon is closer to the circle center than the radius, collides
					//If a line from the center perpendicular to a side of the polygon hits the side and 
					//is shorter than the radius of the circle, collide
					return false;
				default:
					return false;
				}
			}

			return false;
		}

		/// <summary>
		/// Tests if this object overlaps of collides with another object
		/// </summary>
		/// <param name="TestObj">Object to test against for collisions</param>
		/// <returns>True if a collisions has occurred, false otherwise</returns>
		public bool TestCollision(ICollidable TestObj) {
			return TestCollision(TestObj.GetCollisionRegions());
		}

		/// <summary>
		/// Gets the coordinates that are the center of the shape
		/// </summary>
		/// <returns></returns>
		public Vector2 GetCenterCoordinates() {
			Vector2 vCoords = cvBaseOffset;

			vCoords.X += cvMove.X;
			vCoords.Y += cvMove.Y;

			return vCoords;
		}

		/// <summary>
		/// Add another vertex to the polygon.  These vertexes will not be sorted, so the order they 
		/// are added must create a convex shape.
		/// </summary>
		/// <param name="NewVert">The coordinates of the new vertex</param>
		/// <returns>True if the vertex was added</returns>
		public bool AddVertex(Vector2 NewVert) {
			cavBaseVertexList.Add(NewVert);

			RecalculateCoordinates();

			return true;
		}

		/// <summary>
		/// Specify all base vertexes this polygon should use
		/// These vertexes are as they appear before any of the transformations
		/// Directional offsets, scaling, and rotation
		/// </summary>
		/// <param name="avVertList"></param>
		/// <returns></returns>
		public bool SetVertexes(IEnumerable<Vector2> avVertList) {
			cavBaseVertexList.Clear();
			cavBaseVertexList.AddRange(avVertList);

			RecalculateCoordinates();

			if (cCollisionList.Vertexes.Count == 1) {
				caTexturePos.Add(new Vector2(0, 0));
			} else {
				switch (cCollisionList.Vertexes.Count % 3) {
				case 0:
					caTexturePos.Add(new Vector2(0, 1));
					break;
				case 1:
					caTexturePos.Add(new Vector2(1, 1));
					break;
				case 2:
				default:
					caTexturePos.Add(new Vector2(1, 0));
					break;
				}
			}

			return true;
		}

		public bool AddVertex(Vector2 NewVert, Vector2 TexturePos) {
			cCollisionList.Vertexes.Add(NewVert);

			caTexturePos.Add(TexturePos);

			return true;
		}

		/// <summary>
		/// Change the coordinates of an existing vertex
		/// </summary>
		/// <param name="nIdx">Index (order added) of the vertex</param>
		/// <param name="Vert">New coordinates for the vertex</param>
		/// <returns>True if the vertex was updated</returns>
		public bool UpdateVertex(int nIdx, Vector2 Vert) {
			Vector2 vAdjVert;

			if (nIdx >= cavBaseVertexList.Count) { //Vertex does not exist
				return false;
			}

			//Undo all transformations, move and center offset
			Vert.X -= cvMove.X + cvBaseOffset.X;
			Vert.Y -= cvMove.Y + cvBaseOffset.Y;

			//Rotation
			vAdjVert.X = (float)((Vert.X * Math.Cos(-1 * cnRotation)) - (Vert.Y * Math.Sin(-1 * cnRotation)));
			vAdjVert.Y = (float)((Vert.X * Math.Sin(-1 * cnRotation)) + (Vert.Y * Math.Cos(-1 * cnRotation)));

			//Scaling
			vAdjVert.X /= cvScale.X;
			vAdjVert.Y /= cvScale.Y;

			//Re-apply center offset
			vAdjVert.X += cvBaseOffset.X;
			vAdjVert.Y += cvBaseOffset.Y;

			cavBaseVertexList[nIdx] = vAdjVert;

			RecalculateCoordinates();

			return true;
		}

		/// <summary>
		/// Specifies the center coordinates for this shape
		/// </summary>
		public Vector2 CenterCoordinates {
			get {
				return cvMove;
			}

			set {
				cvMove.X = value.X - cvBaseOffset.X;
				cvMove.Y = value.Y - cvBaseOffset.Y;

				RecalculateCoordinates();

				return;
			}
		}

		/// <summary>
		/// Resize the polygon proportional to its current size
		/// The scalign will have its origin be the shape's center coordinates
		/// </summary>
		public Vector2 ScaleShape {
			get {
				return cvScale;
			}

			set {
				cvScale = value;

				RecalculateCoordinates();

				return;
			}
		}

		/// <summary>
		/// Rotate the shape around the center coordinates this amount in Radians
		/// </summary>
		public float RotateShape {
			get {
				return cnRotation;
			}

			set {
				cnRotation = value;

				RecalculateCoordinates();

				return;
			}
		}

		/// <summary>
		/// Specift the values for all transformations
		/// </summary>
		/// <param name="vMove">Directional offset</param>
		/// <param name="vScale">Scaling factors</param>
		/// <param name="nRadians">Radians of rotation</param>
		public void SetPositionOffsets(Vector2 vMove, Vector2 vScale, float nRadians) {
			cvMove = vMove;
			cvScale = vScale;
			cnRotation = nRadians;

			RecalculateCoordinates();

			return;
		}

		/// <summary>
		/// Removes all vertexes from the polygon
		/// </summary>
		/// <returns>True upon success, false on error</returns>
		public bool RemoveAllVertexes() {
			cCollisionList.Vertexes.Clear();

			return true;
		}

		/// <summary>
		/// Retrieve a list of all vertexes for this shape
		/// </summary>
		/// <returns>A collection of all vertexes in this shape</returns>
		public IEnumerable<Vector2> GetVertexes() {
			return cavBaseVertexList;
		}

		/// <summary>
		/// If the graphics device being used is modified externally this must be called for
		/// the changes to be reflected in this objects rendering
		/// </summary>
		/// <param name="NewGraphDev">Modified graphics device</param>
		public void UpdateGaphicsDevice(GraphicsDevice NewGraphDev) {
			cGraphDev = NewGraphDev;

			//Create a basec shader to use when rendering the polygon
			cBasicShader = new BasicEffect(cGraphDev) {
				VertexColorEnabled = true,
				World = Matrix.CreateOrthographicOffCenter(0, cGraphDev.Viewport.Width, cGraphDev.Viewport.Height, 0, 0, 1),
			};
		}

		/// <summary>
		/// Call to render this shape through the specified graphics device
		/// </summary>
		/// <returns>True if the shape was drawn successfully</returns>
		public bool Draw() {
			Rectangle LineRect;
			int nCtr, nPrevVert, nSurfNum;
			Vector2 LineFromOrigin, RotOrigin;
			Vector LineSeg = new Vector();
			RasterizerState PriorRaster, NewRaster;
			VertexPositionTexture[] aVertexes;
			VertexBuffer VtxBuff;

			SpriteBatch DrawBatch = new SpriteBatch(cGraphDev);

			DrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			if (DrawOutline == true) {
				RotOrigin.X = 0f;
				RotOrigin.Y = 0.5f; //Rotation from texture, middle of left side

				//Draw lines from Vertex -1 to current vertex
				for (nCtr = 0; nCtr < cCollisionList.Vertexes.Count; nCtr++) {
					if (nCtr != 0) {
						nPrevVert = nCtr - 1;
					} else {
						nPrevVert = cCollisionList.Vertexes.Count - 1;
					}

					//Get the vector as if the line segment started at the origin
					LineFromOrigin.X = cCollisionList.Vertexes[nCtr].X - cCollisionList.Vertexes[nPrevVert].X;
					LineFromOrigin.Y = cCollisionList.Vertexes[nCtr].Y - cCollisionList.Vertexes[nPrevVert].Y;
					LineSeg.SetRectangularCoordinates(LineFromOrigin.X, LineFromOrigin.Y);

					//Create the rectangle for this line segment
					LineRect.X = (int)cCollisionList.Vertexes[nPrevVert].X;
					LineRect.Y = (int)cCollisionList.Vertexes[nPrevVert].Y;

					LineRect.Width = (int)(LineSeg.Polar.Length + (LineWidth / 2));
					LineRect.Height = LineWidth;

					//Draw the rectangle rotated to create the line
					DrawBatch.Draw(cLineTexture, LineRect, cLineTexture.Bounds, Color.White, (float)(LineSeg.Polar.Angle * Math.PI / 180), RotOrigin, SpriteEffects.None, 0);
				}
			}

			DrawBatch.End();

			if ((FillShape == true) && (cCollisionList.Vertexes.Count > 2)) {
				//Draw the triangle fill (triangles needed is Vertexes -2, then 3 vertexes per triangle)
				aVertexes = new VertexPositionTexture[(cCollisionList.Vertexes.Count - 2) * 3];

				for (nCtr = 2; nCtr < cCollisionList.Vertexes.Count; nCtr++) {
					nSurfNum = (nCtr - 2) * 3; //Which surface/triangle are we filling in

					//Every triangle gets 3 vertexes in the list, none are shared in a TriangleList
					//Always use index zero as a common point
					aVertexes[nSurfNum].Position = new Vector3(cCollisionList.Vertexes[0].X, cCollisionList.Vertexes[0].Y, 0);
					aVertexes[nSurfNum].TextureCoordinate = caTexturePos[0];

					//The other vertexes are pairs of the remaining vertexes
					aVertexes[nSurfNum + 1].Position = new Vector3(cCollisionList.Vertexes[nCtr].X, cCollisionList.Vertexes[nCtr].Y, 0);
					aVertexes[nSurfNum + 1].TextureCoordinate = caTexturePos[nCtr];
					aVertexes[nSurfNum + 2].Position = new Vector3(cCollisionList.Vertexes[nCtr - 1].X, cCollisionList.Vertexes[nCtr - 1].Y, 0);
					aVertexes[nSurfNum + 2].TextureCoordinate = caTexturePos[nCtr - 1];
				}

				//Save off the current rasterizer, then make sure all primitives are drawn
				PriorRaster = cGraphDev.RasterizerState;
				NewRaster = new RasterizerState {
					CullMode = CullMode.None
				};

				cGraphDev.RasterizerState = NewRaster;

				cBasicShader.Texture = cFillTexture;

				VtxBuff = new VertexBuffer(cGraphDev, typeof(VertexPositionTexture), aVertexes.Length, BufferUsage.WriteOnly);
				VtxBuff.SetData(aVertexes);
				cGraphDev.SetVertexBuffer(VtxBuff);

				//Make sure all passes of the effects/shader are being used
				foreach (EffectPass CurrShadderPass in cBasicShader.CurrentTechnique.Passes) {
					//This is the all-important line that sets the effect, and all of its settings, on the graphics device
					CurrShadderPass.Apply();

					cGraphDev.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, aVertexes, 0, aVertexes.Length / 3);
				}

				//Restore the rasterizer
				cGraphDev.RasterizerState = PriorRaster;
			}

			return true;
		}

		/// <summary>
		/// Any routine adjustments that this object needs to make regularly are handled
		/// by this function
		/// </summary>
		/// <param name="CurrTime">Current time in the game</param>
		/// <returns></returns>
		public bool Update(GameTime CurrTime) {
			return true;
		}

		private void RecalculateCoordinates() {
			int nCtr;
			Vector2 vScale, vRotate;

			cvBaseOffset.X = 0;
			cvBaseOffset.Y = 0;

			//Now calculate the position of all the collision vertexes
			cCollisionList.Vertexes.Clear();
			for (nCtr = 0; nCtr < cavBaseVertexList.Count; nCtr++) {
				vScale.X = cavBaseVertexList[nCtr].X - cvBaseOffset.X;
				vScale.Y = cavBaseVertexList[nCtr].Y - cvBaseOffset.Y;

				//With Center as the origin Scale first
				vScale.X *= cvScale.X;
				vScale.Y *= cvScale.Y;

				//With center as origin, handle the rotation next
				vRotate.X = (float)((vScale.X * Math.Cos(cnRotation)) - (vScale.Y * Math.Sin(cnRotation)));
				vRotate.Y = (float)((vScale.X * Math.Sin(cnRotation)) + (vScale.Y * Math.Cos(cnRotation)));

				//Finally apply any movement (put back in the cente offset as well)
				vRotate.X += cvMove.X + cvBaseOffset.X;
				vRotate.Y += cvMove.Y + cvBaseOffset.Y;

				//Add this to the collision list
				cCollisionList.Vertexes.Add(vRotate);
			}
		}
	}
}
