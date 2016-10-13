using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace BookCode
{
    interface ICameraInterface
    {
        Vector3 Position { get;}
        Vector3 Forward { get;}
        Vector3 UpVector { get;}

        Matrix ViewMatrix { get;}
        Matrix ProjectionMatrix { get;}
    }
}
