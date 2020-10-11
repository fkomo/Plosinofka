
namespace Ujeby.Plosinofka.Common
{
	struct BoundingBox
	{
		public Vector2f TopLeft;
		public Vector2i Size;

		public bool IsIn(Vector2f p)
		{
			return !(p.X < TopLeft.X || p.Y < TopLeft.Y || p.X > TopLeft.X + Size.X || p.Y > TopLeft.Y + Size.Y);
		}

		public bool IsIn(int x, int y)
		{
			return !(x < TopLeft.X || y < TopLeft.Y || x > TopLeft.X + Size.X || y > TopLeft.Y + Size.Y);
		}

		public bool Overlapping(BoundingBox box)
		{
			// above
			if (box.TopLeft.Y - box.Size.Y > TopLeft.Y)
				return false;

			// bellow
			if (box.TopLeft.Y < TopLeft.Y + Size.Y)
				return false;

			// left
			if (box.TopLeft.X + box.Size.X < TopLeft.X)
				return false;

			// right
			if (box.TopLeft.X > TopLeft.X + Size.X)
				return false;

			return true;
		}

		public bool Overlapping(Vector2f topLeft, Vector2i size)
		{
			// above
			if (topLeft.Y - size.Y > TopLeft.Y)
				return false;

			// bellow
			if (topLeft.Y < TopLeft.Y + Size.Y)
				return false;

			// left
			if (topLeft.X + size.X < TopLeft.X)
				return false;

			// right
			if (topLeft.X > TopLeft.X + Size.X)
				return false;

			return true;
		}
	}
}
