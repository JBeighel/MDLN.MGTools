using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace MDLN.Tools {
	/// <summary>
	/// Collection of tools for dealing with files.  Mainly text files but other types may be included
	/// </summary>
	public static class FileTools {
		/// <summary>
		/// Structure to hold records loaded from and XML configuration file
		/// </summary>
		public struct XMLFileData {
			/// <summary>
			/// Constructor to populate all member values as well as set the tag name
			/// </summary>
			/// <param name="TagName">THe name of the tag to assign this record</param>
			public XMLFileData(string TagName) {
				Name = TagName;
				Value = "";
				Attributes = new Dictionary<string, string>();
			}

			/// <summary>
			/// The name of the tag this record was loaded from
			/// </summary>
			public string Name;
			/// <summary>
			/// The textual value read from this tag
			/// </summary>
			public string Value;
			/// <summary>
			/// Collection of attributes read from this XML tag.  Key is the attribute name, value is the texual value 
			/// </summary>
			public Dictionary<string, string> Attributes;
		}

		/// <summary>
		/// CFF, Common File Format, files have a text header that includes the file extension followed by the data for that extension.
		/// This function will break the CFF files into multiple files with the same name but the headers specified in the file.  Each 
		/// file will contain the data listed under that extension header.
		/// </summary>
		/// <param name="FileName">Name of the CFF file to work from</param>
		public static void BreakOutCFFFile(string FileName) {
			string Extension, FileNameBase, NewFileName, Line;
			StreamReader FileIn;
			StreamWriter FileOut = null;
			Match rxResult;

			Extension = GetFileExtension(FileName);

			FileNameBase = FileName.Substring(0, FileName.Length - Extension.Length);

			FileIn = new StreamReader(FileName);

			Line = FileIn.ReadLine();
			NewFileName = FileName + ".NEW";
			while (Line != null) {
				rxResult = Regex.Match(Line, "^--- file type: ([A-Z]{3,3})( [A-Z]+)? ---$", RegexOptions.IgnoreCase);

				if (rxResult.Success == true) { //Begin a new file
					Extension = rxResult.Groups[1].Value;

					if (FileOut != null) {
						FileOut.Close();
					}

					FileOut = null;
					NewFileName = FileNameBase + Extension;
				} else { //possible data to put in a file
					if ((FileOut != null) || (Line.Trim().CompareTo("") != 0)) { //Non-blank lines won't be written if we haven't started a file
						if (FileOut == null) {
							FileOut = new StreamWriter(NewFileName, false);
						}

						FileOut.WriteLine(Line);
					}
				}

				Line = FileIn.ReadLine();
			}

			if (FileOut != null) { //Make sure the file is closed
				FileOut.Close();
			}

			FileIn.Close();
		}

		/// <summary>
		/// retrieves the extension of a given file name.  Assuming that the extension is the text that follows the last period in the file name
		/// </summary>
		/// <param name="FileName">Name of the file</param>
		/// <returns>The exptension of the file</returns>
		public static string GetFileExtension(string FileName) {
			Match rxResult;

			rxResult = Regex.Match(FileName, @"\.([^\.]+)$");

			if (rxResult.Success == true) {
				return rxResult.Groups[1].Value;
			} else {
				return "";
			}
		}

		/// <summary>
		/// Loads a text file into an array of strings.  Each string is one line of the file.
		/// </summary>
		/// <param name="strFileName">The path and file name to read</param>
		/// <returns>Each line of the file is an element in the returned string array</returns>
		public static String[] LoadTextFile(string strFileName) {
			string[] astrReturnLines;
			string strCurrLine;
			int iCtr;
			List<string> lstrLines;
			StreamReader srFile;

			srFile = new StreamReader(strFileName);
			lstrLines = new List<String>();

			//Read the file into the string list
			strCurrLine = srFile.ReadLine();
			while (strCurrLine != null) {
				lstrLines.Add(strCurrLine);
				strCurrLine = srFile.ReadLine();
			}

			//Convert string list to string array
			astrReturnLines = new String[lstrLines.Count];
			iCtr = 0;
			foreach (string strOneLine in lstrLines) {
				astrReturnLines[iCtr] = strOneLine;
				iCtr++;
			}

			return astrReturnLines;
		}

		/// <summary>
		/// Load a CSV file into an array of Dictionary objects.
		/// The first line of the CSV is taken to be headers, these values will be used as the indexes/keys to the dictionaries.
		/// Each subsequent line will be placed in a dictionary with the headers as keys
		/// </summary>
		/// <param name="strFileName">The path and file name to read</param>
		/// <returns>The array of dictionaries created when reading the CSV data</returns>
		public static Dictionary<String, String>[] LoadCSVFile(string strFileName) {
			string[] astrTextFile, astrLineValues;
			List<String> lstrHeaders, lstrValues;
			Dictionary<String, String>[] adctRetFile;
			Dictionary<String, String> dctOneLine;
			int iCnt;

			astrTextFile = LoadTextFile(strFileName);
			adctRetFile = new Dictionary<string, string>[astrTextFile.Length - 1];//First line is the header, don't count it

			//Determine the headers
			lstrHeaders = (List<String>)RegEx.SplitOnMatches(astrTextFile[0], "([^,\"]*|[^,\"]*\"[^\"]*\"[^,\"]*)(,|$)");

			//Loop through all lines building the dictionary, first line is headers skip it
			for (int iCtr = 1; iCtr < astrTextFile.Length; iCtr++) {
				dctOneLine = new Dictionary<string, string>();

				lstrValues = (List<String>)RegEx.SplitOnMatches(astrTextFile[iCtr], "([^,\"]*|[^,\"]*\"[^\"]*\"[^,\"]*)(,|$)");

				astrLineValues = lstrValues.ToArray();
				iCnt = 0;
				foreach (string strCurrHeader in lstrHeaders) {
					//Trim white space and trailing commas
					string strCleanHeader = strCurrHeader.Trim();
					if (strCleanHeader.EndsWith(",") == true) {
						strCleanHeader = strCleanHeader.Substring(0, strCleanHeader.Length - 1);
						strCleanHeader = strCleanHeader.Trim();
					}

					astrLineValues[iCnt] = astrLineValues[iCnt].Trim();
					if (astrLineValues[iCnt].EndsWith(",") == true) {
						astrLineValues[iCnt] = astrLineValues[iCnt].Substring(0, astrLineValues[iCnt].Length - 1);
						astrLineValues[iCnt] = astrLineValues[iCnt].Trim();
					}

					dctOneLine.Add(strCleanHeader, astrLineValues[iCnt]);
					iCnt++;
				}

				//Add dictionary to array
				adctRetFile[iCtr - 1] = dctOneLine;
			}

			return adctRetFile;
		}

		/// <summary>
		/// Retrieves a list of all files in a directory that match the specified regular expression
		/// </summary>
		/// <param name="DirPath">Directory path to search</param>
		/// <param name="FileRegex">Regular expression to apply to each file name</param>
		/// <returns>The list of file names that matched the regular expression</returns>
		public static IEnumerable<string> ListFilesInDir(string DirPath, string FileRegex) {
			List<string> FilterFileList = new List<string>();

			string[] FullFileList = Directory.GetFiles(DirPath, "*.*");
			string FileName;

			foreach (string FilePath in FullFileList) {
				FileName = Path.GetFileName(FilePath);

				if (RegEx.QuickTest(FileName, FileRegex) == true) {
					FilterFileList.Add(FileName);
				}
			}

			return FilterFileList;
		}

		/// <summary>
		/// Retrieves the first file in alphabetical order from the directory that matches the regular expression
		/// </summary>
		/// <param name="DirPath">Directory path to search</param>
		/// <param name="FileRegex">Regular expression to apply to each file name</param>
		/// <returns>The file name of the first matching file</returns>
		public static string GetFirstFileInDir(string DirPath, string FileRegex) {
			IEnumerable<string> FileList = ListFilesInDir(DirPath, FileRegex);

			return FileList.OrderBy(file => file).SingleOrDefault();
		}

		/// <summary>
		/// Reads in a binary file and returns its contents as a List of byte values
		/// </summary>
		/// <param name="BinFile">Path and file name of the file to read</param>
		/// <returns>A list containing all of the bytes in the file</returns>
		public static List<byte> LoadBinaryFile(string BinFile) {
			BinaryReader BinRead = new BinaryReader(File.Open(BinFile, FileMode.Open));

			return LoadBinaryFile(BinRead);
		}

		/// <summary>
		/// Reads in a binary file and returns its contents as a List of byte values
		/// </summary>
		/// <param name="BinFile">BinaryReader object with the file opened</param>
		/// <returns>A list containing all of the bytes in the file</returns>
		public static List<byte> LoadBinaryFile(BinaryReader BinFile) {
			byte[] Buffer = new byte[1024];
			List<byte> FileData = new List<byte>();
			int CntRead, Ctr;

			CntRead = BinFile.Read(Buffer, 0, Buffer.Length);
			while (CntRead != 0) {
				for (Ctr = 0; Ctr < CntRead; Ctr++) {
					FileData.Add(Buffer[Ctr]);
				}

				CntRead = BinFile.Read(Buffer, 0, Buffer.Length);
			}

			BinFile.Close();

			return FileData;
		}

		/// <summary>
		/// Attempts to load a configuration XML file.  It will open all tags within the tag specified RootNodePath
		/// and load their values into XMLFileData structures.
		/// </summary>
		/// <param name="FileName">Path and file name of the XML file to load</param>
		/// <param name="RootNodePath">XPath to the root node of the configuration data</param>
		/// <returns>A list of XMLFileData structures, one for each tag found as children to the root tag</returns>
		public static List<XMLFileData> LoadXMLConfig(string FileName, string RootNodePath) {
			List<XMLFileData> Data = new List<XMLFileData>();
			XMLFileData NewTag;
			XmlDocument xdConfig = new XmlDocument();
			XmlNode RootNode;

			xdConfig.Load(FileName);

			RootNode = xdConfig.DocumentElement.SelectSingleNode(RootNodePath);

			if (RootNode == null) {
				return Data;
			}

			foreach (XmlNode Node in RootNode.ChildNodes) {
				NewTag = new XMLFileData();

				NewTag.Name = Node.Name;
				NewTag.Value = Node.InnerText;
				NewTag.Attributes = new Dictionary<string, string>();

				foreach (XmlAttribute Attrib in Node.Attributes) {
					NewTag.Attributes.Add(Attrib.Name, Attrib.InnerText);
				}

				Data.Add(NewTag);
			}

			return Data;
		}

		/// <summary>
		/// Attempts to write configuration tags into an XML file.  It will create or update tags as children to the taf
		/// specified in RootNodePath.
		/// </summary>
		/// <param name="FileName">Path and file name of the XML file to load</param>
		/// <param name="RootNodePath">XPath to the root node of the configuration data</param>
		/// <param name="AttemptUpdates">If true it will attempt to update existing tags, otherwise it will create all tags and new children</param>
		/// <param name="Data">List of configuration data to write to the XML</param>
		/// <returns>True if the XML file is updated successfully, false or exception on error</returns>
		public static bool WriteXMLConfig(string FileName, string RootNodePath, bool AttemptUpdates, IEnumerable<XMLFileData> Data) {
			XmlDocument xdConfig = new XmlDocument();
			XmlNode RootNode, NewNode;
			XmlAttribute NewAttr;

			try {
				xdConfig.Load(FileName);
			} catch (FileNotFoundException) { //If the file doesn't exist, create it
				xdConfig.AppendChild(xdConfig.CreateXmlDeclaration("1.0", "UTF-8", null));
			}

			RootNode = xdConfig.SelectSingleNode(RootNodePath);

			if (RootNode == null) { //Root node doesn't exist, create it
				string[] RootParts = RootNodePath.Split(new char[] { '/' });

				foreach (string TagName in RootParts) {
					if (String.IsNullOrWhiteSpace(TagName) == true) {
						continue;
					}

					RootNode = xdConfig.CreateNode(XmlNodeType.Element, TagName, "");

					if (xdConfig.DocumentElement != null) { //Append to existing document
						xdConfig.DocumentElement.AppendChild(RootNode);
					} else {
						xdConfig.AppendChild(RootNode);
					}

				}
			}

			foreach (XMLFileData NewTag in Data) {
				NewNode = RootNode.SelectSingleNode(NewTag.Name);

				if ((AttemptUpdates == false) && (NewNode != null)) { //OVerwrite existing tags
					RootNode.RemoveChild(NewNode);

					NewNode = null;
				}

				if (NewNode == null) {
					NewNode = xdConfig.CreateElement(NewTag.Name);

					RootNode.AppendChild(NewNode);
				}

				NewNode.InnerText = NewTag.Value;

				if (NewTag.Attributes != null) {
					foreach (string AttrName in NewTag.Attributes.Keys) {
						if (NewNode.Attributes[AttrName] == null) {
							NewAttr = xdConfig.CreateAttribute(AttrName);

							NewNode.Attributes.Append(NewAttr);
						} else {
							NewAttr = NewNode.Attributes[AttrName];
						}

						NewAttr.InnerText = NewTag.Attributes[AttrName];
					}
				}
			}

			xdConfig.Save(FileName);

			return true;
		}

		/// <summary>
		/// Attemps to create a basic XML file to hold configuration data.  It will try to build the tag structure
		/// needed to reach the RootNodePath.  It can only handle simple tag names, if the base path is not 
		/// /name/name/name format then it will not be able to create the file.
		/// </summary>
		/// <param name="FileName">Path and name of the XML file to create</param>
		/// <param name="RootNodePath">Simple XPath describing the tag path to create</param>
		/// <param name="DeleteExisting">True to create a new file, false to edit existing file</param>
		static public void CreateBasicXMLConfig(string FileName, string RootNodePath, bool DeleteExisting) {
			XmlDocument xdConfig = new XmlDocument();
			XmlNode RootNode, NextNode;
			string[] TagList;
			string CurrPath;

			TagList = RootNodePath.Trim(new char[] { '/' }).Split('/');

			if ((File.Exists(FileName) == true) && (DeleteExisting == false)) { //File exists and we can't overwrite it
				xdConfig.Load(FileName);
			} else if (DeleteExisting == true) {
				File.Delete(FileName);
			}

			if (xdConfig.DocumentElement == null) {
				xdConfig.AppendChild(xdConfig.CreateElement(TagList[0]));
			}

			RootNode = xdConfig.DocumentElement;
			CurrPath = "";
			foreach (string TagName in TagList) {
				CurrPath += "/TagName";

				NextNode = RootNode.SelectSingleNode("/" + TagName);

				if (NextNode == null) {
					NextNode = xdConfig.CreateElement(TagName);

					RootNode.AppendChild(NextNode);
				}

				RootNode = NextNode; //Keep descending into the path
			}

			xdConfig.Save(FileName);
		}

		/// <summary>
		/// Retrieves a list of all sub-directories in a directory that match the specified regular expression
		/// </summary>
		/// <param name="DirPath">Directory path to search</param>
		/// <param name="DirRegEx">Regular expression to apply to each directory name</param>
		/// <returns>The list of directory names that matched the regular expression</returns>
		static public IEnumerable<string> ListDirsInDir(string DirPath, string DirRegEx) {
			List<string> FilterDirList = new List<string>();

			string[] FullFileList = Directory.GetDirectories(DirPath);
			string FileName;

			foreach (string FilePath in FullFileList) {
				FileName = Path.GetFileName(FilePath);

				if (RegEx.QuickTest(FileName, DirRegEx) == true) {
					FilterDirList.Add(FileName);
				}
			}

			return FilterDirList;
		}
	}

	/// <summary>
	/// Class to add entries to a log file.  It will filter them based on a set of log levels so that
	/// only the requested amount of detail is written to the file.
	/// </summary>
	public class MessageLog {
		private string cLogFile;

		/// <summary>
		/// SPecifies the detail level that will be saved in the log.  Entries that are not at least this level
		/// will be discarded.
		/// </summary>
		public LogLevels LogLevel;

		/// <summary>
		/// Specifies if log entries should include the log level in the text.  If false the log level will be omitted.
		/// </summary>
		public bool IncludeLogLevel;

		/// <summary>
		/// Constructor that prepares the class for use.
		/// </summary>
		/// <param name="LogFile">Path and file name of the file to use for the log</param>
		/// <param name="AppendEntries">True to preserve the logs contents, false to delete the log and start clean</param>
		public MessageLog(string LogFile, bool AppendEntries) {
			LogLevel = LogLevels.Information;
			IncludeLogLevel = true;

			cLogFile = LogFile;

			if ((File.Exists(cLogFile) == true) && (AppendEntries == false)) {
				//Erase the log file  so that this run starts clean
				using (StreamWriter FileWriter = new StreamWriter(cLogFile, false)) {
					FileWriter.Write("");
				}
			}
		}

		/// <summary>
		/// Add a new message into the log file
		/// </summary>
		/// <param name="EntryLevel">Detail level of this entry</param>
		/// <param name="LogMessage">Text to include in the message</param>
		public void AddLogMessage(LogLevels EntryLevel, string LogMessage) {
			string FullEntry, Header;

			if ((int)EntryLevel < (int)LogLevel) { //does not exceed configured log level, discard
				return;
			}

			if (IncludeLogLevel == true) {
				Header = String.Format("{0}-{1}", TypeTools.DateTimeToStandardString(DateTime.Now), EntryLevel);
			} else {
				Header = String.Format("{0}", TypeTools.DateTimeToStandardString(DateTime.Now));
			}

			FullEntry = String.Format("{0}: {1}", Header, LogMessage) + Environment.NewLine;

			using (StreamWriter LogFile = new StreamWriter(cLogFile, true)) {
				LogFile.Write(FullEntry);
			}
		}
	}

	/// <summary>
	/// Enumeration of priority levels for log entries
	/// </summary>
	public enum LogLevels {
		/// <summary>
		/// Debug messages used in development diagnostics
		/// </summary>
		Debug = 0,
		/// <summary>
		/// Information messages used for verbose messaging
		/// </summary>
		Information = 1,
		/// <summary>
		/// Entries listing specific events
		/// </summary>
		Event = 2,
		/// <summary>
		/// Entries listing warnings or abnormal conditions that were recovered from
		/// </summary>
		Warning = 3,
		/// <summary>
		/// Failures in the application that could not be recovered from
		/// </summary>
		Error = 4
	}
}

