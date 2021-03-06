﻿using OpenTK.Input;
using Sharp2D;

namespace TestGame
{
    public class TestSprite : PhysicsSprite
    {
        public override string Name
        {
            get { return "Hans"; }
        }

        public bool MoveFlag = false;

        public TestSprite()
        {
            Texture = Texture.NewTexture("sprites/Hans/Hans.png");
            Texture.LoadTextureFromFile();
            Width = Texture.TextureWidth;
            Height = Texture.TextureHeight;

            AttachModule<AnimationModule>();
        }


        protected override void OnUnload()
        {
        }

        protected override void OnDisplay()
        {
        }

        protected override void BeforeDraw()
        {
        }

        public override void Update()
        {
            base.Update();

            if (!MoveFlag) { return; }

            if (Keyboard.GetState()[Key.W])
            {
                Y -= 5;
            }

            if (Keyboard.GetState()[Key.S])
            {
                Y += 5;
            }

            if (Keyboard.GetState()[Key.A])
            {
                X -= 5;
            }

            if (Keyboard.GetState()[Key.D])
            {
                X += 5;
            }
        }
    }
}