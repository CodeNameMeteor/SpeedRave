using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpeedRave
{
    // Turns bind strings from the config ("U", "F6", "INSERT", "left shift", "joystick button 7", "[1]") into
    // KeyCodes once and caches the result, instead of string-parsing (and, for bad names, throwing) every frame.
    public static class KeyBinds
    {
        private static readonly Dictionary<string, KeyCode> cache = new Dictionary<string, KeyCode>();

        // Where warnings about invalid bind names go. Plugin points this at the BepInEx log; it is a delegate so
        // this file has no BepInEx dependency and can be unit tested on its own.
        public static Action<string> WarningSink = message => Debug.LogWarning("[SpeedRave] " + message);

        // Unity input names that don't match a KeyCode name once spaces are removed.
        private static readonly Dictionary<string, KeyCode> aliases = new Dictionary<string, KeyCode>(StringComparer.OrdinalIgnoreCase)
        {
            { "up", KeyCode.UpArrow },
            { "down", KeyCode.DownArrow },
            { "left", KeyCode.LeftArrow },
            { "right", KeyCode.RightArrow },
            { "leftctrl", KeyCode.LeftControl },
            { "rightctrl", KeyCode.RightControl },
            { "leftcmd", KeyCode.LeftCommand },
            { "rightcmd", KeyCode.RightCommand },
            { "enter", KeyCode.KeypadEnter },
            { "[+]", KeyCode.KeypadPlus },
            { "[-]", KeyCode.KeypadMinus },
            { "[*]", KeyCode.KeypadMultiply },
            { "[/]", KeyCode.KeypadDivide },
            { "[.]", KeyCode.KeypadPeriod },
        };

        // Parses a bind name. Returns false (and KeyCode.None) for empty or unknown names.
        public static bool TryParse(string bind, out KeyCode key)
        {
            key = KeyCode.None;
            if (string.IsNullOrWhiteSpace(bind)) return false;

            string name = bind.Trim().Replace(" ", "");
            if (aliases.TryGetValue(name, out key)) return true;

            // Unity names digits "0"-"9" (the number row) and keypad keys "[0]"-"[9]".
            if (name.Length == 1 && name[0] >= '0' && name[0] <= '9')
            {
                key = KeyCode.Alpha0 + (name[0] - '0');
                return true;
            }
            if (name.Length == 3 && name[0] == '[' && name[2] == ']' && name[1] >= '0' && name[1] <= '9')
            {
                key = KeyCode.Keypad0 + (name[1] - '0');
                return true;
            }

            // Enum.TryParse also accepts numbers ("123"), which are not key names.
            if (char.IsDigit(name[0]) || name[0] == '-') return false;

            if (Enum.TryParse(name, true, out KeyCode parsed) && Enum.IsDefined(typeof(KeyCode), parsed) && parsed != KeyCode.None)
            {
                key = parsed;
                return true;
            }
            return false;
        }

        public static bool IsValid(string bind)
        {
            return Resolve(bind) != KeyCode.None;
        }

        public static bool GetKeyDown(string bind)
        {
            KeyCode key = Resolve(bind);
            return key != KeyCode.None && Input.GetKeyDown(key);
        }

        private static KeyCode Resolve(string bind)
        {
            if (string.IsNullOrWhiteSpace(bind)) return KeyCode.None;
            if (cache.TryGetValue(bind, out KeyCode key)) return key;

            if (!TryParse(bind, out key))
            {
                WarningSink?.Invoke($"'{bind}' is not a valid key name; that bind is ignored.");
            }
            cache[bind] = key;
            return key;
        }
    }
}
