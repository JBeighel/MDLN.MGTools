using System;
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
		private Dictionary<Int32, List<PhysicalObject>> cObjGroups;

		/// <summary>
		/// This is a collection of all object definitions that were loaded and are available for use
		/// </summary>
		private Dictionary<string, sObjectInfo_t> cObjDefsList;

		/// <summary>
		/// Graphics device used in this application, used when creating new game objects
		/// </summary>
		private GraphicsDevice cGraphDev;

		/// <summary>
		/// Texture atlas holding the images for the objects managed by this class
		/// </summary>
		private readonly TextureAtlas cImageAtlas;

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

				if (ObjNode.Attributes["width"] != null) {
					if (Int32.TryParse(ObjNode.Attributes["width"].InnerText, out NewDef.nWidth) == false) {
						throw new Exception(String.Format("Inside object tag named {0} attribute 'width' had invalid format: {1}{2}In file {3}", NewDef.strObjName, ObjNode.Attributes["width"].InnerText, Environment.NewLine, strXMLFile));
					}
				}

				if (ObjNode.Attributes["height"] != null) {
					if (Int32.TryParse(ObjNode.Attributes["height"].InnerText, out NewDef.nHeight) == false) {
						throw new Exception(String.Format("Inside object tag named {0} attribute 'height' had invalid format: {1}{2}In file {3}", NewDef.strObjName, ObjNode.Attributes["height"].InnerText, Environment.NewLine, strXMLFile));
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

		/// <summary>
		/// Creates a new instance of PhysicalObject, applies the specified object definition,
		/// ands adds it to the suggested group to be managed by this class
		/// </summary>
		/// <param name="strDefName">Object definition name</param>
		/// <param name="nGroupID">Group to add this object to</param>
		/// <returns></returns>
		public PhysicalObject SpawnGameObject(string strDefName, Int32 nGroupID) {
			PhysicalObject NewObj;

			if (cObjDefsList.ContainsKey(strDefName) == false) {
				throw new Exception("No object definition with name '" + strDefName + "' exists in this manager");
			}

			//Create and initialize the object
			NewObj = new PhysicalObject(cGraphDev, cImageAtlas);

			ImportGameObject(NewObj, strDefName, nGroupID);

			return NewObj;
		}

		/// <summary>
		/// Takes an externally created PhysicalObject applies an object definition to it, then 
		/// adds it to the group to be managed by this class.
		/// </summary>
		/// <param name="NewObj">Object to define and import</param>
		/// <param name="strDefName">Object definition to apply</param>
		/// <param name="nGroupID">Group to add this object to</param>
		public void ImportGameObject(PhysicalObject NewObj,  string strDefName, Int32 nGroupID) {
			ApplyDefinition(NewObj, strDefName);

			//Add the object to all lists
			if (cObjGroups.ContainsKey(nGroupID) == false) { //New gorup, add the entry
				cObjGroups.Add(nGroupID, new List<PhysicalObject>());
			}

			cObjGroups[nGroupID].Add(NewObj);

			return;
		}

		/// <summary>
		/// Applies an object definition to an externally created PhysicalObject only
		/// The object is not imported to a group and will not be managed by this class
		/// </summary>
		/// <param name="Obj">Object to apply a definition to</param>
		/// <param name="strDefName">Object definition name</param>
		public void ApplyDefinition(PhysicalObject Obj, string strDefName) {
			sObjectInfo_t ObjDef;
			
			if (cObjDefsList.ContainsKey(strDefName) == false) {
				throw new Exception("No object definition with name '" + strDefName + "' exists in this manager");
			}

			//Load the object definition
			ObjDef = cObjDefsList[strDefName];

			Obj.Width = ObjDef.nWidth;
			Obj.Height = ObjDef.nHeight;
			Obj.TextureName = ObjDef.strTextureName;
			Obj.TextureRotation = ObjDef.nTextureRotate;
			Obj.SetCollisionVertexes(ObjDef.avVertexes);

			return;
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
			List<Int32> aIdxToRemove = new List<int>();
			List<PhysicalObject> CurrList;
			List<Int32> aIdxList;
			PhysicalObject CurrObj;
			Int32 nCtr;

			//Get the keys in a separatex collection so updates to key list won't cause problems
			aIdxList = new List<int>(cObjGroups.Keys);

			//Loop through all of hte lists calling for updates
			foreach (Int32 CurrKey in aIdxList) { 
				CurrList = cObjGroups[CurrKey];
				aIdxToRemove.Clear();

				//Loop through all objects in this list
				for (nCtr = 0; nCtr < CurrList.Count; nCtr++) {
					CurrObj = CurrList[nCtr];
					if (CurrObj.Update(CurrTime) == false) {
						aIdxToRemove.Add(nCtr); //Save this index to be purged later
					}
				}

				//Any Updates that return false must be removed,go backwards to avoid removed keys 
				//messing up the indexing
				for (nCtr = aIdxToRemove.Count - 1; nCtr >= 0; nCtr--) { 
					CurrList.RemoveAt(aIdxToRemove[nCtr]);
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

		/// <summary>
		/// Definition structure for objects that can be created
		/// </summary>
		private struct sObjectInfo_t {
			/// <summary>
			/// Identifying name of the object
			/// </summary>
			public string strObjName;
			/// <summary>
			/// Name of the texture in the atlas
			/// </summary>
			public string strTextureName;
			/// <summary>
			/// Radians the texture needs rotated to be a 0 degrees on screen
			/// </summary>
			public float nTextureRotate;
			/// <summary>
			/// List of collision vertexes for this object
			/// </summary>
			public List<Vector2> avVertexes;
			public Int32 nHeight;
			public Int32 nWidth;
		}
	}
}
