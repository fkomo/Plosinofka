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
	public struct Layer
	{
		public string SpriteId;
		public string DataSpriteId;
		public int Depth;
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
		public AABB[] Colliders;

		/// <summary>ordered array of SpriteId from farthest to nearest</summary>
		public Layer[] Layers { get; protected set; }

		public Level(string name) => Name = name;

		public Level(string name, AABB[] colliders) : this(name)
		{
			Colliders = colliders;
			if (colliders.Any())
				Size = new Vector2i((int)Colliders.Max(c => c.Right), (int)Colliders.Max(c => c.Top));
		}

		/// <summary></summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Level Load(string name)
		{
			var start = Game.GetElapsed();

			var level = new Level(name);

			// load layers
			level.Layers = Directory.EnumerateFiles($".\\Content\\Worlds\\", $"{ name }-color*")
				.Select(layerFile =>
				{
					var sprite = SpriteCache.LoadSprite(layerFile);
					var layer = new Layer
					{
						SpriteId = sprite?.Id,
					};

					var fileInfo = new FileInfo(layerFile);
					layer.Depth = Convert.ToInt32(
						fileInfo.Name
						.Replace($"{ name }-color", string.Empty)
						.Replace(fileInfo.Extension, string.Empty));

					var dataSpriteFilename = layerFile.Replace($"{ name }-color", $"{ name }-data");
					if (File.Exists(dataSpriteFilename))
						layer.DataSpriteId = SpriteCache.LoadSprite(dataSpriteFilename)?.Id;
					return layer;

				}).OrderBy(l => l.Depth).ToArray();

			var mainLayer = level.Layers.SingleOrDefault(l => l.Depth == 0);

			var dataSpriteId = mainLayer.DataSpriteId;
			if (dataSpriteId != null)
				level.Colliders = AABB.FromMap(SpriteCache.Get(dataSpriteId), ShadowCasterMask);

			level.Size = SpriteCache.Get(mainLayer.SpriteId).Size;

			var elapsed = Game.GetElapsed() - start;
			Log.Add($"Level.Load('{ name }'): { (int)elapsed }ms");

			return level;
		}

		public const uint ShadowCasterMask = 0xff0000ff;
		public const uint ShadowReceiverMask = 0xff00ff00;

		public double Trace(AABB box, Vector2f direction, out Vector2f normal)
		{
			// TODO not working very well in world1/room1 - platforms are not high enough

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
