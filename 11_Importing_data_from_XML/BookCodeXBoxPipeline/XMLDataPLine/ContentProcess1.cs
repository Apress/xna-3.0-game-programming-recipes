using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.IO;
using TypeReaders;

namespace XMLDataPLine
{
    [ContentTypeWriter]
    public class MapDataTypeWriter : ContentTypeWriter<MapData>
    {
        protected override void Write(ContentWriter output, MapData value)
        {
            output.WriteObject<string>(value.mapName);
            output.WriteObject<int>(value.numberOfCastles);
            output.WriteObject<List<string>>(value.allies);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(MapDataReader).AssemblyQualifiedName;
        }
    }    
}
