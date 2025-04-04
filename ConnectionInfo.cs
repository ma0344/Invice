using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace Invoice
{
    public static class ConnectionInfo
    {
        private static readonly Lazy<IConfigurationSection> _lazyConfigurationSection = new(() =>
        {
            return (ConfigurationSection)new ConfigurationBuilder()
                 .SetBasePath(AppContext.BaseDirectory)
                 .AddIniFile("Connection.ini")
                 .Build()
                 .GetSection("ConnectionInfo");
        });

        private static IConfigurationSection ConfigurationSection => _lazyConfigurationSection.Value;

        public static MySqlConnectionStringBuilder Builder = new()
        {
            Server = ConfigurationSection["Server"] as string ?? "192.168.10.10",
            //"192.168.10.10",
            Port = uint.Parse((string)ConfigurationSection["Port"] ?? "3307"),
            UserID = ConfigurationSection["UserID"] as string ?? "application",
            Password = ConfigurationSection["Password"] as string ?? "Ma0344@GmailCom",
            Database = ConfigurationSection["Database"] as string ?? "invoice",
        };

            
    }
}
