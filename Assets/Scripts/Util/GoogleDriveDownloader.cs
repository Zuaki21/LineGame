#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityGoogleDrive;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;

public static class GoogleDriveDownloader
{
    //アクセストークンを更新するメソッド
    public static IEnumerator RefreshAccessToken()
    {
        AuthController.RefreshAccessToken();
        while (AuthController.IsRefreshingAccessToken)
            yield return null;
        // ... now it's safe to use the APIs
    }

    // ファイルの更新日時を比較するメソッド
    static bool CompareDate(DateTime time1, DateTime time2)
    {
        if (time1.Year == time2.Year && time1.Month == time2.Month && time1.Day == time2.Day && time1.Hour == time2.Hour && time1.Minute == time2.Minute && time1.Second == time2.Second)
        {
            return true;
        }
        return false;
    }

    // ファイルの拡張子を確認するメソッド
    static bool CheckFileExtension(string name, string[] extensions)
    {
        bool type = false;
        foreach (var item in extensions)
        {
            if (Regex.IsMatch(name, item))
            {
                type = true;
                break;
            }
        }
        return type;
    }

    public class FileInfo
    {
        public string localFilePath;
        public string localDirPath;
        public string remoteId;
        public DateTime modifiedTime;
        public string name;
    }

    /// <summary>
    /// 指定したフォルダ内のファイルのうち、更新のあるファイルのみダウンロードします。
    /// </summary>
    /// <param name="localFolderPath">ダウンロード先のフォルダのパス</param>
    /// <param name="GoogleFolderId">ダウンロード元のグーグルドライブのフォルダID</param>
    /// <param name="downloadAllFiles">全てのファイルをダウンロードするかどうか</param>
    /// <param name="extensions">ダウンロードする拡張子の指定(無い場合は全てダウンロードします)</param>
    /// <param name="isDebug">デバッグモード</param>
    /// <returns></returns>
    public static IEnumerator DownloadFiles(string localFolderPath, string GoogleFolderId, bool downloadAllFiles = false, string[] extensions = null, bool isDebug = false)
    {
        Debug.Log("同期を開始します");
        if (EditorUtility.DisplayCancelableProgressBar("同期中", "アクセストークンを取得しています。", 0))
        {
            EditorUtility.ClearProgressBar();
            yield break;
        }
        // アクセストークンの更新が終わるまで待機
        yield return RefreshAccessToken();


        if (EditorUtility.DisplayCancelableProgressBar("同期中", "ダウンロード対象を探しています", 0.05f))
        {
            EditorUtility.ClearProgressBar();
            yield break;
        }
        // ダウンロードするファイルのリストを作成
        List<FileInfo> downloadFiles = new List<FileInfo>();
        yield return MakeDownloadList(localFolderPath, GoogleFolderId, downloadAllFiles, extensions, isDebug, downloadFiles);

        int index = 0;
        // ファイルをダウンロードする
        foreach (FileInfo downloadFile in downloadFiles)
        {
            index++;

            var downloadFileRequest = GoogleDriveFiles.Download(fileId: downloadFile.remoteId);
            var downloadFileAsync = downloadFileRequest.Send();

            while (!downloadFileAsync.IsDone)
            {
                if (EditorUtility.DisplayCancelableProgressBar("同期中", downloadFile.name + "をダウンロード中です", (downloadFileAsync.Progress + index) / downloadFiles.Count * 0.95f + 0.05f))
                {
                    EditorUtility.ClearProgressBar();
                    yield break;
                }
                yield return null;

                // エラーが発生した場合
                if (downloadFileRequest.IsError)
                {
                    Debug.LogError(downloadFileRequest.Error);
                    yield break;
                }
            }

            // ダウンロードしたファイルを保存
            if (!Directory.Exists(downloadFile.localDirPath))
            {
                Directory.CreateDirectory(downloadFile.localDirPath);
                if (isDebug)
                {
                    UnityEngine.Object folderObj = AssetDatabase.LoadAssetAtPath(downloadFile.localDirPath, typeof(UnityEngine.Object));
                    Debug.Log("フォルダがローカルに存在しなかったため作成しました (" + downloadFile.localDirPath + ")", folderObj);
                }
            }
            File.WriteAllBytes(downloadFile.localFilePath, downloadFileRequest.ResponseData.Content);
            File.SetLastWriteTimeUtc(downloadFile.localFilePath, downloadFile.modifiedTime);
            if (isDebug)
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(downloadFile.localFilePath, typeof(UnityEngine.Object));
                Debug.Log(downloadFile.name + "をダウンロードしました。( " + downloadFile.localFilePath + ")", obj);
            }
        }
        AssetDatabase.Refresh();

