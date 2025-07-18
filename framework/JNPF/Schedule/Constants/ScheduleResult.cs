﻿namespace JNPF.Schedule;

/// <summary>
/// 作业调度器操作结果
/// </summary>
[SuppressSniffer]
public enum ScheduleResult
{
    /// <summary>
    /// 不存在
    /// </summary>
    NotFound = 0,

    /// <summary>
    /// 未指定作业 Id
    /// </summary>
    NotIdentify = 1,

    /// <summary>
    /// 已存在
    /// </summary>
    Exists = 2,

    /// <summary>
    /// 成功
    /// </summary>
    Succeed = 3,

    /// <summary>
    /// 失败
    /// </summary>
    Failed = 4
}