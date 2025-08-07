using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter.ModelNewDB {
    internal class NewTag {

        public int TagId { get; set; } // タグID (主キー)
        public string? TagName { get; set; } // タグ名
        public string? TagColor { get; set; } // タグの色 (例: "Red", "Blue")
        public int? Parent { get; set; } // 親タグのID (NULLの場合はトップレベル)
        public int? OrderInGroup { get; set; } // グループ内での表示順
        public int? IsExpanded { get; set; }  // 折りたたまれているかどうか (1: 折りたたまれている)
        public int? IsGroup { get; set; } // グループかどうか (1: グループ, 0: タグ)
        public int UpdatedId { get; set; } // 更新後の新ID
    }
}
