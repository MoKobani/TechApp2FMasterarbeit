using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL
{
    internal class AppSettings : ConfigurationSection
    {
        [ConfigurationProperty("user", DefaultValue = "2F")]
        public string User
        {
            get { return (string)this["user"]; }
            set { this["user"] = value; }
        }


        [ConfigurationProperty("tamplatesPath", DefaultValue = "G:TK")]
        public string TamplatesPath { get => (string)this["tamplatesPath"]; set => this["tamplatesPath"] = value; }

        [ConfigurationProperty("serverIP", DefaultValue = "127.0.0.1")]
        public string ServerIP { get => (string)this["serverIP"]; set => this["serverIP"] = value; }

        [ConfigurationProperty("portNumber", DefaultValue = 6030 )]
        public int PortNumber { get => (int)this["portNumber"]; set => this["portNumber"] = value; }

        [ConfigurationProperty("dbDescriptor", DefaultValue = "2:1")]
        public string DbDescriptor { get => (string)this["dbDescriptor"]; set => this["dbDescriptor"] = value; }

        
    }
}
