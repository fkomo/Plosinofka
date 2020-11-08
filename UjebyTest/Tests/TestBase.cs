using System;

namespace UjebyTest
{
	public abstract class TestBase
	{
		protected TestBase()
		{
			Init();
		}

		public void Run(Func<bool> handleInput)
		{
			while (handleInput())
			{
				Update();
				Render();
			}
		}

		protected abstract void Init();
		protected abstract void Update();
		protected abstract void Render();
	}
}
