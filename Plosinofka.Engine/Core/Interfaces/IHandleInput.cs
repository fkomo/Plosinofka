
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

	public enum KeyboardButton
	{
		F1,
		F2,
		F3,
		F4,

		F5,
		F6,
		F7,
		F8,

		F9,
		F10,
		F11,
		F12,
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
