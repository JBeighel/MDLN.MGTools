using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

namespace MDLN.Cards {
	public class Card : Container {
		private string cCardTitle;
		private List<string> cCardDescLines;
		private Texture2D cCardImage;
		private TextureFont cFont;
		private int cCurrHealth, cMaxHealth, cAttack;

		public Card(GraphicsDevice GraphDev, int Height, int Width, TextureFont Font) : base(GraphDev, Height, Width) {
			this.OpenEffect = DisplayEffect.Fade;
			this.CloseEffect = DisplayEffect.Fade;
			cFont = Font;

			cCardTitle = "";
			cCardDescLines = new List<string>();
		}

		public string Title {
			get {
				return cCardTitle;
			}

			set {
				cCardTitle = value;
				this.HasChanged = true;
			}
		}

		public IEnumerable<string> DescriptionLines {
			get {
				return cCardDescLines;
			}

			set {
				cCardDescLines.Clear();

				foreach (string Line in value) {
					cCardDescLines.Add(Line);
				}

				this.HasChanged = true;
			}
		}

		public Texture2D CardImage {
			get {
				return cCardImage;
			}

			set {
				cCardImage = value;
				this.HasChanged = true;
			}
		}

		public int MaxHealth {
			get {
				return cMaxHealth;
			}

			set {
				cMaxHealth = value;
			}
		}

		public int CurrentHealth {
			get {
				return cCurrHealth;
			}

			set {
				cCurrHealth = value;
			}

		}

		public int Attack {
			get {
				return cAttack;
			}

			set {
				cAttack = value;
			}
		}

		public override void DrawContents(GameTime CurrTime) {
			int LineTop;

			cDrawBatch.Draw(cCardImage, new Rectangle(10, 10, ClientRegion.Width - 20, 145), Color.Wheat);

			cFont.WriteText(cDrawBatch, cCardTitle, 15, 165, 10, Color.Black);

			LineTop = 190;
			foreach (string Line in cCardDescLines) {
				cFont.WriteText(cDrawBatch, Line, 12, LineTop, 10, Color.Black);
				LineTop += 12;
			}
		}

		public override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
		
		}
	}
}

