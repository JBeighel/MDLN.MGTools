﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MDLN.MGTools;

namespace MDLN.MGTools
{
	public class ObjectManager {
		/// <summary>
		/// This is a list of all object groups
		/// Each list in the dictionary is a collection of every object in that group
		/// </summary>
		Dictionary<Int32, List<PhysicalObject>> cObjGroups;

		/// <summary>
		/// This is a collection of all object definitions that were loaded and are available for use
		/// </summary>
		Dictionary<string, sObjectInfo_t> cObjDefsList;

		/// <summary>
		/// Graphics device used in this application, used when creating new game objects
		/// </summary>
		GraphicsDevice cGraphDev;

		TextureAtlas cImageAtlas;

		public ObjectManager(GraphicsDevice GraphDev, TextureAtlas TileAtlas, string strXMLFile, string strXMLRoot = "/objects") {
			XmlDocument ObjXML;
			XmlNodeList ObjList, VertList;
			sObjectInfo_t NewDef;
			Vector2 vNewVert;

			//Prepare all class variables
			cObjDefsList = new Dictionary<string, sObjectInfo_t>();
			cObjGroups = new Dictionary<int, List<PhysicalObject>>();
			cGraphDev = GraphDev;
			cImageAtlas = TileAtlas;

			//Load object definitions from XML file
			try {
				ObjXML = new XmlDocument();
				ObjXML.Load(strXMLFile);
			} catch (Exception ExErr) {
				throw new Exception(String.Format("Failed to load XML File {1}{0}Exception {2}{0}Message {3}", Environment.NewLine, strXMLFile, ExErr.GetType().ToString(), ExErr.Message));
			}

			ObjList = ObjXML.DocumentElement.SelectNodes(strXMLRoot + "/object");

			foreach (XmlNode ObjNode in ObjList) {
				NewDef = new sObjectInfo_t();
				NewDef.avVertexes = new List<Vector2>();

				//Read all attribute values
				if (ObjNode.Attributes["name"] != null) {
					NewDef.strObjName = ObjNode.Attributes["name"].InnerText;
				} else {
					throw new Exception("Found object tag with no name attribute in file " + strXMLFile);
				}

				if (ObjNode.Attributes["subtexturename"] != null) {
					NewDef.strTextureName= ObjNode.Attributes["subtexturename"].InnerText;
				} else {
					throw new Exception(String.Format("Found object tag named '{0}' with no subtexturename attribute in file {1}", NewDef.strObjName, strXMLFile));
				}

				if (ObjNode.Attributes["texturerotation"] != null) {
					if (float.TryParse(ObjNode.Attributes["texturerotation"].InnerText, out NewDef.nTextureRotate) == false) {
						throw new Exception(String.Format("Inside object tag named {0} attribute 'texturerotation' had invalid format: {1}{2}In file {3}", NewDef.strObjName, ObjNode.Attributes["texturerotation"].InnerText, Environment.NewLine, strXMLFile));
					}

					if (Math.Abs(NewDef.nTextureRotate) > 2 * Math.PI) { //Angle is kinda big, user probably entered degrees
						NewDef.nTextureRotate *= (float)(Math.PI / 180);
					}
				}

				//Read all child node values
				VertList = ObjNode.SelectNodes("vertex");
				foreach (XmlNode VertNode in VertList) {
					vNewVert = new Vector2(0, 0);

					if (VertNode.Attributes["x"] != null) {
						if (float.TryParse(VertNode.Attributes["x"].InnerText, out vNewVert.X) == false) {
							throw new Exception(String.Format("Inside object tag named {0} vertex tag attribute 'x' had invalid format: {1}{2}In file {3}", NewDef.strObjName, VertNode.Attributes["x"].InnerText, Environment.NewLine, strXMLFile));
						}
					}

					if (VertNode.Attributes["y"] != null) {
						if (float.TryParse(VertNode.Attributes["y"].InnerText, out vNewVert.Y) == false) {
							throw new Exception(String.Format("Inside object tag named {0} vertex tag attribute 'y' had invalid format: {1}{2}In file {3}", NewDef.strObjName, VertNode.Attributes["y"].InnerText, Environment.NewLine, strXMLFile));
						}
					}

					NewDef.avVertexes.Add(vNewVert);
				}

				//Object definition loaded, add it to the list
				cObjDefsList.Add(NewDef.strObjName, NewDef);
			}

			return;
		}

		public PhysicalObject SpawnGameObject(string strDefName, Int32 nGroupID) {
			sObjectInfo_t ObjDef;
			PhysicalObject NewObj;

			if (cObjDefsList.ContainsKey(strDefName) == false) {
				throw new Exception("No object definition with name '" + strDefName + "' exists in this manager");
			}

			//Load the object definition
			ObjDef = cObjDefsList[strDefName];

			//Create and initialize the object
			NewObj = new PhysicalObject(cGraphDev, cImageAtlas);

			NewObj.TextureName = ObjDef.strTextureName;
			NewObj.TextureRotation = ObjDef.nTextureRotate;
			NewObj.SetCollisionVertexes(ObjDef.avVertexes);
			
			//Add the object to all lists
			if (cObjGroups.ContainsKey(nGroupID) == false) { //New gorup, add the entry
				cObjGroups.Add(nGroupID, new List<PhysicalObject>());
			}

			cObjGroups[nGroupID].Add(NewObj);

			return NewObj;
		}

		public List<PhysicalObject> this[Int32 nIndex] {
			get {
				return cObjGroups[nIndex];
			}
		}

		public void UpdateGraphicsDevice(GraphicsDevice GraphDev) {
			cGraphDev = GraphDev;

			foreach (KeyValuePair<Int32, List<PhysicalObject>> CurrList in cObjGroups) {
				foreach (PhysicalObject CurrObj in CurrList.Value) {
					CurrObj.UpdateGaphicsDevice(cGraphDev);
				}
			}

			return;
		}

		public void UpdateObjects(GameTime CurrTime) {
			foreach (KeyValuePair<Int32, List<PhysicalObject>> CurrList in cObjGroups) {
				foreach (PhysicalObject CurrObj in CurrList.Value) {
					CurrObj.Update(CurrTime);
				}
			}

			return;
		}

		public void DrawObjects() {
			foreach (KeyValuePair<Int32, List<PhysicalObject>> CurrList in cObjGroups) {
				foreach (PhysicalObject CurrObj in CurrList.Value) {
					CurrObj.Draw();
				}
			}

			return;
		}

		private struct sObjectInfo_t {
			public string strObjName;
			public string strTextureName;
			public float nTextureRotate;
			public List<Vector2> avVertexes;
		}

		private struct sObjectIndexes_t
		{
			/// <summary>
			/// Game object class instance
			/// </summary>
			public PhysicalObject Obj;
			/// <summary>
			/// True if a unique identifer was set, false otherwise
			/// </summary>
			public bool bHasID;
			/// <summary>
			/// Unique identifier assigned
			/// </summary>
			public Int32 nID;
			/// <summary>
			/// List of all groups it was assigned to
			/// </summary>
			public List<Int32> anGroups;
		}
	}
}
