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

using Furion.DependencyInjection;
using Furion.EventBus;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 认证模块事件总线
/// </summary>
public class OpenApiAuthEventSubscriber : IEventSubscriber, ISingleton
{
    /// <summary>
    /// 登录事件
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe(EventSubscriberConst.LoginOpenApi)]
    public async Task LoginOpenApi(EventHandlerExecutingContext context)
    {
        LoginOpenApiEvent loginEvent = (LoginOpenApiEvent)context.Source.Payload;//获取参数
        OpenApiUser openApiUser = loginEvent.OpenApiUser;
        var db = DbContext.Db.CopyNew();
        #region 重新赋值属性,设置本次登录信息为最新的信息

        db.Tracking(openApiUser);//创建跟踪,只更新修改字段
        openApiUser.LastLoginDevice = openApiUser.LatestLoginDevice;
        openApiUser.LastLoginIp = openApiUser.LatestLoginIp;
        openApiUser.LastLoginTime = openApiUser.LatestLoginTime;
        openApiUser.LatestLoginDevice = loginEvent.Device.ToString();
        openApiUser.LatestLoginIp = loginEvent.Ip;
        openApiUser.LatestLoginTime = loginEvent.DateTime;

        #endregion 重新赋值属性,设置本次登录信息为最新的信息

        //更新用户信息
        if (await db.UpdateableWithAttr(openApiUser).ExecuteCommandAsync() > 0)
        {
            Cache.SysMemoryCache.Set(CacheConst.CACHE_OPENAPIUSER + openApiUser.Id, openApiUser, false); //更新Cache信息
        }
    }
}