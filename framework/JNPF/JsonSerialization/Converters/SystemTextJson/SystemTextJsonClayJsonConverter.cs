﻿using JNPF.ClayObject;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JNPF.JsonSerialization;

/// <summary>
/// Clay 类型序列化
/// </summary>
[SuppressSniffer]
public class SystemTextJsonClayJsonConverter : JsonConverter<Clay>
{
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public SystemTextJsonClayJsonConverter()
    {
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override Clay Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Clay.Parse(reader.GetString());
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, Clay value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.ToString());
    }
}