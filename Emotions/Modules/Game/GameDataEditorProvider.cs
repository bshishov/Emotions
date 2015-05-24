using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Emotions.KinectTools.Readers;
using Emotions.Modules.Spreadsheet.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.Modules.Game
{
    [Export(typeof(IEditorProvider))]
    class GameDataEditorProvider : IEditorProvider
    {
        public bool Handles(string path)
        {
            return path.Contains("game.bin");
        }

        public IDocument Create(string path)
        {
            var frames = new List<GameFrame>();
            using (var reader = new StreamableReader<GameFrame>())
            {
                reader.Open(path);
                while (!reader.IsEnded)
                    frames.Add(reader.Read());
                reader.Close();
            }

            return new SpreadsheetViewModel<GameFrame>(frames);
        }
    }
}
