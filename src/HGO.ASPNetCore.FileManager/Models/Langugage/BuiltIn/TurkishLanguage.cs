using HGO.ASPNetCore.FileManager.Models.Langugage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.Models.Language.BuiltIn
{
    public class TurkishLanguage : ILanguage
    {
        // Properties with backing fields
        public string Browse { get; } = "Gözat";
        public string Copy { get; } = "Kopyala";
        public string Cut { get; } = "Kes";
        public string Paste { get; } = "Yapıştır";
        public string Rename { get; } = "Yeniden Adlandır";
        public string Edit { get; } = "Düzenle";
        public string Delete { get; } = "Sil";
        public string CreateNewFolder { get; } = "Yeni Klasör Oluştur";
        public string NewFolderPlaceHolder { get; } = "Yeni Klasör";
        public string CreateNewFile { get; } = "Yeni Dosya Oluştur";
        public string NewFilePlaceHolder { get; } = "Yeni Metin.txt";
        public string View { get; } = "Görüntüle";
        public string Download { get; } = "İndir";
        public string Search { get; } = "Ara";
        public string Zip { get; } = "Sıkıştır";
        public string Unzip { get; } = "Aç";
        public string GetFolderContent { get; } = "Klasör İçeriğini Al";
        public string GetFileContent { get; } = "Dosya İçeriğini Al";
        public string Upload { get; } = "Yükle";
        public string ToggleView { get; } = "Görüntü Değiştir";
        public string Reload { get; } = "Yenile";
        public string Breadcrumb { get; } = "Kırıntı";
        public string FoldersTree { get; } = "Klasör Ağaç";
        public string MenuBar { get; } = "Menü Çubuğu";
        public string ContextMenu { get; } = "Bağlam Menüsü";
        public string FilePreview { get; } = "Dosya Önizleme";
        public string NoItemsSelectedMessage { get; } = "Lütfen istediğiniz öğeleri seçin.";
    }
}

