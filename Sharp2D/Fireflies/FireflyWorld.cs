﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp2D;

namespace Fireflies
{
    public class FireflyWorld : GenericWorld
    {
        public static int FIREFLY_COUNT = 100;
        public const int CLOUD_COUNT = 100;
        public override string Name
        {
            get { return "worlds/fireflys.json"; }
        }

        protected override void OnDisplay()
        {
            base.OnDisplay();

            Sprite moon = Sprite.FromImage("sprites/moon.png");
            moon.X = (3.5f * 16f) + (moon.Width / 2f);
            moon.Y = (7.5f * 16f) + (moon.Height / 2f) + 3f;
            moon.IgnoreLights = true;
            AddSprite(moon);

            BackgroundSprite sprite = new BackgroundSprite();
            sprite.X = sprite.Width / 2f;
            sprite.Y = sprite.Height / 2f;
            sprite.Layer = 1;
            AddSprite(sprite);

            Random rand = new Random();
            for (int i = 0; i < FIREFLY_COUNT; i++)
            {
                Firefly fly = new Firefly();

                fly.X = rand.Next(4, 40) * 16f;
                fly.Y = rand.Next(8, 25) * 16f;

                AddLogical(fly);

                AddLight(fly);
            }

            for (int i = 0; i < CLOUD_COUNT; i++)
            {
                Cloud c = new Cloud();

                c.X = rand.Next(-10, 50) * 16f;
                c.Y = 8f * 16f;
                c.IgnoreLights = true;

                AddSprite(c);
            }
        }
    }
}
