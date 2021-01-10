using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MDLN.MGTools {
	class TextureAtlas {
		private Dictionary<string, Rectangle> cTileSet;
		private Texture2D cTexture;

		public TextureAtlas(GraphicsDevice GraphDev, string strImageFile, string strXMLFile, string strXMLRoot = "/TextureAtlas") {
			XmlDocument TilesXML;
			XmlNodeList TilesList;
			Rectangle NewRect;

			cTileSet = new Dictionary<string, Rectangle>();

			FileStream FileLoad = new FileStream(strImageFile, FileMode.Open);
			cTexture = Texture2D.FromStream(GraphDev, FileLoad);
			FileLoad.Close();

			try {
				TilesXML = new XmlDocument();
				TilesXML.Load(strXMLFile);
			} catch (Exception ExErr) {
				throw new Exception(String.Format("Failed to load XML File {1}{0}Exception {2}{0}Message {3}", Environment.NewLine, strXMLFile, ExErr.GetType().ToString(), ExErr.Message));
			}

			TilesList = TilesXML.DocumentElement.SelectNodes(strXMLRoot + "/SubTexture");
			foreach (XmlNode TileNode in TilesList) {
				NewRect = new Rectangle() { //Default values to supply for omitted attributes
					X = 0,
					Y = 0,
					Width = 0,
					Height = 0,
				};

				if (TileNode.Attributes["x"] != null) {
					if (Int32.TryParse(TileNode.Attributes["x"].InnerText, out NewRect.X) == false) {
						throw new Exception("Attribute 'x' in SubTexture tag had invalid format: " + TileNode.Attributes["x"].InnerText + Environment.NewLine + TileNode.ToString());
					}
				}

				if (TileNode.Attributes["y"] != null) {
					if (Int32.TryParse(TileNode.Attributes["y"].InnerText, out NewRect.Y) == false) {
						throw new Exception("Attribute 'Y' in SubTexture tag had invalid format: " + TileNode.Attributes["y"].InnerText + Environment.NewLine + TileNode.ToString());
					}
				}

				if (TileNode.Attributes["width"] != null) {
					if (Int32.TryParse(TileNode.Attributes["width"].InnerText, out NewRect.Width) == false) {
						throw new Exception("Attribute 'width' in SubTexture tag had invalid format: " + TileNode.Attributes["width"].InnerText + Environment.NewLine + TileNode.ToString());
					}
				}

				if (TileNode.Attributes["height"] != null) {
					if (Int32.TryParse(TileNode.Attributes["height"].InnerText, out NewRect.Height) == false) {
						throw new Exception("Attribute 'height' in SubTexture tag had invalid format: " + TileNode.Attributes["height"].InnerText + Environment.NewLine + TileNode.ToString());
					}
				}

				if ((TileNode.Attributes["name"] == null) || (String.IsNullOrEmpty(TileNode.Attributes["name"].InnerText) == true)) {
					throw new Exception("Attribute 'name' in SubTexture tag is blank or missing" + Environment.NewLine + TileNode.ToString());
				}

				cTileSet.Add(TileNode.Attributes["name"].InnerText, NewRect);
			}

			return;
		}

		public bool ContainsImage(string strTileName) {
			return cTileSet.ContainsKey(strTileName);
		}

		public void GetTileInfo(string strTileName, out Texture2D ImgTexture, out Rectangle ImgRect) {
			if (cTileSet.ContainsKey(strTileName) == false) {
				throw new Exception("Loaded tile set does not contain an entry named: " + strTileName);
			}
			
			ImgTexture = cTexture;
			ImgRect = cTileSet[strTileName];

			return;
		}

		public void DrawTile(string strTileName, SpriteBatch DrawBatch, Rectangle DrawRect, Color TintClr) {
			DrawTile(strTileName, DrawBatch, DrawRect, TintClr, 0);
		}

		public void DrawTile(string strTileName, SpriteBatch DrawBatch, Rectangle DrawRect, Color TintClr, float nRotateRadians) {
			Vector2 vRotateOrigin;
			
			if (cTileSet.ContainsKey(strTileName) == false) {
				throw new Exception("Loaded tile set does not contain an entry named: " + strTileName);
			}

			//Set the origin to the percent across the region
			Vector2 OriginProp = new Vector2(0.5f, 0.5f);

			vRotateOrigin = new Vector2() {
				X = OriginProp.X * cTileSet[strTileName].Width,
				Y = OriginProp.Y * cTileSet[strTileName].Height,
			};

			//Adjust the draw region so that image origin is in the same proportional place
			DrawRect.X += (int)(DrawRect.Width * OriginProp.X);
			DrawRect.Y += (int)(DrawRect.Height * OriginProp.Y);

			//nRotateRadians = (float)(-45 * Math.PI / 180);
			DrawBatch.Draw(cTexture, DrawRect, cTileSet[strTileName], TintClr, nRotateRadians, vRotateOrigin, SpriteEffects.None, 0);
			return;
		}
	}
}
