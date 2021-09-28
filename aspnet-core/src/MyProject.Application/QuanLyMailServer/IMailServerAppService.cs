namespace MyProject.QuanLyMailServer
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DbEntities;
    using MyProject.Global.Dtos;
    using MyProject.QuanLyMailServer.Dtos;

    public interface IMailServerAppService
    {
        Task GetBaoCao();

        Task<MailServer> GetForEditAsync();

        void SendMailBatDauKiemKe(long idKiemKe);

        void SendMailKetThucKiemKe(long idKiemKe);
    }
}
