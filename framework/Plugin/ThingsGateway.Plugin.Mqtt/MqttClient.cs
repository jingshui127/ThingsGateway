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

using Mapster;

using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;

using System.Collections.Concurrent;
using System.Text;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttClient
/// </summary>
public class MqttClient : UpLoadBase
{
    private readonly MqttClientProperty driverPropertys = new();
    private readonly EasyLock easyLock = new();
    private readonly MqttClientVariableProperty variablePropertys = new();
    private List<CollectDeviceRunTime> _collectDevice;
    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private GlobalDeviceData _globalDeviceData;
    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private RpcSingletonService _rpcCore;

    private List<DeviceVariableRunTime> _uploadVariables = new();

    private CollectDeviceWorker collectDeviceHostService;
    private TimerTick exDeviceTimerTick;

    private TimerTick exVariableTimerTick;
    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(MqttClientDebugPage);
    /// <inheritdoc/>
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;


    /// <inheritdoc/>
    public override List<DeviceVariableRunTime> UploadVariables => _uploadVariables;


    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    /// <inheritdoc/>
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
                    var varData = varList.ChunkTrivialBetter(driverPropertys.SplitSize);
                    foreach (var item in varData)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                await MqttUp($"{driverPropertys.VariableTopic}", item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel), cancellationToken);

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
                            var varData = varList.ChunkTrivialBetter(driverPropertys.SplitSize);
                            foreach (var item in varData)
                            {
                                try
                                {
                                    if (!cancellationToken.IsCancellationRequested)
                                    {

                                        await MqttUp($"{driverPropertys.VariableTopic}", item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel), cancellationToken);

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
                                await MqttUp($"{driverPropertys.DeviceTopic}", item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel), cancellationToken);
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
                                    await MqttUp($"{driverPropertys.DeviceTopic}", item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel), cancellationToken);
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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override bool IsConnected() => _mqttClient?.IsConnected == true;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(MqttClient)} IP:{driverPropertys.IP} Port:{driverPropertys.Port}";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        try
        {
            _globalDeviceData?.AllVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);

            _globalDeviceData?.CollectDevices?.ForEach(a =>
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
            base.Dispose(disposing);
        }
        catch (Exception ex)
        {
            LogMessage?.LogError(ex);
        }
    }

    /// <inheritdoc/>
    protected override void Init(UploadDeviceRunTime device)
    {
        var log = new MqttNetEventLogger();
        log.LogMessagePublished += Log_LogMessagePublished;
        var mqttFactory = new MqttFactory(log);
        _mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
           .WithClientId(driverPropertys.ConnectId)
           .WithCredentials(driverPropertys.UserName, driverPropertys.Password)//账密
           .WithTcpServer(driverPropertys.IP, driverPropertys.Port)//服务器
           .WithCleanSession(true)
           .WithKeepAlivePeriod(TimeSpan.FromSeconds(120.0))
           .WithoutThrowOnNonSuccessfulConnectResponse()
           .Build();
        _mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(driverPropertys.RpcWriteTopic);
                })
                     .WithTopicFilter(
                f =>
                {
                    f.WithTopic(driverPropertys.QuestRpcTopic);
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
        if (!driverPropertys.IsInterval)
        {
            _collectDevice.ForEach(a =>
            {
                a.DeviceStatusChange += DeviceStatusChange;
            });
            _uploadVariables.ForEach(a =>
            {
                a.VariableValueChange += VariableValueChange;
            });
        }

        if (driverPropertys.UploadInterval <= 1000) driverPropertys.UploadInterval = 1000;
        exVariableTimerTick = new(driverPropertys.UploadInterval);
        exDeviceTimerTick = new(driverPropertys.UploadInterval);

    }

    private void Log_LogMessagePublished(object sender, MqttNetLogMessagePublishedEventArgs e)
    {
        LogMessage.LogOut(e.LogMessage.Level, e.LogMessage.Source, e.LogMessage.Message, e.LogMessage.Exception);
    }

    private async Task AllPublishAsync(CancellationToken cancellationToken)
    {
        //保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = _globalDeviceData.AllVariables.Adapt<List<VariableData>>().ChunkTrivialBetter(driverPropertys.SplitSize);
        var devData = _globalDeviceData.CollectDevices.Adapt<List<DeviceData>>().ChunkTrivialBetter(driverPropertys.SplitSize);
        var isConnect = await TryMqttClientAsync(cancellationToken);
        foreach (var item in devData)
        {
            var devMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{driverPropertys.DeviceTopic}")
.WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel)).Build();
            if (isConnect.IsSuccess)
                await _mqttClient.PublishAsync(devMessage, cancellationToken);
        }

        foreach (var item in varData)
        {
            var varMessage = new MqttApplicationMessageBuilder()
            .WithTopic($"{driverPropertys.VariableTopic}")
            .WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel)).Build();
            if (isConnect.IsSuccess)
                await _mqttClient.PublishAsync(varMessage, cancellationToken);
        }
    }

    private void DeviceStatusChange(CollectDeviceRunTime collectDeviceRunTime)
    {
        if (driverPropertys?.IsInterval != true)
            _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        if (args.ApplicationMessage.Topic == driverPropertys.QuestRpcTopic && args.ApplicationMessage.PayloadSegment.Count > 0)
        {
            await AllPublishAsync(CancellationToken.None);
            return;
        }

        if (!driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(args.ClientId))
            return;
        if (args.ApplicationMessage.Topic != driverPropertys.RpcWriteTopic)
            return;
        var rpcDatas = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).FromJsonString<MqttRpcNameVaueWithId>();
        if (rpcDatas == null)
            return;

        MqttRpcResult mqttRpcResult = new() { RpcId = rpcDatas.RpcId, Success = true };
        try
        {
            foreach (var rpcData in rpcDatas.WriteInfos)
            {

                var tag = _uploadVariables.FirstOrDefault(a => a.Name == rpcData.Key);
                if (tag != null)
                {
                    var rpcEnable = GetPropertyValue(tag, nameof(variablePropertys.VariableRpcEnable)).ToBoolean();
                    if (rpcEnable == true)
                    {

                    }
                    else
                    {
                        mqttRpcResult.Success = false;
                        mqttRpcResult.Message.Add(rpcData.Key, new("权限不足，变量不支持写入"));
                    }
                }
                else
                {
                    mqttRpcResult.Success = false;
                    mqttRpcResult.Message.Add(rpcData.Key, new("不存在该变量"));
                }
            }

            var result = await _rpcCore.InvokeDeviceMethodAsync(ToString() + "-" + args.ClientId,
                rpcDatas.WriteInfos.Where(
                a => !mqttRpcResult.Message.Any(b => b.Key == a.Key)).ToDictionary(a => a.Key, a => a.Value));

            mqttRpcResult.Message.AddRange(result);
            mqttRpcResult.Success = !mqttRpcResult.Message.Any(a => !a.Value.IsSuccess);

        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
        try
        {
            var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{driverPropertys.RpcSubTopic}")
.WithPayload(mqttRpcResult.ToJsonString()).Build();
            var isConnect = await TryMqttClientAsync(CancellationToken.None);
            if (isConnect.IsSuccess)
                await _mqttClient.PublishAsync(variableMessage);
        }
        catch
        {
        }
    }

    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions);
        if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
        {
            LogMessage?.Warning($"订阅失败-{subResult.Items
                .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                .Select(a =>
                new
                {
                    Topic = a.TopicFilter.Topic,
                    ResultCode = a.ResultCode.ToString()
                }
                )
                .ToJsonString()}");
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
                {
                    return new OperResult("未初始化");

                }
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
