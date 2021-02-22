using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Graphics;
using Ujeby.Plosinofka.Engine.SDL;
using Ujeby.Plosinofka.Game;
using Ujeby.Plosinofka.Game.Graphics;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public class Platform : Obstacle, IRender, ITrack
	{
		private string SpriteId;
		protected List<Vector2f> Path { get; private set; } = new List<Vector2f>();
		private int CurrentPathPoint = 0;

		/// <summary>path movement speed</summary>
		public int PathStep { get; set; } = 4;

		/// <summary>platform wait time at each point [ms]</summary>
		public double PathPointWaitDuration { get; set; } = 2000;

		private double WaitStart = 0;

		public override bool Responsive => false;

		public override string ToString() => $"{ base.ToString() }; curr_path:{ CurrentPathPoint }";

		public Platform(string name, IEnumerable<Vector2f> path = null) : base(default)
		{
			var sprite = SpriteCache.LoadSprite(Program.ContentDirectory + $"World\\{ name }.png");
			SpriteId = sprite?.Id;

			var dataSprite = SpriteCache.LoadSprite(Program.ContentDirectory + $"World\\{ name }-data.png");
			if (dataSprite != null)
				BoundingBox = AABB.Merge(AABB.FromMap(dataSprite, Level.ObstacleMask));

			else if (sprite != null)
				BoundingBox = new AABB(Vector2f.Zero, sprite.Size);

			if (path != null)
			{
				Path.AddRange(path);
				Position = Path.First();
			}
		}

		public void Render(AABB view, double interpolation)
		{
			var position = InterpolatedPosition(interpolation);

			// animation not found, use default sprite
			Renderer.Instance.RenderSprite(view, SpriteCache.Get(SpriteId), position);
		}

		public override void Update()
		{
			base.Update();

			Velocity = Vector2f.Zero;
			if (Path.Count > 0 && Core.GameLoop.GetElapsed() > (WaitStart + PathPointWaitDuration))
			{
				var nextPathPoint = (CurrentPathPoint + 1) % Path.Count;
				if ((Path[nextPathPoint] - Position).Length() <= PathStep)
				{
					Velocity = Path[nextPathPoint] - Position;
					WaitStart = Core.GameLoop.GetElapsed();
					CurrentPathPoint = nextPathPoint;
				}
				else
					Velocity = (Path[nextPathPoint] - Path[CurrentPathPoint]).Normalize() * PathStep;
			}
		}

		public string TrackId() => Name;

		public TrackedData Track() => new TrackedData { Position = Center, Velocity = Velocity };
	}
}
