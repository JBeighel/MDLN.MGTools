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
		private Vector2 cvMouseVertLast;
		private MouseState cPriorMouse;
		private GraphicsDevice cGraphDev;

		public float TextureRotation;
		public float ObjectRotation;
		public string TextureName;

		public Rectangle DrawRegion {
			get {
				return cDrawRegion;
			}

			set {
				Vector2 Move, Center, Scale;

				Move.X = value.X - cDrawRegion.X;
				Move.Y = value.Y - cDrawRegion.Y;

				Center.X = (float)cDrawRegion.X + ((float)cDrawRegion.Width / 2.0f);
				Center.Y = (float)cDrawRegion.Y + ((float)cDrawRegion.Height / 2.0f);

				if ((Move.X != 0) || (Move.Y != 0)) {
					cPolyGon.MoveShape(Move);
				}

				if (cDrawRegion.Width != 0) { //Can't scale from 0 lengths
					Scale.X = (float)value.Width / (float)cDrawRegion.Width;
				} else {
					Scale.X = 1;
				}

				if (cDrawRegion.Height != 0) { //Can't scale from 0 lengths
					Scale.Y = (float)value.Height / (float)cDrawRegion.Height;
				} else {
					Scale.Y = 1;
				}

				if ((Move.X != 1) || (Move.Y != 1)) {
					cPolyGon.ScaleShape(Center, Scale);
				}

				cDrawRegion = value;

				return;
			}
		}

		public PhysicalObject(GraphicsDevice GraphDev, TextureAtlas TextureList) {
			cPolyGon = new ConvexPolygon(GraphDev) {
				LineColor = Color.Blue,
				FillColor = Color.Orange,
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

			cPriorMouse = CurrMouse;

			return true;
		}

		public bool Draw() {
			Rectangle Block;

			SpriteBatch DrawBatch = new SpriteBatch(cGraphDev);
			DrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			cImgAtlas.DrawTile(TextureName, DrawBatch, cDrawRegion, Color.White, ObjectRotation - TextureRotation);
			DrawBatch.End();

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

			return true;
		}

		public void SetCollisionVertexes(IEnumerable<Vector2> VertexList) {
			//Clear out the vertexes
			cPolyGon.RemoveAllVertexes();

			foreach (Vector2 CurrVect in VertexList) {
				cPolyGon.AddVertex(CurrVect);
			}

			return;
		}

		public Vector2 GetCenterCoordinates() {
			Vector2 Center = new Vector2(0, 0);
			int nVertCnt = 0;

			foreach (Vector2 Vertex in cPolyGon.GetVertexes()) {
				Center.X += Vertex.X;
				Center.Y += Vertex.Y;

				nVertCnt += 1;
			}

			Center.X /= nVertCnt;
			Center.Y /= nVertCnt;

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
