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
		private Texture2D cCardImage, cCardBase;
		private TextureFont cFont;
		private int cCurrHealth, cMaxHealth, cAttack;
		private bool cIsFullCard;

		public Card(GraphicsDevice GraphDev, int Height, int Width, TextureFont Font) : base(GraphDev, Height, Width) {
			this.OpenEffect = DisplayEffect.Fade;
			this.CloseEffect = DisplayEffect.Fade;
			this.BackgroundColor = new Color(255, 255, 255, 0);
			cFont = Font;

			cCardTitle = "";
			cCardDescLines = new List<string>();

			cIsFullCard = true;
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

		public Texture2D CardBase {
			get {
				return cCardBase;
			}

			set {
				cCardBase = value;
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

		public bool ShowFullCard {
			get {
				return cIsFullCard;
			}

			set {
				if (cIsFullCard != value) {
					HasChanged = true;
				}

				cIsFullCard = value;
			}
		}

		public override void DrawContents(GameTime CurrTime) {
			int LineTop;

			if (cIsFullCard == true) {
				cDrawBatch.Draw(cCardBase, ClientRegion, Color.White);
				cDrawBatch.Draw(cCardImage, new Rectangle(10, 10, ClientRegion.Width - 20, 145), Color.White);

				cFont.WriteText(cDrawBatch, cCardTitle, 15, 165, 10, Color.Black);

				LineTop = 190;
				foreach (string Line in cCardDescLines) {
					cFont.WriteText(cDrawBatch, Line, 12, LineTop, 10, Color.Black);
					LineTop += 12;
				}
			} else { //Show a small version with title and picture
				Rectangle DrawRegion = new Rectangle();

				DrawRegion.X = ClientRegion.X;
				DrawRegion.Y = ClientRegion.Y;
				DrawRegion.Width = 200;
				DrawRegion.Height = 175;

				cDrawBatch.Draw(cCardBase, DrawRegion, Color.White);

				cDrawBatch.Draw(cCardImage, new Rectangle(10, 10, 184, 116), Color.White);
				cFont.WriteText(cDrawBatch, cCardTitle, 12, 135, 10, Color.Black);
				cFont.WriteText(cDrawBatch, "H:# A:#", 15, 150, 10, Color.Black);
			}
		}

		public override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
		
		}
	}
}

