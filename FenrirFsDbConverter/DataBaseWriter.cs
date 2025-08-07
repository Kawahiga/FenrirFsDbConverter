using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter {
    // データ書き込み担当
    internal class DataBaseWriter {

        private readonly string _dbPath;

        public DataBaseWriter( string dbPath ) {
            _dbPath = dbPath;
        }

        // / ファイル情報を変換後の形式でDBに保存する
        public void SaveFiles( List<object> files ) {
            // TODO: DBに接続し、変換後のファイル情報を書き込む
            Console.WriteLine( "Writing file data to destination DB..." );
        }

        // ラベル情報を変換後の形式でDBに保存する
        public void SaveLabels( List<object> labels ) {
            // TODO: DBに接続し、変換後のカテゴリ情報を書き込む
            Console.WriteLine( "Writing label data to destination DB..." );
        }
    }
}
