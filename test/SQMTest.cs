using System.Globalization;
using System.Net;
using System.Numerics;
using StringQueryMap;

namespace StringQueryMapTest;

public class SQMTests
{
    public enum SampleEnum
    {
        None,
        One,
        Two
    }

    [Fact]
    public void AddAndToString_ShouldSerializeCorrectly()
    {
        var sqm = new SQM();
        sqm.Add("int", 42);
        sqm.Add("str", "hello");
        sqm.Add("bool", true);

        var str = sqm.ToString();
        Assert.Contains("int=42", str);
        Assert.Contains("str=hello", str);
        Assert.Contains("bool=True", str);
    }

    [Fact]
    public void Get_ShouldParseBuiltInTypes()
    {
        var sqm = new SQM();
        sqm.Add("i", 123);
        sqm.Add("d", 3.14);
        sqm.Add("b", true);
        sqm.Add("s", "hello");
        sqm.Add("g", Guid.NewGuid());

        Assert.Equal(123, sqm.Get<int>("i"));
        Assert.Equal(3.14, sqm.Get<double>("d"));
        Assert.True(sqm.Get<bool>("b"));
        Assert.Equal("hello", sqm.Get<string>("s"));
        Assert.IsType<Guid>(sqm.Get<Guid>("g"));
    }

    [Fact]
    public void Get_ShouldThrowIfKeyMissing()
    {
        var sqm = new SQM();
        Assert.Throws<KeyNotFoundException>(() => sqm.Get<int>("x"));
    }

    [Fact]
    public void TryGet_ShouldReturnFalseIfKeyMissing()
    {
        var sqm = new SQM();
        var found = sqm.TryGet("x", out int val);
        Assert.False(found);
        Assert.Equal(default, val);
    }

    [Fact]
    public void CustomType_ShouldWorkWithParse()
    {
        var sqm = new SQM();
        sqm.Add("custom", new CustomWithParse { Value = 99 });
        var val = sqm.Get<CustomWithParse>("custom");
        Assert.Equal(99, val.Value);
    }

    [Fact]
    public void Remove_ShouldDeleteKey()
    {
        var sqm = new SQM();
        sqm.Add("a", 1);
        Assert.True(sqm.Remove("a"));
        Assert.False(sqm.ContainsKey("a"));
    }

    [Fact]
    public void RemoveRange_ShouldDeleteMultipleKeys()
    {
        var sqm = new SQM();
        sqm.Add("a", 1);
        sqm.Add("b", 2);
        sqm.Add("c", 3);
        var removed = sqm.RemoveRange(new[] { "a", "c" });
        Assert.Equal(2, removed);
        Assert.False(sqm.ContainsKey("a"));
        Assert.False(sqm.ContainsKey("c"));
        Assert.True(sqm.ContainsKey("b"));
    }


    [Fact]
    public void ContainsKey_ShouldReturnCorrectly()
    {
        var sqm = new SQM();
        sqm.Add("key", 10);
        Assert.True(sqm.ContainsKey("key"));
        Assert.False(sqm.ContainsKey("missing"));
    }

    [Fact]
    public void Add_ShouldThrowOnNullOrEmptyKey()
    {
        var sqm = new SQM();
        Assert.Throws<ArgumentNullException>(() => sqm.Add(null, 1));
        Assert.Throws<ArgumentNullException>(() => sqm.Add("", 1));
    }

    [Fact]
    public void Add_ShouldThrowIfKeyContainsJoinerOrDelimiter()
    {
        var sqm = new SQM("|", ",");
        Assert.Throws<ArgumentException>(() => sqm.Add("a|b", 1));
        Assert.Throws<ArgumentException>(() => sqm.Add("c,d", 1));
    }


    [Fact]
    public void Parse_ShouldTrimSpaces()
    {
        var sqm = SQM.Parse(" a = 1 ; b = 2 ", "=", ";");
        Assert.Equal("1", sqm.Get<string>("a"));
        Assert.Equal("2", sqm.Get<string>("b"));
    }

