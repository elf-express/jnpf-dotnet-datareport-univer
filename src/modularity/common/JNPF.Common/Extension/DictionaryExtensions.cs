﻿using JNPF.DependencyInjection;
using System.Data;

namespace JNPF.Common.Extension;

/// <summary>
/// 字典辅助扩展方法.
/// </summary>
[SuppressSniffer]
public static class DictionaryExtensions
{
    /// <summary>
    /// 从字典中获取值，不存在则返回字典<typeparamref name="TValue"/>类型的默认值.
    /// </summary>
    /// <typeparam name="TKey">字典键类型.</typeparam>
    /// <typeparam name="TValue">字典值类型.</typeparam>
    /// <param name="dictionary">要操作的字典.</param>
    /// <param name="key">指定键名.</param>
    /// <returns>获取到的值.</returns>
    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
    {
        return dictionary.TryGetValue(key, out TValue value) ? value : default(TValue);
    }

    /// <summary>
    /// 获取指定键的值，不存在则按指定委托添加值.
    /// </summary>
    /// <typeparam name="TKey">字典键类型.</typeparam>
    /// <typeparam name="TValue">字典值类型.</typeparam>
    /// <param name="dictionary">要操作的字典.</param>
    /// <param name="key">指定键名.</param>
    /// <param name="addFunc">添加值的委托.</param>
    /// <returns>获取到的值.</returns>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> addFunc)
    {
        if (dictionary.TryGetValue(key, out TValue value))
        {
            return value;
        }

        return dictionary[key] = addFunc();
    }

    /// <summary>
    /// 替换值.
    /// </summary>
    /// <param name="dictionary1"></param>
    /// <param name="dictionary2"></param>
    public static void ReplaceValue(this Dictionary<string, object> dictionary1, Dictionary<string, object> dictionary2)
    {
        foreach (var item in dictionary2.Keys)
        {
            if (dictionary2[item] != null && !(dictionary2[item] is Array array && array.Length == 0) && dictionary1.ContainsKey(item))
            {
                dictionary1[item] = dictionary2[item];
            }
        }
    }

    /// <summary>
    /// DataTable转DicList.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static List<Dictionary<string, object>> DataTableToDicList(DataTable dt)
    {
        return dt.AsEnumerable().Select(
                row => dt.Columns.Cast<DataColumn>().ToDictionary(
                column => column.ColumnName,
                column => row[column])).ToList();
    }

    /// <summary>
    /// 动态表单时间格式转换.
    /// </summary>
    /// <param name="diclist"></param>
    /// <returns></returns>
    public static List<Dictionary<string, object>> DateConver(List<Dictionary<string, object>> diclist)
    {
        foreach (var item in diclist)
        {
            foreach (var dic in item.Keys)
            {
                if (item[dic] is DateTime)
                {
                    item[dic] = item[dic].ToString() + " ";
                }

                if (item[dic] is decimal)
                {
                    item[dic] = item[dic].ToString();
                }
            }
        }
        return diclist;
    }
}