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

		public TextureFont(Texture2D FontTexture) {
			cFontTexture = FontTexture;

			cTextureColWidth = cFontTexture.Width / 16;
			cTextureRowHeight = cFontTexture.Height / 16;
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

		public void WriteText(SpriteBatch DrawBatch, string Text, int Top, int Left, Color FontColor) {
			WriteText (DrawBatch, Text, cTextureRowHeight, Top, Left, FontColor);
		}

		public void WriteText(SpriteBatch DrawBatch, string Text, int FontHeight, int Top, int Left, Color FontColor) {
			Rectangle LetterPos = new Rectangle(Left, Top, cTextureColWidth, cTextureRowHeight);

			if (FontHeight != cTextureRowHeight) {
				LetterPos.Height = FontHeight;
				LetterPos.Width = (int)(((float)FontHeight / (float)cTextureRowHeight) * (float)cTextureRowHeight);
			}

			foreach(byte CurrChar in System.Text.Encoding.UTF8.GetBytes(Text)) {
				DrawBatch.Draw(cFontTexture, LetterPos, GetCharacterTextureRegion(CurrChar), FontColor);
				LetterPos.X += LetterPos.Width;
			}
		}

		protected Rectangle GetCharacterTextureRegion(byte Ascii) {
			int TexRow, TexCol;

			TexRow = Ascii / 16;
			TexCol = Ascii % 16;

			return new Rectangle(TexCol * cTextureColWidth, TexRow * cTextureRowHeight, cTextureColWidth, cTextureRowHeight);
		}
	}
}