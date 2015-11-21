
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseTools
{
	namespace Extensions
	{
		public static class StringExtensions
		{

			public static string RegExReplace(this string value, string pattern, string replacement)
			{
				return System.Text.RegularExpressions.Regex.Replace(value, pattern, replacement);
			}

			public static string IsNull(this string value, string replacement)
			{
				return (string.IsNullOrEmpty(value) ? replacement : value);
			}

			public static bool IsNumeric(this string value)
			{
				if (!(string.IsNullOrEmpty(value)) && System.Text.RegularExpressions.Regex.Replace(value, "[^0-9]", "").Equals(value))
				{
					return true;
				}
				return false;
			}

			public static string FromDatabaseName(this string value)
			{
				System.Text.StringBuilder sbValue = new System.Text.StringBuilder();
				foreach (string part in value.Split('_'))
				{
					if (sbValue.Length > 0)
					{
						sbValue.Append(" ");
					}
					if (part.Length > 0)
					{
						sbValue.Append(part.Substring(0, 1).ToUpper());
					}
					if (part.Length > 1)
					{
						sbValue.Append(part.Substring(1));
					}
				}
				return sbValue.ToString();
			}

			public static string FromPascalCase(this string value)
			{
				if (2 > value.Length)
				{
					return value;
				}
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				char[] chars = value.ToCharArray();

				sb.Append(chars[0]);

				for (int i = 1; i <= chars.Length - 2; i++)
				{
					char c = chars[i];
					if (char.IsUpper(c) && (char.IsLower(chars[i + 1]) || char.IsLower(chars[i - 1])))
					{
						sb.Append(' ');
					}
					sb.Append(c);
				}

				sb.Append(chars[chars.Length - 1]);

				return sb.ToString().Replace("E Payment", " ePayment").Trim();
			}

			public static string ToPlural(this string value)
			{
				string strLeftPart = string.Empty;
				string strWordToPlural = value;

				if (strWordToPlural.Contains(" "))
				{
					strLeftPart = strWordToPlural.Remove(strWordToPlural.LastIndexOf(" ") + 1);
					strWordToPlural = strWordToPlural.Substring(strWordToPlural.LastIndexOf(" ") + 1);
				}

				if (strWordToPlural.EndsWith("y", StringComparison.InvariantCultureIgnoreCase))
				{
					strWordToPlural = strWordToPlural.Remove(strWordToPlural.Length - 1) + "ies";
				}
				else if (strWordToPlural.EndsWith("tch", StringComparison.InvariantCultureIgnoreCase))
				{
					strWordToPlural += "es";
				}
				else
				{
					strWordToPlural += "s";
				}

				return string.Concat(strLeftPart, strWordToPlural);
			}

			public static string ToSingular(this string value)
			{
				if (value.EndsWith("ies", StringComparison.InvariantCultureIgnoreCase))
				{
					value = value.Remove(value.Length - 3) + "y";
				}
				else if (value.EndsWith("eases", StringComparison.InvariantCultureIgnoreCase))
				{
					value = value.Remove(value.Length - 1);
				}
				else if (value.EndsWith("ses", StringComparison.InvariantCultureIgnoreCase))
				{
					value = value.Remove(value.Length - 2);
				}
				else if (value.EndsWith("xes", StringComparison.InvariantCultureIgnoreCase))
				{
					value = value.Remove(value.Length - 2);
				}
				else if (value.EndsWith("s"))
				{
					value = value.Remove(value.Length - 1);
				}
				return value;
			}

			public static bool EqualsIgnoreCase(this string a, string b)
			{
				return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
			}

			public static bool StartsWithIgnoreCase(this string a, string b)
			{
				return a.IsNull("").StartsWith(b.IsNull(""), StringComparison.InvariantCultureIgnoreCase);
			}

			public static string TrimNonNumericChars(this string value)
			{
				return System.Text.RegularExpressions.Regex.Replace(value, "[^0-9]", "");
			}

			public static string Unescape(this string value)
			{
				var result = System.Text.RegularExpressions.Regex.Unescape(value);
				return result;
			}

		}
	}


}