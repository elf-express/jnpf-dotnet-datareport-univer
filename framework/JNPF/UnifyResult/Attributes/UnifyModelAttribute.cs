﻿namespace JNPF.UnifyResult;

/// <summary>
/// 规范化模型特性
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Class)]
public class UnifyModelAttribute : Attribute
{
    /// <summary>
    /// 规范化模型
    /// </summary>
    /// <param name="modelType"></param>
    public UnifyModelAttribute(Type modelType)
    {
        ModelType = modelType;
    }

    /// <summary>
    /// 模型类型（泛型）
    /// </summary>
    public Type ModelType { get; set; }
}