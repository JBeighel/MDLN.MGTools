using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MDLN.MGTools;

namespace MDLN.MGTools {
	class PhysicalObject : ICollidable, IVisible {
		private const int nHandleWidth = 16;

		private ConvexPolygon cPolyGon;
		private TextureAtlas cImgAtlas;
		private Rectangle cDrawRegion;
		private Texture2D cHandleTexture;
		private int cnMouseVertex;
		private MouseState cPriorMouse;
		private GraphicsDevice cGraphDev;
		private bool cbVertexEdits;
		private float cnRotation;

		public float TextureRotation;
		public string TextureName;
		public bool AllowCollisionVertexEdits {
			get {
				return cbVertexEdits;
			}

			set {
				cbVertexEdits = value;

				cPolyGon.FillShape = cbVertexEdits;
				cPolyGon.DrawOutline = cbVertexEdits;
			}
		}

		public float ObjectRotation { 
			get {
				return cnRotation;
			}

			set {
				cnRotation = value;
				cPolyGon.RotateShape(cnRotation);

				return;
			}
		}


		public Vector2 CenterPoint {
			get {
				Vector2 vCenter;

				vCenter.X = cDrawRegion.X + (cDrawRegion.Width / 2);
				vCenter.Y = cDrawRegion.Y + (cDrawRegion.Height / 2);

				return vCenter;
			}

			set {
				Vector2 vPolyCenter;
				//Find the polygon center point
				vPolyCenter = cPolyGon.GetCenterCoordinates();

				//Find the difference to the new point
				vPolyCenter.X = value.X - vPolyCenter.X;
				vPolyCenter.Y = value.Y - vPolyCenter.Y;

				//Adjust for the center offset the draw region gets
				vPolyCenter.X += cDrawRegion.Width / 2;
				vPolyCenter.Y += cDrawRegion.Height / 2;

				//Move the collision polygons
				cPolyGon.MoveShape(vPolyCenter);

				//Update the rendered object
				cDrawRegion.X = (int)(value.X - (cDrawRegion.Width / 2));
				cDrawRegion.Y = (int)(value.Y - (cDrawRegion.Height / 2));

				return;
			}
		}

		public int Height {
			get {
				return cDrawRegion.Height;
			}

			set {
				cDrawRegion.Height = value;
			}
		}

		public int Width {
			get {
				return cDrawRegion.Width;
			}

			set {
				cDrawRegion.Width = value;
			}
		}

		public PhysicalObject(GraphicsDevice GraphDev, TextureAtlas TextureList) {
			cPolyGon = new ConvexPolygon(GraphDev) {
				LineColor = Color.Blue,
				FillColor = new Color(Color.Orange, 128),
				DrawOutline = true,
				FillShape = true,
			};

			cImgAtlas = TextureList;
			cGraphDev = GraphDev;

			TextureName = "";

			ObjectRotation = 0;
			TextureRotation = 0;

			cDrawRegion = new Rectangle() { 
				X = 0, 
				Y = 0,
				Width = 0,
				Height = 0,
			};

			cHandleTexture = new Texture2D(GraphDev, 1, 1);
			cHandleTexture.SetData(new[] { Color.Purple });

			return;
		}

		public bool Update(GameTime CurrTime) {
			Rectangle Handle;
			int nVertCtr;
			Vector2 vVertPos;
			MouseState CurrMouse = Mouse.GetState();

			if (cbVertexEdits == true) {
				if ((CurrMouse.LeftButton == ButtonState.Pressed) && (cnMouseVertex >= 0)) {
					//User is draging a vertex around
					vVertPos.X = CurrMouse.X;
					vVertPos.Y = CurrMouse.Y;

					cPolyGon.UpdateVertex(cnMouseVertex, vVertPos);
				}

				if ((CurrMouse.LeftButton == ButtonState.Pressed) && (cPriorMouse.LeftButton == ButtonState.Released)) {
					//User just clicked, see if they got a polygon vertex
					Handle.Width = nHandleWidth;
					Handle.Height = nHandleWidth;

					nVertCtr = 0;
					foreach (Vector2 vCurrVert in cPolyGon.GetVertexes()) {
						Handle.X = (int)(vCurrVert.X - (nHandleWidth / 2));
						Handle.Y = (int)(vCurrVert.Y - (nHandleWidth / 2));

						if (MGMath.IsPointInRect(CurrMouse.Position, Handle) == true) {
							cnMouseVertex = nVertCtr;

							break;
						}

						nVertCtr += 1;
					}
				}

				if (CurrMouse.LeftButton == ButtonState.Released) {
					cnMouseVertex = -1;
				}
			}

			cPriorMouse = CurrMouse;

			return true;
		}

		public bool Draw() {
			Rectangle Block;

			//Draw the object
			SpriteBatch DrawBatch = new SpriteBatch(cGraphDev);
			DrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			cImgAtlas.DrawTile(TextureName, DrawBatch, cDrawRegion, Color.White, ObjectRotation - TextureRotation);
			DrawBatch.End();

			if (cbVertexEdits == true) {
				cPolyGon.Draw();

				DrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
				//Draw handles on each vertex
				foreach (Vector2 CurrVert in cPolyGon.GetVertexes()) {
					Block.X = (int)(CurrVert.X - (nHandleWidth / 2));
					Block.Y = (int)(CurrVert.Y - (nHandleWidth / 2));
					Block.Width = nHandleWidth;
					Block.Height = nHandleWidth;

					DrawBatch.Draw(cHandleTexture, Block, Color.White);
				}
				DrawBatch.End();
			}

			return true;
		}

		public void SetCollisionVertexes(IEnumerable<Vector2> VertexList) {
			//Clear out the vertexes
			cPolyGon.SetVertexes(VertexList);

			return;
		}

		public Vector2 GetCenterCoordinates() {
			Vector2 Center = new Vector2(0, 0);
			/*int nVertCnt = 0;

			foreach (Vector2 Vertex in cPolyGon.GetVertexes()) {
				Center.X += Vertex.X;
				Center.Y += Vertex.Y;

				nVertCnt += 1;
			}

			Center.X /= nVertCnt;
			Center.Y /= nVertCnt;
			*/

			Center.X = cDrawRegion.X + (cDrawRegion.Width / 2);
			Center.Y = cDrawRegion.Y + (cDrawRegion.Height / 2);
			return Center;
		}

		public IEnumerable<CollisionRegion> GetCollisionRegions() {
			return cPolyGon.GetCollisionRegions();
		}

		public bool TestCollision(ICollidable TestObj) {
			return cPolyGon.TestCollision(TestObj);
		}

		public bool TestCollision(IEnumerable<CollisionRegion> aRegions) {
			return cPolyGon.TestCollision(aRegions);
		}
	}
}
