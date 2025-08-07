using FenrirFsDbConverter.ModelFenrirFSDB;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FenrirFsDbConverter {
    // データ読み込み担当
    internal class DataBaseReaderFromFenrirFs {

        private readonly string _dbPath;

        public DataBaseReaderFromFenrirFs( string dbPath ) {
            _dbPath = dbPath;
        }

        // FenrirFSのDBからファイル情報を読み込む
        public List<FenrirFile> ReadFiles() {

            Console.WriteLine( "Reading files from FenrirFS DB..." );

            try {
                var videos = new List<FenrirFile>();
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT FileID, AliasTarget, DisplayFileName, FileSize, LastWriteDate, LastWriteTime, MediaDuration
                    FROM files";

                using var reader = command.ExecuteReader();
                while ( reader.Read() ) {
                    var id = reader.GetInt32(0);
                    var aliasTarget = reader.GetString(1);
                    var displayFileName = reader.GetString(2);
                    var fileSize = reader.GetInt64(3);
                    // ISO 8601形式で保存した日付を正しく読み込むため、スタイルを指定します
                    //var lastModified = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind);
                    var lastModifiedDate = reader.GetString(4);
                    var lastModifiedTime = reader.GetString(5);
                    var mediaDuration = reader.GetInt32(6);

                    var video = new FenrirFile
                    {
                        FileID = id,
                        DisplayFileName = displayFileName,
                        AliasTarget = aliasTarget,
                        FileSize = fileSize,
                        LastWriteDate = lastModifiedDate,
                        LastWriteTime = lastModifiedTime,
                        MediaDuration = mediaDuration,
                    };
                    videos.Add( video );
                }
                return videos;
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"Error reading files from FenrirFS DB: {ex.Message}" );
                return new List<FenrirFile>();
            }
        }


        // ラベル情報を読み込む
        //public List<FenrirTag> ReadAllTags() {
        //    // TODO: DBに接続し、タグ情報を読み込んでFenrirTagのリストを返す
        //    Console.WriteLine( "Reading tags from FenrirFS DB..." );
        //    return new List<FenrirTag>();
        //}
    }
}
