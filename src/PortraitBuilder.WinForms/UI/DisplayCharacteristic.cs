using PortraitBuilder.Model.Portrait;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PortraitBuilder.WinForms.UI
{
    public class DisplayCharacteristic : Characteristic, INotifyPropertyChanged
    {
        public DisplayCharacteristic(Characteristic c) : base(c.Name, c.Index, c.Type, c.Custom) { }

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
