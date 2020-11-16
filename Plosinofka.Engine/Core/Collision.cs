using System;
using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Engine.Core
{
	public class Collisions
	{
		public static bool Overlap(AABB box, AABB[] obstacles)
		{
			foreach (var bb in obstacles)
				if (bb.Overlap(box))
					return true;

			return false;
		}

		public static bool Intersect(Ray ray, AABB[] obstacles, double from = 0, double to = double.PositiveInfinity)
		{
			foreach (var obstacle in obstacles)
				if (obstacle.Intersect(ray, from, to))
					return true;

			return false;
		}

		public static double Trace(Vector2f origin, Vector2f direction, AABB[] obstacles, out Vector2f normal)
		{
			normal = Vector2f.Zero;

			var tMin = double.PositiveInfinity;
			foreach (var bb in obstacles)
			{
				var t = bb.Trace(origin, direction, out Vector2f n);
				if (t < tMin)
				{
					tMin = t;
					normal = n;
				}
			}

			return tMin;
		}

		public static double Trace(AABB aabb, Vector2f move, AABB[] obstacles, out Vector2f normal)
		{
			normal = Vector2f.Zero;
			var tMin = double.PositiveInfinity;

			// no movement, nothing to trace
			if (move.X.Eq(0) && move.Y.Eq(0))
				return tMin;

			var movedAabb = aabb + move;
			if (move.X.Eq(0) || move.Y.Eq(0))
			{
				var path = new AABB(
					new Vector2f(Math.Min(aabb.Right, movedAabb.Left), Math.Min(aabb.Top, movedAabb.Bottom)),
					new Vector2f(Math.Max(aabb.Left, movedAabb.Right), Math.Max(aabb.Bottom, movedAabb.Top)));

				var obstaclesInPath = obstacles.Where(o => o.Overlap(path)).ToArray();
				if (obstaclesInPath.Length != 0)
				{
					normal = move.Inv().Normalize();

					// find closest obstacle in path
					if (move.X > 0)
						tMin = Math.Abs(obstaclesInPath.Min(o => o.Left) - aabb.Right);
					else if (move.X < 0)
						tMin = Math.Abs(obstaclesInPath.Max(o => o.Right) - aabb.Left);
					else if (move.Y > 0)
						tMin = Math.Abs(obstaclesInPath.Min(o => o.Bottom) - aabb.Top);
					else if (move.Y < 0)
						tMin = Math.Abs(obstaclesInPath.Max(o => o.Top) - aabb.Bottom);
				}
			}
			else
			{
				var v1 = aabb.Min;
				var v2 = aabb.Max;
				var v3 = movedAabb.Min;
				var v4 = movedAabb.Max;
				if (move.X * move.Y > 0)
				{
					v1 = new Vector2f(aabb.Right, aabb.Bottom);
					v2 = new Vector2f(aabb.Left, aabb.Top);
					v3 = new Vector2f(movedAabb.Right, movedAabb.Bottom);
					v4 = new Vector2f(movedAabb.Left, movedAabb.Top);
				}

				var t1 = new Triangle(v1, v3, v4);
				var t2 = new Triangle(v1, v2, v4);

				var obstaclesInPath = obstacles.Where(o => movedAabb.Overlap(o) || t1.Overlap(o) || t2.Overlap(o))
					.ToArray();
				if (obstaclesInPath.Length != 0)
				{
					tMin = double.NegativeInfinity;

					if (move.X > 0)
					{
						var tBefore = tMin;
						tMin = GetTMin(tMin,
							obstaclesInPath.Where(o => o.Left.GrEq(aabb.Right))
								.Select(o => (o.Left - aabb.Right) / (movedAabb.Right - aabb.Right)),
							aabb, move, obstaclesInPath);
						if (tMin > tBefore)
							normal = Vector2f.Left;
					}
					else
					{
						var tBefore = tMin;
						tMin = GetTMin(tMin,
							obstaclesInPath.Where(o => o.Right.LeEq(aabb.Left))
								.Select(o => (o.Right - aabb.Left) / (movedAabb.Right - aabb.Right)),
							aabb, move, obstaclesInPath);
						if (tMin > tBefore)
							normal = Vector2f.Right;
					}

					if (move.Y > 0)
					{
						var tBefore = tMin;
						tMin = GetTMin(tMin,
							obstaclesInPath.Where(o => o.Bottom.GrEq(aabb.Top))
								.Select(o => (o.Bottom - aabb.Top) / (movedAabb.Top - aabb.Top)),
							aabb, move, obstaclesInPath);
						if (tMin > tBefore)
							normal = Vector2f.Down;
					}
					else
					{
						var tBefore = tMin;
						tMin = GetTMin(tMin,
							obstaclesInPath.Where(o => o.Top.LeEq(aabb.Bottom))
								.Select(o => (o.Top - aabb.Bottom) / (movedAabb.Top - aabb.Top)),
							aabb, move, obstaclesInPath);
						if (tMin > tBefore)
							normal = Vector2f.Up;
					}

					if (tMin < 0)
						// this should never happen
						tMin = double.PositiveInfinity;

					else
						tMin *= move.Length();
				}
			}

			return tMin;
		}

		private static double GetTMin(double tMin, IEnumerable<double> tArray, AABB aabb, Vector2f move, AABB[] obstacles)
		{
			var tOrdered = tArray.OrderBy(t => t).ToArray();
			for (var i = 0; i < tOrdered.Length; i++)
			{
				var t = tOrdered[i];
				if (t < tMin)
					continue;

				t = Math.Max(t, 0);
				var newAabb = aabb + move * t;
				if (obstacles.Any(o => o.Overlap(newAabb)))
					break;

				tMin = t;
			}

			return tMin;
		}
	}
}
