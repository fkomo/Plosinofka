using System.Collections.Generic;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Entities;
using Ujeby.Plosinofka.Game.Core;

namespace Ujeby.Plosinofka.Game
{
	public class Game0 : Engine.Core.Game
	{
		public override int GameSpeed { get; protected set; } = 50;
		public override double LastUpdateDuration { get; protected set; }

		public List<Entity> Entities { get; protected set; } = new List<Entity>();
		public Level CurrentLevel { get; protected set; }

		private readonly DebugData DebugData = new DebugData();
		private readonly GameStateMachine State = new GameStateMachine();

		public Game0()
		{
		}

		public override void Initialize()
		{
			// make camera view smaller then window size for more pixelated look!
			var view = Vector2i.FullHD / 4;
			var cameraWindowSize = view / 3;
			Camera = new WindowCamera(view, new AABB(Vector2f.Zero, cameraWindowSize) + cameraWindowSize);

			ChangeState(new LoadingLevel("Level0"));
		}

		public override void Destroy()
		{
			Log.Add($"Simulation0.Destroy()");
		}

		public override void AddEntity(Entity entity)
		{
			Entities.Add(entity);
			DebugData.TrackEntity(entity as ITrack);
		}

		public void ClearCurrentLevel()
		{
			// TODO clear current level resources?

			Player = null;
			CurrentLevel = null;

			Entities.Clear();
			DebugData.Clear();
		}

		public void SetCurrentLevel(Level level, Player player, Camera camera = null)
		{
			Player = player;
			CurrentLevel = level;

			Camera = camera ?? Camera;

			AddEntity(player);
		}

		public override void Update()
		{
			var start = GameLoop.GetElapsed();

			State.Current.Update(this);

			LastUpdateDuration = GameLoop.GetElapsed() - start;
		}

		public override void Render(double interpolation)
		{
			State.Current.Render(this, interpolation);
		}

		internal void ChangeState(GameState newState)
		{
			State.Change(State.Current, newState);
		}
	}
}
