
namespace Ujeby.Plosinofka.Common
{
	public struct Ray
	{
		public Vector2f Origin;
		public Vector2f Direction;
		public Vector2f InvDirection;
		public Vector2i Sign;

		public Ray(Vector2f origin, Vector2f direction, bool normalized = false)
		{
			Origin = origin;
			
			if (!normalized)
				Direction = direction.Normalize();
			else
				Direction = direction;

			InvDirection.X = 1.0 / Direction.X;
			InvDirection.Y = 1.0 / Direction.Y;

			Sign.X = InvDirection.X < 0 ? 1 : 0;
			Sign.Y = InvDirection.Y < 0 ? 1 : 0;
		}
	}
}
