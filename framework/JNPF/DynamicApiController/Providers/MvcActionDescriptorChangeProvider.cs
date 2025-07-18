﻿using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace JNPF.DynamicApiController;

/// <summary>
/// MVC 控制器感知提供器
/// </summary>
[SuppressSniffer]
public class MvcActionDescriptorChangeProvider : IActionDescriptorChangeProvider
{
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationChangeToken _stoppingToken;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MvcActionDescriptorChangeProvider()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _stoppingToken = new CancellationChangeToken(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// 获取改变 ChangeToken
    /// </summary>
    /// <returns></returns>
    public IChangeToken GetChangeToken()
    {
        return _stoppingToken;
    }

    /// <summary>
    /// 通知变化
    /// </summary>
    public void NotifyChanges()
    {
        var oldCancellationTokenSource = Interlocked.Exchange(ref _cancellationTokenSource, new CancellationTokenSource());
        _stoppingToken = new CancellationChangeToken(_cancellationTokenSource.Token);
        oldCancellationTokenSource.Cancel();
    }
}