﻿namespace JNPF.Logging;

/// <summary>
/// 数据库日志写入器
/// </summary>
public interface IDatabaseLoggingWriter
{
    /// <summary>
    /// 写入数据库
    /// </summary>
    /// <param name="logMsg">结构化日志消息</param>
    /// <param name="flush">清除缓冲区</param>
    /// <returns><see cref="Task"/></returns>
    Task WriteAsync(LogMessage logMsg, bool flush);
}