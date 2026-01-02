using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services;

namespace AppRazor.Pages
{
    public class SeedModel : PageModel
    {
        //Just like for WebApi
        readonly IAdminService _admin_service = null;
        readonly IMusicServiceActive _dataSourceActive = null;
        readonly ILogger<SeedModel> _logger = null;

        public int NrOfFriends { get; private set; }
        public Services.MusicDataSource ActiveDataSource => _dataSourceActive.ActiveDataSource;

        [BindProperty]
        [Required (ErrorMessage = "You must enter nr of items to seed")]
        public int NrOfItemsToSeed { get; set; } = 100;

        [BindProperty]
        public bool RemoveSeeds { get; set; } = true;

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCountsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            await LoadCountsAsync();

            if (ModelState.IsValid)
            {
                if (ActiveDataSource == Services.MusicDataSource.WebApi)
                {
                    ModelState.AddModelError(string.Empty, "Friends WebApi does not support seeding from this app. Switch datasource to SQLDatabase to seed local in-memory data.");
                    return Page();
                }

                if (RemoveSeeds)
                {
                    await _admin_service.RemoveSeedAsync(true);
                    await _admin_service.RemoveSeedAsync(false);
                }

                await _admin_service.SeedAsync(NrOfItemsToSeed);
                return Redirect("~/DataSourceInfo");
            }
            return Page();
        }

        private async Task LoadCountsAsync()
        {
            try
            {
                var info = await _admin_service.GuestInfoAsync();
                NrOfFriends = (info?.Item?.Db?.NrSeededFriends ?? 0) + (info?.Item?.Db?.NrUnseededFriends ?? 0);
            }
            catch
            {
                NrOfFriends = 0;
            }
        }

        //Inject services just like in WebApi
        public SeedModel(IAdminService admin_service, IMusicServiceActive dataSourceActive, ILogger<SeedModel> logger)
        {
            _admin_service = admin_service;
            _dataSourceActive = dataSourceActive;
            _logger = logger;
        }
    }
}
