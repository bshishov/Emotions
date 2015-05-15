using System;
using System.ComponentModel.Composition;
using System.IO;
using Caliburn.Micro;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.Services.Engine
{
    [Export(typeof(IEditorProvider))]
    class RecordingProvider : IEditorProvider
    {
        [Import] private IEngineService _engine;
        private readonly ILog _log = LogManager.GetLog(typeof(RecordingProvider));

        public bool Handles(string path)
        {
            return Path.GetExtension(path) == ".erec";
        }

        public IDocument Create(string path)
        {
            try
            {
                _engine.LoadRecording(Recording.FromFile(path));
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            
            return null;
        }
    }
}
