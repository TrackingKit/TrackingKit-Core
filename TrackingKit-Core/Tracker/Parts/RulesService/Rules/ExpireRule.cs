using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TrackingKit;

namespace Tracking.Rules
{
    public partial class ExpireRule : ITrackerRule
    {
        // This can be inaccurate if tickrate changes

        /// <summary> How much till it expires </summary>
        public int Seconds { get; set; } = 5;

        public bool? ShouldAdd(string propertyName, object obj)
        {
            throw new NotImplementedException();
        }


    }
}
