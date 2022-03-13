using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace DDNSManager.Lib.Configuration
{
    [Serializable]
    public class ManagerSettings : NotifyBase, IEquatable<ManagerSettings>
    {
        [JsonIgnore]
        public string? SettingsPath { get; set; }
        [JsonInclude]
        public Interval Interval { get; set; }
        [JsonIgnore]
        public IList<IServiceSettings> ServiceSettings { get; set; }

        [JsonIgnore]
        public int EnabledProfiles => ServiceSettings.Where(s => s.Enabled).Count();

        /// <summary>
        /// Used for the serializer.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("ServiceSettings")]
        [JsonConverter(typeof(ServiceSettingsCollectionConverter))]
        public IEnumerable<IServiceSettings> SerializableSettings
        {
            get => ServiceSettings ?? Array.Empty<IServiceSettings>();
            private set
            {
                if (value is ObservableCollection<IServiceSettings> settings)
                    ServiceSettings = settings;
                else
                    ServiceSettings = new ObservableCollection<IServiceSettings>(value);
            }
        }

        public ManagerSettings()
        {
            Interval = new Interval();
            ServiceSettings = new ObservableCollection<IServiceSettings>();
        }
        [JsonConstructor]
        public ManagerSettings(Interval? interval, IEnumerable<IServiceSettings> serializableSettings)
        {
            Interval = interval ?? new Interval();
            ServiceSettings = new ObservableCollection<IServiceSettings>(serializableSettings) ?? new ObservableCollection<IServiceSettings>();
        }

        public bool Equals(ManagerSettings? obj)
        {
            if (obj is ManagerSettings other && Interval.Equals(other.Interval)
                && ServiceSettings.Count == other.ServiceSettings.Count)
            {
                for (int i = 0; i < ServiceSettings.Count; i++)
                {
                    if (!ServiceSettings[i].Equals(other.ServiceSettings[i]))
                        return false;
                }
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public class Interval : NotifyBase, IEquatable<Interval?>
    {
        private int _minutes = 0;

        public int Minutes
        {
            get => _minutes;
            set
            {
                if (_minutes == value) return;
                if (value < 0)
                    _minutes = 0;
                else
                {
                    while (value >= 60)
                    {
                        value -= 60;
                        Hours++;
                    }
                    _minutes = value;
                }
                NotifyPropertyChanged();
            }
        }

        private int _hours = 6;

        public int Hours
        {
            get => _hours;
            set
            {
                if (_hours == value) return;
                if (value < 0)
                    _hours = 0;
                else
                {
                    while (value >= 24)
                    {
                        value -= 24;
                        Days++;
                    }
                    _hours = value;
                }
                NotifyPropertyChanged();
            }
        }

        private int _days = 0;

        public int Days
        {
            get => _days;
            set
            {
                if (_days == value) return;
                _days = value;
                NotifyPropertyChanged();
            }
        }
        public Interval() { }

        [JsonConstructor]
        public Interval(int minutes, int hours, int days)
        {
            Days = days;
            Hours = hours;
            Minutes = minutes;
        }

        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan(Days, Hours, Minutes, 0);
        }

        public bool Equals(Interval? other)
        {
            return other != null &&
                   Minutes == other.Minutes &&
                   Hours == other.Hours &&
                   Days == other.Days;
        }
    }
}
