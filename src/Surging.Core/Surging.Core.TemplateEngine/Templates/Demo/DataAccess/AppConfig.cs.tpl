using System;
using System.Collections.Generic;
using System.Text;

namespace {{ prefix }}.DataAccess.{{ project.name }}
{
    public class AppConfig
    {
        private static {{ project.name }}DataOption _serverOptions = new {{ project.name }}DataOption();
        public static  {{ project.name }}DataOption? {{ project.name }}DataOptions
        {
            get
            {
                return _serverOptions;
            }
            set
            {
                _serverOptions = value;
            }
        }
    }
}
