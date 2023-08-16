namespace Tracking
{
    public interface IReadOnlyFilter<T>
    {
        public bool ShouldInclude(T ident);

        bool ShouldIncludes(IEnumerable<T> idents);

    }
}
