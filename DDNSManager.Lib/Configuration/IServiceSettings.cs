using System;

namespace DDNSManager.Lib.Configuration
{
    public interface IServiceSettings : IEquatable<IServiceSettings>
    {
        /// <summary>
        /// Id of the <see cref="IDDNSService"/> that uses these settings.
        /// </summary>
        string ServiceId { get; }
        /// <summary>
        /// User-configurable name of the <see cref="IServiceSettings"/>.
        /// </summary>
        string? Name { get; }
        /// <summary>
        /// Hostname for the target DNS record.
        /// </summary>
        string? Hostname { get; }
        /// <summary>
        /// IP address to associate with <see cref="Hostname"/>. 
        /// If null, uses the external IP of the current device.
        /// </summary>
        string? IP { get; }
        /// <summary>
        /// Whether this <see cref="IServiceSettings"/> is active or not.
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// Returns true if this <see cref="IServiceSettings"/> has a valid set of settings.
        /// </summary>
        bool IsValid();
    }
}
