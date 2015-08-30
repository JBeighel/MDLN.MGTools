using MDLN.MGTools;
using MDLN.Tools;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
//using System.Collections.Generic;
//using System.ComponentModel;

namespace MDNLN.CardTable {
	class Launcher {
		public static void Main(string[] args) {
			using (var game = new CardTable())
			{
				game.Run();
			}
		}
	}

	public class CardTable : Game {
		private GraphicsDeviceManager cGraphDevMgr;

		public CardTable() {
			cGraphDevMgr = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}
	}
}
