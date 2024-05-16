using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Temelie.Repository.EntityFrameworkCore;
#if NETSTANDARD
public class EntityIdShortValueConverter<T> : ValueConverter<T, short>
{
    public EntityIdShortValueConverter(Func<T, short> getValue, Func<short, T> createNew) : base(
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
        private int _current = -32668;

        private readonly Func<short, T> _createNew;

        public Generator(Func<short, T> createNew)
        {
            _createNew = createNew;
        }

        public override bool GeneratesTemporaryValues => true;

        public override T Next(EntityEntry entry)
        {
            return _createNew((short)Interlocked.Increment(ref _current));
        }

    }

}
#endif
