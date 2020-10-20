using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	public enum LevelResourceType
	{
		Background = 0,
		Data = 1,

		Count
	}

	public class Level : IRayCasting, IRayMarching
	{
		public string Name;
		public Vector2i Size;
		public Guid[] Resources = new Guid[(int)LevelResourceType.Count];
		public BoundingBox[] Colliders;

		public Level(string name) => Name = name;

		public Level(string name, IEnumerable<BoundingBox> colliders) : this(name)
		{
			Colliders = colliders.ToArray();
			Size = new Vector2i((int)Colliders.Max(c => c.Right), (int)Colliders.Max(c => c.Top));
		}

		/// <summary>
		/// load level by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Level Load(string name)
		{
			var start = Game.GetElapsed();

			var level = new Level(name);

			var color = ResourceCache.LoadSprite($".\\Content\\Worlds\\{ name }-color.png", true);
			level.Size = color.Size;
			level.Resources[(int)LevelResourceType.Background] = color.Id;

			var data = ResourceCache.LoadSprite($".\\Content\\Worlds\\{ name }-data.png", true);
			level.Resources[(int)LevelResourceType.Data] = data.Id;
			level.Colliders = ProcessCollisionMap(data);

			var elapsed = Game.GetElapsed() - start;
			Log.Add($"Level.Load('{ name }'): { (int)elapsed }ms");

			return level;
		}

		/// <summary>
		/// create bounding boxes from collision map for faster testing
		/// collision object is colored as 0xff0000ff
		/// resulting bounding boxes are not overlaping
		/// </summary>
		/// <param name="map"></param>
		private static BoundingBox[] ProcessCollisionMap(Sprite map)
		{
			var start = Game.GetElapsed();

			var colliders = new List<BoundingBox>();
			for (var y = 0; y < map.Size.Y; y++)
			{
				for (var x = 0; x < map.Size.X; x++)
				{
					var p = y * map.Size.X + x;

					// skip if point is already in another collider
					if (FindCollider(colliders, x, y, out BoundingBox oldCollider))
						x += (int)oldCollider.Size.X;

					else if (IsShadowCaster(map.Data[p]))
					{
						// new colider
						var collider = new BoundingBox
						{
							Position = new Vector2f(x, y),
						};

						var width = 1;
						var height = 1;

						// find width
						while (x + width < map.Size.X &&
							IsShadowCaster(map.Data[p + width]) &&
							!colliders.Any(c => c.IsIn(x + width, y)))
							width++;

						// find height
						var cleanRow = true;
						while (y + height < map.Size.Y && cleanRow)
						{
							var offset = p + height * map.Size.X;
							for (var i = 0; i < width; i++)
								if (!IsShadowCaster(map.Data[offset + i]) ||
									colliders.Any(c => c.IsIn(x + i, y + height)))
								{
									cleanRow = false;
									break;
								}

							if (cleanRow)
								height++;
						}

						collider.Size = new Vector2f(width, height);
						colliders.Add(collider);

						// advance just after collider
						x += width;
					}
				}
			}

			var elapsed = Game.GetElapsed() - start;
			Log.Add($"Level.ProcessCollisionMap('{ map.Filename }'): { colliders.Count } colliders, { (int)elapsed }ms");

			return colliders.ToArray();
		}

		private static bool FindCollider(IEnumerable<BoundingBox> colliders, int x, int y, out BoundingBox oldCollider)
		{
			oldCollider = default;

			foreach (var collider in colliders)
				if (collider.IsIn(x, y))
				{
					oldCollider = collider;
					return true;
				}

			return false;
		}

		public const uint ShadowCasterMask = 0xff0000ff;
		public const uint ShadowReceiverMask = 0xff00ff00;

		public static bool IsShadowReceiverMask(uint pixelValue)
		{
			return (pixelValue & ShadowReceiverMask) == ShadowReceiverMask;
		}

		public static bool IsShadowCaster(uint pixelValue)
		{
			return (pixelValue & ShadowCasterMask) == ShadowCasterMask;
		}

		public double Intersect(BoundingBox box, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;
			var tMin = double.PositiveInfinity;

			// bottom left
			if (!(direction.X > 0 && direction.Y > 0))
				tMin = GetClosest(box.Position, direction, tMin, normal, out normal);

			// bottom right
			if (!(direction.X < 0 && direction.Y > 0))
				tMin = GetClosest(box.Position + Vector2f.Right * box.Size.X, direction, tMin, normal, out normal);

			// top left
			if (!(direction.X > 0 && direction.Y < 0))
				tMin = GetClosest(box.Position + Vector2f.Up * box.Size.Y, direction, tMin, normal, out normal);

			// top right
			if (!(direction.X < 0 && direction.Y < 0))
				tMin = GetClosest(box.Position + box.Size, direction, tMin, normal, out normal);

			// top center
			if (direction.Y > 0)
				tMin = GetClosest(box.Position + Vector2f.Up * box.Size.Y + Vector2f.Right * box.Size.X * 0.5, 
					direction, tMin, normal, out normal);

			// bottom center
			if (direction.Y < 0)
				tMin = GetClosest(box.Position + Vector2f.Right * box.Size.X * 0.5,
					direction, tMin, normal, out normal);

			// right side
			if (direction.X > 0)
			{
				tMin = GetClosest(box.Position + Vector2f.Right * box.Size.X + Vector2f.Up * box.Size.Y * 0.25, 
					direction, tMin, normal, out normal);

				tMin = GetClosest(box.Position + Vector2f.Right * box.Size.X + Vector2f.Up * box.Size.Y * 0.5, 
					direction, tMin, normal, out normal);

				tMin = GetClosest(box.Position + Vector2f.Right * box.Size.X + Vector2f.Up * box.Size.Y * 0.75, 
					direction, tMin, normal, out normal);
			}

			// left side
			if (direction.X < 0)
			{
				tMin = GetClosest(box.Position + Vector2f.Up * box.Size.Y * 0.25, direction,
					tMin, normal, out normal);

				tMin = GetClosest(box.Position + Vector2f.Up * box.Size.Y * 0.5, direction,
					tMin, normal, out normal);

				tMin = GetClosest(box.Position + Vector2f.Up * box.Size.Y * 0.75, direction,
					tMin, normal, out normal);
			}

			return tMin;
		}

		private double GetClosest(Vector2f origin, Vector2f direction, double tMin, Vector2f nMin, out Vector2f normal)
		{
			var t = Intersect(origin, direction, out Vector2f n);
			if (t < tMin && Math.Abs(t) < Math.Abs(tMin))
			{
				normal = n;
				return t;
			}

			normal = nMin;
			return tMin;
		}

		public double Intersect(Vector2f origin, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;

			var tMin = double.PositiveInfinity;
			foreach (var bb in Colliders)
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

		private const double RAY_MARCH_EPSILON = 0.1;
		private const double RAY_MARCH_MAX_STEPS = 128;

		private double Distance(Vector2f point)
		{
			var tMin = double.PositiveInfinity;
			foreach (ISdf sdf in Colliders)
			{
				var t = sdf.Distance(point);
				if (t < tMin)
					tMin = t;
			}

			return tMin;
		}

		public double RayMarch(Vector2f origin, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;

			// only raymarch sdf's which are in front of ray
			var validColliders = Colliders.Where(c =>
			{
				if (direction.X < 0 && origin.X < c.Left)
					return false;

				if (direction.X > 0 && origin.X > c.Right)
					return false;

				if (direction.Y < 0 && origin.Y < c.Bottom)
					return false;

				if (direction.Y > 0 && origin.Y > c.Top)
					return false;

				return true;
			}).ToArray();

			var tMin = double.PositiveInfinity;
			if (validColliders.Length > 0)
			{
				var t = 0.0;
				for (var step = 0; step < RAY_MARCH_MAX_STEPS; step++)
				{
					var p = origin + direction * t;
					var d = Colliders.Min(c => c.Distance(p));

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
					Distance(p + Vector2f.Right * RAY_MARCH_EPSILON) - Distance(p - Vector2f.Right * RAY_MARCH_EPSILON),
					Distance(p + Vector2f.Up * RAY_MARCH_EPSILON) - Distance(p - Vector2f.Up * RAY_MARCH_EPSILON))
					.Normalize();
			}

			return tMin;
		}
	}
}
