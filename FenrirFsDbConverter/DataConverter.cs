using FenrirFsDbConverter.ModelFenrirFSDB;
using FenrirFsDbConverter.ModelNewDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FenrirFsDbConverter {
    // データ変換担当
    internal class DataConverter {

        private readonly string _hardCodePath;

        public DataConverter( string hardCodePath ) {
            _hardCodePath = hardCodePath;
        }

        // FenrirFSのデータを新しいアプリケーションの形式に変換する
        public List<NewFile> ConvertFiles( List<FenrirFile> fenrirFiles ) {
            // TODO: FenrirFileのリストをNewAppFileのリストに変換するロジックを実装
            Console.WriteLine( "Converting file data..." );
            var newFiles = new List<NewFile>();

            foreach ( var fenlirFile in fenrirFiles ) {

                var id = fenlirFile.FileID; // データベースの主キー

                var fileName = fenlirFile.DisplayFileName;
                // ファイル名が空の場合はスキップ
                if ( string.IsNullOrEmpty( fileName ) ) continue;
                
                var filePath = fenlirFile.AliasTarget;
                // ファイルパスが空の場合はハードコード
                if ( string.IsNullOrEmpty( filePath ) ) filePath = string.Concat( _hardCodePath, fileName );

                var fileSize = fenlirFile.FileSize ?? 0; // ファイルサイズがnullの場合は0とする

                // ファイルの書き込み日時をISO 8601形式で保存するため、DateTimeに変換
                var dateTimeString = $"{fenlirFile.LastWriteDate}T{fenlirFile.LastWriteTime}";
                var lastModified = DateTime.Parse( dateTimeString );

                double duration = fenlirFile.MediaDuration * 0.0000001 ?? 0.0; // メディアの再生時間がnullの場合は0とする

                var file = new NewFile {
                    Id = id,
                    FileName = fileName,
                    FilePath = filePath,
                    FileSize = fileSize,
                    LastModified = lastModified,
                    Duration = duration
                };

                newFiles.Add( file );
            }
            return newFiles;
        }

        // FenrirFSのタグ情報を新しいアプリケーションのカテゴリ形式に変換する
        //public List<NewAppCategory> ConvertTagsToCategories( List<FenrirTag> fenrirTags ) {
        //    // TODO: FenrirTagのリストをNewAppCategoryのリストに変換するロジックを実装
        //    Console.WriteLine( "Converting tag data..." );
        //    var newCategories = new List<NewAppCategory>();
        //    foreach ( var t in fenrirTags ) {
        //        newCategories.Add( new NewAppCategory { CategoryName = t.Name } );
        //    }
        //    return newCategories;
        //}
    }
}
