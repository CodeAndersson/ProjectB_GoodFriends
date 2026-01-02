using Microsoft.AspNetCore.Mvc;

using Services;

namespace AppMvc.Models
{
	public class SelectDataSourceViewModel
    {
        //ModelBinding for Selections
        [BindProperty]
        public DataSource SelectedDataSource { get; set; }
    }
}

