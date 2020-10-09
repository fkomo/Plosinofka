﻿using System;

namespace Ujeby.Plosinofka
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				new Game("Plosinofka")
					.Run();
			}
			catch (Exception ex)
			{
				Log.Add(ex.ToString());
			}
		}
	}
}
