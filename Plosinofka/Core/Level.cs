using System;
using System.Collections.Generic;
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
	public class Level
	{
		public const uint DeathZoneMask = 0xffff0000;
		public const uint ObstacleMask = 0xff0000ff;
		public const uint ShadeMask = 0xff00ff00;
		
		public string Name { get; private set; }
		public Vector2i Size { get; private set; }

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
			if (obstacles.Any())
				Size = new Vector2i((int)obstacles.Max(c => c.Right), (int)obstacles.Max(c => c.Top));

			foreach (var aabb in obstacles)
				Simulation.Instance.AddEntity(new Obstacle(aabb));
		}

		/// <summary></summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Level Load(string name)
		{
			var start = Engine.Core.Game.GetElapsed();

			var level = new Level(name)
			{
				Start = new Vector2f(64, 20),
				Finish = new AABB(new Vector2f(1000, 0), new Vector2f(1100, 100)),

				// load layers
				Layers = Directory.EnumerateFiles(Program.ContentDirectory + $"World\\{ name }\\", $"color*.png")
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

					}).OrderBy(l => l.Depth).ToArray()
			};

			var mainLayer = level.Layers.SingleOrDefault(l => l.Depth == 0);

			// TODO divide obstacles to multiple regions for faster collision detection
			var dataSpriteId = mainLayer.DataMapId;
			if (dataSpriteId != null)
			{
				var dataSprite = SpriteCache.Get(dataSpriteId);

				foreach (var aabb in AABB.FromMap(dataSprite, ObstacleMask))
					Simulation.Instance.AddEntity(new Obstacle(aabb));

				foreach (var aabb in AABB.FromMap(dataSprite, DeathZoneMask))
					Simulation.Instance.AddEntity(new DeathZone(aabb));
			}

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

			var p = new Platform("platform",
				path:new Vector2f[] 
				{ 
					new Vector2f(40, 56), 
					new Vector2f(40, 132),
					new Vector2f(192, 132),
					new Vector2f(192, 56),
				})
				{
					PathStep = 2,
					PathPointWaitDuration = 1000,
				};
			Simulation.Instance.AddEntity(p);

			var elapsed = Engine.Core.Game.GetElapsed() - start;
			Log.Add($"Level.Load('{ name }'): { (int)elapsed }ms");

			return level;
		}
	}
}
