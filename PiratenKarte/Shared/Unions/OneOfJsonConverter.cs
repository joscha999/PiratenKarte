using OneOf;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PiratenKarte.Shared.Unions;

public class OneOfJsonConverterFactory : JsonConverterFactory {
    public override bool CanConvert(Type typeToConvert) {
        if (!typeToConvert.IsGenericType)
            return false;

        return typeof(IOneOf).IsAssignableFrom(typeToConvert);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => new OneOfJsonConverter();
}

public class OneOfJsonConverter : JsonConverter<IOneOf> {
    public override IOneOf? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var assemblyName = ReadString(ref reader);
        var typeName = ReadString(ref reader);

        var oneOfAssembly = ReadString(ref reader);
        var oneOfType = ReadString(ref reader);

        reader.Read();
        reader.Read();
        var index = reader.GetInt32();

        var assembly = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.FullName == assemblyName);
        if (assembly == null)
            throw new InvalidOperationException($"Unable to locate assembly: {assemblyName}");
        var ooAssembly = Array.Find(AppDomain.CurrentDomain.GetAssemblies(), a => a.FullName == oneOfAssembly);
        if (ooAssembly == null)
            throw new InvalidOperationException($"Unable to locate assembly: {oneOfAssembly}");

        var type = assembly.GetType(typeName!);
        if (type == null)
            throw new InvalidOperationException($"Unable to locate type: {typeName}");
        var ooType = ooAssembly.GetType(oneOfType!);
        if (ooType == null)
            throw new InvalidOperationException($"Unable to locate type: {oneOfType}");

        reader.Read();
        reader.Read();
        var value = JsonSerializer.Deserialize(ref reader, type);

        var genericCount = ooType.GenericTypeArguments.Length;
        var parameters = new object[genericCount + 1];
        parameters[0] = index;
        parameters[index + 1] = value!;

        var valInstance = Activator.CreateInstance(ooType, BindingFlags.Instance | BindingFlags.NonPublic,
            null, parameters, null);

        reader.Read();
        return (IOneOf?)valInstance;
    }

    public override void Write(Utf8JsonWriter writer, IOneOf value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WriteString("Assembly", value.Value.GetType().Assembly.FullName);
        writer.WriteString("Type", value.Value.GetType().FullName);

        writer.WriteString("OOAssembly", value.GetType().Assembly.FullName);
        writer.WriteString("OOType", value.GetType().FullName);
        writer.WriteNumber("OOIndex", value.Index);

        writer.WritePropertyName("Value");
        JsonSerializer.Serialize(writer, value.Value, value.Value.GetType(), options);
        writer.WriteEndObject();
    }

    private string ReadString(ref Utf8JsonReader reader) {
        reader.Read();
        reader.Read();
        return reader.GetString()!;
    }
}