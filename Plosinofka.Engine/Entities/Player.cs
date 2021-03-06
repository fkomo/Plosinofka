﻿using System.Linq;
using Ujeby.Plosinofka.Engine.Common;
using Ujeby.Plosinofka.Engine.Core;
using Ujeby.Plosinofka.Engine.Graphics;

namespace Ujeby.Plosinofka.Engine.Entities
{
	public enum Side : int
	{
		Up = 0,
		Down,
		Left,
		Right,

		Count
	}

	public abstract class Player : DynamicEntity, IRender, IHandleInput
	{
		public bool Alive { get; private set; } = true;

		public abstract void Render(AABB view, double interpolation);

		public abstract void HandleButton(InputButton button, InputButtonState state);

		private bool[] Obstacle = new bool[(int)Side.Count];
		public bool ObstacleAt(Side side) => Obstacle[(int)side];

		public virtual void UpdateSurroundings(Obstacle[] obstacles)
		{
			var obstacleAABBs = obstacles.Select(o => o.BoundingBox + o.Position).ToArray();

			var bb = BoundingBox + Position;

			Obstacle[(int)Side.Down] = Collisions.Overlap(
				new AABB(new Vector2f(bb.Left + 1, bb.Bottom - 1), new Vector2f(bb.Right - 1, bb.Bottom)), obstacleAABBs);

			Obstacle[(int)Side.Up] = Collisions.Overlap(
				new AABB(new Vector2f(bb.Left + 1, bb.Top), new Vector2f(bb.Right - 1, bb.Top + 1)), obstacleAABBs);

			Obstacle[(int)Side.Left] = Collisions.Overlap(
				new AABB(new Vector2f(bb.Left - 1, bb.Bottom + 1), new Vector2f(bb.Left, bb.Top - 1)), obstacleAABBs);

			Obstacle[(int)Side.Right] = Collisions.Overlap(
				new AABB(new Vector2f(bb.Right, bb.Bottom + 1), new Vector2f(bb.Right + 1, bb.Top - 1)), obstacleAABBs);
		}

		public virtual void Die()
		{
			Alive = false;

			PreviousPosition = Position;
			PreviousVelocity = Velocity;
			Log.Add($"{ ToString() }; died!");
		}
	}
}