    [Fact]
    public void TryParse_ShouldReturnFalseOnInvalidString()
    {
        var ok = SQM.TryParse("=invalid", "=", ";", out var sqm);
        Assert.False(ok);
        Assert.Null(sqm);
    }

    [Fact]
    public void Parse_ShouldHandleBoolean()
    {
        var sqm = SQM.Parse("b=true;f=false", "=", ";");
        Assert.True(sqm.Get<bool>("b"));
        Assert.False(sqm.Get<bool>("f"));
    }

    [Fact]
    public void Parse_ShouldHandleNumbers()
    {
        var sqm = SQM.Parse("i=123;d=4.56;f=7.8", "=", ";");
        Assert.Equal(123, sqm.Get<int>("i"));
        Assert.Equal(4.56, sqm.Get<double>("d"));
        Assert.Equal(7.8f, sqm.Get<float>("f"));
    }

    [Fact]
    public void Parse_ShouldHandleIP()
    {
        var sqm = SQM.Parse("ip=127.0.0.1;ipv6=::1", "=", ";");
        Assert.Equal(IPAddress.Parse("127.0.0.1"), sqm.Get<IPAddress>("ip"));
        Assert.Equal(IPAddress.Parse("::1"), sqm.Get<IPAddress>("ipv6"));
    }

    [Fact]
    public void Parse_CustomTypeWithParse_ShouldWork()
    {
        var sqm = SQM.Parse("x=55;y=99", "=", ";");
        var val = sqm.Get<CustomWithParse>("x");
        Assert.Equal(55, val.Value);
        val = sqm.Get<CustomWithParse>("y");
        Assert.Equal(99, val.Value);
    }

    [Fact]
    public void TryGet_ShouldReturnFalseForInvalidParse()
    {
        var sqm = SQM.Parse("a=abc", "=", ";");
        var ok = sqm.TryGet<int>("a", out var val);
        Assert.False(ok);
        Assert.Equal(default, val);
    }

    [Fact]
    public void AddAndToString_ShouldSerializeAllBuiltInTypes()
    {
        var sqm = new SQM();
        sqm.Add("bool", true);
        sqm.Add("byte", (byte)10);
        sqm.Add("sbyte", (sbyte)-5);
        sqm.Add("char", 'A');
        sqm.Add("decimal", 1.23m);
        sqm.Add("double", 4.56);
        sqm.Add("float", 7.89f);
        sqm.Add("int", 123);
        sqm.Add("uint", 456u);
        sqm.Add("long", 789L);
        sqm.Add("ulong", 987UL);
        sqm.Add("short", (short)12);
        sqm.Add("ushort", (ushort)34);
        sqm.Add("string", "hello");
        sqm.Add("guid", Guid.NewGuid());
        sqm.Add("datetime", DateTime.UtcNow);
        sqm.Add("datetimeoffset", DateTimeOffset.UtcNow);
        sqm.Add("timespan", TimeSpan.FromHours(1.5));
        sqm.Add("version", new Version(1, 2, 3));
        sqm.Add("uri", new Uri("https://example.com"));
        sqm.Add("ip", IPAddress.Loopback);
        sqm.Add("bigint", new BigInteger(123456));
        sqm.Add("culture", CultureInfo.InvariantCulture);
        sqm.Add("timezone", TimeZoneInfo.Local);
        sqm.Add("enum", SampleEnum.Two);

        var serialized = sqm.ToString();
        Assert.Contains("bool=True", serialized);
        Assert.Contains("char=A", serialized);
        Assert.Contains("string=hello", serialized);
        Assert.Contains("ip=127.0.0.1", serialized);
        Assert.Contains("enum=Two", serialized);
    }

