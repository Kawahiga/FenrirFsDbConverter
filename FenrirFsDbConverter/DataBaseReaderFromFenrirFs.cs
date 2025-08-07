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

        // ファイル情報を読み込む
        public List<FenrirFile> ReadFiles() {

            Console.WriteLine( "Reading files from FenrirFS DB..." );

            var videos = new List<FenrirFile>();

            try {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT FileID, AliasTarget, DisplayFileName, FileSize, LastWriteDate, LastWriteTime, MediaDuration, Extension
                    FROM files";

                using var reader = command.ExecuteReader();
                while ( reader.Read() ) {
                    var id = reader.GetInt32(0);
                    var aliasTarget = reader.GetString(1);
                    var displayFileName = reader.GetString(2);
                    var fileSize = reader.GetInt64(3);
                    var lastModifiedDate = reader.GetString(4);
                    var lastModifiedTime = reader.GetString(5);
                    var mediaDuration = reader.GetInt64(6);
                    var extension = reader.GetString(7);

                    var video = new FenrirFile {
                        FileID = id,
                        DisplayFileName = displayFileName,
                        AliasTarget = aliasTarget,
                        FileSize = fileSize,
                        LastWriteDate = lastModifiedDate,
                        LastWriteTime = lastModifiedTime,
                        MediaDuration = mediaDuration,
                        Extension = extension
                    };
                    videos.Add( video );
                }
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"Error reading files from FenrirFS DB: {ex.Message}" );
            }
            return videos;
        }

        // ラベル情報を読み込む
        public List<FenrirLabel> ReadLabels() {
            Console.WriteLine( "Reading labels from FenrirFS DB..." );

            var labels = new List<FenrirLabel>();

            try {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT LabelID, LabelName, LabelColorName, GroupId, OrderInGroup
                    FROM labels";

                using var reader = command.ExecuteReader();
                while ( reader.Read() ) {
                    var id = reader.GetInt32(0);
                    var labelName = reader.GetString(1);
                    var labelColorName = reader.GetString(2);
                    var groupId = reader.GetInt32(3);
                    var orderInGroup = reader.GetInt32(4);

                    var label = new FenrirLabel {
                        LabelID = id,
                        LabelName = labelName,
                        LabelColorName = labelColorName,
                        GroupId = groupId,
                        OrderInGroup = orderInGroup
                    };
                    labels.Add( label );
                }
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"Error reading labels from FenrirFS DB: {ex.Message}" );
            }
            return labels;
        }

        // ラベルグループ情報を読み込む
        public List<FenrirLabelGroup> ReadLabelGroups() {
            Console.WriteLine( "Reading label groups from FenrirFS DB..." );

            var labelGroups = new List<FenrirLabelGroup>();

            try {
                using var connection = new SqliteConnection($"Data Source={_dbPath}");
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT LabelGroupID, LabelGroupName, OrderInList, ParentGroupId
                    FROM labelgroup";

                using var reader = command.ExecuteReader();
                while ( reader.Read() ) {
                    var labelGroup = new FenrirLabelGroup {
                        LabelGroupID = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                        LabelGroupName = reader.IsDBNull(1) ? null : reader.GetString(1),
                        OrderInList = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                        ParentGroupId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3)
                    };
                    labelGroups.Add( labelGroup );
                }
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"Error reading label groups from FenrirFS DB: {ex.Message}" );
            }
            return labelGroups;
        }
    }
}
