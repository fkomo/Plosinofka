
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
		L1,
		R1,
		L2,
		R2
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
