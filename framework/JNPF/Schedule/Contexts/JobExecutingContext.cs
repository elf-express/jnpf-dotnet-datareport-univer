﻿namespace JNPF.Schedule;

/// <summary>
/// 作业执行前上下文
/// </summary>
[SuppressSniffer]
public sealed class JobExecutingContext : JobExecutionContext
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="trigger">作业触发器</param>
    /// <param name="occurrenceTime">作业计划触发时间</param>
    /// <param name="runId">当前作业触发器触发的唯一标识</param>
    /// <param name="serviceProvider">服务提供器</param>
    internal JobExecutingContext(JobDetail jobDetail
        , Trigger trigger
        , DateTime occurrenceTime
        , string runId
        , IServiceProvider serviceProvider)
        : base(jobDetail, trigger, occurrenceTime, runId, serviceProvider)
    {
    }

    /// <summary>
    /// 执行前时间
    /// </summary>
    public DateTime ExecutingTime { get; internal set; }
}