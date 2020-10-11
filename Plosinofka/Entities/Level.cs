using System;
using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Graphics;

namespace Ujeby.Plosinofka.Entities
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

	class Level
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
						x += oldCollider.Size.X;

					else if (IsCollider(map.Data[p]))
					{
						// new colider
						var collider = new BoundingBox
						{
							TopLeft = new Vector2f(x, y),
							Size = new Vector2i(1, 1)
						};

						// find width
						while (x + collider.Size.X < map.Size.X &&
							IsCollider(map.Data[p + collider.Size.X]) &&
							!colliders.Any(c => c.IsIn(x + collider.Size.X, y)))
							collider.Size.X++;

						// find height
						var cleanRow = true;
						while (y + collider.Size.Y < map.Size.Y && cleanRow)
						{
							var offset = p + collider.Size.Y * map.Size.X;
							for (var i = 0; i < collider.Size.X; i++)
								if (!IsCollider(map.Data[offset + i]) ||
									colliders.Any(c => c.IsIn(x + i, y + collider.Size.Y)))
								{
									cleanRow = false;
									break;
								}

							if (cleanRow)
								collider.Size.Y++;
						}

						colliders.Add(collider);

						// advance just after collider
						x += collider.Size.X;
					}
				}
			}

			// transform to world cordinates
			var result = colliders.ToArray();
			for (var i = 0; i < result.Length; i++)
				result[i].TopLeft.Y = map.Size.Y - result[i].TopLeft.Y;

			var elapsed = Game.GetElapsed() - start;
			Log.Add($"Level.ProcessCollisionMap('{ map.Filename }'): { colliders.Count } colliders, { (int)elapsed }ms");

			return result;
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

		public double CastRay(Vector2f origin, Vector2f direction, out Entity entity)
		{
			entity = null;



			return double.PositiveInfinity;
		}
	}
}
