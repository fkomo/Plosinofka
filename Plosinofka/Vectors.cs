
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
		public static Vector2f operator *(Vector2f a, double k) => new Vector2f(a.X * k, a.Y * k);
	}
}
