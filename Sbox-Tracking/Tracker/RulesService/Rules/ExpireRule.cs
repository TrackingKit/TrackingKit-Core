using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Sandbox;

namespace Tracking.Rules
{
    public partial class ExpireRule : TrackerRule
    {
        // This can be inaccurate if tickrate changes

        /// <summary> How much till it expires </summary>
        public int Seconds { get; set; } = 5;

        public override bool? ShouldDelete(TrackerKey key, object obj)
        {

            


            return base.ShouldDelete(key, obj);
        }

    }
}
