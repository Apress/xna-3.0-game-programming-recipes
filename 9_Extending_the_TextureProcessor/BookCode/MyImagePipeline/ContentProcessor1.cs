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

namespace MyImagePipeline
{
    [ContentProcessor(DisplayName = "ExtendedExample")]
    public class ExtentedTextureProcessor : TextureProcessor
    {
        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            TextureContent texContent = base.Process(input, context);
            texContent.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            
            for (int face = 0; face < texContent.Faces.Count; face++)
            {
                MipmapChain mipChain = texContent.Faces[face];
                for (int mipLevel = 0; mipLevel < mipChain.Count; mipLevel++)
                {
                    PixelBitmapContent<Color> image = (PixelBitmapContent<Color>)input.Faces[face][mipLevel];
                    Color toReplace = new Color(81, 92, 164);
                    image.ReplaceColor(toReplace, Color.Yellow);
                }
            }

            return texContent;
        }
    }
}