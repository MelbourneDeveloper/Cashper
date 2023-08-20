using System.Text;
using System.Text.Json;

namespace Cashper.Tests;

class BasicAdapter : ISerializationAdapter
{
    public T? Deserialize<T>(string key, byte[]? value) =>
        value == null ? default : JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(value));

    public byte[] Serialize(object value) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, value.GetType()));
}
