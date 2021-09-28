namespace MyProject.QuanLyPhieuDuTruMuaSam
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.Authorization;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization;
    using MyProject.Authorization.Users;
    using MyProject.Data;
    using MyProject.Global;
    using MyProject.Net.MimeTypes;
    using MyProject.QuanLyPhieuDuTruMuaSam.Dtos;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class PhieuDuTruMuaSamAppService : MyProjectAppServiceBase, IPhieuDuTruMuaSamAppService
    {
        private readonly IRepository<PhieuDuTruMuaSam, long> phieuDuTruMuaSamRepository;
        private readonly IRepository<PhieuDuTruMuaSamChiTiet, long> phieuDuTruMuaSamChiTietRepository;
        private readonly IRepository<PhieuDuTruMuaSamDinhKemFile, long> phieuDuTruMuaSamDinhKemFileRepository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IRepository<User, long> userRepository;
        private readonly IAppFolders appFolders;

        public PhieuDuTruMuaSamAppService(
            IRepository<PhieuDuTruMuaSam, long> phieuDuTruMuaSamRepository,
            IRepository<PhieuDuTruMuaSamChiTiet, long> phieuDuTruMuaSamChiTietRepository,
            IRepository<PhieuDuTruMuaSamDinhKemFile, long> phieuDuTruMuaSamDinhKemFileRepository,
            IRepository<ToChuc> toChucRepository,
            IRepository<User, long> userRepository,
            IAppFolders appFolders)
        {
            this.phieuDuTruMuaSamRepository = phieuDuTruMuaSamRepository;
            this.phieuDuTruMuaSamChiTietRepository = phieuDuTruMuaSamChiTietRepository;
            this.phieuDuTruMuaSamDinhKemFileRepository = phieuDuTruMuaSamDinhKemFileRepository;
            this.toChucRepository = toChucRepository;
            this.userRepository = userRepository;
            this.appFolders = appFolders;
        }

        [AbpAuthorize(PermissionNames.Pages_QuanLyDuTruMuaSam)]
        public async Task<PagedResultDto<DuTruMuaSamOutPut>> GetAll(DuTruMuaSamInput input)
        {
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            if (input.PhongBan == null || input.PhongBan.Count == 0)
            {
                input.PhongBan = new List<int?> { -1 };
            }

            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            input.Keyword = GlobalFunction.RegexFormat(input.Keyword);
            int? trangThai = null;
            if (input.Keyword == "Khởi tạo" || input.Keyword == "khởi tạo")
            {
                trangThai = 0;
                input.Keyword = null;
            }
            else if (input.Keyword == "Hoàn thành" || input.Keyword == "hoàn thành")
            {
                trangThai = 1;
                input.Keyword = null;
            }
            else if (input.Keyword == "Hủy bỏ" || input.Keyword == "hủy bỏ")
            {
                trangThai = 2;
                input.Keyword = null;
            }

            var query = from duTru in this.phieuDuTruMuaSamRepository.GetAll()
                        .WhereIf(!string.IsNullOrEmpty(input.Keyword), e => e.MaPhieu.Contains(input.Keyword) || e.TenPhieu.Contains(input.Keyword) || e.NgayLapPhieuStr.Contains(input.Keyword))
                        .WhereIf(trangThai != null, e => e.TrangThaiId == trangThai)
                        .Where(w => input.PhongBan.Contains(w.ToChucId))
                        from toChuc in this.toChucRepository.GetAll().Where(w => w.Id == duTru.ToChucId)
                        from us in this.userRepository.GetAll().Where(w => w.Id == duTru.CreatorUserId)
                        select new DuTruMuaSamOutPut
                        {
                            Id = duTru.Id,
                            TenPhongBan = toChuc.TenToChuc,
                            ToChucId = duTru.ToChucId,
                            MaPhieu = duTru.MaPhieu,
                            TenPhieu = duTru.TenPhieu,
                            SoLuongDeXuat = this.phieuDuTruMuaSamChiTietRepository.GetAll().Where(w => w.PhieuDuTruMuaSamId == duTru.Id).Sum(w => (int)w.SoLuong),
                            ChiPhiDeXuat = this.phieuDuTruMuaSamChiTietRepository.GetAll().Where(w => w.PhieuDuTruMuaSamId == duTru.Id).Sum(w => (int)w.SoLuong * w.DonGia),
                            NguoiLap = us.Name,
                            NguoiLapPhieuId = duTru.NguoiLapPhieuId,
                            NgayLap = duTru.CreationTime.ToString("dd/MM/yyyy"),
                            NgayLapDate = duTru.CreationTime,
                            NgayCapNhat = duTru.LastModificationTime.HasValue ? duTru.LastModificationTime.Value.ToString("dd/MM/yyyy") : string.Empty,
                            NgayCapNhatDate = duTru.LastModificationTime.HasValue ? duTru.LastModificationTime.Value : (DateTime?)null,
                            TrangThai = GlobalModel.TrangThaiPhieuDuTru[(int)duTru.TrangThaiId],
                            TrangThaiId = duTru.TrangThaiId,
                        };
            int totalCount = await query.CountAsync();
            var output = await query.OrderBy(input.Sorting ?? "NgayLapDate DESC")
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<DuTruMuaSamOutPut>(totalCount, output);
        }

        public async Task<string> GetUserDangNhap(int input)
        {
            var query = await this.userRepository.GetAll().Where(w => w.Id == input).Select(s => s.Name).FirstOrDefaultAsync();
            return query;
        }

        [AbpAuthorize(PermissionNames.Pages_QuanLyDuTruMuaSam)]
        public async Task<CreateDuTruInput> GetForEditAsync(long input, bool? isView)
        {
            List<PhieuDuTruMuaSamChiTiet> phieuDuTruMuaSamChiTiet = new List<PhieuDuTruMuaSamChiTiet>();
            List<PhieuDuTruMuaSamDinhKemFile> listDinhKem = new List<PhieuDuTruMuaSamDinhKemFile>();
            var entity = await this.phieuDuTruMuaSamRepository.FirstOrDefaultAsync(input);
            var edit = this.ObjectMapper.Map<CreateDuTruInput>(entity);
            edit.NguoiLapPhieuId = (int)entity.CreatorUserId;
            var listphieuDuTruChiTiet = await this.phieuDuTruMuaSamChiTietRepository.GetAll().Where(w => w.PhieuDuTruMuaSamId == input).ToListAsync();

            if (listphieuDuTruChiTiet.Count > 0)
            {
                foreach (var item in listphieuDuTruChiTiet)
                {
                    phieuDuTruMuaSamChiTiet.Add(item);
                }
            }

            var listFileDinhKem = await this.phieuDuTruMuaSamDinhKemFileRepository.GetAll().Where(w => w.PhieuDuTruMuaSamId == input).ToListAsync();

            if (listFileDinhKem.Count > 0)
            {
                foreach (var item in listFileDinhKem)
                {
                    listDinhKem.Add(item);
                }
            }

            edit.ListPhieuChiTiet = phieuDuTruMuaSamChiTiet;
            edit.ListDinhKem = listDinhKem;
            return await Task.FromResult(edit);
        }

        public async Task<FileDto> DownloadFileUpload(string linkFile)
        {
            if (string.IsNullOrEmpty(linkFile))
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            var fileName = linkFile.Split(@"\").Last();
            var path = this.appFolders.DuTruMuaSamUploadFolder + linkFile.Replace(fileName, string.Empty);

            // _appFolders.DemoFileDownloadFolder : Thư mục chưa file mẫu cần tải
            // _appFolders.TempFileDownloadFolder : Không được sửa
            return await GlobalFunction.DownloadFileMau(fileName, path, this.appFolders.TempFileDownloadFolder);
        }

        public async Task<int> Deleted(int input)
        {
            try
            {
                await this.phieuDuTruMuaSamRepository.DeleteAsync(input);
                return 1;
            }
            catch
            {
                return 2;
            }
        }

        public void HoanThanh(int input)
        {
            var query = this.phieuDuTruMuaSamRepository.FirstOrDefault(w => w.Id == input);
            query.TrangThaiId = (int)GlobalConst.TrangThaiDuTruMuaSamConst.HoanThanh;
        }

        public void HuyBo(int input)
        {
            var query = this.phieuDuTruMuaSamRepository.FirstOrDefault(w => w.Id == input);
            query.TrangThaiId = (int)GlobalConst.TrangThaiDuTruMuaSamConst.HuyBo;
        }

        public async Task XoaList(List<int> input)
        {
            try
            {
#pragma warning disable CA1062 // Validate arguments of public methods
                foreach (var item in input)
                {
                    await this.phieuDuTruMuaSamRepository.DeleteAsync((int)item);
                }
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.Message);
            }
        }

        public async Task<FileDto> ExportToExcel(DuTruMuaSamInput input)
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
            worksheet.Cells[1, 1].Value = "Đơn vị";
            worksheet.Cells[1, 2].Value = "Mã phiếu";
            worksheet.Cells[1, 3].Value = "Tên phiếu";
            worksheet.Cells[1, 4].Value = "Số lượng đề xuất";
            worksheet.Cells[1, 5].Value = "Chi phí đề xuất";
            worksheet.Cells[1, 6].Value = "Người lập";
            worksheet.Cells[1, 7].Value = "Ngày lập";
            worksheet.Cells[1, 8].Value = "Ngày Cập nhật";
            worksheet.Cells[1, 9].Value = "Trạng thái";

            // Bôi đậm header
            using (ExcelRange r = worksheet.Cells[1, 1, 1, 9])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 2;
            list.Items.ToList().ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = item.TenPhongBan;
                worksheet.Cells[rowNumber, 2].Value = item.MaPhieu;
                worksheet.Cells[rowNumber, 3].Value = item.TenPhieu;
                worksheet.Cells[rowNumber, 4].Value = item.SoLuongDeXuat;
                worksheet.Cells[rowNumber, 5].Value = item.ChiPhiDeXuat != null ? item.ChiPhiDeXuat.Value.ToString("#,###") : string.Empty;
                worksheet.Cells[rowNumber, 6].Value = item.NgayLap;
                worksheet.Cells[rowNumber, 7].Value = item.NgayLap;
                worksheet.Cells[rowNumber, 8].Value = item.NgayCapNhat;
                worksheet.Cells[rowNumber, 9].Value = item.TrangThai;
                rowNumber++;
            });

            // Cho các ô rộng theo dữ liệu
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // Tên file
            var fileName = string.Join(".", new string[] { "Danh sách Phiếu dự trù mua sắm", "xlsx" });

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

        public async Task<int> CreateOrEdit(CreateDuTruInput input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            input.MaPhieu = GlobalFunction.RegexFormat(input.MaPhieu);
            input.TenPhieu = GlobalFunction.RegexFormat(input.TenPhieu);
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

        private async Task Create(CreateDuTruInput input)
        {
            List<PhieuDuTruMuaSamChiTiet> phieuDuTruList = new List<PhieuDuTruMuaSamChiTiet>();
            List<PhieuDuTruMuaSamDinhKemFile> phieuDuTruDinhKem = new List<PhieuDuTruMuaSamDinhKemFile>();

            if (input.ListPhieuChiTiet != null)
            {
                phieuDuTruList.AddRange(input.ListPhieuChiTiet.Select(e => new PhieuDuTruMuaSamChiTiet
                {
                    TenTaiSan = e.TenTaiSan,
                    ProductNumber = e.ProductNumber,
                    HangSanXuat = e.HangSanXuat,
                    NhaCungCap = e.NhaCungCap,
                    SoLuong = e.SoLuong,
                    DonGia = e.DonGia,
                    GhiChu = e.GhiChu,
                }));
            }

            if (input.ListPhieuChiTiet != null)
            {
                phieuDuTruDinhKem.AddRange(input.ListDinhKem.Select(e => new PhieuDuTruMuaSamDinhKemFile
                {
                    LinkFile = e.LinkFile,
                    PhanLoaiId = e.PhanLoaiId,
                    TenFile = e.TenFile,
                }));
            }

            input.NgayLapPhieuStr = DateTime.Now.Date.ToString("dd/MM/yyyy");
            var create = this.ObjectMapper.Map<PhieuDuTruMuaSam>(input);
            create.TrangThaiId = (int)GlobalConst.TrangThaiDuTruMuaSamConst.KhoiTao;
            create.PhieuDuTruMuaSamChiTietList = phieuDuTruList;
            create.PhieuDuTruMuaSamDinhKemFile = phieuDuTruDinhKem;
            await this.phieuDuTruMuaSamRepository.InsertAndGetIdAsync(create);
        }

        private async Task Update(CreateDuTruInput input)
        {
            var update = await this.phieuDuTruMuaSamRepository.FirstOrDefaultAsync((int)input.Id);
            update.Id = (long)input.Id;
            update.MaPhieu = input.MaPhieu;
            update.TenPhieu = input.TenPhieu;
            update.NguoiLapPhieuId = input.NguoiLapPhieuId;
            update.ToChucId = input.ToChucId;
            var updateChiTiet = this.phieuDuTruMuaSamChiTietRepository.GetAllList(w => w.PhieuDuTruMuaSamId == input.Id);
            var delete = updateChiTiet.Select(w => w.Id).Except(input.ListPhieuChiTiet.Select(w => w.Id));
            var insert = input.ListPhieuChiTiet.Select(w => w.Id).Except(updateChiTiet.Select(w => w.Id));
            foreach (var itemXoa in delete)
            {
                await this.phieuDuTruMuaSamChiTietRepository.DeleteAsync(w => w.Id == itemXoa);
            }

            var listInsert = input.ListPhieuChiTiet.FindAll(w => w.Id == 0);
            foreach (var itemUpdate in listInsert)
            {
                var create = new PhieuDuTruMuaSamChiTiet
                {
                    PhieuDuTruMuaSamId = input.Id,
                    TenTaiSan = itemUpdate.TenTaiSan,
                    ProductNumber = itemUpdate.ProductNumber,
                    HangSanXuat = itemUpdate.HangSanXuat,
                    NhaCungCap = itemUpdate.NhaCungCap,
                    SoLuong = itemUpdate.SoLuong,
                    DonGia = itemUpdate.DonGia,
                    GhiChu = itemUpdate.GhiChu,
                };
                await this.phieuDuTruMuaSamChiTietRepository.InsertAndGetIdAsync(create);
            }

            foreach (var item in input.ListDinhKem)
            {
                var file = new PhieuDuTruMuaSamDinhKemFile
                {
                    PhieuDuTruMuaSamId = input.Id,
                    LinkFile = item.LinkFile,
                    TenFile = item.TenFile,
                    PhanLoaiId = item.PhanLoaiId,
                };

                await this.phieuDuTruMuaSamDinhKemFileRepository.InsertAsync(file);
            }

            await this.phieuDuTruMuaSamDinhKemFileRepository.DeleteAsync(w => w.PhieuDuTruMuaSamId == (int)input.Id);
        }
    }
}
