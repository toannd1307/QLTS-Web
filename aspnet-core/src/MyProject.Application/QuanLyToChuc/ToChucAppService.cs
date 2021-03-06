namespace MyProject.QuanLyToChuc
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
    using Abp.Domain.Uow;
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
    using MyProject.QuanLyToChuc.Dtos;
    using MyProject.Shared;
    using OfficeOpenXml;
    using OfficeOpenXml.Style;

    public class ToChucAppService : MyProjectAppServiceBase, IToChucAppService
    {
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IRepository<ViTriDiaLy> viTriDiaLyRepository;
        private readonly IRepository<User, long> userRepository;
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IAppFolders appFolders;

        public ToChucAppService(
                                IRepository<ToChuc> toChucRepository,
                                IRepository<ViTriDiaLy> viTriDiaLyRepository,
                                IRepository<User, long> userRepository,
                                IRepository<TaiSan> taiSanRepository,
                                IAppFolders appFolders)
        {
            this.userRepository = userRepository;
            this.taiSanRepository = taiSanRepository;
            this.viTriDiaLyRepository = viTriDiaLyRepository;
            this.toChucRepository = toChucRepository;
            this.appFolders = appFolders;
        }

        public async Task<List<ToChucTreeTableForViewDto>> GetAllAsync(ToChucGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            var filter = await (from toChuc in this.toChucRepository.GetAll()
                                from diaChi in this.viTriDiaLyRepository.GetAll().Where(e => e.Id == toChuc.ViTriDiaLyId)
                                select new ToChucForViewDto
                                {
                                    ToChuc = toChuc,
                                    DiaChi = diaChi.TenViTri,
                                }).ToListAsync();

            foreach (var item in filter)
            {
                item.MaHe10 = GlobalFunction.ConverHexaToDecimal(item.ToChuc.MaHexa);
            }

            List<ToChucTreeTableForViewDto> listTong = this.GetToChucTreeTableChildren(filter, null);
            List<ToChucTreeTableForViewDto> result = new List<ToChucTreeTableForViewDto>();

            var filterText = GlobalFunction.RegexFormat(input.Keyword ?? string.Empty).ToLower();
            if (!string.IsNullOrEmpty(filterText))
            {
                List<ToChucTreeTableForViewDto> filterItems = new List<ToChucTreeTableForViewDto>();
                foreach (var item in listTong)
                {
                    ToChucTreeTableForViewDto newItem = this.FilterItem(item, filterText);
                    if (newItem != null)
                    {
                        filterItems.Add(newItem);
                    }
                }

                result = filterItems;
            }
            else
            {
                result = listTong;
            }

            return await Task.FromResult(result);
        }

        public async Task<ToChucCreateInputDto> GetForEditAsync(EntityDto input, bool isView)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            var entity = await this.toChucRepository.FirstOrDefaultAsync(input.Id);
            var edit = this.ObjectMapper.Map<ToChucCreateInputDto>(entity);
            return await Task.FromResult(edit);
        }

        public async Task<int> CreateOrEdit(ToChucCreateInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            input.MaToChuc = GlobalFunction.RegexFormat(input.MaToChuc);
            input.TenToChuc = GlobalFunction.RegexFormat(input.TenToChuc);
            input.GhiChu = GlobalFunction.RegexFormat(input.GhiChu);

            int isExist = this.CheckExist(input);
            if (isExist > 0)
            {
                return isExist;
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

            // Check c?? ng?????i d??ng trong ph??ng ban ch??a
            var soNhanVienPhongBan = await this.userRepository.CountAsync(e => e.ToChucId == input.Id);
            var soTaiSanPhongBan = await this.taiSanRepository.CountAsync(e => e.ToChucId == input.Id);
            if (soNhanVienPhongBan > 0)
            {
                throw new UserFriendlyException(string.Format(StringResources.PhongBanKhongTheXoa, "nh??n vi??n"));
            }
            else if (soNhanVienPhongBan > 0)
            {
                throw new UserFriendlyException(string.Format(StringResources.NullParameter, "t??i s???n"));
            }

            // Check c?? t??i s???n c???a ph??ng ban ch??a
            await this.toChucRepository.DeleteAsync((int)input.Id);
        }

        public async Task<FileDto> ExportToExcel(ToChucGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // L???y danh s??ch c???n xu???t excel
            var list = await this.GetAllAsync(input);

            var toChucIdList = new List<ToChucForExportDto>();
            foreach (var lv1 in list)
            {
                this.PrintNodesRecursive(lv1, toChucIdList);
            }

            using var package = new ExcelPackage();

            // Add sheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ToChuc");

            var namedStyle = package.Workbook.Styles.CreateNamedStyle("HyperLink");
            namedStyle.Style.Font.UnderLine = true;
            namedStyle.Style.Font.Color.SetColor(Color.Blue);

            // set header
            worksheet.Cells[1, 1].Value = "M?? ph??ng ban";
            worksheet.Cells[1, 2].Value = "T??n ph??ng ban";
            worksheet.Cells[1, 3].Value = "?????a ch???";

            // B??i ?????m header
            using (ExcelRange r = worksheet.Cells[1, 1, 1, 3])
            {
                using var f = new Font("Calibri", 12, FontStyle.Bold);
                r.Style.Font.SetFromFont(f);
                r.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Gan gia tri
            var rowNumber = 2;
            toChucIdList.ToList().ForEach(item =>
            {
                worksheet.Cells[rowNumber, 1].Value = item.MaToChuc;
                worksheet.Cells[rowNumber, 2].Value = item.TenToChuc;
                worksheet.Cells[rowNumber, 3].Value = item.DiaChi;
                if (item.BoiDam)
                {
                    worksheet.Cells[rowNumber, 1, rowNumber, 3].Style.Font.Bold = true;
                }

                rowNumber++;
            });

            // Cho c??c ?? r???ng theo d??? li???u
            worksheet.Cells.AutoFitColumns(0);

            worksheet.PrinterSettings.FitToHeight = 1;

            // T??n file
            var fileName = "ToChuc.xlsx";

            // L??u file v??o server
            using (var stream = new MemoryStream())
            {
                package.SaveAs(stream);
            }

            var file = new FileDto(fileName, MimeTypeNames.ApplicationVndOpenxmlformatsOfficedocumentSpreadsheetmlSheet);
            var filePath = Path.Combine(this.appFolders.TempFileDownloadFolder, file.FileToken);
            package.SaveAs(new FileInfo(filePath));
            return file;
        }

        public async Task<List<ToChucForExportDto>> GetAllToChucForExcel()
        {
            var filter = await (from toChuc in this.toChucRepository.GetAll()
                                from diaChi in this.viTriDiaLyRepository.GetAll().Where(e => e.Id == toChuc.ViTriDiaLyId)
                                select new ToChucForViewDto
                                {
                                    ToChuc = toChuc,
                                    DiaChi = diaChi.TenViTri,
                                }).ToListAsync();

            List<ToChucTreeTableForViewDto> listTong = this.GetToChucTreeTableChildren(filter, null);
            return null;
        }

        /// <summary>
        /// Import excel.
        /// </summary>
        /// <param name="filePath">???????ng d???n file tr??n server.</param>
        /// <returns>Danh s??ch ???????ng d???n.</returns>
        public async Task<string> ImportFileExcel(string filePath)
        {
            StringBuilder returnMessage = new StringBuilder();
            returnMessage.Append("K???t qu??? nh???p file: ");
            ReadFromExcelDto<ToChucCreateInputDto> readResult = new ReadFromExcelDto<ToChucCreateInputDto>();

            // Kh??ng t??m th???y file
            if (!File.Exists(filePath))
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.FileNotFound;
            }

            // ?????c h???t file excel
            var data = await GlobalFunction.ReadFromExcel(filePath);

            // Kh??ng c?? d??? li???u
            if (data.Count <= 0)
            {
                readResult.ResultCode = (int)GlobalConst.ReadExcelResultCodeConst.CantReadData;
            }
            else
            {
                // ?????c l???n l?????t t???ng d??ng
                for (int i = 0; i < data.Count; i++)
                {
                    try
                    {
                        string maToChuc = GlobalFunction.RegexFormat(data[i][0]);
                        string tenToChuc = GlobalFunction.RegexFormat(data[i][1]);
                        string phongBanCha = GlobalFunction.RegexFormat(data[i][2]);
                        string viTri = GlobalFunction.RegexFormat(data[i][3]);

                        var queryTC = this.toChucRepository.FirstOrDefault(e => e.TenToChuc.ToLower() == phongBanCha.ToLower());
                        var queryViTriDiaLy = this.viTriDiaLyRepository.FirstOrDefault(e => e.TenViTri.ToLower() == viTri.ToLower());

                        var create = new ToChucCreateInputDto
                        {
                            MaToChuc = maToChuc,
                            TenToChuc = tenToChuc,
                            TrucThuocToChucId = queryTC?.Id,
                            ViTriDiaLyId = queryViTriDiaLy?.Id,
                        };

                        // N???u kh??ng b??? tr??ng
                        if ((string.IsNullOrEmpty(phongBanCha) || queryTC != null) && queryViTriDiaLy != null && await this.CreateOrEdit(create) == 0)
                        {
                            // ????nh d???u c??c b???n ghi th??m th??nh c??ng
                            readResult.ListResult.Add(create);
                        }
                        else
                        {
                            // ????nh d???u c??c b???n ghi l???i
                            readResult.ListErrorRow.Add(data[i]);
                            readResult.ListErrorRowIndex.Add(i + 1);
                        }
                    }
                    catch
                    {
                        // ????nh d???u c??c b???n ghi l???i
                        readResult.ListErrorRow.Add(data[i]);
                        readResult.ListErrorRowIndex.Add(i + 1);
                    }
                }
            }

            // Th??ng tin import
            readResult.ErrorMessage = GlobalModel.ReadExcelResultCodeSorted[readResult.ResultCode];

            // N???u ?????c file th???t b???i
            if (readResult.ResultCode != 200)
            {
                return readResult.ErrorMessage;
            }
            else
            {
                // ?????c file th??nh c??ng
                // Tr??? k???t qu??? import
                returnMessage.Append(string.Format("\r\n\u00A0-T???ng ghi: {0}", readResult.ListResult.Count + readResult.ListErrorRow.Count));
                returnMessage.Append(string.Format("\r\n\u00A0-S??? b???n ghi th??nh c??ng: {0}", readResult.ListResult.Count));
                returnMessage.Append(string.Format("\r\n\u00A0-S??? b???n ghi th???t b???i: {0}", readResult.ListErrorRow.Count));
                if (readResult.ListErrorRowIndex.Count > 0)
                {
                    returnMessage.Append(string.Format("\r\n\u00A0-C??c d??ng th???t b???i: {0}", string.Join(", ", readResult.ListErrorRowIndex)));
                }
            }

            return returnMessage.ToString();
        }

        /// <summary>
        /// T???i file m???u cho import.
        /// </summary>
        /// <returns>File m???u.</returns>
        public async Task<FileDto> DownloadFileMau()
        {
            string fileName = "ToChucImport.xlsx";
            try
            {
                // _appFolders.DemoFileDownloadFolder : Th?? m???c ch??a file m???u c???n t???i
                // _appFolders.TempFileDownloadFolder : Kh??ng ???????c s???a
                return await GlobalFunction.DownloadFileMau(fileName, this.appFolders.ToChucFileDownloadFolder, this.appFolders.TempFileDownloadFolder);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException("C?? l???i: " + ex.Message);
            }
        }

        public async Task<List<LookupTableDto>> GetAllToChucCha()
        {
            var result = await this.toChucRepository.GetAll().Where(e => e.TrucThuocToChucId == null || e.TrucThuocToChucId == 1)
                .Select(e => new LookupTableDto
                {
                    Id = e.Id,
                    DisplayName = e.TenToChuc,
                }).ToListAsync();

            return result;
        }

        private ToChucTreeTableForViewDto FilterItem(ToChucTreeTableForViewDto item, string filterText)
        {
            if (item.Data.ToChuc.TenToChuc.ToLower().Contains(filterText) || item.Data.ToChuc.MaToChuc.ToLower().Contains(filterText)
                || item.Data.ToChuc.MaHexa.ToLower().Contains(filterText))
            {
                return item;
            }
            else
            {
                if (item.Children != null)
                {
                    List<ToChucTreeTableForViewDto> children = new List<ToChucTreeTableForViewDto>();

                    item.Children.ForEach(child =>
                    {
                        var newChild = this.FilterItem(child, filterText);
                        if (newChild != null)
                        {
                            children.Add(newChild);
                        }
                    });

                    if (children.Count > 0)
                    {
                        var newItem = item;
                        newItem.Expanded = true;
                        newItem.Children = children;
                        return newItem;
                    }
                }
            }

            return null;
        }

        private int CheckExist(ToChucCreateInputDto input)
        {
            // L???y h???t id cha con c???a th???ng cha
            input.MaToChuc = GlobalFunction.RegexFormat(input.MaToChuc);
            input.TenToChuc = GlobalFunction.RegexFormat(input.TenToChuc);

            // N???u query > 0 th?? l?? b??? tr??ng m?? => return true
            var query = this.toChucRepository.GetAll().Where(e => e.TrucThuocToChucId == input.TrucThuocToChucId
                                && (e.MaToChuc == input.MaToChuc || e.TenToChuc == input.TenToChuc || e.MaHexa == input.MaHexa))
                .WhereIf(input.Id != null, e => e.Id != input.Id).FirstOrDefault();
            if (query != null)
            {
                if (query.MaToChuc.ToLower() == input.MaToChuc.ToLower())
                {
                    return 1;
                }
                else if (query.TenToChuc.ToLower() == input.TenToChuc.ToLower())
                {
                    return 2;
                }
                else if (query.MaHexa.ToLower() == input.MaHexa.ToLower())
                {
                    return 3;
                }
            }

            return 0;
        }

        private async Task Create(ToChucCreateInputDto input)
        {
            input.MaHexa = this.GenHexaCode((int)input.TrucThuocToChucId);
            var create = this.ObjectMapper.Map<ToChuc>(input);
            await this.toChucRepository.InsertAndGetIdAsync(create);
        }

        private async Task Update(ToChucCreateInputDto input)
        {
            var update = await this.toChucRepository.FirstOrDefaultAsync((int)input.Id);
            this.ObjectMapper.Map(input, update);
        }

        private List<ToChucTreeTableForViewDto> GetToChucTreeTableChildren(List<ToChucForViewDto> listTong, int? id)
        {
            return listTong.Where(w => w.ToChuc.TrucThuocToChucId == id).OrderBy(e => e.ToChuc.TenToChuc).Select(w => new ToChucTreeTableForViewDto
            {
                Expanded = true,
                Data = w,
                Children = this.GetToChucTreeTableChildren(listTong, w.ToChuc.Id),
            }).ToList();
        }

        private void PrintNodesRecursive(ToChucTreeTableForViewDto oParentNode, List<ToChucForExportDto> list, string prePad = "")
        {
            if (oParentNode.Data.ToChuc.TrucThuocToChucId == null)
            {
                list.Add(new ToChucForExportDto
                {
                    MaToChuc = oParentNode.Data.ToChuc.MaToChuc,
                    TenToChuc = oParentNode.Data.ToChuc.TenToChuc,
                    DiaChi = oParentNode.Data.DiaChi,
                    BoiDam = true,
                });
                prePad = string.Empty;
            }

            prePad += "-";

            // Start recursion on all subnodes.
            foreach (ToChucTreeTableForViewDto oSubNode in oParentNode.Children)
            {
                list.Add(new ToChucForExportDto
                {
                    MaToChuc = prePad + oSubNode.Data.ToChuc.MaToChuc,
                    TenToChuc = oSubNode.Data.ToChuc.TenToChuc,
                    DiaChi = oSubNode.Data.DiaChi,
                    BoiDam = false,
                });
                this.PrintNodesRecursive(oSubNode, list, prePad);
            }
        }

        private string GenHexaCode(int donViQuanLyId)
        {
            return GlobalFunction.ConverDecimalToHexa(this.GetHexaCodeMax(donViQuanLyId), 2);
        }

        private int GetHexaCodeMax(int donViQuanLyId)
        {
            var query = this.toChucRepository.GetAll().Where(e => e.TrucThuocToChucId == donViQuanLyId).ToList();
            var hexaMax = query.Count > 0 ? query.Max(e => GlobalFunction.ConverHexaToDecimal(e.MaHexa)) : 0;
            return hexaMax + 1;
        }
    }
}