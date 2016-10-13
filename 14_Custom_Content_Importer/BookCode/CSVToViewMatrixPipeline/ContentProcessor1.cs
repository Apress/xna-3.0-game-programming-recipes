using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace CVSToViewMatrixPipeline
{
    public class CSVImporterToProcessor
    {
        private Vector3 position;
        private Vector3 target;

        public Vector3 Position { get { return position; } }
        public Vector3 Target { get { return target; } }

        public CSVImporterToProcessor(Vector3 position, Vector3 target)
        {
            this.position = position;
            this.target = target;
        }
    }

    [ContentImporter(".csv", DefaultProcessor = "CSVMatrixProcessor")]
    public class CSVImporter : ContentImporter<CSVImporterToProcessor>
    {
        public override CSVImporterToProcessor Import(string filename, ContentImporterContext context)
        {
            StreamReader file = new StreamReader(filename);

            string line = file.ReadLine();
            string[] lineData = line.Split(';');
            float x = float.Parse(lineData[0]);
            float y = float.Parse(lineData[1]);
            float z = float.Parse(lineData[2]);
            Vector3 position = new Vector3(x,y,z);

            line = file.ReadLine();
            lineData = line.Split(';');
            x = float.Parse(lineData[0]);
            y = float.Parse(lineData[1]);
            z = float.Parse(lineData[2]);
            Vector3 target = new Vector3(x,y,z);

            CSVImporterToProcessor finalData = new CSVImporterToProcessor(position, target);

            return finalData;
        }
    }

    [ContentProcessor]
    public class CSVMatrixProcessor : ContentProcessor<CSVImporterToProcessor, Matrix>
    {
        public override Matrix Process(CSVImporterToProcessor input, ContentProcessorContext context)
        {
            Vector3 up = new Vector3(0, 1, 0);
            Vector3 forward = input.Target - input.Position;
            Vector3 right = Vector3.Cross(forward, up);
            up = Vector3.Cross(right, forward);

            Matrix viewMatrix = Matrix.CreateLookAt(input.Position, input.Target, up);
            return viewMatrix;
        }
    }
}