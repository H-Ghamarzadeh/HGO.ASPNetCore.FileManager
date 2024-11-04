using HGO.ASPNetCore.FileManager.Models.LanguageModels;

namespace HGO.ASPNetCore.FileManager.Models.LanguageModels.BuiltIn
{
    public sealed class TurkishLanguage : LanguageBase, ILanguage
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string Browse { get; } = "Gözat";
        public string Copy { get; } = "Kopyala";
        public string Cut { get; } = "Kes";
        public string Paste { get; } = "Yapıştır";
        public string Rename { get; } = "Yeniden Adlandır";
        public string Edit { get; } = "Düzenle";
        public string Save { get; } = "Kaydet";
        public string Delete { get; } = "Sil";
        public string CreateNewFolder { get; } = "Yeni Klasör Oluştur";
        public string CreateNewFile { get; } = "Yeni Dosya Oluştur";
        public string View { get; } = "Görüntüle";
        public string Download { get; } = "İndir";
        public string Search { get; } = "Ara";
        public string Zip { get; } = "Sıkıştır";
        public string Unzip { get; } = "Sıkıştırmayı Aç";
        public string GetFolderContent { get; } = "Klasör İçeriğini Al";
        public string GetFileContent { get; } = "Dosya İçeriğini Al";
        public string Upload { get; } = "Yükle";
        public string ToggleView { get; } = "Görünümü Değiştir";
        public string Reload { get; } = "Yenile";
        public string Breadcrumb { get; } = "Kırıntı Gezinti";
        public string FoldersTree { get; } = "Klasör Ağacı";
        public string MenuBar { get; } = "Menü Çubuğu";
        public string ContextMenu { get; } = "Bağlam Menüsü";
        public string FilePreview { get; } = "Dosya Önizlemesi";
        public string NewFolderPlaceHolder { get; } = "Yeni Klasör";
        public string NewFilePlaceHolder { get; } = "Yeni Metin.txt";
        public string NoItemsSelectedMessage { get; } = "Lütfen istediğiniz öğeyi(leri) seçin.";
        public string CreateDate { get; } = "Oluşturma Tarihi";
        public string ModifiedDate { get; } = "Değiştirilme Tarihi";
        public string FileName { get; } = "Dosya Adı";
        public string FolderName { get; } = "Klasör Adı";
        public string Size { get; } = "Boyut";
        public string Back { get; } = "Geri";
        public string Up { get; } = "Yukarı";
        public string Close { get; } = "Kapat";
        public string EnterNewFolderNameMessage { get; } = "Lütfen klasör adını girin:";
        public string EnterNewFileNameMessage { get; } = "Lütfen istediğiniz dosya adını girin:";
        public string DeleteConfirmationMessage { get; } = "Seçilen öğeleri silmek istediğinizden emin misiniz?";
        public string RenameMessage { get; } = "Lütfen yeni adı girin:";
        public string ItemAlreadyExistMessage { get; } = "zaten var, üzerine yazmak ister misiniz?";
        public string ZipFileNameMessage { get; } = "Lütfen Zip dosyası adını girin:";
        public string OverrideConfirmationMessage { get; } = "Bazı öğeler zaten var, üzerine yazmak ister misiniz?";

        // Dropzone-related messages in Turkish
        public string DictDefaultMessage { get; } = "Dosyaları buraya sürükleyin veya yüklemek için tıklayın";
        public string DictFallbackMessage { get; } = "Tarayıcınız dosya yüklemeyi desteklemiyor.";
        public string DictFallbackText { get; } = "Eski tarayıcıları kullanıyorsunuz, lütfen dosyalarınızı manuel olarak yükleyin.";
        public string DictFileTooBig { get; } = "Dosya çok büyük ({{filesize}}MiB). Maksimum dosya boyutu: {{maxFilesize}}MiB.";
        public string DictInvalidFileType { get; } = "Bu dosya türüne izin verilmiyor.";
        public string DictResponseError { get; } = "Sunucu {{statusCode}} hata yanıtı verdi.";
        public string DictCancelUpload { get; } = "Yüklemeyi iptal et";
        public string DictCancelUploadConfirmation { get; } = "Bu yüklemeyi iptal etmek istediğinizden emin misiniz?";
        public string DictRemoveFile { get; } = "Dosyayı kaldır";
        public string DictMaxFilesExceeded { get; } = "Daha fazla dosya yükleyemezsiniz.";
    }
}
