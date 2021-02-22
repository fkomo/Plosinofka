using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Entities;
using Ujeby.Plosinofka.Game.PlayerStates;

namespace Ujeby.Plosinofka.Game.Graphics
{
	public class Sprites
	{
		/// <summary>
		/// spriteId vs image path
		/// </summary>
		public static readonly Dictionary<string, string> LibraryFileMap = new Dictionary<string, string>()
		{
			{ PlayerDecals.DustParticlesLeft.ToString() , Program.ContentDirectory + "Effects\\DustParticlesLeft.png" },
			{ PlayerDecals.DustParticlesRight.ToString() , Program.ContentDirectory + "Effects\\DustParticlesRight.png" },

			{ PlayerAnimations.Idle.ToString() , Program.ContentDirectory + "Player\\player-idle.png" },
			{ PlayerAnimations.WalkingLeft.ToString() , Program.ContentDirectory + "Player\\player-walkingLeft.png" },
			{ PlayerAnimations.WalkingRight.ToString() , Program.ContentDirectory + "Player\\player-walkingRight.png" },
		};
	}
}
