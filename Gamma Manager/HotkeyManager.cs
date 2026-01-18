using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Gamma_Manager
{
    /// <summary>
    /// Global hotkey manager for registering and handling system-wide hotkeys
    /// </summary>
    public class HotkeyManager : IDisposable
    {
        // Windows API functions for registering global hotkeys
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Modifier key constants
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;
        public const uint MOD_NOREPEAT = 0x4000;

        // Windows message constant for hotkey
        private const int WM_HOTKEY = 0x0312;

        private IntPtr _windowHandle;
        private int _currentId = 1;
        private Dictionary<int, Action> _hotkeyActions = new Dictionary<int, Action>();
        private Dictionary<string, int> _presetHotkeys = new Dictionary<string, int>();

        public HotkeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        /// <summary>
        /// Register a global hotkey
        /// </summary>
        /// <param name="modifiers">Modifier keys (Ctrl, Alt, Shift, Win)</param>
        /// <param name="key">Main key</param>
        /// <param name="action">Action to execute when hotkey is pressed</param>
        /// <returns>Hotkey ID or -1 if registration failed</returns>
        public int RegisterHotkey(uint modifiers, Keys key, Action action)
        {
            int id = _currentId++;

            // Add MOD_NOREPEAT to prevent repeated triggers when key is held down
            modifiers |= MOD_NOREPEAT;

            if (RegisterHotKey(_windowHandle, id, modifiers, (uint)key))
            {
                _hotkeyActions[id] = action;
                return id;
            }

            return -1;
        }

        /// <summary>
        /// Register a hotkey for a specific preset
        /// </summary>
        /// <param name="presetName">Name of the preset</param>
        /// <param name="modifiers">Modifier keys</param>
        /// <param name="key">Main key</param>
        /// <param name="action">Action to execute</param>
        /// <returns>True if successful</returns>
        public bool RegisterPresetHotkey(string presetName, uint modifiers, Keys key, Action action)
        {
            // Unregister existing hotkey for this preset if any
            UnregisterPresetHotkey(presetName);

            int id = RegisterHotkey(modifiers, key, action);
            if (id != -1)
            {
                _presetHotkeys[presetName] = id;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unregister a hotkey by ID
        /// </summary>
        public void UnregisterHotkey(int id)
        {
            if (_hotkeyActions.ContainsKey(id))
            {
                UnregisterHotKey(_windowHandle, id);
                _hotkeyActions.Remove(id);
            }
        }

        /// <summary>
        /// Unregister a preset's hotkey
        /// </summary>
        public void UnregisterPresetHotkey(string presetName)
        {
            if (_presetHotkeys.ContainsKey(presetName))
            {
                UnregisterHotkey(_presetHotkeys[presetName]);
                _presetHotkeys.Remove(presetName);
            }
        }

        /// <summary>
        /// Unregister all hotkeys
        /// </summary>
        public void UnregisterAllHotkeys()
        {
            foreach (var id in _hotkeyActions.Keys)
            {
                UnregisterHotKey(_windowHandle, id);
            }
            _hotkeyActions.Clear();
            _presetHotkeys.Clear();
        }

        /// <summary>
        /// Process Windows messages to handle hotkey events
        /// Call this from WndProc
        /// </summary>
        public bool ProcessHotkey(Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (_hotkeyActions.ContainsKey(id))
                {
                    _hotkeyActions[id]?.Invoke();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Convert modifier flags to string representation
        /// </summary>
        public static string ModifiersToString(uint modifiers)
        {
            List<string> parts = new List<string>();

            if ((modifiers & MOD_CONTROL) != 0) parts.Add("Ctrl");
            if ((modifiers & MOD_ALT) != 0) parts.Add("Alt");
            if ((modifiers & MOD_SHIFT) != 0) parts.Add("Shift");
            if ((modifiers & MOD_WIN) != 0) parts.Add("Win");

            return string.Join(" + ", parts);
        }

        /// <summary>
        /// Get full hotkey string representation
        /// </summary>
        public static string GetHotkeyString(uint modifiers, Keys key)
        {
            string modStr = ModifiersToString(modifiers);
            if (!string.IsNullOrEmpty(modStr))
                return modStr + " + " + key.ToString();
            return key.ToString();
        }

        public void Dispose()
        {
            UnregisterAllHotkeys();
        }
    }
}
