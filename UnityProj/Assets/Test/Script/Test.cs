using MFramework.AssetService;
using MFramework.Common;
using MFramework.DownloadService;
using MFramework.HotUpdateService;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour {

	public Button loadBtn;
	public Button loadSyncBtn;
	public InputField loadPathInputField;
	public Transform content;


	// Use this for initialization
	void Start () {
        //Object.Instantiate(AssetBundle.LoadFromFile(@"D:\other\other_project\MFramework\Build\Windows\assets_resources_uiprefab.bundle").LoadAsset("Assets/Resources/UIPrefab/Cube.prefab"));
        //var curTime = Time.deltaTime;
        //      AssetService.GetInstance().LoadAssetAsync("Assets/Resources/UIPrefab/Cube.prefab").Completed += (obj) =>
        //      {
        //	Log.LogE((Time.deltaTime - curTime).ToString("f6"));
        //      };
        //loadBtn.onClick.AddListener(() =>
        //{
        //    AssetBase assetBase = AssetService.GetInstance().LoadAsset(loadPathInputField.text);
        //    if (assetBase != null)
        //    {
        //        Add(assetBase);
        //    }
        //    else
        //    {
        //        Log.LogE("加载失败:{0}", loadPathInputField.text);
        //    }
        //});
        //loadSyncBtn.onClick.AddListener(() =>
        //{
        //    AssetRequest assetRequest = AssetService.GetInstance().LoadAssetAsync(loadPathInputField.text);
        //    if (assetRequest != null)
        //    {
        //        assetRequest.Completed += (obj) =>
        //        {
        //            Add(assetRequest.Asset);
        //            assetRequest.Asset.Unload();
        //        };
        //    }
        //    else
        //    {
        //        Log.LogE("加载失败:{0}", loadPathInputField.text);
        //    }
        //    AssetRequest assetRequest2 = AssetService.GetInstance().LoadAssetAsync(loadPathInputField.text);
        //    if (assetRequest2 != null)
        //    {
        //        assetRequest2.Completed += (obj) =>
        //        {
        //            Add(assetRequest2.Asset);
        //        };
        //    }
        //    else
        //    {
        //        Log.LogE("加载失败:{0}", loadPathInputField.text);
        //    }
        //});
        //StartCoroutine(start());
    }

	IEnumerator start()
	{
        HotUpdateAsyncOperation hotUpdateAsyncOperation = HotUpdateService.GetInstance().StartUpdate();
        while (!hotUpdateAsyncOperation.IsDone)
        {
            Log.LogD("progress:{0},speed:{1}", hotUpdateAsyncOperation.Progress, hotUpdateAsyncOperation.DownloadSpeed);
            yield return new WaitForSecondsRealtime(0.01f);
        }
        Log.LogD("progress:{0},speed:{1}", hotUpdateAsyncOperation.Progress, hotUpdateAsyncOperation.DownloadSpeed);
        Log.LogD(hotUpdateAsyncOperation.IsDone.ToString());
    }


	private void Add(AssetBase assetBase)
	{
        GameObject obj = Object.Instantiate(loadBtn.gameObject, content);
        obj.transform.Find("Text").GetComponent<Text>().text = Path.GetFileNameWithoutExtension(assetBase.ResPath);
        obj.GetComponent<Button>().onClick.AddListener(() =>
        {
            assetBase.Unload();
            Object.Destroy(obj);
        });
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