    [Fact]
    public void Get_ShouldParseAllBuiltInTypes()
    {
        var sqm = new SQM();
        var guid = Guid.NewGuid();
        var dt = DateTime.UtcNow.AddDays(1);
        var dto = DateTimeOffset.UtcNow;
        var ts = TimeSpan.FromMinutes(90);
        var ver = new Version(1, 2);
        var uri = new Uri("https://test.com");
        var ip = IPAddress.Parse("::1");
        var bi = new BigInteger(9999);

        sqm.Add("bool", true);
        sqm.Add("byte", (byte)1);
        sqm.Add("sbyte", (sbyte)-1);
        sqm.Add("char", 'Z');
        sqm.Add("decimal", 1.1m);
        sqm.Add("double", 2.2);
        sqm.Add("float", 3.3f);
        sqm.Add("int", 10);
        sqm.Add("uint", 11u);
        sqm.Add("long", 12L);
        sqm.Add("ulong", 13UL);
        sqm.Add("short", (short)14);
        sqm.Add("ushort", (ushort)15);
        sqm.Add("string", "str");
        sqm.Add("guid", guid);
        sqm.Add("datetime", dt);
        sqm.Add("datetimeoffset", dto);
        sqm.Add("timespan", ts);
        sqm.Add("version", ver);
        sqm.Add("uri", uri);
        sqm.Add("ip", ip);
        sqm.Add("bigint", bi);
        sqm.Add("culture", CultureInfo.InvariantCulture);
        sqm.Add("timezone", TimeZoneInfo.Local);
        sqm.Add("enum", SampleEnum.One);

        Assert.True(sqm.Get<bool>("bool"));
        Assert.Equal((byte)1, sqm.Get<byte>("byte"));
        Assert.Equal((sbyte)-1, sqm.Get<sbyte>("sbyte"));
        Assert.Equal('Z', sqm.Get<char>("char"));
        Assert.Equal(1.1m, sqm.Get<decimal>("decimal"));
        Assert.Equal(2.2, sqm.Get<double>("double"));
        Assert.Equal(3.3f, sqm.Get<float>("float"));
        Assert.Equal(10, sqm.Get<int>("int"));
        Assert.Equal(11u, sqm.Get<uint>("uint"));
        Assert.Equal(12L, sqm.Get<long>("long"));
        Assert.Equal(13UL, sqm.Get<ulong>("ulong"));
        Assert.Equal((short)14, sqm.Get<short>("short"));
        Assert.Equal((ushort)15, sqm.Get<ushort>("ushort"));
        Assert.Equal("str", sqm.Get<string>("string"));
        Assert.Equal(guid, sqm.Get<Guid>("guid"));
        Assert.Equal(dt, sqm.Get<DateTime>("datetime"));
        Assert.Equal(dto, sqm.Get<DateTimeOffset>("datetimeoffset"));
        Assert.Equal(ts, sqm.Get<TimeSpan>("timespan"));
        Assert.Equal(ver, sqm.Get<Version>("version"));
        Assert.Equal(uri, sqm.Get<Uri>("uri"));
        Assert.Equal(ip, sqm.Get<IPAddress>("ip"));
        Assert.Equal(bi, sqm.Get<BigInteger>("bigint"));
        Assert.Equal(CultureInfo.InvariantCulture, sqm.Get<CultureInfo>("culture"));
        Assert.Equal(SampleEnum.One, sqm.Get<SampleEnum>("enum"));
    }

    [Fact]
    public void TryGet_ShouldReturnTrueForAllBuiltInTypes()
    {
        var sqm = new SQM();
        sqm.Add("int", 42);
        var ok = sqm.TryGet<int>("int", out var val);
        Assert.True(ok);
        Assert.Equal(42, val);
    }

