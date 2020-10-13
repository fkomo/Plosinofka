using System.Collections.Generic;
using Ujeby.Plosinofka.Interfaces;
using Ujeby.Plosinofka.Entities;

namespace Ujeby.Plosinofka.Common
{
	public abstract class State
	{
		
	}

	public class StateMachine<T> where T : State
	{
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

			return null;
		}

		public T Change(T from, T to)
		{
			Log.Add($"StateChange({ from } -> { to })");

			if (from != null)
				Push(from);

			return to;
		}

		public void Clear()
		{
			States.Clear();
		}
	}
}
