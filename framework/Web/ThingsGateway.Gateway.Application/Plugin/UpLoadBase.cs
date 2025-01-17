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

using Microsoft.Extensions.Logging;



namespace ThingsGateway.Gateway.Application;


/// <summary>
/// 上传插件
/// <para></para>
/// 约定：
/// <para></para>
/// 如果设备属性需要密码输入，属性名称中需包含Password字符串
/// <para></para>
/// 如果设备属性需要大文本输入，属性名称中需包含BigText字符串
/// <br></br>
/// 因为自定义上传插件需求比较大，这里着重解释代码运行原理
/// 继承<see cref="UpLoadBase"/>后，可以看到需要实现各类虚方法/属性<br></br>
/// <see cref="UploadVariables"/> <br></br>
/// <see cref="VariablePropertys"/><br></br>
/// <see cref="DriverBase.DriverPropertys"/><br></br>
/// <see cref="BeforStartAsync"/><br></br>
/// <see cref="ExecuteAsync"/><br></br>
/// <see cref="DriverBase.IsConnected"/><br></br>
/// <see cref="Init(UploadDeviceRunTime)"/><br></br>
/// 含义可看注释，下面看看网关上传插件的生命周期<br></br>
/// 1、构造函数<see cref="UpLoadBase()"/> 传入参数服务工厂，在需要获取服务时使用<see cref="App.GetService{TService}(IServiceProvider)"/><br></br>
/// 2、<see cref="Init(UploadDeviceRunTime)"/>初始化函数，传入上传设备参数，只执行一次，在这个方法内，一般会初始化一些必要的实例，比如new MqttClient，以及一些必要的实现属性，比如<see cref="UploadVariables"/><br></br>
/// 3、<see cref="BeforStartAsync"/>开始前执行的方法，比如连接mqtt等，只执行一次<br></br>
/// 4、<see cref="ExecuteAsync"/>核心执行的方法，需实现上传方法，在插件结束前会一直循环调用<br></br>
/// 5、<see cref="DisposableObject.Dispose(bool)"/> 结束时调用的方法，实现资源释放方法<br></br>
/// 网关的数据是如何传入到上传插件的，下面会以Mqtt上传为例<br></br>
/// 1、如何获取采集变量值？在初始化函数中<see cref="Init(UploadDeviceRunTime)"/>获取全局设备/变量<br></br>
/// 通过<see cref="App.GetService{TService}(IServiceProvider)"/>获取单例服务<see cref="GlobalDeviceData"/><br></br>
/// 可以看到在这个单例服务中，已经拥有全部的采集设备与变量<br></br>
/// 2、如何获取采集变量中的上传属性？UploadBase中封装了通用方法<see cref="GetPropertyValue(DeviceVariableRunTime, string)"/><br></br>
/// 比如定义了变量属性Enable，只有设置为true的变量才会用作某逻辑，执行方法GetPropertyValue(tag,"Enable")，也可用硬编码传入propertyName参数<br></br>
/// 3、如何定义自己的上传实体，第一步中获取获取单例服务<see cref="GlobalDeviceData"/>，在拥有全局变量下，可以使用<see cref="Mapster"/> 或者 手动赋值到DTO实体<br></br>
/// 4、完整的参考可以查看MqttClient插件<br></br>
/// </summary>
public abstract class UpLoadBase : DriverBase
{
    private string LogSource = string.Empty;

    /// <summary>
    /// 当前上传设备
    /// </summary>
    public UploadDeviceRunTime CurrentDevice { get; protected set; }

    /// <summary>
    /// 返回插件的上传变量，一般在<see cref="Init(UploadDeviceRunTime)"/>后初始化
    /// </summary>
    public abstract List<DeviceVariableRunTime> UploadVariables { get; }

    /// <summary>
    /// 插件配置项 ，继承实现<see cref="VariablePropertyBase"/>后，返回继承类，如果不存在，返回null
    /// </summary>
    public abstract VariablePropertyBase VariablePropertys { get; }

    /// <summary>
    /// 离线缓存
    /// </summary>
    protected CacheDb CacheDb { get; set; }

    private Microsoft.Extensions.Logging.LogLevel CustomLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Trace;

    /// <summary>
    /// 结束通讯后执行的方法
    /// </summary>
    /// <returns></returns>
    public abstract Task AfterStopAsync();

    /// <summary>
    /// 开始执行的方法
    /// </summary>
    /// <returns></returns>
    public abstract Task BeforStartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 循环执行
    /// </summary>
    public abstract Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// 获取设备的属性值
    /// </summary>
    public virtual string GetDevicePropertyValue(CollectDeviceRunTime collectDeviceRunTime, string propertyName)
    {

        if (collectDeviceRunTime == null)
            return null;
        return collectDeviceRunTime.DevicePropertys.FirstOrDefault(a => a.PropertyName == propertyName).Value;

    }

    /// <summary>
    /// 获取变量的属性值
    /// </summary>
    public virtual string GetPropertyValue(DeviceVariableRunTime variableRunTime, string propertyName)
    {
        if (variableRunTime == null)
            return null;
        if (variableRunTime.VariablePropertys.ContainsKey(CurrentDevice.Id))
        {
            var data = variableRunTime.VariablePropertys[CurrentDevice.Id].FirstOrDefault(a =>
                  a.PropertyName == propertyName);
            if (data != null)
            {
                return data.Value;
            }
        }
        return null;
    }


    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(ILogger logger, UploadDeviceRunTime device)
    {
        _logger = logger;
        IsLogOut = device.IsLogOut;
        CurrentDevice = device;
        CacheDb = new(CurrentDevice.Id.ToString());
        CustomLevel = App.GetConfig<Microsoft.Extensions.Logging.LogLevel?>("Logging:LogLevel:BackendLog") ?? Microsoft.Extensions.Logging.LogLevel.Trace;
        LogSource = "上传设备：" + CurrentDevice.Name;
        Init(device);
    }
    internal override void NewMessage(ThingsGateway.Foundation.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        if (IsSaveLog)
        {
            if (arg3.StartsWith(FoundationConst.LogMessageHeader))
            {
                if ((byte)arg1 < (byte)CustomLevel)
                {
                    var logRuntime = new BackendLog
                    {
                        LogLevel = (Microsoft.Extensions.Logging.LogLevel)arg1,
                        LogMessage = arg3,
                        LogSource = LogSource,
                        LogTime = DateTimeExtensions.CurrentDateTime,
                        Exception = null,
                    };
                    _logQueues.Enqueue(logRuntime);
                }
            }

        }
        base.NewMessage(arg1, arg2, arg3, arg4);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="device">设备</param>
    protected abstract void Init(UploadDeviceRunTime device);

    /// <summary>
    /// 底层日志输出
    /// </summary>
    protected override void Log_Out(ThingsGateway.Foundation.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        if (arg1 >= ThingsGateway.Foundation.Core.LogLevel.Warning)
        {
            CurrentDevice.SetDeviceStatus(lastErrorMessage: arg3);
        }
        if (IsLogOut || arg1 >= ThingsGateway.Foundation.Core.LogLevel.Warning)
        {
            _logger.Log_Out(arg1, arg2, arg3, arg4);
        }
    }
}


