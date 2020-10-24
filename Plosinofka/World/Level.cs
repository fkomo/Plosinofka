using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	public enum LevelResourceType
	{
		BackgroundLayer = 0,
		Data = 1,

		Count
	}

	/// <summary>
	/// level properties
	/// - minimum block size 8x8
	/// -
	/// </summary>
	public class Level : IRayCasting, IRayMarching
	{
		public string Name { get; protected set; }
		public Vector2i Size { get; protected set; }
		public Guid[] Resources = new Guid[(int)LevelResourceType.Count];
		public AABB[] Colliders;

		/// <summary>ordered from farthest to nearest</summary>
		public Guid[] BackgroundLayers { get; protected set; }

		/// <summary>ordered from farthest to nearest</summary>
		public Guid[] ForegroundLayers { get; protected set; }

		public Level(string name) => Name = name;

		public Level(string name, AABB[] colliders) : this(name)
		{
			Colliders = colliders;
			if (colliders.Any())
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
			level.Resources[(int)LevelResourceType.BackgroundLayer] = color.Id;

			var data = ResourceCache.LoadSprite($".\\Content\\Worlds\\{ name }-data.png", true);
			level.Resources[(int)LevelResourceType.Data] = data.Id;
			level.Colliders = ProcessCollisionMap(data);

			// load background layers
			level.BackgroundLayers = Directory.EnumerateFiles($".\\Content\\Worlds\\", $"{ name }-bg-*")
				.OrderBy(f => f).Select(layerFile =>
				{
					var fileInfo = new FileInfo(layerFile);
					var layerId = Convert.ToInt32(fileInfo.Name
						.Replace($"{ name }-bg-", string.Empty)
						.Replace(fileInfo.Extension, string.Empty));
					return ResourceCache.LoadSprite(layerFile).Id;
				}).ToArray();

			// load foreground layers
			level.ForegroundLayers = Directory.EnumerateFiles($".\\Content\\Worlds\\", $"{ name }-fg-*")
				.OrderBy(f => f).Select(layerFile =>
				 {
					 var fileInfo = new FileInfo(layerFile);
					 var layerId = Convert.ToInt32(fileInfo.Name
						 .Replace($"{ name }-bg-", string.Empty)
						 .Replace(fileInfo.Extension, string.Empty));

					 return ResourceCache.LoadSprite(layerFile).Id;
				 }).ToArray();

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
		private static AABB[] ProcessCollisionMap(Sprite map)
		{
			var start = Game.GetElapsed();

			var colliders = new List<AABB>();
			for (var y = 0; y < map.Size.Y; y++)
			{
				for (var x = 0; x < map.Size.X; x++)
				{
					var p = y * map.Size.X + x;

					// skip if point is already in another collider
					if (FindCollider(colliders, x, y, out AABB oldCollider))
						x += (int)oldCollider.Size.X;

					else if (IsShadowCaster(map.Data[p]))
					{
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

						// new colider
						var min = new Vector2f(x, y);
						colliders.Add(new AABB(min, min + new Vector2f(width, height)));

						// advance just after collider
						x += width;
					}
				}
			}

			var elapsed = Game.GetElapsed() - start;
			Log.Add($"Level.ProcessCollisionMap('{ map.Filename }'): { colliders.Count } colliders, { (int)elapsed }ms");

			return colliders.ToArray();
		}

		private static bool FindCollider(IEnumerable<AABB> colliders, int x, int y, out AABB oldCollider)
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

		private const uint ShadowCasterMask = 0xff0000ff;
		private const uint ShadowReceiverMask = 0xff00ff00;

		public static bool IsShadowReceiver(uint pixelValue)
		{
			return (pixelValue & ShadowReceiverMask) == ShadowReceiverMask;
		}

		public static bool IsShadowCaster(uint pixelValue)
		{
			return (pixelValue & ShadowCasterMask) == ShadowCasterMask;
		}

		public double Trace(AABB box, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;
			var tMin = double.PositiveInfinity;

			// bottom left
			if (!(direction.X > 0 && direction.Y > 0))
				tMin = GetClosest(box.Min, direction, tMin, normal, out normal);

			// bottom right
			if (!(direction.X < 0 && direction.Y > 0))
				tMin = GetClosest(new Vector2f(box.Right, box.Bottom), direction, tMin, normal, out normal);

			// top left
			if (!(direction.X > 0 && direction.Y < 0))
				tMin = GetClosest(new Vector2f(box.Left, box.Top), direction, tMin, normal, out normal);

			// top right
			if (!(direction.X < 0 && direction.Y < 0))
				tMin = GetClosest(box.Max, direction, tMin, normal, out normal);

			// top center
			if (direction.Y > 0)
				tMin = GetClosest(new Vector2f(box.Center.X, box.Top), direction, tMin, normal, out normal);

			// bottom center
			if (direction.Y < 0)
				tMin = GetClosest(new Vector2f(box.Center.X, box.Bottom), direction, tMin, normal, out normal);

			// right side
			if (direction.X > 0)
			{
				tMin = GetClosest(new Vector2f(box.Right, box.Bottom + box.Size.Y * 0.25),
					direction, tMin, normal, out normal);

				tMin = GetClosest(new Vector2f(box.Right, box.Center.Y), direction, tMin, normal, out normal);

				tMin = GetClosest(new Vector2f(box.Right, box.Bottom + box.Size.Y * 0.75),
					direction, tMin, normal, out normal);
			}

			// left side
			if (direction.X < 0)
			{
				tMin = GetClosest(new Vector2f(box.Left, box.Bottom + box.Size.Y * 0.25),
					direction, tMin, normal, out normal);

				tMin = GetClosest(new Vector2f(box.Left, box.Center.Y), direction, tMin, normal, out normal);

				tMin = GetClosest(new Vector2f(box.Left, box.Bottom + box.Size.Y * 0.75),
					direction, tMin, normal, out normal);
			}

			return tMin;
		}

		private double GetClosest(Vector2f origin, Vector2f direction, double tMin, Vector2f nMin, out Vector2f normal)
		{
			var t = Trace(origin, direction, out Vector2f n);
			if (t < tMin && Math.Abs(t) < Math.Abs(tMin))
			{
				normal = n;
				return t;
			}

			normal = nMin;
			return tMin;
		}

		public double Trace(Vector2f origin, Vector2f direction, out Vector2f normal)
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

		public bool Overlaps(AABB box)
		{
			foreach (var bb in Colliders)
				if (bb.Overlaps(box))
					return true;

			return false;
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

		public bool Intersect(Ray ray, double from = 0, double to = double.PositiveInfinity)
		{
			foreach (var collider in Colliders)
				if (collider.Intersects(ray, from, to))
					return true;

			return false;
		}
	}
}
