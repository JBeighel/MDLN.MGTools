using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MDLN.MGTools {
	public class TextureFont {
		private Texture2D cFontTexture;
		private int cTextureColWidth, cTextureRowHeight;

		public TextureFont() : this(null) { }

		public TextureFont(Texture2D FontTexture) {
			UpdateFontTexture(FontTexture);
		}

		public int CharacterWidth {
			get {
				return cTextureColWidth;
			}
		}

		public int CharacterHeight {
			get {
				return cTextureRowHeight;
			}
		}

		public Texture2D FontTexture {
			get {
				return cFontTexture;
			}

			set {
				UpdateFontTexture(value);
			}
		}

		public void WriteText(SpriteBatch DrawBatch, string Text, int Top, int Left, Color FontColor) {
			WriteText (DrawBatch, Text, cTextureRowHeight, Top, Left, FontColor);
		}

		public void WriteText(SpriteBatch DrawBatch, string Text, int FontHeight, int Top, int Left, Color FontColor) {
			Rectangle LetterPos = new Rectangle(Left, Top, cTextureColWidth, cTextureRowHeight);

			LetterPos.Height = FontHeight;
			LetterPos.Width = GetCharacterWidth(FontHeight);

			foreach(byte CurrChar in System.Text.Encoding.UTF8.GetBytes(Text)) {
				DrawBatch.Draw(cFontTexture, LetterPos, GetCharacterTextureRegion(CurrChar), FontColor);
				LetterPos.X += LetterPos.Width;
			}
		}

		public void WriteAsciiCharacter(SpriteBatch DrawBatch, byte[] AsciiChars, int FontHeight, int Top, int Left, Color FontColor) {
			Rectangle LetterPos = new Rectangle(Left, Top, cTextureColWidth, cTextureRowHeight);

			LetterPos.Height = FontHeight;
			LetterPos.Width = GetCharacterWidth(FontHeight);

			foreach(byte CurrChar in AsciiChars) {
				DrawBatch.Draw(cFontTexture, LetterPos, GetCharacterTextureRegion(CurrChar), FontColor);
				LetterPos.X += LetterPos.Width;
			}
		}

		public int DetermineRenderWidth(string Text) {
			return DetermineRenderWidth(Text, cTextureColWidth);
		}

		public int DetermineRenderWidth(string Text, int FontHeight) {
			return Text.Length * GetCharacterWidth(FontHeight);
		}

		protected Rectangle GetCharacterTextureRegion(byte Ascii) {
			int TexRow, TexCol;

			TexRow = Ascii / 16;
			TexCol = Ascii % 16;

			return new Rectangle(TexCol * cTextureColWidth, TexRow * cTextureRowHeight, cTextureColWidth, cTextureRowHeight);
		}

		private void UpdateFontTexture(Texture2D NewTexture) {
			cFontTexture = NewTexture;

			if (cFontTexture != null) {
				cTextureColWidth = cFontTexture.Width / 16;
				cTextureRowHeight = cFontTexture.Height / 16;
			}
		}

		private int GetCharacterWidth(int CharHeight) {
			if (CharHeight == cTextureRowHeight) {
				return cTextureColWidth;
			} else {
				return (int)(((float)CharHeight / (float)cTextureRowHeight) * (float)cTextureRowHeight);
			}
		}
	}
}