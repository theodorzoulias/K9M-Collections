using System.Runtime.CompilerServices;
namespace K9M;

// Enumerator implementation.
public partial class KeyedCollection<TKey, TItem>
{
    // Implementing the IEnumerator<TItem> interface serves no practical purpose.
    // The Enumerator struct cannot be cast to IEnumerator<TItem>.
    public ref partial struct Enumerator
    {
        private KeyedCollection<TKey, TItem> _parent;
        private Entry[] _entries; // Replacing the array with a ref field is not viable.
        private int _index;
        private ushort _version;

        private const int DisposedFlag = -2;

        internal Enumerator(KeyedCollection<TKey, TItem> parent)
        {
            _parent = parent;
            _entries = parent._entries;
            _index = -1;
        }

        public partial bool MoveNext()
        {
            if (_index == DisposedFlag) ThrowHelper.ObjectDisposedException_Enumerator(typeof(Enumerator));
            return _parent.TryMoveNext(ref _index, ref _version);
        }

        public partial ref TItem Current
        {
            get
            {
                if (_index == DisposedFlag) ThrowHelper.ObjectDisposedException_Enumerator(typeof(Enumerator));
                if (_index >= 0 && _index < _entries.Length) return ref _entries[_index].Item;
                return ref Unsafe.NullRef<TItem>();
            }
        }

        public partial void Dispose()
        {
            _index = DisposedFlag;
            _parent = null;
            _entries = null;
        }
    }
}