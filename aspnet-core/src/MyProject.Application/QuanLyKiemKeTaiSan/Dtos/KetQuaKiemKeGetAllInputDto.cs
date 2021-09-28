namespace MyProject.QuanLyKiemKeTaiSan.Dtos
{
    using Abp.Application.Services.Dto;

    public class KetQuaKiemKeGetAllInputDto  : ISortedResultRequest
    {
        public long Id { get; set; }

        public int Status { get; set; }

        public string Keyword { get; set; }

        public string Sorting { get; set; }

        public bool? IsSearch { get; set; }
    }
}
