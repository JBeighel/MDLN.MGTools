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
	class DeckFrame : Container {
		private const float DEFAULT_CARDRATIO = 0.714f;
		private const float DEFAULT_MARGINPERCENT = 0.02f;
		private const float DEFAULT_CARDWIDTHPERCENT = 0.40f;
		private const float DEFAULT_FONTPERCENT = 0.07f;
		private const int DEFAULT_ZOOMTIME = 500;

		/// <summary>
		/// Ratio of the card with / card height
		/// Used to scale the cards
		/// </summary>
		public float CardRatio { get; set; }

		/// <summary>
		/// Percent of container size that should be used in margins
		/// </summary>
		public float MarginPercent { get; set; }

		/// <summary>
		/// Percent of the container width that should be used for each card
		/// </summary>
		public float CardWidthPercent { get; set; }

		/// <summary>
		/// Amount of time in milliseconds that the zoom animation should take
		/// </summary>
		public int ZoomDuration { get; set; }

		/// <summary>
		/// List of textures to use as card backs
		/// </summary>
		public List<Texture2D> CardBackList;

		/// <summary>
		/// List of textures to use as card faces
		/// </summary>
		public List<Texture2D> CardFaceList;

		/// <summary>
		/// True if the selected card is zoomed in, false for zoomed out
		/// </summary>
		public bool SelectedCardIsZoomed;

		/// <summary>
		/// Index of the currently selected card.  Negative number means no selected card
		/// </summary>
		public int SelectedCardIndex;

		/// <summary>
		/// Index of the card back selected to display.  Negative number means none chosen
		/// </summary>
		public int SelectedBackIndex {
			get {
				return cSelectedBackIndex;
			}

			set {
				if (value >= CardBackList.Count) {
					throw new IndexOutOfRangeException("Index exceeds number of card backs loaded.");
				}

				cSelectedBackIndex = value;

				if (cSelectedBackIndex >= 0) {
					cCardBackCtrl.Background = CardBackList[cSelectedBackIndex];
				}
				cCardBackCtrl.Text = "";

				HasChanged = true;
			}
		}

		/// <summary>
		/// Maximum number of cards that can be drawn and shown
		/// Additional cards drawn will begin to replace the currently draw ones
		/// Set to -1 for no draw limit
		/// </summary>
		public int MaxCardsShown { get; set; }

		/// <summary>
		/// Any textual information drawn by this control will use this font
		/// </summary>
		public TextureFont Font {
			get {
				return cFont;
			}

			set {
				cFont = value;

				cCardBackCtrl.Font = Font;
			}
		}

		public event CardClickedEventHandler CardClick;

		/// <summary>
		/// Random number generator for deck shuffling
		/// </summary>
		private Random cRandNumber;
		/// <summary>
		/// Order of the cards in the deck when shuffled
		/// </summary>
		private List<int> cDeckShuffleList;
		/// <summary>
		/// Current position in the shuffled deck
		/// </summary>
		private int cDeckShuffleIndex;
		/// <summary>
		/// Control that displays the card back/draw deck
		/// </summary>
		private Button cCardBackCtrl;
		/// <summary>
		/// List of cards that have been drawn from the deck and are displayed
		/// </summary>
		private List<ShownCard> cCardsShownList;
		/// <summary>
		/// Index in the card back array of the image to show as card back
		/// </summary>
		private int cSelectedBackIndex;
		/// <summary>
		/// Holds the font used by this and all child controls
		/// </summary>
		private TextureFont cFont;
		/// <summary>
		/// Index of the card that a mouse button went down on
		/// </summary>
		private int cMouseDownCardIndex;
		/// <summary>
		/// Percent complete of any current card zoom animation
		/// </summary>
		private int cZoomAnimPercent;
		private bool cCardRemoved;
		
		/// <summary>
		/// Constructor to prepare the class for use
		/// </summary>
		/// <param name="GraphDev">Graphics device to use to draw</param>
		/// <param name="Height">Height in pixels of this control</param>
		/// <param name="Width">Width in pixels of this control</param>
		public DeckFrame(GraphicsDevice GraphDev, int Height, int Width) 
			: base(GraphDev, null, 0, 0, Height, Width) {
			Rectangle CardRegion = new Rectangle();

			CardRegion.X = (int)(ClientRegion.Width * MarginPercent);
			CardRegion.Y = (int)(ClientRegion.Height * MarginPercent);
			CardRegion.Width = (int)(ClientRegion.Width * CardWidthPercent);
			CardRegion.Height = (int)(CardRegion.Width / CardRatio);

			cCardBackCtrl = new Button(GraphDev, null, CardRegion.Y, CardRegion.X, CardRegion.Height, CardRegion.Width);
			cCardBackCtrl.BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
			cCardBackCtrl.Click += CardBackClickHandler;
			cCardBackCtrl.Visible = true;
			cCardBackCtrl.FontColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
			cCardBackCtrl.Text = "No Cards";
			cCardBackCtrl.Alignment = Justify.MiddleCenter;

			CardBackList = new List<Texture2D>();
			CardFaceList = new List<Texture2D>();
			cCardsShownList = new List<ShownCard>();

			cDeckShuffleList = new List<int>();

			cRandNumber = new Random();

			MaxCardsShown = -1;
			SelectedBackIndex = -1;
			SelectedCardIndex = -1;
			SelectedCardIsZoomed = false;
			cDeckShuffleIndex = -1;
			cMouseDownCardIndex = -1;

			CardRatio = DEFAULT_CARDRATIO;
			MarginPercent = DEFAULT_MARGINPERCENT;
			CardWidthPercent = DEFAULT_CARDWIDTHPERCENT;
			ZoomDuration = DEFAULT_ZOOMTIME;

			BackgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
		}

		/// <summary>
		/// Shuffles the cards in the deck into the draw pile
		/// </summary>
		/// <param name="ClearShownCards">Removes all visible drawn cards from the control</param>
		/// <param name="OmitShownCardsFromDeck">When shuffling the deck do not shuffle in the cards being shown</param>
		public void ShuffleCompleteDeck(bool ClearShownCards, bool OmitShownCardsFromDeck) {
			List<int> CardList = new List<int>();
			int Index;

			for (Index = 0; Index < CardFaceList.Count; Index++) {
				if ((OmitShownCardsFromDeck == false) || (cCardsShownList.Where(rec => rec.TextureIndex == Index).Count() == 0)) {
					CardList.Add(Index);
				}
			}

			cDeckShuffleList.Clear();
			while (CardList.Count > 0) {
				Index = cRandNumber.Next(0, CardList.Count);

				cDeckShuffleList.Add(CardList[Index]);
				
				CardList.RemoveAt(Index);
			}

			if (cDeckShuffleList.Count == 0) {//No cards in deck, set index to invalid
				cDeckShuffleIndex = -1;
			} else { //Deck contains cards, put index at beginning
				cDeckShuffleIndex = 0;
			}

			if (ClearShownCards == true) {
				cCardsShownList.Clear();
				HasChanged = true;
			}
		}

		/// <summary>
		/// Select a random card back image to use
		/// </summary>
		public void SelectRandomCardBack() {
			SelectedBackIndex = cRandNumber.Next(0, CardBackList.Count);
		}

		public void EmptyDeck() {
			cCardsShownList.Clear();
			CardFaceList.Clear();

			ShuffleCompleteDeck(false, true);
			HasChanged = true;
		}

		public void RemoveShownCard(int Index) {
			if ((Index >= 0) && (Index < cCardsShownList.Count)) {
				cCardsShownList.RemoveAt(Index);
				HasChanged = true;
				cCardRemoved = true;
			}

			if (Index == SelectedCardIndex) {
				SelectedCardIndex = -1;
				SelectedCardIsZoomed = false;
				cZoomAnimPercent = 100;
			}
		}

		/// <summary>
		/// Render the content of this control to the screen
		/// </summary>
		/// <param name="CurrTime">Current time information of the game</param>
		protected override void DrawContents(GameTime CurrTime) {
			cCardBackCtrl.Draw(cDrawBatch);

			for (int Ctr = 0; Ctr < cCardsShownList.Count; Ctr++) {
				cDrawBatch.Draw(CardFaceList[cCardsShownList[Ctr].TextureIndex], cCardsShownList[Ctr].DrawRegion, Color.White);
			}

			if ((SelectedCardIndex >= 0) && ((SelectedCardIsZoomed == true) || (cZoomAnimPercent < 70))) { //Draw zoomed card last so it's on top
				cDrawBatch.Draw(CardFaceList[cCardsShownList[SelectedCardIndex].TextureIndex], cCardsShownList[SelectedCardIndex].DrawRegion, Color.White);
			}
		}

		/// <summary>
		/// Update all information use by this control in response to time passage or user input
		/// </summary>
		/// <param name="CurrTime">Current time information of the game</param>
		/// <param name="CurrKeyboard">Current state of the keyboard</param>
		/// <param name="CurrMouse">Current state of the mouse</param>
		protected override void UpdateContents(GameTime CurrTime, KeyboardState CurrKeyboard, MouseState CurrMouse, bool ProcessMouseEvent) {
			int MarginVert, MarginHoriz, CardHeight, CardWidth, FontHeight;
			Rectangle CurrCardRegion;

			MarginVert = (int)(ClientRegion.Width * MarginPercent);
			MarginHoriz = (int)(ClientRegion.Height * MarginPercent);
			CardWidth = (int)(ClientRegion.Width * CardWidthPercent);
			CardHeight = (int)(CardWidth / CardRatio);
			FontHeight = (int)(CardHeight * DEFAULT_FONTPERCENT);

			//Update the card back, draw deck, control
			if (cCardBackCtrl.Top != MarginVert) {
				cCardBackCtrl.Top = MarginVert;
				cCardBackCtrl.Left = MarginHoriz;
				HasChanged = true;
			}

			if (cCardBackCtrl.Width != CardWidth) {
				cCardBackCtrl.Width = CardWidth;
				cCardBackCtrl.Height = CardHeight;
				HasChanged = true;
			}

			if (cCardBackCtrl.Font.CharacterHeight != FontHeight) {
				cCardBackCtrl.FontSize = FontHeight;
			}

			cCardBackCtrl.Update(CurrTime, CurrKeyboard, CurrMouse, !SelectedCardIsZoomed);
				
			if (cCardsShownList.Count > 0) {
				//Update all of the face card, shown cards, controls
				CurrCardRegion.Y = MarginVert;
				CurrCardRegion.X = CardWidth + (MarginHoriz * 2);
				CurrCardRegion.Height = CardHeight;
				CurrCardRegion.Width = CardWidth;

				//Determine spacing between cards

				if (cCardsShownList.Count > 1) {
					CardHeight = ClientRegion.Height - ((MarginHoriz * 2) + CardHeight);
					CardHeight /= cCardsShownList.Count - 1;
				} else {
					CardHeight = MarginHoriz + CurrCardRegion.Height;
				}

				if (CardHeight > MarginHoriz + CurrCardRegion.Height) {
					CardHeight = MarginHoriz + CurrCardRegion.Height;
				}

				for (int Ctr = 0; Ctr < cCardsShownList.Count; Ctr++) {
					ShownCard CurrCard = cCardsShownList[Ctr];

					if ((Ctr == SelectedCardIndex) && ((cZoomAnimPercent < 100) || (SelectedCardIsZoomed == true))) {
						CurrCard.DrawRegion = DetermineZoomCardRegion(CurrTime.ElapsedGameTime.Milliseconds, CurrCardRegion);
						HasChanged = true;
					} else {
						CurrCard.DrawRegion.Y = CurrCardRegion.Y;
						CurrCard.DrawRegion.X = CurrCardRegion.X;
						CurrCard.DrawRegion.Height = CurrCardRegion.Height;
						CurrCard.DrawRegion.Width = CurrCardRegion.Width;
					}

					cCardsShownList[Ctr] = CurrCard;

					CurrCardRegion.Y += CardHeight;
				}

				if (cCardRemoved == true) {
					HasChanged = true;
					cCardRemoved = false;
				}
			}
		}

		protected override void MouseEventButtonDown(MouseState CurrMouse, MouseButton Button) {
			cMouseDownCardIndex = DetermineShownCardFromCoords(CurrMouse.Position);
		}

		protected override void MouseEventButtonUp(MouseState CurrMouse, MouseButton Button) {
			int MouseUpIndex;

			if (cMouseDownCardIndex < 0) {
				return;
			}

			MouseUpIndex = DetermineShownCardFromCoords(CurrMouse.Position);

			if (MouseUpIndex == cMouseDownCardIndex) {
				SelectedCardIndex = cMouseDownCardIndex;
				cZoomAnimPercent = 0;
				SelectedCardIsZoomed = !SelectedCardIsZoomed;

				HasChanged = true;

				if (CardClick != null) {
					CardClick(this, SelectedCardIndex, Button);
				}
			}

			cMouseDownCardIndex = -1;
		}
		
		private void CardBackClickHandler(object Sender, MouseButton Button) {
			ShownCard NewCard;

			if ((cDeckShuffleIndex >= cDeckShuffleList.Count) || (cDeckShuffleIndex < 0)) {
				ShuffleCompleteDeck(false, true);
			} else {
				NewCard = new ShownCard();

				NewCard.DrawRegion = new Rectangle(0, 0, 100, 100);
				NewCard.TextureIndex = cDeckShuffleList[cDeckShuffleIndex];
				cCardsShownList.Add(NewCard);

				cDeckShuffleIndex += 1;
				HasChanged = true;

				while ((cCardsShownList.Count > MaxCardsShown) && (MaxCardsShown > 0)) {
					cCardsShownList.RemoveAt(0);
				}
			}

			if (cDeckShuffleIndex >= cDeckShuffleList.Count) {
				cCardBackCtrl.Text = "Shuffle";
			} else if (cDeckShuffleIndex == -1) {
				cCardBackCtrl.Text = "No Cards";
			} else {
				cCardBackCtrl.Text = "";
			}

			HasChanged = true;
		}

		private int DetermineShownCardFromCoords(Point Coord) {
			if ((SelectedCardIndex != -1) && (SelectedCardIsZoomed == true)) {
				if (MGMath.IsPointInRect(Coord, cCardsShownList[SelectedCardIndex].DrawRegion) == true) {
					return SelectedCardIndex;
				}
			}

			for (int Ctr = cCardsShownList.Count - 1; Ctr >= 0; Ctr--) { //Cards are placed on top, so start from bottom to get most visible cards
				if (MGMath.IsPointInRect(Coord, cCardsShownList[Ctr].DrawRegion) == true) {
					return Ctr;
				}
			}

			return -1;
		}

		private Rectangle DetermineZoomCardRegion(double EllapsedTime, Rectangle ZoomOut) {
			Rectangle Region = new Rectangle();
			Rectangle ZoomIn = new Rectangle();
			int MoveAmnt, ZoomAdjust;

			if (ClientRegion.Width / CardRatio < ClientRegion.Height) {
				ZoomIn.Width = ClientRegion.Width;
				ZoomIn.Height = (int)(ClientRegion.Width / CardRatio);
			} else {
				ZoomIn.Height = ClientRegion.Height;
				ZoomIn.Width = (int)(ClientRegion.Height * CardRatio);
			}
			ZoomIn.X = (ClientRegion.Width - ZoomIn.Width) / 2;
			ZoomIn.Y = (ClientRegion.Height - ZoomIn.Height) / 2;

			cZoomAnimPercent += (int)((EllapsedTime * 100) / ZoomDuration);
			if (cZoomAnimPercent >= 100) {
				cZoomAnimPercent = 100;

				if (SelectedCardIsZoomed == true) {
					return ZoomIn;
				} else {
					return ZoomOut;
				}
			} else {
				ZoomAdjust = (cZoomAnimPercent * cZoomAnimPercent) / 200;
				ZoomAdjust += 1;

				if (SelectedCardIsZoomed == false) {
					MoveAmnt = ZoomIn.X - ZoomOut.X;
					Region.X = ZoomOut.X + (MoveAmnt / ZoomAdjust);

					MoveAmnt = ZoomIn.Y - ZoomOut.Y;
					Region.Y = ZoomOut.Y + (MoveAmnt / ZoomAdjust);

					MoveAmnt = ZoomIn.Height - ZoomOut.Height;
					Region.Height = ZoomOut.Height + (MoveAmnt / ZoomAdjust);

					MoveAmnt = ZoomIn.Width - ZoomOut.Width;
					Region.Width = ZoomOut.Width + (MoveAmnt / ZoomAdjust);
				} else {
					MoveAmnt = ZoomOut.X - ZoomIn.X;
					Region.X = ZoomIn.X + (MoveAmnt / ZoomAdjust);

					MoveAmnt = ZoomOut.Y - ZoomIn.Y;
					Region.Y = ZoomIn.Y + (MoveAmnt / ZoomAdjust);

					MoveAmnt = ZoomOut.Height - ZoomIn.Height;
					Region.Height = ZoomIn.Height + (MoveAmnt / ZoomAdjust);

					MoveAmnt = ZoomOut.Width - ZoomIn.Width;
					Region.Width = ZoomIn.Width + (MoveAmnt / ZoomAdjust);
				}
			}

			return Region;
		}

		private struct ShownCard {
			public int TextureIndex;
			public Rectangle DrawRegion;
		}
	}

	public delegate void CardClickedEventHandler(object Sender, int CardIndex, MouseButton Button);
}
