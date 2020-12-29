using Microsoft.Xna.Framework.Input;

using System;

namespace MDLN.MGTools {
	/// <summary>
	/// Class to assist in interpretting mouse and keyboard input
	/// </summary>
	public static class MGInput {
		/// <summary>
		/// Examine the currently pressed keys to determine what text has been typed
		/// </summary>
		/// <returns>The typed chars.</returns>
		/// <param name="CurrKeys">Curren keyboardstate.</param>
		/// <param name="PriorKeys">Prior keyboard state, how it was during a previous update.</param>
		public static string GetTypedChars(KeyboardState CurrKeys, KeyboardState PriorKeys) {
			Keys[] PressedList = CurrKeys.GetPressedKeys();
			string NewKeys = "";
			bool ShiftDown = false;

			if ((CurrKeys.IsKeyDown(Keys.LeftShift) == true) || (CurrKeys.IsKeyDown(Keys.RightShift) == true)) {
				ShiftDown = true;
			} else {
				ShiftDown = false;
			}

			foreach (Keys CurrKey in PressedList) {
				if (PriorKeys.IsKeyDown(CurrKey) == false) {
					if ((CurrKey >= Keys.A) && (CurrKey <= Keys.Z)) {
						if (ShiftDown == true) {
							NewKeys += CurrKey.ToString();
						} else {
							NewKeys += CurrKey.ToString().ToLower();
						}
					} else if ((CurrKey >= Keys.D0) && (CurrKey <= Keys.D9)) {
						string Num = ((int)(CurrKey - Keys.D0)).ToString();

						if (ShiftDown == true) {
							switch (Num) {
							case "1":
								NewKeys += "!";
								break;
							case "2":
								NewKeys += "@";
								break;
							case "3":
								NewKeys += "#";
								break;
							case "4":
								NewKeys += "$";
								break;
							case "5":
								NewKeys += "%";
								break;
							case "6":
								NewKeys += "^";
								break;
							case "7":
								NewKeys += "&";
								break;
							case "8":
								NewKeys += "*";
								break;
							case "9":
								NewKeys += "(";
								break;
							case "0":
								NewKeys += ")";
								break;
							default:
								//wtf?
								break;
							}
						} else {
							NewKeys += ((int)(CurrKey - Keys.D0)).ToString();
						}
					} else if ((CurrKey >= Keys.NumPad0) && (CurrKey <= Keys.NumPad9)) {
						NewKeys += ((int)(CurrKey - Keys.NumPad0)).ToString();
					} else if (CurrKey == Keys.OemPlus) {
						if (ShiftDown == true) {
							NewKeys += "+";
						} else {
							NewKeys += "=";
						}
					} else if (CurrKey == Keys.OemMinus) {
						if (ShiftDown == true) {
							NewKeys += "_";
						} else {
							NewKeys += "-";
						}
					} else if (CurrKey == Keys.OemOpenBrackets) {
						if (ShiftDown == true) {
							NewKeys += "{";
						} else {
							NewKeys += "[";
						}
					} else if (CurrKey == Keys.OemCloseBrackets) {
						if (ShiftDown == true) {
							NewKeys += "}";
						} else {
							NewKeys += "]";
						}
					} else if (CurrKey == Keys.Space) {
						NewKeys += " ";
					} else if (CurrKey == Keys.Enter) {
						NewKeys += "\n";
					} else if (CurrKey == Keys.Back) {
						NewKeys += "\b";
					}
				}
			}

			return NewKeys;
		}
	}
}

