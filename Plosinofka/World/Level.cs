using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	enum LevelResourceType
	{
		Background = 0,
		Collision = 1,

		Count
	}

	class Collider
	{
	}

	class Level : IRayCaster
	{
		public string Name;
		public Vector2i Size;
		public Guid[] Resources = new Guid[(int)LevelResourceType.Count];
		public BoundingBox[] Colliders;

		public Level(string name) => Name = name;

		/// <summary>
		/// load level by name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Level Load(string name)
		{
			var start = Game.GetElapsed();

			var level = new Level(name);

			var color = ResourceCache.LoadSprite($".\\Content\\Worlds\\{ name }-color.png");
			level.Size = color.Size;
			level.Resources[(int)LevelResourceType.Background] = color.Id;

			var collision = ResourceCache.LoadSprite($".\\Content\\Worlds\\{ name }-collision.png", true);
			level.Resources[(int)LevelResourceType.Collision] = collision.Id;
			level.Colliders = ProcessCollisionMap(collision);

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

					else if (IsCollider(map.Data[p]))
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
							IsCollider(map.Data[p + width]) &&
							!colliders.Any(c => c.IsIn(x + width, y)))
							width++;

						// find height
						var cleanRow = true;
						while (y + height < map.Size.Y && cleanRow)
						{
							var offset = p + height * map.Size.X;
							for (var i = 0; i < width; i++)
								if (!IsCollider(map.Data[offset + i]) ||
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

		private const uint ColliderMask = 0xff0000ff;

		private static bool IsCollider(uint pixelValue)
		{
			return (pixelValue & ColliderMask) == ColliderMask;
		}

		public double Intersect(BoundingBox box, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;

			// TODO one only 3 Intersections are needed

			// bottom left
			var tMin = double.PositiveInfinity;
				
			var t1 = Intersect(box.Position, direction, out Vector2f n1);
			if (t1 < tMin && Math.Abs(t1) < Math.Abs(tMin))
			{
				tMin = t1;
				normal = n1;
			}

			// bottom right
			var t2 = Intersect(box.Position + Vector2f.Right * box.Size.X, direction, out Vector2f n2);
			if (t2 < tMin && Math.Abs(t2) < Math.Abs(tMin))
			{
				tMin = t2;
				normal = n2;
			}

			// top left
			var t3 = Intersect(box.Position + Vector2f.Up * box.Size.Y, direction, out Vector2f n3);
			if (t3 < tMin && Math.Abs(t3) < Math.Abs(tMin))
			{
				tMin = t3;
				normal = n3;
			}

			// top right
			var t4 = Intersect(box.Position + box.Size, direction, out Vector2f n4);
			if (t4 < tMin && Math.Abs(t4) < Math.Abs(tMin))
			{
				tMin = t4;
				normal = n4;
			}

			Log.Add($"Level.Intersect(box={ box }, dir={ direction }): { tMin }, normal={ normal }");
			return tMin;
		}

		public double Intersect(Vector2f origin, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;

			var tMin = double.PositiveInfinity;
			foreach (var bb in Colliders.Take(2))
			{
				var t = bb.Intersect(origin, direction, out Vector2f n);
				if (t < tMin)
				{
					tMin = t;
					normal = n;
				}
			}

			return tMin;
		}
	}
}
