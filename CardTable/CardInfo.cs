using MDLN.MGTools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.CardTable {
	public class CardInfo {
		private const float FULLMARGINTOPBOTTOMPERCENT = 0.03f;
		private const float FULLMARGINLEFTRIGHTPERCENT = 0.04f;
		private const float FULLTITLETOPPERCENT = 0.47f;
		private const float FULLTITLEHEIGHTPERCENT = 0.043f;
		private const float FULLDESCTOPPERCENT = 0.543f;
		private const float FULLDESCTEXTHEIGHTPERCENT = 0.034f;
		private const float FULLIMAGEHEIGHTPERCENT = 0.41f;
		private const float FULLCOMBATSTATSLEFTPERCENT = 0.37f;
		private const float FULLCOMBATSTATSHEIGHT = 0.057f;

		private const float MINIMARGINTOPBOTTOMPERCENT = 0.057f;
		private const float MINIMARGINLEFTRIGHTPERCENT = 0.05f;
		private const float MINITITLETOPPERCENT = 0.771f;
		private const float MINITITLEHEIGHTPERCENT = 0.0686f;
		private const float MINIIMAGEHEIGHTPERCENT = 0.664f;
		private const float MINICOMBATSTATSLEFTPERCENT = 0.55f;
		private const float MINICOMBATSTATSHEIGHT = 0.086f;

		private string cTitle;
		private int cCurrHealth, cMaxHealth, cAttack;
		private Texture2D cBackground, cImage, cHeartIcon, cSwordIcon;
		private string cDesc;

		public CardInfo() : this(CardType.None) { }

		public CardInfo(CardType Type) {
			this.Type = Type;
			cDesc = "";
			cTitle = "";
		}

		public bool HasChanged { get; set; }

		public CardType Type { get; set; }

		public string Title { 
			get {
				return cTitle;
			}

			set {
				if (cTitle.CompareTo(value) != 0) {
					cTitle = value;
					HasChanged = true;
				}
			}
		}

		public int CurrentHealth {
			get { 
				return cCurrHealth;
			}

			set {
				if (cCurrHealth != value) {
					cCurrHealth = value;
					HasChanged = true;
				}
			}
		}

		public int MaxHealth {
			get { 
				return cMaxHealth;
			}

			set {
				if (cMaxHealth != value) {
					cMaxHealth = value;
					HasChanged = true;
				}
			}
		}

		public int AttackStrength {
			get { 
				return cAttack;
			}

			set {
				if (cAttack != value) {
					cAttack = value;
					HasChanged = true;
				}
			}
		}

		public Texture2D Image {
			get {
				return cImage; 
			}

			set {
				cImage = value;
				HasChanged = true;
			}
		}

		public Texture2D Background {
			get {
				return cBackground; 
			}

			set {
				cBackground = value;
				HasChanged = true;
			}
		}

		public Texture2D HeartIcon {
			get {
				return cHeartIcon; 
			}

			set {
				cHeartIcon = value;
				HasChanged = true;
			}
		}

		public Texture2D SwordIcon {
			get {
				return cSwordIcon; 
			}

			set {
				cSwordIcon = value;
				HasChanged = true;
			}
		}

		public string Description {
			get {
				return cDesc;
			}

			set {
				cDesc = value;

				HasChanged = true;
			}
		}

		public void RenderMiniCard(Rectangle DrawRegion, Color ImageTint, Color FontColor, SpriteBatch Draw, TextureFont Font) {
			Vector2 TitleOrigin, ComStatOrigin;
			int TitleHeight, ComStatHeight;
			Rectangle ImageRegion, IconRegion;

			//Calculate dimensions of the parts of the card
			TitleOrigin.X = (DrawRegion.Width * MINIMARGINLEFTRIGHTPERCENT) + DrawRegion.X;
			TitleOrigin.Y = (DrawRegion.Height * MINITITLETOPPERCENT) + DrawRegion.Y;
			TitleHeight = (int)(DrawRegion.Height * MINITITLEHEIGHTPERCENT);

			ComStatOrigin.X = (DrawRegion.Width * MINICOMBATSTATSLEFTPERCENT) + DrawRegion.X;
			ComStatOrigin.Y = DrawRegion.Height - (DrawRegion.Height * MINICOMBATSTATSHEIGHT) - (DrawRegion.Height * MINIMARGINTOPBOTTOMPERCENT) + DrawRegion.Y;
			ComStatHeight = (int)(DrawRegion.Height * MINICOMBATSTATSHEIGHT);

			ImageRegion.X = (int)TitleOrigin.X;
			ImageRegion.Y = (int)(DrawRegion.Height * MINIMARGINTOPBOTTOMPERCENT) + DrawRegion.Y;
			ImageRegion.Width = DrawRegion.Width - (int)(DrawRegion.Width * MINIMARGINLEFTRIGHTPERCENT * 2);
			ImageRegion.Height = (int)(DrawRegion.Height * MINIIMAGEHEIGHTPERCENT);

			IconRegion.X = (int)ComStatOrigin.X;
			IconRegion.Y = (int)ComStatOrigin.Y;
			IconRegion.Width = ComStatHeight;
			IconRegion.Height = ComStatHeight;

			//Draw the card
			Draw.Draw(cBackground, DrawRegion, cBackground.Bounds, ImageTint);
			Draw.Draw(cImage, ImageRegion, cImage.Bounds, ImageTint);
			Draw.Draw(cHeartIcon, IconRegion, cHeartIcon.Bounds, ImageTint);
			IconRegion.X += ComStatHeight * 3;
			Draw.Draw(cSwordIcon, IconRegion, cSwordIcon.Bounds, ImageTint);

			Font.WriteText(Draw, cTitle, TitleHeight, (int)TitleOrigin.Y, (int)TitleOrigin.X, FontColor);
			Font.WriteText(Draw, " " + cCurrHealth + "  " + cAttack, ComStatHeight, (int)ComStatOrigin.Y, (int)ComStatOrigin.X, FontColor);
		}

		public void RenderFullCard(Rectangle DrawRegion, Color ImageTint, Color FontColor, SpriteBatch Draw, TextureFont Font) {
			Vector2 TitleOrigin, DescOrigin, ComStatOrigin;
			int TitleHeight, DescHeight, ComStatHeight;
			Rectangle ImageRegion, IconRegion;

			//Calculate dimensions of the parts of the card
			TitleOrigin.X = (DrawRegion.Width * FULLMARGINLEFTRIGHTPERCENT) + DrawRegion.X;
			TitleOrigin.Y = (DrawRegion.Height * FULLTITLETOPPERCENT) + DrawRegion.Y;
			TitleHeight = (int)(DrawRegion.Height * FULLTITLEHEIGHTPERCENT);

			DescOrigin.X = TitleOrigin.X;
			DescOrigin.Y = (DrawRegion.Height * FULLDESCTOPPERCENT) + DrawRegion.Y;
			DescHeight = (int)(DrawRegion.Height * FULLDESCTEXTHEIGHTPERCENT);

			ComStatOrigin.X = (DrawRegion.Width * FULLCOMBATSTATSLEFTPERCENT) + DrawRegion.X;
			ComStatOrigin.Y = DrawRegion.Height - (DrawRegion.Height * FULLCOMBATSTATSHEIGHT) - (DrawRegion.Height * FULLMARGINTOPBOTTOMPERCENT) + DrawRegion.Y;
			ComStatHeight = (int)(DrawRegion.Height * FULLCOMBATSTATSHEIGHT);

			ImageRegion.X = (int)TitleOrigin.X;
			ImageRegion.Y = (int)(DrawRegion.Height * FULLMARGINTOPBOTTOMPERCENT) + DrawRegion.Y;
			ImageRegion.Width = DrawRegion.Width - (int)(DrawRegion.Width * FULLMARGINLEFTRIGHTPERCENT * 2);
			ImageRegion.Height = (int)(DrawRegion.Height * FULLIMAGEHEIGHTPERCENT);

			IconRegion.X = (int)ComStatOrigin.X;
			IconRegion.Y = (int)ComStatOrigin.Y;
			IconRegion.Width = ComStatHeight;
			IconRegion.Height = ComStatHeight;

			//Draw the card
			Draw.Draw(cBackground, DrawRegion, cBackground.Bounds, ImageTint);
			Draw.Draw(cImage, ImageRegion, cImage.Bounds, ImageTint);
			Draw.Draw(cHeartIcon, IconRegion, cHeartIcon.Bounds, ImageTint);
			IconRegion.X += ComStatHeight * 5;
			Draw.Draw(cSwordIcon, IconRegion, cSwordIcon.Bounds, ImageTint);

			Font.WriteText(Draw, cTitle, TitleHeight, (int)TitleOrigin.Y, (int)TitleOrigin.X, FontColor);
			Font.WriteText(Draw, cDesc, DescHeight, (int)DescOrigin.Y, (int)DescOrigin.X, FontColor);
			Font.WriteText(Draw, " " + cCurrHealth + "/" + cMaxHealth + "  " + cAttack, ComStatHeight, (int)ComStatOrigin.Y, (int)ComStatOrigin.X, FontColor);
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

