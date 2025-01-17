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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 角色常量
/// </summary>
public class RoleConst
{
    /// <summary>
    /// 超级管理员
    /// </summary>
    public const string SuperAdmin = "superAdmin";

    #region 关系表

    /// <summary>
    /// 角色有哪些权限
    /// </summary>
    public const string Relation_SYS_ROLE_HAS_PERMISSION = "SYS_ROLE_HAS_PERMISSION";

    /// <summary>
    /// 角色有哪些资源
    /// </summary>
    public const string Relation_SYS_ROLE_HAS_RESOURCE = "SYS_ROLE_HAS_RESOURCE";

    /// <summary>
    /// 用户有哪些角色
    /// </summary>
    public const string Relation_SYS_USER_HAS_ROLE = "SYS_USER_HAS_ROLE";

    #endregion 关系表

}