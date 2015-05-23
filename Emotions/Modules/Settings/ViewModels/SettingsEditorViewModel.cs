using System.ComponentModel.Composition;
using Caliburn.Micro;
using Gemini.Modules.Settings;

namespace Emotions.Modules.Settings.ViewModels
{
    [Export(typeof(ISettingsEditor))]
    class SettingsEditorViewModel : PropertyChangedBase, ISettingsEditor
    {
        public void ApplyChanges()
        {

        }

        public string SettingsPageName { get { return "Application"; } }
        public string SettingsPagePath { get { return "Application"; } }
    }
}
