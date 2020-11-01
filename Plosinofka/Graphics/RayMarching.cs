using System;
using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Graphics
{
	public class RayMarching
	{
		private const double RAY_MARCH_EPSILON = 0.1;
		private const double RAY_MARCH_MAX_STEPS = 128;

		private static double Distance(ISdf[] sdfs, Vector2f point)
		{
			var tMin = double.PositiveInfinity;
			foreach (var sdf in sdfs)
			{
				var t = sdf.Distance(point);
				if (t < tMin)
					tMin = t;
			}

			return tMin;
		}

		public static double Distance(ISdf[] sdfs, Vector2f origin, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;

			// only raymarch sdf's which are in front of ray
			sdfs = sdfs.Where(sdf =>
			{
				var aabb = sdf.GetAABB();

				if (direction.X < 0 && origin.X < aabb.Left)
					return false;

				if (direction.X > 0 && origin.X > aabb.Right)
					return false;

				if (direction.Y < 0 && origin.Y < aabb.Bottom)
					return false;

				if (direction.Y > 0 && origin.Y > aabb.Top)
					return false;

				return true;
			}).ToArray();

			var tMin = double.PositiveInfinity;
			if (sdfs.Length > 0)
			{
				var t = 0.0;
				for (var step = 0; step < RAY_MARCH_MAX_STEPS; step++)
				{
					var p = origin + direction * t;
					var d = sdfs.Min(c => c.Distance(p));

					if (Math.Abs(d) < RAY_MARCH_EPSILON)
					{
						tMin = t;
						break;
					}

					t += d;
				}
			}

			if (tMin > 0 && !double.IsInfinity(tMin))
			{
				var p = origin + direction * tMin;
				normal = new Vector2f(
					Distance(sdfs, p + Vector2f.Right * RAY_MARCH_EPSILON) - 
					Distance(sdfs, p - Vector2f.Right * RAY_MARCH_EPSILON),
					Distance(sdfs, p + Vector2f.Up * RAY_MARCH_EPSILON) - 
					Distance(sdfs, p - Vector2f.Up * RAY_MARCH_EPSILON))
					.Normalize();
			}

			return tMin;
		}
	}
}
