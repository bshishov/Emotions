using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Gemini;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions
{
    class MainBootstrapper : AppBootstrapper
    {
        private readonly SimpleContainer _container = new SimpleContainer();

        protected override void Configure()
        {
            _container.Singleton<IEventAggregator, EventAggregator>();
            base.Configure();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            if (e.Args.Length > 0)
            {
                var shell = IoC.Get<IShell>();
                foreach (var document in GetEditors(e.Args[0]))
                {
                    shell.OpenDocument(document);    
                }
            }
        }

        private static IEnumerable<IDocument> GetEditors(string path)
        {
            return IoC.GetAllInstances(typeof(IEditorProvider))
                .Cast<IEditorProvider>()
                .Where(provider => provider.Handles(path))
                .Select(provider => provider.Create(path));
        }
    }
}
