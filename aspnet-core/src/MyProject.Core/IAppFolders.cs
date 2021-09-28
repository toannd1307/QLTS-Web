namespace MyProject
{
    public interface IAppFolders
    {
        string TempFileDownloadFolder { get; }

        string DemoUploadFolder { get; }

        string DemoFileDownloadFolder { get; }

        string NhaCungCapFileDownloadFolder { get; }

        string NhaCungCapUploadFolder { get; }

        string DuTruMuaSamUploadFolder { get; }

        string DuTruMuaSamFileDownloadFolder { get; }

        string ToanBoTSFileDownloadFolder { get; }

        string ToanBoTSUploadFolder { get; }

        string DauLocFileDownloadFolder { get; }

        string DauLocFileUploadFolder { get; }

        string LoaiTaiSanFileDownloadFolder { get; }

        string LoaiTaiSanUploadFolder { get; }

        string ToChucFileDownloadFolder { get; }

        string ToChucUploadFolder { get; }

        string ViTriDiaLyFileDownloadFolder { get; }

        string ViTriDiaLyUploadFolder { get; }

        string TempMailFolder { get; }

        string AndroidAppFolder { get; }

        string IphoneAppFolder { get; }
    }
}