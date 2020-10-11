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
				new Game("Plosinofka", new Vector2i(1920, 1080))
					.Run();
			}
			catch (Exception ex)
			{
				Log.Add(ex.ToString());
			}
		}
	}
}
