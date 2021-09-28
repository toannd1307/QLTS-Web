namespace MyProject.Users.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Abp.Application.Services.Dto;

    public class UsersForGetFliterViewDto
    {
        public string Keyword { get; set; }

        public int? ToChucId { get; set; }
    }
}
