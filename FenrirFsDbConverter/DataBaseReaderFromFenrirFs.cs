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

        // ファイルとタグの関連付け情報を読み込む
        public List<FenrirLabeledfiles> ReadLabeledFiles() {
            Console.WriteLine( "Reading labeled files from FenrirFS DB..." );

            var labeledFiles = new List<FenrirLabeledfiles>();

            try {
                using var connection = new SqliteConnection( $"Data Source={_dbPath}" );
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT LabeledFileID, LabelID, FileId
                    FROM labeledfiles";

                using var reader = command.ExecuteReader();
                while ( reader.Read() ) {
                    var labeledFile = new FenrirLabeledfiles {
                        LabeledFileID = reader.GetInt32( 0 ),
                        LabelID = reader.GetInt32( 1 ),
                        FileId = reader.GetInt32( 2 ),
                    };
                    labeledFiles.Add( labeledFile );
                }
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"Error reading labeled files from FenrirFS DB: {ex.Message}" );
            }
            return labeledFiles;
        }

        // ファイルの自動振り分け設定を読み込む
        public List<FenrirFilter> ReadFilters() {
            Console.WriteLine( "Reading filters from FenrirFS DB..." );

            var filters = new List<FenrirFilter>();

            try {
                using var connection = new SqliteConnection( $"Data Source={_dbPath}" );
                connection.Open();

                var command = connection.CreateCommand();
                // どの項目が必要か精査できていない
                command.CommandText = @"
                    SELECT FilterId, Enabled, FileName, FileNameCompType, Extension, ExtensionCompType, Comment, CommentCompType, FileSizeEnabled, MinFileSize, MaxFileSize, DateEnabled, StartDate, EndDate, SkipTray, SetStar, LabelEnabled, LabelId, DoDelete, ImmediateApply, FilterOrder
                    FROM filter";

                using var reader = command.ExecuteReader();
                while ( reader.Read() ) {
                    var filter = new FenrirFilter
                    {
                        FilterId = reader.IsDBNull( 0 ) ? (int?)null : reader.GetInt32( 0 ),
                        Enabled = reader.IsDBNull( 1 ) ? (int?)null : reader.GetInt32( 1 ),
                        FileName = reader.IsDBNull( 2 ) ? null : reader.GetString( 2 ),
                        FileNameCompType = reader.IsDBNull( 3 ) ? (int?)null : reader.GetInt32( 3 ),
                        Extension = reader.IsDBNull( 4 ) ? null : reader.GetString( 4 ),
                        ExtensionCompType = reader.IsDBNull( 5 ) ? (int?)null : reader.GetInt32( 5 ),
                        Comment = reader.IsDBNull( 6 ) ? null : reader.GetString( 6 ),
                        CommentCompType = reader.IsDBNull( 7 ) ? (int?)null : reader.GetInt32( 7 ),
                        FileSizeEnabled = reader.IsDBNull( 8 ) ? (int?)null : reader.GetInt32( 8 ),
                        MinFileSize = reader.IsDBNull( 9 ) ? (int?)null : reader.GetInt32( 9 ),
                        MaxFileSize = reader.IsDBNull( 10 ) ? (int?)null : reader.GetInt32( 10 ),
                        DateEnabled = reader.IsDBNull( 11 ) ? (int?)null : reader.GetInt32( 11 ),
                        StartDate = reader.IsDBNull( 12 ) ? null : reader.GetString( 12 ),
                        EndDate = reader.IsDBNull( 13 ) ? null : reader.GetString( 13 ),
                        SkipTray = reader.IsDBNull( 14 ) ? (int?)null : reader.GetInt32( 14 ),
                        SetStar = reader.IsDBNull( 15 ) ? (int?)null : reader.GetInt32( 15 ),
                        LabelEnabled = reader.IsDBNull( 16 ) ? (int?)null : reader.GetInt32( 16 ),
                        LabelId = reader.IsDBNull( 17 ) ? (int?)null : reader.GetInt32( 17 ),
                        DoDelete = reader.IsDBNull( 18 ) ? (int?)null : reader.GetInt32( 18 ),
                        ImmediateApply = reader.IsDBNull( 19 ) ? (int?)null : reader.GetInt32( 19 ),
                        FilterOrder = reader.IsDBNull( 20 ) ? (int?)null : reader.GetInt32( 20 )
                    };
                    filters.Add( filter );
                }
            }
            catch ( Exception ex ) {
                Console.WriteLine( $"Error reading filters from FenrirFS DB: {ex.Message}" );
            }
            return filters;
        }
    }
}
