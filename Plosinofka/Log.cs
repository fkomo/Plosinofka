using System;
using System.Drawing;
using SDL2;

namespace Ujeby.Plosinofka
{
	class Log
	{
		public static string Add(string message)
		{
			var line = $"{ DateTime.Now:HH:mm:ss.fff}: { message }";
			Console.WriteLine(line);
			return line;
		}
	}
}
