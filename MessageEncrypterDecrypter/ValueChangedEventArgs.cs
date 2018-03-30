using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBoX
{
    class ValueChangedEventArgs
    {
        public readonly string LastValue;
        public readonly string NewValue;

        public ValueChangedEventArgs(string LastValue, string NewValue)
        {
            this.LastValue = LastValue;
            this.NewValue = NewValue;
        }
    }

    class Values
    {
        public Values(string InitialValue)
        {
            _value = InitialValue;
        }

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected virtual void OnValueChanged(ValueChangedEventArgs e)
        {
            if (ValueChanged != null)
                ValueChanged(this, e);
        }

        private string _value;

        public string Value
        {
            get { return _value; }
            set
            {
                string oldValue = _value;
                _value = value;
                OnValueChanged(new ValueChangedEventArgs(oldValue, _value));
            }
        }
    }
}
