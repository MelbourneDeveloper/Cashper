namespace Cashper;

public interface ISerializationAdapter
{
    T? Deserialize<T>(string key, byte[]? value);
    byte[] Serialize(object value);
}
