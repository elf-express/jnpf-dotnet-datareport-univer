﻿namespace JNPF.Schedule;

/// <summary>
/// 每小时特定分钟开始作业触发器特性
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class HourlyAtAttribute : CronAttribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fields">字段值</param>
    public HourlyAtAttribute(params object[] fields)
        : base("@hourly", fields)
    {
    }
}