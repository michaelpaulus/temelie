using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Temelie.Repository.EntityFrameworkCore;
#if NETSTANDARD
public class EntityIdGuidValueConverter<T> : ValueConverter<T, Guid>
{
    

    public EntityIdGuidValueConverter(Func<T, Guid> getValue, Func<Guid, T> createNew) : base(
        v => getValue(v),
        v => createNew(v)
        ,
         new ConverterMappingHints(valueGeneratorFactory: (prop, value) =>
         {
             return new Generator(createNew);
         }))
    {
    }


    private sealed class Generator : ValueGenerator<T>
    {
        private readonly Func<Guid, T> _createNew;

        public Generator(Func<Guid, T> createNew)
        {
            _createNew = createNew;
        }

        public override bool GeneratesTemporaryValues => true;

        public override T Next(EntityEntry entry)
        {
            return _createNew(Guid.NewGuid());
        }

    }
}
#endif
