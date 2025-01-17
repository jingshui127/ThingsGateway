﻿#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Extension.ConcurrentQueue;

/// <inheritdoc/>
public static class ConcurrentQueueExtensions
{
    /// <summary>
    /// 批量出队
    /// </summary>
    public static List<T> ToListWithDequeue<T>(this ConcurrentQueue<T> values, int conut = 0)
    {
        if (conut == 0)
        {
            conut = values.Count;
        }
        List<T> newlist = new();
        for (int i = 0; i < conut; i++)
        {
            if (values.TryDequeue(out T result))
            {
                newlist.Add(result);
            }
            else
            {
                break;
            }
        }
        return newlist;
    }
}