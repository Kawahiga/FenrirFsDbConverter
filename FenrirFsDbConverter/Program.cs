using FenrirFsDbConverter;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;

// アプリケーションのエントリーポイント
public class Program {
    public static void Main( string[] args ) {
        Console.WriteLine( "FenrirFS DB Converter started." );

        // TODO: コマンドライン引数や設定ファイルからDBのパスなどを取得する
        // 【暫定処理】パスをハードコーディング
        string fenrirDbPath = @"H:\サンプル ビデオ\Fenrir用のべたん\Fenrir管理\個人用.profile\db\FenrirFS.db";
        //string destinationDbPath = @"C:\Users\kawahigashi\Desktop\videos.db";
        string destinationDbPath = @"C:\Users\kawahigashi\AppData\Local\Packages\c262e66b-9f42-492e-b1a2-4ab4e846c855_yee5g11jw9r6r\LocalCache\Roaming\VideoManager3\videos.db";
        string hardCodePath = @"H:\サンプル ビデオ\Fenrir用のべたん\Fenrir管理\個人用.profile\files\";

        // 1. FenrirFSからデータを読み込む
        var reader = new DataBaseReaderFromFenrirFs(fenrirDbPath);
        var fenrirFiles = reader.ReadFiles();
        var fenrirLabels = reader.ReadLabels();
        var fenrirLabelGroups = reader.ReadLabelGroups();
        var fenrirLabeledFiles = reader.ReadLabeledFiles();
        
        // 2. データを新しい形式に変換する
        var converter = new DataConverter( hardCodePath );
        var newAppFiles = converter.ConvertFiles(fenrirFiles, fenrirLabeledFiles);
        var newAppTags = converter.ConvertTags(fenrirLabels, fenrirLabelGroups);
        var newAppVideoTag = converter.ConvertVideoTags(fenrirLabeledFiles);
        var newAppArtists = converter.ConvertArtists(newAppFiles, newAppTags);
        converter.LinkVideoArtists( newAppArtists, newAppVideoTag, newAppTags, newAppFiles );
        converter.RemoveYUUMEITags( newAppTags, "有名" ); // "有名"タググループを削除

        // 3. 新しいDBにデータを書き込む
        var writer = new DataBaseWriter(destinationDbPath);
        writer.SaveFiles( newAppFiles );
        writer.SaveTags( newAppTags );
        writer.SaveVideoTags( newAppVideoTag, newAppFiles, newAppTags );
        writer.SaveArtists( newAppArtists );
        writer.SaveVideoArtists( newAppArtists );

        Console.WriteLine( "Conversion completed successfully!" );
        Console.ReadLine();
    }
}






