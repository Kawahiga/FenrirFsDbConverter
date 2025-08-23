using FenrirFsDbConverter.ModelFenrirFSDB;
using FenrirFsDbConverter.ModelNewDB;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter {
    // データ書き込み担当
    internal class DataBaseWriter {

        private readonly string _dbPath;
        
        // 書き込み進捗の表示用
        private int totalItems;
        private int lastPercentage;

        public DataBaseWriter( string dbPath ) {
            _dbPath = dbPath;
            InitializeDatabase();
        }

        private void InitializeDatabase() {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            try {
                var command = connection.CreateCommand();

                command.CommandText =
                @"
                    DROP TABLE IF EXISTS Videos;
                    DROP TABLE IF EXISTS Tags;
                    DROP TABLE IF EXISTS VideoTags;
                    DROP TABLE IF EXISTS Artists;  
                    DROP TABLE IF EXISTS VideoArtists;

                    CREATE TABLE IF NOT EXISTS Videos (
                        FileID INTEGER PRIMARY KEY AUTOINCREMENT,
                        FilePath TEXT NOT NULL UNIQUE,
                        FileName TEXT NOT NULL,
                        FileNameWithoutArtist TEXT NOT NULL,
                        Extension TEXT DEFAULT '',
                        FileSize INTEGER DEFAULT 0,
                        LastModified TEXT,
                        Duration REAL DeFAULT 0.0,
                        LikeCount INTEGER DEFAULT 0,
                        ViewCount INTEGER DEFAULT 0
                    );
                
                    CREATE TABLE IF NOT EXISTS Tags (
                        TagID INTEGER PRIMARY KEY AUTOINCREMENT,
                        TagName TEXT NOT NULL,
                        TagColor TEXT,
                        Parent INTEGER,
                        OrderInGroup INTEGER DEFAULT 0,
                        IsGroup BOOLEAN DEFAULT 0,
                        IsExpand BOOLEAN DEFAULT 1,
                        FOREIGN KEY (Parent) REFERENCES Tags(TagID)
                    );
                
                    CREATE TABLE IF NOT EXISTS VideoTags (
                        VideoId INTEGER,
                        TagId INTEGER,
                        PRIMARY KEY (VideoId, TagId)
                    );

                   CREATE TABLE IF NOT EXISTS Artists (
                        ArtistID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ArtistName TEXT NOT NULL,
                        IsFavorite BOOLEAN DEFAULT 0,
                        IconPath TEXT
                    );

                    CREATE TABLE IF NOT EXISTS VideoArtists (
                        VideoId INTEGER NOT NULL,
                        ArtistID INTEGER NOT NULL,
                        PRIMARY KEY (VideoId, ArtistID)                      
                    );
                ";
                command.ExecuteNonQuery();
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"Error initializing database: {ex.Message}" );
            }
        }

        // ファイル情報を変換後の形式でDBに保存する
        public void SaveFiles( List<NewFile> files ) {
            Console.WriteLine( "Writing file data to destination DB..." );

            try {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                // パフォーマンス向上のためトランザクションを開始
                using var transaction = connection.BeginTransaction();

                totalItems = files.Count;
                lastPercentage = -1;
                int processedItems = 0;
                foreach ( var video in files ) {
                    // 進捗状況を表示
                    DisplayProgress( ++processedItems );

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT OR IGNORE INTO Videos 
                        (FilePath, FileName, FileNameWithoutArtist, Extension, FileSize, LastModified, Duration, LikeCount, ViewCount) 
                        VALUES 
                        ($filePath, $fileName, $fileNameWithiutArtist, $extension, $fileSize, $lastModified, $duration, $like, $view)";


                    command.Parameters.AddWithValue( "$filePath", video.FilePath );
                    command.Parameters.AddWithValue( "$fileName", video.FileName );
                    command.Parameters.AddWithValue( "$fileNameWithiutArtist", video.FileNameWithoutArtist );
                    command.Parameters.AddWithValue( "$extension", video.Extension ?? string.Empty );
                    command.Parameters.AddWithValue( "$fileSize", video.FileSize );
                    // 日付は環境に依存しないISO 8601形式("o")で保存する
                    command.Parameters.AddWithValue( "$lastModified", video.LastModified.ToString( "o" ) );
                    command.Parameters.AddWithValue( "$duration", video.Duration );
                    command.Parameters.AddWithValue( "$like", video.LikeCount );
                    command.Parameters.AddWithValue( "$view", video.ViewCount );

                    // コマンドを実行
                    var rowsAffected = command.ExecuteNonQuery();

                    // DB更新後のIDを取得
                    command.CommandText = "SELECT last_insert_rowid()";
                    command.Parameters.Clear();
                    video.UpdatedId = Convert.ToInt32( command.ExecuteScalar() );
                }
                // トランザクションをコミット
                transaction.Commit();
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"\nError writing file data to destination DB: {ex.Message}" );
            }
        }

        // タグ情報を変換後の形式でDBに保存する
        public void SaveTags( List<NewTag> tags ) {
            Console.WriteLine( "Writing tag data to destination DB..." );

            try {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                totalItems = tags.Count;
                lastPercentage = -1;
                int processedItems = 0;

                foreach (var tag in tags) {
                    // 進捗状況を表示
                    DisplayProgress( ++processedItems );

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT OR IGNORE INTO Tags 
                        (TagID, TagName, TagColor, Parent, OrderInGroup, IsGroup, IsExpand ) 
                        VALUES 
                        ($tagId, $tagName, $tagColor, $parent, $orderInGroup, $isGroup, $isExpand)";

                    command.Parameters.AddWithValue("$tagId", tag.TagId);
                    command.Parameters.AddWithValue("$tagName", tag.TagName);
                    command.Parameters.AddWithValue("$tagColor", tag.TagColor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$parent", tag.Parent ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$orderInGroup", tag.OrderInGroup ?? 0);
                    command.Parameters.AddWithValue("$isGroup", tag.IsGroup ?? 0);
                    command.Parameters.AddWithValue("$isExpand", tag.IsExpanded ?? 0);
                    tag.UpdatedId = tag.TagId;

                    // コマンドを実行
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"\nError writing tag data to destination DB: {ex.Message}");
            }
        }

        // ビデオとタグの関連情報をDBに保存する
        public void SaveVideoTags( List<NewVideoTag> videoTags, List<NewFile> files, List<NewTag> tags ) {
            Console.WriteLine( "Writing video tag data to destination DB..." );
            try {
                // ファイルとタグの古いIDと新しいIDの対応表を作成
                var fileIdMap = files.ToDictionary( f => f.Id, f => f.UpdatedId );

                // TagIdが重複する可能性があるため、安全にDictionaryを作成する
                var tagIdMap = new Dictionary<int, int>();
                foreach (var tag in tags)
                {
                    tagIdMap.TryAdd(tag.TagId, tag.UpdatedId);
                }

                using var connection = new SqliteConnection( $"Data Source={_dbPath}" );
                connection.Open();

                // パフォーマンス向上のためトランザクションを開始
                using var transaction = connection.BeginTransaction();

                totalItems = videoTags.Count;
                lastPercentage = -1;
                int processedItems = 0;

                var command = connection.CreateCommand();

                foreach ( var videoTag in videoTags ) {
                    // 進捗状況を表示
                    DisplayProgress( ++processedItems );

                    // 新しいIDを取得
                    if ( fileIdMap.TryGetValue( videoTag.VideoId, out int newFileId ) &&
                         tagIdMap.TryGetValue( videoTag.TagId, out int newTagId ) ) {

                        command.CommandText = @"
                        INSERT OR IGNORE INTO VideoTags (VideoId, TagId) 
                        VALUES ($videoId, $tagId)";

                        command.Parameters.Clear();
                        command.Parameters.AddWithValue( "$videoId", newFileId );
                        command.Parameters.AddWithValue( "$tagId", newTagId );

                        command.ExecuteNonQuery();
                    }
                }
                // トランザクションをコミット
                transaction.Commit();
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"\nError writing video tag data to destination DB: {ex.Message}" );
            }
        }

        // アーティスト情報をDBに保存する
        public void SaveArtists( List<NewArtist> artists ) {
            Console.WriteLine( "Writing artist data to destination DB..." );
            try {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                // パフォーマンス向上のためトランザクションを開始
                using var transaction = connection.BeginTransaction();
                totalItems = artists.Count;
                lastPercentage = -1;
                int processedItems = 0;
                foreach ( var artist in artists ) {
                    // 進捗状況を表示
                    DisplayProgress( ++processedItems );
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT OR IGNORE INTO Artists 
                        (ArtistID, ArtistName, IsFavorite, IconPath) 
                        VALUES 
                        ($artistId, $artistName, $isFavorite, $iconPath)";
                    command.Parameters.AddWithValue( "$artistId", artist.Id );
                    command.Parameters.AddWithValue( "$artistName", artist.Name );
                    command.Parameters.AddWithValue( "$isFavorite", artist.IsFavorite ? 1 : 0 );
                    command.Parameters.AddWithValue( "$iconPath", artist.IconPath ?? string.Empty );
                    // コマンドを実行
                    command.ExecuteNonQuery();

                    //// 更新後のIDを設定
                    //command.CommandText = "SELECT last_insert_rowid()";
                    //command.Parameters.Clear();
                    //video.UpdatedId = Convert.ToInt32( command.ExecuteScalar() );
                }
                // トランザクションをコミット
                transaction.Commit();
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"\nError writing artist data to destination DB: {ex.Message}" );
            }
        }

        // ビデオとアーティストの関連情報をDBに保存する
        public void SaveVideoArtists( List<NewArtist> artists ) {
            Console.WriteLine( "Writing video artist data to destination DB..." );
            try {
                using var connection = new SqliteConnection( $"Data Source={_dbPath}" );
                connection.Open();
                // パフォーマンス向上のためトランザクションを開始
                using var transaction = connection.BeginTransaction();
                totalItems = artists.Count;
                lastPercentage = -1;
                int processedItems = 0;
                var command = connection.CreateCommand();
                foreach ( var videoArtist in artists ) {
                    // 進捗状況を表示
                    DisplayProgress( ++processedItems );

                    foreach ( var videoId in videoArtist.VideoIds ) {
                        // 新しいIDを取得
                        if ( videoId.UpdatedId == 0 ) continue; // 更新後のIDがない場合はスキップ
                        command.CommandText = @"
                            INSERT OR IGNORE INTO VideoArtists (VideoId, ArtistID) 
                            VALUES ($videoId, $artistId)";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue( "$videoId", videoId.UpdatedId );
                        command.Parameters.AddWithValue( "$artistId", videoArtist.Id );
                        command.ExecuteNonQuery();
                    }
                }
                // トランザクションをコミット
                transaction.Commit();
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"\nError writing video artist data to destination DB: {ex.Message}" );
            }
        }

        // DB保存の進捗状況を表示する
        private void DisplayProgress( int processed ) {
            int currentPercentage = (int)((double)processed / totalItems * 100);

            if ( currentPercentage != lastPercentage ) {
                Console.Write( $"\rProgress: {currentPercentage}% ({processed}/{totalItems})" );
                
                lastPercentage = currentPercentage;
            }

            if ( processed == totalItems ) {
                Console.Write( "\n" );
            }
        }
    }
}
