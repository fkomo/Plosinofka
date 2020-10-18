
namespace Ujeby.Plosinofka.Interfaces
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
		RB
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
