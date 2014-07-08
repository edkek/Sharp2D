﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Sharp2D.Game.Tiled;
using Sharp2D.Game.Sprites.Tiled;
using Sharp2D.Game.Sprites;
using Sharp2D.Core.Graphics;
using Sharp2D.Core.Graphics.Shaders;
using System.Drawing;

namespace Sharp2D.Game.Worlds
{
    class DrawBatch : SpriteBatch
    {
        public int type;
        private int drawCount;
        public int DrawCount
        {
            get
            {
                if (type == 0)
                    return Count;
                else
                    return drawCount;
            }
        }

        public override void Add(Sprite sprite)
        {
            base.Add(sprite);
            if (type == 1)
                drawCount += sprite.Lights.Count;
            else
                drawCount += sprite.Lights.Count + 1;
        }
    }
    public class GenericRenderJob : SpriteRenderJob
    {

        protected const int POS_LOCATION = 0;
        protected const int TEXCOORD_LOCATION = 1;

        private static Shader lightShader;
        private static Shader ambiantShader;
        private static Shader alphaLightShader;

        private int vao_id;
        private int vbo_id;
        private int tri_id;
        private bool gen;

        private float[] quad_points = new float[] 
        {
            0.0f, 0.0f, 0.0f, 0.0f,   1.0f, 0.0f, 1.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f, 1.0f, 0.0f, 1.0f
        };
        private uint[] rectangleindicies = new uint[] 
        {
            0, 1, 2, 0, 2, 3
        };
        private GenericWorld parent;

        public GenericRenderJob(GenericWorld parent)
        {
            Screen.Camera = new OpenGL3Camera();
            this.parent = parent;
        }

        DrawBatch[] CreateCulledBatches()
        {
            DrawBatch culled_batch = new DrawBatch();
            DrawBatch culled_batch_light = new DrawBatch();
            culled_batch_light.type = 1;
            DrawBatch culled_batch_alpha = new DrawBatch();
            culled_batch_alpha.type = 2;



            Batch.ForEach(delegate(Sprite sprite)
            {
                if (!sprite.IsOffScreen && sprite.Visible)
                {
                    if (sprite.Texture != null && sprite.Texture.HasAlpha)
                    {
                        culled_batch_alpha.Add(sprite);
                    }
                    else
                    {
                        culled_batch.Add(sprite);
                        if (sprite.Lights.Count > 0)
                        {
                            culled_batch_light.Add(sprite);
                        }
                    }
                }
            });

            int ocount = culled_batch.Count;

            float width = Screen.Settings.GameSize.Width;
            float height = Screen.Settings.GameSize.Height;

            float cx = -Screen.Camera.X;
            float cy = Screen.Camera.Y;

            float cull_width = 380f * (Screen.Camera.Z / 100f);
            float cull_height = 256f * (Screen.Camera.Z / 100f);
            cull_width *= (Screen.Settings.GameSize.Width / 1024);
            cull_height *= (Screen.Settings.GameSize.Height / 720);
            cull_width /= 2f;
            cull_height /= 2f;
            foreach (Layer layer in parent.Layers)
            {
                if (!layer.IsTileLayer)
                    continue;
                float ex = cx + (cull_width + (3f * 16f));
                float ey = cy + cull_height;
                float sx = cx - cull_width;
                float sy = cy - cull_height;

                int s_i_x = Math.Max((int)(sx / 16f), 0);
                int s_i_y = Math.Max((int)Math.Ceiling((sy - 8f) / 16f), 0);

                int e_i_x = Math.Max((int)(ex / 16f), 0);
                int e_i_y = Math.Max((int)Math.Ceiling((ey - 8f) / 16f), 0);


                for (int x = s_i_x; x <= e_i_x; x++)
                {
                    for (int y = s_i_y; y < e_i_y; y++)
                    {
                        TileSprite sprite = layer[x, y];
                        if (sprite == null)
                            continue;

                        if (sprite.Texture != null && sprite.Texture.HasAlpha)
                        {
                            culled_batch_alpha.Add(sprite);
                        }
                        else
                        {
                            culled_batch.Add(sprite);
                            if (sprite.Lights.Count > 0)
                            {
                                culled_batch_light.Add(sprite);
                            }
                        }
                    }
                }
            }

            return new DrawBatch[] { culled_batch, culled_batch_alpha, culled_batch_light };
        }

