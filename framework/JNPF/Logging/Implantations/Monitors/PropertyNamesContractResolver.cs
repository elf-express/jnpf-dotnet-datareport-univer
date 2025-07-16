﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JNPF.Logging;

/// <summary>
/// 支持忽略特定属性的 CamelCase 序列化
/// </summary>
internal sealed class CamelCasePropertyNamesContractResolverWithIgnoreProperties : CamelCasePropertyNamesContractResolver
{
    /// <summary>
    /// 被忽略的属性名称
    /// </summary>
    private readonly string[] _names;

    /// <summary>
    /// 被忽略的属性类型
    /// </summary>
    private readonly Type[] _type;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="names"></param>
    /// <param name="types"></param>
    public CamelCasePropertyNamesContractResolverWithIgnoreProperties(string[] names, Type[] types)
    {
        _names = names ?? Array.Empty<string>();
        _type = types ?? Array.Empty<Type>();
    }

    /// <summary>
    /// 重写需要序列化的属性名
    /// </summary>
    /// <param name="type"></param>
    /// <param name="memberSerialization"></param>
    /// <returns></returns>
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var allProperties = base.CreateProperties(type, memberSerialization);

        return allProperties.Where(p =>
                !_names.Contains(p.PropertyName, StringComparer.OrdinalIgnoreCase)
                && !_type.Contains(p.PropertyType)).ToList();
    }
}

/// <summary>
/// 支持忽略特定属性的 Default 序列化
/// </summary>
internal sealed class DefaultContractResolverWithIgnoreProperties : DefaultContractResolver
{
    /// <summary>
    /// 被忽略的属性名称
    /// </summary>
    private readonly string[] _names;

    /// <summary>
    /// 被忽略的属性类型
    /// </summary>
    private readonly Type[] _type;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="names"></param>
    /// <param name="types"></param>
    public DefaultContractResolverWithIgnoreProperties(string[] names, Type[] types)
    {
        _names = names ?? Array.Empty<string>();
        _type = types ?? Array.Empty<Type>();
    }

    /// <summary>
    /// 重写需要序列化的属性名
    /// </summary>
    /// <param name="type"></param>
    /// <param name="memberSerialization"></param>
    /// <returns></returns>
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var allProperties = base.CreateProperties(type, memberSerialization);

        return allProperties.Where(p =>
                !_names.Contains(p.PropertyName, StringComparer.OrdinalIgnoreCase)
                && !_type.Contains(p.PropertyType)).ToList();
    }
}