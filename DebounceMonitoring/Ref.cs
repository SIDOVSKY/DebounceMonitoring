namespace DebounceMonitoring
{
    internal class Ref<T> where T : struct
    {
        public T Value;

        public Ref(T value = default)
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
    }
}