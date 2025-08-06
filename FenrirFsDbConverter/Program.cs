using System;
using System.Collections.Generic;

// アプリケーションのエントリーポイント
public class Program {
    public static void Main( string[] args ) {
        Console.WriteLine( "FenrirFS DB Converter started." );

        // TODO: コマンドライン引数や設定ファイルからDBのパスなどを取得する
        string fenrirDbPath = "path/to/fenrirfs.db";
        string destinationDbPath = "path/to/new_app.db";

        // 1. FenrirFSからデータを読み込む
        var reader = new FenrirFsReader(fenrirDbPath);
        var fenrirFiles = reader.ReadAllFiles();
        var fenrirTags = reader.ReadAllTags();
        // ... 他のデータも同様に読み込む

        // 2. データを新しい形式に変換する
        var converter = new DataConverter();
        var newAppFiles = converter.ConvertFiles(fenrirFiles);
        var newAppCategories = converter.ConvertTagsToCategories(fenrirTags);
        // ...

        // 3. 新しいDBにデータを書き込む
        var writer = new DestinationDbWriter(destinationDbPath);
        writer.SaveFiles( newAppFiles );
        writer.SaveCategories( newAppCategories );
        // ...

        Console.WriteLine( "Conversion completed successfully!" );
        Console.ReadLine();
    }
}

// --- 以下は各クラスのスケルトン（骨格） ---

// FenrirFSのデータを表現するモデル
public class FenrirFile {
    public int Id { get; set; }
    public string Path { get; set; }
    // ... 他のプロパティ
}

public class FenrirTag {
    public int Id { get; set; }
    public string Name { get; set; }
    // ... 他のプロパティ
}


// 新しいアプリのデータを表現するモデル
public class NewAppFile {
    public int FileId { get; set; }
    public string FilePath { get; set; }
    // ... 他のプロパティ
}

public class NewAppCategory {
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    // ... 他のプロパティ
}


// データ読み込み担当
public class FenrirFsReader {
    private readonly string _dbPath;

    public FenrirFsReader( string dbPath ) {
        _dbPath = dbPath;
    }

    public List<FenrirFile> ReadAllFiles() {
        // TODO: DBに接続し、ファイル情報を読み込んでFenrirFileのリストを返す
        Console.WriteLine( "Reading files from FenrirFS DB..." );
        return new List<FenrirFile>();
    }

    public List<FenrirTag> ReadAllTags() {
        // TODO: DBに接続し、タグ情報を読み込んでFenrirTagのリストを返す
        Console.WriteLine( "Reading tags from FenrirFS DB..." );
        return new List<FenrirTag>();
    }
}

// データ変換担当
public class DataConverter {
    public List<NewAppFile> ConvertFiles( List<FenrirFile> fenrirFiles ) {
        // TODO: FenrirFileのリストをNewAppFileのリストに変換するロジックを実装
        Console.WriteLine( "Converting file data..." );
        var newFiles = new List<NewAppFile>();
        foreach ( var f in fenrirFiles ) {
            newFiles.Add( new NewAppFile { FilePath = f.Path } );
        }
        return newFiles;
    }

    public List<NewAppCategory> ConvertTagsToCategories( List<FenrirTag> fenrirTags ) {
        // TODO: FenrirTagのリストをNewAppCategoryのリストに変換するロジックを実装
        Console.WriteLine( "Converting tag data..." );
        var newCategories = new List<NewAppCategory>();
        foreach ( var t in fenrirTags ) {
            newCategories.Add( new NewAppCategory { CategoryName = t.Name } );
        }
        return newCategories;
    }
}

// データ書き込み担当
public class DestinationDbWriter {
    private readonly string _dbPath;

    public DestinationDbWriter( string dbPath ) {
        _dbPath = dbPath;
    }

    public void SaveFiles( List<NewAppFile> files ) {
        // TODO: DBに接続し、変換後のファイル情報を書き込む
        Console.WriteLine( "Writing file data to destination DB..." );
    }

    public void SaveCategories( List<NewAppCategory> categories ) {
        // TODO: DBに接続し、変換後のカテゴリ情報を書き込む
        Console.WriteLine( "Writing category data to destination DB..." );
    }
}