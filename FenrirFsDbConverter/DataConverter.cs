using FenrirFsDbConverter.ModelFenrirFSDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// メモ
// if ( string.IsNullOrEmpty( filePath ) ) {
// ファイルパスが空の場合はハードコード
// filePath = string.Concat( @"H:\サンプル ビデオ\Fenrir用のべたん\Fenrir管理\個人用.profile\files\", fileName );
// }

namespace FenrirFsDbConverter {
    // データ変換担当
    internal class DataConverter {

        // FenrirFSのデータを新しいアプリケーションの形式に変換する
        public List<object> ConvertFiles( List<FenrirFile> fenrirFiles ) {
            // TODO: FenrirFileのリストをNewAppFileのリストに変換するロジックを実装
            Console.WriteLine( "Converting file data..." );
            var newFiles = new List<object>();
            //foreach ( var f in fenrirFiles ) {
            //    newFiles.Add( new NewAppFile { FilePath = f.Path } );
            //}
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
