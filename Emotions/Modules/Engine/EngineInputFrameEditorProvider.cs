using System.Collections.Generic;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Emotions.KinectTools.Readers;
using Emotions.KinectTools.Tracking;
using Emotions.Modules.Spreadsheet.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.Modules.Engine
{
    [Export(typeof(IEditorProvider))]
    class EngineInputFrameEditorProvider : IEditorProvider
    {
        private readonly ILog _log = LogManager.GetLog(typeof(EngineInputFrameEditorProvider));

        public bool Handles(string path)
        {
            return path.Contains("engine.bin");
        }

        public IDocument Create(string path)
        {
            const int limit = 1500;
            _log.Warn("Displaying only {0} rows", limit);
            var frames = new List<EngineInputFrame>();
            using (var reader = new StreamableReader<EngineInputFrame>())
            {
                reader.Open(path);
                while (!(reader.IsEnded || frames.Count > limit))
                    frames.Add(reader.Read());
                reader.Close();
            }

            return new SpreadsheetViewModel<EngineInputFrame>(frames);
        }
    }
}
