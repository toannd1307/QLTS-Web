namespace MyProject.TaiSanChuaSuDung
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Abp;
    using Abp.Application.Services.Dto;
    using Abp.Authorization;
    using Abp.Authorization.Users;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization;
    using MyProject.Authorization.Users;
    using MyProject.Global;
    using MyProject.QuanLyTaiSan.Dtos;
    using MyProject.QuanLyTaiSan.QuanLyTaiSanChuaSuDung.Dtos;

    public class TaiSanChuaSuDungAppService : MyProjectAppServiceBase, ITaiSanChuaSuDungAppService
    {
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IRepository<LoaiTaiSan> loaiTaiSanRepository;
        private readonly IRepository<PhieuTaiSan, long> phieuTaiSanRepository;
        private readonly IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRepository;
        private readonly IRepository<ViTriDiaLy> viTriDiaLyRepository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IRepository<NhaCungCap> nhaCungCapRepository;
        private readonly IRepository<User, long> userRepository;
        private readonly IRepository<CanhBao> canhBaoRepository;
        private readonly LookupTableAppService lookupTableAppService;

        public TaiSanChuaSuDungAppService(
            IRepository<TaiSan> taiSanRepository,
            IRepository<LoaiTaiSan> loaiTaiSanRepository,
            IRepository<PhieuTaiSan, long> phieuTaiSanRepository,
            IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRepository,
            IRepository<ViTriDiaLy> viTriDiaLyRepository,
            IRepository<ToChuc> toChucRepository,
            IRepository<NhaCungCap> nhaCungCapRepository,
            LookupTableAppService lookupTableAppService,
            IRepository<User, long> userRepository,
            IRepository<CanhBao> canhBaoRepository)
        {
            this.taiSanRepository = taiSanRepository;
            this.loaiTaiSanRepository = loaiTaiSanRepository;
            this.phieuTaiSanRepository = phieuTaiSanRepository;
            this.phieuTaiSanChiTietRepository = phieuTaiSanChiTietRepository;
            this.viTriDiaLyRepository = viTriDiaLyRepository;
            this.toChucRepository = toChucRepository;
            this.nhaCungCapRepository = nhaCungCapRepository;
            this.lookupTableAppService = lookupTableAppService;
            this.userRepository = userRepository;
            this.canhBaoRepository = canhBaoRepository;
        }

        // danh sách tài sản chưa sử dụng
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
                    .Where(w => w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.CapPhat
                        || w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.DieuChuyen || w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.ThuHoi || w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.KhoiTao)
                                            .WhereIf(input.LoaiTaiSanId != null, e => e.LoaiTaiSanId == input.LoaiTaiSanId)
                                            .WhereIf(input.NhaCungCapId != null, e => e.NhaCungCapId == input.NhaCungCapId)
                                            .WhereIf(input.TinhTrangSuDung != null, e => (e.TinhTrangSuDungQRCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.QRCode) ||
                                            (e.TinhTrangSuDungBarCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.Barcode)
                                            || (e.TinhTrangSuDungRFIDCode == true && input.TinhTrangSuDung == (int)GlobalConst.TinhTrangMaSuDungTaiSanConst.RFID));

                input.KeyWord = GlobalFunction.RegexFormat(input.KeyWord);
                var query = from q in filter.WhereIf(!string.IsNullOrEmpty(input.KeyWord), e => e.EPCCode.Contains(input.KeyWord) || e.TenTaiSan.Contains(input.KeyWord)
                            || e.SerialNumber.Contains(input.KeyWord) || e.NgayMuaStr.Contains(input.KeyWord)
                            || e.NguyenGiaStr.Contains(input.KeyWord))
                            from toChuc in this.toChucRepository.GetAll().Where(w => w.Id == q.ToChucId).DefaultIfEmpty()
                            from loaiTaiSan in this.loaiTaiSanRepository.GetAll().Where(w => w.Id == q.LoaiTaiSanId).DefaultIfEmpty()
                            from nhaCungCap in this.nhaCungCapRepository.GetAll().Where(w => w.Id == q.NhaCungCapId).DefaultIfEmpty()
                            from viTriDiaLy in this.viTriDiaLyRepository.GetAll().Where(w => w.Id == toChuc.ViTriDiaLyId).DefaultIfEmpty()
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
                                ToChucId = toChuc != null ? toChuc.Id : (int?)null,
                                MaSuDung = GlobalFunction.GetMaSuDungTaiSan(q.TinhTrangSuDungQRCode, q.TinhTrangSuDungBarCode, q.TinhTrangSuDungRFIDCode),
                                ViTriTaiSan = viTriDiaLy.TenViTri,
                                TimeSort = q.LastModificationTime != null ? q.LastModificationTime : q.CreationTime,
                                CapPhat = q.ToChucId == toChucId,
                                DieuChuyen = q.ToChucId != toChucId,
                                ThuHoi = q.ToChucId != toChucId,
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

        public async Task<PagedResultDto<TaiSanChuaSuDungForViewDto>> GetAllByMaAndSerialNumberAsync(TaiSanChuaSuDungGetAllInputDto input)
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
                    .Where(w => w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.CapPhat
                        || w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.DieuChuyen || w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.ThuHoi || w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.KhoiTao)
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
                    .Where(w => w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.CapPhat
                        || w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.DieuChuyen || w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.ThuHoi || w.TrangThaiId == (int)GlobalConst.TrangThaiTaiSanConst.KhoiTao)
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

        public async Task<int> CapPhatTaiSanAsync(PhieuTaiSanCreateInputDto input)
        {
            try
            {
                #region Check null
                if (input == null)
                {
                    throw new UserFriendlyException(StringResources.NullParameter);
                }
                #endregion
                if (input.PhanLoaiId != null && (input.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.ThuHoi))
                {
                    input.ToChucDuocNhanId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
                }

                // khai báo nội dung thông báo
                string noiDung = string.Empty;
                string tochucNhan = input.ToChucDuocNhanId != null ? this.toChucRepository.FirstOrDefault(w => w.Id == input.ToChucDuocNhanId).TenToChuc : string.Empty;
                string nguoiGui = this.userRepository.FirstOrDefault(w => w.Id == this.AbpSession.UserId).Name;

                // Truyền thông tin cho phiếu
                input.NguoiKhaiBaoId = this.AbpSession.UserId;
                input.ToChucKhaiBaoId = this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId;
                input.NgayKhaiBao = GlobalFunction.GetDateTime(input.NgayKhaiBao);
                var createPhieuTaiSan = this.ObjectMapper.Map<PhieuTaiSan>(input);
                var taiSanIdList = createPhieuTaiSan.PhieuTaiSanChiTietList.Select(s => (int)s.TaiSanId).ToList();
                var listtaisan = this.taiSanRepository.GetAll().Where(w => taiSanIdList.Contains(w.Id)).ToList();
                foreach (var item in createPhieuTaiSan.PhieuTaiSanChiTietList)
                {
                    item.ToChucDangQuanLyId = listtaisan.FirstOrDefault(e => e.Id == item.TaiSanId)?.ToChucId;
                }

                // Cập nhật Tài sản
                if (input.PhanLoaiId != null && (input.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanDangSuDung))
                {
                    await GlobalFunction.CapNhatTrangThaiTaiSanList(this.taiSanRepository, taiSanIdList, (int)createPhieuTaiSan.PhanLoaiId);
                }
                else

                // Cấp phát tài sản
                if (input.PhanLoaiId != null && (input.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.CapPhat))
                {
                    await GlobalFunction.CapNhatTrangThaiTaiSanList(this.taiSanRepository, taiSanIdList, (int)createPhieuTaiSan.PhanLoaiId, (int)input.ToChucDuocNhanId);
                    if (createPhieuTaiSan.PhieuTaiSanChiTietList.Count == 1)
                    {
                        var taiSan = this.taiSanRepository.FirstOrDefault(w => w.Id == input.PhieuTaiSanChiTietList[0].TaiSanId);
                        string toChuc = this.toChucRepository.FirstOrDefault(w => w.Id == taiSan.ToChucId).TenToChuc;
                        noiDung = nguoiGui + " đã cấp phát " + taiSan.TenTaiSan + " cho " + tochucNhan;
                    }
                    else if (createPhieuTaiSan.PhieuTaiSanChiTietList.Count > 1)
                    {
                        noiDung = nguoiGui + " đã cấp phát " + createPhieuTaiSan.PhieuTaiSanChiTietList.Count + " tài sản cho " + tochucNhan;
                    }

                    var listUser = this.userRepository.GetAllList();
                    await GlobalFunction.GuiThongBaoAsync(this.canhBaoRepository, this.toChucRepository, listUser, this.PermissionChecker, (int)this.AbpSession.UserId, input.ToChucDuocNhanId, noiDung, (int)GlobalConst.CanhBaoThongBao.CapPhat);
                }
                else
                {
                    await GlobalFunction.CapNhatTrangThaiTaiSanList(this.taiSanRepository, taiSanIdList, (int)createPhieuTaiSan.PhanLoaiId, (int)input.ToChucDuocNhanId);
                    var listUser = this.userRepository.GetAllList();

                    // Gửi thông báo Thu Hồi
                    if (input.PhanLoaiId == (int)GlobalConst.CanhBaoThongBao.ThuHoi)
                    {
                        var b = createPhieuTaiSan.PhieuTaiSanChiTietList.ToList().GroupBy(e => e.ToChucDangQuanLyId).Select(g => new
                        {
                            ToChucQuanLyId = g.Key,
                            ListTS = g.ToList(),
                        }).ToList();
                        foreach (var ts in b)
                        {
                            string toChuc = this.toChucRepository.FirstOrDefault(w => w.Id == ts.ToChucQuanLyId).TenToChuc;
                            if (ts.ListTS.Count == 1)
                            {
                                var taiSan = this.taiSanRepository.FirstOrDefault(w => w.Id == input.PhieuTaiSanChiTietList[0].TaiSanId);
                                noiDung = nguoiGui + " đã thu hồi " + taiSan.TenTaiSan + " từ " + toChuc;
                            }
                            else if (ts.ListTS.Count > 1)
                            {
                                noiDung = nguoiGui + " đã thu hồi " + ts.ListTS.Count + " tài sản từ " + toChuc;
                            }

                            await GlobalFunction.GuiThongBaoAsync(this.canhBaoRepository, this.toChucRepository, listUser, this.PermissionChecker, (int)this.AbpSession.UserId, ts.ToChucQuanLyId, noiDung, (int)GlobalConst.CanhBaoThongBao.ThuHoi);
                        }
                    }

                    // Gửi thông báo Điều chuyển
                    if (input.PhanLoaiId == (int)GlobalConst.CanhBaoThongBao.Dieuchuyen)
                    {
                        var b = createPhieuTaiSan.PhieuTaiSanChiTietList.ToList().GroupBy(e => e.ToChucDangQuanLyId).Select(g => new
                        {
                            ToChucQuanLyId = g.Key,
                            ListTS = g.ToList(),
                        }).ToList();
                        foreach (var ts in b)
                        {
                            string toChucQuanLy = this.toChucRepository.FirstOrDefault(w => w.Id == ts.ToChucQuanLyId).TenToChuc;
                            if (ts.ListTS.Count == 1)
                            {
                                var taiSan = this.taiSanRepository.FirstOrDefault(w => w.Id == input.PhieuTaiSanChiTietList[0].TaiSanId);
                                noiDung = nguoiGui + " đã điều chuyển " + taiSan.TenTaiSan + " từ " + toChucQuanLy + " sang " + tochucNhan;
                            }
                            else if (ts.ListTS.Count > 1)
                            {
                                noiDung = nguoiGui + " đã điều chuyển " + ts.ListTS.Count + " tài sản từ " + toChucQuanLy + " sang " + tochucNhan;
                            }

                            await GlobalFunction.GuiThongBaoAsync(this.canhBaoRepository, this.toChucRepository, listUser, this.PermissionChecker, (int)this.AbpSession.UserId, input.ToChucDuocNhanId, noiDung, (int)GlobalConst.CanhBaoThongBao.Dieuchuyen);
                        }
                    }
                }

                // Tạo phiếu và phiếu chi tiết mới
                await this.phieuTaiSanRepository.InsertAsync(createPhieuTaiSan);

                return 1;
            }
            catch (Exception e)
            {
                var error = e.Message;
                return 0;
            }
        }
    }
}
