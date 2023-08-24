using System;
using System.ComponentModel;

namespace LibraryManagement.Models
{
    public class MyCashBox : INotifyPropertyChanged
    {
        private double _totalCash;

        public double TotalCash
        {
            get => _totalCash;
            set
            {
                if (_totalCash != value)
                {
                    _totalCash = value;
                    RaisePropertyChanged(nameof(TotalCash));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MyCashBox()
        {
            
        }

        public MyCashBox(double cashToInsert)
        {
            this.TotalCash = cashToInsert;
        }

        public void AddCash(double cashToAdd)
        {
            TotalCash += cashToAdd;
        }
    }
}
