using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MDLN.MGTools {
	public class TextureAtlas {
		private GraphicsDevice cgdGraphDevice;

		/// <summary>
		/// This is the texture data that all rendering will use
		/// </summary>
		private Texture2D cTexture;

		/// <summary>
		/// Dictionary contining all sub-image/tiles available
		/// </summary>
		private Dictionary<string, sSubImage_t> cTileSet;

		/// <summary>
		/// When drawing is requested, this is the list of all polygons
		/// that are to be drawn to the screen
		/// </summary>
		private List<sRender_t> caRenderList;

		private BasicEffect cbeShader;

		/// <summary>
		/// Prepare the class instance for use
		/// Load the image file into a texture
		/// Load all sup-image or tile definitions from the XML file
		/// </summary>
		/// <param name="gdDevice">Graphics device that will display images to the screen</param>
		/// <param name="strImageFile">Image data file to load the texture from</param>
		/// <param name="strXMLFile">XML file containing all sub-image and tile definitions</param>
		/// <param name="strXMLRool">XPath to the root tag to find the sub-image definitions in </param>
		public TextureAtlas(GraphicsDevice gdDevice, string strImageFile, string strXMLFile, string strXMLRool = "/TextureAtlas") {
			XmlDocument TilesXML;
			XmlNodeList aNodeList, axChildren;
			sSubImage_t NewSub;
			Vector2 vVertex;
			Rectangle rectNew;

			//Prepare all class variables
			cgdGraphDevice = gdDevice;
			cTileSet = new Dictionary<string, sSubImage_t>();
			caRenderList = new List<sRender_t>();

			//Create a basec shader to use when rendering the polygon textures and tints
			cbeShader = new BasicEffect(gdDevice) {
				TextureEnabled = true,
				VertexColorEnabled = true,
				World = Matrix.CreateOrthographicOffCenter(0, gdDevice.Viewport.Width, gdDevice.Viewport.Height, 0, 0, 1),
			};

			//Load the image data
			FileStream fsLoad = new FileStream(strImageFile, FileMode.Open);
			cTexture = Texture2D.FromStream(gdDevice, fsLoad);
			fsLoad.Close();

			//Load sub-image definitions from XML
			try {
				TilesXML = new XmlDocument();
				TilesXML.Load(strXMLFile);
			} catch (Exception ExErr) {
				throw new Exception(String.Format("Failed to load XML File {1}{0}Exception {2}{0}Message {3}", Environment.NewLine, strXMLFile, ExErr.GetType().ToString(), ExErr.Message));
			}

			//Load all sub-image definitions from the XML file
			aNodeList = TilesXML.DocumentElement.SelectNodes(strXMLRool)[0].ChildNodes;
			foreach (XmlNode xNode in aNodeList) {
				NewSub = new sSubImage_t();
				NewSub.avVertexes = new List<Vector2>();

				if (String.Compare(xNode.Name, "rectangle") == 0) {
					rectNew = new Rectangle();

					//Sub-image boundaries are all listed in attributes
					if (xNode.Attributes["x"] != null) {
						if (Int32.TryParse(xNode.Attributes["x"].InnerText, out rectNew.X) == false) {
							throw new Exception("Attribute 'x' in rectangle tag had invalid format: " + xNode.Attributes["x"].InnerText + Environment.NewLine + xNode.ToString());
						}
					}

					if (xNode.Attributes["y"] != null) {
						if (Int32.TryParse(xNode.Attributes["y"].InnerText, out rectNew.Y) == false) {
							throw new Exception("Attribute 'Y' in rectangle tag had invalid format: " + xNode.Attributes["y"].InnerText + Environment.NewLine + xNode.ToString());
						}
					}

					if (xNode.Attributes["width"] != null) {
						if (Int32.TryParse(xNode.Attributes["width"].InnerText, out rectNew.Width) == false) {
							throw new Exception("Attribute 'width' in rectangle tag had invalid format: " + xNode.Attributes["width"].InnerText + Environment.NewLine + xNode.ToString());
						}
					}

					if (xNode.Attributes["height"] != null) {
						if (Int32.TryParse(xNode.Attributes["height"].InnerText, out rectNew.Height) == false) {
							throw new Exception("Attribute 'height' in rectangle tag had invalid format: " + xNode.Attributes["height"].InnerText + Environment.NewLine + xNode.ToString());
						}
					}

					if ((xNode.Attributes["name"] == null) || (String.IsNullOrEmpty(xNode.Attributes["name"].InnerText) == true)) {
						throw new Exception("Attribute 'name' in rectangle tag is blank or missing" + Environment.NewLine + xNode.ToString());
					}

					//Create all the vertexes for the corners in clockwise order
					vVertex = new Vector2((float)rectNew.X / cTexture.Width, (float)rectNew.Y / cTexture.Height);
					NewSub.avVertexes.Add(vVertex);

					vVertex = new Vector2((float)(rectNew.X + rectNew.Width) / cTexture.Width, (float)rectNew.Y / cTexture.Height);
					NewSub.avVertexes.Add(vVertex);

					vVertex = new Vector2((float)(rectNew.X + rectNew.Width) / cTexture.Width, (float)(rectNew.Y + rectNew.Height) / cTexture.Height);
					NewSub.avVertexes.Add(vVertex);

					vVertex = new Vector2((float)rectNew.X / cTexture.Width, (float)(rectNew.Y + rectNew.Height) / cTexture.Height);
					NewSub.avVertexes.Add(vVertex);

					//Add the sub-image definition to the dictionary
					cTileSet.Add(xNode.Attributes["name"].InnerText, NewSub);
				} else if (String.Compare(xNode.Name, "polygon") == 0) {
					if ((xNode.Attributes["name"] == null) || (String.IsNullOrEmpty(xNode.Attributes["name"].InnerText) == true)) {
						throw new Exception("Attribute 'name' in polygon tag is blank or missing" + Environment.NewLine + xNode.ToString());
					}

					//Sub-image corners are all in vertex child nodes
					axChildren = xNode.SelectNodes("vertex");
					foreach (XmlNode xChild in axChildren) {
						vVertex = new Vector2();

						//Vertex coordinates are all listed in attributes
						if (xChild.Attributes["x"] != null) {
							if (float.TryParse(xChild.Attributes["x"].InnerText, out vVertex.X) == false) {
								throw new Exception("Attribute 'x' in vertex tag had invalid format: " + xNode.Attributes["x"].InnerText + Environment.NewLine + xNode.ToString());
							}
						}

						if (xChild.Attributes["y"] != null) {
							if (float.TryParse(xChild.Attributes["y"].InnerText, out vVertex.Y) == false) {
								throw new Exception("Attribute 'Y' in vertex tag had invalid format: " + xNode.Attributes["y"].InnerText + Environment.NewLine + xNode.ToString());
							}
						}

						//Convert from pixels to percentages
						vVertex.X /= cTexture.Width;
						vVertex.Y /= cTexture.Height;

						//Add the vertext to the list
						NewSub.avVertexes.Add(vVertex);
					}

					//Add the sub-image definition to the dictionary
					cTileSet.Add(xNode.Attributes["name"].InnerText, NewSub);
				}
			}

			return;
		}

		public void EnqueueImageDraw(string strImgName, IEnumerable<Vector2> aScreenCoords) {
			EnqueueImageDraw(strImgName, aScreenCoords, false, Color.White);

			return;
		}

		public void EnqueueImageDraw(string strImgName, IEnumerable<Vector2> aScreenCoords, Color clrTint) {
			EnqueueImageDraw(strImgName, aScreenCoords, true, clrTint);

			return;
		}

		public void Draw() {
			sSubImage_t sImg;
			VertexPositionColorTexture[] aRenderVertexes;
			VertexPositionColor[] aColorVertexes;
			Int32 nCtr, nTriNum;
			RasterizerState rsPriorRaster, rsNewRaster;
			VertexBuffer vtxBuff;

			rsNewRaster = new RasterizerState {
				CullMode = CullMode.None,
			};

			//Save the previous rasterizer settimgs
			rsPriorRaster = cgdGraphDevice.RasterizerState;

			//Set the rasterizer settings needed to draw primitives
			cgdGraphDevice.RasterizerState = rsNewRaster;

			//Give the texture shader the texture to render with
			cbeShader.Texture = cTexture;

			//Loop through all the render objects in the queue
			foreach (sRender_t CurrRender in caRenderList) {
				if (CurrRender.bApplyTexture == true) {
					sImg = cTileSet[CurrRender.strImgName];

					aRenderVertexes = new VertexPositionColorTexture[(CurrRender.avScreenCoords.Count - 2) * 3];

					for (nCtr = 2; nCtr < CurrRender.avScreenCoords.Count; nCtr += 1) {
						nTriNum = (nCtr - 2) * 3; //Calculate which triangle we are setting vertexes for

						//Set the screen coordinate
						aRenderVertexes[nTriNum].Position = new Vector3(CurrRender.avScreenCoords[0].X, CurrRender.avScreenCoords[0].Y, 0);
						aRenderVertexes[nTriNum + 1].Position = new Vector3(CurrRender.avScreenCoords[nCtr].X, CurrRender.avScreenCoords[nCtr].Y, 0);
						aRenderVertexes[nTriNum + 2].Position = new Vector3(CurrRender.avScreenCoords[nCtr - 1].X, CurrRender.avScreenCoords[nCtr - 1].Y, 0);

						//Set the texture coordinate (in percentage)
						aRenderVertexes[nTriNum].TextureCoordinate = sImg.avVertexes[0];
						aRenderVertexes[nTriNum + 1].TextureCoordinate = sImg.avVertexes[nCtr];
						aRenderVertexes[nTriNum + 2].TextureCoordinate = sImg.avVertexes[nCtr - 1];

						if (CurrRender.bApplyTint == true) {
							aRenderVertexes[nTriNum].Color = CurrRender.clrTint;
							aRenderVertexes[nTriNum + 1].Color = CurrRender.clrTint;
							aRenderVertexes[nTriNum + 2].Color = CurrRender.clrTint;
						} else {
							aRenderVertexes[nTriNum].Color = Color.White;
							aRenderVertexes[nTriNum + 1].Color = Color.White;
							aRenderVertexes[nTriNum + 2].Color = Color.White;
						}

					}

					vtxBuff = new VertexBuffer(cgdGraphDevice, typeof(VertexPositionColorTexture), aRenderVertexes.Length, BufferUsage.WriteOnly);
					cgdGraphDevice.SetVertexBuffer(vtxBuff);

					//Make sure all passes of the shder are used
					foreach (EffectPass CurrPass in cbeShader.CurrentTechnique.Passes) {
						CurrPass.Apply();
						cgdGraphDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, aRenderVertexes, 0, aRenderVertexes.Length / 3);
					}
				}
			}

			//All rendering done, reset for next frame
			caRenderList.Clear();

			//Restore rasterizer settings
			cgdGraphDevice.RasterizerState = rsPriorRaster;

			return;
		}

		private void EnqueueImageDraw(string strImgName, IEnumerable<Vector2> avScreenCoords, bool bIncludeTint, Color clrTint) {
			sSubImage_t sReqImg;
			sRender_t NewRender;
			List<Vector2> avCoords = new List<Vector2>(avScreenCoords);

			//Make sure the requested image exists
			if (cTileSet.ContainsKey(strImgName) == false) {
				throw new Exception("Attempt to draw image " + strImgName + " faile, requested image does not exist");
			}

			sReqImg = cTileSet[strImgName];

			//Be sure we have enough screen vertexes
			if (sReqImg.avVertexes.Count != avCoords.Count) {
				throw new Exception("Attempt to draw image " + strImgName + " faile, incorrect number of vertexes given.  Received " + avCoords.Count + " expected " + sReqImg.avVertexes.Count);
			}

			//Looks like a good request, save it in the render list
			NewRender = new sRender_t();
			NewRender.strImgName = strImgName;
			NewRender.bApplyTexture = true;
			NewRender.avScreenCoords = new List<Vector2>(avCoords);
			NewRender.clrTint = clrTint;
			NewRender.bApplyTint = bIncludeTint;

			caRenderList.Add(NewRender);

			return;
		}

		/// <summary>
		/// Structre containing all detials for a sub-image within the texture atlas
		/// </summary>
		struct sSubImage_t {
			/// <summary>
			/// These are the vertexes in the texture file
			/// They are percentages across the texture file, not pixel counts
			/// </summary>
			public List<Vector2> avVertexes;
		}

		/// <summary>
		/// Structure containing the details of a sub-image to draw during the render routine
		/// </summary>
		struct sRender_t {
			/// <summary>
			/// Name of the image definition to texture this object with
			/// </summary>
			public string strImgName;

			/// <summary>
			/// True to apply a texture to this draw, false to not have a texture
			/// </summary>
			public bool bApplyTexture;

			/// <summary>
			/// Screen coordinates for the image vertexes
			/// </summary>
			public List<Vector2> avScreenCoords;

			/// <summary>
			/// True to apply the tint color, false to only apply texture
			/// </summary>
			public bool bApplyTint;

			/// <summary>
			/// Tint color to apply when drawing the image
			/// </summary>
			public Color clrTint;
		}
	}
}
