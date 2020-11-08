
using System;

namespace Ujeby.Plosinofka.Engine.Common
{
	public struct Vector2f
	{
		public double X;
		public double Y;

		public static Vector2f Up = new Vector2f(0, 1);
		public static Vector2f Down = new Vector2f(0, -1);
		public static Vector2f Left = new Vector2f(-1, 0);
		public static Vector2f Right = new Vector2f(1, 0);
		public static Vector2f Zero = new Vector2f(0);

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

		public override string ToString() => $"[{ X:0.00}; { Y:0.00}]";
		public override bool Equals(object obj) => obj is Vector2f v && this == v;
		public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

		public static Vector2f operator +(Vector2f a, Vector2f b) => new Vector2f(a.X + b.X, a.Y + b.Y);
		public static Vector2f operator +(Vector2f a, Vector2i b) => new Vector2f(a.X + b.X, a.Y + b.Y);
		public static Vector2f operator -(Vector2f a, Vector2f b) => new Vector2f(a.X - b.X, a.Y - b.Y);
		public static Vector2f operator -(Vector2f a, Vector2i b) => new Vector2f(a.X - b.X, a.Y - b.Y);
		public static Vector2f operator *(Vector2f a, double k) => new Vector2f(a.X * k, a.Y * k);
		public static Vector2f operator *(Vector2f a, Vector2f b) => new Vector2f(a.X * b.X, a.Y * b.Y);
		public static Vector2f operator /(Vector2f a, double k) => new Vector2f(a.X / k, a.Y / k);
		public static Vector2f operator /(Vector2f a, Vector2f b) => new Vector2f(a.X / b.X, a.Y / b.Y);

		public static Vector2f operator +(Vector2f a, double k) => new Vector2f(a.X + k, a.Y + k);
		public static Vector2f operator -(Vector2f a, double k) => new Vector2f(a.X - k, a.Y - k);

		public static bool operator ==(Vector2f a, Vector2f b) => a.X.Eq(b.X) && a.Y.Eq(b.Y);
		public static bool operator !=(Vector2f a, Vector2f b) => !(a == b);

		public double Length() => Math.Sqrt(X * X + Y * Y);

		public static double Dot(Vector2f v1, Vector2f v2) => v1.X * v2.X + v1.Y * v2.Y;

		public Vector2f Normalize() => this * 1.0 / Length();
		public Vector2f Abs() => new Vector2f(Math.Abs(X), Math.Abs(Y));
		public Vector2f Min(Vector2f v) => new Vector2f(Math.Min(X, v.X), Math.Min(Y, v.Y));
		public Vector2f Max(Vector2f v) => new Vector2f(Math.Max(X, v.X), Math.Max(Y, v.Y));
		public Vector2f Inv() => new Vector2f(-X, -Y);

		public static Vector2f Min(Vector2f v1, Vector2f v2) => new Vector2f(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y));
		public static Vector2f Max(Vector2f v1, Vector2f v2) => new Vector2f(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y));

		public Vector2i Trunc() => new Vector2i((int)X, (int)Y);
		public Vector2i Round() => new Vector2i((int)Math.Round(X), (int)Math.Round(Y));
	}

	public struct Vector2i
	{
		public int X;
		public int Y;

		public static Vector2i Up = new Vector2i(0, 1);
		public static Vector2i Down = new Vector2i(0, -1);
		public static Vector2i Left = new Vector2i(-1, 0);
		public static Vector2i Right = new Vector2i(1, 0);
		public static Vector2i Zero = new Vector2i(0);

		public static Vector2i FullHD = new Vector2i(1920, 1080);
		public static Vector2i UltraHD2K = new Vector2i(2560, 1440);
		public static Vector2i UltraHD4K = FullHD * 2;

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

		public override string ToString() => $"[{ X }; { Y }]";
		public override bool Equals(object obj) => obj is Vector2i v && this == v;
		public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

		public void Set(int x, int y) => (X, Y) = (x, y);

		public int Area() => X * Y;

		public static Vector2i Min(Vector2i v1, Vector2i v2) => new Vector2i(Math.Min(v1.X, v2.X), Math.Min(v1.Y, v2.Y));
		public static Vector2i Max(Vector2i v1, Vector2i v2) => new Vector2i(Math.Max(v1.X, v2.X), Math.Max(v1.Y, v2.Y));

		public static Vector2i operator +(Vector2i a, Vector2i b) => new Vector2i(a.X + b.X, a.Y + b.Y);
		public static Vector2i operator -(Vector2i a, Vector2i b) => new Vector2i(a.X - b.X, a.Y - b.Y);
		public static Vector2i operator *(Vector2i a, Vector2i b) => new Vector2i(a.X * b.X, a.Y * b.Y);
		public static Vector2i operator *(Vector2i a, double k) => new Vector2i((int)(a.X * k), (int)(a.Y * k));
		public static Vector2i operator /(Vector2i a, double k) => new Vector2i((int)(a.X / k), (int)(a.Y / k));
		public static Vector2f operator /(Vector2i a, Vector2i b) => new Vector2f((double)a.X / b.X, (double)a.Y / b.Y);

		public static bool operator ==(Vector2i a, Vector2i b) => a.X == b.X && a.Y == b.Y;
		public static bool operator !=(Vector2i a, Vector2i b) => !(a == b);

		public static implicit operator Vector2f(Vector2i v) => new Vector2f(v.X, v.Y);
	}
}
