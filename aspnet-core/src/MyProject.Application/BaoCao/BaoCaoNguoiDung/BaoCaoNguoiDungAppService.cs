namespace MyProject.BaoCaoNguoiDung
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Abp.Auditing;
    using Abp.Domain.Repositories;
    using CRM.Auditing;
    using DbEntities;
    using MyProject.Authorization.Users;
    using MyProject.BaoCao.BaoCaoNguoiDung.Dtos;
    using MyProject.Data;
    using MyProject.Net.MimeTypes;
    using Newtonsoft.Json;
    using OfficeOpenXml;
    using OfficeOpenXml.Drawing.Chart;
    using OfficeOpenXml.Style;

    public class BaoCaoNguoiDungAppService : MyProjectAppServiceBase, IBaoCaoNguoiDungAppService
    {
        private readonly IRepository<AuditLog, long> auditLogRepository;
        private readonly IRepository<User, long> userRepository;
        private readonly INamespaceStripper namespaceStripper;
        private readonly IRepository<ToChuc> toChucRespository;
        private readonly IAppFolders appFolders;

        public BaoCaoNguoiDungAppService(
            IRepository<AuditLog, long> auditLogRepository,
            IRepository<User, long> userRepository,
            IAppFolders appFolders,
            INamespaceStripper namespaceStripper,
            IRepository<ToChuc> toChucRespository)
        {
            this.auditLogRepository = auditLogRepository;
            this.toChucRespository = toChucRespository;
            this.userRepository = userRepository;
            this.appFolders = appFolders;
            this.namespaceStripper = namespaceStripper;
        }

        public async Task<List<ListViewDto>> GetAllBaoCao(List<int> phongBanqQL, DateTime? tuNgay, DateTime? denNgay, bool? isSearch)
        {
            if (phongBanqQL == null || phongBanqQL.Count == 0)
            {
                phongBanqQL = new List<int> { -1 };
            }

            List<ListViewDto> listZero = new List<ListViewDto>();

            var query = (from audit in this.auditLogRepository.GetAll()
                         .Where(w => (!tuNgay.HasValue || w.ExecutionTime.Date >= tuNgay.Value.Date)
                          && (!denNgay.HasValue || w.ExecutionTime.Date <= denNgay.Value.Date))
                         from user in this.userRepository.GetAll()
                         .Where(w => phongBanqQL.Contains((int)w.ToChucId))
                         .Where(w => w.Id == audit.UserId)
                         select audit).ToList();
            var b = new ThongTinNguoiDung();
            List<string> listSearchLogin = new List<string>();
            var userPB = this.userRepository.GetAll().Where(w => phongBanqQL.Contains((int)w.ToChucId)).Select(e => e.UserName).ToList();
            var queryAuthen = this.auditLogRepository.GetAll().Where(w => w.MethodName.Contains("Authenticate"))
                .Where(w => (!tuNgay.HasValue || w.ExecutionTime.Date >= tuNgay.Value.Date)
                          && (!denNgay.HasValue || w.ExecutionTime.Date <= denNgay.Value.Date)).ToList();

            userPB.ForEach(item =>
            {
                b.userNameOrEmailAddress = item;
                var json = JsonConvert.SerializeObject(b);
                string[] g = json.Split("}");
                listSearchLogin.Add(g[0]);
            });
            query.AddRange(queryAuthen.Select(e => e));

            var listData = query.GroupBy(e => e.ExecutionTime.Date).OrderBy(w => w.Key.Date).Select(e => new ListViewDto
            {
                NgayKhaiBao = e.Key,
                ListCreate = e.Where(w => w.MethodName.Contains("Create") && (w.Parameters.Contains("\"normalizedName\":") || w.Parameters.Contains("\"surname\":") || w.Parameters.Contains("\"id\":null") || w.Parameters.Contains("\"id\":0"))).Count(),
                ListEdit = e.Where(w => (w.MethodName.Contains("CreateOrEdit") || w.MethodName.Contains("UpdateAsync")) && !w.Parameters.Contains("\"isView\":true") && !(w.Parameters.Contains("\"id\":null") || w.Parameters.Contains("\"id\":0"))).Count(),
                ListDelete = e.Where(w => w.MethodName.Contains("Delete")).Count(),
                ListHoanTac = e.Where(w => w.MethodName.Contains("HoanTac")).Count(),
                ListDownLoad = e.Where(w => w.MethodName.Contains("Export")).Count(),
                ListUpload = e.Where(w => w.MethodName.Contains("Import")).Count(),
                ListView = e.Where(w => w.MethodName.Contains("ForEdit") && w.Parameters.Contains("\"isView\":true")).Count(),
                ListKhaiBaoSuDung = e.Where(w => w.MethodName.Contains("CapPhatTaiSan") && w.Parameters.Contains("\"phanLoaiId\":4")).Count(),
                ListCapPhat = e.Where(w => w.MethodName.Contains("CapPhatTaiSan") && w.Parameters.Contains("\"phanLoaiId\":1")).Count(),
                ListDieuChuyen = e.Where(w => w.MethodName.Contains("CapPhatTaiSan") && w.Parameters.Contains("\"phanLoaiId\":2")).Count(),
                ListThuHoi = e.Where(w => w.MethodName.Contains("CapPhatTaiSan") && w.Parameters.Contains("\"phanLoaiId\":3")).Count(),
                ListHongMatTaiSan = e.Where(w => w.MethodName.Contains("CreateTaiSanHong") || w.MethodName.Contains("CreateTaiSanMat")).Count(),
                ListLogin = e.Where(w => listSearchLogin.Any(s => w.Parameters.Contains(s))).Count(),
                ListLogout = e.Where(w => w.MethodName.Contains("Logout")).Count(),
                ListSearch = e.Where(w => w.MethodName.Contains("GetAll") && w.Parameters.Contains("\"isSearch\":true")).Count(),
            }).ToList();

            listData.ForEach(item =>
            {
                if (item.ListCreate != 0 || item.ListEdit != 0 || item.ListDelete != 0 || item.ListHoanTac != 0 || item.ListDownLoad != 0
                || item.ListUpload != 0 || item.ListView != 0 || item.ListKhaiBaoSuDung != 0 || item.ListCapPhat != 0
                || item.ListDieuChuyen != 0 || item.ListThuHoi != 0 || item.ListHongMatTaiSan != 0 || item.ListLogin != 0 || item.ListLogout != 0 || item.ListSearch != 0)
                {
                    listZero.Add(item);
                }
            });

            if (listZero.Count > 0)
            {
                listZero.Add(new ListViewDto
                {
                    ToTal = "Tổng: ",
                    isCheck = true,
                    ListCreate = listData.Sum(e => e.ListCreate),
                    ListEdit = listData.Sum(e => e.ListEdit),
                    ListDelete = listData.Sum(e => e.ListDelete),
                    ListDownLoad = listData.Sum(e => e.ListDownLoad),
                    ListUpload = listData.Sum(e => e.ListUpload),
                    ListView = listData.Sum(e => e.ListView),
                    ListHoanTac = listData.Sum(e => e.ListHoanTac),
                    ListKhaiBaoSuDung = listData.Sum(e => e.ListKhaiBaoSuDung),
                    ListCapPhat = listData.Sum(e => e.ListCapPhat),
                    ListDieuChuyen = listData.Sum(e => e.ListDieuChuyen),
                    ListThuHoi = listData.Sum(e => e.ListThuHoi),
                    ListHongMatTaiSan = listData.Sum(e => e.ListHongMatTaiSan),
                    ListLogin = listData.Sum(e => e.ListLogin),
                    ListLogout = listData.Sum(e => e.ListLogout),
                    ListSearch = listData.Sum(e => e.ListSearch),
                });
            }

            return await Task.FromResult(listZero);
        }

        public async Task<FileDto> ExportToExcel(List<int> phongBanqQL, DateTime? tuNgay, DateTime? denNgay, bool? isSearch)
        {
            // Lấy danh sách cần xuất excel
            var list = await this.GetAllBaoCao(phongBanqQL, tuNgay, denNgay, isSearch);
            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("BaoCaoNguoiDung");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[17, 1].Value = "Thời gian";
            worksheet.Cells[17, 2].Value = "Đăng nhập";
            worksheet.Cells[17, 3].Value = "Đăng xuất";
            worksheet.Cells[17, 4].Value = "Thêm mới";
            worksheet.Cells[17, 5].Value = "Sửa";
            worksheet.Cells[17, 6].Value = "Xoá";
            worksheet.Cells[17, 7].Value = "Xem";
            worksheet.Cells[17, 8].Value = "Xuất Excel";
            worksheet.Cells[17, 9].Value = "Nhập Excel";
            worksheet.Cells[17, 10].Value = "Hoàn tác";
            worksheet.Cells[17, 11].Value = "Khai báo sử dụng";
            worksheet.Cells[17, 12].Value = "Cấp phát";
            worksheet.Cells[17, 13].Value = "Điều chuyển";
            worksheet.Cells[17, 14].Value = "Thu hồi";
            worksheet.Cells[17, 15].Value = "Báo hỏng mất";
            worksheet.Cells[17, 16].Value = "Tìm kiếm";

            // Bôi đậm header
            using (ExcelRange r = worksheet.Cells[17, 1, 17, 16])
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
                worksheet.Cells[rowNumber, 2].Value = item.ListLogin;
                worksheet.Cells[rowNumber, 3].Value = item.ListLogout;
                worksheet.Cells[rowNumber, 4].Value = item.ListCreate;
                worksheet.Cells[rowNumber, 5].Value = item.ListEdit;
                worksheet.Cells[rowNumber, 6].Value = item.ListDelete;
                worksheet.Cells[rowNumber, 7].Value = item.ListView;
                worksheet.Cells[rowNumber, 8].Value = item.ListDownLoad;
                worksheet.Cells[rowNumber, 9].Value = item.ListUpload;
                worksheet.Cells[rowNumber, 10].Value = item.ListHoanTac;
                worksheet.Cells[rowNumber, 11].Value = item.ListKhaiBaoSuDung;
                worksheet.Cells[rowNumber, 12].Value = item.ListCapPhat;
                worksheet.Cells[rowNumber, 13].Value = item.ListDieuChuyen;
                worksheet.Cells[rowNumber, 14].Value = item.ListThuHoi;
                worksheet.Cells[rowNumber, 15].Value = item.ListHongMatTaiSan;
                worksheet.Cells[rowNumber, 16].Value = item.ListSearch;
                if (item.isCheck == true)
                {
                    worksheet.Cells[rowNumber, 1, rowNumber, 16].Style.Font.Bold = true;
                }

                rowNumber++;
            });

            // add chart of type Pie.
            var myChart = worksheet.Drawings.AddChart("chart", eChartType.ColumnClustered);

            // Define series for the chart
            var series = myChart.Series.Add($"B{list.Count + 17}: P{list.Count + 17}", "B17: P17");
            myChart.Title.Text = string.Format("Báo cáo người dùng");
            myChart.Legend.Remove();
            myChart.SetSize(650, 289);

            // Add to 6th row and to the 6th column
            myChart.SetPosition(0, 0, 3, 0);

            // Cho các ô rộng theo dữ liệu
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // Tên file
            var fileName = string.Join(".", new string[] { "Báo cáo người dùng", "xlsx" });

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
