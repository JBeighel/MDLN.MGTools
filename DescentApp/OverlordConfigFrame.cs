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

		public TextureFont Font {
			get {
				return cFont;
			}

			set {
				cFont = value;
				cCardsFrame.Font = value;
			}
		}

		private OverlordCardsFrame cCardsFrame;
		private Dictionary<string, Button> cDeckChoiceCtrlList;
		private List<OverlordCard> cOverlordCardList;
		private TextureFont cFont;

		public List<OverlordCard> CardList {
			get {
				return cOverlordCardList;
			}
		}

		public OverlordDecksFrame(GraphicsDevice GraphDev, int Height, int Width)
			: base(GraphDev, Height, Width) {
			cCardsFrame = new OverlordCardsFrame(GraphDev, Height, GraphDev.Viewport.Width - Width);
			cCardsFrame.Top = 0;
			cCardsFrame.Left = Width;
			cCardsFrame.Visible = false;
			cCardsFrame.OpenEffect = DisplayEffect.SlideUp;
			cCardsFrame.CloseEffect = DisplayEffect.SlideDown;

			cDeckChoiceCtrlList = new Dictionary<string, Button>();
			cOverlordCardList = new List<OverlordCard>();

			BackgroundColor = new Color(0.8f, 0.608f, 0.398f, 1.0f);
			cCardsFrame.BackgroundColor = new Color(0.8f, 0.608f, 0.398f, 1.0f);
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
				cDeckChoiceCtrlList[NewCard.Class].Click += DeckChoiceClickHandler;

				HasChanged = true;
			}
		}

		public void SetIconImageSet(Dictionary<string, Texture2D> IconDict) {
			cCardsFrame.SetIconSet(IconDict);
		}
		
		public new void Draw(SpriteBatch DrawBatch) {
			cCardsFrame.Draw();
			base.Draw(DrawBatch);
		}

		protected override void DrawContents(GameTime CurrTime) {
			cCardsFrame.Draw();

			foreach(Button DeckOption in cDeckChoiceCtrlList.Values) {
				DeckOption.Draw(cDrawBatch);
			}
		}

		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse, bool ProcessMouseEvent) {
			int OptionTop = 0;

			if (Visible == true) {
				OptionTop += 0;
			}

			if (ProcessMouseEvent == false) {
				ProcessMouseEvent = false;
			}

			cCardsFrame.Update(CurrTime, CurrKeyboard, CurrMouse, ProcessMouseEvent);

			foreach (Button DeckOption in cDeckChoiceCtrlList.Values) {
				DeckOption.Top = OptionTop;
				DeckOption.Update(CurrTime, CurrKeyboard, CurrMouse, ProcessMouseEvent);

				OptionTop += ClientRegion.Height / DECKOPTIONS_PERPAGE;
			}

			HasChanged = true; //Hack fix for the deck option selection, forces full redraw every frame
		}

		protected override void Resized() {
			foreach (Button DeckOption in cDeckChoiceCtrlList.Values) {
				DeckOption.Width = ClientRegion.Width;
				DeckOption.Height = ClientRegion.Height / DECKOPTIONS_PERPAGE;
				DeckOption.FontSize = (ClientRegion.Height / DECKOPTIONS_PERPAGE) / 2;
			}

			cCardsFrame.Height = Height;
			cCardsFrame.Width = cGraphicsDevice.Viewport.Width - Width;
			Repositioned();

		}

		protected override void Repositioned() {
			cCardsFrame.Top = Top;
			cCardsFrame.Left = Left + Width;
		}

		protected override void VisibleChanged() {
			cCardsFrame.Visible = Visible;
		}

		private void DeckChoiceClickHandler(object Sender, MouseButton Button) {
			Button CurrChoice = (Button)Sender;

			foreach (Button Choice in cDeckChoiceCtrlList.Values) {
				Choice.BackgroundColor = new Color(0.8f, 0.608f, 0.398f, 1.0f);
			}

			CurrChoice.BackgroundColor = new Color(0.57f, 0.169f, 0.129f, 1.0f);

			cCardsFrame.SetOptionList(cOverlordCardList.Where(x => x.Class.CompareTo(CurrChoice.Text) == 0));
			HasChanged = true;
		}
	}

	class OverlordCardsFrame : Container {
		private List<OverlordCard> cOptionList;
		private List<Rectangle> cOptionLocList;
		private Dictionary<string, Texture2D> cIconSet;
		private Point cMouseDown;
		private int cMouseCardIndex;

		public OverlordCardsFrame(GraphicsDevice GraphDev, int Height, int Width)
			: base(GraphDev, Height, Width) {
			cOptionList = new List<OverlordCard>();
			cOptionLocList = new List<Rectangle>();
			cIconSet = new Dictionary<string, Texture2D>();
			cMouseDown = new Point(-1, -1);
			cMouseCardIndex = -1;
		}

		public void SetOptionList(IEnumerable<OverlordCard> OptList) {
			cOptionList.Clear();
			cOptionList.AddRange(OptList);

			while (cOptionList.Count > cOptionLocList.Count) {
				cOptionLocList.Add(new Rectangle());
			}
			HasChanged = true;
		}

		public TextureFont Font { get; set; }

		public void SetIconSet(Dictionary<string, Texture2D> IconSet) {
			cIconSet.Clear();

			foreach (KeyValuePair<string, Texture2D> IconPair in IconSet) {
				cIconSet.Add(IconPair.Key, IconPair.Value);
			}
		}

		protected override void DrawContents(GameTime CurrTime) {
			Rectangle OptionLoc = new Rectangle();
			int LinePos, Ctr, TextWidth, TextTop, TextLeft;
			string CountText;

			OptionLoc.X = 0;
			OptionLoc.Y = 0;
			OptionLoc.Width = Width / 3;
			OptionLoc.Height = Height / 2;

			LinePos = 0;
			for (Ctr = 0; Ctr < cOptionList.Count; Ctr++) {
				cDrawBatch.Draw(cOptionList[Ctr].Image, OptionLoc, Color.White);
				cOptionLocList[Ctr] = OptionLoc;

				if (cOptionList[Ctr].Include > 0) {
					cDrawBatch.Draw(cIconSet["enabled"], OptionLoc, new Color(0.0f, 0.9f, 0.0f, 0.3f));

					CountText = String.Format("{0} of {1}", cOptionList[Ctr].Include, cOptionList[Ctr].Count);
					TextWidth = Font.DetermineRenderWidth(CountText, OptionLoc.Height / 10);
					TextTop = OptionLoc.Y + ((OptionLoc.Height * 9) / 10);
					TextLeft = ((OptionLoc.Width - TextWidth) / 2) + OptionLoc.X;
					Font.WriteText(cDrawBatch, CountText, OptionLoc.Height / 10, TextTop, TextLeft, new Color(0.0f, 0.9f, 0.0f, 0.7f));
				} else {
					cDrawBatch.Draw(cIconSet["disabled"], OptionLoc, new Color(0.9f, 0.0f, 0.0f, 0.3f));
				}

				LinePos += 1;
				if (LinePos % 3 != 0) {
					OptionLoc.X += OptionLoc.Width;
				} else {
					OptionLoc.X = 0;
					OptionLoc.Y += OptionLoc.Height;
				}
			}
		}

		protected override void MouseEventButtonDown(MouseState CurrMouse, MouseButton Button) {
			int Ctr;
			
			if (Button != MouseButton.Left) {
				return;
			}

			cMouseDown = CurrMouse.Position;

			for (Ctr = 0; Ctr < cOptionList.Count; Ctr++) {
				if (MGMath.IsPointInRect(cMouseDown, cOptionLocList[Ctr]) == true) {
					cMouseCardIndex = Ctr;

					cOptionList[Ctr].Include += 1;
					if (cOptionList[Ctr].Include > cOptionList[Ctr].Count) {
						cOptionList[Ctr].Include = 0;
					}

					break;
				}
			}

			HasChanged = true;
		}

		protected override void MouseEventButtonUp(MouseState CurrMouse, MouseButton Button) {
			if (Button != MouseButton.Left) {
				return;
			}
		}
	}

	/// <summary>
	/// Class to hold overlord card details
	/// 
	/// Using class instead of structure in order to make all variables references to a common
	/// object instead of copies of the object.
	/// </summary>
	public class OverlordCard {
		//Filter points
		public string Class;
		public string Set;
		//Other values
		public Texture2D Image;
		public int Count;
		public int Include;
	}
}

