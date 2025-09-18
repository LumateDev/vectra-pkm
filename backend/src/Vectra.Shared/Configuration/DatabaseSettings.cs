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

        public string GetConnectionString() =>
            $"Host={Host};Port={Port};Database={Name};Username={Username};Password={Password}";
    }
}
