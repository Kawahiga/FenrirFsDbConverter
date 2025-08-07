using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter.ModelFenrirFSDB {
    // fenrirFSのラベル情報を表すクラス
    internal class FenrirLabel {

        // ラベルID (主キー)
        public int? LabelID { get; set; }

        // ラベル名
        public string? LabelName { get; set; }

        // ラベルの色名 (例: "Red", "Blue")
        public string? LabelColorName { get; set; }

        // 所属するグループのID
        public int? GroupId { get; set; }

        // グループ内での表示順
        public int? OrderInGroup { get; set; }

        // 作成順
        public int? CreateOrder { get; set; }

        // 使用頻度 (Frequency)
        public int? Freq { get; set; }

        // ラベルのGUID
        public string? Guid { get; set; }

        // 最終更新日時
        public string? LastModified { get; set; }

        // 複合アイテム内での順序
        public int? CompoundItemOrder { get; set; }

        // 作成日時
        public string? CreationTime { get; set; }

        // 自動同期で生成されたかどうかのフラグ
        public int? AutoSyncGenerated { get; set; }
    }
}

