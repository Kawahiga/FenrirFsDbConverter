using FenrirFsDbConverter.ModelFenrirFSDB;
using FenrirFsDbConverter.ModelNewDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// データ変換担当

namespace FenrirFsDbConverter {
    
    internal class DataConverter {

        private readonly string _hardCodePath;

        public DataConverter( string hardCodePath ) {
            _hardCodePath = hardCodePath;
        }

        // FenrirFSのファイルデータを新しいアプリケーションの形式に変換する
        public List<NewFile> ConvertFiles( List<FenrirFile> fenrirFiles, List<FenrirLabeledfiles> videoTags ) {
            Console.WriteLine( "Converting file data..." );
            var newFiles = new List<NewFile>();

            var likedFileIds = new HashSet<int>(videoTags.Where( vt => vt.LabelID == 16 && vt.FileId.HasValue ).Select( vt => vt.FileId.Value ));

            foreach ( var fenlirFile in fenrirFiles ) {

                var id = fenlirFile.FileID; // データベースの主キー

                var fileName = fenlirFile.DisplayFileName;
                // ファイル名が空の場合はスキップ
                if ( string.IsNullOrEmpty( fileName ) ) continue;

                var filePath = fenlirFile.AliasTarget;
                // ファイルパスが空の場合はハードコード
                if ( string.IsNullOrEmpty( filePath ) ) filePath = string.Concat( _hardCodePath, fileName );
                var fileSize = fenlirFile.FileSize ?? 0; // ファイルサイズがnullの場合は0とする
                // ファイルの書き込み日時をISO 8601形式で保存するため、DateTimeに変換
                var dateTimeString = $"{fenlirFile.LastWriteDate}T{fenlirFile.LastWriteTime}";
                var lastModified = DateTime.Parse( dateTimeString );
                double duration = fenlirFile.MediaDuration * 0.0000001 ?? 0.0; // メディアの再生時間がnullの場合は0とする
                string extension = fenlirFile.Extension?.ToLower() ?? string.Empty; // 拡張子がnullの場合は空文字列とする
                int likeCount = likedFileIds.Contains( id ) ? 1 : 0;    // いいねの数。likedFileIdsに含まれていれば1、そうでなければ0

                var file = new NewFile
                {
                    Id = id,
                    FileName = fileName,
                    FilePath = filePath,
                    FileSize = fileSize,
                    LastModified = lastModified,
                    Duration = duration,
                    Extension = extension,
                    LikeCount = likeCount,
                    ViewCount = 0, // 再生回数は初期値として0を設定
                };

                newFiles.Add( file );
            }
            return newFiles;
        }

