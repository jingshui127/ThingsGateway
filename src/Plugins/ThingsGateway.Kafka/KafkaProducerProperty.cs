﻿#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Kafka;

/// <summary>
/// kafka 生产者属性
/// </summary>
public class KafkaProducerProperty : DriverPropertyBase
{
    [DeviceProperty("服务地址", "")] public string BootStrapServers { get; set; } = "127.0.0.1";
    [DeviceProperty("设备主题", "")] public string DeviceTopic { get; set; } = "test1";
    [DeviceProperty("变量主题", "")] public string VariableTopic { get; set; } = "test2";
    [DeviceProperty("分组ID", "")] public string GroupId { get; set; } = "test-consumer-group";
    [DeviceProperty("客户端ID", "")] public string ClientId { get; set; } = "test-consumer";
    [DeviceProperty("线程循环间隔", "最小500ms")] public int CycleInterval { get; set; } = 1000;

}