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

using Masa.Blazor;
using Masa.Blazor.Presets;

using SqlSugar;



namespace ThingsGateway.Admin.Blazor;
/// <summary>
/// 角色页面
/// </summary>
public partial class Role
{
    private readonly RolePageInput search = new();
    private IAppDataTable _datatable;
    private List<UserSelectorOutput> AllUsers;
    long ChoiceRoleId;
    bool IsShowResuorces;
    bool IsShowUsers;
    List<RoleGrantResourceMenu> ResTreeSelectors = new();
    List<RelationRoleResuorce> RoleHasResuorces = new();
    private List<UserSelectorOutput> UsersChoice;

    [CascadingParameter]
    MainLayout MainLayout { get; set; }



    private string SearchKey { get; set; }

    private Task AddCallAsync(RoleAddInput input)
    {
        return App.GetService<RoleService>().AddAsync(input);
    }
    private async Task DeleteCallAsync(IEnumerable<SysRole> sysRoles)
    {
        await App.GetService<RoleService>().DeleteAsync(sysRoles.Select(a => a.Id).ToArray());
        await MainLayout.StateHasChangedAsync();
    }

    private async Task EditCallAsync(RoleEditInput input)
    {
        await App.GetService<RoleService>().EditAsync(input);
        await MainLayout.StateHasChangedAsync();
    }
    private async Task OnRoleHasResuorcesSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            GrantResourceInput userGrantRoleInput = new();
            var data = new List<SysResource>();
            userGrantRoleInput.Id = ChoiceRoleId;
            userGrantRoleInput.GrantInfoList = RoleHasResuorces;
            await App.GetService<RoleService>().GrantResourceAsync(userGrantRoleInput);
            IsShowResuorces = false;
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        await MainLayout.StateHasChangedAsync();
    }
    private async Task OnUsersSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            GrantUserInput userGrantRoleInput = new();
            userGrantRoleInput.Id = ChoiceRoleId;
            userGrantRoleInput.GrantInfoList = UsersChoice.Select(it => it.Id).ToList();
            await App.GetService<RoleService>().GrantUserAsync(userGrantRoleInput);
            IsShowUsers = false;
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        await MainLayout.StateHasChangedAsync();
    }

    private Task<ISqlSugarPagedList<SysRole>> QueryCallAsync(RolePageInput input)
    {
        return App.GetService<RoleService>().PageAsync(input);
    }

    private async Task ResuorceInitAsync()
    {
        ResTreeSelectors = (await App.GetService<ResourceService>().GetRoleGrantResourceMenusAsync());
        RoleHasResuorces = (await App.GetService<RoleService>().OwnResourceAsync(ChoiceRoleId))?.GrantInfoList;
    }

    private async Task<List<UserSelectorOutput>> UserInitAsync()
    {
        AllUsers = await App.GetService<SysUserService>().UserSelectorAsync(SearchKey);
        var data = await App.GetService<RoleService>().OwnUserAsync(ChoiceRoleId);
        UsersChoice = AllUsers.Where(a => data.Contains(a.Id)).ToList();
        return AllUsers;
    }
}