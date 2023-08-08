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

using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using Opc.Ua;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Application;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.OPCUA;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.OPCUA;

/// <summary>
/// OPCUA客户端
/// </summary>
public class OPCUAClient : CollectBase
{
    readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(60));
    /// <summary>
    /// OPCUA客户端
    /// </summary>
    public OPCUAClient()
    {
        _ = RunTimerAsync();
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            if (PLC != null && PLC.Session == null)
            {
                try
                {
                    await PLC.ConnectAsync();
                }
                catch (Exception ex)
                {
                    LogMessage.Exception(ex);
                }
            }
        }

    }

    internal CollectDeviceRunTime Device;

    internal Foundation.Adapter.OPCUA.OPCUAClient PLC = null;

    private List<DeviceVariableRunTime> _deviceVariables = new();

    private readonly OPCUAClientProperty driverPropertys = new();

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(OPCUAClientDebugDriverPage);

    /// <inheritdoc/>
    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

    /// <inheritdoc/>
    public override bool IsSupportRequest => !driverPropertys.ActiveSubscribe;

    /// <inheritdoc/>
    public override ThingsGatewayBitConverter ThingsGatewayBitConverter { get; } = new(EndianType.Little);
    /// <inheritdoc/>
    public override Task AfterStopAsync()
    {
        PLC?.Disconnect();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task BeforStartAsync(CancellationToken token)
    {
        await PLC?.ConnectAsync();
    }
    /// <inheritdoc/>
    public override void InitDataAdapter()
    {
    }

    /// <inheritdoc/>
    public override bool IsConnected()
    {

        return PLC.Connected;
    }

    /// <inheritdoc/>
    public override List<DeviceVariableSourceRead> LoadSourceRead(List<DeviceVariableRunTime> deviceVariables)
    {
        _deviceVariables = deviceVariables;
        PLC.Variables.AddRange(deviceVariables.Select(a => a.VariableAddress).ToList());
        var dataLists = deviceVariables.ChunkTrivialBetter(driverPropertys.GroupSize);
        var dataResult = new List<DeviceVariableSourceRead>();
        foreach (var variable in dataLists)
        {
            var sourVars = new DeviceVariableSourceRead(driverPropertys.UpdateRate)
            {
                Address = "",
                DeviceVariables = variable
            };
            dataResult.Add(sourVars);
        }

        return dataResult;

    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadSourceAsync(DeviceVariableSourceRead deviceVariableSourceRead, CancellationToken token)
    {
        var result = await PLC.ReadJTokenValueAsync(deviceVariableSourceRead.DeviceVariables.Select(a => a.VariableAddress).ToArray(), token);
        foreach (var data in result)
        {
            if (!token.IsCancellationRequested)
            {
                var data1 = deviceVariableSourceRead.DeviceVariables.Where(a => a.VariableAddress == data.Item1);

                foreach (var item in data1)
                {

                    object value;
                    if (data.Item3 is JValue jValue)
                    {
                        value = jValue.Value;
                    }
                    else
                    {
                        value = data.Item3;
                    }
                    var quality = StatusCode.IsGood(data.Item2.StatusCode);

                    var time = data.Item2.SourceTimestamp;
                    if (value != null && quality)
                    {
                        var operResult = item.SetValue(value, time);
                        if (!operResult.IsSuccess)
                        {
                            LogMessage?.LogWarning(operResult.Message);
                        }
                    }
                    else
                    {
                        var operResult = item.SetValue(null, time);
                        if (!operResult.IsSuccess)
                        {
                            LogMessage?.LogWarning(operResult.Message);
                        }
                    }
                }
                LogMessage.Trace(FoundationConst.LogMessageHeader + ToString() + "状态变化:" + Environment.NewLine + data.Item1 + ":" + data.Item3.ToString());

            }
        }
        if (result.Any(a => StatusCode.IsBad(a.Item2.StatusCode)))
        {
            return new OperResult<byte[]>($"读取失败");
        }
        else
        {
            return OperResult.CreateSuccessResult<byte[]>(null);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult> WriteValueAsync(DeviceVariableRunTime deviceVariable, JToken value, CancellationToken token)
    {
        var result = await PLC.WriteNodeAsync(deviceVariable.VariableAddress, value, token);
        return result;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _periodicTimer?.Dispose();
        if (PLC != null)
        {
            PLC.DataChangedHandler -= DataChangedHandler;
            PLC.Disconnect();
            PLC.SafeDispose();
            PLC = null;
        }
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        Device = device;
        OPCNode opcNode = new()
        {
            OPCUrl = driverPropertys.OPCURL,
            UpdateRate = driverPropertys.UpdateRate,
            DeadBand = driverPropertys.DeadBand,
            GroupSize = driverPropertys.GroupSize,
            KeepAliveInterval = driverPropertys.KeepAliveInterval,
            IsUseSecurity = driverPropertys.IsUseSecurity,
            ActiveSubscribe = driverPropertys.ActiveSubscribe,
            UserName = driverPropertys.UserName,
            Password = driverPropertys.Password
        };
        if (PLC == null)
        {
            PLC = new(LogMessage);
            PLC.DataChangedHandler += DataChangedHandler;
        }

        PLC.OPCNode = opcNode;
    }

    /// <inheritdoc/>
    protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token)
    {
        //不走ReadAsync
        throw new NotImplementedException();
    }

    private void DataChangedHandler((VariableNode variableNode, DataValue dataValue, JToken jToken) data)
    {
        try
        {
            if (!Device.KeepRun)
            {
                return;
            }

            LogMessage.Trace(FoundationConst.LogMessageHeader + ToString() + "状态变化:" + Environment.NewLine + data.variableNode.NodeId + ":" + data.jToken?.ToString());

            if (!Device.KeepRun)
            {
                return;
            }
            //尝试固定点位的数据类型
            var type = TypeInfo.GetSystemType(TypeInfo.GetBuiltInType(data.variableNode.DataType, PLC.Session.SystemContext.TypeTable), data.variableNode.ValueRank);

            var itemReads = _deviceVariables.Where(it => it.VariableAddress == data.variableNode.NodeId).ToList();

            object value;
            if (data.jToken is JValue jValue)
            {
                value = jValue.Value;
            }
            else
            {
                value = data.jToken;
            }
            var quality = StatusCode.IsGood(data.dataValue.StatusCode);

            var time = data.dataValue.SourceTimestamp.ToLocalTime();
            foreach (var item in itemReads)
            {
                if (item.DataTypeEnum == DataTypeEnum.Object)
                    if (type.Namespace.StartsWith("System"))
                        try { item.DataTypeEnum = Enum.Parse<DataTypeEnum>(type.Name); } catch { }
                if (value != null && quality)
                {
                    var operResult = item.SetValue(value, time);
                    if (!operResult.IsSuccess)
                    {
                        LogMessage?.LogWarning(operResult.Message);
                    }
                }
                else
                {
                    var operResult = item.SetValue(null, time);
                    if (!operResult.IsSuccess)
                    {
                        LogMessage?.LogWarning(operResult.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, ToString());
        }
    }
}
