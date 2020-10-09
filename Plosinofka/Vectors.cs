
namespace Ujeby.Plosinofka
{
	public struct Vector2f
	{
		public double X;
		public double Y;

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

		public override string ToString() => $"({ X:0.00}; { Y:0.00})";
		public static Vector2f operator +(Vector2f a, Vector2f b) => new Vector2f(a.X + b.X, a.Y + b.Y);
		public static Vector2f operator -(Vector2f a, Vector2f b) => new Vector2f(a.X - b.X, a.Y - b.Y);
		public static Vector2f operator -(Vector2f a, Vector2i b) => new Vector2f(a.X - b.X, a.Y - b.Y);
		public static Vector2f operator *(Vector2f a, double k) => new Vector2f(a.X * k, a.Y * k);
		public static Vector2f operator /(Vector2f a, double k) => new Vector2f(a.X / k, a.Y / k);

		public static implicit operator Vector2i(Vector2f v) => new Vector2i((int)v.X, (int)v.Y);
	}

	public struct Vector2i
	{
		public int X;
		public int Y;

		public Vector2i(Vector2i v) : this(v.X, v.Y)
		{
		}

		public Vector2i(int x, int y)
		{
			X = x;
			Y = y;
		}

		public Vector2i(int d) : this(d, d)
		{
		}

		public override string ToString() => $"({ X }; { Y })";
		public static Vector2i operator +(Vector2i a, Vector2i b) => new Vector2i(a.X + b.X, a.Y + b.Y);
		public static Vector2i operator -(Vector2i a, Vector2i b) => new Vector2i(a.X - b.X, a.Y - b.Y);
		public static Vector2i operator *(Vector2i a, double k) => new Vector2i((int)(a.X * k), (int)(a.Y * k));
		public static Vector2i operator /(Vector2i a, double k) => new Vector2i((int)(a.X / k), (int)(a.Y / k));
	}
}
