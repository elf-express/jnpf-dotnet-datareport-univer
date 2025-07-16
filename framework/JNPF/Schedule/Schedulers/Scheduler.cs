﻿using Microsoft.Extensions.Logging;

namespace JNPF.Schedule;

/// <summary>
/// 作业计划
/// </summary>
internal sealed partial class Scheduler : IScheduler
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="triggers">作业触发器集合</param>
    internal Scheduler(JobDetail jobDetail, Dictionary<string, Trigger> triggers)
    {
        JobId = jobDetail.JobId;
        GroupName = jobDetail.GroupName;
        JobDetail = jobDetail;
        Triggers = triggers;
    }

    /// <summary>
    /// 作业 Id
    /// </summary>
    public string JobId { get; private set; }

    /// <summary>
    /// 作业组名称
    /// </summary>
    public string GroupName { get; private set; }

    /// <summary>
    /// 作业触发器数量
    /// </summary>
    public int TriggerCount => Triggers.Count;

    /// <summary>
    /// 作业信息
    /// </summary>
    internal JobDetail JobDetail { get; private set; }

    /// <summary>
    /// 作业触发器集合
    /// </summary>
    internal Dictionary<string, Trigger> Triggers { get; private set; } = new();

    /// <summary>
    /// 作业计划工厂
    /// </summary>
    internal ISchedulerFactory Factory { get; set; }

    /// <summary>
    /// 作业调度器日志服务
    /// </summary>
    internal IScheduleLogger Logger { get; set; }

    /// <summary>
    /// 作业处理类型日志服务
    /// </summary>
    internal ILogger JobLogger { get; set; }
}