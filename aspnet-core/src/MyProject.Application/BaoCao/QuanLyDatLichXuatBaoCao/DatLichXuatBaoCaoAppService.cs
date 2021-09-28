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
using MyProject.BaoCao.QuanLyDatLichXuatBaoCao.Dto;
using MyProject.Global;
using MyProject.QuanLyDatLichXuatBaoCao.Dto;

namespace MyProject.QuanLyDatLichXuatBaoCao
{
    public class DatLichXuatBaoCaoAppService : MyProjectAppServiceBase, IDatLichXuatBaoCaoAppService
    {
        private readonly IRepository<DatLichXuatBaoCao> datLichRepository;

        public DatLichXuatBaoCaoAppService(IRepository<DatLichXuatBaoCao> datLichRepository)
        {
            this.datLichRepository = datLichRepository;
        }

        public async Task<PagedResultDto<GetAllDatLichBCDtos>> GetAllDatLich(InputGetAllDatLichDto input)
        {
            try
            {
                IQueryable<GetAllDatLichBCDtos> query = null;

                query = from dl in this.datLichRepository.GetAll()
                        select new GetAllDatLichBCDtos
                        {
                            Id = dl.Id,
                            TenBaoCao = GlobalModel.TenBaoCaoSorted[(int)dl.BaoCaoId],
                            LapLai = GlobalModel.LapLaiSorted[(int)dl.LapLaiId],
                            NgayGio = string.Empty,
                            GioGuiBC = dl.GioGuiBaoCao.Value.ToString("HH:mm"),
                            NgayGuiTuan = dl.NgayGuiBaoCaoTheoTuan,
                            NgayGuiThang = dl.NgayGuiBaoCaoTheoThang,
                            NgayGuiNam = dl.NgayGuiBaoCaoTheoNam,
                            NgayTao = dl.CreationTime,
                            NgayCapNhat = dl.LastModificationTime,
                        };

                int totalCount = await query.CountAsync();
                var output = await query.OrderBy(input.Sorting ?? "NgayTao DESC").PageBy(input).ToListAsync();
                foreach (var item in output)
                {
                    List<string> result = new List<string>();

                    var phanTachTime = item.GioGuiBC.ToString().Split(':').ToList<string>();

                    if (item.LapLai.Equals("Ngày"))
                    {
                        item.NgayGio = phanTachTime[0] + "h" + phanTachTime[1] + " hàng ngày";
                    }
                    else if (item.LapLai.Equals("Tuần"))
                    {
                        item.NgayGio = phanTachTime[0] + "h" + phanTachTime[1] + " " + GlobalModel.ThuSorted[(int)item.NgayGuiTuan];
                    }
                    else if (item.LapLai.Equals("Tháng"))
                    {
                        item.NgayGio = phanTachTime[0] + "h" + phanTachTime[1] + " " + GlobalModel.NgaySorted[(int)item.NgayGuiThang] + " hàng tháng";
                    }
                    else if (item.LapLai.Equals("Quý"))
                    {
                        var ngayTaoCV = item.NgayTao.ToString("MM");

                        if (item.NgayCapNhat != null)
                        {
                            ngayTaoCV = item.NgayCapNhat?.ToString("MM");
                        }

                        List<string> quyI = new List<string>() { "01", "02", "03" };
                        List<string> quyII = new List<string>() { "04", "05", "06" };
                        List<string> quyIII = new List<string>() { "07", "08", "09" };
                        List<string> quyIV = new List<string>() { "10", "11", "12" };
                        if (quyI.Contains(ngayTaoCV))
                        {
                            item.NgayGio = phanTachTime[0] + "h" + phanTachTime[1] + " 01 Tháng 04";
                        }
                        else if (quyII.Contains(ngayTaoCV))
                        {
                            item.NgayGio = phanTachTime[0] + "h" + phanTachTime[1] + " 01 Tháng 07";
                        }
                        else if (quyIII.Contains(ngayTaoCV))
                        {
                            item.NgayGio = phanTachTime[0] + "h" + phanTachTime[1] + " 01 Tháng 10";
                        }
                        else if (quyIV.Contains(ngayTaoCV))
                        {
                            item.NgayGio = phanTachTime[0] + "h" + phanTachTime[1] + " 01 Tháng 01";
                        }
                    }
                    else if (item.LapLai.Equals("Năm"))
                    {
                        item.NgayGio = phanTachTime[0] + "h" + phanTachTime[1] + " " + item.NgayGuiNam + " hàng năm";
                    }
                }

                return new PagedResultDto<GetAllDatLichBCDtos>(totalCount, output);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<int> CreateOrEdit(CreateOrEditDatLichDtos input)
        {
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

        private async Task Create(CreateOrEditDatLichDtos input)
        {
            try
            {
                var create = new DatLichXuatBaoCao();
                create.BaoCaoId = input.BaoCaoId;
                create.LapLaiId = input.LapLaiId;
                create.ToChucNhanBaoCaoId = input.PhongBanNhanId;
                create.NguoiNhanBaoCaoId = input.NguoiNhanBaoCaoId;
                create.GhiChu = input.GhiChu;

                if (input.LapLaiId == 0 || input.LapLaiId == 3)
                {
                    create.GioGuiBaoCao = input.GioGuiBaoCao.ToLocalTime();
                }
                else if (input.LapLaiId == 1)
                {
                    create.NgayGuiBaoCaoTheoTuan = Convert.ToInt32(input.NgayGuiBaoCao);
                    create.GioGuiBaoCao = input.GioGuiBaoCao.ToLocalTime();
                }
                else if (input.LapLaiId == 2)
                {
                    create.NgayGuiBaoCaoTheoThang = Convert.ToInt32(input.NgayGuiBaoCao);
                    create.GioGuiBaoCao = input.GioGuiBaoCao.ToLocalTime();
                }
                else if (input.LapLaiId == 4)
                {
                    create.NgayGuiBaoCaoTheoNam = input.NgayGuiBaoCao;
                    create.GioGuiBaoCao = input.GioGuiBaoCao.ToLocalTime();
                }

                await this.datLichRepository.InsertAsync(create);
                this.CurrentUnitOfWork.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task Update(CreateOrEditDatLichDtos input)
        {
            var query = await this.datLichRepository.FirstOrDefaultAsync((int)input.Id);
            query.BaoCaoId = input.BaoCaoId;
            query.GhiChu = input.GhiChu;
            query.LapLaiId = input.LapLaiId;
            query.NguoiNhanBaoCaoId = input.NguoiNhanBaoCaoId;
            query.ToChucNhanBaoCaoId = input.PhongBanNhanId;

            if (input.LapLaiId == 0 || input.LapLaiId == 3)
            {
                query.GioGuiBaoCao = input.GioGuiBaoCao.ToLocalTime();
            }
            else if (input.LapLaiId == 1)
            {
                query.NgayGuiBaoCaoTheoTuan = Convert.ToInt32(input.NgayGuiBaoCao);
                query.GioGuiBaoCao = input.GioGuiBaoCao.ToLocalTime();
            }
            else if (input.LapLaiId == 2)
            {
                query.NgayGuiBaoCaoTheoThang = Convert.ToInt32(input.NgayGuiBaoCao);
                query.GioGuiBaoCao = input.GioGuiBaoCao.ToLocalTime();
            }
            else if (input.LapLaiId == 4)
            {
                query.NgayGuiBaoCaoTheoNam = input.NgayGuiBaoCao;
                query.GioGuiBaoCao = input.GioGuiBaoCao.ToLocalTime();
            }

            await this.datLichRepository.UpdateAsync(query);
            this.CurrentUnitOfWork.SaveChanges();
        }

        public async Task DeleteAsync(EntityDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            await this.datLichRepository.DeleteAsync((int)input.Id);
        }

        public async Task<GetValueForViewDatLich> GetForEdit(int input, bool isView)
        {
            var entity = await this.datLichRepository.GetAll().Where(w => w.Id == (int)input).FirstOrDefaultAsync();

            GetValueForViewDatLich output = new GetValueForViewDatLich();
            output.BaoCaoId = (int)entity.BaoCaoId;
            output.TenBaoCao = GlobalModel.TenBaoCaoSorted[(int)entity.BaoCaoId];
            output.LapLaiId = entity.LapLaiId;
            output.PhongBanNhan = (int)entity.ToChucNhanBaoCaoId;
            output.NguoiNhan = entity.NguoiNhanBaoCaoId;

            if (entity.LapLaiId == 0 || entity.LapLaiId == 3)
            {
                output.GioBaoCao = (DateTime)entity.GioGuiBaoCao;
            }
            else if (entity.LapLaiId == 1)
            {
                output.NgayGuiBaoCao = entity.NgayGuiBaoCaoTheoTuan.ToString();
                output.GioBaoCao = (DateTime)entity.GioGuiBaoCao;
            }
            else if (entity.LapLaiId == 2)
            {
                output.NgayGuiBaoCao = entity.NgayGuiBaoCaoTheoThang.ToString();
                output.GioBaoCao = (DateTime)entity.GioGuiBaoCao;
            }
            else if (entity.LapLaiId == 4)
            {
                output.NgayGuiBaoCao = entity.NgayGuiBaoCaoTheoNam;
                output.GioBaoCao = (DateTime)entity.GioGuiBaoCao;
            }

            output.GhiChu = entity.GhiChu;

            return output;
        }
    }
}
