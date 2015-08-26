using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;

namespace MDLN.Cards {
	public class CardDisplay : Container {
		private CardInfo cCard;
		private TextureFont cFont;
		private bool cIsFullCard;

		public CardDisplay(GraphicsDevice GraphDev, int Height, int Width, TextureFont Font) : base(GraphDev, Height, Width) {
			this.OpenEffect = DisplayEffect.Fade;
			this.CloseEffect = DisplayEffect.Fade;
			this.BackgroundColor = new Color(255, 255, 255, 0);
			cFont = Font;

			cIsFullCard = true;
		}

		public CardInfo Card {
			get {
				return cCard;
			}

			set {
				cCard = value;
				HasChanged = true;
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
				cDrawBatch.Draw(cCard.Background, ClientRegion, Color.White);
				cDrawBatch.Draw(cCard.Image, new Rectangle(10, 10, ClientRegion.Width - 20, 145), Color.White);

				cFont.WriteText(cDrawBatch, cCard.Title, 15, 165, 10, Color.Black);

				LineTop = 190;
				foreach (string Line in cCard.Description) {
					cFont.WriteText(cDrawBatch, Line, 12, LineTop, 10, Color.Black);
					LineTop += 12;
				}
			} else { //Show a small version with title and picture
				Rectangle DrawRegion = new Rectangle();

				DrawRegion.X = ClientRegion.X;
				DrawRegion.Y = ClientRegion.Y;
				DrawRegion.Width = 200;
				DrawRegion.Height = 175;

				cDrawBatch.Draw(cCard.Background, DrawRegion, Color.White);

				cDrawBatch.Draw(cCard.Image, new Rectangle(10, 10, 184, 116), Color.White);
				cFont.WriteText(cDrawBatch, cCard.Title, 12, 135, 10, Color.Black);
				cFont.WriteText(cDrawBatch, "H:# A:#", 15, 150, 10, Color.Black);
			}

			cCard.Changed = false;
		}

		public override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
			if (cCard.Changed == true) {
				HasChanged = true;
			}
		}
	}

	public class CardInfo {
		public CardType Type;
		public bool Changed;

		protected Texture2D cImage, cBackground;
		protected List<string> cDescLines;
		protected string cTitle;
		protected int cCurrHealth, cMaxHealth, cAttack;

		public CardInfo() : this(CardType.None) { }

		public CardInfo(CardType SetType) {
			cDescLines = new List<string>();
			cCurrHealth = 0;
			cMaxHealth = 0;
			cAttack = 0;
			cImage = null;
			cBackground = null;
			cTitle = "";
			Type = SetType;
			Changed = false;
		}

		public string Title {
			get {
				return cTitle;
			}

			set {
				cTitle = value;
				Changed = true;
			}
		}

		public int CurrentHealth {
			get {
				return cCurrHealth;
			}

			set {
				cCurrHealth = value;
				Changed = true;
			}
		}

		public int MaxHealth {
			get {
				return cMaxHealth;
			}

			set {
				cMaxHealth = value;
				Changed = true;
			}
		}

		public int Attack {
			get {
				return cAttack;
			}

			set {
				cAttack = value;
				Changed = true;
			}
		}

		public Texture2D Image {
			get {
				return cImage;
			}

			set {
				cImage = value;
				Changed = true;
			}
		}

		public Texture2D Background {
			get {
				return cBackground;
			}

			set {
				cBackground = value;
				Changed = true;
			}
		}

		public IEnumerable<string> Description {
			get {
				return cDescLines;
			}

			set {
				cDescLines.Clear();

				foreach (string Line in value) {
					cDescLines.Add(Line);
				}

				Changed = true;
			}
		}
	}

	public enum CardType {
		Monster,
		Room,
		Ability,
		Treasure,
		None
	}
}

