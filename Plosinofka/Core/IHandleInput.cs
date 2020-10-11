
namespace Ujeby.Plosinofka.Interfaces
{
	enum InputButton
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

	enum InputButtonState
	{
		Pressed,
		Released
	}

	interface IHandleInput
	{
		void HandleButton(InputButton button, InputButtonState state);
	}
}
