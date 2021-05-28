namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text.Json;

    public class Settings : INotifyPropertyChanged
    {
        public static readonly string SettingsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/Sideways.cfg");
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            AllowTrailingCommas = true,
            WriteIndented = true,
        };

        private AlphaVantageSettings? alphaVantage;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AlphaVantageSettings? AlphaVantage
        {
            get => this.alphaVantage;
            set
            {
                if (ReferenceEquals(value, this.alphaVantage))
                {
                    return;
                }

                this.alphaVantage = value;
                this.OnPropertyChanged();
            }
        }

        public static Settings FromFile()
        {
            if (ReadLegacyApiKey() is { } apiKey)
            {
                var settings = new Settings
                {
                    AlphaVantage = new AlphaVantageSettings
                    {
                        ClientSettings = new AlphaVantageClientSettings
                        {
                            ApiKey = apiKey,
                            MaxCallsPerMinute = 5,
                        },
                    },
                };

                File.WriteAllText(SettingsFile, JsonSerializer.Serialize(settings, SerializerOptions));
                return settings;
            }

            if (File.Exists(SettingsFile))
            {
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsFile), SerializerOptions) ?? throw new InvalidOperationException("Empty settings.");
            }

            return new Settings();

            static string? ReadLegacyApiKey()
            {
                var apiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/AlphaVantage.key");
                if (File.Exists(apiKeyFile))
                {
                    var key = File.ReadAllText(apiKeyFile).Trim();
                    File.Delete(apiKeyFile);
                    return key;
                }

                return null;
            }
        }

        public void Save()
        {
            File.WriteAllText(SettingsFile, JsonSerializer.Serialize(this));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
