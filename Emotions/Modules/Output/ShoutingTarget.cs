using System;
using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using NLog;
using NLog.Targets;

namespace Emotions.Modules.Output
{
    [Target("ShoutingTarget")]
    class ShoutingTarget : TargetWithLayout
    {
        private Action<string> _addmethod;
        public ObservableCollection<string> Messages { get; set; }


        public ShoutingTarget()
        {
            Messages = new ObservableCollection<string>();
            _addmethod = Messages.Add;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            Application.Current.Dispatcher.BeginInvoke(_addmethod, this.Layout.Render(logEvent));
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            IoC.Get<ViewModels.OutputViewModel>().Register(this);
        }
    }
}
