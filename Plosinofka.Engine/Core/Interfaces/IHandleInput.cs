
namespace Ujeby.Plosinofka.Engine.Core
{
	public enum InputButton
	{
		Up,
		Down,
		Left,
		Right,
		A,
		B,
		X,
		Y,
		LT,
		LB,
		RT,
		RB,
		Start,
		Back
	}

	public enum InputButtonState
	{
		Pressed,
		Released
	}

	public interface IHandleInput
	{
		void HandleButton(InputButton button, InputButtonState state);
	}
}
