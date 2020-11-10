using System;
using System.Collections.Generic;

namespace Ujeby.Plosinofka.Engine.Common
{
	public abstract class State<T> where T : struct, IConvertible
	{
		public override string ToString() => AsEnum.ToString();
		public abstract T AsEnum { get; }
	}

	public class StateMachine<T>
	{
		public T Current { get; set; }

		public Stack<T> States = new Stack<T>();

		public T Push(T state)
		{
			States.Push(state);
			return state;
		}

		public T Pop()
		{
			if (States.TryPop(out T lastState))
				return lastState;

			return default;
		}

		public T Change(T previous, T next, bool pushPreviousState = true)
		{
			if (previous != null && pushPreviousState)
				Push(previous);

			//Log.Add($"State.Change({ next })");

			Current = next;
			return Current;
		}

		public void Clear()
		{
			States.Clear();
		}
	}
}
