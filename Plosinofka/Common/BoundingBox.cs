
using System;

namespace Ujeby.Plosinofka.Common
{
	struct BoundingBox
	{
		/// <summary>bottom left</summary>
		public Vector2f Position;
		public Vector2i Size;

		public bool IsIn(Vector2f p)
		{
			return !(p.X < Position.X || p.Y < Position.Y || p.X >= Position.X + Size.X || p.Y >= Position.Y + Size.Y);
		}

		public bool IsIn(int x, int y)
		{
			return !(x < Position.X || y < Position.Y || x >= Position.X + Size.X || y >= Position.Y + Size.Y);
		}

		public bool Overlapping(BoundingBox box)
		{
			// above
			if (box.Position.Y >= Position.Y + Size.Y)
				return false;

			// bellow
			if (box.Position.Y + box.Size.Y <= Position.Y)
				return false;

			// left
			if (box.Position.X + box.Size.X <= Position.X)
				return false;

			// right
			if (box.Position.X >= Position.X + Size.X)
				return false;

			return true;
		}

		public bool Overlapping(Vector2f position, Vector2i size)
		{
			// above
			if (position.Y >= Position.Y + Size.Y)
				return false;

			// bellow
			if (position.Y + size.Y <= Position.Y)
				return false;

			// left
			if (position.X + size.X <= Position.X)
				return false;

			// right
			if (position.X >= Position.X + Size.X)
				return false;

			return true;
		}

		internal double Interserction(Vector2f origin, Vector2f direction, out Vector2f normal)
		{
			normal = Vector2f.Zero;
			var tMin = double.PositiveInfinity;

			if (IsIn(origin))
				return 0;

			// ! origin is not inside

			if (origin.X <= Position.X)
			{
				var t = (-origin.X + Math.Abs(Position.X)) / direction.X;
				if (t < tMin)
				{
					var hit = origin.Y + direction.Y * t;
					if (hit >= Position.Y && hit < Position.Y + Size.Y)
					{
						tMin = t;
						normal = Vector2f.Left;
					}
				}
			}
			else if (origin.X >= Position.X + Size.X - 1)
			{
				var t = (-origin.X + Math.Abs(Position.X + Size.X - 1)) / direction.X;
				if (t < tMin)
				{
					var hit = origin.Y + direction.Y * t;
					if (hit >= Position.Y && hit < Position.Y + Size.Y)
					{
						tMin = t;
						normal = Vector2f.Right;
					}
				}
			}

			if (origin.Y <= Position.Y)
			{
				var t = (-origin.Y + Math.Abs(Position.Y)) / direction.Y;
				if (t < tMin)
				{
					var hit = origin.X + direction.X * t;
					if (hit >= Position.X && hit < Position.X + Size.X)
					{
						tMin = t;
						normal = Vector2f.Down;
					}
				}
			}
			else if (origin.Y >= Position.Y + Size.Y - 1)
			{
				var t = (-origin.Y + Math.Abs(Position.Y + Size.Y - 1)) / direction.Y;
				if (t < tMin)
				{
					var hit = origin.X + direction.X * t;
					if (hit >= Position.X && hit < Position.X + Size.X)
					{
						tMin = t;
						normal = Vector2f.Up;
					}
				}
			}

			return tMin;
		}
	}
}
