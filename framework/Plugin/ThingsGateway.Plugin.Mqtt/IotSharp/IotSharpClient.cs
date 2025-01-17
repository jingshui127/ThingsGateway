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

using IoTSharp.Data;

using Mapster;

using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;

using System.Collections.Concurrent;

using ThingsGateway.Admin.Core;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// 参考IotSharpClient.SDK.MQTT
/// </summary>
public class IotSharpClient : UpLoadBase
{
    /// <summary>
    /// rpcmethodname存疑，定为自定义方法，在ThingsGateway上写入变量的方法固定为"Write"
    /// </summary>
    private const string WriteMethod = "WRITE";

    private readonly IotSharpClientProperty driverPropertys = new();
    private readonly EasyLock easyLock = new();
    private readonly IotSharpClientVariableProperty variablePropertys = new();
    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private GlobalDeviceData _globalDeviceData;

    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private RpcSingletonService _rpcCore;
    private List<DeviceVariableRunTime> _uploadVariables = new();
    private CollectDeviceWorker collectDeviceHostService;
    /// <inheritdoc/>
    public override Type DriverDebugUIType => null;
    private TimerTick exVariableTimerTick;
    private TimerTick exDeviceTimerTick;


    /// <inheritdoc/>
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;
    /// <inheritdoc/>
    public override List<DeviceVariableRunTime> UploadVariables => _uploadVariables;
    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override Task AfterStopAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient != null)
        {
            var result = await TryMqttClientAsync(cancellationToken);
            if (!result.IsSuccess)
            {
                LogMessage?.LogWarning($"{ToString()}-连接MqttServer失败：{result.Message}");
            }
        }
    }
    /// <inheritdoc/>
    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!driverPropertys.IsInterval)
            {
                ////变化推送
                var varList = _collectVariableRunTimes.ToListWithDequeue();
                if (varList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制
                    var varData = varList.GroupBy(a => a.DeviceName).ToList();
                    foreach (var item in varData)
                    {
                        try
                        {
                            Dictionary<string, object> nameValueDict = new();
                            foreach (var pair in item)
                            {
                                //只用最新的变量值
                                nameValueDict.AddOrUpdate(pair.Name, pair.Value);
                            }
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                await MqttUp($"devices/{item.Key}/telemetry", nameValueDict.ToJsonString(), cancellationToken);
                            }
                            else
                            {
                                break;
                            }

                        }
                        catch (Exception ex)
                        {
                            LogMessage?.LogWarning(ex);
                        }

                    }
                }
            }
            else
            {
                if (exVariableTimerTick.IsTickHappen())
                {
                    try
                    {
                        var varList = _uploadVariables.Adapt<List<VariableData>>();
                        if (varList?.Count != 0)
                        {
                            //分解List，避免超出mqtt字节大小限制
                            var varData = varList.GroupBy(a => a.DeviceName).ToList();
                            foreach (var item in varData)
                            {
                                try
                                {
                                    Dictionary<string, object> nameValueDict = new();
                                    foreach (var pair in item)
                                    {
                                        //只用最新的变量值
                                        nameValueDict.AddOrUpdate(pair.Name, pair.Value);
                                    }
                                    if (!cancellationToken.IsCancellationRequested)
                                    {
                                        await MqttUp($"devices/{item.Key}/telemetry", nameValueDict.ToJsonString(), cancellationToken);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    LogMessage?.LogWarning(ex);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }

                }
            }

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
        try
        {
            if (!driverPropertys.IsInterval)
            {
                ////变化推送
                var devList = _collectDeviceRunTimes.ToListWithDequeue();
                if (devList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制
                    var devData = devList.ChunkTrivialBetter(driverPropertys.SplitSize);
                    foreach (var item in devData)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {

                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage?.LogWarning(ex);
                        }
                    }

                }
            }
            else
            {
                if (exDeviceTimerTick.IsTickHappen())
                {
                    var devList = _collectDevice.Adapt<List<DeviceData>>();
                    if (devList?.Count != 0)
                    {
                        //分解List，避免超出mqtt字节大小限制
                        var devData = devList.ChunkTrivialBetter(driverPropertys.SplitSize);
                        foreach (var item in devData)
                        {
                            try
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {

                                }
                                else
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                LogMessage?.LogWarning(ex);
                            }
                        }

                    }

                }

            }

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            try
            {
                await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval, cancellationToken);
            }
            catch
            {
            }
        }
        else
        {

        }

    }

    /// <inheritdoc/>
    public override bool IsConnected() => _mqttClient?.IsConnected == true;


    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(IotSharpClient)}-IP:{driverPropertys.IP}-Port:{driverPropertys.Port}-Accesstoken:{driverPropertys.Accesstoken}";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            _globalDeviceData?.AllVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);

            _collectDevice?.ForEach(a =>
            {
                a.DeviceStatusChange -= DeviceStatusChange;
            });
            _mqttClient?.SafeDispose();
            _mqttClient = null;
            _uploadVariables = null;
            _collectDeviceRunTimes.Clear();
            _collectVariableRunTimes.Clear();
            _collectDeviceRunTimes = null;
            _collectVariableRunTimes = null;
        }
        catch (Exception ex)
        {
            LogMessage?.LogError(ex);
        }

    }
    private List<CollectDeviceRunTime> _collectDevice;

    /// <inheritdoc/>
    protected override void Init(UploadDeviceRunTime device)
    {
        var log = new MqttNetEventLogger();
        log.LogMessagePublished += Log_LogMessagePublished;
        var mqttFactory = new MqttFactory(log);
        _mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
           .WithClientId(Guid.NewGuid().ToString())
           .WithCredentials(driverPropertys.Accesstoken)//账密
           .WithTcpServer(driverPropertys.IP, driverPropertys.Port)//服务器
           .WithCleanSession(true)
           .WithKeepAlivePeriod(TimeSpan.FromSeconds(120.0))
           .Build();
        _mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic($"devices/+/rpc/request/+/+");//RPC控制请求，需要订阅
                })

            .Build();
        _mqttClient = mqttFactory.CreateMqttClient();
        _mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
        _globalDeviceData = App.GetService<GlobalDeviceData>();
        _rpcCore = App.GetService<RpcSingletonService>();
        collectDeviceHostService = BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>();


        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
           .Where(b => GetPropertyValue(b, nameof(variablePropertys.Enable)).GetBoolValue())
           .ToList();

        _uploadVariables = tags;

        _collectDevice = _globalDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ToList();

        _collectDevice?.ForEach(a =>
        {
            a.DeviceStatusChange += DeviceStatusChange;
        });
        _uploadVariables.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
        });
        if (driverPropertys.UploadInterval <= 1000) driverPropertys.UploadInterval = 1000;
        exVariableTimerTick = new(driverPropertys.UploadInterval);
        exDeviceTimerTick = new(driverPropertys.UploadInterval);
    }

    private void Log_LogMessagePublished(object sender, MqttNetLogMessagePublishedEventArgs e)
    {
        LogMessage.LogOut(e.LogMessage.Level, e.LogMessage.Source, e.LogMessage.Message, e.LogMessage.Exception);
    }

    private void DeviceStatusChange(CollectDeviceRunTime collectDeviceRunTime)
    {
        if (driverPropertys?.IsInterval != true)
            _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {

        if (e.ApplicationMessage.Topic.StartsWith($"devices/") && e.ApplicationMessage.Topic.Contains("/rpc/request/"))
        {
            var tps = e.ApplicationMessage.Topic.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var rpcmethodname = tps[4];
            var rpcdevicename = tps[1];
            var rpcrequestid = tps[5];
            if (!string.IsNullOrEmpty(rpcmethodname) && !string.IsNullOrEmpty(rpcdevicename) && !string.IsNullOrEmpty(rpcrequestid))
            {
                var rpcResponse = new RpcResponse()
                {
                    DeviceId = rpcdevicename,
                    ResponseId = rpcrequestid,
                    Method = rpcmethodname,
                    Success = false,
                    Data = "参数为空"
                };
                await SendResponseAsync(rpcResponse);
                return;
            }
            if (!driverPropertys.DeviceRpcEnable)
            {
                var rpcResponse = new RpcResponse()
                {
                    DeviceId = rpcdevicename,
                    ResponseId = rpcrequestid,
                    Method = rpcmethodname,
                    Success = false,
                    Data = "不允许写入"
                };
                await SendResponseAsync(rpcResponse);
                return;
            }
            //rpcmethodname定为自定义方法，在ThingsGateway上写入变量的方法固定为"Write"
            if (rpcmethodname.ToUpper() != WriteMethod)
            {
                var rpcResponse = new RpcResponse()
                {
                    DeviceId = rpcdevicename,
                    ResponseId = rpcrequestid,
                    Method = rpcmethodname,
                    Success = false,
                    Data = "不支持的方法"
                };
                await SendResponseAsync(rpcResponse);
                return;
            }
            else
            {
                RpcResponse rpcResponse = new();
                var nameValue = e.ApplicationMessage.ConvertPayloadToString().FromJsonString<List<KeyValuePair<string, string>>>();
                Dictionary<string, OperResult> results = new();
                if (nameValue?.Count > 0)
                {
                    foreach (var item in nameValue)
                    {
                        var tag = _uploadVariables.FirstOrDefault(a => a.Name == item.Key);
                        if (tag != null)
                        {
                            var rpcEnable =
GetPropertyValue(tag, nameof(variablePropertys.VariableRpcEnable)).ToBoolean()
&& driverPropertys.DeviceRpcEnable;
                            if (rpcEnable == true)
                            {


                            }
                            else
                            {
                                results.Add(item.Key, new OperResult("权限不足，变量不支持写入"));
                            }

                        }
                        else
                        {
                            results.Add(item.Key, new OperResult("不存在该变量"));
                        }
                    }

                    var result = await _rpcCore.InvokeDeviceMethodAsync(ToString() + "-" + rpcrequestid, nameValue
                        .Where(a => !results.Any(b => b.Key == a.Key))
                        .ToDictionary(a => a.Key, a => a.Value));

                    results.AddRange(result);
                    rpcResponse = new()
                    {
                        DeviceId = rpcdevicename,
                        ResponseId = rpcrequestid,
                        Method = rpcmethodname,
                        Success = !results.Any(a => !a.Value.IsSuccess),
                        Data = results.ToJsonString()
                    };
                }
                else
                {
                    rpcResponse = new()
                    {
                        DeviceId = rpcdevicename,
                        ResponseId = rpcrequestid,
                        Method = rpcmethodname,
                        Success = false,
                        Data = "消息体参数无法解析"
                    };
                }

                await SendResponseAsync(rpcResponse);

            }


        }

        async Task SendResponseAsync(RpcResponse rpcResponse)
        {
            try
            {
                var topic = $"devices/{rpcResponse.DeviceId}/rpc/response/{rpcResponse.Method}/{rpcResponse.ResponseId}";

                var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{topic}")
.WithPayload(rpcResponse.ToJsonString()).Build();
                var isConnect = await TryMqttClientAsync(CancellationToken.None);
                if (isConnect.IsSuccess)
                    await _mqttClient.PublishAsync(variableMessage);
            }
            catch
            {
            }
        }

    }

    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions);

        if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
        {
            LogMessage?.LogWarning(subResult.Items
                .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                .Select(a => a.ToString()).ToJsonString());
        }
    }
    /// <summary>
    /// 上传mqtt内容，并进行离线缓存
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="payLoad"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task MqttUp(string topic, string payLoad, CancellationToken cancellationToken)
    {
        var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic(topic)
.WithPayload(payLoad).Build();
        var isConnect = await TryMqttClientAsync(cancellationToken);
        if (isConnect.IsSuccess)
        {
            //连接成功时补发缓存数据
            var cacheData = await CacheDb.GetCacheData();
            foreach (var item in cacheData)
            {
                var cacheMessage = new MqttApplicationMessageBuilder()
.WithTopic(item.Topic)
.WithPayload(item.CacheStr).Build();
                var cacheResult = await _mqttClient.PublishAsync(cacheMessage, cancellationToken);
                if (cacheResult.IsSuccess)
                {
                    await CacheDb.DeleteCacheData(item.Id);
                    LogMessage.Trace($"{FoundationConst.LogMessageHeader}主题：{item.Topic}{Environment.NewLine}负载：{item.CacheStr}");
                }
            }

            var result = await _mqttClient.PublishAsync(variableMessage, cancellationToken);
            if (!result.IsSuccess)
            {
                await CacheDb.AddCacheData(topic, payLoad, driverPropertys.CacheMaxCount);
            }
            else
            {
                LogMessage.Trace($"{FoundationConst.LogMessageHeader}主题：{topic}{Environment.NewLine}负载：{payLoad}");
            }
        }
        else
        {
            await CacheDb.AddCacheData(topic, payLoad, driverPropertys.CacheMaxCount);
        }
    }

    private async Task<OperResult> TryMqttClientAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient?.IsConnected == true)
            return OperResult.CreateSuccessResult();
        return await Cilent();

        async Task<OperResult> Cilent()
        {
            if (_mqttClient?.IsConnected == true)
                return OperResult.CreateSuccessResult();
            try
            {
                await easyLock.WaitAsync();
                if (_mqttClient?.IsConnected == true)
                    return OperResult.CreateSuccessResult();
                using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(driverPropertys.ConnectTimeOut));
                using CancellationTokenSource StoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);
                if (_mqttClient?.IsConnected == true)
                    return OperResult.CreateSuccessResult();
                if (_mqttClient == null)
                    return new OperResult("未初始化");
                var result = await _mqttClient?.ConnectAsync(_mqttClientOptions, StoppingToken.Token);
                if (result.ResultCode == MqttClientConnectResultCode.Success)
                {
                    return OperResult.CreateSuccessResult();
                }
                else
                {
                    return new OperResult(result.ReasonString);
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                easyLock.Release();
            }
        }
    }
    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        if (driverPropertys?.IsInterval != true)
            _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}
