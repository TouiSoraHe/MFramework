namespace MFramework.Common
{
    public class Singleton<T> where T : Singleton<T>,new()
    {
        private static T Instance;

        public static T GetInstance()
        {
            if (Instance == null)
            {
                Instance = new T();
            }
            return Instance;
        }
    }
}