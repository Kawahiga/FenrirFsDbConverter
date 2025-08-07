using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter.ModelNewDB {
    // 変換先DBのFile情報を表すクラス
    internal class NewFile {
        public int Id { get; set; } // データベースの主キー
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }  // ファイルサイズ（バイト単位）
        public DateTime LastModified { get; set; }  // 最終更新日時（YYYY/MM/dd HH:mm:ss形式）
        public double Duration { get; set; }    // 動画の再生時間（秒）
    }
}
