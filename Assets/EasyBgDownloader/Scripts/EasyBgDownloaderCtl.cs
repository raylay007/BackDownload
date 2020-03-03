using SDKs.Downloader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class EasyBgDownloaderCtl : MonoBehaviour {
	[SerializeField, TooltipAttribute("URL where file that to be downloaded exists.")]
	private string fileURL = "";
	[SerializeField, TooltipAttribute("Local path where downloaded file will be.")]
	private string destinationDirPath = "";
	//    [SerializeField, TooltipAttribute("If it's enabled, you get notification when download proccess finished as background.")]
	private bool notificationEnabled = true;
	//    [SerializeField, TooltipAttribute("If it's enabled, download proccess will be cached and you resume process after proccess stoped.")]
	private bool cacheEnabled = false;


	private static readonly string DEFAULT_CACHE_DIR = "ebd_tmp";

	private string DestinationDirectoryPath {
		set { destinationDirPath = value; }
		get {
			if (string.IsNullOrEmpty(destinationDirPath)) {
				string dirPath;
				if (Application.platform == RuntimePlatform.Android) {
					dirPath = Application.temporaryCachePath + "/" + DEFAULT_CACHE_DIR;
				} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
					dirPath = Application.temporaryCachePath + "/" + DEFAULT_CACHE_DIR;
				} else {
					dirPath = Application.streamingAssetsPath + "/" + DEFAULT_CACHE_DIR;
				}

				if (!Directory.Exists(dirPath)) {
					Directory.CreateDirectory (dirPath);
				}

				return dirPath;
			} else {
				return destinationDirPath;
			}
		}
	}

	//Event delegate values
	public delegate void CompleteDL (string requestURL, string filePath);
	public delegate void ErrorDL (string requestURL, DOWNLOAD_ERROR errorCode, string errorMessage);
	public delegate void ClickAndroidDLStatusBar (string requestURL);
	public CompleteDL OnComplete;
	public ErrorDL OnError;
	public ClickAndroidDLStatusBar OnClickAndroidStatus;


	public enum DOWNLOAD_STATUS {
		NOT_IN_QUEUE = -100,
		IN_QUEUE = 0, //PENDING || RUNNING || PAUSED || FAILED
		PENDING = 10,
		RUNNING = 20,
		PAUSED = 30,
		FAILED = 40
	}

	public enum DOWNLOAD_ERROR {
		NETWORK_ERROR = 1,
		INVALID_URL = 2,
		INVALID_DIR_PATH = 3,
		UNKNOWN_ERROR = 4
	}



	// Use this for initialization
	void Start () {
		initEBD ();
	}

	//	// Update is called once per frame
	//	void Update () {
	//		
	//	}

	void OnApplicationPause (bool pauseStatus) {
		if (pauseStatus) {
			pauseEBD ();
		} else {
			resumeEBD ();
		}
	}

	void OnDestroy () {
		terminateEBD ();
	}




	#region /*** Common Functions ***/
	/*
	* Common Functions
	*/
	public void SetDestinationDirPath (string path) {
		DestinationDirectoryPath = path; 
	}

	public string GetDestinationDirPath () {
		return DestinationDirectoryPath;
	}

	private bool isInvalidURL (string url) {
		//Has Scheme or not
		//file url or not
		Uri uriResult;
		if (Uri.TryCreate (url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)) {
			return false;		
		} else {
			return true;
		}
	}

	private bool isInvalidPath (string destPath) {
		//No dir
		return !Directory.Exists(destPath);
	}

	#endregion

	#if !UNITY_EDITOR && !UNITY_STANDALONE
	#if UNITY_ANDROID

	#region Android Functions
	/*
	* Platform values
	*/
	private const string ANDROID_DOWNLOAD_MANAGER_PACKAGE_CLASS_NAME = "nl.scopic.downloadmanagerplugin.DownloadManagerPlugin";
	private AndroidJavaObject androidJavaObj = null;

	/*
	* Life Cycle Control
	*/
	private void initEBD() {
	initEBDInAndroid();
	}

	private void resumeEBD() {
	resumeEBDInAndroid();
	}

	private void pauseEBD() {
	pauseEBDInAndroid();
	}

	private void terminateEBD() {
	terminateEBDInAndroid();
	}

	/*
	* Download Control
	*/
	public void StartDL (string requestURL = null, string naviTitle = null) {
	if (string.IsNullOrEmpty(requestURL)) {
	if (string.IsNullOrEmpty(fileURL)) {
	//ERROR : NULL Reference URL error
	return;
	}
	requestURL = fileURL;
	}

	if (isInvalidURL(requestURL)) {
	//ERROR : Invalid URL error
	return;
	}

	if (isInvalidPath(DestinationDirectoryPath)) {
	//ERROR : Invalid Directory Path Error
	return;
	}

	if (string.IsNullOrEmpty(naviTitle)) {
	naviTitle = Path.GetFileName(requestURL);
	}

	string destFilePath = "file://" + DestinationDirectoryPath + "/" + Path.GetFileName(requestURL); 

	startInAndroid(requestURL, destFilePath, naviTitle);
	}

	public void StopDL (string requestURL = null) {
	if (string.IsNullOrEmpty(requestURL)) {
	if (string.IsNullOrEmpty(fileURL)) {
	//ERROR : NULL Reference URL error
	return;
	}
	requestURL = fileURL;
	}

	if (isInvalidURL(requestURL)) {
	//ERROR : Invalid URL error
	return;
	}

	stopInAndroid(requestURL);
	}

	/*
	* Download Status
	*/
	public int GetStatus (string requestURL) {
	if (string.IsNullOrEmpty(requestURL)) {
	if (string.IsNullOrEmpty(fileURL)) {
	//ERROR : NULL Reference URL error
	return (int)DOWNLOAD_STATUS.NOT_IN_QUEUE;
	}
	requestURL = fileURL;
	}

	return getStatusInAndroid(requestURL);
	}

	public bool IsInQueue (string requestURL) {
	return (GetStatus(requestURL) != (int)DOWNLOAD_STATUS.NOT_IN_QUEUE) ? true : false;
	}

	public bool IsPending (string requestURL) {
	return (GetStatus(requestURL) == (int)DOWNLOAD_STATUS.PENDING) ? true : false;
	}

	public bool IsRunning (string requestURL) {
	return (GetStatus(requestURL) == (int)DOWNLOAD_STATUS.RUNNING) ? true : false;
	}

	public bool IsPaused (string requestURL) {
	return (GetStatus(requestURL) == (int)DOWNLOAD_STATUS.PAUSED) ? true : false;
	}

	public bool IsFailed (string requestURL) {
	return (GetStatus(requestURL) == (int)DOWNLOAD_STATUS.FAILED) ? true : false;
	}


	/*
	* Download Progress
	*/
	public float GetProgress (string requestURL = null) {
	if (string.IsNullOrEmpty(requestURL)) {
	if (string.IsNullOrEmpty(fileURL)) {
	//ERROR : NULL Reference URL error
	return 0.0f;
	}
	requestURL = fileURL;
	}

	return getProgressInAndroid(requestURL);
	}


	/*
	* Download Event
	*/
	private void onCompleteDL (string taskInfo) {
	if (OnComplete == null) return;
	string[] taskInfoStrings = taskInfo.Split(','); //[0] requestURL, [1] destinationPath
	string requestURL = !string.IsNullOrEmpty(taskInfoStrings[0]) ? taskInfoStrings[0] : "";
	string destFilePath = !string.IsNullOrEmpty(taskInfoStrings[1]) ? taskInfoStrings[1] : "";

	OnComplete (requestURL, destFilePath);
	}

	private void onErrorDL (string errorInfo) {
	if (OnError == null) return;

	string[] errorInfoStrings = errorInfo.Split(','); //[0] requestURL, [1] errorCode, [2] errorMessage
	string requestURL = !string.IsNullOrEmpty(errorInfoStrings[0]) ? errorInfoStrings[0] : "";
	int errorCodeInt = !string.IsNullOrEmpty(errorInfoStrings[1]) ? int.Parse(errorInfoStrings[1]) : (int)DOWNLOAD_ERROR.UNKNOWN_ERROR;
	DOWNLOAD_ERROR errorCode;
	switch (errorCodeInt) {
	case (int)DOWNLOAD_ERROR.NETWORK_ERROR:
	errorCode = DOWNLOAD_ERROR.NETWORK_ERROR;
	break;
	default:
	errorCode = DOWNLOAD_ERROR.UNKNOWN_ERROR;
	break;
	}
	string errorMessage = !string.IsNullOrEmpty(errorInfoStrings[2]) ? errorInfoStrings[2] : "Unknown Error.";
	OnError (requestURL, errorCode, errorMessage);
	}

	private void onClickAndroidStatusBar (string requestURL  = null) {

	if (OnClickAndroidStatus != null) {
	OnClickAndroidStatus(requestURL);
	}
	}


	/*
	* Platform Specific Functions
	*/


	/*
	* Plugin Interface
	*/
	private AndroidJavaObject getJavaObj() {
	if (androidJavaObj == null) {
	androidJavaObj = new AndroidJavaObject(ANDROID_DOWNLOAD_MANAGER_PACKAGE_CLASS_NAME, gameObject.name, notificationEnabled, cacheEnabled);
	}
	return androidJavaObj;
	}
	private void initEBDInAndroid () {
	getJavaObj().Call("initEBD");
	}
	private void resumeEBDInAndroid () {
	getJavaObj ().Call ("resumeEBD");
	}
	private void pauseEBDInAndroid () {
	getJavaObj ().Call ("pauseEBD");
	}
	private void terminateEBDInAndroid () {
	getJavaObj().Call("terminateEBD");
	}
	private void startInAndroid(string requestURL, string destPath, string naviTitle) {
	getJavaObj().Call("startDL", requestURL, destPath, naviTitle);
	}
	private void stopInAndroid(string requestURL) {
	getJavaObj().Call("stopDL", requestURL);
	}
	private int getStatusInAndroid(string requestURL) {
	return getJavaObj().Call<int>("getStatus", requestURL);
	}
	private float getProgressInAndroid(string requestURL) {
	return getJavaObj().Call<float>("getProgress", requestURL);
	}
	//Test
	public void CallTest () {
	getJavaObj().Call("callTest");
	}
	public void CallStaticTest () {
	getJavaObj().CallStatic("callStaticTest");
	}

	#endregion // END : Android Functions

	#elif UNITY_IPHONE // end : UNITY_ANDROID

	#region iOS Functions
	/*
	* Platform values
	*/

	/*
	* Life Cycle Control
	*/
	private void initEBD() {
	EBDInterfaceInit(gameObject.name, cacheEnabled, notificationEnabled);
	}

	private void resumeEBD() {
	EBDInterfaceResume();
	}

	private void pauseEBD() {
	EBDInterfacePause();
	}

	private void terminateEBD() {
	EBDInterfaceTerminate();
	}

	/*
	* Download Control
	*/
	public void StartDL (string requestURL = null, string naviTitle = null) {
	if (string.IsNullOrEmpty(requestURL)) {
	if (string.IsNullOrEmpty(fileURL)) {
	//ERROR : NULL Reference URL error
	return;
	}
	requestURL = fileURL;
	}

	if (isInvalidURL(requestURL)) {
	//ERROR : Invalid URL error
	return;
	}

	if (isInvalidPath(DestinationDirectoryPath)) {
	//ERROR : Invalid Directory Path Error
	return;
	}
	/*        
	if (string.IsNullOrEmpty(naviTitle)) {
	naviTitle = Path.GetFileName(requestURL);
	}
	*/
	string destFilePath = "file://" + DestinationDirectoryPath + "/" + Path.GetFileName(requestURL); 

	EBDInterfaceStartDL(requestURL, destFilePath);
	}

	public void StopDL (string requestURL = null) {
	if (string.IsNullOrEmpty(requestURL)) {
	if (string.IsNullOrEmpty(fileURL)) {
	//ERROR : NULL Reference URL error
	return;
	}
	requestURL = fileURL;
	}

	if (isInvalidURL(requestURL)) {
	//ERROR : Invalid URL error
	return;
	}

	EBDInterfaceStopDL(requestURL);
	}

	/*
	* Download Status
	*/
	public int GetStatus (string requestURL = null) {
	if (string.IsNullOrEmpty(requestURL)) {
	if (string.IsNullOrEmpty(fileURL)) {
	//ERROR : NULL Reference URL error
	return (int)DOWNLOAD_STATUS.NOT_IN_QUEUE;
	}
	requestURL = fileURL;
	}

	return EBDInterfaceGetStatus(requestURL);
	}

	public bool IsInQueue (string requestURL = null) {
	return (GetStatus(requestURL) != (int)DOWNLOAD_STATUS.NOT_IN_QUEUE) ? true : false;
	}

	public bool IsPending (string requestURL = null) {
	return (GetStatus(requestURL) == (int)DOWNLOAD_STATUS.PENDING) ? true : false;
	}

	public bool IsRunning (string requestURL = null) {
	return (GetStatus(requestURL) == (int)DOWNLOAD_STATUS.RUNNING) ? true : false;
	}

	public bool IsPaused (string requestURL = null) {
	return (GetStatus(requestURL) == (int)DOWNLOAD_STATUS.PAUSED) ? true : false;
	}

	public bool IsFailed (string requestURL = null) {
	return (GetStatus(requestURL) == (int)DOWNLOAD_STATUS.FAILED) ? true : false;
	}


	/*
	* Download Progress
	*/
	public float GetProgress (string requestURL = null) {
	if (string.IsNullOrEmpty(requestURL)) {
	if (string.IsNullOrEmpty(fileURL)) {
	//ERROR : NULL Reference URL error
	return 0.0f;
	}
	requestURL = fileURL;
	}

	return EBDInterfaceGetProgress(requestURL);
	}


	/*
	* Download Event
	*/
	private void onCompleteDL (string taskInfo) {
	if (OnComplete == null) return;
	string[] taskInfoStrings = taskInfo.Split(','); //[0] requestURL, [1] destinationPath
	string requestURL = !string.IsNullOrEmpty(taskInfoStrings[0]) ? taskInfoStrings[0] : "";
	string destFilePath = !string.IsNullOrEmpty(taskInfoStrings[1]) ? taskInfoStrings[1] : "";

	OnComplete (requestURL, destFilePath);
	}

	private void onErrorDL (string errorInfo) {
	if (OnError == null) return;

	string[] errorInfoStrings = errorInfo.Split(','); //[0] requestURL, [1] errorCode, [2] errorMessage
	string requestURL = !string.IsNullOrEmpty(errorInfoStrings[0]) ? errorInfoStrings[0] : "";
	int errorCodeInt = !string.IsNullOrEmpty(errorInfoStrings[1]) ? int.Parse(errorInfoStrings[1]) : (int)DOWNLOAD_ERROR.UNKNOWN_ERROR;
	DOWNLOAD_ERROR errorCode;
	switch (errorCodeInt) {
	case (int)DOWNLOAD_ERROR.NETWORK_ERROR:
	errorCode = DOWNLOAD_ERROR.NETWORK_ERROR;
	break;
	default:
	errorCode = DOWNLOAD_ERROR.UNKNOWN_ERROR;
	break;
	}
	string errorMessage = !string.IsNullOrEmpty(errorInfoStrings[2]) ? errorInfoStrings[2] : "Unknown Error.";
	OnError (requestURL, errorCode, errorMessage);
	}


	/*
	* Platform Specific Functions
	*/


	/*
	* Plugin Interface
	*/
	[DllImport("__Internal")]
	private static extern void EBDInterfaceInit (string gameObjName, bool cacheEnabled, bool notificationEnabled);
	[DllImport("__Internal")]
	private static extern void EBDInterfaceTerminate ();
	[DllImport("__Internal")]
	private static extern void EBDInterfaceResume ();
	[DllImport("__Internal")]
	private static extern void EBDInterfacePause ();
	[DllImport("__Internal")]
	private static extern void EBDInterfaceStartDL (string requestURL, string destPath);
	[DllImport("__Internal")]
	private static extern void EBDInterfaceStopDL(string requestURL);
	[DllImport("__Internal")]
	private static extern int EBDInterfaceGetStatus(string requestURL);
	[DllImport("__Internal")]
	private static extern float EBDInterfaceGetProgress(string requestURL);
	//test
	[DllImport("__Internal")]
	private static extern void EBDTestVoid ();
	[DllImport("__Internal")]
	private static extern int EBDTestReturnInt ();
	[DllImport("__Internal")]
	private static extern void EBDTestArgInt (int i);
	public void CallTest () {
	EBDTestVoid();
	}
	public void CallStaticTest () {
	EBDTestArgInt(10);
	}
	public void CallUnitySendMessage (string message) {
	Debug.Log ("Get Message and Call it from Unity : " + message);
	}


	#endregion // END : iOS Functions
	#endif

	#else 

	#region /*** Editor Functions ***/
	/*
	* Platform values
	*/
	private Dictionary<string, string> downloadTaskListInEditor;
	private string requestURLInEditor = "";
	private float currentProgressInEditor = 0.0f;

	/*
	 * Life Cycle Control
	 */
	private void initEBD () {
		downloadTaskListInEditor = new Dictionary<string, string>();
	}

	private void terminateEBD () {
	}

	private void pauseEBD () {
	}

	private void resumeEBD () {
	}

	/*
	 * Download Control
	 */
	public void StartDL (string requestURL = null, string naviTitle = null) {
		if (string.IsNullOrEmpty(requestURL)) {
			if (string.IsNullOrEmpty(fileURL)) {
				//ERROR : NULL Reference URL error
				return;
			}
			requestURL = fileURL;
		}

		if (isInvalidURL(requestURL)) {
			//ERROR : Invalid URL error
			return;
		}

		if (isInvalidPath(DestinationDirectoryPath)) {
			//ERROR : Invalid Directory Path Error
			return;
		}

		if (isInQueue(requestURL)) {
			return;
		}

        Debug.LogFormat("网络路径: {0}, 本地保存路径：{1} ", requestURL, DestinationDirectoryPath);

        addTask(requestURL, DestinationDirectoryPath);
        //StartCoroutine ("startInEditor", requestURL);

        downloader = DownloadManager.StartDownload(requestURL, DestinationDirectoryPath);
        downloader.OnDownloading += OnDownloading;
        downloader.OnComplete += OnLoadFilesComplete;
        downloader.OnError += onDownErrorLoadFiles;

    }

    private void OnApplicationQuit()
    {
        if(downloader != null)
        {
            downloader.OnDownloading -= OnDownloading;
            downloader.OnComplete -= OnLoadFilesComplete;
            downloader.OnError -= onDownErrorLoadFiles;

        }
    }

    private void OnDownloading(WebFileDownloader obj)
    {
        currentProgressInEditor = obj.Progress;
        //Debug.Log("进度：" + currentProgressInEditor);
    }

    private void onDownErrorLoadFiles(WebFileDownloader obj)
    {
        Debug.LogError("下载错误："+obj.Error);
        onErrorDL(obj.Url, obj.Error, DOWNLOAD_ERROR.NETWORK_ERROR);
    }

    private void OnLoadFilesComplete(WebFileDownloader obj)
    {
        Debug.Log("数据长度："+obj.NetDatas.Length);
        saveFileAtPath(obj.NetDatas, obj.Url, downloadTaskListInEditor[obj.Url]);

    }

    private WebFileDownloader downloader;

    public void StopDL (string requestURL = null) {
		if (string.IsNullOrEmpty(requestURL)) {
			if (string.IsNullOrEmpty(fileURL)) {
				//ERROR : NULL Reference URL error
				return;
			}
			requestURL = fileURL;
		}

		stopInEditor (requestURL);
	}

	public int GetStatus (string requestURL = null) {
		if (string.IsNullOrEmpty(requestURL)) {
			if (string.IsNullOrEmpty(fileURL)) {
				//ERROR : NULL Reference URL error
				return (int)DOWNLOAD_STATUS.NOT_IN_QUEUE;
			}
			requestURL = fileURL;
		}

		return 1;
	}

	public bool IsRunning (string requestURL = null) {
		if (string.IsNullOrEmpty(requestURL)) {
			if (string.IsNullOrEmpty(fileURL)) {
				//ERROR : NULL Reference URL error
				return false;
			}
			requestURL = fileURL;
		}

		return isInQueue (requestURL) ? true : false; 
	}

	public float GetProgress (string requestURL = null) {
		if (string.IsNullOrEmpty(requestURL)) {
			if (string.IsNullOrEmpty(fileURL)) {
				//ERROR : NULL Reference URL error
				return 0.0f;
			}
			requestURL = fileURL;
		}

		if (requestURLInEditor != requestURL) {
			changeCurrentTask (requestURL);
		}

		return currentProgressInEditor;
	}

	/*
	 * Download Event
	 */
	private void onCompleteDL (string requestURL, string filePath) {
		removeTask (requestURL);

		if (OnComplete != null) {
			OnComplete (requestURL, filePath);
		}
	}

	private void onErrorDL (string requestURL, string errorMessage, DOWNLOAD_ERROR errorCode) {
		//TODO : convert error message to error code
		if (OnError != null) {
			OnError (requestURL, errorCode, errorMessage);
		}
	}

    private IEnumerator startInEditor(string requestURL)
    {
        UnityWebRequest request = UnityWebRequest.Get(requestURL);

        yield return request.SendWebRequest();

        if (request.isDone)
        {
            if (isInQueue(requestURL))
            {
                Debug.Log("Progress : " + request.downloadProgress);
                if (requestURLInEditor == requestURL)
                {
                    currentProgressInEditor = request.downloadProgress;
                    if (currentProgressInEditor > 0.995)
                    {
                        currentProgressInEditor = 1.0f;
                    }
                }
                yield return null;
            }

        }

        if (isInQueue(requestURL))
        {
            if (!string.IsNullOrEmpty(request.error))
            {
                //Error occured
                onErrorDL(requestURL, request.error, DOWNLOAD_ERROR.NETWORK_ERROR);
            }
            else
            {
                if (requestURLInEditor == requestURL)
                {
                    currentProgressInEditor = 1.0f;
                }
                saveFileAtPath(request, requestURL, downloadTaskListInEditor[requestURL]);
            }
        }
    }

    /*
	 * Platform Specific Functions
	 */
    //private IEnumerator startInEditor (string requestURL) {
    //	WWW www = new WWW (requestURL);

    //	while (!www.isDone) {
    //		if (isInQueue (requestURL)) {
    //			//Debug.Log("Progress : " + www.progress);
    //			if (requestURLInEditor == requestURL) {
    //				currentProgressInEditor = www.progress;
    //				if (currentProgressInEditor > 0.995) {
    //					currentProgressInEditor = 1.0f;
    //				}
    //			}
    //			yield return null;
    //		} else {
    //			//Queue Stopped
    //			break;
    //		}
    //	}

    //	if (isInQueue (requestURL)) {
    //		if (!string.IsNullOrEmpty (www.error)) {
    //			//Error occured
    //			onErrorDL(requestURL, www.error, DOWNLOAD_ERROR.NETWORK_ERROR);
    //		} else {
    //			if (requestURLInEditor == requestURL) {
    //				currentProgressInEditor = 1.0f;
    //			}
    //			saveFileAtPath (www, requestURL, downloadTaskListInEditor[requestURL]);
    //		}
    //	}
    //}

    private void stopInEditor (string requestURL) {
		removeTask (requestURL);
	}

    private void saveFileAtPath(byte[] data, string requestURL, string destPath)
    {
        if (!System.IO.Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        string fileName = Path.GetFileName(requestURL.Split('?')[0]);
        System.IO.File.WriteAllBytes(destPath + "/" + fileName, data);

        //Donwload Complete
        onCompleteDL(requestURL, destPath);

        Debug.LogFormat("最后保存----------------------- requestURL: {0},  destPath{1}: ", requestURL, destPath);
    }

    private void saveFileAtPath(UnityWebRequest request, string requestURL, string destPath)
    {
        if (!System.IO.Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        string fileName = Path.GetFileName(requestURL.Split('?')[0]);
        System.IO.File.WriteAllBytes(destPath + "/" + fileName, request.downloadHandler.data);

        //Donwload Complete
        onCompleteDL(requestURL, destPath);

        Debug.LogFormat("最后保存----------------------- requestURL: {0},  destPath{1}: ", requestURL, destPath);
    }

    //private void saveFileAtPath (WWW data, string requestURL, string destPath) {
    //	if (!System.IO.Directory.Exists(destPath)) {
    //		Directory.CreateDirectory(destPath);
    //	}

    //	string fileName = Path.GetFileName(requestURL.Split('?')[0]);
    //	System.IO.File.WriteAllBytes(destPath + "/" + fileName, data.bytes);

    //	//Donwload Complete
    //	onCompleteDL(requestURL, destPath);

    //       Debug.LogFormat("requestURL: {0},  destPath{1}: ", requestURL,destPath);
    //}

    private void changeCurrentTask (string requestURL) {
		requestURLInEditor = requestURL;
		currentProgressInEditor = 0.0f;
	}

	private bool isInQueue (string requestURL) {
		if (downloadTaskListInEditor.ContainsKey (requestURL)) {
			return true;
		} else {
			return false;
		}
	}

	private void addTask (string requestURL, string destPath) {
		if (downloadTaskListInEditor.ContainsKey (requestURL)) {
			if (downloadTaskListInEditor [requestURL] != destPath) {
				downloadTaskListInEditor [requestURL] = destPath;
			}
		} else {
			downloadTaskListInEditor.Add (requestURL, destPath);
		}
	}

	private void removeTask (string requestURL) {
		if (downloadTaskListInEditor.ContainsKey (requestURL)) {
			downloadTaskListInEditor.Remove (requestURL);
		}
	}

	#endregion // end : UNITY_EDITOR && UNITY_STANDALONE

	#endif   
}
