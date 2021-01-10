﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MDLN.MGTools;

namespace MDLN.MGTools {
	public delegate void PhysicalObjectEvent(PhysicalObject CurrObj);

	public class PhysicalObject : ICollidable, IVisible {
		/// <summary>
		/// Size of handles used when editing collision vertexes
		/// </summary>
		private const int nHandleWidth = 16;

		/// <summary>
		/// Polygon object used to store collision regions
		/// </summary>
		private ConvexPolygon cPolyGon;
		/// <summary>
		/// Texture atlas holding the texture for this object
		/// </summary>
		private TextureAtlas cImgAtlas;
		/// <summary>
		/// Rectangle holding the region to draw this shape in
		/// </summary>
		private Rectangle crectExtents;
		/// <summary>
		/// Texture used for the handles when editing collision vertexes
		/// </summary>
		private Texture2D cHandleTexture;
		/// <summary>
		/// Vertex that was clicked on and is being moved by the mouse
		/// </summary>
		private int cnMouseVertex;
		/// <summary>
		/// Prior state of the mouse from the last update
		/// </summary>
		private MouseState cPriorMouse;
		/// <summary>
		/// Graphics device being used to render this object
		/// </summary>
		private GraphicsDevice cGraphDev;
		/// <summary>
		/// True if the vertexes are beng displayed and edited, false otherwise
		/// </summary>
		private bool cbVertexEdits;
		/// <summary>
		/// Requested rotation of this object
		/// </summary>
		private float cnRotation;
		/// <summary>
		/// How far to turn to make the texture facing 0 degrees
		/// </summary>
		private float cnTextureRotation;
		/// <summary>
		/// Scale factors for this object
		/// </summary>
		private Vector2 cvScale;

		public event PhysicalObjectEvent Updating;

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
				cPolyGon.RotateShape = cnRotation + cnTextureRotation;

				return;
			}
		}

		public Vector2 CenterPoint {
			get {
				Vector2 vCenter;

				vCenter.X = crectExtents.X + (crectExtents.Width / 2);
				vCenter.Y = crectExtents.Y + (crectExtents.Height / 2);

				return vCenter;
			}

			set {
				//Move the collision polygons
				cPolyGon.CenterCoordinates = value;

				//Update the rendered object
				crectExtents.X = (int)(value.X - (crectExtents.Width / 2));
				crectExtents.Y = (int)(value.Y - (crectExtents.Height / 2));

				return;
			}
		}

		public int Height {
			get {
				return crectExtents.Height;
			}

			set {
				crectExtents.Height = value;
			}
		}

		public int Width {
			get {
				return crectExtents.Width;
			}

			set {
				crectExtents.Width = value;
			}
		}

		public Vector2 Scale {
			get {
				return cvScale;
			}

			set {
				cvScale = value;

				cPolyGon.ScaleShape = cvScale;
			}
		}

		public Vector2 CollisionOffset {
			get {
				return cPolyGon.BaseOffset;
			}

			set {
				cPolyGon.BaseOffset = value;
			}
		}

		public float TextureRotation {
			get {
				return cnTextureRotation;
			}

			set {
				//Remove the old rotation
				cPolyGon.RotateShape -= cnTextureRotation;

				cnTextureRotation = value;

				//Apply the new rotation
				cPolyGon.RotateShape += cnTextureRotation;
			}
		}

		/// <summary>
		/// This is not used by the class, it is only there for external usage
		/// </summary>
		public object Tag;

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

			crectExtents = new Rectangle() { 
				X = 0, 
				Y = 0,
				Width = 0,
				Height = 0,
			};

			cHandleTexture = new Texture2D(GraphDev, 1, 1);
			cHandleTexture.SetData(new[] { Color.Purple });

			return;
		}

		public void SetPosition(Vector2 vCenter, int nHeight, int nWidth, Vector2 vScale, float nRotateRadians) {
			Height = nHeight;
			Width = nWidth;
			cvScale = vScale;
			ObjectRotation = nRotateRadians;
			CenterPoint = vCenter;

			return;
		}

		/// <summary>
		/// Will update the object in order to allow editing of the collision vertexes
		/// Can be replaced in the inheritting class.  It only needs called if the vertex editing is 
		/// needed feature, otherwise it can be skipped.
		/// </summary>
		/// <param name="CurrTime"></param>
		/// <returns>True on success, false to indicate an error</returns>
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

					CenterPoint = CenterPoint;
				}

				if ((CurrMouse.LeftButton == ButtonState.Pressed) && (cPriorMouse.LeftButton == ButtonState.Released)) {
					//User just clicked, see if they got a polygon vertex
					List<CollisionRegion> CollList = new List<CollisionRegion>(cPolyGon.GetCollisionRegions());

					Handle.Width = nHandleWidth;
					Handle.Height = nHandleWidth;

					nVertCtr = 0;
					foreach (Vector2 vCurrVert in CollList[0].Vertexes) {
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

			//Raise the event handler for external logic
			Updating?.Invoke(this);

			return true;
		}

		public bool Draw() {
			Rectangle rectBlock;

			rectBlock = crectExtents;
			rectBlock.Width = (int)(rectBlock.Width * cvScale.X); // Resize the rendered image
			rectBlock.Height = (int)(rectBlock.Height * cvScale.Y);
			rectBlock.X += (crectExtents.Width - rectBlock.Width) / 2; //Keep the image centered on origin
			rectBlock.Y += (crectExtents.Height - rectBlock.Height) / 2;

			//Draw the object
			SpriteBatch DrawBatch = new SpriteBatch(cGraphDev);
			DrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
			cImgAtlas.DrawTile(TextureName, DrawBatch, rectBlock, Color.White, cnTextureRotation + cnRotation);
			DrawBatch.End();

			if (cbVertexEdits == true) {
				cPolyGon.Draw();

				DrawBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
				//Draw handles on each vertex
				List<CollisionRegion> CollList = new List<CollisionRegion>(cPolyGon.GetCollisionRegions());

				foreach (Vector2 CurrVert in CollList[0].Vertexes) {
					rectBlock.X = (int)(CurrVert.X - (nHandleWidth / 2));
					rectBlock.Y = (int)(CurrVert.Y - (nHandleWidth / 2));
					rectBlock.Width = nHandleWidth;
					rectBlock.Height = nHandleWidth;

					DrawBatch.Draw(cHandleTexture, rectBlock, Color.White);
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

		/// <summary>
		/// Returns the coordinates of the center point of this object in its draw area
		/// </summary>
		/// <returns>Center point coordinates</returns>
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

			Center.X = crectExtents.X + (crectExtents.Width / 2);
			Center.Y = crectExtents.Y + (crectExtents.Height / 2);
			return Center;
		}

		/// <summary>
		/// Must be called when the graphics device is changed in some way.
		/// This includes when the viewport is resized
		/// </summary>
		public void UpdateGaphicsDevice(GraphicsDevice NewGraphDev) {
			cGraphDev = NewGraphDev;
			cPolyGon.UpdateGaphicsDevice(NewGraphDev);
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

		/// <summary>
		/// This retreives the list of collision vertexes before any transforms are done.
		/// The coordinates will be before movement, scaling, or rotations.
		/// </summary>
		/// <returns>List of unmodified collision vertexes</returns>
		public IEnumerable<Vector2> GetCollisionVertexes() {
			return cPolyGon.GetVertexes();
		}
	}
}
