﻿using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace JNPF.Common.Cache;

/// <summary>
/// 内存缓存.
/// </summary>
public class MemoryCache : ICache, ISingleton
{
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// 初始化一个<see cref="MemoryCache"/>类型的新实例.
    /// </summary>
    /// <param name="memoryCache"></param>
    public MemoryCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// 用于在 key 存在时删除 key.
    /// </summary>
    /// <param name="key">键.</param>
    public long Del(params string[] key)
    {
        foreach (string? k in key)
        {
            _memoryCache.Remove(k);
        }

        return key.Length;
    }

    /// <summary>
    /// 用于在 key 存在时删除 key.
    /// </summary>
    /// <param name="key">键.</param>
    public Task<long> DelAsync(params string[] key)
    {
        foreach (var k in key)
        {
            _memoryCache.Remove(k);
        }

        return Task.FromResult((long)key.Length);
    }

    /// <summary>
    /// 用于在 key 模板存在时删除.
    /// </summary>
    /// <param name="pattern">key模板.</param>
    public async Task<long> DelByPatternAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return default;

        // pattern = Regex.Replace(pattern, @"\{*.\}", "(.*)");
        IEnumerable<string>? keys = GetAllKeys().Where(k => k.StartsWith(pattern));
        if (keys != null && keys.Any())
            return await DelAsync(keys.ToArray());

        return default;
    }

    /// <summary>
    /// 检查给定 key 是否存在.
    /// </summary>
    /// <param name="key">键.</param>
    public bool Exists(string key)
    {
        return _memoryCache.TryGetValue(key, out _);
    }

    /// <summary>
    /// 检查给定 key 是否存在.
    /// </summary>
    /// <param name="key">键.</param>
    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    /// <summary>
    /// 获取指定 key 的增量值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="incrBy">增量.</param>
    /// <returns></returns>
    public long Incrby(string key, long incrBy)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取指定 key 的增量值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="incrBy">增量.</param>
    /// <returns></returns>
    public Task<long> IncrbyAsync(string key, long incrBy)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 获取指定 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    public string Get(string key)
    {
        return _memoryCache.GetOrCreate(key, entry => { return entry.Value; })?.ToString();
    }

    /// <summary>
    /// 获取指定 key 的值.
    /// </summary>
    /// <typeparam name="T">byte[] 或其他类型.</typeparam>
    /// <param name="key">键.</param>
    public T Get<T>(string key)
    {
        ICacheEntry entry = _memoryCache.Get<ICacheEntry>(key);
        return entry == null ? default(T) : (T)(entry.Value);
    }

    /// <summary>
    /// 获取指定 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    public Task<string> GetAsync(string key)
    {
        return Task.FromResult(Get(key));
    }

    /// <summary>
    /// 获取指定 key 的值.
    /// </summary>
    /// <typeparam name="T">byte[] 或其他类型.</typeparam>
    /// <param name="key">键.</param>
    public Task<T> GetAsync<T>(string key)
    {
        return Task.FromResult(Get<T>(key));
    }

    /// <summary>
    /// 设置指定 key 的值，所有写入参数object都支持string | byte[] | 数值 | 对象.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    public bool Set(string key, object value)
    {
        var entry = _memoryCache.CreateEntry(key);
        entry.Value = value;
        _memoryCache.Set(key, entry);
        return true;
    }

    /// <summary>
    /// 设置指定 key 的值，所有写入参数object都支持string | byte[] | 数值 | 对象.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="expire">有效期.</param>
    public bool Set(string key, object value, TimeSpan expire)
    {
        var entry = _memoryCache.CreateEntry(key);
        entry.Value = value;
        _memoryCache.Set(key, entry, expire);
        return true;
    }

    /// <summary>
    /// 设置指定 key 的值，所有写入参数object都支持string | byte[] | 数值 | 对象.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    public Task<bool> SetAsync(string key, object value)
    {
        var entry = _memoryCache.CreateEntry(key);
        entry.Value = value;
        Set(key, entry);
        return Task.FromResult(true);
    }

    /// <summary>
    /// 保存.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="expire">过期时间.</param>
    public Task<bool> SetAsync(string key, object value, TimeSpan expire)
    {
        var entry = _memoryCache.CreateEntry(key);
        entry.Value = value;
        Set(key, entry, expire);
        return Task.FromResult(true);
    }

    /// <summary>
    /// 获取所有key.
    /// </summary>
    public List<string> GetAllKeys()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var entries = _memoryCache.GetType().GetField("_entries", flags).GetValue(_memoryCache);
        var cacheItems = entries.GetType().GetProperty("Keys").GetValue(entries) as ICollection<object>;
        var keys = new List<string>();
        if (cacheItems == null) return keys;
        return cacheItems.Select(u => u.ToString()).ToList();
    }

    /// <summary>
    /// 获取缓存过期时间0.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public DateTime GetCacheOutTime(string key)
    {
        ICacheEntry? entry = _memoryCache.GetOrCreate(key, entry =>
        {
            return entry;
        });
        if (entry.AbsoluteExpiration == null)
        {
            return DateTime.Now;
        }

        DateTimeOffset dateTimeOffset = (DateTimeOffset)entry.AbsoluteExpiration;
        return dateTimeOffset.UtcDateTime;
    }

    /// <summary>
    /// 只有在 key 不存在时设置 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    /// <param name="expire">有效期.</param>
    public bool SetNx(string key, object value, TimeSpan expire)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 只有在 key 不存在时设置 key 的值.
    /// </summary>
    /// <param name="key">键.</param>
    /// <param name="value">值.</param>
    public bool SetNx(string key, object value)
    {
        throw new NotImplementedException();
    }
}