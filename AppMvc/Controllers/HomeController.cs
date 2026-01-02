using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AppMvc.Models;
using Services;

namespace AppMvc.Controllers;

public class HomeController : Controller
{
    readonly ILogger<HomeController> _logger;
    readonly IDataSourceActive _dataSourceActive;
    readonly AdminServiceWapi _wapi;
    readonly AdminServiceLocal _local;

    public HomeController(
        ILogger<HomeController> logger,
        IDataSourceActive dataSourceActive,
        AdminServiceWapi wapi,
        AdminServiceLocal local)
    {
        _logger = logger;
        _dataSourceActive = dataSourceActive;
        _wapi = wapi;
        _local = local;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> DataSourceInfo()
    {
        var vm = new DataSourceInfoViewModel
        {
            ActiveDataSource = _dataSourceActive.ActiveDataSource
        };

        try
        {
            var wapiResult = await _wapi.GuestInfoAsync();
            vm.WebApiInfo = wapiResult?.Item;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load datasource info from WebApi");
            vm.WebApiInfo = null;
        }

        try
        {
            var localResult = await _local.GuestInfoAsync();
            vm.LocalInfo = localResult?.Item;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load datasource info from Local in-memory store");
            vm.LocalInfo = null;
        }

        // Ensure the view never crashes on nulls
        vm.WebApiInfo ??= new global::Models.DTO.GstUsrInfoAllDto();
        vm.WebApiInfo.Db ??= new global::Models.DTO.GstUsrInfoDbDto();
        vm.WebApiInfo.Friends ??= new List<global::Models.DTO.GstUsrInfoFriendsDto>();
        vm.WebApiInfo.Pets ??= new List<global::Models.DTO.GstUsrInfoPetsDto>();
        vm.WebApiInfo.Quotes ??= new List<global::Models.DTO.GstUsrInfoQuotesDto>();

        vm.LocalInfo ??= new global::Models.DTO.GstUsrInfoAllDto();
        vm.LocalInfo.Db ??= new global::Models.DTO.GstUsrInfoDbDto();
        vm.LocalInfo.Friends ??= new List<global::Models.DTO.GstUsrInfoFriendsDto>();
        vm.LocalInfo.Pets ??= new List<global::Models.DTO.GstUsrInfoPetsDto>();
        vm.LocalInfo.Quotes ??= new List<global::Models.DTO.GstUsrInfoQuotesDto>();

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

