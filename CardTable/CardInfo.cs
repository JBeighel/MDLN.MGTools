using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Collections.Generic;

namespace MDLN.CardTable {
	public class CardInfo {
		private string cTitle;
		private int cCurrHealth, cMaxHealth, cAttack;
		private Texture2D cBackground, cImage;
		private List<string> cDescLines;

		public CardInfo() : this(CardType.None) { }

		public CardInfo(CardType Type) {
			this.Type = Type;
			cDescLines = new List<string>();
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

		public IEnumerable<string> DescriptionLines {
			get {
				return cDescLines;
			}

			set {
				cDescLines.Clear();

				foreach (string Line in value) {
					cDescLines.Add(Line);
				}

				HasChanged = true;
			}
		}

		public void RenderMiniCard(Rectangle DrawRegion, SpriteBatch Draw) {
			
		}

		public void RenderFullCard(Rectangle DrawRegion, SpriteBatch Draw) {

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

