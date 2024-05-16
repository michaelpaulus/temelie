using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Temelie.Repository.EntityFrameworkCore;
#if NETSTANDARD
public class EntityIdStringValueConverter<T> : ValueConverter<T, string>
{
    public EntityIdStringValueConverter(Func<T, string> getValue, Func<string, T> createNew) : base(
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

        private readonly Func<string, T> _createNew;

        public Generator(Func<string, T> createNew)
        {
            _createNew = createNew;
        }

        public override bool GeneratesTemporaryValues => true;

        public override T Next(EntityEntry entry)
        {
            return _createNew(Guid.NewGuid().ToString());
        }

    }

}
#endif
