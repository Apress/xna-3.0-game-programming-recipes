using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace TypeReaders
{
    public class MapData
    {
        public string mapName;
        public int numberOfCastles;
        public List<string> allies = new List<string>();
    }

    public class MapDataReader : ContentTypeReader<MapData>
    {
        protected override MapData Read(ContentReader input, MapData existingInstance)
        {
            MapData map = new MapData();
            map.mapName = input.ReadObject<string>();
            map.numberOfCastles = input.ReadObject<int>();
            map.allies = input.ReadObject<List<string>>();

            return map;
        }
    }
}
