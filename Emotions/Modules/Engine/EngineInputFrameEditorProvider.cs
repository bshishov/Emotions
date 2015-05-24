using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
        public bool Handles(string path)
        {
            return path.Contains("engine.bin");
        }

        public IDocument Create(string path)
        {
            const int limit = 500;
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
