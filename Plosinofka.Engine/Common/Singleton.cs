namespace Ujeby.Plosinofka.Engine.Common
{
	public class Singleton<T> where T : new()
	{
		/// <summary>instance of type T</summary>
		protected static T instance;

		/// <summary>
		/// sync object for multithread access
		/// </summary>
		private static readonly object LockObject = new object();

		/// <summary>singleton instance property</summary>
		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					lock (LockObject)
					{
						if (instance == null)
							instance = new T();
					}
				}

				return instance;
			}
		}
	}
}