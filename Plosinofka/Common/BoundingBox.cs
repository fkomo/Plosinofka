
using System;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Common
{
	public struct BoundingBox : IRayTracable, ISdf
	{
		/// <summary>bottom left - world coordinates</summary>
		public Vector2f Min { get; private set; }

		/// <summary>top right - world coordinates</summary>
		public Vector2f Max { get; private set; }

		private Vector2f HalfSize;
		public Vector2f Center { get; private set; }

		public Vector2f Size => Max - Min;

		public BoundingBox(Vector2f min, Vector2f max)
		{
			Min = min;
			Max = max;
			Center = (min + max) * 0.5;
			HalfSize = (max - min) * 0.5;
		}

		public double Top => Max.Y;
		public double Bottom => Min.Y;
		public double Left => Min.X;
		public double Right => Max.X;

		public bool IsIn(Vector2f p) => IsIn(p.X, p.Y);
		public bool IsIn(int x, int y) => IsIn((double)x, (double)y);
		public bool IsIn(double x, double y) => !(x < Left || y < Bottom || x >= Right || y >= Top);

		public bool IsOutOrOnSurface(Vector2f p) => IsOutOrOnSurface(p.X, p.Y);
		public bool IsOutOrOnSurface(double x, double y) => (x <= Left || y <= Bottom || x >= Right || y >= Top);

		public override string ToString() => $"{ Min }-{ Max }";

		public bool Overlaps(BoundingBox other)
		{
			// above
			if (other.Min.Y > Max.Y)
				return false;

			// bellow
			if (other.Max.Y < Min.Y)
				return false;

			// left
			if (other.Max.X < Min.X)
				return false;

			// right
			if (other.Min.X > Max.X)
				return false;

			return true;
		}

		public bool Overlaps2(BoundingBox other)
		{
			if (Math.Abs(Center.X - other.Center.X) > HalfSize.X + other.HalfSize.X)
				return false;
			if (Math.Abs(Center.Y - other.Center.Y) > HalfSize.Y + other.HalfSize.Y)
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
				if (origin.X <= Min.X && direction.X > 0)
				{
					var t = (-origin.X + Math.Abs(Min.X)) / direction.X;
					if (t < tMin && !double.IsInfinity(t))
					{
						var hit = origin.Y + direction.Y * t;
						if (hit > Min.Y && hit < Max.Y)
						{
							tMin = t;
							normal = Vector2f.Left;
						}
					}
				}
				else if (origin.X >= Max.X && direction.X < 0)
				{
					var t = (-origin.X + Math.Abs(Max.X)) / direction.X;
					if (t < tMin && !double.IsInfinity(t))
					{
						var hit = origin.Y + direction.Y * t;
						if (hit > Min.Y && hit < Max.Y)
						{
							tMin = t;
							normal = Vector2f.Right;
						}
					}
				}

				if (origin.Y <= Min.Y && direction.Y > 0)
				{
					var t = (-origin.Y + Math.Abs(Min.Y)) / direction.Y;
					if (t < tMin && !double.IsInfinity(t))
					{
						var hit = origin.X + direction.X * t;
						if (hit > Min.X && hit < Max.X)
						{
							tMin = t;
							normal = Vector2f.Down;
						}
					}
				}
				else if (origin.Y >= Max.Y && direction.Y < 0)
				{
					var t = (-origin.Y + Math.Abs(Max.Y)) / direction.Y;
					if (t < tMin && !double.IsInfinity(t))
					{
						var hit = origin.X + direction.X * t;
						if (hit > Min.X && hit < Max.X)
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
					var t = (-origin.X + Max.X) / direction.X;
					if (t < tMin)
					{
						var hit = origin.Y + direction.Y * t;
						if (hit > Min.Y && hit < Max.Y)
						{
							tMin = t;
							normal = Vector2f.Left;
						}
					}
				}
				else
				{
					var t = (-origin.X + Min.X) / direction.X;
					if (t < tMin)
					{
						var hit = origin.Y + direction.Y * t;
						if (hit > Min.Y && hit < Max.Y)
						{
							tMin = t;
							normal = Vector2f.Right;
						}
					}
				}

				if (direction.Y > 0)
				{
					var t = (-origin.Y + Max.Y) / direction.Y;
					if (t < tMin)
					{
						var hit = origin.X + direction.X * t;
						if (hit > Min.X && hit < Max.X)
						{
							tMin = t;
							normal = Vector2f.Down;
						}
					}
				}
				else
				{
					var t = (-origin.Y + Min.Y) / direction.Y;
					if (t < tMin)
					{
						var hit = origin.X + direction.X * t;
						if (hit > Min.X && hit < Max.X)
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
			var q = (p - (Min + HalfSize)).Abs() - HalfSize;
			return q.Max(Vector2f.Zero).Length() + Math.Min(Math.Max(q.X, q.Y), 0.0);
		}

		public bool Intersects(Ray ray, double from = 0, double to = double.PositiveInfinity)
		{
			// "An Efficient and Robust Ray-Box Intersection Algorithm"
			// Journal of graphics tools, 10(1):49-54, 2005
			// Amy Williams, Steve Barrus, R. Keith Morley, and Peter Shirley
			// http://jgt.akpeters.com/papers/WilliamsEtAl05/

			//ray.Origin = Center - ray.Origin;
			//var minCorner = HalfSize;
			//minCorner.X = -minCorner.X;
			//minCorner.Y = -minCorner.Y;
			//var maxCorner = HalfSize;

			//var tmin = ((ray.Sign.X == 0 ? minCorner.X : maxCorner.X) + ray.Origin.X) * ray.InvDirection.X;
			//var tmax = ((ray.Sign.X == 1 ? minCorner.X : maxCorner.X) + ray.Origin.X) * ray.InvDirection.X;
			//var tymin = ((ray.Sign.Y == 0 ? minCorner.Y : maxCorner.Y) + ray.Origin.Y) * ray.InvDirection.Y;
			//var tymax = ((ray.Sign.Y == 1 ? minCorner.Y : maxCorner.Y) + ray.Origin.Y) * ray.InvDirection.Y;

			//if (double.IsNaN(tymin))
			//	tymin = 0;

			//if (double.IsNaN(tymax))
			//	tymax = 0;

			//if ((tmin > tymax) || (tymin > tmax))
			//	return false;

			//else
			//{
			//	if (tymin > tmin)
			//		tmin = tymin;

			//	if (tymax < tmax)
			//		tmax = tymax;
			//}

			//return (tmin < to) && (tmax > from);

			// Fast, Branchless Ray/Bounding Box Intersections
			// https://tavianator.com/2015/ray_box_nan.html

			var minCorner = Min;
			var maxCorner = Max;

			var tmin = double.NegativeInfinity;
			var tmax = double.PositiveInfinity;

			var t1 = (minCorner.X - ray.Origin.X) * ray.InvDirection.X;
			var t2 = (maxCorner.X - ray.Origin.X) * ray.InvDirection.X;
			tmin = Math.Max(tmin, Math.Min(t1, t2));
			tmax = Math.Min(tmax, Math.Max(t1, t2));

			t1 = (minCorner.Y - ray.Origin.Y) * ray.InvDirection.Y;
			t2 = (maxCorner.Y - ray.Origin.Y) * ray.InvDirection.Y;
			tmin = Math.Max(tmin, Math.Min(t1, t2));
			tmax = Math.Min(tmax, Math.Max(t1, t2));

			return tmax > Math.Max(tmin, 0.0) && ((from < tmin && tmin < to) || (from < tmax && tmax < to));
		}
	}
}
