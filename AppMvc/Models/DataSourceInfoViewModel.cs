using Models.DTO;
using Services;

namespace AppMvc.Models
{
	public class DataSourceInfoViewModel
    {
        public GstUsrInfoAllDto WebApiInfo { get; set; }
        public GstUsrInfoAllDto LocalInfo { get; set; }
        public MusicDataSource ActiveDataSource { get; set; }
    }
}