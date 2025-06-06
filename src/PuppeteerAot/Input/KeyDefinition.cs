// <copyright file="KeyDefinition.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Puppeteer.Input
{
    public class KeyDefinition
    {
        public int KeyCode { get; set; }

        public int? ShiftKeyCode { get; set; }

        public string Key { get; set; }

        public string ShiftKey { get; set; }

        public string Code { get; set; }

        public string Text { get; set; }

        public string ShiftText { get; set; }

        public int Location { get; set; }
    }
}
