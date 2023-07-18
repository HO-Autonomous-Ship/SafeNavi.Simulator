namespace SyDLab.Usv.Simulator.Domain
{
	public sealed class Singleton<T> where T : class, new()
	{
		private Singleton()
		{

		}

		class SingletonCreator
		{
			static SingletonCreator()
			{

			}

			// Private object instantiated with private constructor
			internal static readonly T instance = new T();
		}

		public static T UniqueInstance
		{
			get { return SingletonCreator.instance; }
		}
	}
}