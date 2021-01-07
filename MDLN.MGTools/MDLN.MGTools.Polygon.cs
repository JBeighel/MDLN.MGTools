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
	public class ConvexPolygon : ICollidable
	{
		private CollisionRegion cCollisionList;
		private Color cLineClr;
		private Color cFillClr;
		private Texture2D cLineTexture;
		private readonly GraphicsDevice cGraphDev;
		private readonly BasicEffect cBasicShader;

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

		/// <summary>
		/// Constructor for the class
		/// </summary>
		/// <param name="GraphDev">Graphics Device this object should use to draw though</param>
		public ConvexPolygon(GraphicsDevice GraphDev) {
			//Setup all class variables
			FillShape = true;
			DrawOutline = true;
			LineWidth = 1;

			cGraphDev = GraphDev;
			cLineClr = new Color(0, 0, 0, 0); //Black and fully transparent
			cFillClr = new Color(0, 0, 0, 0); //Black and fully transparent
			cCollisionList = new CollisionRegion {
				Type = CollideType.ConvexPolygon,
				Vertexes = new List<Vector2>()
			};

			//Create a basec shader to use when rendering the polygon
			cBasicShader = new BasicEffect(GraphDev) {
				TextureEnabled = true,
				World = Matrix.CreateOrthographicOffCenter(0, cGraphDev.Viewport.Width, cGraphDev.Viewport.Height, 0, 0, 1)
			};

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
		/// Add another vertex to the polygon.  These vertexes will not be sorted, so the order they 
		/// are added must create a convex shape.
		/// </summary>
		/// <param name="NewVert">The coordinates of the new vertex</param>
		/// <returns>True if the vertex was added</returns>
		public bool AddVertex(Vector2 NewVert) {
			cCollisionList.Vertexes.Add(NewVert);

			return true;
		}

		/// <summary>
		/// Change the coordinates of an existing vertex
		/// </summary>
		/// <param name="nIdx">Index (order added) of the vertex</param>
		/// <param name="Vert">New coordinates for the vertex</param>
		/// <returns>True if the vertex was updated</returns>
		public bool UpdateVertex(int nIdx, Vector2 Vert) {
			if (nIdx >= cCollisionList.Vertexes.Count) { //Vertex does not exist
				return false;
			}

			//Update the array contents
			cCollisionList.Vertexes[nIdx] = Vert;

			return true;
		}

		/// <summary>
		/// Moves all of the vertexes of the shape by some X and/or Y distance
		/// </summary>
		/// <param name="Move">Distance to move the shape in X and Y directions</param>
		/// <returns>True if the shape was correctly repositioned</returns>
		public bool MoveShape(Vector2 Move) {
			int nCtr;
			Vector2 Vert;

			for (nCtr = 0; nCtr < cCollisionList.Vertexes.Count; nCtr++) {
				//Get the vertex data
				Vert = cCollisionList.Vertexes[nCtr];

				//Move it the horizontal and vertical distance
				Vert.X += Move.X;
				Vert.Y += Move.Y;

				//Update the vertex lest
				cCollisionList.Vertexes[nCtr] = Vert;
			}

			return true;
		}

		/// <summary>
		/// Retrieve a list of all vertexes for this shape
		/// </summary>
		/// <returns>A collection of all vertexes in this shape</returns>
		public IEnumerable<Vector2> GetVertexes() {
			return cCollisionList.Vertexes;
		}

		/// <summary>
		/// Call to render this shape through the specified graphics device
		/// </summary>
		/// <param name="DrawBatch">Specify a SpriteBatch to use to render the perimeter</param>
		/// <returns>True if the shape was drawn successfully</returns>
		public bool Draw(SpriteBatch DrawBatch) {
			Rectangle LineRect;
			int nCtr, nPrevVert, nSurfNum;
			Vector2 LineFromOrigin, RotOrigin;
			Vector LineSeg = new Vector();
			RasterizerState PriorRaster, NewRaster;

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

			if (FillShape == true) {
				//Draw the triangle fill (triangles needed is Vertexes -2, then 3 vertexes per triangle)
				VertexPositionColor[] aVertexes = new VertexPositionColor[(cCollisionList.Vertexes.Count - 2) * 3];

				for (nCtr = 2; nCtr < cCollisionList.Vertexes.Count; nCtr++) {
					nSurfNum = (nCtr - 2) * 3; //Which surface/triangle are we filling in

					//Every triangle gets 3 vertexes in the list, none are shared in a TriangleList
					//Always use index zero as a common point
					aVertexes[nSurfNum] = new VertexPositionColor(new Vector3(cCollisionList.Vertexes[0].X, cCollisionList.Vertexes[0].Y, 0), cFillClr);

					//The other vertexes are pairs of the remaining vertexes
					aVertexes[nSurfNum + 1] = new VertexPositionColor(new Vector3(cCollisionList.Vertexes[nCtr - 1].X, cCollisionList.Vertexes[nCtr - 1].Y, 0), cFillClr);
					aVertexes[nSurfNum + 2] = new VertexPositionColor(new Vector3(cCollisionList.Vertexes[nCtr].X, cCollisionList.Vertexes[nCtr].Y, 0), cFillClr);
				}

				//Save off the current rasterizer, then make sure all primitives are drawn
				PriorRaster = cGraphDev.RasterizerState;
				NewRaster = new RasterizerState();
				NewRaster.CullMode = CullMode.None;
				cGraphDev.RasterizerState = NewRaster;

				//Make sure all passes of the effects/shader are being used
				foreach (EffectPass CurrShadderPass in cBasicShader.CurrentTechnique.Passes) {
					//This is the all-important line that sets the effect, and all of its settings, on the graphics device
					CurrShadderPass.Apply();
					//cBasicShader.CurrentTechnique.Passes[0].Apply();

					cGraphDev.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, aVertexes, 0, aVertexes.Length / 3);
				}

				//Restore the rasterizer
				cGraphDev.RasterizerState = PriorRaster;
			}

			return true;
		}
	}
}
