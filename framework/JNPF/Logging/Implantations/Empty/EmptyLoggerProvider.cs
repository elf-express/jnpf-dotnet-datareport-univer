﻿using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace JNPF.Logging;

/// <summary>
/// 空日志记录器提供程序
/// </summary>
/// <remarks>https://docs.microsoft.com/zh-cn/dotnet/core/extensions/custom-logging-provider</remarks>
[SuppressSniffer, ProviderAlias("Empty")]
public sealed class EmptyLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// 存储多日志分类日志记录器
    /// </summary>
    private readonly ConcurrentDictionary<string, EmptyLogger> _emptyLoggers = new();

    /// <summary>
    /// 创建空日志记录器
    /// </summary>
    /// <param name="categoryName">日志分类名</param>
    /// <returns><see cref="ILogger"/></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return _emptyLoggers.GetOrAdd(categoryName, name => new EmptyLogger());
    }

    /// <summary>
    /// 释放非托管资源
    /// </summary>
    /// <remarks>控制日志消息队列</remarks>
    public void Dispose()
    {
        // 清空空日志记录器
        _emptyLoggers.Clear();
    }
}