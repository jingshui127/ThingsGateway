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

using SqlSugar;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 第三方用户表
///</summary>
[SugarTable("openapi_user", TableDescription = "第三方用户表")]
[Tenant(SqlSugarConst.DB_Admin)]
public class OpenApiUser : BaseEntity
{
    /// <summary>
    /// 账号
    ///</summary>
    [SugarColumn(ColumnDescription = "账号", Length = 200)]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public virtual string Account { get; set; }

    /// <summary>
    /// 邮箱
    ///</summary>
    [SugarColumn(ColumnDescription = "邮箱", Length = 200, IsNullable = true)]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public string Email { get; set; }

    /// <summary>
    /// 上次登录设备
    ///</summary>
    [SugarColumn(ColumnDescription = "上次登录设备", IsNullable = true)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, DefaultFilter = true)]
    public string LastLoginDevice { get; set; }

    /// <summary>
    /// 上次登录ip
    ///</summary>
    [SugarColumn(ColumnDescription = "上次登录ip", Length = 200, IsNullable = true)]
    [DataTable(Order = 9, IsShow = true, Sortable = true, DefaultFilter = true)]
    public string LastLoginIp { get; set; }

    /// <summary>
    /// 上次登录时间
    ///</summary>
    [SugarColumn(ColumnDescription = "上次登录时间", IsNullable = true)]
    [DataTable(Order = 10, IsShow = true, Sortable = true, DefaultFilter = true)]
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 最新登录设备
    ///</summary>
    [SugarColumn(ColumnDescription = "最新登录设备", IsNullable = true)]
    [DataTable(Order = 5, IsShow = true, Sortable = true)]
    public string LatestLoginDevice { get; set; }

    /// <summary>
    /// 最新登录ip
    ///</summary>
    [SugarColumn(ColumnDescription = "最新登录ip", Length = 200, IsNullable = true)]
    [DataTable(Order = 6, IsShow = true, Sortable = true)]
    public string LatestLoginIp { get; set; }

    /// <summary>
    /// 最新登录时间
    ///</summary>
    [SugarColumn(ColumnDescription = "最新登录时间", IsNullable = true)]
    [DataTable(Order = 7, IsShow = true, Sortable = true)]
    public DateTime? LatestLoginTime { get; set; }

    /// <summary>
    /// 密码
    ///</summary>
    [SugarColumn(ColumnDescription = "密码", Length = 200)]
    public virtual string Password { get; set; }

    /// <summary>
    /// 权限码集合
    /// </summary>
    [SugarColumn(ColumnDescription = "权限json", ColumnDataType = StaticConfig.CodeFirst_BigString, IsJson = true, IsNullable = true)]
    public List<string> PermissionCodeList { get; set; }

    /// <summary>
    /// 手机
    ///</summary>
    [SugarColumn(ColumnDescription = "手机", Length = 200, IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public string Phone { get; set; }

    /// <summary>
    /// 是否启用
    ///</summary>
    [SugarColumn(ColumnDescription = "用户状态")]
    [DataTable(Order = 4, IsShow = true, Sortable = true)]
    public virtual bool UserEnable { get; set; }
}