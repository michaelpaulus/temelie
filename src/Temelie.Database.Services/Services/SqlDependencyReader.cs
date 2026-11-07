using System.Text;

namespace Temelie.Database.Services;

internal static class SqlDependencyReader
{
    internal sealed record Reference(string? Schema, string Name, string? Database = null);
    private sealed record Token(string Text, bool Identifier = false, bool Quoted = false);

    internal static bool IsSchemaBound(string? sql) => Tokenize(sql ?? "", false)
        .Any(t => !t.Quoted && t.Text.Equals("SCHEMABINDING", StringComparison.OrdinalIgnoreCase));

    internal static IEnumerable<Reference> Read(string? sql, bool tableChangesOnly = false, bool mysql = false)
    {
        var tokens = Tokenize(sql ?? "", mysql);
        var fromDepths = new HashSet<int>();
        var ctes = new List<(string Name, int Start, int End)>();
        for (var i = 0; i + 2 < tokens.Count; i++)
        {
            if (tokens[i].Text.Equals("WITH", StringComparison.OrdinalIgnoreCase) || tokens[i].Text == ",")
            {
                var name = i + 1;
                if (tokens[name].Text.Equals("RECURSIVE", StringComparison.OrdinalIgnoreCase))
                {
                    name++;
                }

                var end = name + 1;
                if (end < tokens.Count && tokens[end].Text == "(")
                {
                    while (end < tokens.Count && tokens[end].Text != ")")
                    {
                        end++;
                    }

                    end++;
                }

                if (tokens[name].Identifier && end + 1 < tokens.Count &&
                    tokens[end].Text.Equals("AS", StringComparison.OrdinalIgnoreCase) && tokens[end + 1].Text == "(")
                {
                    var statementEnd = tokens.FindIndex(end, t => t.Text == ";");
                    ctes.Add((tokens[name].Text, i, statementEnd < 0 ? tokens.Count : statementEnd));
                }
            }
        }

        var depth = 0;
        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Text == "(")
            {
                depth++;
            }

            if (token.Text == ")")
            {
                fromDepths.Remove(depth);
                depth--;
            }

            var keyword = token.Quoted ? "" : token.Text.ToUpperInvariant();
            if (keyword is "WHERE" or "GROUP" or "ORDER" or "HAVING" or "UNION" or "EXCEPT" or "INTERSECT" or "SET" or ";")
            {
                fromDepths.Remove(depth);
            }

            if (keyword == "FROM")
            {
                fromDepths.Add(depth);
            }

            var start = i + 1;
            var context = keyword is "FROM" or "JOIN" or "APPLY" or "UPDATE" or "INTO" or "EXEC" or "EXECUTE" or "CALL" or "MERGE" or "REFERENCES";
            if (keyword == "TABLE" && i > 0)
            {
                context = tokens[i - 1].Text.ToUpperInvariant() is "ALTER" or "DROP" or "CREATE" or "TRUNCATE" or "RENAME";
            }

            if (tableChangesOnly)
            {
                if (keyword != "TABLE" || !context)
                {
                    continue;
                }
            }
            else
            {
                context |= token.Text == "," && fromDepths.Contains(depth);
                if (!context)
                {
                    start = i;
                    if (!token.Identifier || (i > 0 && tokens[i - 1].Text == "."))
                    {
                        continue;
                    }

                    if (!token.Quoted && keyword is "AS" or "EXISTS" or "IN" or "VALUES" or "WITH" or "OVER")
                    {
                        continue;
                    }
                }
            }

            if (start >= tokens.Count || !tokens[start].Identifier)
            {
                continue;
            }

            if (keyword is "EXEC" or "EXECUTE" && tokens[start].Text.StartsWith('@') &&
                start + 1 < tokens.Count && tokens[start + 1].Text == "=")
            {
                start += 2;
            }

            if (tokens[start].Text.Equals("IF", StringComparison.OrdinalIgnoreCase))
            {
                start++;
                if (start < tokens.Count && tokens[start].Text.Equals("NOT", StringComparison.OrdinalIgnoreCase))
                {
                    start++;
                }

                if (start < tokens.Count && tokens[start].Text.Equals("EXISTS", StringComparison.OrdinalIgnoreCase))
                {
                    start++;
                }
            }

            if (start >= tokens.Count || !tokens[start].Identifier)
            {
                continue;
            }

            var parts = new List<string> { tokens[start].Text };
            var end = start + 1;
            while (end < tokens.Count && tokens[end].Text == ".")
            {
                end++;
                if (end < tokens.Count && tokens[end].Text == ".")
                {
                    parts.Add("");
                }
                else if (end < tokens.Count && tokens[end].Identifier)
                {
                    parts.Add(tokens[end++].Text);
                }
                else
                {
                    break;
                }
            }

            var isCall = end < tokens.Count && tokens[end].Text == "(";
            if (!context && !isCall)
            {
                continue;
            }

            if (parts[0].StartsWith('@') || parts[0].StartsWith('#'))
            {
                continue;
            }

            if (parts.Count == 1 && ctes.Any(cte => start >= cte.Start && start < cte.End &&
                cte.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (parts.Count > 3)
            {
                continue;
            }

            yield return new Reference(parts.Count >= 2 ? parts[^2] : null, parts[^1],
                parts.Count == 3 ? parts[0] : null);
        }
    }

    private static List<Token> Tokenize(string sql, bool mysql)
    {
        var tokens = new List<Token>();
        for (var i = 0; i < sql.Length;)
        {
            var c = sql[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if ((c == '-' && i + 1 < sql.Length && sql[i + 1] == '-') || (mysql && c == '#'))
            {
                while (i < sql.Length && sql[i] != '\n')
                {
                    i++;
                }

                continue;
            }

            if (c == '/' && i + 1 < sql.Length && sql[i + 1] == '*')
            {
                i += 2;
                var nesting = 1;
                while (i < sql.Length && nesting > 0)
                {
                    if (i + 1 < sql.Length && sql[i] == '/' && sql[i + 1] == '*')
                    {
                        nesting++;
                        i += 2;
                    }
                    else if (i + 1 < sql.Length && sql[i] == '*' && sql[i + 1] == '/')
                    {
                        nesting--;
                        i += 2;
                    }
                    else
                    {
                        i++;
                    }
                }

                continue;
            }

            if (c is '\'' or '[' or '`' or '"')
            {
                var close = c == '[' ? ']' : c;
                var text = new StringBuilder();
                i++;
                while (i < sql.Length)
                {
                    if (sql[i] == close)
                    {
                        i++;
                        if (i < sql.Length && sql[i] == close)
                        {
                            text.Append(close);
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (mysql && c == '\'' && sql[i] == '\\' && i + 1 < sql.Length)
                    {
                        i += 2;
                    }
                    else
                    {
                        text.Append(sql[i++]);
                    }
                }

                tokens.Add(c == '\'' ? new Token("") : new Token(text.ToString(), true, true));
                continue;
            }

            if (char.IsLetter(c) || c is '_' or '@' or '#')
            {
                var start = i++;
                while (i < sql.Length && (char.IsLetterOrDigit(sql[i]) || sql[i] is '_' or '$' or '@' or '#'))
                {
                    i++;
                }

                tokens.Add(new Token(sql[start..i], true));
            }
            else
            {
                tokens.Add(new Token(sql[i++].ToString()));
            }
        }

        return tokens;
    }
}
