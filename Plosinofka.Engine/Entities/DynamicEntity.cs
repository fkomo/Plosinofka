using Ujeby.Plosinofka.Engine.Common;
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

	public abstract class DynamicEntity : Entity
	{
		public Vector2f Velocity;

		public Vector2f PreviousVelocity { get; protected set; }
		public Vector2f PreviousPosition { get; protected set; }

		private bool[] Obstacle = new bool[(int)Side.Count];
		public bool ObstacleAt(Side side) => Obstacle[(int)side];

		public virtual void AfterUpdate(IEnvironment env)
		{
			UpdateObstacles(env);
		}

		protected void UpdateObstacles(IEnvironment env)
		{
			var bb = BoundingBox + Position;

			Obstacle[(int)Side.Down] = env.Overlap(
				new AABB(new Vector2f(bb.Left + 1, bb.Bottom - 1), new Vector2f(bb.Right - 1, bb.Bottom)));

			Obstacle[(int)Side.Up] = env.Overlap(
				new AABB(new Vector2f(bb.Left + 1, bb.Top), new Vector2f(bb.Right - 1, bb.Top + 1)));

			Obstacle[(int)Side.Left] = env.Overlap(
				new AABB(new Vector2f(bb.Left - 1, bb.Bottom + 1), new Vector2f(bb.Left, bb.Top - 1)));

			Obstacle[(int)Side.Right] = env.Overlap(
				new AABB(new Vector2f(bb.Right, bb.Bottom + 1), new Vector2f(bb.Right + 1, bb.Top - 1)));
		}

		public override void Update(IEnvironment environment)
		{
			// save state before update
			PreviousPosition = Position;
			PreviousVelocity = Velocity;
		}

		public Vector2f InterpolatedPosition(double interpolation) 
			=> PreviousPosition + (Position - PreviousPosition) * interpolation;
	}
}
