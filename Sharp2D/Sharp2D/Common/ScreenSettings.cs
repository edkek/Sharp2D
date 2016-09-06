﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using Sharp2D.Core;
using Sharp2D.Core.Graphics;

namespace Sharp2D
{
    public class ScreenSettings : SaveableSettings
    { 
        /// <summary>
        /// <para>The title of the window</para>
        /// Default: Sharp2D
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// <para>How long between each logic tick to wait.</para>
        /// <para>Measurment: ms</para>
        /// Default: 40ms
        /// </summary>
        public double LogicTickRate { get; set; }

        /// <summary>
        /// <para>The maxium number of frames to skip when the computer is running slow.</para>
        /// <para>Notes: Ignored when UseOpenTKLoop is true</para>
        /// <para>Measurement: frames</para>
        /// Default: 5 frames
        /// </summary>
        public int MaxSkippedFrames { get; set; }

        /// <summary>
        /// <para>The size of the game space on screen</para>
        /// <para>Measurement: pixels</para>
        /// Default: 1280x720
        /// </summary>
        public System.Drawing.Size GameSize { get; set; }

        /// <summary>
        /// <para>The size of the window</para>
        /// <para>Measurement: pixels</para>
        /// Default: 1280x720
        /// </summary>
        public System.Drawing.Size WindowSize { get; set; }

        /// <summary>
        /// <para>Whether to run in fullscreen or not</para>
        /// Default: False
        /// </summary>
        public bool Fullscreen { get; set; }

        /// <summary>
        /// <para>Whether ro enable vsync or not</para>
        /// Default: False
        /// </summary>
        public bool VSync { get; set; }

        /// <summary>
        /// <para>The max number of frames to draw per second, -1 being unlimited</para>
        /// <para>Notes: If the value of this setting is anything but -1, then Sharp2D will use the OpenTK loop</para>
        /// Default: -1
        /// </summary>
        public int MaxFPS { get; set; }

        private bool _opentk;
        /// <summary>
        /// <para>Whether Sharp2D should use OpenTK's game loop code, or it's own game loop code.</para>
        /// <para>Notes: Sharp2D's game loop code does not support the MaxFPS setting, so if a MaxFPS is set, then OpenTK's game loop code will be used.</para>
        /// Default: True
        /// </summary>
        public bool UseOpenTKLoop
        {
            get
            {
                return _opentk || MaxFPS != -1;
            }
            set
            {
                _opentk = value;
            }
        }

        private Vector2 aspect;
        private System.Drawing.Size savedW;
        public Vector2 WindowAspectRatio
        {
            get
            {
                if (savedW == WindowSize) return aspect;
                float width = WindowSize.Width;
                float height = WindowSize.Height;

                float newWidth = width;
                float newHeight = height;

                while (height != 0)
                {
                    float remainder = width % height;
                    width = height;
                    height = remainder;
                }

                newWidth = newWidth / width;
                newHeight = newHeight / width;
                aspect = new OpenTK.Vector2(newWidth, newHeight);
                return aspect;
            }
        }

        public ScreenSettings(ScreenSettings orginal)
        {
            if (orginal == null)
                return;
            WindowTitle = orginal.WindowTitle;
            LogicTickRate = orginal.LogicTickRate;
            MaxSkippedFrames = orginal.MaxSkippedFrames;
            GameSize = orginal.GameSize;
            WindowSize = orginal.GameSize;
            Fullscreen = orginal.Fullscreen;
            VSync = orginal.VSync;
            MaxFPS = orginal.MaxFPS;
        }

        public ScreenSettings() : this(Screen.DefaultSettings) { }

        protected override void OnLoad() { }

        protected override void OnSave() { }
    }
}
