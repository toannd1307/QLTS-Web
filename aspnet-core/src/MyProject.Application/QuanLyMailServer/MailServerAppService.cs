namespace MyProject.QuanLyMailServer
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using Abp.Application.Services.Dto;
    using Abp.Authorization.Users;
    using Abp.Domain.Entities;
    using Abp.Domain.Repositories;
    using Abp.Linq.Extensions;
    using Abp.UI;
    using DbEntities;
    using Microsoft.EntityFrameworkCore;
    using MyProject.Authorization.Roles;
    using MyProject.Authorization.Users;
    using MyProject.Global;
    using MyProject.Global.Dtos;
    using MyProject.QuanLyKiemKeTaiSan.Dtos;
    using MyProject.QuanLyMailServer.Dtos;
    using MyProject.QuanLyPhieuDuTruMuaSam.Dtos;
    using MyProject.QuanLyTaiSan.QuanLyTaiSanChuaSuDung.Dtos;

    public class MailServerAppService : MyProjectAppServiceBase, IMailServerAppService
    {
        private readonly IRepository<MailServer> mailServerRepository;
        private readonly IRepository<ToChuc> toChucRepository;
        private readonly IRepository<User, long> useRepository;
        private readonly IRepository<UserRole, long> roleRepository;
        private readonly IAppFolders appFolders;
        private readonly IRepository<KiemKeTaiSan, long> kiemKeTaiSanRepository;
        private readonly IRepository<KiemKe_DoiKiemKe, long> doiKiemKeTaiSanRepository;
        private readonly IRepository<KiemKe_KetQuaKiemKe, long> ketQuakiemKeTaiSanRepository;
        private readonly IRepository<TaiSan> taiSanRepository;
        private readonly IRepository<ViTriDiaLy> viTriRepository;
        private readonly IRepository<LoaiTaiSan> loaiTaiSanRepository;
        private readonly IRepository<Role> roleAllRepository;
        private readonly IRepository<PhieuDuTruMuaSam, long> phieuDuTruMuaSamRepository;
        private readonly IRepository<PhieuDuTruMuaSamChiTiet, long> phieuDuTruMuaSamChiTietRepository;
        private string emailFlag = string.Empty;
        private string mailTestDFT = string.Empty;
        private Email email;

        public MailServerAppService(
            IRepository<MailServer> mailServerRepository,
            IRepository<ToChuc> toChucRepository,
            IRepository<User, long> useRepository,
            IRepository<UserRole, long> roleRepository,
            IAppFolders appFolders,
            IRepository<KiemKeTaiSan, long> kiemKeTaiSanRepository,
            IRepository<KiemKe_DoiKiemKe, long> doiKiemKeTaiSanRepository,
            IRepository<KiemKe_KetQuaKiemKe, long> ketQuakiemKeTaiSanRepository,
            IRepository<TaiSan> taiSanRepository,
            IRepository<ViTriDiaLy> viTriRepository,
            IRepository<LoaiTaiSan> loaiTaiSanRepository,
            IRepository<Role> roleAllRepository,
            IRepository<PhieuDuTruMuaSam, long> phieuDuTruMuaSamRepository,
            IRepository<PhieuDuTruMuaSamChiTiet, long> phieuDuTruMuaSamChiTietRepository
            )
        {
            this.mailServerRepository = mailServerRepository;
            this.appFolders = appFolders;
            this.toChucRepository = toChucRepository;
            this.useRepository = useRepository;
            this.roleRepository = roleRepository;
            this.kiemKeTaiSanRepository = kiemKeTaiSanRepository;
            this.doiKiemKeTaiSanRepository = doiKiemKeTaiSanRepository;
            this.ketQuakiemKeTaiSanRepository = ketQuakiemKeTaiSanRepository;
            this.taiSanRepository = taiSanRepository;
            this.viTriRepository = viTriRepository;
            this.loaiTaiSanRepository = loaiTaiSanRepository;
            this.roleAllRepository = roleAllRepository;
            this.phieuDuTruMuaSamRepository = phieuDuTruMuaSamRepository;
            this.phieuDuTruMuaSamChiTietRepository = phieuDuTruMuaSamChiTietRepository;

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["emailFlag"].ToString()))
            {
                this.emailFlag = ConfigurationManager.AppSettings["emailFlag"].ToString();
            }

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["mailTestDFT"].ToString()))
            {
                this.mailTestDFT = ConfigurationManager.AppSettings["mailTestDFT"].ToString();
            }
        }

        public Task GetBaoCao()
        {
            throw new NotImplementedException();
        }

        public List<int> GetRoleLanhDao()
        {
            var query = this.roleAllRepository.GetAll().Where(w => w.DisplayName.ToLower().Equals("trưởng phòng") || w.DisplayName.ToLower().Equals("phó phòng")).Select(e => e.Id).ToList();
            return query;
        }

        public async Task<MailServer> GetForEditAsync()
        {
            var entity = await this.mailServerRepository.GetAll().FirstOrDefaultAsync();
            return await Task.FromResult(entity);
        }

        public async Task<int> UpdateMailServer(MailServerDto input)
        {
            #region Check null
            if (input == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion

            input.Email = GlobalFunction.RegexFormat(input.Email);
            input.Host = GlobalFunction.RegexFormat(input.Host);
            input.Password = GlobalFunction.RegexFormat(input.Password);
            input.Port = GlobalFunction.RegexFormat(input.Port);
            var mailServers = this.mailServerRepository.GetAll().FirstOrDefault();
            if (mailServers == null)
            {
                var create = this.ObjectMapper.Map(input, mailServers);
                await this.mailServerRepository.InsertAsync(create);
            }
            else
            {
                mailServers.Host = input.Host;
                mailServers.Port = input.Port;
                mailServers.Email = input.Email;
                mailServers.Password = input.Password;
                mailServers.CapPhat = input.CapPhat;
                mailServers.ThuHoi = input.ThuHoi;
                mailServers.DieuChuyen = input.DieuChuyen;
                mailServers.BaoMat = input.BaoMat;
                mailServers.BaoHong = input.BaoHong;
                mailServers.ThanhLy = input.ThanhLy;
                mailServers.SuaChuaBaoDuong = input.SuaChuaBaoDuong;
                mailServers.BatDauKiemKe = input.BatDauKiemKe;
                mailServers.KetThucKiemKe = input.KetThucKiemKe;
                mailServers.HoanThanhPhieuDuTruMuaSam = input.HoanThanhPhieuDuTruMuaSam;
                mailServers.HuyBoPhieuDuTruMuaSam = input.HuyBoPhieuDuTruMuaSam;
                await this.mailServerRepository.UpdateAsync(mailServers);
                return 1;
            }

            return 0;
        }

#pragma warning disable SA1118 // Parameter should not span multiple lines

        public void SendMailCapPhat(List<TaiSanChuaSuDungForViewDto> list, int donViId)
        {
            #region Check null
            if (list == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var roleLanhDaoId = this.GetRoleLanhDao();
            if (list.Count > 0)
            {
                var donViDuocCapPhat = this.toChucRepository.FirstOrDefault(w => w.Id == donViId).TenToChuc;
                var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == donViId && w.IsActive == true)
                                   from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                                   select user.EmailAddress).ToList();

                string body = string.Empty;
                var soLuong = list.Count;
                var template = "MailCapPhat.html";
                var subject = "Thông báo Cấp phát tài sản";
                using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
                {
                    body = reader.ReadToEnd();
                }

                body = body.Replace("{phongBan}", donViDuocCapPhat);

                body = body.Replace("{soLuong}", soLuong.ToString());

                var lastRecord = list.Last();
                var stt = 0;
                foreach (var item in list)
                {
                    stt++;
                    if (item != lastRecord)
                    {
                        body = body.Replace("{data}", "<tr>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                          "</tr>" +
                            "{data}");
                    }
                    else
                    {
                        body = body.Replace("{data}", "<tr>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                          "</tr>");
                    }

                    body = body.Replace("{stt}", stt.ToString());
                    body = body.Replace("{tenTaiSan}", item.TenTaiSan);
                    body = body.Replace("{loaiTaiSan}", item.LoaiTaiSan);
                    body = body.Replace("{serialNumber}", item.SerialNumber);
                    body = body.Replace("{nhaCungCap}", item.NhaCungCap);
                    body = body.Replace("{ngayMua}", item.NgayMua.HasValue ? item.NgayMua.Value.ToShortDateString() : string.Empty);
                    body = body.Replace("{nguyenGia}", item.NguyenGia != null ? string.Format("{0:0,0}", item.NguyenGia) : string.Empty);
                }

                this.email = new Email(this.mailServerRepository);
                if (this.emailFlag.Equals("That"))
                {
                    mailLanhDao.ForEach(mailNhan =>
                    {
                        this.email.SendMail(1, mailNhan, subject, body); // 1-chức năng cấp phát
                    });
                }
                else if (this.emailFlag.Equals("TestDFT"))
                {
                    // Check mail test dft co la role lanh dao hay ko?
                    if (mailLanhDao.Contains(this.mailTestDFT))
                    {
                        this.email.SendMailTest(1, this.mailTestDFT, subject, body);
                    }
                }
            }
        }

        public void SendMailThuHoi(List<TaiSanChuaSuDungForViewDto> list, int? donViId)
        {
            #region Check null
            if (list == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var roleLanhDaoId = this.GetRoleLanhDao();
            if (list.Count > 0)
            {
                var a = list.GroupBy(e => e.ToChucId).Select(e => new
                {
                    toChucId = e.Key,
                    listTaiSan = e.ToList(),
                }).ToList();
                a.ForEach(tungPB =>
                {
                    var donViDuocCapPhat = this.toChucRepository.FirstOrDefault(w => w.Id == tungPB.toChucId).TenToChuc;
                    var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == tungPB.toChucId && w.IsActive == true)
                                       from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                                       select user.EmailAddress).ToList();

                    string body = string.Empty;
                    var soLuong = tungPB.listTaiSan.Count;
                    var template = "MailThuHoi.html";
                    var subject = "Thông báo Thu hồi tài sản";
                    using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("{phongBan}", donViDuocCapPhat);

                    body = body.Replace("{soLuong}", soLuong.ToString());

                    var lastRecord = tungPB.listTaiSan.Last();
                    var stt = 0;
                    foreach (var item in tungPB.listTaiSan)
                    {
                        stt++;
                        if (item != lastRecord)
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{donViQuanLy}</td>" +
                              "</tr>" +
                                "{data}");
                        }
                        else
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{donViQuanLy}</td>" +
                              "</tr>");
                        }

                        body = body.Replace("{stt}", stt.ToString());
                        body = body.Replace("{tenTaiSan}", item.TenTaiSan);
                        body = body.Replace("{loaiTaiSan}", item.LoaiTaiSan);
                        body = body.Replace("{serialNumber}", item.SerialNumber);
                        body = body.Replace("{nhaCungCap}", item.NhaCungCap);
                        body = body.Replace("{ngayMua}", item.NgayMua.HasValue ? item.NgayMua.Value.ToShortDateString() : string.Empty);
                        body = body.Replace("{nguyenGia}", item.NguyenGia != null ? string.Format("{0:0,0}", item.NguyenGia) : string.Empty);
                        body = body.Replace("{donViQuanLy}", item.PhongBanQuanLy);
                    }

                    this.email = new Email(this.mailServerRepository);
                    if (this.emailFlag.Equals("That"))
                    {
                        mailLanhDao.ForEach(mailNhan =>
                        {
                            this.email.SendMail(2, mailNhan, subject, body); // 1-chức năng thu hồi
                        });
                    }
                    else if (this.emailFlag.Equals("TestDFT"))
                    {
                        // Check mail test dft co la role lanh dao hay ko?
                        if (mailLanhDao.Contains(this.mailTestDFT))
                        {
                            this.email.SendMailTest(2, this.mailTestDFT, subject, body);
                        }
                    }
                });
            }
        }

        public void SendMailDieuChuyen(List<TaiSanChuaSuDungForViewDto> list, int? donViId)
        {
            #region Check null
            if (list == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var donViDen = this.toChucRepository.FirstOrDefault(w => w.Id == donViId).TenToChuc;
            var roleLanhDaoId = this.GetRoleLanhDao();

            if (list.Count > 0)
            {
                var a = list.GroupBy(e => e.ToChucId).Select(e => new
                {
                    toChucId = e.Key,
                    listTaiSan = e.ToList(),
                }).ToList();
                a.ForEach(tungPB =>
                {
                    var donViDi = this.toChucRepository.FirstOrDefault(w => w.Id == tungPB.toChucId).TenToChuc;
                    var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == donViId && w.IsActive == true)
                                       from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                                       select user.EmailAddress).ToList();

                    string body = string.Empty;
                    var soLuong = tungPB.listTaiSan.Count;
                    var template = "MailDieuChuyen.html";
                    var subject = "Thông báo Điều chuyển tài sản";
                    using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("{phongBanDi}", donViDi);

                    body = body.Replace("{phongBanDen}", donViDen);

                    body = body.Replace("{soLuong}", soLuong.ToString());

                    var lastRecord = tungPB.listTaiSan.Last();
                    var stt = 0;
                    foreach (var item in tungPB.listTaiSan)
                    {
                        stt++;
                        if (item != lastRecord)
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                              "</tr>" +
                                "{data}");
                        }
                        else
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                              "</tr>");
                        }

                        body = body.Replace("{stt}", stt.ToString());
                        body = body.Replace("{tenTaiSan}", item.TenTaiSan);
                        body = body.Replace("{loaiTaiSan}", item.LoaiTaiSan);
                        body = body.Replace("{serialNumber}", item.SerialNumber);
                        body = body.Replace("{nhaCungCap}", item.NhaCungCap);
                        body = body.Replace("{ngayMua}", item.NgayMua.HasValue ? item.NgayMua.Value.ToShortDateString() : string.Empty);
                        body = body.Replace("{nguyenGia}", item.NguyenGia != null ? string.Format("{0:0,0}", item.NguyenGia) : string.Empty);
                    }

                    this.email = new Email(this.mailServerRepository);
                    if (this.emailFlag.Equals("That"))
                    {
                        mailLanhDao.ForEach(mailNhan =>
                        {
                            this.email.SendMail(3, mailNhan, subject, body); // 1-chức năng dieu chuyen
                        });
                    }
                    else if (this.emailFlag.Equals("TestDFT"))
                    {
                        // Check mail test dft co la role lanh dao hay ko?
                        if (mailLanhDao.Contains(this.mailTestDFT))
                        {
                            this.email.SendMailTest(3, this.mailTestDFT, subject, body);
                        }
                    }
                });
            }
        }

        public void SendMailBaoHong(List<ViewTaiSanHong> list, int? donViBaoId)
        {
            #region Check null
            if (list == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var roleLanhDaoId = this.GetRoleLanhDao();
            if (list.Count > 0)
            {
                var a = list.GroupBy(e => e.PhongBanQuanLyId).Select(e => new
                {
                    toChucId = e.Key,
                    listTaiSan = e.ToList(),
                }).ToList();
                a.ForEach(tungPB =>
                {
                    var donViQuanLy = this.toChucRepository.FirstOrDefault(w => w.Id == tungPB.toChucId).TenToChuc;
                    var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == tungPB.toChucId && w.IsActive == true)
                                       from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                                       select user.EmailAddress).ToList();

                    string body = string.Empty;
                    var soLuong = tungPB.listTaiSan.Count;
                    var template = "MailBaoHong.html";
                    var subject = "Thông báo tài sản hỏng";
                    using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("{phongBanBao}", donViQuanLy);

                    body = body.Replace("{soLuong}", soLuong.ToString());

                    var lastRecord = tungPB.listTaiSan.Last();
                    var stt = 0;
                    foreach (var item in tungPB.listTaiSan)
                    {
                        stt++;
                        if (item != lastRecord)
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                              "</tr>" +
                                "{data}");
                        }
                        else
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                              "</tr>");
                        }

                        body = body.Replace("{stt}", stt.ToString());
                        body = body.Replace("{tenTaiSan}", item.TenTaiSan);
                        body = body.Replace("{loaiTaiSan}", item.LoaiTaiSan);
                        body = body.Replace("{serialNumber}", item.SerialNumber);
                        body = body.Replace("{nhaCungCap}", item.NhaCungCap);
                        body = body.Replace("{ngayMua}", item.NgayMua.HasValue ? item.NgayMua.Value.ToShortDateString() : string.Empty);
                        body = body.Replace("{nguyenGia}", item.NguyenGia != null ? string.Format("{0:0,0}", item.NguyenGia) : string.Empty);
                    }

                    this.email = new Email(this.mailServerRepository);
                    if (this.emailFlag.Equals("That"))
                    {
                        mailLanhDao.ForEach(mailNhan =>
                        {
                            this.email.SendMail(5, mailNhan, subject, body); // 1-chức năng bao hong
                        });
                    }
                    else if (this.emailFlag.Equals("TestDFT"))
                    {
                        // Check mail test dft co la role lanh dao hay ko?
                        if (mailLanhDao.Contains(this.mailTestDFT))
                        {
                            this.email.SendMailTest(5, this.mailTestDFT, subject, body);
                        }
                    }
                });
            }
        }

        public void SendMailKetQuaBaoDuong(ViewTaiSanSuaChuaBaoDuong record)
        {
            #region Check null
            if (record == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var roleLanhDaoId = this.GetRoleLanhDao();

            var donViDuocSua = this.toChucRepository.FirstOrDefault(w => w.Id == record.PhongBanQuanLyId).TenToChuc;
            var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == record.PhongBanQuanLyId && w.IsActive == true)
                               from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                               select user.EmailAddress).ToList();

            string body = string.Empty;
            var template = "MailSuaChuaBaoDuong.html";
            var subject = "Thông báo tình trạng sửa chữa tài sản";
            using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
            {
                body = reader.ReadToEnd();
            }

            var ketQua = record.TrangThai == (int)GlobalConst.TrangThaiSuaChuaBaoDuongConst.ThanhCong ? "Thành công" : "Không thành công";
            body = body.Replace("{phongBan}", donViDuocSua);

            body = body.Replace("{soLuong}", 1.ToString());

            body = body.Replace("{data}", "<tr>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{maTaiSan}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{thoiGianBatDau}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{liDoSuaChua}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{diaChiSuaChua}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ketQua}</td>" +
              "</tr>");

            body = body.Replace("{maTaiSan}", record.EPCCode ?? string.Empty);
            body = body.Replace("{tenTaiSan}", record.TenTaiSan ?? string.Empty);
            body = body.Replace("{loaiTaiSan}", record.LoaiTaiSan ?? string.Empty);
            body = body.Replace("{nhaCungCap}", record.NhaCungCap ?? string.Empty);
            body = body.Replace("{nguyenGia}", record.NguyenGia != null ? string.Format("{0:0,0}", record.NguyenGia) : string.Empty);
            body = body.Replace("{thoiGianBatDau}", record.ThoiGianBatDau != null ? record.ThoiGianBatDau.Value.ToShortDateString() : string.Empty);
            body = body.Replace("{liDoSuaChua}", record.LiDoSuaChuaBaoDuong ?? string.Empty);
            body = body.Replace("{diaChiSuaChua}", record.DiaChiSuaChuaBaoDuong ?? string.Empty);
            body = body.Replace("{ketQua}", ketQua);

            this.email = new Email(this.mailServerRepository);
            if (this.emailFlag.Equals("That"))
            {
                mailLanhDao.ForEach(mailNhan =>
                {
                    this.email.SendMail(7, mailNhan, subject, body); // -chức năng ket qua bao duong
                });
            }
            else if (this.emailFlag.Equals("TestDFT"))
            {
                // Check mail test dft co la role lanh dao hay ko?
                if (mailLanhDao.Contains(this.mailTestDFT))
                {
                    this.email.SendMailTest(7, this.mailTestDFT, subject, body);
                }
            }
        }

        public void SendMailBaoMat(List<ViewTaiSanHong> list, int? donViBaoId)
        {
            #region Check null
            if (list == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var roleLanhDaoId = this.GetRoleLanhDao();
            if (list.Count > 0)
            {
                var a = list.GroupBy(e => e.PhongBanQuanLyId).Select(e => new
                {
                    toChucId = e.Key,
                    listTaiSan = e.ToList(),
                }).ToList();
                a.ForEach(tungPB =>
                {
                    var donViDuocCapPhat = this.toChucRepository.FirstOrDefault(w => w.Id == tungPB.toChucId).TenToChuc;
                    var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == tungPB.toChucId && w.IsActive == true)
                                       from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                                       select user.EmailAddress).ToList();

                    string body = string.Empty;
                    var soLuong = tungPB.listTaiSan.Count;
                    var template = "MailBaoMat.html";
                    var subject = "Thông báo tài sản mất";
                    using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("{phongBan}", donViDuocCapPhat);

                    body = body.Replace("{soLuong}", soLuong.ToString());

                    var lastRecord = tungPB.listTaiSan.Last();
                    var stt = 0;
                    foreach (var item in tungPB.listTaiSan)
                    {
                        stt++;
                        if (item != lastRecord)
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                              "</tr>" +
                                "{data}");
                        }
                        else
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                              "</tr>");
                        }

                        body = body.Replace("{stt}", stt.ToString());
                        body = body.Replace("{tenTaiSan}", item.TenTaiSan);
                        body = body.Replace("{loaiTaiSan}", item.LoaiTaiSan);
                        body = body.Replace("{serialNumber}", item.SerialNumber);
                        body = body.Replace("{nhaCungCap}", item.NhaCungCap);
                        body = body.Replace("{ngayMua}", item.NgayMua.HasValue ? item.NgayMua.Value.ToShortDateString() : string.Empty);
                        body = body.Replace("{nguyenGia}", item.NguyenGia != null ? string.Format("{0:0,0}", item.NguyenGia) : string.Empty);
                    }

                    this.email = new Email(this.mailServerRepository);
                    if (this.emailFlag.Equals("That"))
                    {
                        mailLanhDao.ForEach(mailNhan =>
                        {
                            this.email.SendMail(4, mailNhan, subject, body); // -chức năng bao mat
                        });
                    }
                    else if (this.emailFlag.Equals("TestDFT"))
                    {
                        // Check mail test dft co la role lanh dao hay ko?
                        if (mailLanhDao.Contains(this.mailTestDFT))
                        {
                            this.email.SendMailTest(4, this.mailTestDFT, subject, body);
                        }
                    }
                });
            }
        }

        public void SendMailHoanThanhPhieu(DuTruMuaSamOutPut record)
        {
            #region Check null
            if (record == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var roleLanhDaoId = this.GetRoleLanhDao();
            var donViDeXuat = this.phieuDuTruMuaSamRepository.GetAll().Where(w => w.Id == record.Id).FirstOrDefault();
            var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == donViDeXuat.ToChucId && w.IsActive == true)
                               from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                               select user.EmailAddress).ToList();

            string body = string.Empty;
            var template = "MailHoanThanhPhieuDTMS.html";
            var subject = "Thông báo Hoàn thành phiếu dự trù mua sắm tài sản";
            using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{tenPhieu}", record.TenPhieu ?? string.Empty);

            body = body.Replace("{data}", "<tr>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{donVi}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{maPhieu}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{tenPhieu}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{soLuongDeXuat}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{chiPhiDeXuat}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{nguoiLap}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayLap}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayCapNhat}</td>" +
              "</tr>");

            body = body.Replace("{donVi}", record.TenPhongBan ?? string.Empty);
            body = body.Replace("{maPhieu}", record.MaPhieu ?? string.Empty);
            body = body.Replace("{tenPhieu}", record.TenPhieu ?? string.Empty);
            body = body.Replace("{soLuongDeXuat}", record.SoLuongDeXuat.ToString() ?? string.Empty);
            body = body.Replace("{chiPhiDeXuat}", record.ChiPhiDeXuat != null ? string.Format("{0:0,0}", record.ChiPhiDeXuat) : string.Empty);
            body = body.Replace("{nguoiLap}", record.NguoiLap ?? string.Empty);
            body = body.Replace("{ngayLap}", record.NgayLap ?? string.Empty);
            body = body.Replace("{ngayCapNhat}", record.NgayCapNhat ?? string.Empty);

            this.email = new Email(this.mailServerRepository);
            if (this.emailFlag.Equals("That"))
            {
                mailLanhDao.ForEach(mailNhan =>
                {
                    this.email.SendMail(10, mailNhan, subject, body); // 1-chức năng cấp phát
                });
            }
            else if (this.emailFlag.Equals("TestDFT"))
            {
                // Check mail test dft co la role lanh dao hay ko?
                if (mailLanhDao.Contains(this.mailTestDFT))
                {
                    this.email.SendMailTest(10, this.mailTestDFT, subject, body);
                }
            }
        }

        public void SendMailHuyPhieu(DuTruMuaSamOutPut record)
        {
            #region Check null
            if (record == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var roleLanhDaoId = this.GetRoleLanhDao();
            var donViDeXuat = this.phieuDuTruMuaSamRepository.GetAll().Where(w => w.Id == record.Id).FirstOrDefault();
            var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == donViDeXuat.ToChucId && w.IsActive == true)
                               from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                               select user.EmailAddress).ToList();

            string body = string.Empty;
            var template = "MailHuyPhieuDTMS.html";
            var subject = "Thông báo Huỷ bỏ phiếu dự trù mua sắm tài sản";
            using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{tenPhieu}", record.TenPhieu ?? string.Empty);

            body = body.Replace("{data}", "<tr>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{donVi}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{maPhieu}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{tenPhieu}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{soLuongDeXuat}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{chiPhiDeXuat}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{nguoiLap}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayLap}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayCapNhat}</td>" +
              "</tr>");

            body = body.Replace("{donVi}", record.TenPhongBan ?? string.Empty);
            body = body.Replace("{maPhieu}", record.MaPhieu ?? string.Empty);
            body = body.Replace("{tenPhieu}", record.TenPhieu ?? string.Empty);
            body = body.Replace("{soLuongDeXuat}", record.SoLuongDeXuat.ToString() ?? string.Empty);
            body = body.Replace("{chiPhiDeXuat}", record.ChiPhiDeXuat != null ? string.Format("{0:0,0}", record.ChiPhiDeXuat) : string.Empty);
            body = body.Replace("{nguoiLap}", record.NguoiLap ?? string.Empty);
            body = body.Replace("{ngayLap}", record.NgayLap ?? string.Empty);
            body = body.Replace("{ngayCapNhat}", record.NgayCapNhat ?? string.Empty);

            this.email = new Email(this.mailServerRepository);
            if (this.emailFlag.Equals("That"))
            {
                mailLanhDao.ForEach(mailNhan =>
                {
                    this.email.SendMail(11, mailNhan, subject, body); // 1-chức năng cấp phát
                });
            }
            else if (this.emailFlag.Equals("TestDFT"))
            {
                // Check mail test dft co la role lanh dao hay ko?
                if (mailLanhDao.Contains(this.mailTestDFT))
                {
                    this.email.SendMailTest(11, this.mailTestDFT, subject, body);
                }
            }
        }

        public void SendMailBatDauKiemKe(long idKiemKe)
        {
            var roleLanhDaoId = this.GetRoleLanhDao();
            var doiKiemKe = (from doiKK in this.doiKiemKeTaiSanRepository.GetAll().Where(w => w.KiemKeTaiSanId == idKiemKe)
                             from user in this.useRepository.GetAll().Where(w => w.Id == doiKK.NguoiKiemKeId)
                             select user.Name).ToList();
            var listTenDoi = string.Join(",", doiKiemKe);
            var thongTinDotKiemKe = this.kiemKeTaiSanRepository.FirstOrDefault(w => w.Id == idKiemKe);
            var phongBanDuocKiemKe = this.toChucRepository.FirstOrDefault(w => w.Id == thongTinDotKiemKe.BoPhanDuocKiemKeId);

            var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == phongBanDuocKiemKe.Id && w.IsActive == true)
                               from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                               select user.EmailAddress).ToList();

            string body = string.Empty;
            var template = "MailBatDauKiemKe.html";
            var subject = "Thông báo bắt đầu đợt kiểm kê tài sản";
            using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{tenDotKiemKe}", thongTinDotKiemKe.TenKiemKe);

            body = body.Replace("{data}", "<tr>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{tenDotKiemKe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{maKiemKe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{thoiGianBatDau}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{thoiGianKetThuc}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{thoiGianBatDauThucTe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{thoiGianKetThucThucTe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{boPhanDuocKiemKe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{nguoiKiemKe}</td>" +
              "</tr>");

            body = body.Replace("{tenDotKiemKe}", thongTinDotKiemKe.TenKiemKe ?? string.Empty);
            body = body.Replace("{maKiemKe}", thongTinDotKiemKe.MaKiemKe ?? string.Empty);
            body = body.Replace("{thoiGianBatDau}", thongTinDotKiemKe.ThoiGianBatDauDuKien.HasValue ? thongTinDotKiemKe.ThoiGianBatDauDuKien.Value.ToShortDateString() : string.Empty);
            body = body.Replace("{thoiGianKetThuc}", thongTinDotKiemKe.ThoiGianKetThucDuKien.HasValue ? thongTinDotKiemKe.ThoiGianKetThucDuKien.Value.ToShortDateString() : string.Empty);
            body = body.Replace("{thoiGianBatDauThucTe}", thongTinDotKiemKe.ThoiGianBatDauThucTe.HasValue ? thongTinDotKiemKe.ThoiGianBatDauThucTe.Value.ToShortDateString() : string.Empty);
            body = body.Replace("{thoiGianKetThucThucTe}", thongTinDotKiemKe.ThoiGianKetThucThucTe.HasValue ? thongTinDotKiemKe.ThoiGianKetThucThucTe.Value.ToShortDateString() : string.Empty);
            body = body.Replace("{boPhanDuocKiemKe}", phongBanDuocKiemKe.TenToChuc ?? string.Empty);
            body = body.Replace("{nguoiKiemKe}", listTenDoi);

            this.email = new Email(this.mailServerRepository);
            if (this.emailFlag.Equals("That"))
            {
                mailLanhDao.ForEach(mailNhan =>
                {
                    this.email.SendMail(8, mailNhan, subject, body); // 1-chức năng cấp phát
                });
            }
            else if (this.emailFlag.Equals("TestDFT"))
            {
                // Check mail test dft co la role lanh dao hay ko?
                if (mailLanhDao.Contains(this.mailTestDFT))
                {
                    this.email.SendMailTest(8, this.mailTestDFT, subject, body);
                }
            }
        }

        public void SendMailKetThucKiemKe(long idKiemKe)
        {
            // Lấy thông tin kiểm kê
            var roleLanhDaoId = this.GetRoleLanhDao();
            var doiKiemKe = (from doiKK in this.doiKiemKeTaiSanRepository.GetAll().Where(w => w.KiemKeTaiSanId == idKiemKe)
                             from user in this.useRepository.GetAll().Where(w => w.Id == doiKK.NguoiKiemKeId)
                             select user.Name).ToList();
            var listTenDoi = string.Join(", ", doiKiemKe);
            var thongTinDotKiemKe = this.kiemKeTaiSanRepository.FirstOrDefault(w => w.Id == idKiemKe);
            var phongBanDuocKiemKe = this.toChucRepository.FirstOrDefault(w => w.Id == thongTinDotKiemKe.BoPhanDuocKiemKeId);

            var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == phongBanDuocKiemKe.Id && w.IsActive == true)
                               from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                               select user.EmailAddress).ToList();

            // Lấy kết quả kiểm kê
            var ketQuaKiemKe = this.ketQuakiemKeTaiSanRepository.GetAll().Where(e => e.KiemKeTaiSanId.Equals(idKiemKe));
            var taiSanKiemKe = this.taiSanRepository.GetAll();
            var loaiTaiSans = this.loaiTaiSanRepository.GetAll();
            var viTris = this.viTriRepository.GetAll();

            var taiSanTimThay = (from kiemKe in ketQuaKiemKe.Where(e => e.KetQua.Equals(1))
                                 from taiSan in taiSanKiemKe.Where(e => e.Id.Equals(kiemKe.TaiSanId))
                                 from loaiTaiSan in loaiTaiSans.Where(e => e.Id == taiSan.LoaiTaiSanId)
                                 from viTri in viTris.Where(e => e.Id == kiemKe.ViTriDiaLyId)
                                 select new KetQuaKiemKeForViewDto
                                 {
                                     Id = (int)kiemKe.Id,
                                     MaTaiSan = taiSan.SerialNumber,
                                     TenTaiSan = taiSan.TenTaiSan,
                                     LoaiTaiSan = loaiTaiSan.Ten,
                                     TaiSanId = (int)kiemKe.TaiSanId,
                                     ViTri = viTri.TenViTri,
                                     TrangThai = GlobalModel.TrangThaiTaiSanHienThiSorted.ContainsKey((int)kiemKe.TrangThaiTaiSanId) ? GlobalModel.TrangThaiTaiSanHienThiSorted[(int)kiemKe.TrangThaiTaiSanId] : string.Empty,
                                 }).ToList();

            var taiSanKhongTimThay = (from kiemKe in ketQuaKiemKe.Where(e => e.KetQua.Equals(0))
                                      from taiSan in taiSanKiemKe.Where(e => e.Id.Equals(kiemKe.TaiSanId))
                                      from loaiTaiSan in loaiTaiSans.Where(e => e.Id == taiSan.LoaiTaiSanId)
                                      from viTri in viTris.Where(e => e.Id == kiemKe.ViTriDiaLyId)
                                      select new KetQuaKiemKeForViewDto
                                      {
                                          Id = (int)kiemKe.Id,
                                          MaTaiSan = taiSan.SerialNumber,
                                          TenTaiSan = taiSan.TenTaiSan,
                                          LoaiTaiSan = loaiTaiSan.Ten,
                                          TaiSanId = (int)kiemKe.TaiSanId,
                                          ViTri = viTri.TenViTri,
                                          TrangThai = GlobalModel.TrangThaiTaiSanHienThiSorted.ContainsKey((int)kiemKe.TrangThaiTaiSanId) ? GlobalModel.TrangThaiTaiSanHienThiSorted[(int)kiemKe.TrangThaiTaiSanId] : string.Empty,
                                      }).ToList();

            var taiSanLa = (from kiemKe in ketQuaKiemKe.Where(e => e.KetQua.Equals(2) || e.KetQua.Equals(3))
                            from viTri in viTris.Where(e => e.Id == kiemKe.ViTriDiaLyId)
                            select new KetQuaKiemKeForViewDto
                            {
                                Id = (int)kiemKe.Id,
                                MaTaiSan = kiemKe.Code,
                                TaiSanId = (int)kiemKe.TaiSanId,
                                ViTri = viTri.TenViTri,
                                TinhTrang = GlobalModel.KetQuaKiemKeTaiSanSorted.ContainsKey((int)kiemKe.KetQua) ? GlobalModel.KetQuaKiemKeTaiSanSorted[(int)kiemKe.KetQua] : string.Empty,
                            }).ToList();

            string body = string.Empty;
            var template = "MailKetThucKiem.html";
            var subject = "Thông báo kết thúc đợt kiểm kê tài sản";
            using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{tenDotKiemKe}", thongTinDotKiemKe.TenKiemKe);

            // Thông tin kiểm kê
            body = body.Replace("{data1}", "<tr>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{tenDotKiemKe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{maKiemKe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{thoiGianBatDau}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{thoiGianKetThuc}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{thoiGianBatDauThucTe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{thoiGianKetThucThucTe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{boPhanDuocKiemKe}</td>" +
                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{nguoiKiemKe}</td>" +
              "</tr>");

            body = body.Replace("{tenDotKiemKe}", thongTinDotKiemKe.TenKiemKe ?? string.Empty);
            body = body.Replace("{maKiemKe}", thongTinDotKiemKe.MaKiemKe ?? string.Empty);
            body = body.Replace("{thoiGianBatDau}", thongTinDotKiemKe.ThoiGianBatDauDuKien.HasValue ? thongTinDotKiemKe.ThoiGianBatDauDuKien.Value.ToShortDateString() : string.Empty);
            body = body.Replace("{thoiGianKetThuc}", thongTinDotKiemKe.ThoiGianKetThucDuKien.HasValue ? thongTinDotKiemKe.ThoiGianKetThucDuKien.Value.ToShortDateString() : string.Empty);
            body = body.Replace("{thoiGianBatDauThucTe}", thongTinDotKiemKe.ThoiGianBatDauThucTe.HasValue ? thongTinDotKiemKe.ThoiGianBatDauThucTe.Value.ToShortDateString() : string.Empty);
            body = body.Replace("{thoiGianKetThucThucTe}", thongTinDotKiemKe.ThoiGianKetThucThucTe.HasValue ? thongTinDotKiemKe.ThoiGianKetThucThucTe.Value.ToShortDateString() : string.Empty);
            body = body.Replace("{boPhanDuocKiemKe}", phongBanDuocKiemKe.TenToChuc ?? string.Empty);
            body = body.Replace("{nguoiKiemKe}", listTenDoi);

            // Thông tin tài sản tìm thấy
            var stt = 0;
            if (taiSanTimThay.Count > 0)
            {
                var lastRecord1 = taiSanTimThay.Last();
                taiSanTimThay.ForEach(timthay =>
                {
                    stt++;
                    if (timthay != lastRecord1)
                    {
                        body = body.Replace("{data2}", "<tr>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{maTaiSan1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{viTri1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{trangThaiTaiSan1}</td>" +
                            "</tr>" +
                            "{data2}");
                    }
                    else
                    {
                        body = body.Replace("{data2}", "<tr>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{maTaiSan1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{viTri1}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{trangThaiTaiSan1}</td>" +
                            "</tr>");
                    }

                    body = body.Replace("{stt1}", stt.ToString());
                    body = body.Replace("{maTaiSan1}", timthay.MaTaiSan ?? string.Empty);
                    body = body.Replace("{tenTaiSan1}", timthay.TenTaiSan ?? string.Empty);
                    body = body.Replace("{loaiTaiSan1}", timthay.LoaiTaiSan ?? string.Empty);
                    body = body.Replace("{viTri1}", timthay.ViTri ?? string.Empty);
                    body = body.Replace("{trangThaiTaiSan1}", timthay.TrangThai ?? string.Empty);
                });
            }
            else
            {
                body = body.Replace("{data2}", "Không có dữ liệu");
            }

            // Thông tin tài sản ko tìm thấy
            if (taiSanKhongTimThay.Count > 0)
            {
                var lastRecord2 = taiSanKhongTimThay.Last();
                taiSanKhongTimThay.ForEach(timKoThay =>
                {
                    stt++;
                    if (timKoThay != lastRecord2)
                    {
                        body = body.Replace("{data3}", "<tr>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{maTaiSan2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{viTri2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{trangThaiTaiSan2}</td>" +
                            "</tr>" +
                            "{data3}");
                    }
                    else
                    {
                        body = body.Replace("{data3}", "<tr>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{maTaiSan2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{viTri2}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{trangThaiTaiSan2}</td>" +
                            "</tr>");
                    }

                    body = body.Replace("{stt2}", stt.ToString());
                    body = body.Replace("{maTaiSan2}", timKoThay.MaTaiSan ?? string.Empty);
                    body = body.Replace("{tenTaiSan2}", timKoThay.TenTaiSan ?? string.Empty);
                    body = body.Replace("{loaiTaiSan2}", timKoThay.LoaiTaiSan ?? string.Empty);
                    body = body.Replace("{viTri2}", timKoThay.ViTri ?? string.Empty);
                    body = body.Replace("{trangThaiTaiSan2}", timKoThay.TrangThai ?? string.Empty);
                });
            }
            else
            {
                body = body.Replace("{data3}", "Không có dữ liệu");
            }

            // Thông tin tài sản lạ
            if (taiSanLa.Count > 0)
            {
                var lastRecord3 = taiSanLa.Last();
                taiSanLa.ForEach(la =>
                {
                    stt++;
                    if (la != lastRecord3)
                    {
                        body = body.Replace("{data4}", "<tr>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt3}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{maTaiSan3}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{viTri3}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{trangThaiTaiSan3}</td>" +
                            "</tr>" +
                            "{data4}");
                    }
                    else
                    {
                        body = body.Replace("{data4}", "<tr>" +
                            "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt3}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{maTaiSan3}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{viTri3}</td>" +
                            "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{trangThaiTaiSan3}</td>" +
                            "</tr>");
                    }

                    body = body.Replace("{stt3}", stt.ToString());
                    body = body.Replace("{maTaiSan3}", la.MaTaiSan ?? string.Empty);
                    body = body.Replace("{viTri3}", la.ViTri ?? string.Empty);
                    body = body.Replace("{trangThaiTaiSan3}", la.TrangThai ?? string.Empty);
                });
            }
            else
            {
                body = body.Replace("{data4}", "Không có dữ liệu");
            }

            this.email = new Email(this.mailServerRepository);
            if (this.emailFlag.Equals("That"))
            {
                mailLanhDao.ForEach(mailNhan =>
                {
                    this.email.SendMail(9, mailNhan, subject, body); // 1-chức năng cấp phát
                });
            }
            else if (this.emailFlag.Equals("TestDFT"))
            {
                // Check mail test dft co la role lanh dao hay ko?
                if (mailLanhDao.Contains(this.mailTestDFT))
                {
                    this.email.SendMailTest(9, this.mailTestDFT, subject, body);
                }
            }
        }

        public void SendMailThanhLy(List<ViewTaiSanThanhLy> list, int? donViBaoId)
        {
            #region Check null
            if (list == null)
            {
                throw new UserFriendlyException(StringResources.NullParameter);
            }
            #endregion
            var roleLanhDaoId = this.GetRoleLanhDao();
            if (list.Count > 0)
            {
                var a = list.GroupBy(e => e.PhongBanQuanLyId).Select(e => new
                {
                    toChucId = e.Key,
                    listTaiSan = e.ToList(),
                }).ToList();
                a.ForEach(tungPB =>
                {
                    var donViQuanLy = this.toChucRepository.FirstOrDefault(w => w.Id == tungPB.toChucId).TenToChuc;
                    var mailLanhDao = (from user in this.useRepository.GetAll().Where(w => w.ToChucId == tungPB.toChucId && w.IsActive == true)
                                       from userRole in this.roleRepository.GetAll().Where(w => w.UserId == user.Id && roleLanhDaoId.Contains(w.RoleId))
                                       select user.EmailAddress).ToList();

                    string body = string.Empty;
                    var soLuong = tungPB.listTaiSan.Count;
                    var template = "MailThanhLy.html";
                    var subject = "Thông báo tài sản thanh lý";
                    using (StreamReader reader = new StreamReader(Path.Combine(this.appFolders.TempMailFolder, template)))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("{phongBan}", donViQuanLy);

                    body = body.Replace("{soLuong}", soLuong.ToString());

                    var lastRecord = tungPB.listTaiSan.Last();
                    var stt = 0;
                    foreach (var item in tungPB.listTaiSan)
                    {
                        stt++;
                        if (item != lastRecord)
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                              "</tr>" +
                                "{data}");
                        }
                        else
                        {
                            body = body.Replace("{data}", "<tr>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{stt}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{tenTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{loaiTaiSan}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{serialNumber}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: left;padding: 8px;'>{nhaCungCap}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: center;padding: 8px;'>{ngayMua}</td>" +
                                "<td style='border: 1px solid #dddddd;text-align: right;padding: 8px;'>{nguyenGia}</td>" +
                              "</tr>");
                        }

                        body = body.Replace("{stt}", stt.ToString());
                        body = body.Replace("{tenTaiSan}", item.TenTaiSan);
                        body = body.Replace("{loaiTaiSan}", item.LoaiTaiSan);
                        body = body.Replace("{serialNumber}", item.SerialNumber);
                        body = body.Replace("{nhaCungCap}", item.NhaCungCap);
                        body = body.Replace("{ngayMua}", item.NgayMua.HasValue ? item.NgayMua.Value.ToShortDateString() : string.Empty);
                        body = body.Replace("{nguyenGia}", item.NguyenGia != null ? string.Format("{0:0,0}", item.NguyenGia) : string.Empty);
                    }

                    this.email = new Email(this.mailServerRepository);
                    if (this.emailFlag.Equals("That"))
                    {
                        mailLanhDao.ForEach(mailNhan =>
                        {
                            this.email.SendMail(6, mailNhan, subject, body); // 1-chức năng cấp phát
                        });
                    }
                    else if (this.emailFlag.Equals("TestDFT"))
                    {
                        // Check mail test dft co la role lanh dao hay ko?
                        if (mailLanhDao.Contains(this.mailTestDFT))
                        {
                            this.email.SendMailTest(6, this.mailTestDFT, subject, body);
                        }
                    }
                });
            }
        }
#pragma warning restore SA1118 // Parameter should not span multiple lines
    }
}