    [Fact]
    public void AddRange_ShouldAddMultiplePairs()
    {
        var sqm = new SQM();
        var pairs = new List<KeyValuePair<string, object>>
        {
            new("a", 1),
            new("b", "test"),
            new("c", true)
        };
        var count = sqm.AddRange(pairs);
        Assert.Equal(3, count);
        Assert.True(sqm.ContainsKey("a"));
        Assert.True(sqm.ContainsKey("b"));
        Assert.True(sqm.ContainsKey("c"));
    }

    [Fact]
    public void CustomType_ParseShouldWork()
    {
        var sqm = new SQM();
        sqm.Add("custom", new CustomParse { Value = 123 });
        var val = sqm.Get<CustomParse>("custom");
        Assert.Equal(123, val.Value);
    }

    [Fact]
    public void TryGet_CustomType_ShouldReturnTrue()
    {
        var sqm = new SQM();
        sqm.Add("custom", new CustomParse { Value = 321 });
        var ok = sqm.TryGet<CustomParse>("custom", out var val);
        Assert.True(ok);
        Assert.Equal(321, val.Value);
    }

    [Fact]
    public void RemoveAndContainsKey_ShouldWork()
    {
        var sqm = new SQM();
        sqm.Add("x", 1);
        Assert.True(sqm.Remove("x"));
        Assert.False(sqm.ContainsKey("x"));
    }

    [Fact]
    public void RemoveRange_ShouldWork()
    {
        var sqm = new SQM();
        sqm.Add("a", 1);
        sqm.Add("b", 2);
        sqm.Add("c", 3);
        var removed = sqm.RemoveRange(new[] { "a", "c" });
        Assert.Equal(2, removed);
    }

    [Fact]
    public void Clear_ShouldRemoveAllKeys()
    {
        var sqm = new SQM();
        sqm.Add("k1", 1);
        sqm.Add("k2", 2);
        sqm.Clear();
        Assert.Empty(sqm.AllKeys);
    }

    [Fact]
    public void Add_ShouldThrowOnInvalidKey()
    {
        var sqm = new SQM();
        Assert.Throws<ArgumentNullException>(() => sqm.Add(null, 1));
        Assert.Throws<ArgumentNullException>(() => sqm.Add("", 1));
        var sqm2 = new SQM(":", ",");
        Assert.Throws<ArgumentException>(() => sqm2.Add("x:y", 1));
        Assert.Throws<ArgumentException>(() => sqm2.Add("a,b", 1));
    }

    [Fact]
    public void Parse_ShouldHandleEmptyValue()
    {
        var sqm = SQM.Parse("a=;b=2", "=", ";");
        Assert.Equal("", sqm.Get<string>("a"));
        Assert.Equal("2", sqm.Get<string>("b"));
    }

    [Fact]
    public void TryParse_ShouldFailOnInvalidData()
    {
        var ok = SQM.TryParse("=bad", "=", ";", out var sqm);
        Assert.False(ok);
        Assert.Null(sqm);
    }

    [Fact]
    public void Parse_ShouldHandleIPv4AndIPv6()
    {
        var sqm = SQM.Parse("ip4=127.0.0.1;ip6=::1", "=", ";");
        Assert.Equal(IPAddress.Parse("127.0.0.1"), sqm.Get<IPAddress>("ip4"));
        Assert.Equal(IPAddress.Parse("::1"), sqm.Get<IPAddress>("ip6"));
    }

    [Fact]
    public void EnumParsing_ShouldBeCaseInsensitive()
    {
        var sqm = new SQM();
        sqm.Add("e", SampleEnum.Two);
        var val = sqm.Get<SampleEnum>("e");
        Assert.Equal(SampleEnum.Two, val);
    }


    public class CustomWithParse
    {
        public int Value { get; set; }

        public static CustomWithParse Parse(string s)
        {
            return new CustomWithParse { Value = int.Parse(s) };
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class CustomParse
    {
        public int Value { get; set; }

        public static CustomParse Parse(string s)
        {
            return new CustomParse { Value = int.Parse(s) };
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}