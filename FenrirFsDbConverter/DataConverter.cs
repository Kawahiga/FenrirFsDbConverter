using FenrirFsDbConverter.ModelFenrirFSDB;
using FenrirFsDbConverter.ModelNewDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FenrirFsDbConverter {
    // データ変換担当
    internal class DataConverter {

        private readonly string _hardCodePath;

        public DataConverter( string hardCodePath ) {
            _hardCodePath = hardCodePath;
        }

        // FenrirFSのデータを新しいアプリケーションの形式に変換する
        public List<NewFile> ConvertFiles( List<FenrirFile> fenrirFiles ) {
            // TODO: FenrirFileのリストをNewAppFileのリストに変換するロジックを実装
            Console.WriteLine( "Converting file data..." );
            var newFiles = new List<NewFile>();

            foreach ( var fenlirFile in fenrirFiles ) {

                var id = fenlirFile.FileID; // データベースの主キー

                var fileName = fenlirFile.DisplayFileName;
                // ファイル名が空の場合はスキップ
                if ( string.IsNullOrEmpty( fileName ) )
                    continue;

                var filePath = fenlirFile.AliasTarget;
                // ファイルパスが空の場合はハードコード
                if ( string.IsNullOrEmpty( filePath ) )
                    filePath = string.Concat( _hardCodePath, fileName );

                var fileSize = fenlirFile.FileSize ?? 0; // ファイルサイズがnullの場合は0とする

                // ファイルの書き込み日時をISO 8601形式で保存するため、DateTimeに変換
                var dateTimeString = $"{fenlirFile.LastWriteDate}T{fenlirFile.LastWriteTime}";
                var lastModified = DateTime.Parse( dateTimeString );

                double duration = fenlirFile.MediaDuration * 0.0000001 ?? 0.0; // メディアの再生時間がnullの場合は0とする

                string extension = fenlirFile.Extension?.ToLower() ?? string.Empty; // 拡張子がnullの場合は空文字列とする

                var file = new NewFile
                {
                    Id = id,
                    FileName = fileName,
                    FilePath = filePath,
                    FileSize = fileSize,
                    LastModified = lastModified,
                    Duration = duration,
                    Extension = extension,
                };

                newFiles.Add( file );
            }
            return newFiles;
        }

        // FenrirFSのラベル情報を新しいアプリケーションのタグ形式に変換する
        public List<NewTag> ConvertTags( List<FenrirLabel> labels, List<FenrirLabelGroup> labelGroups ) {
            Console.WriteLine( "Converting tag data..." );

            var newTags = new List<NewTag>();
            var groupMap = new Dictionary<int, NewTag>();

            // 1. グループを変換
            //    まず、すべてのグループをNewTagオブジェクトに変換し、古いIDをキーにして辞書に保存します。
            //    この時点では、親子関係は古いIDのままです。
            foreach ( var group in labelGroups ) {
                if ( !group.LabelGroupID.HasValue )
                    continue;

                var newTag = new NewTag {
                    TagName = group.LabelGroupName,
                    IsGroup = 1,
                    IsExpanded = 1 - ( group.Folded ?? 0 ),
                    OrderInGroup = group.OrderInList,
                    Parent = group.ParentGroupId // 親IDを一時的に保持
                };
                groupMap.Add( group.LabelGroupID.Value, newTag );
                newTags.Add( newTag );
            }

            // 2. ラベルを変換
            //    次に、すべてのラベルをNewTagオブジェクトに変換します。
            foreach ( var label in labels ) {
                var newTag = new NewTag {
                    TagName = label.LabelName,
                    TagColor = label.LabelColorName,
                    IsGroup = 0,
                    OrderInGroup = label.OrderInGroup,
                    IsExpanded = 1, // ラベルは常に展開状態
                    Parent = label.GroupId // 親グループのIDを一時的に保持
                };
                newTags.Add( newTag );
            }

            // 3. 新しいIDを採番し、親子関係を更新
            //    すべてのタグにユニークな新しいIDを割り当てます。
            //    同時に、古いIDで保持していた親子関係を新しいIDに解決します。
            var nextId = 1;
            var oldIdToNewIdMap = new Dictionary<int, int>();

            // まず、グループのIDを確定させます
            foreach ( var oldId in groupMap.Keys ) {
                oldIdToNewIdMap[oldId] = nextId;
                groupMap[oldId].TagId = nextId;
                nextId++;
            }

            // すべてのタグ（グループとラベル）の親子関係を更新します
            foreach ( var tag in newTags ) {
                // IDがまだ割り当てられていない場合は、新しいIDを割り当てます（ラベルの場合）
                if ( tag.TagId == null ) {
                    tag.TagId = nextId;
                    nextId++;
                }

                // 親IDが設定されている場合、新しいIDに変換します
                if ( tag.Parent.HasValue && oldIdToNewIdMap.ContainsKey( tag.Parent.Value ) ) {
                    tag.Parent = oldIdToNewIdMap[tag.Parent.Value];
                } else {
                    // 対応する親が見つからない場合は、親なしとします
                    tag.Parent = null;
                }
            }

            return newTags;
        }
    }
}
