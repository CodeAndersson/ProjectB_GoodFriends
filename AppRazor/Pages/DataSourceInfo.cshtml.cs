using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;
namespace AppRazor.Pages
{
	public class DataSourceInfoModel : PageModel
    {
        public Models.DTO.GstUsrInfoAllDto WebApiInfo { get; set; }
        public Models.DTO.GstUsrInfoAllDto LocalInfo { get; set; }
        public Services.DataSource ActiveDataSource { get; private set; }

        readonly ILogger<DataSourceInfoModel> _logger;
        readonly Services.IDataSourceActive _dataSourceActive;
        readonly Services.AdminServiceWapi _wapi;
        readonly Services.AdminServiceLocal _local;

        public DataSourceInfoModel(
            ILogger<DataSourceInfoModel> logger,
            Services.IDataSourceActive dataSourceActive,
            Services.AdminServiceWapi wapi,
            Services.AdminServiceLocal local)
        {
            _logger = logger;
            _dataSourceActive = dataSourceActive;
            _wapi = wapi;
            _local = local;
        }

        public async Task<IActionResult> OnGet()
        {
            ActiveDataSource = _dataSourceActive.ActiveDataSource;

            try
            {
                var wapiResult = await _wapi.GuestInfoAsync();
                WebApiInfo = wapiResult?.Item;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load datasource info from WebApi");
                WebApiInfo = null;
            }

            try
            {
                var localResult = await _local.GuestInfoAsync();
                LocalInfo = localResult?.Item;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load datasource info from Local in-memory store");
                LocalInfo = null;
            }

            // Ensure the view never crashes on nulls
            WebApiInfo ??= new Models.DTO.GstUsrInfoAllDto();
            WebApiInfo.Db ??= new Models.DTO.GstUsrInfoDbDto();
            WebApiInfo.Friends ??= new List<Models.DTO.GstUsrInfoFriendsDto>();
            WebApiInfo.Pets ??= new List<Models.DTO.GstUsrInfoPetsDto>();
            WebApiInfo.Quotes ??= new List<Models.DTO.GstUsrInfoQuotesDto>();

            LocalInfo ??= new Models.DTO.GstUsrInfoAllDto();
            LocalInfo.Db ??= new Models.DTO.GstUsrInfoDbDto();
            LocalInfo.Friends ??= new List<Models.DTO.GstUsrInfoFriendsDto>();
            LocalInfo.Pets ??= new List<Models.DTO.GstUsrInfoPetsDto>();
            LocalInfo.Quotes ??= new List<Models.DTO.GstUsrInfoQuotesDto>();

            return Page();
        }
    }
}
