﻿using System.Text.Json.Serialization;

namespace JNPF.Schedule;

/// <summary>
/// 作业触发器运行记录
/// </summary>
[SuppressSniffer]
public sealed class TriggerTimeline : IDisposable
{
    /// <summary>
    /// 作业 Id
    /// </summary>
    [JsonInclude]
    public string JobId { get; internal set; }

    /// <summary>
    /// 作业触发器 Id
    /// </summary>
    [JsonInclude]
    public string TriggerId { get; internal set; }

    /// <summary>
    /// 当前运行次数
    /// </summary>
    [JsonInclude]
    public long NumberOfRuns { get; internal set; }

    /// <summary>
    /// 最近运行时间
    /// </summary>
    [JsonInclude]
    public DateTime? LastRunTime { get; internal set; }

    /// <summary>
    /// 下一次运行时间
    /// </summary>
    [JsonInclude]
    public DateTime? NextRunTime { get; internal set; }

    /// <summary>
    /// 作业触发器状态
    /// </summary>
    [JsonInclude]
    public TriggerStatus Status { get; internal set; }

    /// <summary>
    /// 本次执行结果
    /// </summary>
    [JsonInclude]
    public string Result { get; internal set; }

    /// <summary>
    /// 本次执行耗时
    /// </summary>
    [JsonInclude]
    public long ElapsedTime { get; internal set; }

    /// <summary>
    /// 新增时间
    /// </summary>
    [JsonInclude]
    public DateTime CreatedTime { get; internal set; }

    /// <summary>
    /// 触发模式
    /// </summary>
    /// <remarks>默认为定时触发</remarks>
    [JsonInclude]
    public int Mode { get; internal set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    [JsonInclude]
    public string Exception { get; internal set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        Exception = null;
    }
}