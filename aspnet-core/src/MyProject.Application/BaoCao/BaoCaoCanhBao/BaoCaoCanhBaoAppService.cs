namespace MyProject.BaoCaoCanhBao
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Abp.Domain.Repositories;
    using DbEntities;
    using MyProject.Authorization.Users;
    using MyProject.BaoCao.BaoCaoCanhBao.Dtos;
    using MyProject.Data;
    using MyProject.Global;
    using MyProject.Net.MimeTypes;
    using OfficeOpenXml;
    using OfficeOpenXml.Drawing.Chart;
    using OfficeOpenXml.Style;

    public class BaoCaoCanhBaoAppService : MyProjectAppServiceBase, IBaoCaoCanhBaoAppService
    {
        private readonly IRepository<ToChuc> toChucRespository;
        private readonly IRepository<User, long> userRespository;
        private readonly IRepository<TaiSan> taiSanRespository;
        private readonly IRepository<LoaiTaiSan> loaiTaiSanRespository;
        private readonly IRepository<CanhBao> canhBaoRespository;
        private readonly IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRespository;
        private readonly ILookupTableAppService lookupTableAppService;
        private readonly IAppFolders appFolders;

        public BaoCaoCanhBaoAppService(
            IRepository<ToChuc> toChucRespository,
            IRepository<TaiSan> taiSanRespository,
            IRepository<User, long> userRespository,
            IRepository<LoaiTaiSan> loaiTaiSanRespository,
            IRepository<CanhBao> canhBaoRespository,
            IRepository<PhieuTaiSanChiTiet, long> phieuTaiSanChiTietRespository,
            ILookupTableAppService lookupTableAppService,
            IAppFolders appFolders)
        {
            this.toChucRespository = toChucRespository;
            this.taiSanRespository = taiSanRespository;
            this.userRespository = userRespository;
            this.loaiTaiSanRespository = loaiTaiSanRespository;
            this.canhBaoRespository = canhBaoRespository;
            this.phieuTaiSanChiTietRespository = phieuTaiSanChiTietRespository;
            this.lookupTableAppService = lookupTableAppService;
            this.appFolders = appFolders;
        }

        public string CheckPhongBan()
        {
            var query = (from o in this.userRespository.GetAll().Where(w => w.Id == this.AbpSession.UserId)
                         from o1 in this.toChucRespository.GetAll().Where(w => w.Id == o.ToChucId)
                         select o1.TenToChuc).FirstOrDefault();

            return query;
        }

        public List<ListBaoCaoCanhBaoOutputDto> GetAllBaoCao(List<int?> phongBanqQL, DateTime? tuNgay, DateTime? denNgay, bool? isSearch)
        {
            if (phongBanqQL == null || phongBanqQL.Count == 0)
            {
                phongBanqQL = new List<int?> { -1 };
            }

            List<int> phanLoai = new List<int> { (int)GlobalConst.CanhBaoThongBao.PhatHienRa, (int)GlobalConst.CanhBaoThongBao.PhatHienVao, (int)GlobalConst.CanhBaoThongBao.BatDau, (int)GlobalConst.CanhBaoThongBao.KetThuc };
            var query = (from canhBao in this.canhBaoRespository.GetAll().Where(w => phongBanqQL.Contains((int)w.PhongBanNhanId) && w.IsCheckBaoCao == true)
                            .Where(w => phanLoai.Contains((int)w.PhanLoai))
                            .Where(w => !(phongBanqQL != null && phongBanqQL.Count > 0) || (phongBanqQL.Contains((int)w.PhongBanNhanId)
                            && (tuNgay.HasValue ? w.CreationTime.Date >= tuNgay.Value.Date : tuNgay == null)
                            && (denNgay.HasValue ? w.CreationTime.Date <= denNgay.Value.Date : denNgay == null)))
                         select new { canhBao }).ToList();

            var listData = query.GroupBy(e => e.canhBao.CreationTime.Date).OrderBy(w => w.Key.Date).Select(e => new ListBaoCaoCanhBaoOutputDto
            {
                NgayKhaiBao = e.Key,
                TaiSanRa = e.Where(w => w.canhBao.PhanLoai == (int)GlobalConst.CanhBaoThongBao.PhatHienRa).Count(),
                TaiSanVao = e.Where(w => w.canhBao.PhanLoai == (int)GlobalConst.CanhBaoThongBao.PhatHienVao).Count(),
                BatDauKiemKe = e.Where(w => w.canhBao.PhanLoai == (int)GlobalConst.CanhBaoThongBao.BatDau).Count(),
                KetThucKiemKe = e.Where(w => w.canhBao.PhanLoai == (int)GlobalConst.CanhBaoThongBao.KetThuc).Count(),
            }).ToList();

            if (listData.Count > 0)
            {
                listData.Add(new ListBaoCaoCanhBaoOutputDto
                {
                    ToTal = "Tổng: ",
                    IsCheck = true,
                    TaiSanRa = listData.Sum(e => e.TaiSanRa),
                    TaiSanVao = listData.Sum(e => e.TaiSanVao),
                    BatDauKiemKe = listData.Sum(e => e.BatDauKiemKe),
                    KetThucKiemKe = listData.Sum(e => e.KetThucKiemKe),
                });
            }

            return listData;
        }

        public FileDto ExportToExcel(List<int?> phongBanqQL, DateTime? tuNgay, DateTime? denNgay)
        {
            // Lấy danh sách cần xuất excel
            var list = this.GetAllBaoCao(phongBanqQL, tuNgay, denNgay, null);
            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("BaoCaoThongTinTaiSan");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[17, 1].Value = "Thời gian";
            worksheet.Cells[17, 2].Value = "Tài sản ra";
            worksheet.Cells[17, 3].Value = "Tài sản vào";
            worksheet.Cells[17, 4].Value = "Bắt đầu kiểm kê";
            worksheet.Cells[17, 5].Value = "Kết thúc kiểm kê";

            // Bôi đậm header
            using (ExcelRange r = worksheet.Cells[17, 1, 17, 5])
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
                worksheet.Cells[rowNumber, 2].Value = item.TaiSanRa;
                worksheet.Cells[rowNumber, 3].Value = item.TaiSanVao;
                worksheet.Cells[rowNumber, 4].Value = item.BatDauKiemKe;
                worksheet.Cells[rowNumber, 5].Value = item.KetThucKiemKe;
                if (item.IsCheck == true)
                {
                    worksheet.Cells[rowNumber, 1, rowNumber, 5].Style.Font.Bold = true;
                }

                rowNumber++;
            });

            // add chart of type Pie.
            var myChart = worksheet.Drawings.AddChart("chart", eChartType.ColumnClustered);

            // Define series for the chart
            var series = myChart.Series.Add($"B{list.Count + 17}: E{list.Count + 17}", "B17: E17");
            myChart.Title.Text = string.Format("Tổng quan tình trạng cảnh báo");

            myChart.Legend.Remove();
            myChart.SetSize(400, 289);

            // Add to 6th row and to the 6th column
            myChart.SetPosition(0, 0, 0, 0);

            // Cho các ô rộng theo dữ liệu
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // Tên file
            var fileName = string.Join(".", new string[] { "Báo cáo cảnh báo", "xlsx" });

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
