using MDLN.Tools;
using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	class Polygon : ICollidable {
		private CollisionRegion cCollisionList;
		private Color cBackClr;
		private Texture2D cTexture;
		private GraphicsDevice cGraphDev;

		public bool DrawOutlineOnly;
		public int LineWidth;

		public Color BackgroundColor {
			get {
				return cBackClr;
			}

			set {
				//Save the color
				cBackClr = value;

				//Set it as the texture
				cTexture = new Texture2D(cGraphDev, 1, 1);
				cTexture.SetData(new[] { value });
			}
		}

		public Polygon(GraphicsDevice GraphDev) {
			//Setup all class variables
			DrawOutlineOnly = true;
			LineWidth = 1;

			cGraphDev = GraphDev;
			cBackClr = new Color(0, 0, 0, 0); //Black and fully transparent
			cCollisionList = new CollisionRegion();

			cCollisionList.Type = CollideType.ConvexPolygon;
			cCollisionList.Vertexes = new List<Vector2>();

			return;
		}
		
		public IEnumerable<CollisionRegion> GetCollisionRegions() {
			return new CollisionRegion[1] { cCollisionList };
		}
		public bool TestCollision(IEnumerable<CollisionRegion> TestRegions) {
			return false;
		}

		public bool TestCollision(ICollidable TestObj) {
			return false;
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

		public IEnumerable<Vector2>GetVertexes() {
			return cCollisionList.Vertexes;
		}

		public bool Draw(SpriteBatch DrawBatch) {
			Rectangle LineRect;
			int nCtr, nPrevVert;
			Vector2 LineFromOrigin, RotOrigin;
			Vector LineSeg = new Vector();

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
				DrawBatch.Draw(cTexture, LineRect, cTexture.Bounds, Color.White, (float)(LineSeg.Polar.Angle * Math.PI / 180) , RotOrigin, SpriteEffects.None, 0);
			}

			return true;
		}
	}
}