        public override void PerformJob()
        {
            if (!gen)
            {
                gen = true;
                OnFirstRun();
            }

            DrawBatch[] batches = CreateCulledBatches();
            DrawBatch batch = batches[0];
            DrawBatch alpha_batch = batches[1];
            DrawBatch batch_light = batches[2];

            Vector2 aspect = Screen.Settings.WindowAspectRatio;
            if (batch.Count > 0)
            {
                ambiantShader.Use();

                ambiantShader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), ambiantShader.Uniforms["camPosAndScale"]);
                ambiantShader.Uniforms.SetUniform(aspect.X / aspect.Y, ambiantShader.Uniforms["screenRatioFix"]);

                ambiantShader.Uniforms.SetUniform(parent.AmbientShaderColor, ambiantShader.Uniforms["brightness"]);

                batch.ForEach(delegate(Shader shader, Texture texture, Sprite sprite)
                {
                    if (sprite.FirstRun)
                    {
                        sprite.Display();
                        sprite.FirstRun = false;
                    }

                    if (shader != null)
                        shader.Use();

                    if (texture != null && sprite.Texture.ID != texture.ID)
                        sprite.Texture.Bind();
                    else if (texture != null)
                        texture.Bind();

                    sprite.PrepareDraw(); //Let the sprite setup for drawing, maybe setup it's own custom shader

                    ambiantShader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, sprite.Width, sprite.Height), ambiantShader.Uniforms["spritePos"]);
                    float tsize = sprite.TexCoords.SquardSize;
                    ambiantShader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), ambiantShader.Uniforms["texCoordPosAndScale"]);

                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                });
            }
            if (batch_light.Count > 0)
            {
                lightShader.Use();

                lightShader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), lightShader.Uniforms["camPosAndScale"]);
                lightShader.Uniforms.SetUniform(aspect.X / aspect.Y, lightShader.Uniforms["screenRatioFix"]);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

                batch_light.ForEach(delegate(Shader shader, Texture texture, Sprite sprite)
                {
                    if (sprite.Lights.Count == 0)
                        return;


                    if (sprite.FirstRun)
                    {
                        sprite.Display();
                        sprite.FirstRun = false;
                    }

                    if (shader != null)
                        shader.Use();

                    if (texture != null && sprite.Texture.ID != texture.ID)
                        sprite.Texture.Bind();
                    else if (texture != null)
                        texture.Bind();

                    sprite.PrepareDraw(); //Let the sprite setup for drawing, maybe setup it's own custom shader

                    lightShader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, sprite.Width, sprite.Height), lightShader.Uniforms["spritePos"]);
                    float tsize = sprite.TexCoords.SquardSize;
                    lightShader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), lightShader.Uniforms["texCoordPosAndScale"]);

                    lock (sprite.light_lock)
                    {
                        foreach (Light light in sprite.Lights)
                        {
                            lightShader.Uniforms.SetUniform(light.ShaderColor, lightShader.Uniforms["lightcolor"]);
                            lightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), lightShader.Uniforms["lightdata"]);

                            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                        }
                    }
                });

                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }

            if (alpha_batch.Count > 0)
            {
                alphaLightShader.Use();
                alphaLightShader.Uniforms.SetUniform(new Vector3(Screen.Camera.X, Screen.Camera.Y, 1f / Screen.Camera.Z), alphaLightShader.Uniforms["camPosAndScale"]);
                alphaLightShader.Uniforms.SetUniform(aspect.X / aspect.Y, alphaLightShader.Uniforms["screenRatioFix"]);

                alphaLightShader.Uniforms.SetUniform(parent.AmbientShaderColor, alphaLightShader.Uniforms["ambient"]);


                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);

                alpha_batch.ForEach(delegate(Shader shader, Texture texture, Sprite sprite)
                {
                    if (sprite.FirstRun)
                    {
                        sprite.Display();
                        sprite.FirstRun = false;
                    }

                    if (shader != null)
                        shader.Use();

                    if (texture != null && sprite.Texture.ID != texture.ID)
                        sprite.Texture.Bind();
                    else if (texture != null)
                        texture.Bind();

                    sprite.PrepareDraw(); //Let the sprite setup for drawing, maybe setup it's own custom shader

                    alphaLightShader.Uniforms.SetUniform(new Vector4(sprite.X, -sprite.Y, sprite.Width, sprite.Height), alphaLightShader.Uniforms["spritePos"]);
                    float tsize = sprite.TexCoords.SquardSize;
                    alphaLightShader.Uniforms.SetUniform(new Vector4(sprite.TexCoords.BottomLeft.X, sprite.TexCoords.BottomLeft.Y, (sprite.TexCoords.BottomLeft.X - sprite.TexCoords.BottomRight.X), (sprite.TexCoords.BottomLeft.Y - sprite.TexCoords.TopLeft.Y)), alphaLightShader.Uniforms["texCoordPosAndScale"]);

                    alphaLightShader.Uniforms.SetUniform(1f, alphaLightShader.Uniforms["ambientmult"]);

                    GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

                    lock (sprite.light_lock)
                    {
                        if (sprite.Lights.Count > 0)
                        {
                            Light light = sprite.Lights[0];
                            alphaLightShader.Uniforms.SetUniform(light.ShaderColor, alphaLightShader.Uniforms["lightcolor"]);
                            alphaLightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), alphaLightShader.Uniforms["lightdata"]);
                            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                        }
                    }

                    if (sprite.Lights.Count <= 1)
                        return;

                    alphaLightShader.Uniforms.SetUniform(0f, alphaLightShader.Uniforms["ambientmult"]);
                    GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

                    lock (sprite.light_lock)
                    {
                        for (int i = 1; i < sprite.Lights.Count; i++)
                        {
                            Light light = sprite.Lights[i];
                            alphaLightShader.Uniforms.SetUniform(light.ShaderColor, alphaLightShader.Uniforms["lightcolor"]);
                            alphaLightShader.Uniforms.SetUniform(new Vector3(light.X, -light.Y, light.Radius), alphaLightShader.Uniforms["lightdata"]);

                            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
                        }
                    }
                });

                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
        }

        protected void OnFirstRun()
        {
            Screen.ValidateOpenGLSafe("CreateVBOs");


            GL.GenVertexArrays(1, out vao_id);

            GL.BindVertexArray(vao_id);

            vbo_id = GL.GenBuffer();
            tri_id = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_id);
            unsafe
            {
                fixed (float* data = quad_points)
                {
                    fixed (uint* tdata = rectangleindicies)
                    {
                        IntPtr point = (IntPtr)data;
                        IntPtr tpoint = (IntPtr)tdata;
                        IntPtr size = (IntPtr)(4 * 4 * sizeof(float));
                        IntPtr tsize = (IntPtr)(6 * sizeof(uint));

                        GL.BufferData(BufferTarget.ArrayBuffer, size, point, BufferUsageHint.StaticDraw); //TODO Maybe don't use static draw

                        GL.EnableVertexAttribArray(POS_LOCATION);
                        OpenTK.Graphics.ES20.GL.VertexAttribPointer(POS_LOCATION, 2, OpenTK.Graphics.ES20.All.Float, false, 4 * sizeof(float), new IntPtr(0));

                        GL.EnableVertexAttribArray(TEXCOORD_LOCATION);
                        OpenTK.Graphics.ES20.GL.VertexAttribPointer(TEXCOORD_LOCATION, 2, OpenTK.Graphics.ES20.All.Float, false, 4 * sizeof(float), new IntPtr(2 * sizeof(float)));

                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, tri_id);
                        GL.BufferData(BufferTarget.ElementArrayBuffer, tsize, tpoint, BufferUsageHint.StaticDraw); //TODO Maybe don't use static draw
                    }
                }
            }

            if (lightShader == null)
            {

                lightShader = new Shader("Sharp2D.Resources.sprite_light.vert", "Sharp2D.Resources.sprite_light.frag"); //TODO Change files

                lightShader.LoadAll();
                lightShader.CompileAll();
                GL.BindAttribLocation(lightShader.ProgramID, POS_LOCATION, "posattrib");
                GL.BindAttribLocation(lightShader.ProgramID, TEXCOORD_LOCATION, "tcattrib");
                lightShader.LinkAll();
            }

            if (ambiantShader == null)
            {
                ambiantShader = new Shader("Sharp2D.Resources.sprite_amb.vert", "Sharp2D.Resources.sprite_amb.frag");
                ambiantShader.LoadAll();
                ambiantShader.CompileAll();
                GL.BindAttribLocation(ambiantShader.ProgramID, POS_LOCATION, "posattrib");
                GL.BindAttribLocation(ambiantShader.ProgramID, TEXCOORD_LOCATION, "tcattrib");
                ambiantShader.LinkAll();
            }

            if (alphaLightShader == null)
            {
                alphaLightShader = new Shader("Sharp2D.Resources.sprite_light_alpha.vert", "Sharp2D.Resources.sprite_light_alpha.frag");
                alphaLightShader.LoadAll();
                alphaLightShader.CompileAll();
                GL.BindAttribLocation(alphaLightShader.ProgramID, POS_LOCATION, "posattrib");
                GL.BindAttribLocation(alphaLightShader.ProgramID, TEXCOORD_LOCATION, "tcattrib");
                alphaLightShader.LinkAll();
            }
        }
    }
}