        Debug.Log("同期が完了しました");
        EditorUtility.ClearProgressBar();
    }

    static IEnumerator MakeDownloadList(string localFolderPath, string GoogleFolderId, bool downloadAllFiles = false, string[] extensions = null, bool isDebug = false, List<FileInfo> downloadFiles = null)
    {
        using (GoogleDriveFiles.ListRequest fileListRequest = GoogleDriveFiles.List())
        {
            // 取得するフィールドを設定
            fileListRequest.Fields = new List<string> { "files(id, name, size, createdTime, modifiedTime, trashed, mimeType)" };
            // クエリの内容を設定
            fileListRequest.Q = $"\'{GoogleFolderId}\' in parents";

            var fileListAsync = fileListRequest.Send();
            while (!fileListAsync.IsDone)
            {
                yield return null;

                // エラーが発生した場合
                if (fileListRequest.IsError)
                {
                    Debug.LogError(fileListRequest.Error);
                    yield break;
                }
            }

            // ファイルリストを取得
            UnityGoogleDrive.Data.FileList list = fileListRequest.ResponseData;

            if (list.Files.Count == 0)
            {
                Debug.LogWarning("フォルダ内にファイルが見つかりませんでした。 (FolderID: " + GoogleFolderId + ")");
                yield break;
            }


            foreach (var file in list.Files)
            {

                // GoogleDrive内ファイルの更新日時(標準時間)
                DateTime remoteDate = (DateTime)file.ModifiedTime;

                // ローカルファイルのパス
                string path = Path.Combine(localFolderPath, file.Name).Replace("\\", "/");


                if (file.MimeType == "application/vnd.google-apps.folder")
                {
                    if (isDebug) Debug.Log("子フォルダを検出しましたダウンロードします");
                    // フォルダの場合は再帰的に呼び出す
                    yield return MakeDownloadList(path, file.Id, downloadAllFiles, extensions, isDebug, downloadFiles);
                    continue;
                }

                // ダウンロードする拡張子が指定されている場合は、指定されていないファイルはダウンロードしない
                if (downloadAllFiles == false)
                {
                    if (!CheckFileExtension(file.Name, extensions))
                    {
                        if (isDebug) Debug.Log(file.Name + "は指定外の拡張子であるためダウンロードしません");
                        continue;
                    }
                }

                // ゴミ箱にある場合はダウンロードしない
                if ((bool)file.Trashed)
                {
                    if (isDebug) Debug.Log(file.Name + "はゴミ箱にあるためダウンロードしません");
                    continue;
                }

                if (File.Exists(path))
                {
                    // ローカルファイル更新日時を取得(標準時間)
                    DateTime localdate = File.GetLastWriteTimeUtc(path);

                    // 既に同じ更新日時のファイルがある場合はダウンロードしない
                    if (CompareDate(localdate, remoteDate))
                    {
                        if (isDebug) Debug.Log(file.Name + "は更新されていないためダウンロードしません");
                        continue;
                    }
                    else
                    {
                        if (isDebug) Debug.Log(file.Name + "は更新されているためダウンロードします");
                    }
                }
                else
                {
                    if (isDebug) Debug.Log(file.Name + "はローカルに存在しないためダウンロードします");
                }
                downloadFiles.Add(new FileInfo { localFilePath = path, localDirPath = localFolderPath, remoteId = file.Id, modifiedTime = remoteDate, name = file.Name });
            }
        }
    }
}
#endif
