using FenrirFsDbConverter;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System;
using System.Collections.Generic;

// アプリケーションのエントリーポイント
public class Program {
    public static void Main( string[] args ) {
        Console.WriteLine( "FenrirFS DB Converter started." );

        //SQLitePCL.Batteries.Init(); // SQLiteの初期化

        // TODO: コマンドライン引数や設定ファイルからDBのパスなどを取得する
        // 【暫定処理】パスをハードコーディング
        string fenrirDbPath = @"C:\Users\kawahigashi\Desktop\えっち.db";
        //string destinationDbPath = @"C:\Users\kawahigashi\Desktop\newDB.db";

        // 1. FenrirFSからデータを読み込む
        var reader = new DataBaseReaderFromFenrirFs(fenrirDbPath);
        var fenrirFiles = reader.ReadFiles();
        //var fenrirLabels = reader.ReadAllLabels();
        //var fenrirLabelGroups = reader.ReadAllLabelGroups();
        //var fenrirLabeledFiles = reader.ReadAllTagsLabeledFiles();
        //var fenrirFilters = reader.ReadAllFilters();
        // ... 他のデータも同様に読み込む

        // 2. データを新しい形式に変換する
        //var converter = new DataConverter();
        //var newAppFiles = converter.ConvertFiles(fenrirFiles);
        //var newAppCategories = converter.ConvertTagsToCategories(fenrirTags);
        // ...

        // 3. 新しいDBにデータを書き込む
        //var writer = new DestinationDbWriter(destinationDbPath);
        //writer.SaveFiles( newAppFiles );
        //writer.SaveCategories( newAppCategories );
        // ...

        Console.WriteLine( "Conversion completed successfully!" );
        //Console.ReadLine();
    }
}






