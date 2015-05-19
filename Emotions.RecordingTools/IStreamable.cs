using System.IO;

namespace Emotions.RecordingTools
{
    public interface IStreamable
    {
        void ToStream(BinaryWriter writer);
        void FromStream(BinaryReader reader);
    }
}
