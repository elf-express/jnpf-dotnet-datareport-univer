﻿using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace JNPF.Logging;

/// <summary>
/// 数据库日志记录器
/// </summary>
/// <remarks>https://docs.microsoft.com/zh-cn/dotnet/core/extensions/custom-logging-provider</remarks>
[SuppressSniffer]
public sealed class DatabaseLogger : ILogger
{
    /// <summary>
    /// 记录器类别名称
    /// </summary>
    private readonly string _logName;

    /// <summary>
    /// 数据库日志记录器提供器
    /// </summary>
    private readonly DatabaseLoggerProvider _databaseLoggerProvider;

    /// <summary>
    /// 日志配置选项
    /// </summary>
    private readonly DatabaseLoggerOptions _options;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logName">记录器类别名称</param>
    /// <param name="databaseLoggerProvider">数据库日志记录器提供器</param>
    public DatabaseLogger(string logName, DatabaseLoggerProvider databaseLoggerProvider)
    {
        _logName = logName;
        _databaseLoggerProvider = databaseLoggerProvider;
        _options = databaseLoggerProvider.LoggerOptions;
    }

    /// <summary>
    /// 开始逻辑操作范围
    /// </summary>
    /// <typeparam name="TState">标识符类型参数</typeparam>
    /// <param name="state">要写入的项/对象</param>
    /// <returns><see cref="IDisposable"/></returns>
    public IDisposable BeginScope<TState>(TState state)
    {
        return _databaseLoggerProvider.ScopeProvider?.Push(state);
    }

    /// <summary>
    /// 检查是否已启用给定日志级别
    /// </summary>
    /// <param name="logLevel">日志级别</param>
    /// <returns><see cref="bool"/></returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _options.MinimumLevel;
    }

    /// <summary>
    /// 写入日志项
    /// </summary>
    /// <typeparam name="TState">标识符类型参数</typeparam>
    /// <param name="logLevel">日志级别</param>
    /// <param name="eventId">事件 Id</param>
    /// <param name="state">要写入的项/对象</param>
    /// <param name="exception">异常对象</param>
    /// <param name="formatter">日志格式化器</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Log<TState>(LogLevel logLevel
        , EventId eventId
        , TState state
        , Exception exception
        , Func<TState, Exception, string> formatter)
    {
        // 判断日志级别是否有效
        if (!IsEnabled(logLevel)) return;

        // 检查日志格式化器
        if (formatter == null) throw new ArgumentNullException(nameof(formatter));

        // 获取格式化后的消息
        var message = formatter(state, exception);

        // 日志消息内容转换（如脱敏处理）
        if (_options.MessageProcess != null)
        {
            message = _options.MessageProcess(message);
        }

        var logDateTime = _options.UseUtcTimestamp ? DateTime.UtcNow : DateTime.Now;
        var logMsg = new LogMessage(_logName, logLevel, eventId, message, exception, null, state, logDateTime, Environment.CurrentManagedThreadId, _options.UseUtcTimestamp, App.GetTraceId())
        {
            // 设置日志上下文
            Context = Penetrates.SetLogContext(_databaseLoggerProvider.ScopeProvider, _options.IncludeScopes)
        };

        // 判断是否自定义了日志筛选器，如果是则检查是否符合条件
        if (_options.WriteFilter?.Invoke(logMsg) == false)
        {
            logMsg.Context?.Dispose();
            return;
        }

        // 设置日志消息模板
        logMsg.Message = _options.MessageFormat != null
            ? _options.MessageFormat(logMsg)
            : Penetrates.OutputStandardMessage(logMsg, _options.DateFormat, withTraceId: _options.WithTraceId, withStackFrame: _options.WithStackFrame);

        // 空检查
        if (logMsg.Message is null)
        {
            logMsg.Context?.Dispose();
            return;
        }

        // 判断是否忽略循环输出日志，解决数据库日志提供程序中也输出日志导致写入递归问题
        if (_options.IgnoreReferenceLoop)
        {
            var stackTrace = new StackTrace();
            if (stackTrace.GetFrames().Any(u => u.HasMethod() && typeof(IDatabaseLoggingWriter).IsAssignableFrom(u.GetMethod().DeclaringType)))
            {
                logMsg.Context?.Dispose();
                return;
            }
        }

        // 写入日志队列
        _databaseLoggerProvider.WriteToQueue(logMsg);
    }
}