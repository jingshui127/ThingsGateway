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

using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Application;

namespace ThingsGateway.Blazor;
/// <summary>
/// 插件管理页面
/// </summary>
public partial class DriverPluginPage
{
    private IAppDataTable _datatable;
    private readonly DriverPluginPageInput search = new();

    private async Task AddCallAsync(DriverPluginAddInput input)
    {
        await DriverPluginService.AddAsync(input);
    }

    private async Task<SqlSugarPagedList<DriverPlugin>> QueryCallAsync(DriverPluginPageInput input)
    {
        var data = await DriverPluginService.PageAsync(input);
        return data;
    }
}