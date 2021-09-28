namespace MyProject
{
    using Abp.Dependency;

    public class AppFolders : IAppFolders, ISingletonDependency
    {
        public string TempFileDownloadFolder { get; set; }

        public string DemoUploadFolder { get; set; }

        public string DemoFileDownloadFolder { get; set; }

        public string NhaCungCapFileDownloadFolder { get; set; }

        public string NhaCungCapUploadFolder { get; set; }

        public string DuTruMuaSamUploadFolder { get; set; }

        public string DuTruMuaSamFileDownloadFolder { get; set; }

        public string ToanBoTSFileDownloadFolder { get; set; }

        public string ToanBoTSUploadFolder { get; set; }

        public string DauLocFileDownloadFolder { get; set; }

        public string DauLocFileUploadFolder { get; set; }

        public string LoaiTaiSanFileDownloadFolder { get; set; }

        public string LoaiTaiSanUploadFolder { get; set; }

        public string ToChucFileDownloadFolder { get; set; }

        public string ToChucUploadFolder { get; set; }

        public string ViTriDiaLyFileDownloadFolder { get; set; }

        public string ViTriDiaLyUploadFolder { get; set; }

        public string TempMailFolder { get; set; }

        public string AndroidAppFolder { get; set; }

        public string IphoneAppFolder { get; set; }
    }
}