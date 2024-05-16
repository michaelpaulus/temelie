using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Temelie.Repository.EntityFrameworkCore;
#if NETSTANDARD
public class EntityIdIntValueConverter<T> : ValueConverter<T, int>
{
    public EntityIdIntValueConverter(Func<T, int> getValue, Func<int, T> createNew) : base(
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
        private int _current = -2147482648;

        private readonly Func<int, T> _createNew;

        public Generator(Func<int, T> createNew)
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
