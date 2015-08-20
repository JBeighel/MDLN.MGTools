using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MDLN.Tools {
	/// <summary>
	/// Collection of tools for dealing with files.  Mainly text files but other types may be included
	/// </summary>
	public static class FileTools {
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
	}
}

