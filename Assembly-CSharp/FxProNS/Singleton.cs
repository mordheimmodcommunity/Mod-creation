namespace FxProNS
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (Compare((T)null, instance))
                {
                    instance = new T();
                }
                return instance;
            }
        }

        private static bool Compare<T>(T x, T y) where T : class
        {
            return x == y;
        }
    }
}
