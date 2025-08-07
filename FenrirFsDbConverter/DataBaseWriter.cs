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

            var command = connection.CreateCommand();

            command.CommandText =
            @"
                DROP TABLE IF EXISTS Videos;
                DROP TABLE IF EXISTS Tags;
                DROP TABLE IF EXISTS VideoTags;

                CREATE TABLE IF NOT EXISTS Videos (
                    FileID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FilePath TEXT NOT NULL UNIQUE,
                    FileName TEXT NOT NULL,
                    Extension TEXT DEFAULT '',
                    FileSize INTEGER DEFAULT 0,
                    LastModified TEXT,
                    Duration REAL DeFAULT 0.0
                );
                
                CREATE TABLE IF NOT EXISTS Tags (
                    TagID INTEGER PRIMARY KEY AUTOINCREMENT,
                    TagName TEXT NOT NULL UNIQUE,
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
                    FOREIGN KEY (VideoId) REFERENCES Videos(FileID),
                    FOREIGN KEY (TagId) REFERENCES Tags(TagID),
                    PRIMARY KEY (VideoId, TagId)
                );
            ";
            command.ExecuteNonQuery();
        }

        // ファイル情報を変換後の形式でDBに保存する
        public void SaveFiles( List<NewFile> files ) {
            Console.WriteLine( "Writing file data to destination DB..." );

            try {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                totalItems = files.Count;
                lastPercentage = -1;
                int processedItems = 0;
                foreach ( var video in files ) {
                    // 進捗状況を表示
                    DisplayProgress( ++processedItems );

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT OR IGNORE INTO Videos 
                        (FilePath, FileName, Extension, FileSize, LastModified, Duration) 
                        VALUES 
                        ($filePath, $fileName, $extension, $fileSize, $lastModified, $duration)";


                    command.Parameters.AddWithValue( "$filePath", video.FilePath );
                    command.Parameters.AddWithValue( "$fileName", video.FileName );
                    command.Parameters.AddWithValue( "$extension", video.Extension ?? string.Empty );
                    command.Parameters.AddWithValue( "$fileSize", video.FileSize );
                    // 日付は環境に依存しないISO 8601形式("o")で保存する
                    command.Parameters.AddWithValue( "$lastModified", video.LastModified.ToString( "o" ) );
                    command.Parameters.AddWithValue( "$duration", video.Duration );

                    // コマンドを実行
                    var rowsAffected = command.ExecuteNonQuery();

                    // DB更新後のIDを取得
                    if ( rowsAffected > 0 ) {
                        // 行が新規に挿入された
                        command.CommandText = "SELECT last_insert_rowid()";
                        command.Parameters.Clear();
                        video.UpdatedId = Convert.ToInt32( command.ExecuteScalar() );
                    } else {
                        // 行が新規に挿入されなかった
                        command.CommandText = "SELECT FileID FROM Videos WHERE FilePath = $filePath";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue( "$filePath", video.FilePath );
                        video.UpdatedId = Convert.ToInt32( command.ExecuteScalar() );
                    }
                }
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
                        (TagName, TagColor, Parent, OrderInGroup, IsGroup, IsExpand ) 
                        VALUES 
                        ($tagName, $tagColor, $parent, $orderInGroup, $isGroup, $isExpand)";

                    command.Parameters.AddWithValue("$tagName", tag.TagName);
                    command.Parameters.AddWithValue("$tagColor", tag.TagColor ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$parent", tag.Parent ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("$orderInGroup", tag.OrderInGroup ?? 0);
                    command.Parameters.AddWithValue("$isGroup", tag.IsGroup ?? 0);
                    command.Parameters.AddWithValue("$isExpand", tag.IsExpanded ?? 0);

                    // コマンドを実行
                    var rowsAffected = command.ExecuteNonQuery();

                    // DB更新後のIDを取得
                    if ( rowsAffected > 0 ) {
                        // 行が新規に挿入された
                        command.CommandText = "SELECT last_insert_rowid()";
                        command.Parameters.Clear();
                        tag.UpdatedId = Convert.ToInt32( command.ExecuteScalar() );
                    } else {
                        // 行が新規に挿入されなかった
                        command.CommandText = "SELECT TagID FROM Tags WHERE TagName = $tagName";
                        command.Parameters.Clear();
                        command.Parameters.AddWithValue( "$tagName", tag.TagName );
                        tag.UpdatedId = Convert.ToInt32( command.ExecuteScalar() );
                    }
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
                // ファイルとタグの新旧IDを対応させる

            }
            catch ( Exception ex ) {
                Console.WriteLine( $"Error writing video tag data to destination DB: {ex.Message}" );
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
