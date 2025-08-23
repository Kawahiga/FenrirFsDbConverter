using FenrirFsDbConverter.ModelFenrirFSDB;
using FenrirFsDbConverter.ModelNewDB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                if ( string.IsNullOrEmpty( fileName ) )
                    continue;
                // アーティスト名を除いたファイル名
                var fileNameWithoutArtist = Regex.Replace( fileName, @"^[\[【].*?[\]】]", "" ).Trim();

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
                int likeCount = likedFileIds.Contains( id ) ? 1 : 0;    // いいねの数。likedFileIdsに含まれていれば1、そうでなければ0

                var file = new NewFile
                {
                    Id = id,
                    FileName = fileName,
                    FileNameWithoutArtist = fileNameWithoutArtist,
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
                if ( !group.LabelGroupID.HasValue || group.LabelGroupID == groupLessId )
                    continue;

                var newTag = new NewTag
                {
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
                if ( tag.TagId == allFilesTagId )
                    continue;

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
                var newTag = new NewTag
                {
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

        // ラベル色文字列をカラーコードに変換する
        private string ConvertColorNameToHex( string? colorName ) {
            return colorName switch
            {
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

        /// <summary>
        /// アーティストのグループ化（同一性の判定）を行うためのデータ構造です。
        /// Union-Find（またはDisjoint Set Union）アルゴリズムを実装しています。
        /// </summary>
        private class UnionFind {
            // 各要素（アーティスト名）の親要素を保持します。
            private readonly Dictionary<string, string> _parent;

            public UnionFind() {
                _parent = new Dictionary<string, string>();
            }

            /// <summary>
            /// 新しい要素をセットに追加します。最初は自分自身を親とします。
            /// </summary>
            public void Add( string name ) {
                if ( !_parent.ContainsKey( name ) ) {
                    _parent[name] = name;
                }
            }

            /// <summary>
            /// 指定した要素の根（グループの代表元）を見つけます。
            /// 途中の要素を根に直接つなぎ直す「経路圧縮」で効率化しています。
            /// </summary>
            public string Find( string name ) {
                if ( !_parent.ContainsKey( name ) ) {
                    Add( name );
                    return name;
                }

                if ( _parent[name] == name ) {
                    return name;
                }

                return _parent[name] = Find( _parent[name] );
            }

            /// <summary>
            /// 2つの要素が含まれるグループを統合します。
            /// 代表元を辞書順で若い方に統一し、動作の安定性を確保します。
            /// </summary>
            public void Union( string name1, string name2 ) {
                string root1 = Find(name1);
                string root2 = Find(name2);
                if ( root1 != root2 ) {
                    if ( string.Compare( root1, root2, StringComparison.Ordinal ) < 0 ) {
                        _parent[root2] = root1;
                    } else {
                        _parent[root1] = root2;
                    }
                }
            }

            /// <summary>
            /// 全てのグループを、代表元をキーとしたメンバーのリストとして取得します。
            /// </summary>
            public Dictionary<string, List<string>> GetGroups() {
                var groups = new Dictionary<string, List<string>>();
                foreach ( var name in _parent.Keys ) {
                    string root = Find(name);
                    if ( !groups.ContainsKey( root ) ) {
                        groups[root] = new List<string>();
                    }
                    groups[root].Add( name );
                }
                return groups;
            }
        }

        public static List<List<string>> ExtractArtistGroupsFromFile(string fileName) {
            var artistGroups = new List<List<string>>();
            if (string.IsNullOrEmpty(fileName)) {
                return artistGroups;
            }

            var match = Regex.Match(fileName, @"^[\[【](.*?)[\]】]");
            if (match.Success) {
                string artistsString = match.Groups[1].Value;
                string pattern = @"\S+(\s*[\(（][^\)）]*[\)）])+|\S+";
                MatchCollection matches = Regex.Matches(artistsString, pattern);
                string[] extractedNames = matches.Cast<Match>().Select(m => m.Value).ToArray();

                foreach (var nameGroup in extractedNames) {
                    var aliasMatch = Regex.Match(nameGroup, @"([^(（]+)[(（]([^)）]+)[)）]");
                    var allNamesInGroup = new List<string>();

                    if (aliasMatch.Success) {
                        string mainName = aliasMatch.Groups[1].Value.Trim();
                        allNamesInGroup.Add(mainName);
                        string[] aliases = aliasMatch.Groups[2].Value.Split(new[] { '、', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var alias in aliases) {
                            allNamesInGroup.Add(alias.Trim());
                        }
                    } else {
                        allNamesInGroup.Add(nameGroup.Trim());
                    }
                    artistGroups.Add(allNamesInGroup);
                }
            }
            return artistGroups;
        }

        /// <summary>
        /// ビデオのリストからアーティスト一覧を作成します。
        /// 別名義を認識し、関連するアーティストをグループ化して表示します。
        /// </summary>
        public List<NewArtist> ConvertArtists( List<NewFile> videos, List<NewTag> tags ) {
            Console.WriteLine( "Converting artist data..." );
            try {
                var newArtists = new List<NewArtist>();
                int nextId = 1; // アーティストIDの開始番号

                // --- パース処理 --- 
                // 1. 全てのアーティスト名（別名含む）と、それに対応するビデオのリストを作成します。
                // 2. 同時に、Union-Find構造を使ってアーティスト間の関連（グループ）を構築します。
                var artistsWithVideos = new Dictionary<string, List<NewFile>>();
                var uf = new UnionFind();

                foreach (var video in videos) {
                    var artistGroups = ExtractArtistGroupsFromFile(video.FileName);
                    foreach (var allNamesInGroup in artistGroups) {
                        // パースした全ての名前をUnion-Find構造に追加し、ビデオと紐付けます。
                        foreach (var name in allNamesInGroup) {
                            uf.Add(name);
                            if (!artistsWithVideos.ContainsKey(name)) {
                                artistsWithVideos[name] = new List<NewFile>();
                            }
                            if (!artistsWithVideos[name].Contains(video)) {
                                artistsWithVideos[name].Add(video);
                            }
                        }

                        // 同じ括弧内に含まれるアーティスト同士を同じグループとして統合します。
                        if (allNamesInGroup.Count > 1) {
                            for (int i = 1; i < allNamesInGroup.Count; i++) {
                                uf.Union(allNamesInGroup[0], allNamesInGroup[i]);
                            }
                        }
                    }
                }

                // --- マージ処理 ---
                // Union-Findで構築したグループ情報に基づき、各アーティストのビデオをグループの代表（root）に集約します。
                var mergedArtists = new Dictionary<string, List<NewFile>>();
                foreach ( var artistName in artistsWithVideos.Keys ) {
                    string root = uf.Find(artistName);
                    if ( !mergedArtists.ContainsKey( root ) ) {
                        mergedArtists[root] = new List<NewFile>();
                    }
                    // ビデオリストをマージ（重複を排除しながら追加）
                    foreach ( var video in artistsWithVideos[artistName] ) {
                        if ( !mergedArtists[root].Contains( video ) ) {
                            mergedArtists[root].Add( video );
                        }
                    }
                }

                // --- ArtistItem作成処理 ---
                // 最終的なアーティストリストをUI向けに作成します。
                var groups = uf.GetGroups();
                // 表示順序：ビデオ数が多い順、次に名前の辞書順
                var sortedRoots = mergedArtists.Keys.OrderByDescending(root => mergedArtists[root].Count).ThenBy(r => r);

                foreach ( var rootName in sortedRoots ) {
                    var groupMembers = groups.ContainsKey(rootName) ? groups[rootName] : new List<string> { rootName };

                    // --- 主名の決定 ---
                    // グループ内で最もビデオ数が多いアーティストを主名（表示名）とします。
                    // ビデオ数が同じ場合は、名前の辞書順で決定します。
                    string displayRoot = groupMembers
                        .OrderByDescending(m => artistsWithVideos.ContainsKey(m) ? artistsWithVideos[m].Count : 0)
                        .ThenBy(m => m, StringComparer.Ordinal)
                        .FirstOrDefault() ?? rootName;

                    var aliases = groupMembers.Where(m => m != displayRoot).OrderBy(a => a).ToList();

                    string displayName = displayRoot;
                    if ( aliases.Any() ) {
                        displayName += $"({string.Join( "、", aliases )})";
                    }

                    // アーティスト名またはその別名義のいずれかがタグと一致すれば、お気に入りを設定します。
                    var iSFavorite = tags.Any(t => t.TagName.Equals(displayRoot, StringComparison.OrdinalIgnoreCase) ||
                                             aliases.Any(alias => alias.Equals(t.TagName, StringComparison.OrdinalIgnoreCase)));

                    var newArtist = new NewArtist
                    {
                        Id = nextId++,
                        Name = displayName,
                        VideoIds = mergedArtists[rootName], // ビデオはグループ（rootName）のものを設定
                        IsFavorite = iSFavorite,
                    };

                    newArtists.Add( newArtist );
                }

                return newArtists;
            } catch ( Exception ex ) {
                // エラーが発生した場合は、アプリがクラッシュしないように例外を捕捉します。
                System.Diagnostics.Debug.WriteLine( $"Error in CreateArtistList: {ex.Message}" );
                return new List<NewArtist>();
            }
        }

        // アーティスト名と一致するタグを持つ動画をアーティストに紐づけます。
        public void LinkVideoArtists( List<NewArtist> artists, List<NewVideoTag> videoTags, List<NewTag> tags, List<NewFile> videos ) {
            Console.WriteLine( "Linking video artists..." );
            // タグ名をキー、タグIDを値とする辞書を作成します。
            var tagNameToId = tags.ToDictionary(t => t.TagName, t => t.TagId);
            // VideoIdをキー、NewFileを値とする辞書を作成して検索を高速化します。
            var videoIdToFile = videos.ToDictionary(v => v.Id, v => v);

            foreach ( var artist in artists ) {
                // アーティスト名とその別名義を分解します。
                var nameMatch = Regex.Match(artist.Name, @"^([^(（]+)([(（](.*)[)）])?$");
                if ( !nameMatch.Success )
                    continue;
                string mainName = nameMatch.Groups[1].Value.Trim();
                var aliases = new List<string>();
                if ( nameMatch.Groups[3].Success ) {
                    aliases = nameMatch.Groups[3].Value.Split(new[] { '、', ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(a => a.Trim())
                                                       .ToList();
                }
                // アーティスト名と別名義の両方でタグIDを検索します。
                var relevantTagIds = new HashSet<int>();
                if ( tagNameToId.ContainsKey(mainName) ) {
                    relevantTagIds.Add( tagNameToId[mainName] );
                }
                foreach ( var alias in aliases ) {
                    if ( tagNameToId.ContainsKey(alias) ) {
                        relevantTagIds.Add( tagNameToId[alias] );
                    }
                }
                // 見つかったタグIDに基づき、動画とアーティストの紐付けを行います。
                foreach ( var videoTag in videoTags ) {
                    if ( relevantTagIds.Contains(videoTag.TagId) ) {
                        // 既に紐付けが存在しない場合のみ追加します。
                        if ( !artist.VideoIds.Any( v => v.Id == videoTag.VideoId ) ) {
                            // videosリストから完全なNewFileオブジェクトを検索します。
                            if ( videoIdToFile.TryGetValue( videoTag.VideoId, out var videoToAdd ) ) {
                                artist.VideoIds.Add( videoToAdd );
                            }
                        }
                    }
                }
            }
        }

        // 指定タググループ配下のタグを削除します。(現状子までで、孫以下は消せない)
        public void RemoveYUUMEITags( List<NewTag> tags, string tagGroupName ) {
            var tagGroup = tags.FirstOrDefault(t => t.TagName == tagGroupName);
            if ( tagGroup != null ) {
                // タググループを削除
                tags.Remove( tagGroup );
                // その配下のタグも削除
                var famousTags = tags.Where(t => t.Parent == tagGroup.TagId).ToList();
                foreach ( var tag in famousTags ) {
                    tags.Remove( tag );
                }
            }
        }
    }
}
