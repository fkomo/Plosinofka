using Ujeby.Plosinofka.Engine.Common;

namespace Ujeby.Plosinofka.Game.Core
{
	public enum GameStateEnum
	{
		LoadingLevel,
		Gameplay,
	}

	public abstract class GameState : State<GameStateEnum>
	{
		public virtual void Update(Game0 game)
		{
			// nothing to update, yet
		}

		public virtual void Render(Game0 game, double interpolation)
		{
			// nothing to render
		}
	}

	public class GameStateMachine : StateMachine<GameState>
	{

	}
}
