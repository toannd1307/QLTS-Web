namespace MyProject.Global
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MyProject.Global.Dtos;
    using MyProject.QuanLyKiemKeTaiSan.Dtos;
    using MyProject.QuanLyTaiSan.Dtos;
    using MyProject.Users.Dto;

    public interface ILookupTableAppService
    {
        Task<List<LookupTableDto<string>>> GetAllStringLookupTableAsync();

        Task<List<LookupTableDto<long>>> GetAllLongLookupTableAsync();

        Task<List<LookupTableDto>> GetAllKetQuaKiemKeTaiSan();

        Task<List<LookupTableDto>> GetAllTrangThaiHieuLucAsync();

        Task<List<LookupTableDto>> GetAllTrangThaiDuyetAsync();

        Task<List<LookupTableDto>> GetAllDemoAsync();

        Task<List<LookupTableDto>> GetAllToChucLookupTableAsync();

        Task<List<TreeviewItemDto>> GetAllToChucTreeAsync();

        Task<List<TreeviewItemDto>> GetAllToChucTheoNguoiDungTreeAsync(bool layCha);

        Task<List<int>> GetAllToChucIdTheoNguoiDungListAsync(bool layCha);

        Task<List<TreeviewItemDto>> GetAllLoaiTaiSanTreeAsync();

        Task<List<LookupTableDto>> GetAllNhaCungCapAsync();

        Task<List<LookupTableDto>> GetAllTinhTrangMaSuDungTaiSanAsync();

        Task<List<LookupTableDto>> GetAllTrangThaiTaiSanAsync();

        Task<List<LookupTableDto>> GetAllTrangThaiSuaChuaBaoDuongAsync();

        Task<List<LookupTableDto>> GetAllTrangThaiKiemKeAsync();

        Task DeleteTaiSanAsync(int taiSanId);

        Task<List<UserForViewDto>> GetAllNguoiDungAsync(UsersForGetFliterViewDto input);

        Task<bool> ChangePassword(ChangePasswordDto input);

        Task<string> GetTrangThaiTaiSanTruocAsync(int taiSanId);

        Task<List<LookupTableDto<long>>> GetAllUserNameByIdAsync(List<long> userIdList);

        Task<List<LookupTableDto>> GetAllTrangThaiTaiSanTimKiemAsync();

        Task<GetEPCCodeDto> GetEPCCodeAsync(string rfidCode);

        Task<List<LookupTableDto<long>>> GetAllDotKiemKeNguoiDungAsync();

        Task<ThongTinKiemKeForMobileDto> GetThongTinDotKiemKeAsync(int dotKiemKeId);

        Task<List<LookupTableDto>> GetAllPhanLoaiTaiSanTrongHeThongAsync();

        Task<List<LookupTableDto>> GetAllChieuTaiSanDiChuyenAsync();

        Task<List<LookupTableDto>> GetAllHoatDongAsync();

        Task<List<FlatTreeSelectDto>> GetAllToChucCuaNguoiDangNhapTreeAsync();

        Task<List<FlatTreeSelectDto>> GetAllToChucAsync();

        Task<List<LookupTableDto>> GetAllBaoCaoLookupTableAsync();

        Task<List<LookupTableDto>> GetAllLapLaiLookupTableAsync();

        Task<List<LookupTableDto>> GetAllThangLookupTableAsync();

        Task<List<LookupTableDto>> GetAllQuyLookupTableAsync();

        Task<List<LookupTableDto>> GetAllThuLookupTableAsync();
    }
}
