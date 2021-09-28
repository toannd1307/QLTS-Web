namespace MyProject.QuanLyAngTenRFID
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
    using MyProject.QuanLyDauDocTheRFID.Dtos;
    using MyProject.QuanLyTaiSan.QuanLyToanBoTaiSan.Dtos;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class AngTenRFIDAppService : MyProjectAppServiceBase, IAngTenRFIDAppService
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

        public AngTenRFIDAppService(
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

        public async Task<PagedResultDto<GetAllOutputDto>> GetAll(InputRFIDDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            input.TenTS = GlobalFunction.RegexFormat(input.TenTS);
            #endregion
            var chuaSuDung = new List<int> { 0, 1, 2, 3 };
            var suaChuaBaoDuong = new List<int> { 5, 6 };

            if (input.PhongBanSuDung == null || input.PhongBanSuDung.Count == 0)
            {
                input.PhongBanSuDung = new List<int?> { -1 };
            }

            IQueryable<TaiSan> filter = null;

            // thiếu getAll dsTS theo phòng ban con
            filter = this.taiSanRepository.GetAll().Where(w => input.PhongBanSuDung.Contains((int)w.ToChucId))
                .WhereIf(input.PhongBanSuDung != null && input.PhongBanSuDung.Count > 0, w => input.PhongBanSuDung.Contains((int)w.ToChucId))
                .Where(w => w.LoaiTaiSanId == (int)GlobalConst.LoaiTaiSanConst.CoDinh && (w.TenTaiSan.Contains(input.TenTS) || input.TenTS == null) && (w.TenTaiSan.Contains(input.TenTS) || input.TenTS == null))
                .Where(w => input.TinhTrangSuDung == null || (input.TinhTrangSuDung == 3 ? chuaSuDung.Contains((int)w.TrangThaiId) : input.TinhTrangSuDung == 5 ? suaChuaBaoDuong.Contains((int)w.TrangThaiId) : w.TrangThaiId == input.TinhTrangSuDung));

            var query = from o in filter
                        from ncc in this.nhaCungCapRepository.GetAll().Where(w => w.Id == o.NhaCungCapId).DefaultIfEmpty()
                        from tochuc in this.toChucRepository.GetAll().Where(w => w.Id == o.ToChucId)
                        from phongBanCha in this.toChucRepository.GetAll().Where(w => w.Id == tochuc.TrucThuocToChucId).DefaultIfEmpty()
                        select new GetAllOutputDto()
                        {
                            Id = o.Id,
                            TenTS = o.TenTaiSan,
                            SerialNumber = o.SerialNumber,
                            ProductNumber = o.ProductNumber,
                            TinhTrangSuDung = o.TrangThaiId != null ? GlobalModel.TrangThaiTaiSanHienThiSorted[(int)o.TrangThaiId] : string.Empty,
                            NhaCungCap = ncc.TenNhaCungCap,
                            NgayMua = o.NgayMua.HasValue ? o.NgayMua.Value.ToString("dd/MM/yyyy") : string.Empty,
                            PhongBanQL = (tochuc.TrucThuocToChucId > 1 ? (phongBanCha.MaToChuc + " - ") : string.Empty) + tochuc.TenToChuc,
                            NgayTao = o.CreationTime,
                            TrangThaiId = o.TrangThaiId,
                            NgayMuaDateTime = o.NgayMua,
                            MaEPC = o.EPCCode,
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
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            input.TenTS = GlobalFunction.RegexFormat(input.TenTS);
            input.SerialNumber = GlobalFunction.RegexFormat(input.SerialNumber);
            input.ProductNumber = GlobalFunction.RegexFormat(input.ProductNumber);
            input.ReaderMACId = GlobalFunction.RegexFormat(input.ReaderMACId);
            input.HangSanXuat = GlobalFunction.RegexFormat(input.HangSanXuat);
            input.GhiChu = GlobalFunction.RegexFormat(input.GhiChu);
            input.NoiDungChotGia = GlobalFunction.RegexFormat(input.NoiDungChotGia);
            input.NgayBaoHanh = GlobalFunction.GetDateTime(input.NgayBaoHanh);
            input.NgayMua = GlobalFunction.GetDateTime(input.NgayMua);
            input.HanSD = GlobalFunction.GetDateTime(input.HanSD);

            if (input.Id == null)
            {
                await this.Create(input);
            }
            else
            {
                await this.Update(input);
            }

            return 0;
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
            GetForViewDto output = new GetForViewDto
            {
                TenTS = entity.TenTaiSan,
                LoaiTS = entity.LoaiTaiSanId,
                SerialNumber = entity.SerialNumber,
                ProductNumber = entity.ProductNumber,
                ReaderMacId = entity.ReaderMacId,
                NhaCC = entity.NhaCungCapId,
                HangSanXuat = entity.HangSanXuat,
                NguyenGia = entity.NguyenGia.ToString(),
                NgayMua = entity.NgayMua,
                NgayBaoHanh = entity.NgayHetHanBaoHanh,
                HanSD = entity.NgayHetHanSuDung,
                MaBarCode = entity.BarCode,
                MaRFID = entity.RFIDCode,
                MaQRCode = entity.QRCode,
                EPCCode = entity.EPCCode,
                GhiChu = entity.GhiChu,
                NoiDungChotGia = entity.NoiDungChotGia,
            };

            return output;
        }

        public async Task XoaList(List<int> input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
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

        public string GenCode(string maLoaiTS, int idTS)
        {
            var pB = (from user in this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId)
                      from tc in this.toChucRepository.GetAll().Where(w => w.Id == user.ToChucId)
                      select tc).FirstOrDefault();

            string result = string.Empty;

            string maHexPBDangNhap = pB.MaHexa;
            string maHexPBCha = this.toChucRepository.GetAll().Where(w => w.Id == pB.TrucThuocToChucId).Select(s => s.MaHexa).FirstOrDefault();

            // Convert integer as a hex in a string variable
            string stringTS = idTS.ToString("X");
            stringTS = stringTS.PadLeft(15, '0');

            int loaiTSId = this.loaiTaiSanRepository.GetAll().Where(w => w.Ma == maLoaiTS).Select(s => s.Id).FirstOrDefault();
            string stringLoaiTS = loaiTSId.ToString("X");
            stringLoaiTS = stringLoaiTS.PadLeft(5, '0');

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

        public async Task<string> ImportFileExcel(string filePath)
        {
            string error = string.Empty;
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
            if (data.Count <= 0 || data[0].Count != 11)
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.CantReadData;
            }
            else
            {
                // Đọc lần lượt từng dòng
                for (int i = 0; i < data.Count; i++)
                {
                    string tenDauLoc = GlobalFunction.RegexFormat(data[i][0]);
                    string seriNumber = GlobalFunction.RegexFormat(data[i][1]);
                    string productNumber = GlobalFunction.RegexFormat(data[i][2]);
                    string readerMACId = GlobalFunction.RegexFormat(data[i][3]);
                    string ncc = GlobalFunction.RegexFormat(data[i][4]);
                    string hangSanXuat = GlobalFunction.RegexFormat(data[i][5]);
                    string nguyenGia = GlobalFunction.RegexFormat(data[i][6]);
                    string ngayMua = GlobalFunction.RegexFormat(data[i][7]);
                    string ngayHetHanBH = GlobalFunction.RegexFormat(data[i][8]);
                    string ngayHetHanSuDung = GlobalFunction.RegexFormat(data[i][9]);
                    string ghiChu = GlobalFunction.RegexFormat(data[i][10]);

                    List<NhaCungCap> listNhaCC = this.nhaCungCapRepository.GetAll().ToList();
                    if (string.IsNullOrEmpty(value: tenDauLoc))
                    {
                        error += "+ Tên Đầu đọc không được để trống " + ":" + " Dòng " + (i + 1) + "\n";
                    }

                    if (string.IsNullOrEmpty(value: readerMACId))
                    {
                        error += "+ ReaderMACId không được để trống " + ":" + " Dòng " + (i + 1) + "\n";
                    }

                    var ncc1 = new NhaCungCap();
                    if (!string.IsNullOrEmpty(value: ncc))
                    {
                        ncc1 = listNhaCC.Find(w => w.TenNhaCungCap == ncc);
                        if (ncc1 == null)
                        {
                            error += "+ Nhà cung cấp không tồn tại " + ":" + " Dòng " + (i + 1) + "\n";
                        }
                    }

                    DateTime checkDate;
                    if (!string.IsNullOrEmpty(value: ngayMua) && !DateTime.TryParse(ngayMua, out checkDate))
                    {
                        error += "+ Ngày mua không đúng định dạng" + ":" + " Dòng " + (i + 1) + "\n";
                    }

                    if (!string.IsNullOrEmpty(value: ngayHetHanBH) && !DateTime.TryParse(ngayHetHanBH, out checkDate))
                    {
                        error += "+ Ngày hết hạn bảo hành không đúng định dạng" + ":" + " Dòng " + (i + 1) + "\n";
                    }

                    if (!string.IsNullOrEmpty(value: ngayHetHanSuDung) && !DateTime.TryParse(ngayHetHanSuDung, out checkDate))
                    {
                        error += "+ Ngày hết hạn sử dụng không đúng định dạng" + ":" + " Dòng " + (i + 1) + "\n";
                    }

                    if (string.IsNullOrEmpty(error))
                    {
                        var create = new CreateInputDto
                        {
                            TenTS = tenDauLoc,
                            LoaiTS = (int)GlobalConst.LoaiTaiSanConst.CoDinh,
                            SerialNumber = seriNumber,
                            ProductNumber = productNumber,
                            ReaderMACId = readerMACId,
                            NhaCC = ncc1.Id != 0 ? ncc1.Id : (int?)null,
                            HangSanXuat = hangSanXuat,
                            NguyenGia = !string.IsNullOrEmpty(value: nguyenGia) ? float.Parse(s: nguyenGia) : (float?)null,
                            NgayMua = !string.IsNullOrEmpty(value: ngayMua) ? DateTime.Parse(s: ngayMua) : (DateTime?)null,
                            NgayBaoHanh = !string.IsNullOrEmpty(value: ngayHetHanBH) ? DateTime.Parse(s: ngayHetHanBH) : (DateTime?)null,
                            HanSD = !string.IsNullOrEmpty(value: ngayHetHanSuDung) ? DateTime.Parse(s: ngayHetHanSuDung) : (DateTime?)null,
                            GhiChu = ghiChu,
                        };
                        if (await this.CreateOrEdit(create) == 0)
                        {
                            // Đánh dấu các bản ghi thêm thành công
                            readResult.ListResult.Add(create);
                        }
                    }
                    else
                    {
                        readResult.ErrorMessage = error;
                        List<string> a = new List<string>
                        {
                            error,
                        };
                        readResult.ListErrorRow.Add(a);
                    }
                }
            }

            // Thông tin import

            // Nếu đọc file thất bại
            if (readResult.ResultCode != 200)
            {
                returnMessage.Append("File sai định dạng!");
                return returnMessage.ToString();
            }
            else
            {
                // Đọc file thành công
                // Trả kết quả import
                returnMessage.Append(string.Format("\r\n\u00A0- Tổng ghi: {0}", readResult.ListResult.Count + readResult.ListErrorRow.Count));
                returnMessage.Append(string.Format("\r\n\u00A0- Số bản ghi thành công: {0}", readResult.ListResult.Count));
                returnMessage.Append(string.Format("\r\n\u00A0- Số bản ghi thất bại: {0}", readResult.ListErrorRow.Count) + "\n" + error);
            }

            return returnMessage.ToString();
        }

        ///// <summary>
        ///// Tải file mẫu cho import.
        ///// </summary>
        ///// <returns>File mẫu.</returns>
        public async Task<FileDto> DownloadFileMau()
        {
            string fileName = string.Format("DauDocCoDinh.xlsx");
            try
            {
                // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
                // _appFolders.TempFileDownloadFolder : Không được sửa
                return await GlobalFunction.DownloadFileMau(fileName, this.appFolders.DauLocFileDownloadFolder, this.appFolders.TempFileDownloadFolder);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Có lỗi: " + ex.Message);
            }
        }

        public async Task<FileDto> ExportToExcel(InputRFIDDto input)
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
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sheet1");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[1, 1].Value = "Tên đầu đọc";
            worksheet.Cells[1, 2].Value = "Đơn vị quản lý";
            worksheet.Cells[1, 3].Value = "Tình trạng sử dụng";
            worksheet.Cells[1, 4].Value = "Ngày mua";
            worksheet.Cells[1, 5].Value = "Nhà cung cấp";
            worksheet.Cells[1, 6].Value = "S/N(Serial Number)";
            worksheet.Cells[1, 7].Value = "P/N(Product Number)";

            // Bôi đậm header
            using (ExcelRange r = worksheet.Cells[1, 1, 1, 7])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 2;
            list.Items.ToList().ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = item.TenTS;
                worksheet.Cells[rowNumber, 2].Value = item.PhongBanQL;
                worksheet.Cells[rowNumber, 3].Value = item.TinhTrangSuDung;
                worksheet.Cells[rowNumber, 4].Value = item.NgayMua;
                worksheet.Cells[rowNumber, 5].Value = item.NhaCungCap;
                worksheet.Cells[rowNumber, 6].Value = item.SerialNumber;
                worksheet.Cells[rowNumber, 7].Value = item.ProductNumber;
                rowNumber++;
            });

            // Cho các ô rộng theo dữ liệu
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // Tên file
            var fileName = string.Join(".", new string[] { "Danh sách đầu đọc cố định", "xlsx" });

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

        private async Task Create(CreateInputDto input)
        {
            string maLoaiTS = await this.loaiTaiSanRepository.GetAll().Where(w => w.Id == input.LoaiTS).Select(s => s.Ma).FirstOrDefaultAsync();

            var create = new TaiSan
            {
                TenTaiSan = input.TenTS,
                LoaiTaiSanId = input.LoaiTS,
                HangSanXuat = input.HangSanXuat,
                GhiChu = input.GhiChu,
                ToChucId = this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId).Select(s => s.ToChucId).FirstOrDefault(),
                NgayMua = input.NgayMua != null ? input.NgayMua : null,
                NguyenGia = input.NguyenGia != null ? input.NguyenGia : null,
                SerialNumber = input.SerialNumber,
                NhaCungCapId = input.NhaCC != null ? input.NhaCC : null,
                ProductNumber = input.ProductNumber,
                ReaderMacId = input.ReaderMACId,
                ThoiDiemChotGia = input.ThoiDiemChotGia,
                NgayHetHanSuDung = input.HanSD != null ? input.HanSD : null,
                NgayHetHanBaoHanh = input.NgayBaoHanh != null ? input.NgayBaoHanh : null,
                TinhTrangSuDungBarCode = false,
                TinhTrangSuDungRFIDCode = false,
                TinhTrangSuDungQRCode = false,
                TrangThaiId = (int)GlobalConst.TrangThaiTaiSanConst.KhoiTao,
            };
            var idResult = await this.taiSanRepository.InsertAndGetIdAsync(create);
            var ma = this.GenCode(maLoaiTS, idResult);

            var query = await this.taiSanRepository.GetAll().Where(w => w.Id == idResult).FirstOrDefaultAsync();
            query.EPCCode = ma;
            query.RFIDCode = ma;
            query.QRCode = ma;
            query.BarCode = ma;
            await this.taiSanRepository.UpdateAsync(query);
        }

        private async Task Update(CreateInputDto input)
        {
            var update = await this.taiSanRepository.FirstOrDefaultAsync(f => f.Id == input.Id);
            update.TenTaiSan = input.TenTS;
            update.LoaiTaiSanId = input.LoaiTS;
            update.HangSanXuat = input.HangSanXuat;
            update.GiaCuoi = input.GiaCuoiTS ?? null;
            update.GhiChu = input.GhiChu;
            update.NgayMua = input.NgayMua ?? null;
            update.NguyenGia = input.NguyenGia ?? null;
            update.NoiDungChotGia = input.NoiDungChotGia;
            update.SerialNumber = input.SerialNumber;
            update.ProductNumber = input.ProductNumber;
            update.ReaderMacId = input.ReaderMACId;
            update.NhaCungCapId = input.NhaCC ?? null;
            update.NgayHetHanSuDung = input.HanSD ?? null;
            update.NgayHetHanBaoHanh = input.NgayBaoHanh ?? null;
            await this.taiSanRepository.UpdateAsync(update);
        }
    }
}
