using System;
using System.ComponentModel;

namespace PortraitBuilder.Model.Portrait
{

    /// <summary>
    /// Represents one DNA or Property element
    /// </summary>
    public class Characteristic : INotifyPropertyChanged
    {

        public Characteristic(string name, int index, CharacteristicType type, bool randomizable, bool custom = false)
        {
            this.Name = name;
            this.Index = index;
            this.Type = type;
            this.randomizable = randomizable;
            this.Custom = custom;
        }

        public string Name { get; }

        /// <summary>
        /// Index of attribute in dna/properties string
        /// </summary>
        public int Index { get; }

        public CharacteristicType Type { get; }

        /// <summary>
        /// Whether the characteristic should be randomized when generating a random portrait.
        /// </summary>
        private bool randomizable;

        public bool Randomizable
        {
            get { return randomizable; }
            private set
            {
                randomizable = value;
                InvokePropertyChanged(new PropertyChangedEventArgs(nameof(Randomizable)));
            }
        }

        /// <summary>
        /// Whether the characteristic is a non-vanilla one.
        /// </summary>
        public bool Custom { get; }

        public enum CharacteristicType
        {
            DNA,
            Property
        }

        public override bool Equals(object obj)
            => !(obj is Characteristic characteristic) ? false : Index.Equals(characteristic.Index) && Type.Equals(characteristic.Type);

        public override int GetHashCode() => Type.GetHashCode() + Index;

        public override string ToString() => $"{Name} ({(Type == CharacteristicType.DNA ? 'd' : 'p')}{Index})";

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void InvokePropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        #endregion
    }
}
