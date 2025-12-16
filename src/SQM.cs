using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;

namespace StringKeyMap
{
    /// <summary>
    ///     Represents a generic key-value mapper that can serialize and deserialize key-value pairs to/from strings.
    /// </summary>
    public class SQM
    {
        private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();

        /// <summary>
        ///     Initializes a new instance of <see cref="SQM" /> using default settings.
        /// </summary>
        /// <remarks>
        ///     Default values:
        ///     <list type="bullet">
        ///         <item>
        ///             <description><see cref="Joiner" /> is set to <c>=</c></description>
        ///         </item>
        ///         <item>
        ///             <description><see cref="Delimiter" /> is set to <c>;</c></description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public SQM() : this("=", ";")
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="SQM" /> with the specified joiner and delimiter.
        /// </summary>
        /// <param name="joiner">The character(s) used to join key and value.</param>
        /// <param name="delimiter">The character(s) used to separate pairs.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="joiner" /> or <paramref name="delimiter" /> is
        ///     null.
        /// </exception>
        public SQM(string joiner, string delimiter)
        {
            Joiner = joiner ?? throw new ArgumentNullException(nameof(joiner));
            Delimiter = delimiter ?? throw new ArgumentNullException(nameof(delimiter));
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="SQM" /> from serialized data.
        /// </summary>
        /// <param name="data">The string containing serialized key-value pairs.</param>
        /// <param name="joiner">The character(s) used to join key and value.</param>
        /// <param name="delimiter">The character(s) used to separate pairs.</param>
        /// <exception cref="FormatException">Thrown when the input string contains invalid pair formats.</exception>
        public SQM(string data, string joiner, string delimiter) : this(joiner, delimiter)
        {
            if (!string.IsNullOrEmpty(data)) ParseData(data);
        }

        /// <summary>
        ///     Gets the joiner string used between key and value.
        /// </summary>
        public string Joiner { get; }

        /// <summary>
        ///     Gets the delimiter string used between key-value pairs.
        /// </summary>
        public string Delimiter { get; }

        /// <summary>
        ///     Gets all keys in the mapper.
        /// </summary>
        public IEnumerable<string> AllKeys => _dictionary.Keys;

        /// <summary>
        ///     Gets all values in the mapper.
        /// </summary>
        public IEnumerable<object> AllValues => _dictionary.Values;

        private T ParseValue<T>(string value)
        {
            var type = typeof(T);

            // primitive
            if (type == typeof(bool)) return (T)(object)bool.Parse(value);
            if (type == typeof(byte)) return (T)(object)byte.Parse(value);
            if (type == typeof(sbyte)) return (T)(object)sbyte.Parse(value);
            if (type == typeof(char)) return (T)(object)char.Parse(value);
            if (type == typeof(decimal)) return (T)(object)decimal.Parse(value);
            if (type == typeof(double)) return (T)(object)double.Parse(value);
            if (type == typeof(float)) return (T)(object)float.Parse(value);
            if (type == typeof(int)) return (T)(object)int.Parse(value);
            if (type == typeof(uint)) return (T)(object)uint.Parse(value);
            if (type == typeof(long)) return (T)(object)long.Parse(value);
            if (type == typeof(ulong)) return (T)(object)ulong.Parse(value);
            if (type == typeof(short)) return (T)(object)short.Parse(value);
            if (type == typeof(ushort)) return (T)(object)ushort.Parse(value);

            // built in
            if (type == typeof(string)) return (T)(object)value;
            if (type == typeof(Guid)) return (T)(object)Guid.Parse(value);
            if (type == typeof(DateTime))
                return (T)(object)DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            if (type == typeof(DateTimeOffset))
                return (T)(object)DateTimeOffset.Parse(value, CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind);
            if (type == typeof(TimeSpan)) return (T)(object)TimeSpan.Parse(value);
            if (type == typeof(Version)) return (T)(object)Version.Parse(value);
            if (type == typeof(Uri)) return (T)(object)new Uri(value, UriKind.RelativeOrAbsolute);
            if (type == typeof(IPAddress)) return (T)(object)IPAddress.Parse(value);
            if (type == typeof(BigInteger)) return (T)(object)BigInteger.Parse(value);
            if (type == typeof(CultureInfo)) return (T)(object)CultureInfo.GetCultureInfo(value);
            if (type.IsEnum) return (T)Enum.Parse(type, value, true);

            // fallback (via reflection)
            var method = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null,
                CallingConventions.Any, new[] { typeof(string) }, null);

            if (method != null)
                return (T)method.Invoke(null, new object[] { value });

            throw new NotSupportedException(
                $"Type '{type.FullName}' does not have a public static Parse(string) method.");
        }

