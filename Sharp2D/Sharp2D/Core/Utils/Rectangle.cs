﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp2D.Core.Utils
{
    public class Rectangle
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        public Rectangle() : this(0, 0, 0, 0)
        {

        }

        public Rectangle(float width, float height) : this(0, 0, width, height)
        {

        }

        public Rectangle(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }
}
