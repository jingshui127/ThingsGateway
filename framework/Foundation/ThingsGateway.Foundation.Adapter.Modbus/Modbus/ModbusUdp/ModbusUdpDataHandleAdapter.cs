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

using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Adapter.Modbus;
/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusUdpDataHandleAdapter : ReadWriteDevicesUdpDataHandleAdapter<ModbusTcpMessage>
{
    private readonly EasyIncrementCount easyIncrementCount = new(ushort.MaxValue);

    /// <summary>
    /// 检测事务标识符
    /// </summary>
    public bool IsCheckMessageId
    {
        get
        {
            return Request?.IsCheckMessageId ?? false;
        }
        set
        {
            Request.IsCheckMessageId = value;
        }
    }

    /// <inheritdoc/>
    public override byte[] PackCommand(byte[] command)
    {
        return ModbusHelper.AddModbusTcpHead(command, (ushort)easyIncrementCount.GetCurrentValue());
    }

    /// <inheritdoc/>
    protected override ModbusTcpMessage GetInstance()
    {
        return new ModbusTcpMessage();
    }

    /// <inheritdoc/>
    protected override OperResult<byte[]> UnpackResponse(byte[] send, byte[] response)
    {
        var result = ModbusHelper.GetModbusData(send.RemoveBegin(6), response.RemoveBegin(6));
        return result;
    }
}
