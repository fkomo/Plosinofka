using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;

namespace Ujeby.Plosinofka
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				new Game("Plosinofka", Vector2i.UltraHD2K / 2)
					.Run();
			}
			catch (Exception ex)
			{
				Log.Add(ex.ToString());
			}
		}
	}
}
