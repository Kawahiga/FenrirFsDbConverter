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
        private SqliteConnection? connection;
        private int totalFiles;
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
                CREATE TABLE IF NOT EXISTS Videos (
                    FileID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FilePath TEXT NOT NULL UNIQUE,
                    FileName TEXT NOT NULL,
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
            // エンハンス案：関連づいたIDが削除された場合に、関連する行も削除するためのON DELETE CASCADEを設定
            // FOREIGN KEY (VideoId) REFERENCES Videos(FileID) ON DELETE CASCADE,
            // FOREIGN KEY(TagId) REFERENCES Tags(TagID) ON DELETE CASCADE,

            command.ExecuteNonQuery();
        }


        // / ファイル情報を変換後の形式でDBに保存する
        public void SaveFiles( List<NewFile> files ) {
            // TODO: DBに接続し、変換後のファイル情報を書き込む
            Console.WriteLine( "Writing file data to destination DB..." );

            try {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                totalFiles = files.Count;
                lastPercentage = -1;
                int processedFiles = 0;
                foreach ( var video in files ) {
                    var command = connection.CreateCommand();
                    command.CommandText = @"
                        INSERT OR IGNORE INTO Videos 
                        (FilePath, FileName, FileSize, LastModified, Duration) 
                        VALUES 
                        ($filePath, $fileName, $fileSize, $lastModified, $duration)";


                    command.Parameters.AddWithValue( "$filePath", video.FilePath );
                    command.Parameters.AddWithValue( "$fileName", video.FileName );
                    command.Parameters.AddWithValue( "$fileSize", video.FileSize );
                    // 日付は環境に依存しないISO 8601形式("o")で保存する
                    command.Parameters.AddWithValue( "$lastModified", video.LastModified.ToString( "o" ) );
                    command.Parameters.AddWithValue( "$duration", video.Duration );
                    
                    // コマンドを実行
                    command.ExecuteNonQuery();

                    // 進捗状況を表示
                    DisplayProgress( ++processedFiles);
                }
                Console.WriteLine("File data written successfully.");
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"Error reading files from FenrirFS DB: {ex.Message}" );
            }
        }

        // ラベル情報を変換後の形式でDBに保存する
        public void SaveLabels( List<object> labels ) {
            // TODO: DBに接続し、変換後のカテゴリ情報を書き込む
            Console.WriteLine( "Writing label data to destination DB..." );
        }

        // DB保存の進捗状況を表示する
        private void DisplayProgress( int processed ) {
            int currentPercentage = (int)((double)processed / totalFiles * 100);

            if ( processed == 0 ) {
                Console.Write( "\n" );
            }

            if ( currentPercentage != lastPercentage ) {
                Console.Write( $"\rProgress: {currentPercentage}% ({processed}/{totalFiles})" );
                lastPercentage = currentPercentage;
            }

            if ( processed == totalFiles ) {
                Console.Write( "\n" );
            }
        }
    }
}
