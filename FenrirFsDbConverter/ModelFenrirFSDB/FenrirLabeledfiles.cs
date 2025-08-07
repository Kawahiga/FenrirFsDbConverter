using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter.ModelFenrirFSDB {
    internal class FenrirLabeledfiles {
        // ラベル付きファイルID (このテーブルの主キー)
        public int? LabeledFileID { get; set; }

        // 関連付けられたラベルのID (labels.LabelIDへの外部キー)
        public int? LabelID { get; set; }

        // 関連付けられたファイルのID (files.FileIdへの外部キー)
        public int? FileId { get; set; }

        // 関連フォルダ
        public string? RelatedFolder { get; set; }
    }
}
