using System;
using System.IO;
using System.Linq;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Game.Graphics;

namespace Ujeby.Plosinofka.Game
{
	/// <summary>
	/// </summary>
	public class Level : IRayCasting
	{
		public string Name { get; private set; }
		public Vector2i Size { get; private set; }
		public AABB[] Obstacles { get; private set; }

		/// <summary>
		/// player starting point
		/// </summary>
		public Vector2f Start { get; private set; }
		
		/// <summary>
		/// destination region to complete level
		/// </summary>
		public AABB Finish { get; private set; }

		/// <summary>ordered from farthest to nearest</summary>
		public Layer[] Layers { get; private set; }

		public Level(string name) => Name = name;

		public Level(string name, AABB[] obstacles) : this(name)
		{
			Obstacles = obstacles;
			if (obstacles.Any())
				Size = new Vector2i((int)Obstacles.Max(c => c.Right), (int)Obstacles.Max(c => c.Top));
		}

		/// <summary></summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Level Load(string name)
		{
			var start = Engine.Core.Game.GetElapsed();

			var level = new Level(name);

			level.Start = new Vector2f(64, 32);
			level.Finish = new AABB(new Vector2f(1000, 0), new Vector2f(1100, 100));

			// load layers
			level.Layers = Directory.EnumerateFiles($".\\Content\\World\\{ name }\\", $"color*.png")
				.Select(layerFile =>
				{
					var sprite = SpriteCache.LoadSprite(layerFile);
					var layer = new Layer
					{
						ColorMapId = sprite?.Id,
					};

					var fileInfo = new FileInfo(layerFile);
					layer.Depth = Convert.ToInt32(
						fileInfo.Name
						.Replace($"color", string.Empty)
						.Replace(fileInfo.Extension, string.Empty));

					var dataSpriteFilename = layerFile.Replace($"color", $"data");
					if (File.Exists(dataSpriteFilename))
						layer.DataMapId = SpriteCache.LoadSprite(dataSpriteFilename)?.Id;

					layer.Size = sprite.Size;

					return layer;

				}).OrderBy(l => l.Depth).ToArray();

			var mainLayer = level.Layers.SingleOrDefault(l => l.Depth == 0);

			var dataSpriteId = mainLayer.DataMapId;
			if (dataSpriteId != null)
				level.Obstacles = AABB.FromMap(SpriteCache.Get(dataSpriteId), ObstacleMask);

			level.Size = mainLayer.Size;

			// layers that have different size than main layer are rendered with parallax scrolling
			for (var i = 0; i < level.Layers.Length; i++)
				level.Layers[i].Parallax = level.Size != level.Layers[i].Size;

			Simulation.Instance.AddEntity(
				new Light(new Color4f(1.0, 0.2, 0.2), 10.0) { Position = new Vector2f(160, 160) });
			Simulation.Instance.AddEntity(
				new Light(new Color4f(0.2, 1.0, 0.2), 10.0) { Position = new Vector2f(380, 180) });
			Simulation.Instance.AddEntity(
				new Light(new Color4f(0.2, 0.2, 1.0), 10.0) { Position = new Vector2f(170, 80) });

			var elapsed = Engine.Core.Game.GetElapsed() - start;
			Log.Add($"Level.Load('{ name }'): { (int)elapsed }ms");

			return level;
		}

		public const uint ObstacleMask = 0xff0000ff;
		public const uint ShadeMask = 0xff00ff00;

		public double Trace(Vector2f origin, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;

			var tMin = double.PositiveInfinity;
			foreach (var bb in Obstacles)
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

		public double Trace(AABB box, Vector2f direction, out Vector2f normal)
		{
			// TODO not working very well in world1/room1 - platforms are not high enough

			normal = Vector2f.Zero;
			var tMin = double.PositiveInfinity;

			// bottom left
			if (!(direction.X > 0 && direction.Y > 0))
				tMin = Trace(box.Min, direction, tMin, normal, out normal);

			// bottom right
			if (!(direction.X < 0 && direction.Y > 0))
				tMin = Trace(new Vector2f(box.Right, box.Bottom), direction, tMin, normal, out normal);

			// top left
			if (!(direction.X > 0 && direction.Y < 0))
				tMin = Trace(new Vector2f(box.Left, box.Top), direction, tMin, normal, out normal);

			// top right
			if (!(direction.X < 0 && direction.Y < 0))
				tMin = Trace(box.Max, direction, tMin, normal, out normal);

			// top center
			if (direction.Y > 0)
				tMin = Trace(new Vector2f(box.Center.X, box.Top), direction, tMin, normal, out normal);

			// bottom center
			if (direction.Y < 0)
				tMin = Trace(new Vector2f(box.Center.X, box.Bottom), direction, tMin, normal, out normal);

			// right side
			if (direction.X > 0)
			{
				tMin = Trace(new Vector2f(box.Right, box.Bottom + box.Size.Y * 0.25),
					direction, tMin, normal, out normal);

				tMin = Trace(new Vector2f(box.Right, box.Center.Y), direction, tMin, normal, out normal);

				tMin = Trace(new Vector2f(box.Right, box.Bottom + box.Size.Y * 0.75),
					direction, tMin, normal, out normal);
			}

			// left side
			if (direction.X < 0)
			{
				tMin = Trace(new Vector2f(box.Left, box.Bottom + box.Size.Y * 0.25),
					direction, tMin, normal, out normal);

				tMin = Trace(new Vector2f(box.Left, box.Center.Y), direction, tMin, normal, out normal);

				tMin = Trace(new Vector2f(box.Left, box.Bottom + box.Size.Y * 0.75),
					direction, tMin, normal, out normal);
			}

			return tMin;
		}

		private double Trace(Vector2f origin, Vector2f direction, double tMin, Vector2f nMin, out Vector2f normal)
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

		public bool Overlap(AABB box)
		{
			foreach (var bb in Obstacles)
				if (bb.Overlaps(box))
					return true;

			return false;
		}

		public bool Intersect(Ray ray, double from = 0, double to = double.PositiveInfinity)
		{
			foreach (var obstacle in Obstacles)
				if (obstacle.Intersects(ray, from, to))
					return true;

			return false;
		}
	}
}
