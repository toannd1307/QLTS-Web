namespace MyProject.ToanBoTaiSan
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization.Users;
    using MyProject.Data;
    using MyProject.Data.Excel.Dtos;
    using MyProject.Global;
    using MyProject.Global.Dtos;
    using MyProject.Net.MimeTypes;
    using MyProject.QuanLyTaiSan.QuanLyToanBoTaiSan.Dtos;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class ToanBoTaiSanAppService : MyProjectAppServiceBase, IToanBoTaiSanAppService
    {
        private readonly IRepository<NhaCungCap> nhaCungCapRepository;
        private readonly IRepository<LoaiTaiSan> loaiTaiSanRepository;
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IRepository<User, long> userRepository;
        private readonly IRepository<ViTriDiaLy> viTriRepository;
        private readonly IRepository<PhieuTaiSan, long> phieuTaiSanRepository;
        private readonly IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRepository;
        private readonly IAppFolders appFolders;
        private readonly IRepository<TaiSanDinhKemFile, long> dinhKemRepository;

        public ToanBoTaiSanAppService(
            IRepository<NhaCungCap> nhaCungCapRepository,
            IRepository<LoaiTaiSan> loaiTaiSanRepository,
            IRepository<TaiSan> taiSanRepository,
            IRepository<ToChuc> toChucRepository,
            IRepository<User, long> userRepository,
            IRepository<ViTriDiaLy> viTriRepository,
            IRepository<PhieuTaiSan, long> phieuTaiSanRepository,
            IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRepository,
            IAppFolders appFolders,
            IRepository<TaiSanDinhKemFile, long> dinhKemRepository)
        {
            this.nhaCungCapRepository = nhaCungCapRepository;
            this.loaiTaiSanRepository = loaiTaiSanRepository;
            this.taiSanRepository = taiSanRepository;
            this.toChucRepository = toChucRepository;
            this.userRepository = userRepository;
            this.viTriRepository = viTriRepository;
            this.appFolders = appFolders;
            this.dinhKemRepository = dinhKemRepository;
            this.phieuTaiSanRepository = phieuTaiSanRepository;
            this.phieuTaiSanChiTietRepository = phieuTaiSanChiTietRepository;
        }

        public async Task<PagedResultDto<GetAllOutputDto>> GetAll(GetAllInputDto input)
        {
            var pbIdUser = this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId).Select(s => s.ToChucId).FirstOrDefault();

            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            string fillter = GlobalFunction.RegexFormat(input.Fillter);
            if (input.PhongBanqQL == null || input.PhongBanqQL.Count == 0)
            {
                input.PhongBanqQL = new List<int> { -1 };
            }

            var filter = this.taiSanRepository.GetAll().Where(w => input.PhongBanqQL.Contains((int)w.ToChucId))
                                                  .WhereIf(!string.IsNullOrEmpty(fillter), e => e.TenTaiSan.Contains(fillter) || e.EPCCode.Contains(fillter)
                                                    || e.SerialNumber.Contains(fillter) || e.NguyenGiaStr.Contains(fillter) || e.NgayMuaStr.Contains(fillter))
                                                  .WhereIf(input.MaSD != null, e => (input.MaSD == 1 ? ((bool)e.TinhTrangSuDungRFIDCode) : (input.MaSD == 2 ? ((bool)e.TinhTrangSuDungBarCode) : (bool)e.TinhTrangSuDungQRCode)))
                                                  .WhereIf(!string.IsNullOrEmpty(input.NhaCungCap.ToString()), e => e.NhaCungCapId == input.NhaCungCap)
                                                  .WhereIf(input.LoaiTS != null, e => e.LoaiTaiSanId == input.LoaiTS);

            var query = from o in filter
                        from lts in this.loaiTaiSanRepository.GetAll().Where(f => f.Id == o.LoaiTaiSanId).DefaultIfEmpty()
                        from ncc in this.nhaCungCapRepository.GetAll().Where(f => f.Id == o.NhaCungCapId).DefaultIfEmpty()
                        from phongBanQL in this.toChucRepository.GetAll().Where(f => f.Id == o.ToChucId)
                        from viTri in this.viTriRepository.GetAll().Where(w => w.Id == phongBanQL.ViTriDiaLyId)
                        from phongBanCha in this.toChucRepository.GetAll().Where(w => w.Id == phongBanQL.TrucThuocToChucId).DefaultIfEmpty()
                        select new GetAllOutputDto()
                        {
                            Id = o.Id,
                            TenTS = o.TenTaiSan,
                            LoaiTS = lts.Ten,
                            SerialNumber = o.SerialNumber,
                            NhaCungCap = ncc.TenNhaCungCap,
                            NgayMua = o.NgayMua != null ? o.NgayMua.Value.ToString("dd/MM/yyyy") : string.Empty,
                            NguyenGia = o.NguyenGia,
                            PhongBanQL = (phongBanQL.TrucThuocToChucId > 1 ? (phongBanCha.MaToChuc + " - ") : string.Empty) + phongBanQL.TenToChuc,
                            MaSD = GlobalFunction.GetMaSuDungTaiSan(o.TinhTrangSuDungQRCode, o.TinhTrangSuDungBarCode, o.TinhTrangSuDungRFIDCode),
                            ViTriTS = viTri.TenViTri,
                            NgayTao = o.CreationTime,
                            TrangThai = GlobalModel.TrangThaiTaiSanHienThiSorted[(int)o.TrangThaiId],
                            TrangThaiId = (int)o.TrangThaiId,
                            NgayMuaDateTime = o.NgayMua,
                            TinhTrangRFID = o.TinhTrangSuDungRFIDCode,
                            TinhTrangQRCode = o.TinhTrangSuDungQRCode,
                            TinhTrangBarCode = o.TinhTrangSuDungBarCode,
                            MaEPC = o.EPCCode,
                            NguonKinhPhiId = o.NguonKinhPhiId,
                        };

            int totalCount = await query.CountAsync();
            var output = await query.OrderBy(input.Sorting ?? "NgayTao DESC")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<GetAllOutputDto>(totalCount, output);
        }

        public async Task<List<LookupTableDto>> GetAllNhaCC()
        {
            var filter = await this.nhaCungCapRepository.GetAll().Select(e => new LookupTableDto()
            {
                Id = e.Id,
                DisplayName = e.TenNhaCungCap,
            }).ToListAsync();
            return filter;
        }

        public async Task<List<LookupTableDto>> GetAllLoaiTS()
        {
            var filter = await this.loaiTaiSanRepository.GetAll().Select(e => new LookupTableDto()
            {
                Id = e.Id,
                DisplayName = e.Ten,
            }).ToListAsync();
            return filter;
        }

        public async Task<int> CreateOrEdit(CreateInputDto input)
        {
            int result = 0;

            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            input.TenTS = GlobalFunction.RegexFormat(input.TenTS);
            input.SerialNumber = GlobalFunction.RegexFormat(input.SerialNumber);
            input.ProductNumber = GlobalFunction.RegexFormat(input.ProductNumber);
            input.HangSanXuat = GlobalFunction.RegexFormat(input.HangSanXuat);
            input.GhiChu = GlobalFunction.RegexFormat(input.GhiChu);
            input.NoiDungChotGia = GlobalFunction.RegexFormat(input.NoiDungChotGia);

            input.NgayMua = GlobalFunction.GetDateTime(input.NgayMua);
            input.NgayBaoHanh = GlobalFunction.GetDateTime(input.NgayBaoHanh);
            input.HanSD = GlobalFunction.GetDateTime(input.HanSD);
            input.ThoiDiemChotGia = GlobalFunction.GetDateTime(input.ThoiDiemChotGia);

            if (input.Id == null)
            {
                result = await this.Create(input);
            }
            else
            {
                await this.Update(input);
            }

            return result;
        }

        public async Task<int> Deleted(int input)
        {
            try
            {
                await this.taiSanRepository.DeleteAsync(input);
                return 1;
            }
            catch
            {
                return 2;
            }
        }

        public async Task<GetForViewDto> GetForEdit(int input, bool isView)
        {
            List<TaiSanDinhKemFile> listHinhAnh = new List<TaiSanDinhKemFile>();

            List<TaiSanDinhKemFile> listFile = new List<TaiSanDinhKemFile>();

            var entity = await this.taiSanRepository.GetAll().Where(w => w.Id == (int)input).FirstOrDefaultAsync();
            GetForViewDto output = new GetForViewDto();
            output.TenTS = entity.TenTaiSan;
            output.LoaiTS = entity.LoaiTaiSanId;
            output.SerialNumber = entity.SerialNumber;
            output.ProductNumber = entity.ProductNumber;
            output.NhaCC = entity.NhaCungCapId;
            output.HangSanXuat = entity.HangSanXuat;
            output.NguyenGia = entity.NguyenGia.ToString();
            output.NgayMua = entity.NgayMua;
            output.NgayBaoHanh = entity.NgayHetHanBaoHanh;
            output.HanSD = entity.NgayHetHanSuDung;
            output.MaBarCode = entity.BarCode;
            output.MaRFID = entity.RFIDCode;
            output.MaQRCode = entity.QRCode;
            output.EPCCode = entity.EPCCode;
            output.GhiChu = entity.GhiChu;
            output.NguoiChotGia = this.userRepository.GetAll().Where(w => w.Id == entity.NguoiChotGiaId).Select(s => s.Name).FirstOrDefault();
            output.ThoiDiemChotGia = entity.ThoiDiemChotGia;
            output.GiaCuoiTS = Convert.ToDouble(entity.GiaCuoi);
            output.NoiDungChotGia = entity.NoiDungChotGia;
            output.TinhTrangMaRFID = GlobalModel.TrangThaiSuDung[(bool)entity.TinhTrangSuDungRFIDCode];
            output.TinhTrangMaQRCode = GlobalModel.TrangThaiSuDung[(bool)entity.TinhTrangSuDungQRCode];
            output.TinhTrangMaBarCode = GlobalModel.TrangThaiSuDung[(bool)entity.TinhTrangSuDungBarCode];
            output.MaQuanLy = GlobalFunction.GetMaSuDungTaiSan(entity.TinhTrangSuDungQRCode, entity.TinhTrangSuDungBarCode, entity.TinhTrangSuDungRFIDCode);
            output.TinhTrangRFID = entity.TinhTrangSuDungRFIDCode;
            output.TinhTrangBarCode = entity.TinhTrangSuDungBarCode;
            output.TinhTrangQRCode = entity.TinhTrangSuDungQRCode;
            output.ThoiGianChietKhauHao = entity.ThoiGianTrichKhauHao;
            output.NguonKinhPhiId = entity.NguonKinhPhiId;

            List<int> listMa = new List<int>();

            if (entity.TinhTrangSuDungRFIDCode == true)
            {
                listMa.Add(1);
            }

            if (entity.TinhTrangSuDungBarCode == true)
            {
                listMa.Add(2);
            }

            if (entity.TinhTrangSuDungQRCode == true)
            {
                listMa.Add(3);
            }

            output.DropdownMultiple = string.Join(",", listMa);

            var listFileDinhKem = await this.dinhKemRepository.GetAll().Where(w => w.TaiSanId == input).ToListAsync();

            if (listFileDinhKem.Count > 0)
            {
                foreach (var item in listFileDinhKem)
                {
                    // h/a
                    if (item.PhanLoaiId == 1)
                    {
                        listHinhAnh.Add(item);
                    }
                    else
                    {
                        // file
                        listFile.Add(item);
                    }
                }
            }

            output.ListHinhAnh = listHinhAnh;
            output.ListFile = listFile;

            var viTriTSList = new List<ViTriTSDto>();
            var thongTinSDList = new List<ThongTinSDDto>();
            var thongTinSCBDList = new List<ThongTinSCBDDto>();
            var thongTinHongList = new List<ThongTinHongDto>();
            var thongTinMatList = new List<ThongTinMatDto>();
            var thongTinHuyList = new List<ThongTinHuyDto>();
            var thongTinThanhLyList = new List<ThongTinThanhLyDto>();

            var getPBFromTS = (from ts in this.taiSanRepository.GetAll().Where(w => w.Id == (int)input)
                               from tc in this.toChucRepository.GetAll().Where(w => w.Id == ts.ToChucId).DefaultIfEmpty()
                               select new
                               {
                                   tc.TenToChuc,
                               }).FirstOrDefault();

            var thongTinPhieuTaiSan = (from ptsct in this.phieuTaiSanChiTietRepository.GetAll().Where(w => w.TaiSanId == (int)input)
                                       from pts in this.phieuTaiSanRepository.GetAll().Where(w => w.Id == ptsct.PhieuTaiSanId)
                                       from toChuc in this.toChucRepository.GetAll().Where(w => w.Id == ptsct.ToChucDangQuanLyId).DefaultIfEmpty()
                                       select new
                                       {
                                           pts.PhanLoaiId,
                                           pts.NgayKhaiBao,
                                           pts.NoiDung,
                                           toChuc.TenToChuc,
                                           ptsct,
                                           ptsct.TrangThaiId,
                                       }).ToList();

            // Lấy hết các phiếu có phân loại là Cấp phát, Điều chuyển, Thu hồi
            viTriTSList = thongTinPhieuTaiSan.Where(e => e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.CapPhat
            || e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.DieuChuyen
            || e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.ThuHoi).Select(e => new ViTriTSDto
            {
                HanhDong = GlobalModel.TrangThaiTaiSanSorted[(int)e.PhanLoaiId],
                NgayThucHien = GlobalFunction.GetDateStringFromDate(e.NgayKhaiBao),
                PhongBanQuanLy = e.TenToChuc,
            }).ToList();

            // Lấy hết các phiếu có phân loại là Đang sử dụng
            var taiSanDangSuDungList = thongTinPhieuTaiSan.Where(e => e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanDangSuDung).Select(e => e).ToList();
            int soLuongtaiSanDangSuDung = taiSanDangSuDungList.Count;

            // Duyệt qua từng tài sản đang sử dụng để lấy trạng thái sau nó để lấy đến này
            foreach (var taiSanDangSuDung in taiSanDangSuDungList)
            {
                var index = thongTinPhieuTaiSan.FindIndex(e => e == taiSanDangSuDung);
                var denNgay = index < (soLuongtaiSanDangSuDung - 1) ? GlobalFunction.GetDateStringFromDate(thongTinPhieuTaiSan[index + 1].NgayKhaiBao) : "Hôm nay";
                thongTinSDList.Add(new ThongTinSDDto
                {
                    TuNgay = GlobalFunction.GetDateStringFromDate(taiSanDangSuDung.NgayKhaiBao),
                    DenNgay = denNgay,
                    NDSuDung = taiSanDangSuDung.NoiDung,
                    PhongBanQuanLy = string.IsNullOrEmpty(taiSanDangSuDung.TenToChuc) ? getPBFromTS.TenToChuc : taiSanDangSuDung.TenToChuc,
                });
            }

            // Thông tin sửa chữa bảo dưỡng
            // Lấy hết các phiếu có phân loại là Đang sửa chữa và BD
            var taiSanSCBDList = thongTinPhieuTaiSan.Where(e => e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanSuaChua
            || e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanBaoDuong).Select(e => e).ToList();

            int soLuongtaiSanSCBD = taiSanSCBDList.Count;

            // Duyệt qua từng tài sản đang sử dụng để lấy trạng thái sau nó để lấy đến này
            foreach (var taiSanSCBD in taiSanSCBDList)
            {
                var index = thongTinPhieuTaiSan.FindIndex(e => e == taiSanSCBD);
                var denNgay = index < (soLuongtaiSanSCBD - 1) ? GlobalFunction.GetDateStringFromDate(thongTinPhieuTaiSan[index + 1].NgayKhaiBao) : "Hôm nay";
                thongTinSCBDList.Add(new ThongTinSCBDDto
                {
                    NgayBatDau = GlobalFunction.GetDateStringFromDate(taiSanSCBD.NgayKhaiBao),
                    NgayHoanThanh = denNgay,
                    NoiDung = taiSanSCBD.NoiDung,
                    PhongBanQuanLy = string.IsNullOrEmpty(taiSanSCBD.TenToChuc) ? getPBFromTS.TenToChuc : taiSanSCBD.TenToChuc,
                    KetQua = GlobalModel.TrangThaiSuaChuaBaoDuongSorted[(int)taiSanSCBD.TrangThaiId],
                });
            }

            // Thông tin hỏng
            // Lấy hết các phiếu có phân loại là Hỏng
            var taiSanHongList = thongTinPhieuTaiSan.Where(e => e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanHong).Select(e => e).ToList();

            int soLuongtaiSanHong = taiSanHongList.Count;

            // Duyệt qua từng tài sản đang sử dụng để lấy trạng thái sau nó để lấy đến này
            foreach (var taiSanHong in taiSanHongList)
            {
                var index = thongTinPhieuTaiSan.FindIndex(e => e == taiSanHong);
                var denNgay = index < (soLuongtaiSanHong - 1) ? GlobalFunction.GetDateStringFromDate(thongTinPhieuTaiSan[index + 1].NgayKhaiBao) : "Hôm nay";
                thongTinHongList.Add(new ThongTinHongDto
                {
                    NgayKhaiBao = GlobalFunction.GetDateStringFromDate(taiSanHong.NgayKhaiBao),
                    NoiDung = taiSanHong.NoiDung,
                    PhongBanQuanLy = string.IsNullOrEmpty(taiSanHong.TenToChuc) ? getPBFromTS.TenToChuc : taiSanHong.TenToChuc,
                });
            }

            // Thông tin mất
            // Lấy hết các phiếu có phân loại là mất
            var taiSanMatList = thongTinPhieuTaiSan.Where(e => e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanMat).Select(e => e).ToList();

            int soLuongtaiSanMat = taiSanMatList.Count;

            // Duyệt qua từng tài sản đang sử dụng để lấy trạng thái sau nó để lấy đến này
            foreach (var taiSanMat in taiSanMatList)
            {
                var index = thongTinPhieuTaiSan.FindIndex(e => e == taiSanMat);
                var denNgay = index < (soLuongtaiSanMat - 1) ? GlobalFunction.GetDateStringFromDate(thongTinPhieuTaiSan[index + 1].NgayKhaiBao) : "Hôm nay";
                thongTinMatList.Add(new ThongTinMatDto
                {
                    NgayKhaiBao = GlobalFunction.GetDateStringFromDate(taiSanMat.NgayKhaiBao),
                    NoiDung = taiSanMat.NoiDung,
                    PhongBanQuanLy = string.IsNullOrEmpty(taiSanMat.TenToChuc) ? getPBFromTS.TenToChuc : taiSanMat.TenToChuc,
                });
            }

            // Thông tin hủy
            // Lấy hết các phiếu có phân loại là hủy
            var taiSanHuyList = thongTinPhieuTaiSan.Where(e => e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanHuy).Select(e => e).ToList();

            int soLuongtaiSanHuy = taiSanHuyList.Count;

            // Duyệt qua từng tài sản đang sử dụng để lấy trạng thái sau nó để lấy đến này
            foreach (var taiSanHuy in taiSanHuyList)
            {
                var index = thongTinPhieuTaiSan.FindIndex(e => e == taiSanHuy);
                var denNgay = index < (soLuongtaiSanHuy - 1) ? GlobalFunction.GetDateStringFromDate(thongTinPhieuTaiSan[index + 1].NgayKhaiBao) : "Hôm nay";
                thongTinHuyList.Add(new ThongTinHuyDto
                {
                    NgayKhaiBao = GlobalFunction.GetDateStringFromDate(taiSanHuy.NgayKhaiBao),
                    NoiDung = taiSanHuy.NoiDung,
                    PhongBanQuanLy = string.IsNullOrEmpty(taiSanHuy.TenToChuc) ? getPBFromTS.TenToChuc : taiSanHuy.TenToChuc,
                });
            }

            // Thông tin thanh lý
            // Lấy hết các phiếu có phân loại là thanh lý
            var taiSanThanhLyList = thongTinPhieuTaiSan.Where(e => e.PhanLoaiId == (int)GlobalConst.TrangThaiTaiSanConst.TaiSanThanhLy).Select(e => e).ToList();

            int soLuongtaiSanThanhLy = taiSanThanhLyList.Count;

            // Duyệt qua từng tài sản đang sử dụng để lấy trạng thái sau nó để lấy đến này
            foreach (var taiSanThanhLy in taiSanThanhLyList)
            {
                var index = thongTinPhieuTaiSan.FindIndex(e => e == taiSanThanhLy);
                var denNgay = index < (soLuongtaiSanThanhLy - 1) ? GlobalFunction.GetDateStringFromDate(thongTinPhieuTaiSan[index + 1].NgayKhaiBao) : "Hôm nay";
                thongTinThanhLyList.Add(new ThongTinThanhLyDto
                {
                    NgayKhaiBao = GlobalFunction.GetDateStringFromDate(taiSanThanhLy.NgayKhaiBao),
                    NoiDung = taiSanThanhLy.NoiDung,
                    PhongBanQuanLy = string.IsNullOrEmpty(taiSanThanhLy.TenToChuc) ? getPBFromTS.TenToChuc : taiSanThanhLy.TenToChuc,
                });
            }

            output.ListViTriTS = viTriTSList;
            output.ListThongTinSD = thongTinSDList;
            output.ListThongTinSCBD = thongTinSCBDList;
            output.ListThongTinHuy = thongTinHuyList;
            output.ListThongTinHong = thongTinHongList;
            output.ListThongTinMat = thongTinMatList;
            output.ListThongTinThanhLy = thongTinThanhLyList;

            return output;
        }

        public async Task XoaList(List<int> input)
        {
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            try
            {
                foreach (var item in input)
                {
                    await this.taiSanRepository.DeleteAsync((int)item);
                }
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
            }
        }

        public string GenCode(int idLoaiTS, int idTS)
        {
            string result = string.Empty;
            string maHexPBCha = string.Empty;
            string maHexPBDangNhap = string.Empty;

            string stringLoaiTS = GlobalFunction.ConverDecimalToHexa(idLoaiTS, 5);
            string stringTS = GlobalFunction.ConverDecimalToHexa(idTS, 15);
            var pB = (from user in this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId)
                      from tc in this.toChucRepository.GetAll().Where(w => w.Id == user.ToChucId)
                      select tc).First();

            // Nếu là phòng ban cấp 1
            if (pB.TrucThuocToChucId == null)
            {
                maHexPBCha = pB.MaHexa;
                maHexPBDangNhap = "00";
            }
            else
            {
                // Nếu là phòng ban cấp 2
                maHexPBDangNhap = pB.MaHexa;
                maHexPBCha = this.toChucRepository.Get((int)pB.TrucThuocToChucId).MaHexa;
            }

            result = maHexPBCha + maHexPBDangNhap + stringLoaiTS + stringTS;
            return result;
        }

        public async Task<string> GetUserDangNhap()
        {
            var query = await this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId).Select(s => s.Name).FirstOrDefaultAsync();
            return query;
        }

        public async Task<FileDto> DownloadFileUpload(string linkFile)
        {
            if (string.IsNullOrEmpty(linkFile))
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            var fileName = linkFile.Split(@"\").Last();
            var path = this.appFolders.ToanBoTSUploadFolder + linkFile.Replace(fileName, string.Empty);

            // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
            // _appFolders.TempFileDownloadFolder : Không được sửa
            return await GlobalFunction.DownloadFileMau(fileName, path, this.appFolders.TempFileDownloadFolder);
        }

        public async Task<FileDto> ExportToExcel(GetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // Lấy danh sách cần xuất excel
            var list = await this.GetAll(input);
            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ToanBoTaiSan");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[1, 1].Value = "Mã tài sản (EPC)";
            worksheet.Cells[1, 2].Value = "Tên tài sản";
            worksheet.Cells[1, 3].Value = "Loại tài sản";
            worksheet.Cells[1, 4].Value = "Serial Number";
            worksheet.Cells[1, 5].Value = "Nhà cung cấp";
            worksheet.Cells[1, 6].Value = "Ngày mua";
            worksheet.Cells[1, 7].Value = "Nguyên giá";
            worksheet.Cells[1, 8].Value = "Đơn vị quản lý";
            worksheet.Cells[1, 9].Value = "Trạng thái";
            worksheet.Cells[1, 10].Value = "Mã sử dụng";
            worksheet.Cells[1, 11].Value = "Vị trí tài sản";

            // Bôi đậm header
            using (ExcelRange r = worksheet.Cells[1, 1, 1, 11])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 2;
            list.Items.ToList().ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = item.MaEPC;
                worksheet.Cells[rowNumber, 2].Value = item.TenTS;
                worksheet.Cells[rowNumber, 3].Value = item.LoaiTS;
                worksheet.Cells[rowNumber, 4].Value = item.SerialNumber;
                worksheet.Cells[rowNumber, 5].Value = item.NhaCungCap;
                worksheet.Cells[rowNumber, 6].Value = item.NgayMua;
                worksheet.Cells[rowNumber, 7].Value = item.NguyenGia;
                worksheet.Cells[rowNumber, 8].Value = item.PhongBanQL;
                worksheet.Cells[rowNumber, 9].Value = item.TrangThai;
                worksheet.Cells[rowNumber, 10].Value = item.MaSD;
                worksheet.Cells[rowNumber, 11].Value = item.ViTriTS;
                rowNumber++;
            });

            // Cho các ô rộng theo dữ liệu
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // Tên file
            var fileName = string.Join(".", new string[] { "Danh sách toàn bộ tài sản", "xlsx" });

            // Lưu file vào server
            using (var stream = new MemoryStream())
            {
                package.SaveAs(stream);
            }

            var file = new FileDto(fileName, MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
            var filePath = Path.Combine(this.appFolders.TempFileDownloadFolder, file.FileToken);
            package.SaveAs(new FileInfo(filePath));
            return file;
        }

        public async Task<FileDto> DownloadFileMau()
        {
            string fileName = string.Format("ToanBoTaiSanImport.xlsx");
            try
            {
                // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
                // _appFolders.TempFileDownloadFolder : Không được sửa
                return await GlobalFunction.DownloadFileMau(fileName, this.appFolders.ToanBoTSFileDownloadFolder, this.appFolders.TempFileDownloadFolder);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Có lỗi: " + ex.Message);
            }
        }

        public async Task<string> ImportFileExcel(string filePath)
        {
            StringBuilder returnMessage = new StringBuilder();
            returnMessage.Append("Kết quả nhập file:");
            ReadFromExcelDto<CreateInputDto> readResult = new ReadFromExcelDto<CreateInputDto>();

            // Không tìm thấy file
            if (!File.Exists(filePath))
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.FileNotFound;
            }

            // Đọc hết file excel
            var data = await GlobalFunction.ReadFromExcel(filePath);

            // Không có dữ liệu
            if (data.Count <= 0)
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.CantReadData;
            }
            else
            {
                // Đọc lần lượt từng dòng
                for (int i = 0; i < data.Count; i++)
                {
                    try
                    {
                        string ten = GlobalFunction.RegexFormat(data[i][0]);
                        string loai = GlobalFunction.RegexFormat(data[i][1]);
                        string maSD = GlobalFunction.RegexFormat(data[i][2]);
                        string serial = GlobalFunction.RegexFormat(data[i][3]);
                        string product = GlobalFunction.RegexFormat(data[i][4]);
                        string nhaCC = GlobalFunction.RegexFormat(data[i][5]);
                        string hangSX = GlobalFunction.RegexFormat(data[i][6]);
                        string nguyenGia = GlobalFunction.RegexFormat(data[i][7]);
                        string ngayMua = GlobalFunction.RegexFormat(data[i][8]);
                        string hanBH = GlobalFunction.RegexFormat(data[i][9]);
                        string hanSD = GlobalFunction.RegexFormat(data[i][10]);
                        string ghiChu = GlobalFunction.RegexFormat(data[i][11]);
                        string giaCuoi = GlobalFunction.RegexFormat(data[i][12]);
                        string noiDungChotGia = GlobalFunction.RegexFormat(data[i][13]);
                        var checkLoaiTS = await this.loaiTaiSanRepository.FirstOrDefaultAsync(f => f.Ten == loai);

                        string[] maQuanLy = maSD != null ? maSD.Split(',') : Array.Empty<string>();
                        List<string> maQL = new List<string>();
                        string resultMa = string.Empty;
                        if (maQuanLy.Contains("RFID"))
                        {
                            maQL.Add("1");
                        }

                        if (maQuanLy.Contains("Barcode"))
                        {
                            maQL.Add("2");
                        }

                        if (maQuanLy.Contains("QRcode"))
                        {
                            maQL.Add("3");
                        }

                        if (maQL.Count > 0)
                        {
                            resultMa = string.Join(",", maQL);
                        }

                        if (checkLoaiTS != null)
                        {
                            long? nguoiChotGia;
                            DateTime? thoiDienChotGia = null;
                            string noDungCG = null;
                            if (giaCuoi != null)
                            {
                                noDungCG = noiDungChotGia;
                                nguoiChotGia = (long)this.AbpSession.UserId;
                                thoiDienChotGia = DateTime.Now;
                            }
                            else
                            {
                                noDungCG = null;
                                nguoiChotGia = null;
                                thoiDienChotGia = null;
                            }

                            var create = new CreateInputDto();
                            create.TenTS = ten;
                            create.LoaiTS = this.loaiTaiSanRepository.GetAll().Where(w => w.Ten == loai).Select(s => s.Id).FirstOrDefault();
                            create.SerialNumber = serial;
                            create.ProductNumber = product;
                            create.NhaCC = this.nhaCungCapRepository.GetAll().Where(w => w.TenNhaCungCap == nhaCC).Select(s => s.Id).FirstOrDefault();
                            create.HangSanXuat = hangSX;
                            create.NguyenGia = !string.IsNullOrEmpty(nguyenGia) ? float.Parse(nguyenGia) : (float?)null;
                            create.NgayMua = GlobalFunction.ConvertStringToDateTime(ngayMua);
                            create.NgayBaoHanh = GlobalFunction.ConvertStringToDateTime(hanBH);
                            create.HanSD = GlobalFunction.ConvertStringToDateTime(hanSD);
                            create.GhiChu = ghiChu;
                            create.GiaCuoiTS = !string.IsNullOrEmpty(giaCuoi) ? float.Parse(giaCuoi) : (float?)null;
                            create.NoiDungChotGia = noDungCG;
                            create.NguoiChotGia = nguoiChotGia;
                            create.ThoiDiemChotGia = thoiDienChotGia;
                            create.DropdownMultiple = resultMa;

                            // Nếu không bị trùng
                            if (await this.CreateOrEdit(create) != 0)
                            {
                                // Đánh dấu các bản ghi thêm thành công
                                readResult.ListResult.Add(create);
                            }
                            else
                            {
                                // Đánh dấu các bản ghi lỗi
                                readResult.ListErrorRow.Add(data[i]);
                                readResult.ListErrorRowIndex.Add(i + 1);
                            }
                        }
                        else
                        {
                            // Đánh dấu các bản ghi lỗi
                            readResult.ListErrorRow.Add(data[i]);
                            readResult.ListErrorRowIndex.Add(i + 1);
                        }
                    }
                    catch
                    {
                        // Đánh dấu các bản ghi lỗi
                        readResult.ListErrorRow.Add(data[i]);
                        readResult.ListErrorRowIndex.Add(i + 1);
                    }
                }
            }

            // Thông tin import
            readResult.ErrorMessage = GlobalModel.ReadExcelResultCodeSorted[readResult.ResultCode];

            // Nếu đọc file thất bại
            if (readResult.ResultCode != 200)
            {
                return readResult.ErrorMessage;
            }
            else
            {
                // Đọc file thành công
                // Trả kết quả import
                returnMessage.Append(string.Format("\r\n\u00A0- Tổng ghi: {0}", readResult.ListResult.Count + readResult.ListErrorRow.Count));
                returnMessage.Append(string.Format("\r\n\u00A0- Số bản ghi thành công: {0}", readResult.ListResult.Count));
                returnMessage.Append(string.Format("\r\n\u00A0- Số bản ghi thất bại: {0}", readResult.ListErrorRow.Count));
                if (readResult.ListErrorRowIndex.Count > 0)
                {
                    returnMessage.Append(string.Format("\r\n\u00A0- Các dòng thất bại: {0}", string.Join(",", readResult.ListErrorRowIndex)));
                }
            }

            return returnMessage.ToString();
        }

        private async Task<int> Create(CreateInputDto input)
        {
            int? pBId = (from user in this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId)
                         from tc in this.toChucRepository.GetAll().Where(w => w.Id == user.ToChucId)
                         select tc.Id).FirstOrDefault();

            string[] maQuanLy = input.DropdownMultiple != null ? input.DropdownMultiple.Split(',') : Array.Empty<string>();
            var create = new TaiSan
            {
                TenTaiSan = input.TenTS,
                LoaiTaiSanId = input.LoaiTS,
                HangSanXuat = input.HangSanXuat,
                GiaCuoi = input.GiaCuoiTS != null ? input.GiaCuoiTS : null,
                GhiChu = input.GhiChu,
                NgayMua = input.NgayMua != null ? input.NgayMua : null,
                NgayMuaStr = GlobalFunction.GetDateStringFromDate(input.NgayMua),
                NguyenGia = input.NguyenGia != null ? input.NguyenGia : null,
                NguyenGiaStr = input.NguyenGia != null ? input.NguyenGia.ToString() : string.Empty,
                NoiDungChotGia = input.NoiDungChotGia,
                SerialNumber = input.SerialNumber,
                NhaCungCapId = input.NhaCC != null ? input.NhaCC : null,
                NguoiChotGiaId = input.NguoiChotGia,
                ProductNumber = input.ProductNumber,
                ThoiDiemChotGia = input.ThoiDiemChotGia,
                NgayHetHanSuDung = input.HanSD != null ? input.HanSD : null,
                NgayHetHanBaoHanh = input.NgayBaoHanh != null ? input.NgayBaoHanh : null,
                TrangThaiId = (int)GlobalConst.TrangThaiTaiSanConst.KhoiTao,
                ToChucId = pBId,
                ThoiGianTrichKhauHao = input.ThoiGianChietKhauHao,
                TinhTrangSuDungRFIDCode = maQuanLy.Contains("1"),
                TinhTrangSuDungBarCode = maQuanLy.Contains("2"),
                TinhTrangSuDungQRCode = maQuanLy.Contains("3"),
                NguonKinhPhiId = input.NguonKinhPhiId,
            };

            List<TaiSanDinhKemFile> taiSanDinhKemFileList = new List<TaiSanDinhKemFile>();

            if (input.ListHA != null)
            {
                taiSanDinhKemFileList.AddRange(input.ListHA.Select(e => new TaiSanDinhKemFile
                {
                    LinkFile = e.LinkFile,
                    TenFile = e.TenFile,
                    PhanLoaiId = 1,
                }));
            }

            if (input.ListFile != null)
            {
                taiSanDinhKemFileList.AddRange(input.ListFile.Select(e => new TaiSanDinhKemFile
                {
                    LinkFile = e.LinkFile,
                    TenFile = e.TenFile,
                    PhanLoaiId = 2,
                }));
            }

            create.TaiSanDinhKemFileList = taiSanDinhKemFileList;
            var idResult = await this.taiSanRepository.InsertAndGetIdAsync(create);
            var ma = this.GenCode((int)input.LoaiTS, idResult);

            var query = await this.taiSanRepository.GetAsync(idResult);
            query.EPCCode = query.RFIDCode = query.QRCode = query.BarCode = ma;

            return idResult;
        }

        private async Task Update(CreateInputDto input)
        {
            var update = await this.taiSanRepository.FirstOrDefaultAsync(f => f.Id == input.Id);
            update.TenTaiSan = input.TenTS;
            update.LoaiTaiSanId = input.LoaiTS;
            update.HangSanXuat = input.HangSanXuat;
            update.GiaCuoi = input.GiaCuoiTS;
            update.GhiChu = input.GhiChu;
            update.NgayMua = input.NgayMua != null ? (DateTime)input.NgayMua : (DateTime?)null;
            update.NguyenGia = input.NguyenGia;
            update.NoiDungChotGia = input.NoiDungChotGia;
            update.SerialNumber = input.SerialNumber;
            update.NhaCungCapId = input.NhaCC;
            update.NguoiChotGiaId = input.NguoiChotGia;
            update.ProductNumber = input.ProductNumber;
            update.ThoiDiemChotGia = input.ThoiDiemChotGia;
            update.NgayHetHanSuDung = input.HanSD != null ? (DateTime)input.HanSD : (DateTime?)null;
            update.NgayHetHanBaoHanh = input.NgayBaoHanh != null ? (DateTime)input.NgayBaoHanh : (DateTime?)null;
            update.NguyenGiaStr = input.NguyenGia.ToString();
            update.NgayMuaStr = GlobalFunction.GetDateStringFromDate(input.NgayMua);
            update.ThoiGianTrichKhauHao = input.ThoiGianChietKhauHao;
            update.NguonKinhPhiId = input.NguonKinhPhiId;
            await this.dinhKemRepository.DeleteAsync(w => w.TaiSanId == (int)input.Id);

            string[] maQuanLy = input.DropdownMultiple != null ? input.DropdownMultiple.Split(',') : Array.Empty<string>();
            update.TinhTrangSuDungRFIDCode = maQuanLy.Contains("1");
            update.TinhTrangSuDungBarCode = maQuanLy.Contains("2");
            update.TinhTrangSuDungQRCode = maQuanLy.Contains("3");

            foreach (var item in input.ListHA)
            {
                var file = new TaiSanDinhKemFile
                {
                    TaiSanId = input.Id,
                    LinkFile = item.LinkFile,
                    TenFile = item.TenFile,
                    PhanLoaiId = 1,
                };

                await this.dinhKemRepository.InsertAsync(file);
            }

            foreach (var item in input.ListFile)
            {
                var file = new TaiSanDinhKemFile
                {
                    TaiSanId = input.Id,
                    LinkFile = item.LinkFile,
                    TenFile = item.TenFile,
                    PhanLoaiId = 2,
                };

                await this.dinhKemRepository.InsertAsync(file);
            }

            await this.taiSanRepository.UpdateAsync(update);
        }
    }
}
