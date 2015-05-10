using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Emotions.Modules.Output.Views;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.Modules.Output.ViewModels
{
    [Export(typeof(OutputViewModel))]
    class OutputViewModel : Tool
    {
        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Bottom; }
        }

        public OutputViewModel()
        {
            DisplayName = "Output";
            Targets = new ObservableCollection<ShoutingTarget>();
        }

        public ObservableCollection<ShoutingTarget> Targets { get; set; }

        private ShoutingTarget _active;
        public ShoutingTarget Active
        {
            get { return _active; }
            set
            {
                if (_active != null && _active.Messages != null)
                    _active.Messages.CollectionChanged -= OnCollectionChanged;
                _active = value;
                NotifyOfPropertyChange(() => Active);
                _active.Messages.CollectionChanged += OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            // SCROLL LISTBOX TO THE BOTTOM
            var view = this.GetView() as OutputView;
            if(view == null)
                return;
            
            var listbox = view.MainListBox;
            if (VisualTreeHelper.GetChildrenCount(listbox) > 0)
            {
                var border = (Border)VisualTreeHelper.GetChild(listbox, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }

        public void Register(ShoutingTarget target)
        {
            Targets.Add(target);
            if (Targets.Count == 1)
                Active = Targets.First();
            NotifyOfPropertyChange(() => Targets);
        }
    }
}
