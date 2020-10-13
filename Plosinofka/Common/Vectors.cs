
using System;

namespace Ujeby.Plosinofka.Common
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

		public override string ToString() => $"({ X:0.00}; { Y:0.00})";

		public override bool Equals(object obj)
		{
			return obj is Vector2f && (Vector2f)obj == this;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static Vector2f operator +(Vector2f a, Vector2f b) => new Vector2f(a.X + b.X, a.Y + b.Y);
		public static Vector2f operator +(Vector2f a, Vector2i b) => new Vector2f(a.X + b.X, a.Y + b.Y);
		public static Vector2f operator -(Vector2f a, Vector2f b) => new Vector2f(a.X - b.X, a.Y - b.Y);
		public static Vector2f operator -(Vector2f a, Vector2i b) => new Vector2f(a.X - b.X, a.Y - b.Y);
		public static Vector2f operator *(Vector2f a, double k) => new Vector2f(a.X * k, a.Y * k);
		public static Vector2f operator *(Vector2f a, Vector2f b) => new Vector2f(a.X * b.X, a.Y * b.Y);
		public static Vector2f operator /(Vector2f a, double k) => new Vector2f(a.X / k, a.Y / k);

		public static implicit operator Vector2i(Vector2f v) => new Vector2i((int)v.X, (int)v.Y);

		public static bool operator ==(Vector2f a, Vector2i b) => a.X == b.X && a.Y == b.Y;
		public static bool operator !=(Vector2f a, Vector2i b) => a.X != b.X || a.Y != b.Y;

		internal double Length() => Math.Sqrt(X * X + Y * Y);

		internal Vector2f Normalize() => this * 1 / Length();
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
		public static Vector2f operator /(Vector2i a, Vector2i b) => new Vector2f((double)a.X / b.X, (double)a.Y / b.Y);
	}

	public struct Vector4b
	{
		public byte X;
		public byte Y;
		public byte Z;
		public byte W;

		public Vector4b(Vector4b v) : this(v.X, v.Y, v.Z, v.W)
		{
		}

		public Vector4b(byte x, byte y, byte z, byte w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public Vector4b(byte b) : this(b, b, b, b)
		{
		}

		public override string ToString() => $"0x{ X:x2}{ Y:x2}{ Z:x2}{ W:x2}";
	}
}
