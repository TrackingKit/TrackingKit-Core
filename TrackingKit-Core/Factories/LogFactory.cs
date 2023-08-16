using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackingKit_Core.TrackingKit_Core.Factories
{
    public static class LogFactory
    {
        public static Action Info(string @value) => default;

        public static Action Warning(string @value) => default;

        public static Action Error(string @value) => default;

    }
}
