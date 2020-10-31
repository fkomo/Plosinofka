using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	public class Simulation : Singleton<Simulation>, ICollisionSolver
	{
		/// <summary>
		/// desired number of updates per second
		/// </summary>
		public const int GameSpeed = 50;
		public static Vector2f Gravity = new Vector2f(0.0, -1);
		public static double TerminalFallingVelocity = -16;

		public double LastUpdateDuration { get; protected set; }

		protected List<Entity> Entities = new List<Entity>();

		/// <summary>
		/// Entity.Name vs its TrackedData
		/// </summary>
		protected Dictionary<string, FixedQueue<TrackedData>> TrackedEntities =
			new Dictionary<string, FixedQueue<TrackedData>>();

		public Player Player { get; protected set; }
		public Camera Camera { get; protected set; }

		private Level CurrentLevel;

		/// <summary>
		/// number of past records of entity properties (position, ...)
		/// </summary>
		public const int EntityTraceLength = 256;

		public Simulation()
		{
			// create player
			Player = new Player("player1");
		}

		public void Initialize()
		{
			LoadLevel("Level0");
		}

		private void LoadLevel(string levelName)
		{
			// clear old entities
			Entities.Clear();
			TrackedEntities.Clear();

			// load level
			CurrentLevel = Level.Load(levelName);

			// set player position
			Player.Position = CurrentLevel.Start;
			AddEntity(Player);

			// make camera view smaller then window size for more pixelated look!
			Camera = new Camera(Vector2i.FullHD / 4, CurrentLevel.Size, Player);
		}

		public void Update()
		{
			var start = Game.GetElapsed();

			// remove obsolete entities
			for (var i = Entities.Count - 1; i >= 0; i--)
				if ((Entities[i] as IDestroyable)?.Obsolete() == true)
				{
					if (Entities[i] is ITrackable trackedEntity)
						TrackedEntities.Remove(trackedEntity.TrackId());

					Entities.RemoveAt(i);
				}

			// update all entities
			for (var i = Entities.Count - 1; i >= 0; i--)
				Entities[i].Update(CurrentLevel);

			// solve collisions of dynamic entities
			foreach (var entity in Entities)
			{
				if (entity is DynamicEntity dynamicEntity)
				{
					Solve(dynamicEntity, out Vector2f position, out Vector2f velocity);
					dynamicEntity.Position = position;
					dynamicEntity.Velocity = velocity;

					Track(entity as ITrackable);
				}
			}

			// update camera with respect to world/level borders
			Camera.Update(Player, CurrentLevel.Size);

			LastUpdateDuration = Game.GetElapsed() - start;
		}

		public void Render(double interpolation)
		{
			var layerOffset = Camera.InterpolatedPosition(interpolation) / (Vector2f)(CurrentLevel.Size - Camera.View);

			// render all layers, back to front
			foreach (var layer in CurrentLevel.Layers)
			{
				// main layer
				if (layer.Depth == 0)
				{
					var playerPosition = Player.InterpolatedPosition(interpolation);
					var colliders = CurrentLevel.Obstacles.ToList();
					colliders.Add(Player.BoundingBox + playerPosition);

					var lights = Entities.Where(e => e is Light).Select(e => e as Light).ToArray();

					Renderer.Instance.RenderLayer(Camera, interpolation,
						SpriteCache.Get(layer.SpriteId),
						SpriteCache.Get(layer.DataSpriteId),
						lights, colliders.ToArray());

					// entities
					foreach (var entity in Entities)
						(entity as IRenderable)?.Render(Camera, interpolation);

					// debug
					if (Settings.Current.GetDebug(DebugSetting.MovementHistory))
						RenderTrackedData(interpolation);
					if (Settings.Current.GetDebug(DebugSetting.DrawAABB))
						DrawAABBs(interpolation);
					if (Settings.Current.GetDebug(DebugSetting.DrawVectors))
						DrawVelocities(interpolation);
				}
				else
					// background/foreground layers
					Renderer.Instance.RenderLayer(Camera, SpriteCache.Get(layer.SpriteId), layerOffset);
			}
		}

		private void Track(ITrackable entity)
		{
			if (entity == null)
				return;	

			if (TrackedEntities.ContainsKey(entity.TrackId()))
				TrackedEntities[entity.TrackId()].Add(entity.Track());
		}

		internal void AddEntity(Entity entity)
		{
			Entities.Add(entity);
			if (entity is ITrackable trackableEntity)
				TrackedEntities.Add(trackableEntity.TrackId(), new FixedQueue<TrackedData>(EntityTraceLength));
		}

		private void DrawVelocities(double interpolation)
		{
			foreach (var entity in Entities)
			{
				if (entity is DynamicEntity dynamicEntity)
				{
					var center = dynamicEntity.InterpolatedPosition(interpolation) + 
						entity.BoundingBox.Min + entity.BoundingBox.Size * 0.5;
					
					Renderer.Instance.RenderLine(Camera, center, center + dynamicEntity.Velocity, 
						Color4b.Green, interpolation);
				}
			}
		}

		private void DrawAABBs(double interpolation)
		{
			var aabbs = CurrentLevel.Obstacles.ToList();
			aabbs.Add(Player.BoundingBox + Player.InterpolatedPosition(interpolation));

			var color = new Color4b(0xff, 0x00, 0x00, 0xaf);
			foreach (var aabb in aabbs)
				Renderer.Instance.RenderRectangle(Camera, aabb, color, interpolation);
		}

		private void RenderTrackedData(double interpolation)
		{
			foreach (var trackedEntity in TrackedEntities)
			{
				if (trackedEntity.Value.Queue.Count < 2)
					continue;

				var color = new Color4b((uint)trackedEntity.Key.GetHashCode()) { A = 0xff };

				// positions
				var values = trackedEntity.Value.Queue.ToArray();
				for (var i = 1; i < values.Length; i++)
					Renderer.Instance.RenderLine(Camera, values[i - 1].Position, values[i].Position, 
						color, interpolation);

				// TODO render tracked velocities ?
			}
		}

		public bool Solve(DynamicEntity entity, out Vector2f position, out Vector2f velocity)
		{
			position = entity.Position;
			velocity = entity.Velocity;

			// no velocity ? no collision!
			if (velocity == Vector2f.Zero)
				return false;

			var remainingVelocity = velocity;

			var collisionFound = false;
			while (true)
			{
				var distance = velocity.Length();
				var direction = velocity.Normalize();

				var entityBox = entity.BoundingBox + position;
				var t = CurrentLevel.Trace(entityBox, direction, out Vector2f normal);
				if (t <= distance)
				{
					collisionFound = true;

					var hitPosition = position + direction * t;
					
					// set new position & velocity
					velocity = (position + velocity) 
						+ normal * Vector2f.Dot(direction.Inv(), normal) * (distance - t) 
						- hitPosition;
					position = hitPosition;

					if (normal.X == 0)
						remainingVelocity.Y = 0;
					else 
						remainingVelocity.X = 0;

					//Log.Add($"Simulation.CollisionSolved({ entity }, bb={ entityBox }): t={ t }; position={ position }; velocity={ velocity }; remainingVelocity={ remainingVelocity }");

					if (velocity == Vector2f.Zero) // or better just close to zero ?
						break; // solved - nowhere to move
				}
				else
					break; // solved - nothing else stands in the way
			}

			position += velocity;
			velocity = remainingVelocity;

			//Log.Add($"World.Solved({ entity }; position={ entity.Position }; velocity={ entity.Velocity }): position={ position }; velocity={ velocity }");

			return collisionFound;
		}

		public static void Destroy()
		{
			instance = null;
		}
	}
}
