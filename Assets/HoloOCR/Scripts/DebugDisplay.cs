using UnityEngine;
using UnityEngine.UI;

namespace HoloToolkit.Unity
{
    /// <summary>
    /// Simple Behaviour which calculates the frames per second and shows the FPS in a referenced Text control.
    /// </summary>
    public class DebugDisplay : Singleton<DebugDisplay>
    {
        [Tooltip("Reference to Text UI control where the debug info should be displayed.")]
        public Text Text;

        public void Log(string log)
        {
            Text.text = log;
        }

    }
}