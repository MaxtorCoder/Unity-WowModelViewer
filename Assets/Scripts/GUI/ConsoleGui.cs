using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GUI
{
    public class ConsoleGui : MonoBehaviour
    {
        public Text LogText;

        private string fullText = string.Empty;
        
        private void Update()
        {
            if (fullText.Length > 5000)
                fullText = fullText.Substring(0, 4000);
            
            LogText.text = fullText;
        }

        static readonly Dictionary<LogType, string> logTypeColors = new Dictionary<LogType, string>()
        {
            { LogType.Assert, "white" },
            { LogType.Error, "#ff0000ff" },
            { LogType.Exception, "#ff0000ff" },
            { LogType.Log, "white" },
            { LogType.Warning, "#00ff00ff" },
        };
        
        public void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleLog;
        }

        public void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
        }
        
        void HandleLog(string logString, string stackTrace, LogType type)
        {
            fullText += $"\n<color={logTypeColors[type]}>{logString}</color>";
        }
    }
}