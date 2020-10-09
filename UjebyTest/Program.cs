using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UjebyTest
{
	public class Vector2f
	{
		public double X = 0.0;
		public double Y = 0.0;

		public Vector2f()
		{
		}

		public Vector2f(Vector2f v) : this(v.X, v.Y)
		{
		}

		public Vector2f(double x, double y)
		{
			X = x;
			Y = y;
		}

		public Vector2f(double d) : this(d, d)
		{
		}

		public override string ToString() => $"{ X:0.00}, { Y:0.00}";
		public static Vector2f operator +(Vector2f a, Vector2f b) => new Vector2f(a.X + b.X, a.Y + b.Y);
		public static Vector2f operator *(Vector2f a, double k) => new Vector2f(a.X * k, a.Y * k);
	}

	public struct Vector2f_s
	{
		public double X;
		public double Y;

		public Vector2f_s(Vector2f_s v) : this(v.X, v.Y)
		{
		}

		public Vector2f_s(double x, double y)
		{
			X = x;
			Y = y;
		}

		public Vector2f_s(double d) : this(d, d)
		{
		}

		public override string ToString() => $"{ X:0.00}, { Y:0.00}";
		public static Vector2f_s operator +(Vector2f_s a, Vector2f_s b) => new Vector2f_s(a.X + b.X, a.Y + b.Y);
		public static Vector2f_s operator *(Vector2f_s a, double k) => new Vector2f_s(a.X * k, a.Y * k);
	}

	class Program
	{
		private static Random rng = new Random();
		private static Stopwatch sw = new Stopwatch();
		private const long count = 10000000;

		static void Main(string[] args)
		{
			//ClassVsStruct();

			var sList = new List<Vector2f_s>();

			sw.Restart();
			for (var i = 0; i < count; i++)
				sList.Add(new Vector2f_s(rng.NextDouble() * -1.0, rng.NextDouble() * -1.0));
			sw.Stop();
			Console.WriteLine($"sList# *-1: { sw.ElapsedMilliseconds }ms");

			sw.Restart();
			for (var i = 0; i < count; i++)
				sList.Add(new Vector2f_s(-rng.NextDouble(), -rng.NextDouble()));
			sw.Stop();
			Console.WriteLine($"sList# -: { sw.ElapsedMilliseconds }ms");
		}

		private static void ClassVsStruct()
		{
			var cList = new List<Vector2f>();
			var sList = new List<Vector2f_s>();

			sw.Restart();
			for (var i = 0; i < count; i++)
				cList.Add(new Vector2f(rng.NextDouble(), rng.NextDouble()));
			sw.Stop();
			Console.WriteLine($"cList#Fill: { sw.ElapsedMilliseconds }ms");

			sw.Restart();
			for (var i = 0; i < count; i++)
				sList.Add(new Vector2f_s(rng.NextDouble(), rng.NextDouble()));
			sw.Stop();
			Console.WriteLine($"sList#Fill: { sw.ElapsedMilliseconds }ms");

			sw.Restart();
			for (var i = 1; i < count; i++)
				cList[i - 1] = cList[i] * cList[i - 1].X;
			sw.Stop();
			Console.WriteLine($"cList# *k: { sw.ElapsedMilliseconds }ms");

			sw.Restart();
			for (var i = 1; i < count; i++)
				sList[i - 1] = sList[i] * sList[i - 1].X;
			sw.Stop();
			Console.WriteLine($"sList# *k: { sw.ElapsedMilliseconds }ms");

			sw.Restart();
			for (var i = 1; i < count; i++)
				cList[i - 1] = cList[i] + cList[i - 1];
			sw.Stop();
			Console.WriteLine($"cList# i1+i2: { sw.ElapsedMilliseconds }ms");

			sw.Restart();
			for (var i = 1; i < count; i++)
				sList[i - 1] = sList[i] + sList[i - 1];
			sw.Stop();
			Console.WriteLine($"sList# i1+i2: { sw.ElapsedMilliseconds }ms");
		}
	}
}
