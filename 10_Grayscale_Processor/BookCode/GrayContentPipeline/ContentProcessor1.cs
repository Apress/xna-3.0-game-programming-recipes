using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace GrayContentPipeline
{
    [ContentProcessor(DisplayName = "GrayScaleProcessor")]
    public class ExtentedTextureProcessor : TextureProcessor
    {
        private float interpolation = 0.8f;
        public float Interpolation
        {
            get { return interpolation; }
            set { interpolation = value; }
        }

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            TextureContent texContent = base.Process(input, context);
            texContent.ConvertBitmapType(typeof(PixelBitmapContent<Color>));

            for (int face = 0; face < input.Faces.Count; face++)
            {
                MipmapChain mipChain = input.Faces[face];
                for (int mipLevel = 0; mipLevel < mipChain.Count; mipLevel++)
                {
                    PixelBitmapContent<Color> oldImage = (PixelBitmapContent<Color>)input.Faces[face][mipLevel];
                    PixelBitmapContent<Color> grayImage = new PixelBitmapContent<Color>(oldImage.Width, oldImage.Height);

                    for (int x = 0; x < oldImage.Width; x++)
                        for (int y = 0; y < oldImage.Height; y++)
                        {
                            Color oldColor = oldImage.GetPixel(x, y);
                            float grayValue = oldColor.R * 0.299f / 255.0f;
                            grayValue += oldColor.G * 0.596f / 255.0f;
                            grayValue += oldColor.B * 0.211f / 255.0f;
                            float alpha = oldColor.A / 255.0f;

                            Color grayColor = new Color(grayValue, grayValue, grayValue, alpha);
                            Color newColor = Color.Lerp(oldColor, grayColor, interpolation);
                            grayImage.SetPixel(x, y, newColor);                            
                        }

                    input.Faces[face][mipLevel] = grayImage;
                }
            }

            return texContent;
        }
    }
}
