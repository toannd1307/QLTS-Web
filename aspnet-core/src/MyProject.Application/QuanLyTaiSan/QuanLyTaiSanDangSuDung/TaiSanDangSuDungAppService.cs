namespace MyProject.TaiSanDangSuDung
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
    using MyProject.Global;
    using MyProject.QuanLyTaiSan.QuanLyTaiSanChuaSuDung.Dtos;

    public class TaiSanDangSuDungAppService : MyProjectAppServiceBase, ITaiSanDangSuDungAppService
    {
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IRepository<LoaiTaiSan> loaiTaiSanRepository;
        private readonly IRepository<PhieuTaiSan, long> phieuTaiSanRepository;
        private readonly IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRepository;
        private readonly IRepository<ViTriDiaLy> viTriDiaLyRepository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IRepository<NhaCungCap> nhaCungCapRepository;
        private readonly LookupTableAppService lookupTableAppService;

        public TaiSanDangSuDungAppService(
            IRepository<TaiSan> taiSanRepository,
            IRepository<LoaiTaiSan> loaiTaiSanRepository,
            IRepository<PhieuTaiSan, long> phieuTaiSanRepository,
            IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRepository,
            IRepository<ViTriDiaLy> viTriDiaLyRepository,
            IRepository<ToChuc> toChucRepository,
            IRepository<NhaCungCap> nhaCungCapRepository,
            LookupTableAppService lookupTableAppService)
        {
            this.taiSanRepository = taiSanRepository;
            this.loaiTaiSanRepository = loaiTaiSanRepository;
            this.phieuTaiSanRepository = phieuTaiSanRepository;
            this.phieuTaiSanChiTietRepository = phieuTaiSanChiTietRepository;
            this.viTriDiaLyRepository = viTriDiaLyRepository;
            this.toChucRepository = toChucRepository;
            this.nhaCungCapRepository = nhaCungCapRepository;
            this.lookupTableAppService = lookupTableAppService;
        }

        public async Task<PagedResultDto<TaiSanChuaSuDungForViewDto>> GetAllAsync(TaiSanChuaSuDungGetAllInputDto input)
        {
            try
            {
                #region Check null
                if (input == null)
                {
                    throw new UserFriendlyException(StringResources.NullParameter);
                }
                #endregion

                if (input.PhongBanQuanLyId == null || input.PhongBanQuanLyId.Count == 0)
                {
                    input.PhongBanQuanLyId = new List<int> { -1 };
                }

                var toChucId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
                var filter = this.taiSanRepository.GetAll().Where(w => input.PhongBanQuanLyId.Contains((int)w.ToChucId))
                                    .Where(w => w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanDangSuDung)
                                    .WhereIf(input.LoaiTaiSanId != null, e => e.LoaiTaiSanId == input.LoaiTaiSanId)
                                    .WhereIf(input.NhaCungCapId != null, e => e.NhaCungCapId == input.NhaCungCapId)
                                    .WhereIf(input.TinhTrangSuDung != null, e => (e.TinhTrangSuDungQRCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.QRCode) ||
                                    (e.TinhTrangSuDungBarCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.Barcode)
                                    || (e.TinhTrangSuDungRFIDCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.RFID));

                input.KeyWord = GlobalFunction.RegexFormat(input.KeyWord);
                var phieuChiTietIdList = (from q in this.taiSanRepository.GetAll()
                                          .Where(w => input.PhongBanQuanLyId.Contains((int)w.ToChucId))
                                          .Where(w => w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanDangSuDung)
                                          from phieuTaiSanChiTiet in this.phieuTaiSanChiTietRepository.GetAll().Where(w => w.TaiSanId == q.Id)
                                          from phieuTaiSan in this.phieuTaiSanRepository.GetAll().Where(w => w.Id == phieuTaiSanChiTiet.PhieuTaiSanId && w.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanDangSuDung)
                                          select new
                                          {
                                              q.Id,
                                              PhieuChiTietId = phieuTaiSanChiTiet.Id,
                                          }).ToList();
                var queryGroup = phieuChiTietIdList.OrderByDescending(e => e.PhieuChiTietId).GroupBy(e => e.Id).Select(e => e.ToList().Max(l => l.PhieuChiTietId)).ToList();

                var query = from q in filter.WhereIf(!string.IsNullOrEmpty(input.KeyWord), e => e.EPCCode.Contains(input.KeyWord) || e.TenTaiSan.Contains(input.KeyWord)
                            || e.SerialNumber.Contains(input.KeyWord) || e.NgayMuaStr.Contains(input.KeyWord)
                            || e.NguyenGiaStr.Contains(input.KeyWord))
                            from toChuc in this.toChucRepository.GetAll().Where(w => w.Id == q.ToChucId).DefaultIfEmpty()
                            from loaiTaiSan in this.loaiTaiSanRepository.GetAll().Where(w => w.Id == q.LoaiTaiSanId).DefaultIfEmpty()
                            from nhaCungCap in this.nhaCungCapRepository.GetAll().Where(w => w.Id == q.NhaCungCapId).DefaultIfEmpty()
                            from viTriDiaLy in this.viTriDiaLyRepository.GetAll().Where(w => w.Id == toChuc.ViTriDiaLyId).DefaultIfEmpty()
                            from phieuTaiSanChiTiet in this.phieuTaiSanChiTietRepository.GetAll().Where(w => w.TaiSanId == q.Id && queryGroup.Contains(w.Id)).DefaultIfEmpty()
                            from phieuTaiSan in this.phieuTaiSanRepository.GetAll().Where(w => w.Id == phieuTaiSanChiTiet.PhieuTaiSanId).DefaultIfEmpty()
                            from phongBanCha in this.toChucRepository.GetAll().Where(w => w.Id == toChuc.TrucThuocToChucId).DefaultIfEmpty()
                            select new TaiSanChuaSuDungForViewDto()
                            {
                                Id = q.Id,
                                TenTaiSan = q.TenTaiSan,
                                EPCCode = q.EPCCode,
                                LoaiTaiSan = loaiTaiSan.Ten,
                                SerialNumber = q.SerialNumber,
                                NhaCungCap = nhaCungCap.TenNhaCungCap,
                                NgayMua = q.NgayMua,
                                NguyenGia = q.NguyenGia,
                                PhongBanQuanLy = (toChuc.TrucThuocToChucId > 1 ? (phongBanCha.MaToChuc + " - ") : string.Empty) + toChuc.TenToChuc,
                                ToChucId = toChuc.Id,
                                MaSuDung = GlobalFunction.GetMaSuDungTaiSan(q.TinhTrangSuDungQRCode, q.TinhTrangSuDungBarCode, q.TinhTrangSuDungRFIDCode),
                                ViTriTaiSan = viTriDiaLy.TenViTri,
                                TimeSort = q.LastModificationTime != null ? q.LastModificationTime : q.CreationTime,
                                CapPhat = q.ToChucId == toChucId,
                                DieuChuyen = q.ToChucId != toChucId,
                                ThuHoi = q.ToChucId != toChucId,
                                NgayKhaiBao = phieuTaiSan.NgayKhaiBao,
                                LastModificationTime = q.LastModificationTime,
                            };

                int totalCount = await query.CountAsync();

                var output = query.PageBy(input)
                                        .ToList();
                if (input.Sorting != null && input.Sorting.Contains("maSuDung"))
                {
                    if (input.Sorting.Contains("DESC"))
                    {
                        output.OrderByDescending(s => s.MaSuDung).ToList();
                    }
                    else
                    {
                        output.OrderBy(s => s.MaSuDung).ToList();
                    }
                }
                else
                {
                    output = await query.OrderBy(input.Sorting ?? "TimeSort DESC")
                                        .PageBy(input)
                                        .ToListAsync();
                }

                return new PagedResultDto<TaiSanChuaSuDungForViewDto>(totalCount, output);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task HoanTacTaiSanDangSuDungAsync(EntityDto[] input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var taiSanList = input.Select(s => s.Id).ToList();
            var phieuTaiSanChiTietList = this.phieuTaiSanChiTietRepository.GetAll().Where(w => taiSanList.Contains((int)w.TaiSanId)).ToList();
            await this.lookupTableAppService.DeleteTaiSanListAsync(taiSanList);
        }

        public async Task HoanTacTaiSanListAsync(List<int> taiSanId)
        {
            await GlobalFunction.XoaTaiSan(this.taiSanRepository, this.phieuTaiSanRepository, this.phieuTaiSanChiTietRepository, taiSanId);
        }

        public async Task<PagedResultDto<TaiSanChuaSuDungForViewDto>> GetAllByMaAndSerialNumberTsdsdAsync(TaiSanChuaSuDungGetAllInputDto input)
        {
            try
            {
                #region Check null
                if (input == null)
                {
                    throw new UserFriendlyException(StringResources.NullParameter);
                }
                #endregion
                var toChucId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
                if (input.PhongBanQuanLyId == null || input.PhongBanQuanLyId.Count == 0)
                {
                    input.PhongBanQuanLyId = new List<int> { -1 };
                }

                var filter = this.taiSanRepository.GetAll().Where(w => w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanDangSuDung)
                                    .Where(w => input.PhongBanQuanLyId.Contains((int)w.ToChucId))
                                    .WhereIf(input.LoaiTaiSanId != null, e => e.LoaiTaiSanId == input.LoaiTaiSanId)
                                    .WhereIf(input.NhaCungCapId != null, e => e.NhaCungCapId == input.NhaCungCapId)
                                    .WhereIf(input.TinhTrangSuDung != null, e => (e.TinhTrangSuDungQRCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.QRCode) ||
                                    (e.TinhTrangSuDungBarCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.Barcode)
                                    || (e.TinhTrangSuDungRFIDCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.RFID));

                input.KeyWord = GlobalFunction.RegexFormat(input.KeyWord);
                var query = from q in filter.WhereIf(!string.IsNullOrEmpty(input.KeyWord), e => e.TenTaiSan.Contains(input.KeyWord)
                            || e.SerialNumber.Contains(input.KeyWord))
                            from toChuc in this.toChucRepository.GetAll().Where(w => w.Id == q.ToChucId).DefaultIfEmpty()
                            from loaiTaiSan in this.loaiTaiSanRepository.GetAll().Where(w => w.Id == q.LoaiTaiSanId).DefaultIfEmpty()
                            from nhaCungCap in this.nhaCungCapRepository.GetAll().Where(w => w.Id == q.NhaCungCapId).DefaultIfEmpty()
                            from viTriDiaLy in this.viTriDiaLyRepository.GetAll().Where(w => w.Id == toChuc.ViTriDiaLyId).DefaultIfEmpty()
                            select new TaiSanChuaSuDungForViewDto()
                            {
                                Id = q.Id,
                                TenTaiSan = q.TenTaiSan,
                                EPCCode = q.EPCCode,
                                LoaiTaiSan = loaiTaiSan.Ten,
                                SerialNumber = q.SerialNumber,
                                NhaCungCap = nhaCungCap.TenNhaCungCap,
                                NgayMua = q.NgayMua,
                                NguyenGia = q.NguyenGia,
                                PhongBanQuanLy = toChuc.TenToChuc,
                                MaSuDung = GlobalFunction.GetMaSuDungTaiSan(q.TinhTrangSuDungQRCode, q.TinhTrangSuDungBarCode, q.TinhTrangSuDungRFIDCode),
                                ViTriTaiSan = viTriDiaLy.TenViTri,
                                TimeSort = q.LastModificationTime != null ? q.LastModificationTime : q.CreationTime,
                                CapPhat = q.ToChucId == toChucId,
                                DieuChuyen = q.ToChucId != toChucId,
                                ThuHoi = q.ToChucId != toChucId,
                                ToChucId = q.ToChucId,
                            };

                int totalCount = await query.CountAsync();

                var output = await query.OrderBy(input.Sorting ?? "TimeSort DESC")
                                        .ToListAsync();
                return new PagedResultDto<TaiSanChuaSuDungForViewDto>(totalCount, output);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<PagedResultDto<TaiSanChuaSuDungForViewDto>> GetAllTaiSanCapPhatAsync(TaiSanChuaSuDungGetAllInputDto input)
        {
            try
            {
                #region Check null
                if (input == null)
                {
                    throw new UserFriendlyException(StringResources.NullParameter);
                }
                #endregion
                var toChucId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
                input.PhongBanQuanLyId = await this.lookupTableAppService.GetAllToChucIdTheoNguoiDungListAsync(true);
                var filter = this.taiSanRepository.GetAll().Where(w => input.PhongBanQuanLyId.Contains((int)w.ToChucId))
                                    .Where(w => w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanDangSuDung)
                                    .WhereIf(input.LoaiTaiSanId != null, e => e.LoaiTaiSanId == input.LoaiTaiSanId)
                                    .WhereIf(input.NhaCungCapId != null, e => e.NhaCungCapId == input.NhaCungCapId)
                                    .WhereIf(input.TinhTrangSuDung != null, e => (e.TinhTrangSuDungQRCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.QRCode) ||
                                    (e.TinhTrangSuDungBarCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.Barcode)
                                    || (e.TinhTrangSuDungRFIDCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.RFID));

                input.KeyWord = GlobalFunction.RegexFormat(input.KeyWord);
                var query = from q in filter.WhereIf(!string.IsNullOrEmpty(input.KeyWord), e => e.TenTaiSan.Contains(input.KeyWord)
                            || e.SerialNumber.Contains(input.KeyWord))
                            from toChuc in this.toChucRepository.GetAll().Where(w => w.Id == q.ToChucId).DefaultIfEmpty()
                            from loaiTaiSan in this.loaiTaiSanRepository.GetAll().Where(w => w.Id == q.LoaiTaiSanId).DefaultIfEmpty()
                            from nhaCungCap in this.nhaCungCapRepository.GetAll().Where(w => w.Id == q.NhaCungCapId).DefaultIfEmpty()
                            from viTriDiaLy in this.viTriDiaLyRepository.GetAll().Where(w => w.Id == toChuc.ViTriDiaLyId).DefaultIfEmpty()
                            select new TaiSanChuaSuDungForViewDto()
                            {
                                Id = q.Id,
                                TenTaiSan = q.TenTaiSan,
                                EPCCode = q.EPCCode,
                                LoaiTaiSan = loaiTaiSan.Ten,
                                SerialNumber = q.SerialNumber,
                                NhaCungCap = nhaCungCap.TenNhaCungCap,
                                NgayMua = q.NgayMua,
                                NguyenGia = q.NguyenGia,
                                PhongBanQuanLy = toChuc.TenToChuc,
                                MaSuDung = GlobalFunction.GetMaSuDungTaiSan(q.TinhTrangSuDungQRCode, q.TinhTrangSuDungBarCode, q.TinhTrangSuDungRFIDCode),
                                ViTriTaiSan = viTriDiaLy.TenViTri,
                                TimeSort = q.LastModificationTime != null ? q.LastModificationTime : q.CreationTime,
                                CapPhat = q.ToChucId == toChucId,
                                DieuChuyen = q.ToChucId != toChucId,
                                ThuHoi = q.ToChucId != toChucId,
                                ToChucId = q.ToChucId,
                            };

                int totalCount = await query.CountAsync();

                var output = await query.OrderBy(input.Sorting ?? "TimeSort DESC")
                                        .ToListAsync();
                return new PagedResultDto<TaiSanChuaSuDungForViewDto>(totalCount, output);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
