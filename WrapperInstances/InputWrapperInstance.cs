﻿using System;
using System.Collections.Generic;

using Android.Widget;

using Aragas.Core.Wrappers;

namespace PokeD.Server.Android.WrapperInstances
{
    public class InputWrapperInstance : IInputWrapper
    {
        public static TextView TextView { private get; set; }
        private static List<string> Lines { get; } = new List<string>();

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        public InputWrapperInstance() { }

        public void ShowKeyboard() { }

        public void HideKeyboard() { }

        public void ConsoleWrite(string message)
        {
            Lines.Add(message);
            if (Lines.Count > Math.Floor(TextView.LineHeight * 0.70f))
                Lines.RemoveAt(0);

            MainActivity.RunOnUI(() => TextView.Text = string.Join(Environment.NewLine, Lines));
        }

        public void LogWriteLine(DateTime time, string message)
        {
            var msg_0 = $"[{DateTime.Now:yyyy-MM-dd_HH:mm:ss}]_{message}";
            LogManager.WriteLine(msg_0);

            var msg_1 = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            ConsoleWrite(msg_1);
        }
    }
}
