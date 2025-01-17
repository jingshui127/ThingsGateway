#region copyright
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

using System.ComponentModel;

using ThingsGateway.Gateway.Core.Extensions;

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// 变量运行状态表示
/// </summary>
public class DeviceVariableRunTime : DeviceVariable, IDeviceVariableRunTime
{

    private object _value;

    private bool isOnline;

    private bool isOnlineChanged;

    /// <summary>
    /// 谨慎使用，务必采用队列等方式
    /// </summary>
    public event VariableChangeEventHandler VariableValueChange;

    /// <summary>
    /// 谨慎使用，务必采用队列等方式
    /// </summary>
    public event VariableChangeEventHandler VariableCollectChange;

    /// <summary>
    /// 变化时间
    /// </summary>
    [Description("变化时间")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public DateTime ChangeTime { get; private set; }

    /// <summary>
    /// 所在采集设备
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    [Description("采集设备")]
    public CollectDeviceRunTime CollectDeviceRunTime { get; set; }

    /// <summary>
    /// 采集时间
    /// </summary>
    [Description("采集时间")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public DateTime CollectTime { get; private set; }

    /// <summary>
    /// 设备名称
    /// </summary>
    [Description("设备名称")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public string DeviceName { get; set; }
    /// <summary>
    /// 是否在线
    /// </summary>
    [Description("是否在线")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public bool IsOnline
    {
        get
        {
            return isOnline;
        }
        private set
        {
            if (IsOnline != value)
            {
                isOnlineChanged = true;
            }
            else
            {
                isOnlineChanged = false;
            }
            isOnline = value;

        }
    }

    /// <summary>
    /// 上次值
    /// </summary>
    [Description("上次值")]
    [DataTable(Order = 3, IsShow = true, Sortable = false, CellClass = " table-text-truncate ")]
    public object LastSetValue { get; private set; }

    /// <summary>
    /// 原始值
    /// </summary>
    [Description("原始值")]
    [DataTable(Order = 3, IsShow = true, Sortable = false, CellClass = " table-text-truncate ")]
    public object RawValue { get; private set; }
    /// <summary>
    /// 实时值
    /// </summary>
    [Description("实时值")]
    [DataTable(Order = 3, IsShow = true, Sortable = false, CellClass = " table-text-truncate ")]
    public object Value { get => _value; private set => _value = value; }
    /// <summary>
    /// 设置变量值与时间/质量戳
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    /// <param name="isOnline"></param>
    public OperResult SetValue(object value, DateTime dateTime = default, bool isOnline = true)
    {
        try
        {

            IsOnline = isOnline;

            if (!IsOnline)
            {
                RawValue = value;
                Set(value);
                return OperResult.CreateSuccessResult();
            }
            RawValue = value;
            if (!string.IsNullOrEmpty(ReadExpressions))
            {
                object data = null;
                try
                {
                    data = ReadExpressions.GetExpressionsResult(RawValue);
                    Set(data);
                }
                catch (Exception ex)
                {
                    Set(null);
                    return new OperResult(Name + " 转换表达式失败：" + ex.Message);
                }
            }
            else
            {
                Set(value);
            }
            return OperResult.CreateSuccessResult();

        }

        catch (Exception ex)
        {
            return new OperResult(ex);
        }

        void Set(object data)
        {
            DateTime time;
            if (dateTime == default)
            {
                time = DateTimeExtensions.CurrentDateTime;
            }
            else
            {
                time = dateTime;
            }
            CollectTime = time;
            {
                if ((data?.ToString() != _value?.ToString()) || isOnlineChanged)
                {
                    ChangeTime = time;

                    LastSetValue = _value;

                    if (IsOnline)
                    {
                        _value = data;
                    }

                    VariableValueChange?.Invoke(this);
                }
            }

            VariableCollectChange?.Invoke(this);
        }
    }




    #region LoadSourceRead
    /// <summary>
    /// 这个参数值由自动打包方法写入<see cref="IReadWrite.LoadSourceRead{T, T2}(List{T2}, int)"/>
    /// </summary>
    [Description("打包索引")]
    [DataTable(Order = 6, IsShow = true, Sortable = true)]
    public int Index { get; set; }
    /// <summary>
    /// 这个参数值由自动打包方法写入<see cref="IReadWrite.LoadSourceRead{T, T2}(List{T2}, int)"/>
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public IThingsGatewayBitConverter ThingsGatewayBitConverter { get; set; }

    #endregion

    #region 报警
    /// <summary>
    /// 报警值
    /// </summary>
    public string AlarmCode { get; set; }

    /// <summary>
    /// 报警使能
    /// </summary>
    public bool AlarmEnable
    {
        get
        {
            return LAlarmEnable || LLAlarmEnable || HAlarmEnable || HHAlarmEnable || BoolOpenAlarmEnable || BoolCloseAlarmEnable;
        }
    }
    /// <summary>
    /// 报警限值
    /// </summary>
    public string AlarmLimit { get; set; }

    /// <summary>
    /// 报警文本
    /// </summary>
    public string AlarmText { get; set; }

    /// <summary>
    /// 报警时间
    /// </summary>
    public DateTime AlarmTime { get; set; }
    /// <summary>
    /// 报警类型
    /// </summary>
    public AlarmEnum AlarmTypeEnum { get; set; } = AlarmEnum.None;

    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime EventTime { get; set; }
    /// <summary>
    /// 事件类型
    /// </summary>
    public EventEnum EventTypeEnum { get; set; } = EventEnum.None;
    #endregion
}

/// <summary>
/// 变量变化委托
/// </summary>
public delegate void VariableChangeEventHandler(DeviceVariableRunTime collectVariableRunTime);


