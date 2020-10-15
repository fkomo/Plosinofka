
using System;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Common
{
	public struct BoundingBox : IRayTracable, ISdf
	{
		/// <summary>bottom left</summary>
		public Vector2f Position;
		public Vector2f Size;

		public double Top => Position.Y + Size.Y;
		public double Bottom => Position.Y;
		public double Left => Position.X;
		public double Right => Position.X + Size.X;

		public bool IsIn(Vector2f p) => IsIn(p.X, p.Y);
		public bool IsIn(int x, int y) => IsIn((double)x, (double)y);
		public bool IsIn(double x, double y) => !(x < Left || y < Bottom || x >= Right || y >= Top);

		public bool IsOutOrOnSurface(Vector2f p) => IsOutOrOnSurface(p.X, p.Y);
		public bool IsOutOrOnSurface(double x, double y) => (x <= Left || y <= Bottom || x >= Right || y >= Top);

		public override string ToString() => $"{ Position }x{ Size }";

		public bool IsOverlapping(BoundingBox box) => IsOverlapping(box.Position, box.Size);

		public bool IsOverlapping(Vector2f position, Vector2f size)
		{
			// above
			if (position.Y > Position.Y + Size.Y)
				return false;

			// bellow
			if (position.Y + size.Y < Position.Y)
				return false;

			// left
			if (position.X + size.X < Position.X)
				return false;

			// right
			if (position.X > Position.X + Size.X)
				return false;

			return true;
		}

		public double Trace(Vector2f origin, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;
			var tMin = double.PositiveInfinity;

			// if ray is coming from outside or from surface
			if (IsOutOrOnSurface(origin))
			{
				if (origin.X <= Position.X && direction.X > 0)
				{
					var t = (-origin.X + Math.Abs(Position.X)) / direction.X;
					if (t < tMin && !double.IsInfinity(t))
					{
						var hit = origin.Y + direction.Y * t;
						if (hit > Position.Y && hit < Position.Y + Size.Y)
						{
							tMin = t;
							normal = Vector2f.Left;
						}
					}
				}
				else if (origin.X >= Position.X + Size.X && direction.X < 0)
				{
					var t = (-origin.X + Math.Abs(Position.X + Size.X)) / direction.X;
					if (t < tMin && !double.IsInfinity(t))
					{
						var hit = origin.Y + direction.Y * t;
						if (hit > Position.Y && hit < Position.Y + Size.Y)
						{
							tMin = t;
							normal = Vector2f.Right;
						}
					}
				}

				if (origin.Y <= Position.Y && direction.Y > 0)
				{
					var t = (-origin.Y + Math.Abs(Position.Y)) / direction.Y;
					if (t < tMin && !double.IsInfinity(t))
					{
						var hit = origin.X + direction.X * t;
						if (hit > Position.X && hit < Position.X + Size.X)
						{
							tMin = t;
							normal = Vector2f.Down;
						}
					}
				}
				else if (origin.Y >= Position.Y + Size.Y && direction.Y < 0)
				{
					var t = (-origin.Y + Math.Abs(Position.Y + Size.Y)) / direction.Y;
					if (t < tMin && !double.IsInfinity(t))
					{
						var hit = origin.X + direction.X * t;
						if (hit > Position.X && hit < Position.X + Size.X)
						{
							tMin = t;
							normal = Vector2f.Up;
						}
					}
				}
			}
			else
			{
				// ray from inside
				if (direction.X > 0)
				{
					var t = (-origin.X + Right) / direction.X;
					if (t < tMin)
					{
						var hit = origin.Y + direction.Y * t;
						if (hit > Bottom && hit < Top)
						{
							tMin = t;
							normal = Vector2f.Left;
						}
					}
				}
				else
				{
					var t = (-origin.X + Left) / direction.X;
					if (t < tMin)
					{
						var hit = origin.Y + direction.Y * t;
						if (hit > Bottom && hit < Top)
						{
							tMin = t;
							normal = Vector2f.Right;
						}
					}
				}

				if (direction.Y > 0)
				{
					var t = (-origin.Y + Top) / direction.Y;
					if (t < tMin)
					{
						var hit = origin.X + direction.X * t;
						if (hit > Left && hit < Right)
						{
							tMin = t;
							normal = Vector2f.Down;
						}
					}
				}
				else
				{
					var t = (-origin.Y + Bottom) / direction.Y;
					if (t < tMin)
					{
						var hit = origin.X + direction.X * t;
						if (hit > Left && hit < Right)
						{
							tMin = t;
							normal = Vector2f.Up;
						}
					}
				}
			}

			if (double.IsInfinity(tMin))
				normal = Vector2f.Zero;

			return tMin;
		}

		public double Distance(Vector2f p)
		{
			var size2 = Size * 0.5;
			var q = (p - (Position + size2)).Abs() - size2;
			return q.Max(Vector2f.Zero).Length() + Math.Min(Math.Max(q.X, q.Y), 0.0);
		}
	}
}
