using System;

namespace Ujeby.Plosinofka.Engine.Common
{
	public class Log
	{
		public static string Add(string message)
		{
			var line = $"{ DateTime.Now:HH:mm:ss.fff}: { message }";
			Console.WriteLine(line);
			return line;
		}
	}
}
