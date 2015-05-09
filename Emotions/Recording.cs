using System.Collections.Generic;

namespace Emotions
{
    class Recording
    {
        public IEnumerable<InputSnapshot> Snapshots { get { return _snapshots; } }
        private readonly List<InputSnapshot> _snapshots;

        public Recording()
        {
            _snapshots = new List<InputSnapshot>();
        }

        public void Add(InputSnapshot snapshot)
        {
            _snapshots.Add(snapshot);
        }
    }
}