using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter.ModelFenrirFSDB {
    internal class FenrirFilter {
        // フィルターID
        public int? FilterId { get; set; }

        // このフィルターが有効かどうか (1: 有効)
        public int? Enabled { get; set; }

        // ファイル名のフィルター条件
        public string? FileName { get; set; }

        // ファイル名の比較タイプ (例: 0=含む, 1=一致する)
        public int? FileNameCompType { get; set; }

        // 拡張子のフィルター条件
        public string? Extension { get; set; }

        // 拡張子の比較タイプ
        public int? ExtensionCompType { get; set; }

        // コメントのフィルター条件
        public string? Comment { get; set; }

        // コメントの比較タイプ
        public int? CommentCompType { get; set; }

        // ファイルサイズでのフィルターが有効かどうか
        public int? FileSizeEnabled { get; set; }

        // 最小ファイルサイズ (バイト)
        public int? MinFileSize { get; set; }

        // 最大ファイルサイズ (バイト)
        public int? MaxFileSize { get; set; }

        // 日付でのフィルターが有効かどうか
        public int? DateEnabled { get; set; }

        // 期間フィルターの開始日
        public string? StartDate { get; set; }

        // 期間フィルターの終了日
        public string? EndDate { get; set; }

        // トレイをスキップするかどうか
        public int? SkipTray { get; set; }

        // フィルターに一致した場合に設定するスターの数
        public int? SetStar { get; set; }

        // ラベルでのフィルターが有効かどうか
        public int? LabelEnabled { get; set; }

        // フィルター対象のラベルID
        public int? LabelId { get; set; }

        // 削除するかどうか
        public int? DoDelete { get; set; }

        // 即時適用するかどうか
        public int? ImmediateApply { get; set; }

        // フィルターの適用順
        public int? FilterOrder { get; set; }
    }
}
