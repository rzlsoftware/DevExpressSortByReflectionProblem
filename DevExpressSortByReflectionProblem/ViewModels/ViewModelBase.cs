using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DevExpressSortByReflectionProblem
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public virtual bool Set<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (field == null || !EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                RaisePropertyChanged(propertyName);
                return true;
            }
            return false;
        }
    }
}
