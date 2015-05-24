using System.ComponentModel.Composition;
using Gemini.Framework;


namespace Emotions
{
    [Export(typeof(IModule))]
    class MainModule : ModuleBase
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
        }
    }
}
