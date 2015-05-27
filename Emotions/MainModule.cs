using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;


namespace Emotions
{
    [Export(typeof(IModule))]
    class MainModule : ModuleBase
    {
        [Import] private IShell _shell;

        public override void Initialize()
        {
            MainWindow.Title = "Emotions";
            _shell.ActiveDocumentChanged += ShellOnActiveDocumentChanged;
            base.Initialize();
        }

        private void ShellOnActiveDocumentChanged(object sender, EventArgs eventArgs)
        {
            
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
        }
    }
}
