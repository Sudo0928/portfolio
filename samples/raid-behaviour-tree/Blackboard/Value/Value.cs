using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ProjectRaid.Runtime.BehaviourTree
{
    [Serializable]
    public class Value<T> : IValue
    {
        public T value { get; set; }
        public Type Type => typeof(T);

        public virtual string ValueString
        {
            get
            {
                object boxed = value;
                return boxed?.ToString() ?? string.Empty;
            }
            set { this.value = Parse(value); }
        }

        public virtual void SetValue(string s) => this.value = Parse(s);

        public virtual bool CanParse(string s)
        {
            try { Parse(s); return true; }
            catch { return false; }
        }

        public virtual bool TryGet<TOut>(out TOut value)
        {
            if(this is Value<TOut> typed)
            {
                value = typed.value;
                return true;
            }
            value = default;
            return false;
        }

        public virtual IValue Clone() => new Value<T> { value = this.value };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Parse(string s)
        {
            var t = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (t == typeof(string))
                return (T)(object)(s ?? string.Empty);

            if (string.IsNullOrWhiteSpace(s))
                return default;

            if (t.IsEnum)
                return (T)Enum.Parse(t, s, ignoreCase: true);

            return (T)Convert.ChangeType(s, t, CultureInfo.InvariantCulture);
        }
    }
}
