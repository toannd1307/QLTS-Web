namespace MyProject.QuanLyMailServer.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Abp.Application.Services.Dto;
    using Abp.AutoMapper;
    using DbEntities;

    [AutoMap(typeof(MailServer))]

    public class MailServerDto : EntityDto<int>
    {
        public virtual string Email { get; set; }

        public virtual string Password { get; set; }

        public virtual string Host { get; set; }

        public virtual string Port { get; set; }

        public virtual bool CapPhat { get; set; }

        public virtual bool ThuHoi { get; set; }

        public virtual bool DieuChuyen { get; set; }

        public virtual bool BaoMat { get; set; }

        public virtual bool BaoHong { get; set; }

        public virtual bool ThanhLy { get; set; }

        public virtual bool SuaChuaBaoDuong { get; set; }

        public virtual bool BatDauKiemKe { get; set; }

        public virtual bool KetThucKiemKe { get; set; }

        public virtual bool HoanThanhPhieuDuTruMuaSam { get; set; }

        public virtual bool HuyBoPhieuDuTruMuaSam { get; set; }

        public virtual string GhiChu { get; set; }
    }
}
