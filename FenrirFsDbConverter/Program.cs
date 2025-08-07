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
        string fenrirDbPath = @"C:\Users\kawahigashi\Desktop\えっち.db";
        string destinationDbPath = @"C:\Users\kawahigashi\Desktop\newDB.db";
        string hardCodePath = @"H:\サンプル ビデオ\Fenrir用のべたん\Fenrir管理\個人用.profile\files\";

        // 1. FenrirFSからデータを読み込む
        var reader = new DataBaseReaderFromFenrirFs(fenrirDbPath);
        var fenrirFiles = reader.ReadFiles();
        var fenrirLabels = reader.ReadLabels();
        var fenrirLabelGroups = reader.ReadLabelGroups();
        var fenrirLabeledFiles = reader.ReadLabeledFiles();
        //var fenrirFilters = reader.ReadFilters();
        // ... 他のデータも同様に読み込む

        // 2. データを新しい形式に変換する
        var converter = new DataConverter( hardCodePath );
        var newAppFiles = converter.ConvertFiles(fenrirFiles);
        var newAppTags = converter.ConvertTags(fenrirLabels, fenrirLabelGroups);
        // ...

        // 3. 新しいDBにデータを書き込む
        var writer = new DataBaseWriter(destinationDbPath);
        writer.SaveFiles( newAppFiles );
        writer.SaveTags( newAppTags );
        // ...

        Console.WriteLine( "Conversion completed successfully!" );
        Console.ReadLine();
    }
}






