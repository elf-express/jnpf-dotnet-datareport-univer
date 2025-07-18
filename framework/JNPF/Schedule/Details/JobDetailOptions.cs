﻿namespace JNPF.Schedule;

/// <summary>
/// 作业信息配置选项
/// </summary>
[SuppressSniffer]
public sealed class JobDetailOptions
{
    /// <summary>
    /// 构造函数
    /// </summary>
    internal JobDetailOptions()
    {
    }

    /// <summary>
    /// 重写 <see cref="ConvertToSQL"/>
    /// </summary>
    public Func<string, string[], JobDetail, PersistenceBehavior, NamingConventions, string> ConvertToSQL
    {
        get
        {
            return ConvertToSQLConfigure;
        }
        set
        {
            ConvertToSQLConfigure = value;
        }
    }

    /// <summary>
    /// 启用作业执行详细日志
    /// </summary>
    public bool LogEnabled
    {
        get
        {
            return InternalLogEnabled;
        }
        set
        {
            InternalLogEnabled = value;
        }
    }

    /// <summary>
    /// <see cref="LogEnabled"/> 静态配置
    /// </summary>
    internal static bool InternalLogEnabled { get; private set; }

    /// <summary>
    /// <see cref="ConvertToSQL"/> 静态配置
    /// </summary>
    internal static Func<string, string[], JobDetail, PersistenceBehavior, NamingConventions, string> ConvertToSQLConfigure { get; private set; }
}