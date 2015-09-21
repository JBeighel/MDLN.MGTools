using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MDLN.Tools {
	/// <summary>
	/// Collection of tools for doing regular expressions quickly and with different options
	/// </summary>
	public static class RegEx {
		/// <summary>
		/// Does a case insensitive regular expression match.  Returns true if the text matches the regular expression, false otherwise.
		/// </summary>
		/// <param name="strText">The text to check</param>
		/// <param name="strRegEx">The regular expression to test with</param>
		/// <returns></returns>
		public static bool QuickTest(string strText, string strRegEx) {
			Match rxResult;

			rxResult = Regex.Match(strText, strRegEx, RegexOptions.IgnoreCase);

			return rxResult.Success;
		}

		/// <summary>
		/// Does a regular expression match, but looses the regular expression.  It is case insensitive and spaces are changed to all any number of tabs 
		/// or spaces to allow for extended white space.
		/// </summary>
		/// <param name="strString">The string to check</param>
		/// <param name="strRegEx">The regular expression to use</param>
		/// <returns>True if the loosened regular expresion matches, false otherwise.</returns>
		public static bool LooseTest(string strString, string strRegEx) {
			Match rxmResult;

			strRegEx = ReplaceSpaces(strRegEx, @"\s");
			strString = strString.Trim();

			rxmResult = Regex.Match(strString, strRegEx, RegexOptions.IgnoreCase);

			return rxmResult.Success;
		}

		/// <summary>
		/// Retrives the value matched by a particular regular expression group
		/// </summary>
		/// <returns>The text matched by reg ex group, or null if the group doesn't exist or the regex doesn't match.</returns>
		/// <param name="strText">String text.</param>
		/// <param name="strRegEx">String reg ex.</param>
		/// <param name="iGroupNum">The group number, 0 is the entire regular expression 1 and up is group identified by parenthesis.</param>
		public static string GetRegExGroup(string strText, string strRegEx, int iGroupNum) {
			Match rxResult;

			rxResult = Regex.Match(strText, strRegEx, RegexOptions.IgnoreCase);

			if ((rxResult.Success == true) && (rxResult.Groups.Count > iGroupNum)) {
				return rxResult.Groups[iGroupNum].Value;
			} else {
				return null;
			}
		}

		/// <summary>
		/// Replaces all the spaces in a regular expression with the specified text.  
		/// Attempts to avoid replacing spaces in bracket expressions as this may cause invalid regular expressions
		/// </summary>
		/// <param name="strRegEx">The regular expression to operate on</param>
		/// <param name="strReplace">The text to replace spaces with</param>
		/// <returns>The string with all spaces replaced with the specified text</returns>
		public static string ReplaceSpaces(string strRegEx, string strReplace) {
			Match rxmBracket, rxmSpace;
			string strRXNew;

			strRXNew = "";

			while (strRegEx != "") {
				rxmBracket = Regex.Match(strRegEx, "(^|[^\\\\ ])\\[");
				rxmSpace = Regex.Match(strRegEx, "[ \t]+");

				if (rxmSpace.Success == false) {
					//All replaces are done
					strRXNew = strRXNew + strRegEx;
					strRegEx = "";
				} else if ((rxmBracket.Success == false) || (rxmBracket.Index > rxmSpace.Index)) {
					//A space is the next interesting character
					strRXNew = strRXNew + strRegEx.Substring(0, rxmSpace.Index) + strReplace;
					strRegEx = strRegEx.Substring(rxmSpace.Index + rxmSpace.Length);
				} else { //Bracket expression is next
					rxmBracket = Regex.Match(strRegEx, "[^\\\\]\\]"); //Find end of bracket expression
					strRXNew = strRXNew + strRegEx.Substring(0, rxmBracket.Index + 2);
					strRegEx = strRegEx.Substring(rxmBracket.Index + rxmBracket.Length);
				}
			}

			return strRXNew;
		}

		/// <summary>
		/// Splits teh string based on regular expressions.  Each matched instance of the regular expression will be treated as a new element in the string.
		/// Unlike the normal split function this one will not remove the separator from the string, in fact there is no separator used.  Each chunk pulled out must
		/// be a match to the regular expression provided.
		/// </summary>
		/// <param name="strString">The string to work on</param>
		/// <param name="strRegEx">The regular expression to compare to the string</param>
		/// <returns>Each matching piece will be an element within an IEnumerable</returns>
		public static IEnumerable<string> SplitOnMatches(string strString, string strRegEx) {
			Match rxmPiece;
			List<String>lstrPieces;

			lstrPieces = new List<String>();

			if (strRegEx.Substring(0, 1).CompareTo("^") != 0) {//put a beginning anchor
				strRegEx = "^" + strRegEx;
			}

			while (strString.CompareTo("") != 0) {//Pull all the pieces out of the string
				rxmPiece = Regex.Match(strString, strRegEx);

				if (rxmPiece.Success == true) { //Found another matching chunk
					lstrPieces.Add(strString.Substring(0, rxmPiece.Length));

					strString = strString.Substring(rxmPiece.Length);
				} else { //No matches, completed spliting
					lstrPieces.Add(strString);
					strString = "";
				}
			}

			return lstrPieces;
		}

		/// <summary>
		/// Strips all HTML tags from a string
		/// </summary>
		/// <param name="strSource">The string to clean</param>
		/// <returns>Returns the string after removing the HTML tags</returns>
		public static string StripHTMLFromString(string strSource) {
			Match rxmResult;
			string strResult;

			strResult = strSource.Trim();
			strResult = strResult.Replace("\n", "");
			strResult = strResult.Replace("\r", "");
			strResult = strResult.Replace("\xA0", "");
			strResult = strResult.Replace("\xC2", "");
			//strResult = strResult.Replace((char)0xA0, ' '); //Ascii cahracter 160
			//strResult = strResult.Replace((char)0xC2, ' '); //Ascii character 194

			strResult = strResult.Replace("</div>", "</div>\n"); //Not all lines end with <br>

			//Replace break tags with new lines
			rxmResult = Regex.Match(strResult, "<br ?/?>", RegexOptions.IgnoreCase);
			while (rxmResult.Success == true) {
				//Keep the text before and aftr the found tag
				strResult = strResult.Substring(0, rxmResult.Index) + "\n" + strResult.Substring(rxmResult.Index + rxmResult.Length);

				//Look for more tags
				rxmResult = Regex.Match(strResult, "<br ?/?>", RegexOptions.IgnoreCase);
			}

			//Delete all other tags
			rxmResult = Regex.Match(strResult, "</?[A-Z0-9 ='\":\\-]+/?>", RegexOptions.IgnoreCase);
			while (rxmResult.Success == true) {
				//Keep the text before and aftr the found tag
				strResult = strResult.Substring(0, rxmResult.Index) + strResult.Substring(rxmResult.Index + rxmResult.Length);

				//Look for more tags
				rxmResult = Regex.Match(strResult, "</?[^>]+/?>", RegexOptions.IgnoreCase);
			}

			//Replace common constants
			strResult = strResult.Replace("&lt;", "<");
			strResult = strResult.Replace("&gt;", ">");
			strResult = strResult.Replace("&quot;", "\"");
			strResult = strResult.Replace("&apos;", "\'");
			strResult = strResult.Replace("&nbsp;", " ");
			strResult = strResult.Replace("&amp;", "&");

			return strResult.Trim();
		}

		/// <summary>
		/// Sanitizes a string so that it can be placed into an XML file without breaking the XML syntax
		/// </summary>
		/// <param name="strSource">The string to clean</param>
		/// <returns>Returns the string with characters reserved for the syntax replaced with HTML codes</returns>
		public static string CleanStringForXML(string strSource) {
			string strClean;

			strClean = strSource.Replace("&", "&amp;");

			strClean = strClean.Replace("<", "&lt;");
			strClean = strClean.Replace(">", "&gt;");
			strClean = strClean.Replace("\"", "&quot;");
			strClean = strClean.Replace("'", "&apos;");

			return strClean.Trim();
		}

		/// <summary>
		/// Restores a string from a form sanitized to include in XML back to its original state
		/// </summary>
		/// <param name="strSource">The sanitized string</param>
		/// <returns>The restored string</returns>
		public static string RestoreStringFromXML(string strSource) {
			string strClean;

			strClean = strSource;

			strClean = strClean.Replace("&lt;", "<");
			strClean = strClean.Replace("&gt;", ">");
			strClean = strClean.Replace("&quot;", "\"");
			strClean = strClean.Replace("&apos;", "'");
			strClean = strClean.Replace("&amp;", "&");

			strClean = strClean.Replace((char)0xA0, ' '); //Ascii cahracter 160
			strClean = strClean.Replace((char)0xC2, ' '); //Ascii character 194

			return strClean.Trim();
		}
	}
}

