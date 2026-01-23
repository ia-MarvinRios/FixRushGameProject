using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RayConsole
{
    public class RayConsoleBehaviour : MonoBehaviour
    {
        public static RayConsoleBehaviour Instance;
        private RayConsole _rayConsole;
        //private float _pausedTimeScale;

        private RayConsole RayConsole
        {
            get
            {
                if (_rayConsole != null) { return _rayConsole; }
                return _rayConsole = new RayConsole(_prefix, _commands);
            }
        }

        private StringBuilder _builder = new StringBuilder();

        [Header("RayConsole Settings")]
        [SerializeField] private TMP_Text _logText;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField, Range(1, 200)] private int _maxLines = 200;
        [SerializeField] private string _prefix = string.Empty;
        [SerializeField] private ConsoleCommand[] _commands = new ConsoleCommand[0];
        [Space(10)]
        [SerializeField] private GameObject _canvas = null;
        [SerializeField] private TMP_InputField _inputField = null;

        public ConsoleCommand[] Commands => _commands;

        private void Awake()
        {
            /*
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            */

            Instance = this;
            SetUpConsole();

            _ = GlobalLogBuffer.Entries;
            // Dump existing logs
            foreach (var entry in GlobalLogBuffer.Entries)
            {
                AppendLog(entry.message, entry.stackTrace, entry.type);
            }

            //DontDestroyOnLoad(gameObject);
        }
        private void OnEnable()
        {
            GlobalLogBuffer.OnLogAdded += OnLogAdded;
        }

        private void OnDisable()
        {
            GlobalLogBuffer.OnLogAdded -= OnLogAdded;
        }

        private void SetUpConsole()
        {
            _logText.text = $"<color=green>RayConsole v1.0</color>\n <color=#747474>Type: '/help' for commands.</color>\n <color=#747474>Waiting for logs...</color>";
        }

        public void Toggle(InputAction.CallbackContext context)
        {
            //if(!context.action.triggered) { return; }

            if (_canvas.activeSelf)
            {
                //Time.timeScale = _pausedTimeScale;
                _canvas.SetActive(false);
            }
            else
            {
                /*
                _pausedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                */
                _canvas.SetActive(true);
                _inputField.ActivateInputField();
            }
        }

        public void EnterCommand(InputAction.CallbackContext context)
        {
            //if (!context.action.triggered) { return; }

            if(_inputField.isFocused)
            {
                ExecuteCommand(_inputField.text);

                _inputField.ActivateInputField();
            }
        }

        public void ExecuteCommand(string inputValue)
        {
            RayConsole.ExecuteCommand(inputValue);

            _inputField.text = string.Empty;
        }

        // ------- LOG HANDLING -------
        private void OnLogAdded(GlobalLogBuffer.LogEntry entry)
        {
            AppendLog(entry.message, entry.stackTrace, entry.type);
        }

        private void AppendLog(string logString, string stackTrace, LogType type)
        {
            (string tag, string color) = type switch
            {
                LogType.Warning => ("WARN", "yellow"),
                LogType.Error or LogType.Exception => ("ERROR", "red"),
                _ => ("LOG", "#747474")
            };

            _builder.AppendLine($"<color={color}>[{tag}] {logString}</color>");

            if (type is LogType.Error or LogType.Exception)
            {
                _builder.AppendLine($"<size=70%><color={color}>{stackTrace}</color></size>");
            }

            TrimLines();
            _logText.text = _builder.ToString();
        }

        private void TrimLines()
        {
            string[] lines = _builder.ToString().Split('\n');
            if (lines.Length <= _maxLines) return;

            _builder.Clear();
            for (int i = lines.Length - _maxLines; i < lines.Length; i++)
                _builder.AppendLine(lines[i]);
        }

        public void Clear()
        {
            _builder.Clear();
            _logText.text = string.Empty;
        }
    }
}
