namespace MyProject.QuanLyKiemKeTaiSan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.Domain.Entities;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization;
    using MyProject.Authorization.Users;
    using MyProject.Global;
    using MyProject.QuanLyKiemKeTaiSan.Dtos;
    using MyProject.QuanLyMailServer;
    using MyProject.Users.Dto;

    public class KiemKeTaiSanAppService : MyProjectAppServiceBase, IKiemKeTaiSanAppService
    {
        private const string V = "Cập nhật thành công";
        private const string V1 = "Đợt kiêm kê không có trong danh sách";
        private readonly IRepository<KiemKeTaiSan, long> kiemKeTaiSanRepository;
        private readonly IRepository<KiemKe_DoiKiemKe, long> doiKiemKeRepository;
        private readonly IRepository<KiemKe_KetQuaKiemKe, long> ketQuaKiemKeRepository;
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IRepository<ViTriDiaLy> viTriRepository;
        private readonly IRepository<LoaiTaiSan> loaiTaiSanRepository;
        private readonly IRepository<User, long> userRepository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IRepository<CanhBao> canhBaoRepository;
        private IMailServerAppService mailService;

        public KiemKeTaiSanAppService(
            IRepository<KiemKeTaiSan, long> kiemKeTaiSanRepository,
            IRepository<KiemKe_DoiKiemKe, long> doiKiemKeRepository,
            IRepository<User, long> userRepository,
            IRepository<ToChuc> toChucRepository,
            IRepository<KiemKe_KetQuaKiemKe, long> ketQuaKiemKeRepository,
            IRepository<TaiSan> taiSanRepository,
            IRepository<ViTriDiaLy> viTriRepository,
            IRepository<LoaiTaiSan> loaiTaiSanRepository,
            IRepository<CanhBao> canhBaoRepository,
            IMailServerAppService mailService)
        {
            this.kiemKeTaiSanRepository = kiemKeTaiSanRepository;
            this.doiKiemKeRepository = doiKiemKeRepository;
            this.userRepository = userRepository;
            this.toChucRepository = toChucRepository;
            this.ketQuaKiemKeRepository = ketQuaKiemKeRepository;
            this.taiSanRepository = taiSanRepository;
            this.viTriRepository = viTriRepository;
            this.loaiTaiSanRepository = loaiTaiSanRepository;
            this.canhBaoRepository = canhBaoRepository;
            this.mailService = mailService;
        }

        public async Task<PagedResultDto<KiemKeTaiSanForViewDto>> GetAllAsync(KiemKeTaiSanGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var output = new List<KiemKeTaiSanForViewDto>();
            int totalCount = 0;
            if (input.BoPhanDuocKiemKeId != null && input.BoPhanDuocKiemKeId.Count > 0)
            {
                input.StartDate = GlobalFunction.GetDateTime(input.StartDate);
                input.EndDate = GlobalFunction.GetDateTime(input.EndDate);
                var checkPermission = await this.PermissionChecker.IsGrantedAsync(PermissionNames.Pages_QuanLyTaiSanTong);
                var pbIdUser = this.userRepository.GetAll().Where(w => w.Id == this.AbpSession.UserId).Select(s => s.ToChucId).FirstOrDefault();
                var listKiemKe = await GlobalFunction.GetAllToChucIdTheoNguoiDungListAsync(this.toChucRepository, (int)pbIdUser);
                var query = this.GetKiemKeQuery(input, input.BoPhanDuocKiemKeId, checkPermission);
                totalCount = await query.CountAsync();
                output = await query.OrderBy(input.Sorting ?? "KiemKeTaiSan.MaKiemKe")
                                        .PageBy(input)
                                            .ToListAsync();
            }

            return new PagedResultDto<KiemKeTaiSanForViewDto>(totalCount, output);
        }

        public async Task<List<KetQuaKiemKeForViewDto>> GetAllTaiSanAsync(KetQuaKiemKeGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var query = this.GetKetQuaKiemKeQuery(input);
            var output = await query.OrderBy(input.Sorting ?? "MaTaiSan").ToListAsync();

            return output;
        }

        public string UpdateTinhTrangTaiSan(List<KetQuaKiemKeForUpdateDto> input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            try
            {
                input.ForEach(async e =>
                {
                    var update = this.ketQuaKiemKeRepository.GetAll().Where(x => x.Id.Equals(e.Id)).FirstOrDefault();
                    update.KetQua = e.TinhTrang;
                    await this.ketQuaKiemKeRepository.UpdateAsync(update);
                });
            }
            catch (System.Exception a)
            {
                return a.ToString();
            }

            return V;
        }

        public async Task<string> UpdateTrangThaiKiemKe(TrangThaiKiemKeForUpdateDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            try
            {
                var update = this.kiemKeTaiSanRepository.GetAll().Where(x => x.Id.Equals(input.Id)).FirstOrDefault();
                var pbId = this.kiemKeTaiSanRepository.GetAll().Where(e => e.Id.Equals(input.Id)).FirstOrDefault().BoPhanDuocKiemKeId;
                var checkKetQua = this.ketQuaKiemKeRepository.GetAll().Where(e => e.KiemKeTaiSanId.Equals(input.Id));
                var viTriId = this.toChucRepository.GetAll().Where(x => x.Id.Equals(pbId)).FirstOrDefault().ViTriDiaLyId;
                if (update != null)
                {
                    if (input.TrangThai)
                    {
                        if (update.TrangThaiId == 0)
                        {
                            var toanBoTaiSan = this.taiSanRepository.GetAll().Where(e => e.ToChucId.Equals(pbId)).ToList();

                            // Them toan bo tai san theo phong ban
                            toanBoTaiSan.ForEach(o =>
                            {
                                var check = checkKetQua.Where(e => e.TaiSanId.Equals(o.Id)).FirstOrDefault();
                                var saveKetQua = new KiemKe_KetQuaKiemKe();
                                if (check == null)
                                {
                                    saveKetQua.KetQua = 0;
                                    saveKetQua.TaiSanId = o.Id;
                                    saveKetQua.KiemKeTaiSanId = input.Id;
                                    saveKetQua.ViTriDiaLyId = viTriId;
                                    saveKetQua.TrangThaiTaiSanId = o.TrangThaiId;
                                    this.ketQuaKiemKeRepository.InsertAsync(saveKetQua);
                                }
                            });
                            update.TrangThaiId = 1;
                            update.ThoiGianBatDauThucTe = DateTime.Now;

                            // Gửi mail
                            this.mailService.SendMailBatDauKiemKe(input.Id);

                            // Gửi thông báo
                            string noiDung = string.Empty;
                            string nguoiGui = this.userRepository.FirstOrDefault(w => w.Id == this.AbpSession.UserId).Name;
                            var listUser = this.userRepository.GetAllList();
                            noiDung = nguoiGui + " đã bắt đầu " + update.TenKiemKe;

                            await GlobalFunction.GuiThongBaoAsync(this.canhBaoRepository, this.toChucRepository, listUser, this.PermissionChecker, (int)this.AbpSession.UserId, pbId, noiDung, (int)GlobalConst.CanhBaoThongBao.BatDau);
                        }
                    }
                    else if (!input.TrangThai)
                    {
                        if (update.TrangThaiId == 1)
                        {
                            update.TrangThaiId = 2;
                            update.ThoiGianKetThucThucTe = DateTime.Now;

                            // Gửi mail
                            this.mailService.SendMailKetThucKiemKe(input.Id);

                            // Gửi thông báo
                            string noiDung = string.Empty;
                            string nguoiGui = this.userRepository.FirstOrDefault(w => w.Id == this.AbpSession.UserId).Name;
                            var listUser = this.userRepository.GetAllList();
                            noiDung = nguoiGui + " đã kết thúc " + update.TenKiemKe;

                            await GlobalFunction.GuiThongBaoAsync(this.canhBaoRepository, this.toChucRepository, listUser, this.PermissionChecker, (int)this.AbpSession.UserId, pbId, noiDung, (int)GlobalConst.CanhBaoThongBao.KetThuc);
                        }
                    }
                }
                else
                {
                    return V1;
                }

                await this.kiemKeTaiSanRepository.UpdateAsync(update);
            }
            catch (System.Exception e)
            {
                return e.ToString();
            }

            return V;
        }

        public async Task<KiemKeTaiSanCreateInputDto> GetForEditAsync(EntityDto input, bool isView)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            var entity = await this.kiemKeTaiSanRepository.FirstOrDefaultAsync(input.Id);
            var edit = this.ObjectMapper.Map<KiemKeTaiSanCreateInputDto>(entity);
            return await Task.FromResult(edit);
        }

        public string CreateKetQuaKiemKe(KetQuaKiemKeCreateDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            else if (input.Code == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter, input.ToString());
            }
            #endregion
            try
            {
                List<string> kiemKeThemSuccess = new List<string>();

                // Lay phong ban ID
                var pbId = this.kiemKeTaiSanRepository.GetAll().Where(e => e.Id.Equals(input.KiemKeTaiSanId)).FirstOrDefault().BoPhanDuocKiemKeId;
                var viTriId = this.toChucRepository.GetAll().Where(x => x.Id.Equals(pbId)).FirstOrDefault().ViTriDiaLyId;

                // var toanBoTaiSan = this.taiSanRepository.GetAll().Where(e => e.ToChucId.Equals(pbId)).ToList();
                var checkKetQua = this.ketQuaKiemKeRepository.GetAll().Where(e => e.KiemKeTaiSanId.Equals(input.KiemKeTaiSanId));

                // Them toan bo tai san theo phong ban
                // toanBoTaiSan.ForEach(o =>
                // {
                //    var check = checkKetQua.Where(e => e.TaiSanId.Equals(o.Id)).FirstOrDefault();
                //    var saveKetQua = new KiemKe_KetQuaKiemKe();
                //    if (check == null)
                //    {
                //        saveKetQua.DauDocId = input.DauDocId;
                //        saveKetQua.KetQua = 0;
                //        saveKetQua.TaiSanId = o.Id;
                //        saveKetQua.KiemKeTaiSanId = input.KiemKeTaiSanId;
                //        saveKetQua.ViTriDiaLyId = viTriId;
                //        saveKetQua.TrangThaiTaiSanId = o.TrangThaiId;
                //        this.ketQuaKiemKeRepository.InsertAsync(saveKetQua);
                //        kiemKeThemSuccess.Add(o.TenTaiSan.ToString());
                //    }
                // });

                // Trường hợp có dữ liệu
                if (input.Code.Count > 0)
                {
                    input.Code.ForEach(code =>
                    {
                        var create = new KiemKe_KetQuaKiemKe();
                        create.KiemKeTaiSanId = input.KiemKeTaiSanId;
                        create.DauDocId = input.DauDocId;
                        create.ViTriDiaLyId = viTriId;
                        var taiSanTimThay = this.taiSanRepository.GetAll().Where(e => e.ToChucId.Equals(pbId) && (e.QRCode.Equals(code) || e.RFIDCode.Equals(code) || e.BarCode.Equals(code))).FirstOrDefault();
                        if (taiSanTimThay != null)
                        {
                            var checkTaiSan = checkKetQua.Where(e => e.TaiSanId.Equals(taiSanTimThay.Id)).FirstOrDefault();
                            if (checkTaiSan != null)
                            {
                                if (checkTaiSan.KetQua == 0)
                                {
                                    checkTaiSan.KetQua = 1;
                                    this.ketQuaKiemKeRepository.UpdateAsync(checkTaiSan);
                                }
                            }
                        }
                        else
                        {
                            var checkKetQuaTrung = checkKetQua.Where(e => e.Code.Equals(code)).FirstOrDefault();
                            if (checkKetQuaTrung == null)
                            {
                                var taiSanDanhSach = this.taiSanRepository.GetAll().Where(e => e.QRCode.Equals(code) || e.RFIDCode.Equals(code) || e.BarCode.Equals(code)).FirstOrDefault();
                                if (taiSanDanhSach != null)
                                {
                                    create.KetQua = 3;
                                    create.Code = code;
                                }
                                else
                                {
                                    create.Code = code;
                                    create.KetQua = 2;
                                }

                                this.ketQuaKiemKeRepository.InsertAsync(create);
                                kiemKeThemSuccess.Add(code.ToString());
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                return e.ToString();
            }

            return "0";
        }

        public async Task<List<UserForViewDto>> GetUserForEditAsync(EntityDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            var entity = this.doiKiemKeRepository.GetAll().Where(e => e.KiemKeTaiSanId == input.Id).Select(e => e.NguoiKiemKeId).ToList();
            var result = await (from user in this.userRepository.GetAll().Where(e => entity.Contains(e.Id))
                                from toChuc in this.toChucRepository.GetAll().Where(e => e.Id == user.ToChucId)
                                select new UserForViewDto
                                {
                                    User = this.ObjectMapper.Map<UserDto>(user),
                                    TenToChuc = toChuc.TenToChuc,
                                }).ToListAsync();
            return result;
        }

        public async Task<int> CreateOrEdit(KiemKeTaiSanCreateInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            input.MaKiemKe = GlobalFunction.RegexFormat(input.MaKiemKe);
            input.TenKiemKe = GlobalFunction.RegexFormat(input.TenKiemKe);
            input.ThoiGianBatDauDuKien = GlobalFunction.GetDateTime(input.ThoiGianBatDauDuKien);
            input.ThoiGianKetThucDuKien = GlobalFunction.GetDateTime(input.ThoiGianKetThucDuKien);

            if (this.CheckExist(input.MaKiemKe, input.Id))
            {
                return 1;
            }

            if (input.Id == 0)
            {
                await this.Create(input);
            }
            else
            {
                await this.Update(input);
            }

            return 0;
        }

        public async Task DeleteAsync(List<long> input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            await this.kiemKeTaiSanRepository.DeleteAsync(x => input.Contains(x.Id));
        }

        private bool CheckExist(string ma, int? id)
        {
            ma = GlobalFunction.RegexFormat(ma);

            // Nếu query > 0 thì là bị trùng mã => return true
            var query = this.kiemKeTaiSanRepository.GetAll().Where(e => e.MaKiemKe == ma)
                .WhereIf(id != null, e => e.Id != id).Count();
            return query > 0;
        }

        private async Task Create(KiemKeTaiSanCreateInputDto input)
        {
            var create = this.ObjectMapper.Map<KiemKeTaiSan>(input);
            create.DoiKiemKeList = input.DoiKiemKeIdList.Select(e => new KiemKe_DoiKiemKe()
            {
                NguoiKiemKeId = e,
            }).ToList();

            await this.kiemKeTaiSanRepository.InsertAndGetIdAsync(create);
        }

        private async Task Update(KiemKeTaiSanCreateInputDto input)
        {
            var update = await this.kiemKeTaiSanRepository.FirstOrDefaultAsync((int)input.Id);
            var doiKiemKe = this.doiKiemKeRepository.GetAll().Where(e => e.KiemKeTaiSanId == (int)input.Id).ToList();
            doiKiemKe.ForEach(e =>
            {
                e.IsDeleted = true;
                this.doiKiemKeRepository.UpdateAsync(e);
            });
            update.DoiKiemKeList = input.DoiKiemKeIdList.Select(e => new KiemKe_DoiKiemKe()
            {
                NguoiKiemKeId = e,
                IsDeleted = false,
            }).ToList();
            this.ObjectMapper.Map(input, update);
        }

        private IQueryable<KiemKeTaiSanForViewDto> GetKiemKeQuery(KiemKeTaiSanGetAllInputDto input, List<int> listKiemKe, bool isPremission)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            IQueryable<KiemKeTaiSan> filter = null;
            filter = this.kiemKeTaiSanRepository.GetAll()
                            .Where(e => listKiemKe.Contains((int)e.BoPhanDuocKiemKeId))
                            .WhereIf(!string.IsNullOrEmpty(input.Keyword), e => e.MaKiemKe.Contains(GlobalFunction.RegexFormat(input.Keyword)) || e.TenKiemKe.Contains(GlobalFunction.RegexFormat(input.Keyword)));

            var phongBan = this.toChucRepository.GetAll();
            var query = from o in filter
                        from i in this.toChucRepository.GetAll().Where(e => e.Id.Equals(o.BoPhanDuocKiemKeId))
                        from phongBanCha in this.toChucRepository.GetAll().Where(w => w.Id == i.TrucThuocToChucId).DefaultIfEmpty()
                        select new KiemKeTaiSanForViewDto()
                        {
                            KiemKeTaiSan = o,
                            PhongBan = (i.TrucThuocToChucId > 1 ? (phongBanCha.MaToChuc + " - ") : string.Empty) + i.TenToChuc,
                        };
            query = query
                .WhereIf(input.BoPhanDuocKiemKeId != null, item => input.BoPhanDuocKiemKeId.Contains((int)item.KiemKeTaiSan.BoPhanDuocKiemKeId))
                .WhereIf(input.TrangThaiId != null, item => item.KiemKeTaiSan.TrangThaiId.Equals(input.TrangThaiId))
                .WhereIf(input.StartDate != null, item => item.KiemKeTaiSan.ThoiGianBatDauDuKien.Value.Date >= input.StartDate.Value.Date)
                .WhereIf(input.EndDate != null, item => item.KiemKeTaiSan.ThoiGianKetThucDuKien.Value.Date <= input.EndDate.Value.Date);
            return query;
        }

        private IQueryable<KetQuaKiemKeForViewDto> GetKetQuaKiemKeQuery(KetQuaKiemKeGetAllInputDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var ketQuaKiemKe = this.ketQuaKiemKeRepository.GetAll().Where(e => e.KiemKeTaiSanId.Equals(input.Id));
            var taiSanKiemKe = this.taiSanRepository.GetAll();
            var loaiTaiSans = this.loaiTaiSanRepository.GetAll();
            var viTris = this.viTriRepository.GetAll();
            IQueryable<KetQuaKiemKeForViewDto> query = null;
            switch (input.Status)
            {
                case 0:
                    query = from kiemKe in ketQuaKiemKe.Where(e => e.KetQua.Equals(1))
                            from taiSan in taiSanKiemKe.Where(e => e.Id.Equals(kiemKe.TaiSanId))
                            from loaiTaiSan in loaiTaiSans.Where(e => e.Id == taiSan.LoaiTaiSanId)
                            from viTri in viTris.Where(e => e.Id == kiemKe.ViTriDiaLyId)
                            select new KetQuaKiemKeForViewDto
                            {
                                Id = (int)kiemKe.Id,
                                MaTaiSan = taiSan.EPCCode,
                                TenTaiSan = taiSan.TenTaiSan,
                                LoaiTaiSan = loaiTaiSan.Ten,
                                TaiSanId = (int)kiemKe.TaiSanId,
                                ViTri = viTri.TenViTri,
                                TrangThai = GlobalModel.TrangThaiTaiSanHienThiSorted.ContainsKey((int)kiemKe.TrangThaiTaiSanId) ? GlobalModel.TrangThaiTaiSanHienThiSorted[(int)kiemKe.TrangThaiTaiSanId] : string.Empty,
                                TrangThaiId = (int)kiemKe.TrangThaiTaiSanId,
                            };
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Keyword), e => e.TenTaiSan.Contains(input.Keyword) || e.MaTaiSan.Contains(input.Keyword) || e.LoaiTaiSan.Contains(input.Keyword) || e.ViTri.Contains(input.Keyword) || e.TrangThaiId.Equals(GlobalModel.trangThaiLists.ContainsKey(input.Keyword) ? (int)GlobalModel.trangThaiLists[input.Keyword] : 99));
                    break;
                case 1:
                    query = from kiemKe in ketQuaKiemKe.Where(e => e.KetQua.Equals(0))
                            from taiSan in taiSanKiemKe.Where(e => e.Id.Equals(kiemKe.TaiSanId))
                            from loaiTaiSan in loaiTaiSans.Where(e => e.Id == taiSan.LoaiTaiSanId)
                            from viTri in viTris.Where(e => e.Id == kiemKe.ViTriDiaLyId)
                            select new KetQuaKiemKeForViewDto
                            {
                                Id = (int)kiemKe.Id,
                                MaTaiSan = taiSan.EPCCode,
                                TenTaiSan = taiSan.TenTaiSan,
                                LoaiTaiSan = loaiTaiSan.Ten,
                                TaiSanId = (int)kiemKe.TaiSanId,
                                ViTri = viTri.TenViTri,
                                TrangThai = GlobalModel.TrangThaiTaiSanHienThiSorted.ContainsKey((int)kiemKe.TrangThaiTaiSanId) ? GlobalModel.TrangThaiTaiSanHienThiSorted[(int)kiemKe.TrangThaiTaiSanId] : string.Empty,
                                TrangThaiId = (int)kiemKe.TrangThaiTaiSanId,
                            };
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Keyword), e => e.TenTaiSan.Contains(input.Keyword) || e.MaTaiSan.Contains(input.Keyword) || e.LoaiTaiSan.Contains(input.Keyword) || e.ViTri.Contains(input.Keyword) || e.TrangThaiId.Equals(GlobalModel.trangThaiLists.ContainsKey(input.Keyword) ? (int)GlobalModel.trangThaiLists[input.Keyword] : 99));
                    break;
                case 2:
                    query = from kiemKe in ketQuaKiemKe.Where(e => e.KetQua.Equals(2) || e.KetQua.Equals(3))
                            from viTri in viTris.Where(e => e.Id == kiemKe.ViTriDiaLyId)
                            select new KetQuaKiemKeForViewDto
                            {
                                Id = (int)kiemKe.Id,
                                MaTaiSan = kiemKe.Code,
                                TaiSanId = (int)kiemKe.TaiSanId,
                                ViTri = viTri.TenViTri,
                                TinhTrang = GlobalModel.KetQuaKiemKeTaiSanSorted.ContainsKey((int)kiemKe.KetQua) ? GlobalModel.KetQuaKiemKeTaiSanSorted[(int)kiemKe.KetQua] : string.Empty,
                                TinhTrangId = (int)kiemKe.KetQua,
                            };
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Keyword), e => e.MaTaiSan.Contains(input.Keyword) || e.ViTri.Contains(input.Keyword) || e.TinhTrangId.Equals(GlobalModel.tinhTrangLists.ContainsKey(input.Keyword) ? (int)GlobalModel.tinhTrangLists[input.Keyword] : 99));
                    break;
                default:
                    break;
            }

            return query;
        }
    }
}
