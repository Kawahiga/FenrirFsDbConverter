using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter.ModelFenrirFSDB {
    internal class FenrirLabelGroup {
        // ラベルグループID
        public int? LabelGroupID { get; set; }

        // ラベルグループ名
        public string? LabelGroupName { get; set; }

        // リスト内での表示順
        public int? OrderInList { get; set; }

        // デフォルトのグループかどうか
        public int? IsDefault { get; set; }

        // グループアイコン
        public int? GroupIcon { get; set; }

        // 折りたたまれているかどうか (1: 折りたたまれている)
        public int? Folded { get; set; }

        // グループのGUID
        public string? Guid { get; set; }

        // 最終更新日時
        public string? LastModified { get; set; }

        // 複合アイテム内での順序
        public int? CompoundItemOrder { get; set; }

        // 作成日時
        public string? CreationTime { get; set; }

        // 親グループのID
        public int? ParentGroupId { get; set; }
    }
}
