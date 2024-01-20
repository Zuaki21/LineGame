#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

[CreateAssetMenu(fileName = "GoogleDriveSettingData", menuName = "Zuaki/GoogleDriveSettingData", order = 100)]

public class GoogleDriveSettingData : ScriptableObject
{
    string LocalFolderPath => AssetDatabase.GetAssetPath(localFolder);
    [SerializeField] Object localFolder;
    [SerializeField, HideInInspector] Object oldLocalFolder;
    [SerializeField] string googleDriveFolderID;
    [SerializeField, Header("全てのファイルをダウンロードする")] bool downloadAllFiles = false;
    [SerializeField, Header("ダウンロードするファイルの拡張子")]
    string[] extentions =
    {
        @".png",
        @".jpg",
        @".jpeg",
        @".ogg",
        @".mp3",
        @".wav",
        @".csv",
        @".txt",
    };
    [SerializeField, Header("デバッグログに詳細を表示する")] bool debugLog = false;

    private void OnValidate()
    {
        //localFolderが変更されたらフォルダかどうかを確認
        if (localFolder != null)
        {
            string path = AssetDatabase.GetAssetPath(localFolder);
            if (!AssetDatabase.IsValidFolder(path))
            {
                // Inspectorにエラーメッセージを表示
                UnityEditor.EditorUtility.DisplayDialog("エラー", "フォルダを指定してください", "OK");
                // 不正な値をnullに戻す
                localFolder = oldLocalFolder;
                return;
            }
            oldLocalFolder = localFolder;
        }
    }

    public IEnumerator Download()
    {
        yield return GoogleDriveDownloader.DownloadFiles(LocalFolderPath, googleDriveFolderID, downloadAllFiles, extentions, debugLog);
    }

    public void OpenFolder()
    {
        Application.OpenURL("https://drive.google.com/drive/folders/" + googleDriveFolderID);
    }
}

//インスペクタにDownloadボタンを表示する
[CustomEditor(typeof(GoogleDriveSettingData))]

public class GoogleDriveSettingDataEditor : Editor
{
    private EditorCoroutine m_coroutine;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("フォルダの同期"))
        {
            if (m_coroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(m_coroutine);
            }
            EditorUtility.ClearProgressBar();
            m_coroutine = EditorCoroutineUtility.StartCoroutine(((GoogleDriveSettingData)target).Download(), this);
        }

        if (GUILayout.Button("フォルダの場所を開く"))
        {
            ((GoogleDriveSettingData)target).OpenFolder();
        }
    }
}
#endif
