using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gemini.Framework;
using Microsoft.Win32;

namespace Emotions.Modules.Spreadsheet.ViewModels
{
    [Export(typeof(SpreadsheetViewModel<>))]
    class SpreadsheetViewModel<T> : Document
    {
        private ObservableCollection<T> _data;
        private DataGrid _dataGrid;

        public ObservableCollection<T> Data
        {
            get { return _data; }
            private set
            {
                _data = value;
                NotifyOfPropertyChange(() => Data);
            }
        }

        public SpreadsheetViewModel(IEnumerable<T> items)
        {
            Data = new ObservableCollection<T>(items);
            DisplayName = string.Format("{0} Spreatsheet", typeof (T).Name);
        }

        public void OnDataGridItialized(object sender)
        {
            var dataGrid = sender as DataGrid;
            if(dataGrid == null)
                return;
            _dataGrid = dataGrid;
        }

        public void SaveToCsv()
        {
            if(_dataGrid == null)
                return;
            var dialog = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = ".csv",
                Filter = string.Format("Comma separated values {0}| *{0}", ".csv")
            };
            var result = dialog.ShowDialog();
            if (result != null && (bool)result)
            {
                _dataGrid.SelectAllCells();
                _dataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                ApplicationCommands.Copy.Execute(null, _dataGrid);
                var res = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                Clipboard.Clear();

                using (var writer = new StreamWriter(dialog.FileName))
                {
                    writer.Write("sep=,\n");
                    writer.Write(res);
                    writer.Close();
                }

                _dataGrid.UnselectAllCells();
            }            
        }
    }
}
