using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Temelie.Repository.EntityFrameworkCore;
#if NETSTANDARD
public class EntityIdLongValueConverter<T> : ValueConverter<T, long>
{
    public EntityIdLongValueConverter(Func<T, long> getValue, Func<long, T> createNew) : base(
        v => getValue(v),
        v => createNew(v),
         new ConverterMappingHints(valueGeneratorFactory: (prop, value) =>
         {
             return new Generator(createNew);
         }))
    {
    }

    private sealed class Generator : ValueGenerator<T>
    {
        private long _current = -9223372036854774808L;

        private readonly Func<long, T> _createNew;

        public Generator(Func<long, T> createNew)
        {
            _createNew = createNew;
        }

        public override bool GeneratesTemporaryValues => true;

        public override T Next(EntityEntry entry)
        {
            return _createNew(Interlocked.Increment(ref _current));
        }

    }

}
#endif
