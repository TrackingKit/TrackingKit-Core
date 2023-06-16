using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking
{
    /// <summary>
    /// Provides tracking of properties within specified groups.
    /// </summary>
    public interface ITracker : ITrackerReadOnly // ITrackerTickReadOnly, (Tick is more of a different context).
    {
        /// <summary> Are we currently scoped. </summary>
        bool IsScoped { get; }

        InputFilterSettings InputFilterSettings { get; }

        /// <summary>
        /// Begins a new tracking scope with the specified identifiers and tick.
        /// </summary>
        /// <param name="specificTick">The specificTick for tracking.</param>
        /// <param name="idents">The identifiers for the scope.</param>
        /// <returns>A read-only version of the tracker for this scope.</returns>
        ITrackerTickReadOnly Scope(int specificTick, params string[] idents);

        /// <summary>
        /// Begins a new tracking scope with the specified identifiers.
        /// </summary>
        /// <param name="idents">The identifiers for the scope.</param>
        /// <returns>A read-only version of the tracker for this scope.</returns>
        ITrackerReadOnly Scope(params string[] idents);

        ITrackerReadOnly Scope();

        /// <summary>
        /// Begins a new group with the specified identifiers.
        /// </summary>
        /// <param name="idents">The identifiers for the group.</param>
        void StartGroup(params string[] idents);

        /// <summary>
        /// Ends the group with the specified identifiers.
        /// </summary>
        /// <param name="idents">The identifiers for the group.</param>
        void EndGroup(params string[] idents);

        /// <summary>
        /// Sets the value of a property with the specified name within the current groups.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        /// <param name="idents">The identifiers for the property.</param>
        void Set(string propertyName, object value, params string[] idents);


    }
}
