using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter.ModelFenrirFSDB {
    // fenrirFSのFile情報を表すクラス
    internal class FenrirFile {
        // ファイルID (主キー)
        public int FileID { get; set; }

        // ファイル名
        public string? FileName { get; set; }

        // ファイルサイズ
        public long? FileSize { get; set; }

        // コメント
        public string? Comment { get; set; }

        // スター (5段階評価)
        public int? Star { get; set; }

        // ポイント
        public int? Point { get; set; }

        // 所属するフォルダのID
        public int? FolderId { get; set; }

        // タイムスタンプ
        public int? TimeStamp { get; set; }

        // 1日の実行回数
        public int? ExecuteCountInOneDay { get; set; }

        // 最終実行日 (YYYY/MM/DD)
        public string? LastExecuteDate { get; set; }

        // 最終実行時刻 (HH:MM:SS)
        public string? LastExecuteTime { get; set; }

        // 最終アクセス日 (YYYY/MM/DD)
        public string? LastAccessDate { get; set; }

        // 最終アクセス時刻 (HH:MM:SS)
        public string? LastAccessTime { get; set; }

        // 拡張子
        public string? Extension { get; set; }

        // エイリアス (ショートカット) かどうか
        public int? Alias { get; set; }

        // エイリアスの場合のリンク先パス
        public string? AliasTarget { get; set; }

        // 表示用のファイル名
        public string? DisplayFileName { get; set; }

        // オリジナルのエイリアスファイル名
        public string? OrgAliasFileName { get; set; }

        // 作業ディレクトリ
        public string? WorkDir { get; set; }

        // 実行時の引数
        public string? Arguments { get; set; }

        // 実行時のウィンドウ表示状態
        public int? ShowCmd { get; set; }

        // フォルダかどうか (1: フォルダ, 0: ファイル)
        public int? IsFolder { get; set; }

        // work_flg (用途不明なフラグ)
        public int? work_flg { get; set; }

        // DBに追加された日 (YYYY/MM/DD)
        public string? AddDate { get; set; }

        // ゴミ箱へ移動する前の元のフォルダパス
        public string? MoveTrashOrgFolder { get; set; }

        // ファイルのGUID
        public string? Guid { get; set; }

        // ファイルの最終更新日時
        public string? LastModified { get; set; }

        // システムファイルインデックス
        public int? SysFileIndex { get; set; }

        // システム最終書き込み日時 (Unix時間など)
        public int? SysLastWrite { get; set; }

        // ファイルの最終書き込み日 (YYYY/MM/DD)
        public string? LastWriteDate { get; set; }

        // ファイルの最終書き込み時刻 (HH:MM:SS)
        public string? LastWriteTime { get; set; }

        // Windowsが認識しているファイルの種類 (例: "テキスト ドキュメント")
        public string? PerceivedType { get; set; }

        // 画像の幅 (ピクセル)
        public int? ImageHorizontalSize { get; set; }

        // 画像の高さ (ピクセル)
        public int? ImageVerticalSize { get; set; }

        // 動画や音声の再生時間
        public long? MediaDuration { get; set; }

        // 写真の撮影日 (YYYY/MM/DD)
        public string? PhotoTakenDate { get; set; }

        // 写真の撮影時刻 (HH:MM:SS)
        public string? PhotoTakenTime { get; set; }

        // ファイル名 (小文字)
        public string? FileName_L { get; set; }

        // エイリアスターゲット (小文字)
        public string? AliasTarget_L { get; set; }

        // 拡張子 (小文字)
        public string? Extension_L { get; set; }

        // 隠しファイル属性かどうか (1: 隠しファイル)
        public int? Hidden { get; set; }

        // 写真の撮影日時 (GMT)
        public int? PhotoDateGMT { get; set; }

        // 画像の向き (EXIF情報)
        public int? ImageOrientation { get; set; }

        // システムプロパティをチェックしたかどうか
        public int? SysPropChecked { get; set; }

        // ファイルの作成日 (YYYY/MM/DD)
        public string? CreationDate { get; set; }

        // ファイルの作成時刻 (HH:MM:SS)
        public string? CreationTime { get; set; }

        // ファイル情報をチェックした日時
        public string? FileInfoCheckedDateTime { get; set; }

        // エイリアスのリンク先が存在するかどうか (1: 存在する)
        public int? AliasTargetExists { get; set; }

        // ファイルの最終更新日時 (UTC)
        public string? LastWriteDateUTC { get; set; }

        // ファイルの最終更新時刻 (UTC)
        public string? LastWriteTimeUTC { get; set; }

        // ファイルの作成日時 (UTC)
        public string? CreationDateUTC { get; set; }

        // ファイルの作成時刻 (UTC)
        public string? CreationTimeUTC { get; set; }
    }
}
