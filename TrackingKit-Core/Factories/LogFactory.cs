using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackingKit_Core.TrackingKit_Core.Factories
{
    public static class LogFactory
    {
        public static Action Info(object @value) => default;

        public static Action Warning(object @value) => default;

        public static Action Error(object @value) => default;

    }
}
