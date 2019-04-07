using PortraitBuilder.Model.Portrait;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PortraitBuilder.WinForms.UI
{
    public class DisplayCharacteristic : INotifyPropertyChanged
    {
        private readonly Characteristic _characteristic;
        public DisplayCharacteristic(Characteristic characteristic)
        {
            _characteristic = characteristic ?? throw new ArgumentNullException(nameof(characteristic));
        }

        public string Name => _characteristic.Name;

        public int Index => _characteristic.Index;

        public CharacteristicType Type => _characteristic.Type;

        public bool Custom => _characteristic.Custom;

        private bool randomizable;
        /// <summary>
        /// Whether the characteristic should be randomized when generating a random portrait.
        /// </summary>
        public bool Randomizable
        {
            get => randomizable;
            set
            {
                if(value != randomizable)
                {
                    randomizable = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString() => _characteristic.ToString();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static explicit operator Characteristic(DisplayCharacteristic dc) => dc._characteristic;
    }
}
