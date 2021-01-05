using MDLN.Tools;
using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	public class Polygon : ICollidable
	{
		private CollisionRegion cCollisionList;
		private Color cLineClr;
		private Color cFillClr;
		private Texture2D cLineTexture;
		private Texture2D cFillTexture;
		private GraphicsDevice cGraphDev;
		private BasicEffect cBasicShader;

		public bool DrawOutline;
		public bool FillShape;
		public int LineWidth;

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

		public Polygon(GraphicsDevice GraphDev) {
			//Setup all class variables
			FillShape = true;
			DrawOutline = true;
			LineWidth = 1;

			cGraphDev = GraphDev;
			cLineClr = new Color(0, 0, 0, 0); //Black and fully transparent
			cFillClr = new Color(0, 0, 0, 0); //Black and fully transparent
			cCollisionList = new CollisionRegion();

			cCollisionList.Type = CollideType.ConvexPolygon;
			cCollisionList.Vertexes = new List<Vector2>();

			//Create a basec shader to use when rendering the polygon
			cBasicShader = new BasicEffect(GraphDev);

			cBasicShader.TextureEnabled = true;
			cBasicShader.World = Matrix.CreateOrthographicOffCenter(0, cGraphDev.Viewport.Width, cGraphDev.Viewport.Height, 0, 0, 1);

			return;
		}

		public IEnumerable<CollisionRegion> GetCollisionRegions() {
			return new CollisionRegion[1] { cCollisionList };
		}

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

		public bool TestCollision(ICollidable TestObj) {
			return TestCollision(TestObj.GetCollisionRegions());
		}

		public bool AddVertex(Vector2 NewVert) {
			cCollisionList.Vertexes.Add(NewVert);

			return true;
		}

		public bool UpdateVertex(int nIdx, Vector2 Vert) {
			if (nIdx >= cCollisionList.Vertexes.Count) { //Vertex does not exist
				return false;
			}

			//Update the array contents
			cCollisionList.Vertexes[nIdx] = Vert;

			return true;
		}

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

		public IEnumerable<Vector2> GetVertexes() {
			return cCollisionList.Vertexes;
		}

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
