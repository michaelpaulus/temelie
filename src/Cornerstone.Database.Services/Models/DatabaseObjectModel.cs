namespace Cornerstone.Database
{
    namespace Models
    {
        public abstract class DatabaseObjectModel : Model
        {
            public abstract void AppendDropScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd);
            public abstract void AppendCreateScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd);

        }
    }

}
