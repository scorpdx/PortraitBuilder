using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PortraitBuilder.Model.Portrait
{

    /// <summary>
    /// Represents one DNA or Property element
    /// </summary>
    public class Characteristic : IEquatable<Characteristic>, INotifyPropertyChanged
    {
        public Characteristic(string name, int index, CharacteristicType type, bool randomizable = false, bool custom = false)
        {
            this.Name = name;
            this.Index = index;
            this.Type = type;
            this.Randomizable = randomizable;
            this.Custom = custom;
        }

        public string Name { get; }

        /// <summary>
        /// Index of attribute in dna/properties string
        /// </summary>
        public int Index { get; }

        public CharacteristicType Type { get; }

        private bool randomizable;
        /// <summary>
        /// Whether the characteristic should be randomized when generating a random portrait.
        /// </summary>
        public bool Randomizable
        {
            get => randomizable;
            set
            {
                if (value != randomizable)
                {
                    randomizable = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether the characteristic is a non-vanilla one.
        /// </summary>
        public bool Custom { get; }

        public bool Equals(Characteristic other)
        {
            return other != null
                && Name == other.Name
                && Index == other.Index
                && Type == other.Type
                && Custom == other.Custom;
        }

        public override int GetHashCode()
        {
            var hashCode = 1047414484;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Custom.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj) => Equals(obj as Characteristic);

        public override string ToString() => $"{Name} ({(Type == CharacteristicType.DNA ? 'd' : 'p')}{Index})";

        public static bool operator ==(Characteristic left, Characteristic right) => EqualityComparer<Characteristic>.Default.Equals(left, right);

        public static bool operator !=(Characteristic left, Characteristic right) => !(left == right);

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
