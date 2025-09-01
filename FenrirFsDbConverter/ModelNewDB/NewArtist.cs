using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter.ModelNewDB {
    internal class NewArtist {
        // アーティストID (主キー)
        public int Id { get; set; } = 0;

        // アーティストが出演する動画のリスト
        public List<NewFile> VideoIds { get; set; } = new List<NewFile>();

        // アーティスト名
        public string Name { get; set; } = string.Empty;

        // お気に入り
        public bool IsFavorite { get; set; } = false;

        // いいね数
        public int LikeCount { get; set; } = 0;

        // アイコンパス (例: "icon.png")
        public string IconPath { get; set; } = string.Empty;

        // 色 (例: "Red", "Blue")
        //public string TagColor { get; set; } = string.Empty;
    }
}
