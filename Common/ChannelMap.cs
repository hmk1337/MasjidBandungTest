using System.Collections;

namespace MasjidBandung.Common;

/// <summary>
/// Pada dasarnya membuat peta index terhadap motor
/// </summary>
public sealed class ChannelMap : IDictionary<int, MotorInfo> {
    private readonly MotorInfo[] _map;

    public ChannelMap(int motorCount) {
        _map = new MotorInfo[motorCount];
    }

    private void SetValue(int index, MotorInfo value) {
        if (index != value.Index) throw new InvalidOperationException();
        _map[index] = value;
    }

    /// <summary>
    /// Mendapatkan motor pada nomor tertentu
    /// </summary>
    /// <param name="index">Nomor motor</param>
    /// <returns>Motor yang bersangkutan</returns>
    private MotorInfo GetValue(int index) {
        return _map[index];
    }

    private bool IndexValid(int index) => index >= 0 && index < _map.Length;


    // IEnumerator<MotorInfo> IEnumerable<MotorInfo>.GetEnumerator() => GetEnumerator();
    IEnumerator<KeyValuePair<int, MotorInfo>> IEnumerable<KeyValuePair<int, MotorInfo>>.GetEnumerator() => throw new NotSupportedException();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public Enumerator GetEnumerator() => new(this);

    /// <summary>
    /// Disarankan menggunakan <see cref="Add(int,MasjidBandung.Common.MotorInfo)"/>
    /// </summary>
    public void Add(KeyValuePair<int, MotorInfo> item) => SetValue(item.Key, item.Value);

    public void Add(MotorInfo item) {
        SetValue(item.Index, item);
    }

    public void Clear() {
        for (int i = 0; i < _map.Length; i++) {
            _map[i] = null!;
        }
    }

    public bool Contains(MotorInfo item) {
        for (int i = 0; i < _map.Length; i++) {
            if (_map[i].Equals(item)) return true;
        }

        return false;
    }

    public void CopyTo(MotorInfo[] array, int arrayIndex) {
        if (array.Length - arrayIndex < _map.Length) throw new IndexOutOfRangeException();
        for (int i = 0; i < _map.Length; i++) {
            array[arrayIndex] = _map[i];
            arrayIndex++;
        }
    }

    public bool Remove(MotorInfo item) {
        for (int i = 0; i < _map.Length; i++) {
            if (!_map[i].Equals(item)) continue;
            _map[i] = null!;
            return true;
        }

        return false;
    }

    public bool Contains(KeyValuePair<int, MotorInfo> item) {
        (int key, var value) = item;
        return _map[key].Equals(value);
    }

    public void CopyTo(KeyValuePair<int, MotorInfo>[] array, int arrayIndex) {
        if (array.Length - arrayIndex < _map.Length) throw new IndexOutOfRangeException();
        for (int i = 0; i < _map.Length; i++) { // foreach (var item in _map) {
            array[arrayIndex] = new KeyValuePair<int, MotorInfo>(i, _map[i]);
            arrayIndex++;
        }
    }

    public bool Remove(KeyValuePair<int, MotorInfo> item) {
        (int key, var value) = item;
        if (!IndexValid(key)) return false;
        if (!_map[key].Equals(value)) return false;
        _map[key] = null!;
        return true;
    }

    public int Count => _map.Length;
    public bool IsReadOnly => true;
    public void Add(int key, MotorInfo value) => SetValue(key, value);
    public bool ContainsKey(int key) => IndexValid(key) && _map[key] != null!;

    public bool Remove(int key) {
        if (!IndexValid(key)) return false;
        _map[key] = null!;
        return true;
    }

    public bool TryGetValue(int key, out MotorInfo value) {
        if (!IndexValid(key)) {
            value = null!;
            return false;
        }

        value = _map[key];
        return true;
    }

    public int IndexOf(MotorInfo item) {
        for (int i = 0; i < _map.Length; i++) {
            if (_map[i].Equals(item)) return i;
        }

        return -1;
    }

    public void Insert(int index, MotorInfo item) => SetValue(index, item);

    public void RemoveAt(int index) {
        if (!IndexValid(index)) throw new IndexOutOfRangeException();
        _map[index] = null!;
    }

    public MotorInfo this[int key] {
        get => GetValue(key);
        set => SetValue(key, value);
    }

    public ICollection<int> Keys => Enumerable.Range(0, _map.Length).ToList();
    public ICollection<MotorInfo> Values => _map;

    public struct Enumerator : IEnumerator<MotorInfo> {
        private readonly ChannelMap _map;
        private int _current;

        public Enumerator(ChannelMap map) {
            _map = map;
            _current = -1;
        }


        public bool MoveNext() {
            _current++;
            return _current < _map.Count;
        }

        public void Reset() => _current = -1;
        public MotorInfo Current => _map[_current];

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}
