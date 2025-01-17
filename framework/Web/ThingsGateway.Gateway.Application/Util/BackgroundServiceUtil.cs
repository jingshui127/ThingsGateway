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

using Furion;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 获取后台服务
/// </summary>
public static class BackgroundServiceUtil
{
    /// <summary>
    /// IServiceProvider获取后台服务，适用于IHostedService
    /// </summary>
    public static T GetBackgroundService<T>() where T : class, IHostedService
    {
        var hostedService = App.RootServices.GetServices<IHostedService>().FirstOrDefault(it => it is T) as T;
        return hostedService;
    }
}
