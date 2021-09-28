namespace MyProject.TaiSanMat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization;
    using MyProject.Authorization.Users;
    using MyProject.Global;
    using MyProject.QuanLyTaiSan.Dtos;

    public class TaiSanMatAppService : MyProjectAppServiceBase, ITaiSanMatAppService
    {
        private readonly IRepository<ViewTaiSanMat> vTaiSanMatRespository;
        private readonly IRepository<ViewTaiSan> vTaiSanRespository;
        private readonly IRepository<TaiSan> taiSanRespository;
        private readonly IRepository<PhieuTaiSan, long> phieuTaiSanRespository;
        private readonly IRepository<ToChuc> toChucRespository;
        private readonly ILookupTableAppService lookupTableAppService;
        private readonly IRepository<User, long> userRepository;
        private readonly IRepository<CanhBao> canhBaoRepository;

        public TaiSanMatAppService(
            IRepository<ViewTaiSanMat> vTaiSanMatRespository,
            IRepository<ViewTaiSan> vTaiSanRespository,
            IRepository<TaiSan> taiSanRespository,
            IRepository<PhieuTaiSan, long> phieuTaiSanRespository,
            IRepository<ToChuc> toChucRespository,
            ILookupTableAppService lookupTableAppService,
            IRepository<User, long> userRepository,
            IRepository<CanhBao> canhBaoRepository)
        {
            this.vTaiSanMatRespository = vTaiSanMatRespository;
            this.vTaiSanRespository = vTaiSanRespository;
            this.taiSanRespository = taiSanRespository;
            this.phieuTaiSanRespository = phieuTaiSanRespository;
            this.toChucRespository = toChucRespository;
            this.lookupTableAppService = lookupTableAppService;
            this.userRepository = userRepository;
            this.canhBaoRepository = canhBaoRepository;
        }

        public async Task<PagedResultDto<ViewTaiSanMat>> GetAllTaiSanMatAsync(TaiSanMatGetAllInputDto input)
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
                var items = this.vTaiSanMatRespository.GetAll().Where(w => phongBanCuaNguoiDung.Contains((int)w.PhongBanQuanLyId)).OrderByDescending(o => o.LastModificationTime).AsQueryable();
                var query = this.vTaiSanMatRespository.GetAll().Where(w => 1 == 0);

                if (!string.IsNullOrEmpty(input.TenTaiSan))
                {
                    input.TenTaiSan = input.TenTaiSan.Trim();

                    query = query.Union(items.Where(w => w.TenTaiSan.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                    || w.SerialNumber.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                    || w.EPCCode.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                    || w.NgayMuaStr.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
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

                int totalCount = await query.CountAsync();

                if (!string.IsNullOrEmpty(input.Sorting))
                {
                    query = query.OrderBy(input.Sorting);
                }

                var output = await query.PageBy(input)
                                        .ToListAsync();

                return new PagedResultDto<ViewTaiSanMat>(totalCount, output);
            }
            else
            {
                var items = this.vTaiSanMatRespository.GetAll().OrderByDescending(o => o.LastModificationTime).AsQueryable();
                var query = this.vTaiSanMatRespository.GetAll().Where(w => 1 == 0);

                if (!string.IsNullOrEmpty(input.TenTaiSan))
                {
                    input.TenTaiSan = input.TenTaiSan.Trim();

                    query = query.Union(items.Where(w => w.TenTaiSan.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                    || w.SerialNumber.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                    || w.EPCCode.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
                    || w.NgayMuaStr.Contains(GlobalFunction.RegexFormat(input.TenTaiSan))
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

                int totalCount = await query.CountAsync();

                if (!string.IsNullOrEmpty(input.Sorting))
                {
                    query = query.OrderBy(input.Sorting);
                }

                var output = await query.PageBy(input)
                                        .ToListAsync();

                return new PagedResultDto<ViewTaiSanMat>(totalCount, output);
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
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanSuaChua ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanBaoDuong ||
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
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanSuaChua ||
            w.HinhThuc == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanBaoDuong ||
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

        public async Task<int> CreateTaiSanMat(PhieuTaiSanCreateInputDto input)
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

            // khai báo nội dung thông báo
            string noiDung = string.Empty;
            string tochucNhan = input.ToChucDuocNhanId != null ? this.toChucRespository.FirstOrDefault(w => w.Id == input.ToChucDuocNhanId).TenToChuc : string.Empty;
            string nguoiGui = this.userRepository.FirstOrDefault(w => w.Id == this.AbpSession.UserId).Name;

            // Gửi thông báo Mất
            var listUser = this.userRepository.GetAllList();
            var b = this.taiSanRespository.GetAll().Where(w => taiSanIdList.Contains(w.Id)).ToList().GroupBy(e => e.ToChucId).Select(g => new
            {
                ToChucQuanLyId = g.Key,
                ListTS = g.ToList(),
            }).ToList();
            foreach (var ts in b)
            {
                string toChuc = this.toChucRespository.FirstOrDefault(w => w.Id == ts.ToChucQuanLyId).TenToChuc;
                if (ts.ListTS.Count == 1)
                {
                    noiDung = nguoiGui + " đã báo mất " + ts.ListTS[0].TenTaiSan;
                }
                else if (ts.ListTS.Count > 1)
                {
                    noiDung = nguoiGui + " đã báo mất " + ts.ListTS.Count + " tài sản";
                }

                await GlobalFunction.GuiThongBaoAsync(this.canhBaoRepository, this.toChucRespository, listUser, this.PermissionChecker, (int)this.AbpSession.UserId, ts.ToChucQuanLyId, noiDung, (int)GlobalConst.CanhBaoThongBao.BaoMat);
            }

            return 0;
        }

        public async Task HoanTacTaiSanMatAsync(EntityDto[] input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            var taiSanList = input.Select(s => s.Id);
            foreach (var taiSan in taiSanList)
            {
                await this.lookupTableAppService.DeleteTaiSanAsync(taiSan);
            }

            //// khai báo nội dung thông báo
            // string noiDung = string.Empty;
            // string nguoiGui = this.userRepository.FirstOrDefault(w => w.Id == this.AbpSession.UserId).Name;

            //// Gửi thông báo Hỏng
            // var listUser = this.userRepository.GetAllList();
            // var a = input.Select(s => s.Id).ToList();
            // var b = this.taiSanRespository.GetAll().Where(w => a.Contains(w.Id)).ToList().GroupBy(e => e.ToChucId).Select(g => new
            // {
            //    ToChuId = g.Key,
            //    ListTS = g.ToList(),
            // });

            // foreach (var ts in b)
            // {
            //    string toChuc = this.toChucRespository.FirstOrDefault(w => w.Id == ts.ToChuId).TenToChuc;
            //    if (ts.ListTS.Count == 1)
            //    {
            //        var taiSan = this.taiSanRespository.FirstOrDefault(w => w.Id == ts.ListTS[0].Id);
            //        noiDung = "Đã tìm thấy " + taiSan.TenTaiSan;
            //    }
            //    else if (ts.ListTS.Count > 1)
            //    {
            //        noiDung = "Đã tìm thấy " + ts.ListTS.Count + " tài sản";
            //    }

            // await GlobalFunction.GuiThongBaoAsync(this.canhBaoRepository, this.toChucRespository, listUser, this.PermissionChecker, (int)this.AbpSession.UserId, ts.ToChuId, noiDung, (int)GlobalConst.CanhBaoThongBao.TimThay);
            // }
        }
    }
}