        private void ParseData(string data)
        {
            var formated = data.Replace(" ", string.Empty);
            var splits = formated.Split(Delimiter.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var split in splits)
            {
                if (split.StartsWith(Joiner) || split.StartsWith(Delimiter))
                {
                    throw new Exception("Bad format");
                }

                var result = split.Split(Joiner.ToArray(), StringSplitOptions.RemoveEmptyEntries);

                if (result.Length < 1 || result.Length > 2) throw new FormatException($"Invalid pair format: {split}");

                var key = result[0];
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException("Key cannot be empty");

                var value = result.Length == 2 ? result[1] : string.Empty;

                _dictionary[key] = value;
            }
        }

        /// <summary>
        ///     Adds a new key-value pair to the map or replaces the value if the key already exists.
        /// </summary>
        /// <param name="key">The key to add or update. Cannot be null, empty, or contain the joiner or delimiter characters.</param>
        /// <param name="value">The value associated with the key.</param>
        /// <returns>Always returns <c>true</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key" /> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key" /> contains the joiner or delimiter characters.</exception>
        public bool Add<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key), "Key cannot be null or empty.");


            if (key.Contains(Delimiter) || key.Contains(Joiner))
                throw new ArgumentException(
                    $"Key cannot contain the joiner ('{Joiner}') or delimiter ('{Delimiter}') characters.",
                    nameof(key));

            string result;

            if (typeof(T) == typeof(DateTime))
            {
                result = ((DateTime)(object)value).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                result = ((DateTimeOffset)(object)value).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            }
            else
            {
                result = value == null ? string.Empty : value.ToString();
            }

            if (result.Contains(Delimiter) || result.Contains(Joiner))
                throw new ArgumentException(
                    $"Value cannot contain the joiner ('{Joiner}') or delimiter ('{Delimiter}') characters.",
                    nameof(key));

            _dictionary[key] = result;
            return true;
        }

        /// <summary>
        ///     Adds a range of key-value pairs.
        /// </summary>
        /// <param name="pairs">The collection of key-value pairs to add.</param>
        /// <returns>The number of pairs added.</returns>
        public int AddRange<T>(IEnumerable<KeyValuePair<string, T>> pairs)
        {
            var count = 0;

            foreach (var kv in pairs)
            {
                Add(kv.Key, kv.Value);
                count++;
            }

            return count;
        }

        /// <summary>
        ///     Removes a key-value pair by key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the key existed and was removed; otherwise, false.</returns>
        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        /// <summary>
        ///     Removes multiple keys.
        /// </summary>
        /// <param name="keys">The collection of keys to remove.</param>
        /// <returns>The number of keys successfully removed.</returns>
        public int RemoveRange(IEnumerable<string> keys)
        {
            return keys.Count(key => _dictionary.Remove(key));
        }

        /// <summary>
        ///     Clears all key-value pairs.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        ///     Determines whether the mapper contains the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        ///     Attempts to retrieve the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="value">The value if the key exists; otherwise, default.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public bool TryGet<T>(string key, out T value)
        {
            try
            {
                value = Get<T>(key);
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key does not exist.</exception>
        public T Get<T>(string key)
        {
            return _dictionary.TryGetValue(key, out var data)
                ? ParseValue<T>(data)
                : throw new KeyNotFoundException($"Key '{key}' not found");
        }

        /// <summary>
        ///     Serializes the mapper to a string.
        /// </summary>
        /// <returns>A string containing all key-value pairs in the format "key{Joiner}value{Delimiter}".</returns>
        public override string ToString()
        {
            return string.Join(Delimiter, _dictionary.Select(x => $"{x.Key}{Joiner}{x.Value}"));
        }

        /// <summary>
        ///     Parses a string into a new <see cref="SQM" /> instance.
        /// </summary>
        /// <param name="data">The string containing serialized key-value pairs.</param>
        /// <param name="joiner">The joiner string.</param>
        /// <param name="delimiter">The delimiter string.</param>
        /// <returns>A new <see cref="SQM" /> instance.</returns>
        /// <exception cref="FormatException">Thrown if the string contains invalid pair formats.</exception>
        public static SQM Parse(string data, string joiner, string delimiter)
        {
            return new SQM(data, joiner, delimiter);
        }

        /// <summary>
        ///     Attempts to parse a string into a new <see cref="SQM" /> instance.
        /// </summary>
        /// <param name="data">The string containing serialized key-value pairs.</param>
        /// <param name="joiner">The joiner string.</param>
        /// <param name="delimiter">The delimiter string.</param>
        /// <param name="result">The resulting <see cref="SQM" /> instance if successful; otherwise, null.</param>
        /// <returns>True if parsing succeeds; otherwise, false.</returns>
        public static bool TryParse(string data, string joiner, string delimiter, out SQM result)
        {
            try
            {
                result = new SQM(data, joiner, delimiter);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}