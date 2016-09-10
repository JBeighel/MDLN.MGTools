using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DescentApp {
	class OverlordDecksFrame : Container {
		private const int DECKOPTIONS_PERPAGE = 15;

		public TextureFont Font;

		private Dictionary<string, Button> cDeckChoiceCtrlList;
		private List<OverlordCard> cOverlordCardList;

		public OverlordDecksFrame(GraphicsDevice GraphDev, int Height, int Width)
			: base(GraphDev, Height, Width) {
			cDeckChoiceCtrlList = new Dictionary<string, Button>();
			cOverlordCardList = new List<OverlordCard>();
		}

		public void AddOverlordCard(OverlordCard NewCard) {
			cOverlordCardList.Add(NewCard);

			if (cDeckChoiceCtrlList.ContainsKey(NewCard.Class) == false) {
				cDeckChoiceCtrlList.Add(NewCard.Class, new Button(cGraphicsDevice, null, 0, 0, ClientRegion.Height / DECKOPTIONS_PERPAGE, ClientRegion.Width));
				cDeckChoiceCtrlList[NewCard.Class].BackgroundColor = new Color(0.8f, 0.608f, 0.398f, 1.0f);
				cDeckChoiceCtrlList[NewCard.Class].Visible = true;
				cDeckChoiceCtrlList[NewCard.Class].Text = NewCard.Class;
				cDeckChoiceCtrlList[NewCard.Class].Font = Font;
				cDeckChoiceCtrlList[NewCard.Class].FontColor = Color.Black;
				cDeckChoiceCtrlList[NewCard.Class].FontSize = (ClientRegion.Height / DECKOPTIONS_PERPAGE) / 2;

				HasChanged = true;
			}
		}

		protected override void DrawContents(GameTime CurrTime) {
			foreach(Button DeckOption in cDeckChoiceCtrlList.Values) {
				DeckOption.Draw(cDrawBatch);
			}
		}

		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse) {
			int OptionTop = 0;

			foreach (Button DeckOption in cDeckChoiceCtrlList.Values) {
				DeckOption.Top = OptionTop;
				DeckOption.Update(CurrTime, CurrKeyboard, CurrMouse);

				OptionTop += ClientRegion.Height / DECKOPTIONS_PERPAGE;
			}
		}

		protected override void Resized() {
			foreach (Button DeckOption in cDeckChoiceCtrlList.Values) {
				DeckOption.Width = ClientRegion.Width;
				DeckOption.Height = ClientRegion.Height / DECKOPTIONS_PERPAGE;
				DeckOption.FontSize = (ClientRegion.Height / DECKOPTIONS_PERPAGE) / 2;
			}
		}
	}

	public struct OverlordCard {
		public string Set;
		public string Class;
		public Texture2D Image;
		public int Count;
		public int Include;
	}
}
