using System;
using Ujeby.Plosinofka.Common;
using Ujeby.Plosinofka.Interfaces;

namespace Ujeby.Plosinofka.Entities
{
	class TestCube : Entity, IRender
	{
		public override Vector2i Size => ResourceCache.Get<Sprite>(SpriteId).Size;
		public Guid SpriteId { get; private set; }

		public TestCube(string name) : base(name)
		{
			SpriteId = ResourceCache.LoadSprite(@".\Content\test.png").Id;
		}

		private TestCube()
		{

		}

		public override Entity Copy()
		{
			return new TestCube
			{
				Id = Id,
				Name = Name,
				Position = Position,
				Velocity = Velocity
			};
		}

		public void Render(Camera camera, Entity beforeUpdate, double interpolation)
		{
			// interpolate position
			var newPosition = beforeUpdate.Position + (Position - beforeUpdate.Position) * interpolation;

			Renderer.Instance.RenderSprite(camera,
				newPosition, ResourceCache.Get<Sprite>(SpriteId),
				interpolation);
		}

		public override void Update()
		{

		}
	}
}
