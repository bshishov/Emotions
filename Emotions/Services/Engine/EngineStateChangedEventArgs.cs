namespace Emotions.Services.Engine
{
    class EngineStateChangedEventArgs
    {
        public readonly EngineState NewValue;
        public readonly EngineState OldValue;

        public EngineStateChangedEventArgs(EngineState oldValue, EngineState newValue)
        {
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}