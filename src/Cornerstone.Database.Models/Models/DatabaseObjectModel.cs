namespace Cornerstone.Database
{
    namespace Models
    {
        public abstract class DatabaseObjectModel : Model
        {
            public abstract void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd);
            public abstract void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd);

        }
    }

}
