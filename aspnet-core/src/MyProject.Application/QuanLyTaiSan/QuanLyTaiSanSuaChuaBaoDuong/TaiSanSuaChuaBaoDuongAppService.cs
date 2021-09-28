namespace MyProject.TaiSanSuaChuaBaoDuong
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization;
    using MyProject.Authorization.Users;
    using MyProject.Global;
    using MyProject.Global.Dtos;
    using MyProject.QuanLyTaiSan.Dtos;

    public class TaiSanSuaChuaBaoDuongAppService : MyProjectAppServiceBase, ITaiSanSuaChuaBaoDuongAppService
    {
        private readonly IRepository<ViewTaiSanSuaChuaBaoDuong> vTaiSanSuaChuaBaoDuongRespository;
        private readonly IRepository<ViewTaiSan> vTaiSanRespository;
        private readonly IRepository<ToChuc> toChucRespository;
        private readonly IRepository<TaiSan> taiSanRespository;
        private readonly IRepository<PhieuTaiSan, long> phieuTaiSanRespository;
        private readonly IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRespository;
        private readonly ILookupTableAppService lookupTableAppService;
        private readonly IRepository<User, long> userRepository;
        private readonly IRepository<CanhBao> canhBaoRepository;

        public TaiSanSuaChuaBaoDuongAppService(
            IRepository<ViewTaiSan> vTaiSanRespository,
            IRepository<ViewTaiSanSuaChuaBaoDuong> vTaiSanSuaChuaBaoDuongRespository,
            IRepository<ToChuc> toChucRespository,
            IRepository<TaiSan> taiSanRespository,
            IRepository<PhieuTaiSan, long> phieuTaiSanRespository,
            IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRespository,
            ILookupTableAppService lookupTableAppService,
            IRepository<User, long> userRepository,
            IRepository<CanhBao> canhBaoRepository)
        {
            this.vTaiSanRespository = vTaiSanRespository;
            this.vTaiSanSuaChuaBaoDuongRespository = vTaiSanSuaChuaBaoDuongRespository;
            this.toChucRespository = toChucRespository;
            this.taiSanRespository = taiSanRespository;
            this.phieuTaiSanRespository = phieuTaiSanRespository;
            this.phieuTaiSanChiTietRespository = phieuTaiSanChiTietRespository;
            this.lookupTableAppService = lookupTableAppService;
            this.userRepository = userRepository;
            this.canhBaoRepository = canhBaoRepository;
        }

        public async Task<PagedResultDto<ViewTaiSanSuaChuaBaoDuong>> GetAllTaiSanSuaChuaBaoDuongAsync(TaiSanSuaChuaBaoDuongGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            if (input.PhongBanqQL == null || input.PhongBanqQL.Count == 0)
            {
                input.PhongBanqQL = new List<int> { -1 };
            }
            #endregion

            var checkPermission = await this.PermissionChecker.IsGrantedAsync(PermissionNames.Pages_QuanLyTaiSanTong);
            var phongBanCuaNguoiDung = GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRespository, (int)this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId).Result;

            if (!checkPermission)
            {
                var items = this.vTaiSanSuaChuaBaoDuongRespository.GetAll().Where(w => phongBanCuaNguoiDung.Contains((int)w.PhongBanQuanLyId)).OrderByDescending(o => o.LastModificationTime).AsQueryable();
                var query = this.vTaiSanSuaChuaBaoDuongRespository.GetAll().Where(w => 1 == 0);

                if (!string.IsNullOrEmpty(input.TenTaiSan))
                {
                    input.TenTaiSan = input.TenTaiSan.Trim();

                    // Xử lí filter theo ngày
                    DateTime? ngayBatDau = null;
                    if (GlobalFunction.ConvertStringToDateTime(input.TenTaiSan) != null)
                    {
                        ngayBatDau = GlobalFunction.ConvertStringToDateTime(input.TenTaiSan).Value.Date;
                        query = items.Where(w => w.ThoiGianBatDau.Value.Date == ngayBatDau);
                    }

                    // Xử lí filter theo tên tài sản, địa chỉ bảo dưỡng
                    query = query.Union(items.Where(w => w.TenTaiSan.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                    || w.DiaChiSuaChuaBaoDuong.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                    || w.EPCCode.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                    || w.NguyenGiaStr.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))));
                }
                else
                {
                    query = items;
                }

                query = query.Where(w => input.PhongBanqQL.Contains((int)w.PhongBanQuanLyId));

                if (input.LoaiTaiSan != null)
                {
                    query = query.Where(w => w.LoaiTaiSanId == input.LoaiTaiSan);
                }

                if (!string.IsNullOrEmpty(input.NhaCungCap))
                {
                    query = query.Where(w => w.NhaCungCap.Contains(GlobalFunction.RegexFormat(input.NhaCungCap)));
                }

                if (input.HinhThuc.HasValue)
                {
                    query = query.Where(w => w.HinhThuc == input.HinhThuc);
                }

                if (input.TrangThai.HasValue)
                {
                    query = query.Where(w => w.TrangThai == input.TrangThai);
                }

                int totalCount = await query.CountAsync();

                if (!string.IsNullOrEmpty(input.Sorting))
                {
                    query = query.OrderBy(input.Sorting);
                }

                var output = await query.PageBy(input)
                                        .ToListAsync();

                return new PagedResultDto<ViewTaiSanSuaChuaBaoDuong>(totalCount, output);
            }
            else
            {
                var items = this.vTaiSanSuaChuaBaoDuongRespository.GetAll().OrderByDescending(o => o.LastModificationTime).AsQueryable();
                var query = this.vTaiSanSuaChuaBaoDuongRespository.GetAll().Where(w => 1 == 0);

                if (!string.IsNullOrEmpty(input.TenTaiSan))
                {
                    input.TenTaiSan = input.TenTaiSan.Trim();

                    // Xử lí filter theo ngày
                    DateTime? ngayBatDau = null;
                    if (GlobalFunction.ConvertStringToDateTime(input.TenTaiSan) != null)
                    {
                        ngayBatDau = GlobalFunction.ConvertStringToDateTime(input.TenTaiSan).Value.Date;
                        query = items.Where(w => w.ThoiGianBatDau.Value.Date == ngayBatDau);
                    }

                    // Xử lí filter theo tên tài sản, địa chỉ bảo dưỡng
                    query = query.Union(items.Where(w => w.TenTaiSan.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                   || w.DiaChiSuaChuaBaoDuong.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                   || w.EPCCode.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                   || w.NguyenGiaStr.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))));
                }
                else
                {
                    query = items;
                }

                query = query.Where(w => input.PhongBanqQL.Contains((int)w.PhongBanQuanLyId));

                if (input.LoaiTaiSan != null)
                {
                    query = query.Where(w => w.LoaiTaiSanId == input.LoaiTaiSan);
                }

                if (!string.IsNullOrEmpty(input.NhaCungCap))
                {
                    query = query.Where(w => w.NhaCungCap.Contains(GlobalFunction.RegexFormat(input.NhaCungCap)));
                }

                if (input.HinhThuc.HasValue)
                {
                    query = query.Where(w => w.HinhThuc == input.HinhThuc);
                }

                if (input.TrangThai.HasValue)
                {
                    query = query.Where(w => w.TrangThai == input.TrangThai);
                }

                int totalCount = await query.CountAsync();

                if (!string.IsNullOrEmpty(input.Sorting))
                {
                    query = query.OrderBy(input.Sorting);
                }

                var output = await query.PageBy(input)
                                        .ToListAsync();

                return new PagedResultDto<ViewTaiSanSuaChuaBaoDuong>(totalCount, output);
            }
        }

        public async Task<PagedResultDto<ViewTaiSan>> GetAllTaiSanAsync(TaiSanGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            if (input.PhongBanqQL == null || input.PhongBanqQL.Count == 0)
            {
                input.PhongBanqQL = new List<int> { -1 };
            }
            #endregion

            var checkPermission = await this.PermissionChecker.IsGrantedAsync(PermissionNames.Pages_QuanLyTaiSanTong);
            var phongBanCuaNguoiDung = GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRespository, (int)this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId).Result;

            if (!checkPermission)
            {
                var query = this.vTaiSanRespository.GetAll().Where(w => phongBanCuaNguoiDung.Contains((int)w.PhongBanQuanLyId)).Where(w =>
            (w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.KhoiTao ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.DieuChuyen ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.CapPhat ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.ThuHoi ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanDangSuDung ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanHong)).OrderByDescending(o => o.LastModificationTime).AsQueryable();

                if (!string.IsNullOrEmpty(input.TenTaiSan))
                {
                    input.TenTaiSan = input.TenTaiSan.Trim();
                    query = query.Where(w => w.TenTaiSan.Contains(GlobalFunction.RegexFormat(input.TenTaiSan)) || w.SerialNumber.Contains(GlobalFunction.RegexFormat(input.TenTaiSan)));
                }

                query = query.Where(w => input.PhongBanqQL.Contains((int)w.PhongBanQuanLyId));

                if (input.LoaiTaiSan != null)
                {
                    query = query.Where(w => w.LoaiTaiSanId == input.LoaiTaiSan);
                }

                int totalCount = await query.CountAsync();
                if (totalCount != 0)
                {
                    input.MaxResultCount = totalCount;
                }

                if (!string.IsNullOrEmpty(input.Sorting))
                {
                    query = query.OrderBy(input.Sorting);
                }

                var output = await query.PageBy(input)
                                        .ToListAsync();

                return new PagedResultDto<ViewTaiSan>(totalCount, output);
            }
            else
            {
                var query = this.vTaiSanRespository.GetAll().Where(w =>
            (w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.KhoiTao ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.DieuChuyen ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.CapPhat ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.ThuHoi ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanDangSuDung ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanHong)).OrderByDescending(o => o.LastModificationTime).AsQueryable();

                if (!string.IsNullOrEmpty(input.TenTaiSan))
                {
                    input.TenTaiSan = input.TenTaiSan.Trim();
                    query = query.Where(w => w.TenTaiSan.Contains(GlobalFunction.RegexFormat(input.TenTaiSan)) || w.SerialNumber.Contains(GlobalFunction.RegexFormat(input.TenTaiSan)));
                }

                query = query.Where(w => input.PhongBanqQL.Contains((int)w.PhongBanQuanLyId));

                if (input.LoaiTaiSan != null)
                {
                    query = query.Where(w => w.LoaiTaiSanId == input.LoaiTaiSan);
                }

                int totalCount = await query.CountAsync();
                if (totalCount != 0)
                {
                    input.MaxResultCount = totalCount;
                }

                if (!string.IsNullOrEmpty(input.Sorting))
                {
                    query = query.OrderBy(input.Sorting);
                }

                var output = await query.PageBy(input)
                                        .ToListAsync();

                return new PagedResultDto<ViewTaiSan>(totalCount, output);
            }
        }

        public async Task HoanTacTaiSanSuaChuaBaoDuongAsync(EntityDto[] input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            var taiSanList = input.Select(s => s.Id).ToList();
            var phieuTaiSanChiTietList = this.phieuTaiSanChiTietRespository.GetAll().Where(w => taiSanList.Contains((int)w.TaiSanId)).ToList();
            foreach (var taiSan in taiSanList)
            {
                await this.lookupTableAppService.DeleteTaiSanAsync(taiSan);
            }
        }

        public async Task<int> EditTaiSanSuaChuaBaoDuong(ViewTaiSanSuaChuaBaoDuong input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // Cập nhật phiếu chi tiết
            var phieuTaiSanChiTiet = this.phieuTaiSanChiTietRespository.GetAll().Where(w => w.Id == input.PhieuTaiSanChiTietId).FirstOrDefault();
            phieuTaiSanChiTiet.TrangThaiId = input.TrangThai;
            await this.phieuTaiSanChiTietRespository.UpdateAsync(phieuTaiSanChiTiet);

            // Cập nhật Tài sản
            if (input.TrangThai == (int)GlobalConst.TrangThaiSuaChuaBaoDuongConst.ThanhCong)
            {
                await GlobalFunction.CapNhatTrangThaiTaiSan(this.taiSanRespository, (int)input.Id, (int)GlobalConst.TrangThaiTaiSanConst.CapPhat);
            }

            // khai báo nội dung thông báo
            string noiDung = string.Empty;
            string tochucNhan = input.PhongBanQuanLyId != null ? this.toChucRespository.FirstOrDefault(w => w.Id == input.PhongBanQuanLyId).TenToChuc : string.Empty;
            string nguoiGui = this.userRepository.FirstOrDefault(w => w.Id == this.AbpSession.UserId).Name;

            // Gửi thông báo Huỷ
            var listUser = this.userRepository.GetAllList();
            if (input.TrangThai == (int)GlobalConst.TrangThaiSuaChuaBaoDuongConst.ThanhCong)
            {
                noiDung = input.TenTaiSan + " đã sửa xong";
            }
            else if (input.TrangThai == (int)GlobalConst.TrangThaiSuaChuaBaoDuongConst.KhongThanhCong)
            {
                noiDung = input.TenTaiSan + " không khắc phục được";
            }

            await GlobalFunction.GuiThongBaoAsync(this.canhBaoRepository, this.toChucRespository, listUser, this.PermissionChecker, (int)this.AbpSession.UserId, input.PhongBanQuanLyId, noiDung, (int)GlobalConst.CanhBaoThongBao.SuaXong);

            return 0;
        }

        public async Task<int> CreateTaiSanSuaChuaBaoDuong(PhieuTaiSanCreateInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // Truyền thông tin cho phiếu
            input.NguoiKhaiBaoId = this.AbpSession.UserId;
            input.ToChucKhaiBaoId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
            input.NgayKhaiBao = GlobalFunction.GetDateTime(input.NgayKhaiBao);
            input.DiaChi = GlobalFunction.RegexFormat(input.DiaChi);
            var createPhieuTaiSan = this.ObjectMapper.Map<PhieuTaiSan>(input);
            var taiSanIdList = createPhieuTaiSan.PhieuTaiSanChiTietList.Select(s => (int)s.TaiSanId).ToList();

            // Tạo phiếu và phiếu chi tiết mới
            await this.phieuTaiSanRespository.InsertAsync(createPhieuTaiSan);

            // Cập nhật Tài sản
            await GlobalFunction.CapNhatTrangThaiTaiSanList(this.taiSanRespository, taiSanIdList, (int)createPhieuTaiSan.PhanLoaiId);
            return 0;
        }
    }
}
