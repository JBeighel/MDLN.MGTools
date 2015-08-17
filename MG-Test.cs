using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MDLN.MGTools;


using System.Reflection;

[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]

namespace MGTest
{
	public static class Launcher
	{
		static void Main()
		{
			using (var game = new MGTest())
			{
				game.Run();
			}
		}
	}

	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class MGTest : Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch spriteBatch;
		private Texture2D background, ConsoleRect;
		private int Transparency;
		private KeyboardState PriorKeyState;
		private Console DevConsole;

		private int FrameNum;

		public MGTest()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Initializes the game class
		/// </summary>
		protected override void Initialize() {
			IsMouseVisible = true;

			FrameNum = 1;

			DevConsole = new Console(GraphicsDevice, Content, "Font.png", 0, 0, GraphicsDevice.Viewport.Bounds.Width, GraphicsDevice.Viewport.Bounds.Height / 2);
			DevConsole.CommandSent += CommandSentEventHandler;
			DevConsole.SlideOpen = SlideEffect.SlideDown;
			DevConsole.SlideClosed = SlideEffect.SlideUp;

			//Initializes monogame
			base.Initialize();
		}

		/// <summary>
		/// Called once at start to load content
		/// </summary>
		protected override void LoadContent() {
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			background = Content.Load<Texture2D>("DiceTexture.png");

			ConsoleRect = new Texture2D(graphics.GraphicsDevice, 1, 1);
			ConsoleRect.SetData(new[] { Color.DarkGray });
		}

		/// <summary>
		/// Called once to unload content not in ContentManager
		/// </summary>
		protected override void UnloadContent() {
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Game logic function.  Update world, handle input, etc
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime) {
			int Seconds = gameTime.TotalGameTime.Seconds;
			KeyboardState KeyState = Keyboard.GetState();

			if ((PriorKeyState.IsKeyDown(Keys.OemTilde) == false) && (KeyState.IsKeyDown(Keys.OemTilde) == true)) {
				DevConsole.ToggleVisible();
			}

			DevConsole.Update(gameTime.TotalGameTime, Keyboard.GetState());

			PriorKeyState = KeyState;

			if (Seconds % 6 + 1 != FrameNum) {
				DevConsole.AddText("Advance frame...");
			}
			FrameNum = (Seconds % 6) + 1;

			if (gameTime.TotalGameTime.Milliseconds <= 500) {
				Transparency = gameTime.TotalGameTime.Milliseconds / 2;
			} else {
				Transparency = 255 - ((gameTime.TotalGameTime.Milliseconds - 500) / 2);
			}

			//Use monogame update
			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime) {
			Color Overlay = new Color(255, 255, 255, Transparency);

			GraphicsDevice.Clear(Color.Blue);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

			switch (FrameNum) {
				case 1 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(0, 0, 114, 114), Overlay);
					break;
				case 2 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(115, 0, 114, 114), Overlay);
					break;
				case 3 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(230, 0, 114, 114), Overlay);
					break;
				case 4 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(345, 0, 114, 114), Overlay);
					break;
				case 5 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(0, 115, 114, 114), Overlay);
					break;
				case 6 :
					spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, new Rectangle(115, 115, 114, 114), Overlay);
					break;
			}
			
			

			spriteBatch.End();

			DevConsole.Draw();

			base.Draw(gameTime);
		}

		protected void CommandSentEventHandler(object Sender, string Command) {
			

			if (Command.ToLower().CompareTo("exit") == 0) {
				this.Exit();
			} else if (Command.ToLower().CompareTo("clear") == 0) {
				DevConsole.ClearText();
			} else {
				DevConsole.AddText("Unrecognized Command: " + Command + "\n ");
			}
		}
	}
}