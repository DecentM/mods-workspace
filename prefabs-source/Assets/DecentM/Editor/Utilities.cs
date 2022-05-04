using System.Linq;
using System.Reflection;

using UnityEditor;

namespace DecentM.EditorTools
{
    public static class AsyncProgress
    {
        private static readonly MethodInfo _displayProgressBar = typeof(Editor).Assembly.GetTypes().FirstOrDefault(e => e.Name == "AsyncProgressBar")?.GetMethod("Display");
        private static readonly MethodInfo _clearProgressBar = typeof(Editor).Assembly.GetTypes().FirstOrDefault(e => e.Name == "AsyncProgressBar")?.GetMethod("Clear");

        public static void Display(string text, float progress)
        {
            _displayProgressBar.Invoke(null, new object[] { text, progress });
        }

        public static void Clear()
        {
            _clearProgressBar.Invoke(null, null);
        }
    }
}
