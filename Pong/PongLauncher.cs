using System;

namespace MDLN.Pong {
	class PongLauncher {
		static void Main(string[] args) {
			using (var game = new PongGame()) {
				game.Run();
			}
		}
	}
}
