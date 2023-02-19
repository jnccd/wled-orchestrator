using Notes.Comms;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Configuration
{
    public class ConfigData
    {
        public DateTime saveTime;

        public Point Pos;
        public Size Size;
        public Color BackColor;

        public List<Note> Notes;

        public ConfigData()
        {
            Pos = new Point(-1, -1);
            Size = new Size(-1, -1);
            Notes = new List<Note>();
        }
    }
}
