﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenTK.Input;
using Sharp2D.Core;

namespace Sharp2D.Game
{
    public static class Input
    {
        private const string FilePath = "config/keys.conf";

        public static Keyboard Keyboard = new Keyboard();
        public static Mouse Mouse = new Mouse();

        static Input()
        {
            if (!File.Exists(FilePath)) return;

            var lines = File.ReadAllLines(FilePath);

            foreach (var l in lines.Where(l => !l.Trim().StartsWith("#")))
            {
                var split = l.Split('=');

                if (split.Length != 2)
                {
                    Logger.Warn("Line \"" + l + "\" in keys.conf could not be parsed.");
                    continue;
                }

                var key = split[0].Trim();
                var value = split[1].Trim();
                ushort parsed;

                if (!UInt16.TryParse(value, out parsed))
                {
                    Logger.Warn("Value \"" + value + "\" in keys.conf is not a valid ushort.");
                    continue;
                }

                if (parsed < 130) { Keyboard.Keys.Add(key, (Key) (parsed + 1)); } //Keyboard codes
                else if (parsed < 142) { Mouse.Buttons.Add(key, (MouseButton) (parsed - 130)); } //Mouse codes
                else { Logger.Warn("Value \"" + parsed + "\" is not a valid keycode."); } //Invalid codes
            }
        }
    }

    public sealed class Keyboard
    {
        internal Dictionary<string, Key> Keys = new Dictionary<string, Key>();

        internal Keyboard()
        {
        }

        public bool this[string keyName]
        {
            get
            {
                if (Keys.ContainsKey(keyName))
                {
                    return OpenTK.Input.Keyboard.GetState()[Keys[keyName]];
                }

                Logger.Warn("Invalid key name specified: \"" + keyName + "\".");
                return false;
            }
        }

        public void SetDefaults(Dictionary<string, Key> defaultMapping)
        {
            foreach (string key in defaultMapping.Keys.Where(key => !Keys.ContainsKey(key)))
            {
                Keys.Add(key, defaultMapping[key]);
            }
        }
    }

    public sealed class Mouse
    {
        internal Dictionary<string, MouseButton> Buttons = new Dictionary<string, MouseButton>();

        internal Mouse()
        {
        }

        public bool this[string buttonName]
        {
            get
            {
                if (Buttons.ContainsKey(buttonName))
                {
                    return OpenTK.Input.Mouse.GetState()[Buttons[buttonName]];
                }

                Logger.Warn("Invalid button name specified: \"" + buttonName + "\".");
                return false;
            }
        }

        public void SetDefaults(Dictionary<string, MouseButton> defaultMapping)
        {
            foreach (string key in Buttons.Keys.Where(key => !Buttons.ContainsKey(key)))
            {
                Buttons.Add(key, defaultMapping[key]);
            }
        }

        public MouseState GetState()
        {
            return OpenTK.Input.Mouse.GetState();
        }

        public System.Drawing.Point GetMousePosition()
        {
            var state = OpenTK.Input.Mouse.GetState();

            return new Point(state.X, state.Y);
        }
    }
}