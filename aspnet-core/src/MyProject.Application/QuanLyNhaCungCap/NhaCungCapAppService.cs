namespace MyProject.QuanLyNhaCungCap
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Data;
    using MyProject.Data.Excel.Dtos;
    using MyProject.Global;
    using MyProject.Net.MimeTypes;
    using MyProject.QuanLyNhaCungCap.Dtos;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class NhaCungCapAppService : MyProjectAppServiceBase, INhaCungCapAppService
    {
        private readonly IRepository<NhaCungCap> nhaCungCapRepository;
        private readonly IRepository<LinhVucKinhDoanh> linhVucKinhDoanhRepository;
        private readonly IRepository<NhaCungCap_File> nhaCungCapFileRepository;
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IAppFolders appFolders;

        public NhaCungCapAppService(
            IRepository<NhaCungCap> nhaCungCapRepository,
            IRepository<LinhVucKinhDoanh> linhVucKinhDoanhRepository,
            IRepository<TaiSan> taiSanRepository,
            IRepository<NhaCungCap_File> nhaCungCapFileRepository,
            IAppFolders appFolders)
        {
            this.nhaCungCapRepository = nhaCungCapRepository;
            this.linhVucKinhDoanhRepository = linhVucKinhDoanhRepository;
            this.taiSanRepository = taiSanRepository;
            this.nhaCungCapFileRepository = nhaCungCapFileRepository ?? throw new ArgumentNullException(nameof(nhaCungCapFileRepository));
            this.appFolders = appFolders;
        }

        public async Task<PagedResultDto<NhaCungCapForViewDto>> GetAllAsync(NhaCungCapGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var filter = from ncc in this.nhaCungCapRepository.GetAll().Where(e => e.MaNhaCungCap.Contains(GlobalFunction.RegexFormat(input.Keyword)) || e.TenNhaCungCap.Contains(GlobalFunction.RegexFormat(input.Keyword)) || e.DiaChi.Contains(GlobalFunction.RegexFormat(input.Keyword)) || e.SoDienThoai.Contains(GlobalFunction.RegexFormat(input.Keyword)) || e.Email.Contains(GlobalFunction.RegexFormat(input.Keyword)) || (input.Keyword == null && (e.LinhVucKinhDoanhId == input.LinhVuc || input.LinhVuc == null)))
                         select ncc;

            var query = from o in filter
                        from linhVuc in this.linhVucKinhDoanhRepository.GetAll().Where(w => w.Id == o.LinhVucKinhDoanhId).DefaultIfEmpty()
                        select new NhaCungCapForViewDto()
                        {
                            NhaCungCap = o,
                            TenLinhVuc = linhVuc.TenLinhVuc,
                        };
            int totalCount = await query.CountAsync();
            var output = await query.OrderBy(input.Sorting ?? "NhaCungCap.MaNhaCungCap")
                                    .PageBy(input)
                                    .ToListAsync();

            return new PagedResultDto<NhaCungCapForViewDto>(totalCount, output);
        }

        public async Task<NhaCungCapCreateInputDto> GetForEditAsync(EntityDto input, bool isView)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            List<NhaCungCap_File> listFile = new List<NhaCungCap_File>();
            var entity = await this.nhaCungCapRepository.FirstOrDefaultAsync(input.Id);
            var edit = this.ObjectMapper.Map<NhaCungCapCreateInputDto>(entity);
            var listFileDinhKem = await this.nhaCungCapFileRepository.GetAll().Where(w => w.NhaCungCapId == input.Id).ToListAsync();

            if (listFileDinhKem.Count > 0)
            {
                foreach (var item in listFileDinhKem)
                {
                    listFile.Add(item);
                }
            }

            edit.ListFile = listFile;
            return await Task.FromResult(edit);
        }

        public async Task<int> CreateOrEdit(NhaCungCapCreateInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            input.MaNhaCungCap = GlobalFunction.RegexFormat(input.MaNhaCungCap);
            input.TenNhaCungCap = GlobalFunction.RegexFormat(input.TenNhaCungCap);
            input.DiaChi = GlobalFunction.RegexFormat(input.DiaChi);
            input.SoDienThoai = GlobalFunction.RegexFormat(input.SoDienThoai);
            input.Email = GlobalFunction.RegexFormat(input.Email);
            input.GhiChu = GlobalFunction.RegexFormat(input.GhiChu);

            if (this.CheckExist(input.MaNhaCungCap, input.TenNhaCungCap, input.Id) != 0)
            {
                return this.CheckExist(input.MaNhaCungCap, input.TenNhaCungCap, input.Id);
            }

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

        public async Task DeleteAsync(EntityDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var check = this.taiSanRepository.GetAll().Where(w => w.NhaCungCapId == input.Id).ToList();
            if (check.Count == 0)
            {
                await this.nhaCungCapRepository.DeleteAsync((int)input.Id);
            }
            else
            {
                throw new UserFriendlyException(StringResources.XoaNhaCungCap);
            }
        }

        public async Task<FileDto> ExportToExcel(NhaCungCapGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // Lấy danh sách cần xuất excel
            var list = await this.GetAllAsync(input);
            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("NhaCungCap");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[1, 1].Value = "Mã nhà cung cấp";
            worksheet.Cells[1, 2].Value = "Tên nhà cung cấp";
            worksheet.Cells[1, 3].Value = "Lĩnh vực kinh doanh";
            worksheet.Cells[1, 4].Value = "Địa chỉ";
            worksheet.Cells[1, 5].Value = "Số điện thoại";
            worksheet.Cells[1, 6].Value = "Email";

            // Bôi đậm header
            using (ExcelRange r = worksheet.Cells[1, 1, 1, 6])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 2;
            list.Items.ToList().ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = item.NhaCungCap.MaNhaCungCap;
                worksheet.Cells[rowNumber, 2].Value = item.NhaCungCap.TenNhaCungCap;
                worksheet.Cells[rowNumber, 3].Value = item.TenLinhVuc;
                worksheet.Cells[rowNumber, 4].Value = item.NhaCungCap.DiaChi;
                worksheet.Cells[rowNumber, 5].Value = item.NhaCungCap.SoDienThoai;
                worksheet.Cells[rowNumber, 6].Value = item.NhaCungCap.Email;
                rowNumber++;
            });

            // Cho các ô rộng theo dữ liệu
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // Tên file
            var fileName = string.Join(".", new string[] { "Danh sách nhà cung cấp", "xlsx" });

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

        /// <summary>
        /// Import excel.
        /// </summary>
        /// <param name="filePath">Đường dẫn file trên server.</param>
        /// <returns>Danh sách đường dẫn.</returns>
        public async Task<string> ImportFileExcel(string filePath)
        {
            StringBuilder returnMessage = new StringBuilder();
            returnMessage.Append("Kết quả nhập file:");
            ReadFromExcelDto<NhaCungCapCreateInputDto> readResult = new ReadFromExcelDto<NhaCungCapCreateInputDto>();

            // Không tìm thấy file
            if (!File.Exists(filePath))
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.FileNotFound;
            }

            // Đọc hết file excel
            var data = await GlobalFunction.ReadFromExcel(filePath);
            string error = string.Empty;
            int thanhCong = 0;
            int thatBai = 0;

            // Không có dữ liệu
            if (data.Count <= 0 || data[0].Count != 8)
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.CantReadData;
            }
            else
            {
                // Đọc lần lượt từng dòng
                for (int i = 0; i < data.Count; i++)
                {
                    string ma = GlobalFunction.RegexFormat(data[i][0]);
                    string ten = GlobalFunction.RegexFormat(data[i][1]);
                    string linhVuc = GlobalFunction.RegexFormat(data[i][2]);
                    string maSoThue = GlobalFunction.RegexFormat(data[i][3]);
                    string diaChi = GlobalFunction.RegexFormat(data[i][4]);
                    string sdt = GlobalFunction.RegexFormat(data[i][5]);
                    string email = GlobalFunction.RegexFormat(data[i][6]);
                    string ghiChu = GlobalFunction.RegexFormat(data[i][7]);

                    List<LinhVucKinhDoanh> listLinhVuc = this.linhVucKinhDoanhRepository.GetAll().ToList();
                    if (string.IsNullOrEmpty(value: ma))
                    {
                        error += "+ Mã nhà cung cấp không được để trống " + ":" + " Dòng " + (i + 1) + "\n";
                    }

                    if (string.IsNullOrEmpty(value: ten))
                    {
                        error += "+ Tên nhà cung cấp không được để trống " + ":" + " Dòng " + (i + 1) + "\n";
                    }

                    var linhVuc1 = new LinhVucKinhDoanh();
                    if (!string.IsNullOrEmpty(value: linhVuc))
                    {
                        linhVuc1 = listLinhVuc.Find(w => w.TenLinhVuc == linhVuc);
                        if (linhVuc1 == null)
                        {
                            error += "+ Lĩnh vực không tồn tại " + ":" + " Dòng " + (i + 1) + "\n";
                        }
                    }

                    if (!string.IsNullOrEmpty(value: email) && !this.IsEmail(email))
                    {
                        error += "+ Email không đúng định dạng " + ":" + " Dòng " + (i + 1) + "\n";
                    }

                    if (string.IsNullOrEmpty(error))
                    {
                        var create = new NhaCungCapCreateInputDto
                        {
                            MaNhaCungCap = ma,
                            TenNhaCungCap = ten,
                            DiaChi = diaChi,
                            LinhVucKinhDoanhId = linhVuc1.Id != 0 ? linhVuc1.Id : (int?)null,
                            MaSoThue = maSoThue,
                            SoDienThoai = sdt,
                            Email = email,
                            GhiChu = ghiChu,
                        };

                        // Nếu không bị trùng
                        if (await this.CreateOrEdit(create) == 0)
                        {
                            // Đánh dấu các bản ghi thêm thành công
                            readResult.ListResult.Add(create);
                            thanhCong++;
                        }
                        else if (await this.CreateOrEdit(create) == 1)
                        {
                            error += "+ Mã nhà cung cấp đã bị trùng không được import " + ":" + " Dòng " + (i + 1) + "\n";
                        }
                        else if (await this.CreateOrEdit(create) == 2)
                        {
                            error += "+ Tên nhà cung cấp đã bị trùng không được import " + ":" + " Dòng " + (i + 1) + "\n";
                        }
                    }
                    else
                    {
                        thatBai++;
                        readResult.ErrorMessage = error;
                        List<string> a = new List<string> { error, };
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
                returnMessage.Append(string.Format("\r\n\u00A0- Tổng ghi: {0}", data.Count));
                returnMessage.Append(string.Format("\r\n\u00A0- Số bản ghi thành công: {0}", thanhCong));
                returnMessage.Append(string.Format("\r\n\u00A0- Số bản ghi thất bại: {0}", thatBai) + "\n" + error);
            }

            return returnMessage.ToString();
        }

        public bool IsEmail(string inputEmail)
        {
            inputEmail = inputEmail ?? string.Empty;
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(inputEmail))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tải file mẫu cho import.
        /// </summary>
        /// <returns>File mẫu.</returns>
        public async Task<FileDto> DownloadFileMau()
        {
            string fileName = string.Format("NhaCungCapImport.xlsx");
            try
            {
                // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
                // _appFolders.TempFileDownloadFolder : Không được sửa
                return await GlobalFunction.DownloadFileMau(fileName, this.appFolders.NhaCungCapFileDownloadFolder, this.appFolders.TempFileDownloadFolder);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("Có lỗi: " + ex.Message);
            }
        }

        public async Task<FileDto> DownloadFileUpload(string linkFile)
        {
            if (string.IsNullOrEmpty(linkFile))
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            var fileName = linkFile.Split(@"\").Last();
            var path = this.appFolders.NhaCungCapUploadFolder + linkFile.Replace(fileName, string.Empty);

            // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
            // _appFolders.TempFileDownloadFolder : Không được sửa
            return await GlobalFunction.DownloadFileMau(fileName, path, this.appFolders.TempFileDownloadFolder);
        }

        private int CheckExist(string ma, string ten, int? id)
        {
            ma = GlobalFunction.RegexFormat(ma);
            ten = GlobalFunction.RegexFormat(ten);

            // Nếu query > 0 thì là bị trùng mã => return true
            var query = this.nhaCungCapRepository.GetAll().Where(e => e.MaNhaCungCap == ma || e.TenNhaCungCap == ten)
                .WhereIf(id != null, e => e.Id != id).FirstOrDefault();
            if (query != null)
            {
                if (query.MaNhaCungCap.ToLower() == ma.ToLower())
                {
                    return 1;
                }
                else if (query.TenNhaCungCap.ToLower() == ten.ToLower())
                {
                    return 2;
                }
            }

            return 0;
        }

        private async Task Create(NhaCungCapCreateInputDto input)
        {
            List<NhaCungCap_File> taiSanDinhKemFileList = new List<NhaCungCap_File>();

            if (input.ListFile != null)
            {
                taiSanDinhKemFileList.AddRange(input.ListFile.Select(e => new NhaCungCap_File
                {
                    LinkFile = e.LinkFile,
                    TenFile = e.TenFile,
                }));
            }

            var create = this.ObjectMapper.Map<NhaCungCap>(input);
            create.NhaCungCapDinhKemFileList = taiSanDinhKemFileList;
            await this.nhaCungCapRepository.InsertAndGetIdAsync(create);
        }

        private async Task Update(NhaCungCapCreateInputDto input)
        {
            var update = await this.nhaCungCapRepository.FirstOrDefaultAsync((int)input.Id);
            await this.nhaCungCapFileRepository.DeleteAsync(w => w.NhaCungCapId == (int)input.Id);

            foreach (var item in input.ListFile)
            {
                var file = new NhaCungCap_File
                {
                    NhaCungCapId = input.Id,
                    LinkFile = item.LinkFile,
                    TenFile = item.TenFile,
                };

                await this.nhaCungCapFileRepository.InsertAsync(file);
            }

            this.ObjectMapper.Map(input, update);
        }
    }
}
