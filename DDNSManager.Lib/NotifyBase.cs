using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DDNSManager.Lib
{
    public abstract class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
