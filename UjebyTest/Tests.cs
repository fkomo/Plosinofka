using System.Collections.Generic;
using Ujeby.Plosinofka.Common;

namespace UjebyTest
{
	public class Vector2f_AsClass
	{
		public double X;
		public double Y;

		public Vector2f_AsClass(Vector2f_AsClass v) : this(v.X, v.Y)
		{
		}

		public Vector2f_AsClass(double x, double y)
		{
			X = x;
			Y = y;
		}

		public static Vector2f_AsClass operator +(Vector2f_AsClass a, Vector2f_AsClass b) => new Vector2f_AsClass(a.X + b.X, a.Y + b.Y);
		public static Vector2f_AsClass operator *(Vector2f_AsClass a, double k) => new Vector2f_AsClass(a.X * k, a.Y * k);
	}

	public class Tests
	{
		private static void ClassVsStruct(int n)
		{
			var cList = new List<Vector2f_AsClass>();
			var sList = new List<Vector2f>();

			var start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				cList.Add(new Vector2f_AsClass(Program.Rng.NextDouble(), Program.Rng.NextDouble()));
			Log.Add($"class#fill: { (Program.Elapsed() - start):0.000}ms");

			start = Program.Elapsed();
			for (var i = 0; i < n; i++)
				sList.Add(new Vector2f(Program.Rng.NextDouble(), Program.Rng.NextDouble()));
			Log.Add($"struct#fill: { (Program.Elapsed() - start):0.000}ms");

			start = Program.Elapsed();
			for (var i = 1; i < n; i++)
				cList[i - 1] = cList[i] * cList[i - 1].X;
			Log.Add($"class#*k: { (Program.Elapsed() - start):0.000}ms");

			start = Program.Elapsed();
			for (var i = 1; i < n; i++)
				sList[i - 1] = sList[i] * sList[i - 1].X;
			Log.Add($"struct#*k: { (Program.Elapsed() - start):0.000}ms");

			start = Program.Elapsed();
			for (var i = 1; i < n; i++)
				cList[i - 1] = cList[i] + cList[i - 1];
			Log.Add($"class#i1+i2: { (Program.Elapsed() - start):0.000}ms");

			start = Program.Elapsed();
			for (var i = 1; i < n; i++)
				sList[i - 1] = sList[i] + sList[i - 1];
			Log.Add($"struct#i1+i2: { (Program.Elapsed() - start):0.000}ms");
		}
	}
}
