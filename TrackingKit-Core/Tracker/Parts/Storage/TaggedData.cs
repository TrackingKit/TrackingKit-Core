namespace Tracking
{
    public class TaggedData<TObject> 
    {
        public TObject Object { get; }
        public IReadOnlyList<string> Tags { get; }

        public TaggedData(TObject @object)
            : this(@object, Enumerable.Empty<string>())
        { }

        public TaggedData(TObject @object, IEnumerable<string> tags)
        {
            Object = @object ?? throw new ArgumentNullException(nameof(@object));
            Tags = (tags ?? throw new ArgumentNullException(nameof(tags))).ToList().AsReadOnly();
        }

        public TaggedData<TObject> CastObject<TObject>()
            where TObject : notnull
        {
            if (Object is not TObject castedObject)
            {
                throw new InvalidCastException($"The object of type {typeof(TObject).Name} cannot be cast to {typeof(TObject).Name}, as the object is {Object.GetType()}.");
            }
            return new TaggedData<TObject>(castedObject, Tags);
        }
    }
}
