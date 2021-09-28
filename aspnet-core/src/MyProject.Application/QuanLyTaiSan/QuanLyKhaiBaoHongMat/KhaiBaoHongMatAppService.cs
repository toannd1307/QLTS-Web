namespace MyProject.KhaiBaoHongMat
{
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
    using MyProject.Authorization;
    using MyProject.Global;
    using MyProject.QuanLyTaiSan.Dtos;

    public class KhaiBaoHongMatAppService : MyProjectAppServiceBase, IKhaiBaoHongMatAppService
    {
        private readonly IRepository<ViewKhaiBaoHongMat, long> vKhaiBaoHongMatRespository;
        private readonly IRepository<ToChuc> toChucRespository;
        private readonly IRepository<ViewKhaiBaoHongMatChiTiet, long> vKhaiBaoHongMatChiTietRespository;

        public KhaiBaoHongMatAppService(
            IRepository<ViewKhaiBaoHongMat, long> vKhaiBaoHongMatRespository,
            IRepository<ViewKhaiBaoHongMatChiTiet, long> vKhaiBaoHongMatChiTietRespository,
            IRepository<ToChuc> toChucRespository)
        {
            this.vKhaiBaoHongMatRespository = vKhaiBaoHongMatRespository;
            this.toChucRespository = toChucRespository;
            this.vKhaiBaoHongMatChiTietRespository = vKhaiBaoHongMatChiTietRespository;
        }

        public async Task<PagedResultDto<ViewKhaiBaoHongMat>> GetAllKhaiBaoHongMatAsync(KhaiBaoHongMatGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }

            if (input.PhongBanqQL == null || input.PhongBanqQL.Count == 0)
            {
                input.PhongBanqQL = new List<int> { -1 };
            }
            #endregion

            var checkPermission = await this.PermissionChecker.IsGrantedAsync(PermissionNames.Pages_QuanLyTaiSanTong);
            var phongBanCuaNguoiDung = GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRespository, (int)this.UserManager.GetUserById((long)this.AbpSession.UserId).ToChucId).Result;

            if (!checkPermission)
            {
                // Tài sản hỏng
                var items = this.vKhaiBaoHongMatRespository.GetAll().Where(w => phongBanCuaNguoiDung.Contains((int)w.PhongBanKhaiBaoId)).OrderByDescending(o => o.CreationTime).AsQueryable();

                var query = items.Where(w => 1 == 0);

                if (!string.IsNullOrEmpty(input.TimKiemKhaiBao))
                {
                    input.TimKiemKhaiBao = input.TimKiemKhaiBao.Trim();

                    // Xử lí filter theo ngày
                    DateTime? ngayKhaoBao = null;
                    if (GlobalFunction.ConvertStringToDateTime(input.TimKiemKhaiBao) != null)
                    {
                        ngayKhaoBao = GlobalFunction.ConvertStringToDateTime(input.TimKiemKhaiBao).Value.Date;
                        query = items.Where(w => w.NgayKhaiBao.Value.Date == ngayKhaoBao);
                    }

                    query = query.Union(items.Where(w => w.NoiDungKhaiBao.Contains(GlobalFunction.RegexFormat(input.TimKiemKhaiBao))
                    || w.NguoiKhaiBao.Contains(GlobalFunction.RegexFormat(input.TimKiemKhaiBao))));
                }
                else
                {
                    query = items;
                }

                query = query.Where(w => input.PhongBanqQL.Contains((int)w.PhongBanKhaiBaoId));

                if (input.KhaiBao != null)
                {
                    query = query.Where(w => w.KhaiBao == input.KhaiBao);
                }

                int totalCount = await query.CountAsync();
                if (!string.IsNullOrEmpty(input.Sorting))
                {
                    query = query.OrderBy(input.Sorting);
                }

                var output = await query.PageBy(input)
                                        .ToListAsync();

                return new PagedResultDto<ViewKhaiBaoHongMat>(totalCount, output);
            }
            else
            {
                // Tài sản hỏng
                var items = this.vKhaiBaoHongMatRespository.GetAll().OrderByDescending(o => o.CreationTime).AsQueryable();

                var query = items.Where(w => 1 == 0);

                if (!string.IsNullOrEmpty(input.TimKiemKhaiBao))
                {
                    input.TimKiemKhaiBao = input.TimKiemKhaiBao.Trim();

                    // Xử lí filter theo ngày
                    DateTime? ngayKhaoBao = null;
                    if (GlobalFunction.ConvertStringToDateTime(input.TimKiemKhaiBao) != null)
                    {
                        ngayKhaoBao = GlobalFunction.ConvertStringToDateTime(input.TimKiemKhaiBao).Value.Date;
                        query = items.Where(w => w.NgayKhaiBao.Value.Date == ngayKhaoBao);
                    }

                    query = query.Union(items.Where(w => w.NoiDungKhaiBao.Contains(GlobalFunction.RegexFormat(input.TimKiemKhaiBao))
                    || w.NguoiKhaiBao.Contains(GlobalFunction.RegexFormat(input.TimKiemKhaiBao))));
                }
                else
                {
                    query = items;
                }

                query = query.Where(w => input.PhongBanqQL.Contains((int)w.PhongBanKhaiBaoId));

                if (input.KhaiBao != null)
                {
                    query = query.Where(w => w.KhaiBao == input.KhaiBao);
                }

                int totalCount = await query.CountAsync();
                if (!string.IsNullOrEmpty(input.Sorting))
                {
                    query = query.OrderBy(input.Sorting);
                }

                var output = await query.PageBy(input)
                                        .ToListAsync();

                return new PagedResultDto<ViewKhaiBaoHongMat>(totalCount, output);
            }
        }

        public async Task<PagedResultDto<ViewKhaiBaoHongMatChiTiet>> GetAllKhaiBaoHongMatChiTiet(KhaiBaoHongMatChiTietGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            // Tài sản hỏng
            var query = this.vKhaiBaoHongMatChiTietRespository.GetAll().Where(w => w.PhieuTaiSanId == input.Id).OrderByDescending(o => o.LastModificationTime).AsQueryable();

            int totalCount = await query.CountAsync();
            if (!string.IsNullOrEmpty(input.Sorting))
            {
                query = query.OrderBy(input.Sorting);
            }

            var output = await query.PageBy(input)
                                    .ToListAsync();

            return new PagedResultDto<ViewKhaiBaoHongMatChiTiet>(totalCount, output);
        }
    }
}
