using System;
using System.Collections.Generic;
using System.Linq;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Core;
using Ujeby.Plosinofka.Entities;
using Ujeby.Plosinofka.Graphics;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka
{
	public class World : Simulation, ICollisionSolver
	{
		private Level CurrentLevel;

		private List<Light> Lights = new List<Light>();

		public World()
		{
			CurrentLevel = Level.Load("world1");

			Player = new Player("jebko");
			Player.Position = new Vector2f(64, 32);
			DynamicEntities.Add(Player);

			var light = new Light(new Color4f(1.0, 1.0, 0.8), 32.0);
			light.Position = new Vector2f(300, 250);
			StaticEntities.Add(light);
			Lights.Add(light);

			// make camera view smaller then window size for more pixelated look!
			Camera = new Camera(Vector2i.FullHD / 4, CurrentLevel.Size, Player);
		}

		public World(Level level)
		{
			CurrentLevel = level;
		}

		public override void Update()
		{
			var start = Game.GetElapsed();

			// update all entities
			foreach (var entity in StaticEntities)
				entity.Update(CurrentLevel);
			foreach (var entity in DynamicEntities)
				entity.Update(CurrentLevel);

			// solve collisions of dynamic entities
			foreach (var dynamicEntity in DynamicEntities)
			{
				Solve(dynamicEntity, out Vector2f position, out Vector2f velocity);
				dynamicEntity.Position = position;
				dynamicEntity.Velocity = velocity;
			}

			// update camera with respect to world/level borders
			Camera.Update(Player, CurrentLevel.Size);

			LastUpdateDuration = Game.GetElapsed() - start;
		}

		public override void Render(double interpolation)
		{
			var layerOffset = Camera.InterpolatedPosition(interpolation) / (Vector2f)(CurrentLevel.Size - Camera.View);

			// background layers
			RenderLayers(CurrentLevel.BackgroundLayers, layerOffset, interpolation);

			// main layer
			var color = ResourceCache.Get<Sprite>(CurrentLevel.Resources[(int)LevelResourceType.BackgroundLayer]);
			if (color != null)
			{
				var data = ResourceCache.Get<Sprite>(CurrentLevel.Resources[(int)LevelResourceType.Data]);
				var playerPosition = Player.InterpolatedPosition(interpolation);

				var colliders = CurrentLevel.Colliders.ToList();
				colliders.Add(new AABB(playerPosition, playerPosition + Player.BoundingBox.Size));

				Renderer.Instance.Render(Camera, color, data, interpolation, 
					Lights.ToArray(), colliders.ToArray());
			}

			// entities
			foreach (var entity in StaticEntities)
				(entity as IRenderable)?.Render(Camera, interpolation);
			foreach (var entity in DynamicEntities)
				(entity as IRenderable)?.Render(Camera, interpolation);

			// foreground layers
			RenderLayers(CurrentLevel.ForegroundLayers, layerOffset, interpolation);
		}

		/// <summary>
		/// render multiple layers with paralax scrolling
		/// </summary>
		/// <param name="interpolation"></param>
		private void RenderLayers(IEnumerable<Guid> layers, Vector2f layerOffset, double interpolation)
		{
			foreach (var layerId in layers)
				Renderer.Instance.RenderLayer(Camera, ResourceCache.Get<Sprite>(layerId), layerOffset);
		}

		public bool Solve(DynamicEntity entity, out Vector2f position, out Vector2f velocity)
		{
			position = entity.Position;
			velocity = entity.Velocity;
			var entityBox = entity.BoundingBox;
			var remainingVelocity = velocity;

			// no velocity ? no collision!
			if (velocity == Vector2f.Zero)
				return false;

			var collisionFound = false;
			while (true)
			{
				var distance = velocity.Length();
				var direction = velocity.Normalize();

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

					entityBox = new AABB(position, position + entityBox.Size);

					if (normal.X == 0)
						remainingVelocity.Y = 0;
					else 
						remainingVelocity.X = 0;

					if (velocity == Vector2f.Zero) // or better just close to zero ?
						break; // solved - nowhere to move
				}
				else
					break; // solved - nothing else stands in the way
			}

			position += velocity;
			velocity = remainingVelocity;

			//Log.Add($"Solved({ entity }; position={ entity.Position }; velocity={ entity.Velocity }): position={ position }; velocity={ velocity }");

			return collisionFound;
		}

		public override void Destroy()
		{
			
		}
	}
}
