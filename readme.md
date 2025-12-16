# StringQueryMap (SQM)

A lightweight library for serializing and deserializing key-value pairs as strings.
All data is stored internally as strings, and parsing to the requested type `T` happens on retrieval.

### Compatible with all .NET versions >= netstandard2.0

> NOTE: Designed to be stable and low-maintenance. Unit tests guarantee safe and predictable usage.

---

## Installation

You can install the package via NuGet:

```bash
dotnet add package StringQueryMap
```

Or via the Package Manager Console:

```powershell
Install-Package StringQueryMap
```

---

## Features

* **Serialization**: All key-value pairs are converted to string using `ToString()` when added.
* **Deserialization**: Values are parsed to the requested type `T` on retrieval.
* **Generic support**: Works with any type `T` that either has a **built-in parser** (21 types supported) or provides a **public static `Parse(string)` method**.
* **Custom delimiters and joiners**: Flexible string syntax.
* **Safe access**: `ContainsKey`, `TryGet`, and `Get` for safe and exact retrieval.

---

## Usage

### Serialization

```csharp
var sqm = new SQM("=", ";");
sqm.Add("one", 1);
sqm.Add("pi", 3.14);
sqm.Add("name", "Alice");

string serialized = sqm.ToString(); // "one=1;pi=3.14;name=Alice"
```

> All values are stored internally as strings via `ToString()`.

### Deserialization

SQM provides two parsing approaches:

* **Exact Parsing** – `Get<T>`: throws exceptions if parsing fails or key not found.
* **Safe Parsing** – `TryGet<T>`: returns a boolean indicating success.

```csharp
var data = "a=1;b=2;c=true";
var sqm = SQM.Parse(data, "=", ";");

// Exact parse
int a = sqm.Get<int>("a");     // 1
int b = sqm.Get<int>("b");     // 2
bool c = sqm.Get<bool>("c");   // true

// Safe parse
bool foundX = sqm.TryGet<int>("x", out int xValue); // false
```

Custom types must provide a **public static `Parse(string)` method**:

```csharp
var customData = "item1=42;item2=100";
var customMapper = SQM.Parse(customData, "=", ";");

int value1 = customMapper.Get<CustomWithParse>("item1").Value;
int value2 = customMapper.Get<CustomWithParse>("item2").Value;
```

---

## Built-in Types with Integrated Parsing

`bool`, `byte`, `sbyte`, `char`, `decimal`, `double`, `float`, `int`, `uint`, `long`, `ulong`, `short`, `ushort`,
`string`, `Guid`, `DateTime`, `DateTimeOffset`, `TimeSpan`, `Version`, `Uri`, `IPAddress`, `BigInteger`, `CultureInfo`,
and enums (any enum type)

> These 21 types plus enums are parsed directly without reflection. All other types rely on reflection to locate a `public static Parse(string)` method.

---

## API Reference

**SQM**

* `SQM()` – creates an empty mapper with default `Joiner = "="` and `Delimiter = ";"`
* `SQM(string joiner, string delimiter)` – creates an empty mapper
* `SQM(string data, string joiner, string delimiter)` – parses from string
* `string Joiner { get; }` – joiner string
* `string Delimiter { get; }` – delimiter string
* `IEnumerable<string> AllKeys { get; }`
* `IEnumerable<object> AllValues { get; }` – all values as strings
* `bool Add<T>(string key, T value)` – adds or updates a key-value pair (`value.ToString()` internally)
* `int AddRange<T>(IEnumerable<KeyValuePair<string, T>> pairs)` – add multiple pairs
* `bool Remove(string key)` – removes a key
* `int RemoveRange(IEnumerable<string> keys)` – remove multiple keys
* `void Clear()` – clear all pairs
* `bool ContainsKey(string key)` – check if key exists
* `bool TryGet<T>(string key, out T value)` – safe retrieval
* `T Get<T>(string key)` – exact retrieval (throws if key missing or parse fails)
* `override string ToString()` – serializes all key-value pairs to string
* `static SQM Parse(string data, string joiner, string delimiter)` – parse string into SQM
* `static bool TryParse(string data, string joiner, string delimiter, out SQM result)` – safe parse

> Serialization converts all values to string via `ToString()`.
> Deserialization converts string values back to type `T` using either built-in parsing or the public static `Parse(string)` method.

---

## Conformance

* Created for KeZero, available to the community.
* Licensed under the MIT License.
* Creator: Alecio Furanze ([https://github.com/afuranze](https://github.com/afuranze))
* Repository: kezerocom/StringQueryMap ([https://github.com/kezerocom/StringQueryMap](https://github.com/kezerocom/StringQueryMap))
