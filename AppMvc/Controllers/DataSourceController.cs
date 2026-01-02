using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using AppMvc.Models;
using Services;
using Microsoft.AspNetCore.Authorization;

namespace AppMvc.Controllers;
[Authorize]
public class DataSourceController : Controller
{
    private readonly ILogger<DataSourceController> _logger;
    readonly IDataSourceActive _service = null;

    //Inject services just like in WebApi
    public DataSourceController(IDataSourceActive service, ILogger<DataSourceController> logger)
    {
        _service = service;
        _logger = logger;
    }

    public IActionResult Index()
    {
        //Create the viewModel
        var vm = new SelectDataSourceViewModel() { SelectedDataSource = _service.ActiveDataSource };

        //Render the View
        return View("SelectDataSource", vm);
    }


    public ActionResult SelectDataSource(SelectDataSourceViewModel vm)
    {
        _service.ActiveDataSource = vm.SelectedDataSource;

        return View("SelectDataSource", vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

