using System;
using System.ComponentModel.Composition;
using Gemini.Framework;
using Gemini.Framework.Services;


namespace Emotions
{
    [Export(typeof(IModule))]
    class MainModule : ModuleBase
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public MainModule(IShell shell)
        {
            _shell = shell;
        }

        public override void Initialize()
        {
            MainWindow.Title = "Emotions";
            _shell.ActiveDocumentChanged += ShellOnActiveDocumentChanged;
            base.Initialize();
        }

        private void ShellOnActiveDocumentChanged(object sender, EventArgs eventArgs)
        {
            
        }
    }
}
