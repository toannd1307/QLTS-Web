namespace MyProject.BaoCaoThongTinThietBiRFID
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Abp.Domain.Repositories;
    using DbEntities;
    using MyProject.Authorization;
    using MyProject.Authorization.Users;
    using MyProject.Data;
    using MyProject.Global;
    using MyProject.Net.MimeTypes;
    using MyProject.QuanLyTaiSan.QuanLyTaiSanSuaChuaBaoDuong.Dtos;
    using OfficeOpenXml;
    using OfficeOpenXml.Drawing.Chart;
    using OfficeOpenXml.Style;

    public class BaoCaoThongTinThietBiRFIDAppService : MyProjectAppServiceBase, IBaoCaoThongTinThietBiRFIDAppService
    {
        private readonly IRepository<ToChuc> toChucRespository;
        private readonly IRepository<User, long> userRespository;
        private readonly IRepository<TaiSan> taiSanRespository;
        private readonly IRepository<LoaiTaiSan> loaiTaiSanRespository;
        private readonly IRepository<PhieuTaiSan, long> phieuTaiSanRespository;
        private readonly IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRespository;
        private readonly ILookupTableAppService lookupTableAppService;
        private readonly IAppFolders appFolders;
        private readonly IRepository<PhieuDuTruMuaSam, long> phieuDuTruMuaSamRespository;
        private readonly IRepository<PhieuDuTruMuaSamChiTiet, long> phieuDuTruMuaSamChiTietRespository;

        public BaoCaoThongTinThietBiRFIDAppService(
            IRepository<ToChuc> toChucRespository,
            IRepository<TaiSan> taiSanRespository,
            IRepository<User, long> userRespository,
            IRepository<LoaiTaiSan> loaiTaiSanRespository,
            IRepository<PhieuTaiSan, long> phieuTaiSanRespository,
            IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRespository,
            ILookupTableAppService lookupTableAppService,
            IRepository<PhieuDuTruMuaSam, long> phieuDuTruMuaSamRespository,
            IRepository<PhieuDuTruMuaSamChiTiet, long> phieuDuTruMuaSamChiTietRespository,
            IAppFolders appFolders)
        {
            this.toChucRespository = toChucRespository;
            this.taiSanRespository = taiSanRespository;
            this.userRespository = userRespository;
            this.loaiTaiSanRespository = loaiTaiSanRespository;
            this.phieuTaiSanRespository = phieuTaiSanRespository;
            this.phieuTaiSanChiTietRespository = phieuTaiSanChiTietRespository;
            this.lookupTableAppService = lookupTableAppService;
            this.appFolders = appFolders;
            this.phieuDuTruMuaSamRespository = phieuDuTruMuaSamRespository;
            this.phieuDuTruMuaSamChiTietRespository = phieuDuTruMuaSamChiTietRespository;
        }

        public async Task<List<ListBaoCaoChiTietDto>> GetAllBaoCao(List<int> phongBanqQL, DateTime? tuNgay, DateTime? denNgay, bool? isSearch)
        {
            if (phongBanqQL == null || phongBanqQL.Count == 0)
            {
                phongBanqQL = new List<int> { -1 };
            }

            List<ListBaoCaoChiTietDto> result = new List<ListBaoCaoChiTietDto>();

            var phongBanCuaNguoiDung = GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRespository, (int)this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId).Result;
            List<int?> listLoaiTS = new List<int?>();
            listLoaiTS.Add(1);
            listLoaiTS.Add(2);
            var query = (
                from phieu in this.phieuTaiSanRespository.GetAll()
                           .Where(w => phongBanqQL.Contains((int)w.ToChucKhaiBaoId))
                           .Where(w => w.PhanLoaiId != 0 && (tuNgay.HasValue ? w.NgayKhaiBao.Value.Date >= tuNgay.Value.Date : tuNgay == null)
                           && (denNgay.HasValue ? w.NgayKhaiBao.Value.Date <= denNgay.Value.Date : denNgay == null))
                from phieuChiTiet in this.phieuTaiSanChiTietRespository.GetAll().Where(w => w.PhieuTaiSanId == phieu.Id)
                from ts in this.taiSanRespository.GetAll().Where(w => listLoaiTS.Contains(w.LoaiTaiSanId) && w.Id == phieuChiTiet.TaiSanId)
                select new { phieu, phieuChiTiet }).ToList();

            var queryPhieuDuTru = (from phieuDuTru in this.phieuDuTruMuaSamRespository.GetAll()
                           .Where(w => phongBanqQL.Contains((int)w.ToChucId))
                           .Where(w => (tuNgay.HasValue ? w.CreationTime.Date >= tuNgay.Value.Date : tuNgay == null)
                           && (denNgay.HasValue ? w.CreationTime.Date <= denNgay.Value.Date : denNgay == null))
                                   from phieuDuTruChiTiet in this.phieuDuTruMuaSamChiTietRespository.GetAll().Where(w => w.PhieuDuTruMuaSamId == phieuDuTru.Id)
                                   select new { phieuDuTru, phieuDuTruChiTiet }).ToList();

            result = query.GroupBy(e => e.phieu.NgayKhaiBao.Value.Date).OrderBy(w => w.Key.Date).Select(e => new ListBaoCaoChiTietDto
            {
                NgayKhaiBao = e.Key,
                ListCapPhat = e.Where(w => w.phieu.PhanLoaiId == 1).Count(),
                ListThuHoi = e.Where(w => w.phieu.PhanLoaiId == 3).Count(),
                ListDieuChuyen = e.Where(w => w.phieu.PhanLoaiId == 2).Count(),
                ListBaoMat = e.Where(w => w.phieu.PhanLoaiId == 7).Count(),
                ListBaoHong = e.Where(w => w.phieu.PhanLoaiId == 8).Count(),
                ListThanhLy = e.Where(w => w.phieu.PhanLoaiId == 9).Count(),
                ListBaoDuong = e.Where(w => w.phieu.PhanLoaiId == 6).Count(),
                ListDangSuDung = e.Where(w => w.phieu.PhanLoaiId == 4).Count(),
                ListSuaChua = e.Where(w => w.phieu.PhanLoaiId == 5).Count(),
                ListBaoHuy = e.Where(w => w.phieu.PhanLoaiId == 10).Count(),
                ListDuTruMuaSam = 0,
            }).ToList();

            var groupBy2 = queryPhieuDuTru.GroupBy(e => e.phieuDuTru.CreationTime.Date).Select(w => new
            {
                w.Key,
                value = w.Count(),
            });

            foreach (var item in groupBy2)
            {
                var xx = result.Where(w => w.NgayKhaiBao == item.Key).FirstOrDefault();

                if (xx != null)
                {
                    xx.ListDuTruMuaSam = item.value;
                }
                else
                {
                    result.Add(new ListBaoCaoChiTietDto
                    {
                        NgayKhaiBao = item.Key,
                        ListDuTruMuaSam = item.value,
                        ListCapPhat = 0,
                        ListThuHoi = 0,
                        ListDieuChuyen = 0,
                        ListBaoMat = 0,
                        ListBaoHong = 0,
                        ListThanhLy = 0,
                        ListBaoDuong = 0,
                        ListDangSuDung = 0,
                        ListSuaChua = 0,
                        ListBaoHuy = 0,
                    });
                }
            }

            result = result.OrderBy(w => w.NgayKhaiBao).ToList();

            if (result.Count > 0)
            {
                result.Add(new ListBaoCaoChiTietDto
                {
                    ToTal = "Tổng: ",
                    isCheck = true,
                    ListCapPhat = result.Sum(e => e.ListCapPhat),
                    ListThuHoi = result.Sum(e => e.ListThuHoi),
                    ListDieuChuyen = result.Sum(e => e.ListDieuChuyen),
                    ListBaoMat = result.Sum(e => e.ListBaoMat),
                    ListBaoHong = result.Sum(e => e.ListBaoHong),
                    ListThanhLy = result.Sum(e => e.ListThanhLy),
                    ListDuTruMuaSam = result.Sum(e => e.ListDuTruMuaSam),
                    ListBaoDuong = result.Sum(e => e.ListBaoDuong),
                    ListDangSuDung = result.Sum(e => e.ListDangSuDung),
                    ListSuaChua = result.Sum(e => e.ListSuaChua),
                    ListBaoHuy = result.Sum(e => e.ListBaoHuy),
                });
            }

            return result;
        }

        public async Task<FileDto> ExportToExcel(List<int> phongBanqQL, DateTime? tuNgay, DateTime? denNgay, bool? isSearch)
        {
            // Lấy danh sách cần xuất excel
            var list = await this.GetAllBaoCao(phongBanqQL, tuNgay, denNgay, isSearch);
            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("BaoCaoThongTinTaiSanRFID");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[17, 1].Value = "Thời gian";
            worksheet.Cells[17, 2].Value = "Cấp phát";
            worksheet.Cells[17, 3].Value = "Thu hồi";
            worksheet.Cells[17, 4].Value = "Điều chuyển";
            worksheet.Cells[17, 5].Value = "Báo mất";
            worksheet.Cells[17, 6].Value = "Báo hỏng";
            worksheet.Cells[17, 7].Value = "Thanh lý";
            worksheet.Cells[17, 8].Value = "Bảo dưỡng";

            // Bôi đậm header
            using (ExcelRange r = worksheet.Cells[17, 1, 17, 9])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 18;
            list.ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = item.ToTal != null ? item.ToTal : "Ngày " + item.NgayKhaiBao.Value.ToString("dd/MM/yyyy");
                worksheet.Cells[rowNumber, 2].Value = item.ListCapPhat;
                worksheet.Cells[rowNumber, 3].Value = item.ListThuHoi;
                worksheet.Cells[rowNumber, 4].Value = item.ListDieuChuyen;
                worksheet.Cells[rowNumber, 5].Value = item.ListBaoMat;
                worksheet.Cells[rowNumber, 6].Value = item.ListBaoHong;
                worksheet.Cells[rowNumber, 7].Value = item.ListThanhLy;
                worksheet.Cells[rowNumber, 8].Value = item.ListBaoDuong;
                if (item.isCheck == true)
                {
                    worksheet.Cells[rowNumber, 1, rowNumber, 8].Style.Font.Bold = true;
                }

                rowNumber++;
            });

            // add chart of type Pie.
            var myChart = worksheet.Drawings.AddChart("chart", eChartType.ColumnClustered);

            // Define series for the chart
            var series = myChart.Series.Add($"B{list.Count + 17}: I{list.Count + 17}", "B17: I17");
            myChart.Title.Text = string.Format("Báo cáo thống kê tài sản RFID");
            myChart.Legend.Remove();
            myChart.SetSize(400, 289);

            // Add to 6th row and to the 6th column
            myChart.SetPosition(0, 0, 1, 0);

            // Cho các ô rộng theo dữ liệu
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // Tên file
            var fileName = string.Join(".", new string[] { "Báo cáo thông tin tài sản RFID", "xlsx" });

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
    }
}
