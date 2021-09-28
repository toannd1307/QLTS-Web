namespace MyProject.Global
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Abp.Auditing;
    using Abp.Authorization;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization;
    using MyProject.Authorization.Accounts;
    using MyProject.Authorization.Users;
    using MyProject.DanhMuc.Demo;
    using MyProject.Global.Dtos;
    using MyProject.QuanLyKiemKeTaiSan.Dtos;
    using MyProject.QuanLyTaiSan.Dtos;
    using MyProject.QuanLyTaiSan.QuanLyToanBoTaiSan.Dtos;
    using MyProject.Shared;
    using MyProject.Users.Dto;

    [AbpAuthorize]
    [DisableAuditing]
    public class LookupTableAppService : MyProjectAppServiceBase, ILookupTableAppService
    {
        private readonly IRepository<Demo> demoRepository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IRepository<LoaiTaiSan> loaiTaiSan;
        private readonly IRepository<NhaCungCap> nhaCungCap;
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IRepository<PhieuTaiSan, long> phieuTaiSanRepository;
        private readonly IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRepository;
        private readonly IRepository<User, long> userRepository;
        private readonly IRepository<ViTriDiaLy> viTriDiaLyRepository;
        private readonly LogInManager logInManager;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IRepository<KiemKeTaiSan, long> kiemKeRepository;
        private readonly IRepository<KiemKe_DoiKiemKe, long> doiKiemKeRepository;
        private readonly IRepository<LinhVucKinhDoanh> linhVucKinhDoanhRepository;

        public LookupTableAppService(
             IRepository<KiemKeTaiSan, long> kiemKeRepository,
             IRepository<KiemKe_DoiKiemKe, long> doiKiemKeRepository,
             IRepository<ViTriDiaLy> viTriDiaLyRepository,
             IRepository<User, long> userRepository,
             LogInManager logInManager,
             IPasswordHasher<User> passwordHasher,
             IRepository<Demo> demoRepository,
             IRepository<ToChuc> toChucRepository,
             IRepository<LoaiTaiSan> loaiTaiSan,
             IRepository<NhaCungCap> nhaCungCap,
             IRepository<TaiSan> taiSanRepository,
             IRepository<PhieuTaiSan, long> phieuTaiSanRepository,
             IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRepository,
             IRepository<LinhVucKinhDoanh> linhVucKinhDoanhRepository)
        {
            this.kiemKeRepository = kiemKeRepository;
            this.doiKiemKeRepository = doiKiemKeRepository;
            this.viTriDiaLyRepository = viTriDiaLyRepository;
            this.userRepository = userRepository;
            this.passwordHasher = passwordHasher;
            this.logInManager = logInManager;
            this.demoRepository = demoRepository;
            this.toChucRepository = toChucRepository;
            this.loaiTaiSan = loaiTaiSan;
            this.nhaCungCap = nhaCungCap;
            this.taiSanRepository = taiSanRepository;
            this.phieuTaiSanRepository = phieuTaiSanRepository;
            this.phieuTaiSanChiTietRepository = phieuTaiSanChiTietRepository;
            this.linhVucKinhDoanhRepository = linhVucKinhDoanhRepository;
        }

        public async Task<List<LookupTableDto<string>>> GetAllStringLookupTableAsync()
        {
            return await Task.FromResult(new List<LookupTableDto<string>>());
        }

        public async Task<List<LookupTableDto<long>>> GetAllLongLookupTableAsync()
        {
            return await Task.FromResult(new List<LookupTableDto<long>>());
        }

        public async Task<List<LookupTableDto>> GetAllTrangThaiHieuLucAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.TrangThaiHieuLucSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllKetQuaKiemKeTaiSan()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.KetQuaKiemKeTaiSanSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllTrangThaiDuyetAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.TrangThaiDuyetSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllDemoAsync()
        {
            var result = this.demoRepository.GetAll().Select(e => new LookupTableDto() { Id = e.Id, DisplayName = e.Ma }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllToChucLookupTableAsync()
        {
            var result = this.toChucRepository.GetAll().Select(e => new LookupTableDto() { Id = e.Id, DisplayName = e.TenToChuc }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllLinhVucKinhDoanhAsync()
        {
            var result = this.linhVucKinhDoanhRepository.GetAll().Select(e => new LookupTableDto() { Id = e.Id, DisplayName = e.TenLinhVuc }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<TreeviewItemDto>> GetAllToChucTreeAsync()
        {
            return await GlobalFunction.GetAllToChucTreeAsync(this.toChucRepository);
        }

        public async Task<List<TreeviewItemDto>> GetAllToChucTheoNguoiDungTreeAsync(bool layCha)
        {
            var toChucId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
            return this.PermissionChecker.IsGranted(PermissionNames.Pages_QuanLyTaiSanTong) ?
                await GlobalFunction.GetAllToChucTreeAsync(this.toChucRepository) :
                await GlobalFunction.GetAllToChucTheoNguoiDungTreeAsync(this.toChucRepository, (int)toChucId, layCha);
        }

        public async Task<List<int>> GetAllToChucIdTheoNguoiDungListAsync(bool layCha)
        {
            var toChucId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
            return await GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRepository, (int)toChucId, layCha);
        }

        public async Task<List<TreeviewItemDto>> GetAllLoaiTaiSanTreeAsync()
        {
            return await GlobalFunction.GetAllLoaiTaiSanTreeAsync(this.loaiTaiSan);
        }

        public async Task<List<LookupTableDto>> GetAllNhaCungCapAsync()
        {
            var query = await this.nhaCungCap.GetAllListAsync();
            return query.Select(e => new LookupTableDto
            {
                Id = e.Id,
                DisplayName = e.TenNhaCungCap,
            }).ToList();
        }

        public async Task<List<LookupTableDto>> GetAllTinhTrangMaSuDungTaiSanAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.TinhTrangMaSuDungTaiSanSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllTrangThaiTaiSanAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.TrangThaiTaiSanSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllTrangThaiTaiSanTimKiemAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.TrangThaiTaiSanTimKiemSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllTrangThaiSuaChuaBaoDuongAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.TrangThaiSuaChuaBaoDuongSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllTrangThaiKiemKeAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.TrangThaiKiemKeSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task DeleteTaiSanAsync(int taiSanId)
        {
            List<int> taiSanIdList = new List<int> { taiSanId };
            await GlobalFunction.XoaTaiSan(this.taiSanRepository, this.phieuTaiSanRepository, this.phieuTaiSanChiTietRepository, taiSanIdList);
        }

        public async Task DeleteTaiSanListAsync(List<int> taiSanId)
        {
            await GlobalFunction.XoaTaiSan(this.taiSanRepository, this.phieuTaiSanRepository, this.phieuTaiSanChiTietRepository, taiSanId);
        }

        public async Task<List<UserForViewDto>> GetAllNguoiDungAsync(UsersForGetFliterViewDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var keyword = GlobalFunction.RegexFormat(input.Keyword);
            var filter = this.userRepository.GetAll()
                                .WhereIf(!string.IsNullOrEmpty(input.Keyword), e => e.UserName.Contains(keyword)
                                || e.Name.Contains(keyword)
                                || e.EmailAddress.Contains(keyword)
                                || e.ChucVu.Contains(keyword))
                                .WhereIf(input.ToChucId != null, e => e.ToChucId.Equals(input.ToChucId));

            var query = from user in filter
                        from toChuc in this.toChucRepository.GetAll().Where(e => e.Id.Equals(user.ToChucId))
                        select new UserForViewDto
                        {
                            User = this.ObjectMapper.Map<UserDto>(user),
                            TenToChuc = toChuc.TenToChuc,
                        };
            var result = await query.ToListAsync();
            return result;
        }

        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            if (this.AbpSession.UserId == null)
            {
                throw new UserFriendlyException(StringResources.UserNotLogin);
            }

            long userId = this.AbpSession.UserId.Value;
            var user = await this.UserManager.GetUserByIdAsync(userId);
            var loginAsync = await this.logInManager.LoginAsync(user.UserName, input.CurrentPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException(StringResources.PasswordIncorect);
            }

            user.Password = this.passwordHasher.HashPassword(user, input.NewPassword);
            this.CurrentUnitOfWork.SaveChanges();
            return true;
        }

        public async Task<string> GetTrangThaiTaiSanTruocAsync(int taiSanId)
        {
            var taiSanChiTiet = (from ptsct in this.phieuTaiSanChiTietRepository.GetAll().Where(e => e.TaiSanId == taiSanId)
                                 from pts in this.phieuTaiSanRepository.GetAll().Where(e => e.Id == ptsct.PhieuTaiSanId)
                                 select new
                                 {
                                     ptsct.Id,
                                     pts.PhanLoaiId,
                                 }).OrderByDescending(e => e.Id).Take(2);

            string trangThaiTruoc = taiSanChiTiet.Count() == 1 ? GlobalModel.TrangThaiTaiSanHienThiSorted[0] : GlobalModel.TrangThaiTaiSanHienThiSorted.ContainsKey((int)taiSanChiTiet.Last().PhanLoaiId) ? GlobalModel.TrangThaiTaiSanHienThiSorted[(int)taiSanChiTiet.Last().PhanLoaiId] : string.Empty;

            return await Task.FromResult(trangThaiTruoc);
        }

        public async Task<List<LookupTableDto<long>>> GetAllUserNameByIdAsync(List<long> userIdList)
        {
            var result = this.userRepository.GetAll().Where(e => userIdList.Contains(e.Id))
                .Select(e => new LookupTableDto<long>()
                {
                    Id = e.Id,
                    DisplayName = e.Name,
                }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllViTriDiaLyLookupTableAsync()
        {
            var result = await this.viTriDiaLyRepository.GetAll()
               .Select(e => new LookupTableDto()
               {
                   Id = e.Id,
                   DisplayName = e.TenViTri,
               }).ToListAsync();

            return result;
        }

        public async Task<GetEPCCodeDto> GetEPCCodeAsync(string rfidCode)
        {
            rfidCode = GlobalFunction.RegexFormat(rfidCode);
            var fpcCode = this.taiSanRepository.GetAll().Where(e => e.RFIDCode == rfidCode).Select(e => new GetEPCCodeDto
            {
                TaiSanId = e.Id,
                TenTaiSan = e.TenTaiSan,
                EPCCode = e.RFIDCode,
                TrangThaiId = (int)e.TrangThaiId,
            }).FirstOrDefault();
            return await Task.FromResult(fpcCode);
        }

        [AbpAllowAnonymous]
        public async Task<GetForViewDto> GetAssetByQRCodeAsync(string qrCode)
        {
            var fpcCode = this.taiSanRepository.GetAll().Where(e => e.QRCode == qrCode).Select(e => new GetForViewDto
            {
                Id = e.Id,
                TenTS = e.TenTaiSan,
                MaQRCode = e.QRCode,
                NgayMua = e.NgayMua,
                NgayBaoHanh = e.NgayHetHanBaoHanh,
                HanSD = e.NgayHetHanSuDung,
                NguyenGia = e.NguyenGia.ToString(),
                SerialNumber = e.SerialNumber,
                HangSanXuat = e.HangSanXuat,
                GhiChu = e.GhiChu,
                LoaiTaiSan = this.loaiTaiSan.GetAll().Where(l => l.Id == e.LoaiTaiSanId).Select(s => s.Ten).SingleOrDefault(),
                ProductNumber = e.ProductNumber,
            }).FirstOrDefault();
            return await Task.FromResult(fpcCode);
        }

        public async Task<List<LookupTableDto<long>>> GetAllDotKiemKeNguoiDungAsync()
        {
            var query = await (from doiKiemKe in this.doiKiemKeRepository.GetAll().Where(e => e.NguoiKiemKeId == this.AbpSession.UserId)
                               from kiemKe in this.kiemKeRepository.GetAll().Where(e => e.Id == doiKiemKe.KiemKeTaiSanId && e.TrangThaiId != (int)GlobalConst.TrangThaiKiemKeConst.DaKetThuc)
                               select new LookupTableDto<long>
                               {
                                   Id = kiemKe.Id,
                                   DisplayName = kiemKe.TenKiemKe,
                               }).ToListAsync();
            return query;
        }

        public async Task<ThongTinKiemKeForMobileDto> GetThongTinDotKiemKeAsync(int dotKiemKeId)
        {
            var query = await (from kiemKe in this.kiemKeRepository.GetAll().Where(e => e.Id == dotKiemKeId)
                               from phongBan in this.toChucRepository.GetAll().Where(e => e.Id == kiemKe.BoPhanDuocKiemKeId)
                               from doiKiemKe in this.doiKiemKeRepository.GetAll().Where(e => e.KiemKeTaiSanId == kiemKe.Id)
                               from user in this.userRepository.GetAll().Where(e => e.Id == doiKiemKe.NguoiKiemKeId)
                               select new
                               {
                                   user.Name,
                                   kiemKe.Id,
                                   kiemKe.TrangThaiId,
                                   kiemKe.ThoiGianBatDauDuKien,
                                   phongBan.TenToChuc,
                                   phongBan.ViTriDiaLyId,
                               }).ToListAsync();

            var result = query.GroupBy(e => new
            {
                e.Id,
                e.TrangThaiId,
                e.ThoiGianBatDauDuKien,
                e.TenToChuc,
                e.ViTriDiaLyId,
            }).Select(e => new ThongTinKiemKeForMobileDto
            {
                KiemKeId = e.Key.Id,
                NgayKiemKe = (DateTime)e.Key.ThoiGianBatDauDuKien,
                PhongBanKiemKe = e.Key.TenToChuc,
                ThanhVienKiemKe = e.Select(e => e.Name).ToList(),
                TrangThaiKiemKe = (int)e.Key.TrangThaiId,
                ViTriDiaLyId = (int)e.Key.ViTriDiaLyId,
            }).FirstOrDefault();
            return result;
        }

        public async Task<List<LookupTableDto>> GetAllPhanLoaiTaiSanTrongHeThongAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.PhanLoaiTaiSanTrongHeThongSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllChieuTaiSanDiChuyenAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.ChieuTaiSanDiChuyenSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllHoatDongAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.LoaiCanhBaoSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<FlatTreeSelectDto>> GetAllToChucCuaNguoiDangNhapTreeAsync()
        {
            var toChucId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
            var listToChucNguoiDung = await GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRepository, (int)toChucId, true);
            var result = this.toChucRepository.GetAll().Where(e => listToChucNguoiDung.Contains(e.Id)).Select(e => new FlatTreeSelectDto
            {
                Id = e.Id,
                ParentId = e.TrucThuocToChucId,
                DisplayName = e.TenToChuc,
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<FlatTreeSelectDto>> GetAllToChucAsync()
        {
            var result = this.toChucRepository.GetAllList().Select(e => new FlatTreeSelectDto
            {
                Id = e.Id,
                ParentId = e.TrucThuocToChucId,
                DisplayName = e.TenToChuc,
            }).ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<FlatTreeSelectDto>> GetAllToChucTheoNguoiDungAsync()
        {
            var result = new List<FlatTreeSelectDto>();
            var quyenTong = this.PermissionChecker.IsGranted(PermissionNames.Pages_QuanLyTaiSanTong);
            if (quyenTong)
            {
                result = this.toChucRepository.GetAllList().Select(e => new FlatTreeSelectDto
                {
                    Id = e.Id,
                    ParentId = e.TrucThuocToChucId,
                    DisplayName = e.TenToChuc,
                }).ToList();
            }
            else
            {
                var toChucId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
                var listToChucNguoiDung = await GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRepository, (int)toChucId, true);
                result = this.toChucRepository.GetAll().Where(e => listToChucNguoiDung.Contains(e.Id)).Select(e => new FlatTreeSelectDto
                {
                    Id = e.Id,
                    ParentId = e.TrucThuocToChucId,
                    DisplayName = e.TenToChuc,
                }).ToList();
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllBaoCaoLookupTableAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.TenBaoCaoSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllLapLaiLookupTableAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.LapLaiSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<TreeviewItemDto>> GetAllToChucTheoDangNhapTreeAsync()
        {
            var toChucId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
            return await GlobalFunction.GetAllToChucTheoNguoiDungTreeAsync(this.toChucRepository, (int)toChucId, true);
        }

        public async Task<List<LookupTableDto>> GetAllThangLookupTableAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();

            for (int i = 1; i < 32; i++)
            {
                result.Add(new LookupTableDto { Id = i, DisplayName = "Ngày " + i });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllQuyLookupTableAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.QuySorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllThuLookupTableAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.ThuSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }

        public async Task<List<LookupTableDto>> GetAllNguoiDungTheoPBLookupTableAsync(int? phongBan)
        {
            var query = this.userRepository.GetAll().Where(w => w.ToChucId == phongBan).Select(s => new LookupTableDto
            {
                Id = (int)s.Id,
                DisplayName = s.Name,
            }).ToList();
            return await Task.FromResult(query);
        }

        public async Task<List<LookupTableDto>> GetAllNguonKinhPhiAsync()
        {
            List<LookupTableDto> result = new List<LookupTableDto>();
            foreach (var item in GlobalModel.NguonKinhPhiSorted)
            {
                result.Add(new LookupTableDto { Id = item.Key, DisplayName = item.Value });
            }

            return await Task.FromResult(result);
        }
    }
}