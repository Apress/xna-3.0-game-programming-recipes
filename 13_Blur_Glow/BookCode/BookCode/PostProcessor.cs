using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace BookCode
{
    class PostProcessor
    {
        VertexPositionTexture[] ppVertices;
        VertexDeclaration ppVertexDeclaration;
        RenderTarget2D targetRenderedTo;
        ResolveTexture2D resolveTexture;
        Effect ppEffect;
        SpriteBatch spriteBatch;
        GraphicsDevice device;

        public EffectParameterCollection Parameters
        {
            get { return ppEffect.Parameters; }
        }

        public PostProcessor(GraphicsDevice device, Effect ppEffect)
        {
            this.device = device;

            PresentationParameters pp = device.PresentationParameters;
            targetRenderedTo = new RenderTarget2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format);
            resolveTexture = new ResolveTexture2D(device, pp.BackBufferWidth, pp.BackBufferHeight, 1, device.DisplayMode.Format);
            this.ppEffect = ppEffect;            
            spriteBatch = new SpriteBatch(device);
            
            InitPostProcessingVertices();
        }

        private void InitPostProcessingVertices()
        {
            ppVertices = new VertexPositionTexture[4];
            int i = 0;
            ppVertices[i++] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 0));
            ppVertices[i++] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 0));
            ppVertices[i++] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 1));
            ppVertices[i++] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 1));

            ppVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);
        }

        public void PostProcess(List<string> ppEffectsList)
        {
            for (int currentTechnique = 0; currentTechnique < ppEffectsList.Count; currentTechnique++)
            {
                device.SetRenderTarget(0, null);
                Texture2D textureRenderedTo;

                if (currentTechnique == 0)
                {
                    device.ResolveBackBuffer(resolveTexture, 0);
                    textureRenderedTo = resolveTexture;
                    ppEffect.Parameters["originalImage"].SetValue(textureRenderedTo);
                }
                else
                {
                    textureRenderedTo = targetRenderedTo.GetTexture();
                }

                if (currentTechnique == ppEffectsList.Count - 1)
                    device.SetRenderTarget(0, null);
                else
                    device.SetRenderTarget(0, targetRenderedTo);

                ppEffect.CurrentTechnique = ppEffect.Techniques[ppEffectsList[currentTechnique]];
                ppEffect.Begin();

                ppEffect.Parameters["textureToSampleFrom"].SetValue(textureRenderedTo);
                foreach (EffectPass pass in ppEffect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    device.VertexDeclaration = ppVertexDeclaration;
                    device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, ppVertices, 0, 2);
                    pass.End();
                }
                ppEffect.End();
            }
        }
    }
}
