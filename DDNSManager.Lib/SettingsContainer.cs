using DDNSManager.Lib.Configuration;

namespace DDNSManager.Lib;
public class SettingsContainer
{
    public IServiceSettings? Settings { get; private set; }
    public void SetSettings(IServiceSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }
    public TSettings GetSettings<TSettings>() where TSettings : class, IServiceSettings, new()
    {
        if (Settings == null)
        {
            throw new InvalidOperationException("Settings are not initialized.");
        }
        if (Settings is TSettings settings)
        {
            return settings;
        }
        throw new InvalidCastException($"Cannot cast {Settings.GetType().Name} to {typeof(TSettings).Name}");
    }
}
