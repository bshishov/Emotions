using System.IO;

namespace Emotions.KinectTools
{
    interface IStreamable
    {
        void ToStream(BinaryWriter writer);
        void FromStream(BinaryReader reader);
    }
}
