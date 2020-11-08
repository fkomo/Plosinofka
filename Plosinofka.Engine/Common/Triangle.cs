using System;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Common
{
	public struct Triangle : IRayTracing, ISdf
	{
		public Vector2f V1;
		public Vector2f V2;
		public Vector2f V3;

		public Triangle(Vector2f v1, Vector2f v2, Vector2f v3)
		{
			V1 = v1;
			V2 = v2;
			V3 = v3;
		}

		public double Distance(Vector2f p)
		{
			throw new NotImplementedException();
		}

		public AABB GetAABB() => new AABB(
			new Vector2f(Math.Min(Math.Min(V1.X, V2.X), V3.X), Math.Min(Math.Min(V1.Y, V2.Y), V3.Y)),
			new Vector2f(Math.Max(Math.Max(V1.X, V2.X), V3.X), Math.Max(Math.Max(V1.Y, V2.Y), V3.Y)));

		/// <summary>
		/// https://www.youtube.com/watch?v=HYAgJN3x4GA
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public bool Inside(Vector2f v)
		{
			var s1 = V3.Y - V1.Y;
			var s2 = V3.X - V1.X;
			var s3 = V2.Y - V1.Y;
			var s4 = v.Y - V1.Y;

			var w1 = (V1.X * s1 + s4 * s2 - v.X * s1) / (s3 * s2 - (V2.X - V1.X) * s1);
			var w2 = (s4 - w1 * s3) / s1;

			return w1.GrEq(0) && w2.GrEq(0) && (w1 + w2).LeEq(1);
		}

		public bool Overlap(AABB aabb)
		{
			var cv1 = V1 - aabb.Center;
			var cv2 = V2 - aabb.Center;
			var cv3 = V3 - aabb.Center;

			// any of triangle points in aabb
			//if (aabb.Inside(V1) || aabb.Inside(V2) || aabb.Inside(V3))
			//	return true;
			var v1Plane = aabb.PointPosition(cv1);
			if (v1Plane == 0)
				return true;
			var v2Plane = aabb.PointPosition(cv2);
			if (v2Plane == 0)
				return true;
			var v3Plane = aabb.PointPosition(cv3);
			if (v3Plane == 0)
				return true;

			// if all triangle points are outside of aabb
			//if (!GetAABB().Overlap(aabb))
			//	return false;
			if ((v1Plane & v2Plane & v3Plane) != 0)
				return false;

			// any of aabb points inside of triangle
			if (Inside(aabb.Min) || Inside(aabb.Max) ||
				Inside(new Vector2f(aabb.Left, aabb.Top)) ||
				Inside(new Vector2f(aabb.Right, aabb.Bottom)))
				return true;

			// check each triangle edge vs aabb edges
			if (aabb.Intersect(V1, V2))
				return true;
			if (aabb.Intersect(V1, V3))
				return true;
			if (aabb.Intersect(V2, V3))
				return true;

			return false;
		}

		public bool Intersect(Ray ray, double from = 0, double to = double.PositiveInfinity)
		{
			throw new NotImplementedException();
		}

		public double Trace(Vector2f origin, Vector2f direction, out Vector2f normal)
		{
			throw new NotImplementedException();
		}
	}
}
