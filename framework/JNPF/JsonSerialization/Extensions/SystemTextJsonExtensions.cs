﻿using JNPF.JsonSerialization;
using System.Text.Json.Serialization;

namespace System.Text.Json;

/// <summary>
/// System.Text.Json 拓展
/// </summary>
[SuppressSniffer]
public static class SystemTextJsonExtensions
{
    /// <summary>
    /// 添加 DateTime/DateTime?/DateTimeOffset/DateTimeOffset? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <param name="localized">自动转换 DateTime/DateTimeOffset 为当地时间</param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDateTimeTypeConverters(this IList<JsonConverter> converters, string outputFormat = "yyyy-MM-dd HH:mm:ss", bool localized = false)
    {
        converters.Add(new SystemTextJsonDateTimeJsonConverter(outputFormat, localized));
        converters.Add(new SystemTextJsonNullableDateTimeJsonConverter(outputFormat, localized));

        converters.Add(new SystemTextJsonDateTimeOffsetJsonConverter(outputFormat, localized));
        converters.Add(new SystemTextJsonNullableDateTimeOffsetJsonConverter(outputFormat, localized));

        return converters;
    }

    /// <summary>
    /// 添加 long/long? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="overMaxLengthOf17">是否超过最大长度 17 再处理</param>
    /// <remarks></remarks>
    public static IList<JsonConverter> AddLongTypeConverters(this IList<JsonConverter> converters, bool overMaxLengthOf17 = false)
    {
        converters.Add(new SystemTextJsonLongToStringJsonConverter(overMaxLengthOf17));
        converters.Add(new SystemTextJsonNullableLongToStringJsonConverter(overMaxLengthOf17));

        return converters;
    }

    /// <summary>
    /// 添加 Clay 类型序列化处理
    /// </summary>
    /// <remarks></remarks>
    public static IList<JsonConverter> AddClayConverters(this IList<JsonConverter> converters)
    {
        converters.Add(new SystemTextJsonClayJsonConverter());

        return converters;
    }

    /// <summary>
    /// 添加 DateOnly/DateOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddDateOnlyConverters(this IList<JsonConverter> converters, string outputFormat = "yyyy-MM-dd")
    {
        converters.Add(new SystemTextJsonDateOnlyJsonConverter(outputFormat));
        converters.Add(new SystemTextJsonNullableDateOnlyJsonConverter(outputFormat));

        return converters;
    }

    /// <summary>
    /// 添加 TimeOnly/TimeOnly? 类型序列化处理
    /// </summary>
    /// <param name="converters"></param>
    /// <param name="outputFormat"></param>
    /// <returns></returns>
    public static IList<JsonConverter> AddTimeOnlyConverters(this IList<JsonConverter> converters, string outputFormat = "HH:mm:ss")
    {
        converters.Add(new SystemTextJsonTimeOnlyJsonConverter(outputFormat));
        converters.Add(new SystemTextJsonNullableTimeOnlyJsonConverter(outputFormat));

        return converters;
    }
}