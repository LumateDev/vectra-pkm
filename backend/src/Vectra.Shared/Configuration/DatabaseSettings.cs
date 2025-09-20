using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vectra.Shared.Configuration
{
    public class DatabaseSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5432;
        public string Name { get; set; } = "vectra";
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;

        // Дополнительные настройки EF Core
        public bool EnableSensitiveDataLogging { get; set; } = false;
        public bool EnableDetailedErrors { get; set; } = false;
        public int CommandTimeout { get; set; } = 30;

        public string GetConnectionString()
        {
            if (!string.IsNullOrEmpty(ConnectionString))
                return ConnectionString;

            return $"Host={Host};Port={Port};Database={Name};Username={Username};Password={Password}";
        }
    }
}
