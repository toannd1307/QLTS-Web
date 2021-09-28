namespace MyProject.Global
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;
    using Abp.Domain.Repositories;
    using Abp.IO;
    using Abp.UI;
    using DbEntities;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using MyProject;
    using MyProject.Data;
    using MyProject.Global;
    using MyProject.Global.Dtos;
    using MyProject.Net.MimeTypes;
    using MyProject.QuanLyLoaiTaiSan.Dtos;
    using MyProject.QuanLyMailServer.Dtos;
    using MyProject.Shared;
    using Newtonsoft.Json;
    using OfficeOpenXml;

    public class Email
    {
        private readonly IRepository<MailServer> mailServerRepository;

        public Email(IRepository<MailServer> mailServerRepository)
        {
            this.mailServerRepository = mailServerRepository;
        }

        public void SendMail(int loaiChucNang, string to, string subject, string body, string cc = "", string bcc = "")
        {
            if (string.IsNullOrEmpty(to) || to == null || !to.Contains("@"))
            {
                throw new UserFriendlyException(StringResources.EmailSaiDinhDang, "Có lỗi");
            }

            var mailServer = this.mailServerRepository.GetAll().First();

            using MailMessage mailMessage = new MailMessage();

            // Cấu hình server gửi mail
            using SmtpClient smtp = new SmtpClient
            {
                Port = int.Parse(mailServer.Port),
                Host = mailServer.Host,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
            };
            smtp.Credentials = new NetworkCredential(mailServer.Email, mailServer.Password);
            mailMessage.From = new MailAddress(mailServer.Email);

            // end mobifone
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            mailMessage.BodyEncoding = Encoding.UTF8;

            mailMessage.To.Add(new MailAddress(to));
            if (!string.IsNullOrEmpty(cc))
            {
                mailMessage.CC.Add(cc);
            }

            if (!string.IsNullOrEmpty(bcc))
            {
                mailMessage.Bcc.Add(bcc);
            }

            // Check chức năng có được phép gửi mail không ?
            var allowSentMail = (bool)typeof(MailServer).GetProperty(GlobalModel.ListLuaChonGuiMail[loaiChucNang]).GetValue(mailServer, null);
            if (allowSentMail)
            {
                smtp.Send(mailMessage);
            }
        }

        public void SendMailTest(int loaiChucNang, string to, string subject, string body, string cc = "", string bcc = "")
        {
            if (string.IsNullOrEmpty(to) || to == null || !to.Contains("@"))
            {
                throw new UserFriendlyException(StringResources.EmailSaiDinhDang, "Có lỗi");
            }

            var mailServer = this.mailServerRepository.GetAll().First();

            using MailMessage mailMessage = new MailMessage();

            // Cấu hình server gửi mail
            using SmtpClient smtp = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = true,
            };
            smtp.Credentials = new NetworkCredential("aspnetzero.dft.testmail@gmail.com", "dft@1234");
            mailMessage.From = new MailAddress("aspnetzero.dft.testmail@gmail.com", "Công ty DFT");

            // end mobifone
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;
            mailMessage.BodyEncoding = Encoding.UTF8;

            mailMessage.To.Add(new MailAddress(to));
            if (!string.IsNullOrEmpty(cc))
            {
                mailMessage.CC.Add(cc);
            }

            if (!string.IsNullOrEmpty(bcc))
            {
                mailMessage.Bcc.Add(bcc);
            }

            // Check chức năng có được phép gửi mail không ?
            var allowSentMail = (bool)typeof(MailServer).GetProperty(GlobalModel.ListLuaChonGuiMail[loaiChucNang]).GetValue(mailServer, null);
            if (allowSentMail)
            {
                smtp.Send(mailMessage);
            }
        }
    }
}