        // FenrirFSのラベル情報を新しいアプリケーションのタグ形式に変換する
        public List<NewTag> ConvertTags( List<FenrirLabel> labels, List<FenrirLabelGroup> labelGroups ) {
            Console.WriteLine( "Converting tag data..." );

            var newTags = new List<NewTag>();
            var oldIdToNewIdMap = new Dictionary<int, int>();

            // 1. IDの開始番号を決定
            var nextId = ( labels.Any() ? labels.Max( l => l.LabelID ?? 0 ) : 0 ) + 1;

            // 2. 固定タグを作成
            var allFilesTagId = nextId++;
            var untaggedTagId = nextId++;
            
            newTags.Add( new NewTag { TagId = allFilesTagId, TagName = "全てのファイル", Parent = null, OrderInGroup = 0, IsGroup = 1, IsExpanded = 1, TagColor = "#FFFFFF", UpdatedId = 0 } );
            newTags.Add( new NewTag { TagId = untaggedTagId, TagName = "タグなし", Parent = allFilesTagId, OrderInGroup = 0, IsGroup = 1, IsExpanded = 1, TagColor = "#FFFFFF", UpdatedId = 0 } );

            // 3. 「グループなし」のIDを取得
            var groupLessId = labelGroups.FirstOrDefault( g => g.LabelGroupName == "グループなし" )?.LabelGroupID;

            // 4. 元のDBのラベルグループを「全てのファイル」配下のタグとして変換
            foreach ( var group in labelGroups ) {
                if ( !group.LabelGroupID.HasValue || group.LabelGroupID == groupLessId ) continue;

                var newTag = new NewTag {
                    TagId = nextId,
                    TagName = group.LabelGroupName,
                    TagColor = "#000000",
                    Parent = group.ParentGroupId, // ここを修正
                    OrderInGroup = group.OrderInList,
                    IsExpanded = 1 - ( group.Folded ?? 0 ),
                    IsGroup = 1,
                    UpdatedId = 0,
                };
                oldIdToNewIdMap[group.LabelGroupID.Value] = nextId;
                newTags.Add( newTag );
                nextId++;
            }

            // 4.5. グループの親子関係を解決 (追加)
            foreach ( var tag in newTags ) {
                // 「全てのファイル」タグ自身は親を持たない
                if ( tag.TagId == allFilesTagId ) continue; 

                if ( tag.IsGroup == 1 && tag.Parent.HasValue && oldIdToNewIdMap.ContainsKey( tag.Parent.Value ) ) {
                    tag.Parent = oldIdToNewIdMap[tag.Parent.Value];
                } else if ( tag.IsGroup == 1 && tag.Parent.HasValue && tag.Parent.Value == 1 ) { // ここを修正
                    // 親がいないグループは「全てのファイル」の直下にする
                    tag.Parent = allFilesTagId;
                }
            }

            // 5. 元のDBのラベルを変換
            foreach ( var label in labels ) {
                int? parentId;
                // ラベルが「グループなし」に所属していた場合、親を「全てのファイル」にする
                if ( groupLessId.HasValue && label.GroupId.HasValue && label.GroupId.Value == groupLessId.Value ) {
                    parentId = allFilesTagId;
                } else {
                    // それ以外のラベルは、元の親グループIDを保持（後で解決）
                    parentId = label.GroupId;
                }

                var labelColor = ConvertColorNameToHex( label.LabelColorName );
                var newTag = new NewTag {
                    TagId = label.LabelID ?? 0,
                    TagName = label.LabelName,
                    TagColor = labelColor,
                    Parent = parentId,
                    OrderInGroup = label.OrderInGroup,
                    IsExpanded = 1,
                    IsGroup = 0,
                    UpdatedId = 0,
                };
                newTags.Add( newTag );
            }

            // 6. ラベルの親子関係を解決
            // この時点で、グループの親は解決済み。ラベルの親（古いグループID）を新しいIDに置換する。
            foreach ( var tag in newTags ) {
                if ( tag.IsGroup == 0 && tag.Parent.HasValue && oldIdToNewIdMap.ContainsKey( tag.Parent.Value ) ) {
                    tag.Parent = oldIdToNewIdMap[tag.Parent.Value];
                }
            }

            // 7. グループ内の表示順を再設定
            var groupedByParent = newTags.GroupBy( t => t.Parent );
            foreach ( var group in groupedByParent ) {
                var orderedTags = group.OrderByDescending( t => t.IsGroup )
                                         .ThenBy( t => t.OrderInGroup )
                                         .ToList();

                for ( int i = 0; i < orderedTags.Count; i++ ) {
                    orderedTags[i].OrderInGroup = i;
                }
            }

            return newTags;
        }

        // FenrirFSの動画とラベルの紐づけ情報を新しいアプリケーションの形式に変換する
        public List<NewVideoTag> ConvertVideoTags( List<FenrirLabeledfiles> videoTags ) {
            Console.WriteLine( "Converting videoTags data..." );

            var newRules = new List<NewVideoTag>();

            foreach ( var rule in videoTags ) {

                var videoId = rule.FileId ?? 0; // ファイルIDがnullの場合は0とする
                var tagId = rule.LabelID ?? 0; // ラベルIDがnullの場合は0とする

                var newRule = new NewVideoTag
                {
                    VideoId = videoId,
                    TagId = tagId,
                };

                newRules.Add( newRule );
            }
            return newRules;
        }

        // FenrirFSのファイルの自動振り分け設定を新しいアプリケーションの形式に変換する
        public List<NewFilter> ConvertFilters( List<FenrirFilter> filters ) {
            // 具体的に何が必要か不明なため、仮の実装
            return new List<NewFilter>();
        }

        // ラベル色文字列をカラーコードに変換する
        private string ConvertColorNameToHex( string? colorName ) {
            return colorName switch {
                "Red" => "#FF0000",
                "Green" => "#00FF00",
                "Blue" => "#0000FF",
                "Yellow" => "#FFFF00",
                "Lime" => "#00FF00",
                "Pink" => "#FFC0CB",
                "Purple" => "#800080",
                "Magenta" => "#FF00FF",
                "Aqua" => "#00FFFF",
                "Silver" => "#C0C0C0",
                "Orange" => "#FFA500",
                "Black" => "#000000",
                "Brown" => "#A52A2A",
                "Olive" => "#808000",
                "SkyBlue" => "#87CEEB",
                _ => "#000000", // デフォルトは黒
            };
        }
    }
}
