using System;
using System.Runtime.CompilerServices;

namespace DebounceMonitoring
{
    internal class ObjectTimeStamps
    {
        private ConditionalWeakTable<object, Ref<DateTime>>? _manyObjectStamps;
        private WeakReference? _singleRef;
        private DateTime _singleRefStamp;

        public ref DateTime GetOrAddRef(object obj)
        {
            if (_manyObjectStamps is not null)
                return ref _manyObjectStamps.GetValue(obj, createValueCallback: _ => new()).Value;

            if (_singleRef?.Target is not object singleObject)
            {
                _singleRef = new WeakReference(singleObject = obj);
            }

            if (singleObject == obj)
                return ref _singleRefStamp;

            var newStamp = new Ref<DateTime>();

            // Transform into the multi-object mode
            _manyObjectStamps = new ConditionalWeakTable<object, Ref<DateTime>>();
            _manyObjectStamps.Add(singleObject, new Ref<DateTime>(_singleRefStamp));
            _manyObjectStamps.Add(obj, newStamp);

            _singleRef = null;

            return ref newStamp.Value;
        }
    }
}