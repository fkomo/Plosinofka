
using System;
using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Common
{
	public struct AABB : IRayTracing, ISdf
	{
		/// <summary>bottom left - world coordinates</summary>
		public Vector2f Min { get; private set; }

		/// <summary>top right - world coordinates</summary>
		public Vector2f Max { get; private set; }

		public Vector2f HalfSize { get; private set; }
		public Vector2f Center { get; private set; }
		public Vector2f Size => Max - Min;

		public double Top => Max.Y;
		public double Bottom => Min.Y;
		public double Left => Min.X;
		public double Right => Max.X;

		public override string ToString() => $"{ Min }-{ Max }";
		public static AABB operator +(AABB bb, Vector2f v) => new AABB(bb.Min + v, bb.Max + v);

		public AABB(Vector2f min, Vector2f max)
		{
			Min = min;
			Max = max;
			Center = (min + max) * 0.5;
			HalfSize = (max - min) * 0.5;
		}

		public bool Inside(Vector2f p) => Inside(p.X, p.Y);
		public bool Inside(int x, int y) => Inside((double)x, (double)y);
		public bool Inside(double x, double y) => !(x < Left || y < Bottom || x.GrEq(Right) || y.GrEq(Top));

		public bool OutsideOrSurface(Vector2f p) => OutsideOrSurface(p.X, p.Y);
		public bool OutsideOrSurface(double x, double y) => x.LeEq(Left) || y.LeEq(Bottom) || x.GrEq(Right) || y.GrEq(Top);

		public bool Overlap(AABB other)
		{
			if (Math.Abs(Center.X - other.Center.X).GrEq(HalfSize.X + other.HalfSize.X))
				return false;

			if (Math.Abs(Center.Y - other.Center.Y).GrEq(HalfSize.Y + other.HalfSize.Y))
				return false;

			return true;
		}

		public double Trace(Vector2f origin, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;
			var tMin = double.PositiveInfinity;

			// if ray is coming from outside or from surface
			if (OutsideOrSurface(origin))
			{
				if (origin.X.LeEq(Min.X) && direction.X > 0)
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
				else if (origin.X.GrEq(Max.X) && direction.X < 0)
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

				if (origin.Y.LeEq(Min.Y) && direction.Y > 0)
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
				else if (origin.Y.GrEq(Max.Y) && direction.Y < 0)
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

		public bool Intersect(Ray ray, double from = 0, double to = double.PositiveInfinity)
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

		/// <summary>
		/// aabb vs edge (line defined by 2 points)
		/// </summary>
		/// <param name="a">line start</param>
		/// <param name="b">line end</param>
		/// <returns>true if line intersects aabb</returns>
		public bool Intersect(Vector2f a, Vector2f b)
		{
			var e = b - a;

			var te = (Bottom - a.Y) / e.Y;
			if (te > 0 && te.LeEq(1))
			{
				var t = (e.X * te) + a.X;
				if (t.GrEq(Left) && t < Right)
					return true;
			}

			te = (Top - a.Y) / e.Y;
			if (te > 0 && te.LeEq(1))
			{
				var t = (e.X * te) + a.X;
				if (t.GrEq(Left) && t < Right)
					return true;
			}

			te = (Left - a.X) / e.X;
			if (te > 0 && te.LeEq(1))
			{
				var t = (e.Y * te) + a.Y;
				if (t.GrEq(Bottom) && t < Top)
					return true;
			}

			te = (Right - a.X) / e.X;
			if (te > 0 && te.LeEq(1))
			{
				var t = (e.Y * te) + a.Y;
				if (t.GrEq(Bottom) && t < Top)
					return true;
			}

			return false;
		}

		/// <summary>
		/// find nonoverlapping aabboxes in sprite
		/// </summary>
		/// <param name="map"></param>
		public static AABB[] FromMap(Sprite map, uint mask)
		{
			var start = Core.GameLoop.GetElapsed();

			var colliders = new List<AABB>();
			for (var y = 0; y < map.Size.Y; y++)
			{
				for (var x = 0; x < map.Size.X; x++)
				{
					var p = y * map.Size.X + x;

					// skip if point is already in another collider
					if (Find(colliders, x, y, out AABB oldCollider))
						x += (int)oldCollider.Size.X - 1;

					else if ((map.Data[p] & mask) == mask)
					{
						var width = 1;
						var height = 1;

						// find width
						while (x + width < map.Size.X &&
							((map.Data[p + width] & mask) == mask) &&
							!colliders.Any(c => c.Inside(x + width, y)))
							width++;

						// find height
						var cleanRow = true;
						while (y + height < map.Size.Y && cleanRow)
						{
							var offset = p + height * map.Size.X;
							for (var i = 0; i < width; i++)
								if (((map.Data[offset + i] & mask) != mask) ||
									colliders.Any(c => c.Inside(x + i, y + height)))
								{
									cleanRow = false;
									break;
								}

							if (cleanRow)
								height++;
						}

						// new colider
						var min = new Vector2f(x, y);
						colliders.Add(new AABB(min, min + new Vector2f(width, height)));

						// advance just after collider
						x += width - 1;
					}
				}
			}

			var elapsed = (int)(Core.GameLoop.GetElapsed() - start);
			Log.Add($"AABB.FromMap('{ map.Filename }', mask:0x{ mask:x}): { colliders.Count } in { elapsed }ms");

			return colliders.ToArray();
		}

		private static bool Find(List<AABB> aabbs, int x, int y, out AABB match)
		{
			match = default;

			foreach (var aabb in aabbs)
				if (aabb.Inside(x, y))
				{
					match = aabb;
					return true;
				}

			return false;
		}

		/// <summary>
		/// returns new aabb that contains all the aabbs from input
		/// </summary>
		/// <param name="aABBs"></param>
		/// <returns></returns>
		public static AABB Merge(AABB[] aABBs)
		{
			return new AABB(
				new Vector2f(aABBs.Min(bb => bb.Min.X), aABBs.Min(bb => bb.Min.Y)),
				new Vector2f(aABBs.Max(bb => bb.Max.X), aABBs.Max(bb => bb.Max.Y)));
		}

		public static AABB Overlap(AABB aabb1, AABB aabb2)
		{
			var min = Vector2f.Zero;
			var max = Vector2f.Zero;

			if (aabb1.Overlap(aabb2))
			{
				if (aabb2.Left < aabb1.Left)
				{
					var tmp = aabb1;
					aabb1 = aabb2;
					aabb2 = tmp;
				}

				min.X = aabb2.Left;
				max.X = Math.Min(aabb1.Right, aabb2.Right);

				if (aabb2.Bottom < aabb1.Bottom)
				{
					var tmp = aabb1;
					aabb1 = aabb2;
					aabb2 = tmp;
				}

				min.Y = aabb2.Bottom;
				max.Y = Math.Min(aabb1.Top, aabb2.Top);
			}

			return new AABB(min, max);
		}

		/// <summary>
		/// point vs 4 aabb planes
		/// each bit (4) represents point side on one plane, 1 == outside, 0 == inside
		/// so when result == 0, the point is inside of aabb
		/// </summary>
		/// <param name="cv">point centered to aabb (v - aabb.center)</param>
		/// <returns>(-y | y | -x | x)</returns>
		public int PointPosition(Vector2f cv)
		{
			var result = 0;

			if (cv.X.GrEq(HalfSize.X)) result |= 1;
			if (cv.X.LeEq(-HalfSize.X)) result |= 2;
			if (cv.Y.GrEq(HalfSize.Y)) result |= 4;
			if (cv.Y.LeEq(-HalfSize.Y)) result |= 8;

			return result;
		}

		public AABB GetAABB() => this;
	}
}
