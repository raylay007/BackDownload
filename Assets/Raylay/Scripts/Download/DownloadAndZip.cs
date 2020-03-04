using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DownloadAndZip : MonoBehaviour
{
    public EasyBgDownloaderCtl ebdCtl;
    public InputField urlInputField;

    private string localSavePath; 
    private string zipFilePath;
    private string url;

    private bool _isDownload = false;
    public bool isDownload {
        get { return _isDownload; }
        set
        {
            _isDownload = value;
            if (_isDownload)
            {
                if (File.Exists(zipFilePath))
                {
                    Debug.Log("已经有这个文件了：" + zipFilePath);
                    isUnzip = true;
                    return;
                }

                ebdCtl.StartDL(url);
                Debug.Log("开始下载：" + url);

            }
            else
            {
                ebdCtl.StopDL(url);
                Debug.Log("停止下载：" + url);

            }
        }
    }

    private bool _isUnzip = false;
    public bool isUnzip {
        get { return _isUnzip; }
        set
        {
            _isUnzip = value;
            if (_isUnzip)
            {
                if (!File.Exists(zipFilePath))
                {
                    Debug.LogError("没有找到压缩包："+ zipFilePath);
                    return;
                }
 
                threadUnzip = new Thread(() => {

                    StartUnZip(zipFilePath, localSavePath, () => {

                        Debug.Log("----------------结束解压！TODO");

                    });

                });

                threadUnzip.Start();
            }
            else
            {
                if (threadUnzip != null) threadUnzip.Abort();

            }
        }
    }

    private void OnApplicationQuit()
    {
        isUnzip = false;
    }

    private float unzipProgress;
    private float downloadProgress;

    public Slider sliderDownload;
    public Slider sliderUnzip;

    private Thread threadUnzip;

    public void OnClickDownLoad()
    {
        if (isDownload) return;

        initDownLoad();

        isDownload = true;
      
    }


    public void OnClickUnZip()
    {
        if (isUnzip) return;

        isUnzip = true;
 
    }

    private void Start()
    {
        Application.runInBackground = true;
       
        sliderDownload.value = 0;
        sliderUnzip.value = 0;

        url = urlInputField.text;
         
        localSavePath = Application.persistentDataPath;
        Debug.Log("数据存储位置：" + localSavePath);

        zipFilePath = Path.Combine(localSavePath, Path.GetFileName(url)).Replace("\\", "//");

    }

    private void initDownLoad()
    {
        url = urlInputField.text;

        if (string.IsNullOrEmpty(url)) { Debug.LogError("下载路径不能为空！"); return; }
        if (!url.Contains("http") || string.IsNullOrEmpty(Path.GetExtension(url))) { Debug.LogError("下载路径不合法！"); return; }
 
        zipFilePath = Path.Combine(localSavePath, Path.GetFileName(url)).Replace("\\", "//");
        Debug.Log("zipFilePath :" + zipFilePath);

        ebdCtl.SetDestinationDirPath(localSavePath);
        ebdCtl.OnComplete = OnLoadComplete;
        ebdCtl.OnError = OnLoadError;

        sliderDownload.value = 0;
        sliderUnzip.value = 0;
        downloadProgress = 0;

        //string path = url;
        //Debug.Log("GetFileName :" + Path.GetFileName(path));
        //Debug.Log("GetFileNameWithoutExtension :" + Path.GetFileNameWithoutExtension(path));
        //Debug.Log("GetFullPath :" + Path.GetFullPath(path));
        //Debug.Log("GetDirectoryName :" + Path.GetDirectoryName(path));
        //Debug.Log("GetPathRoot :" + Path.GetPathRoot(path));
        //Debug.Log("GetExtension :" + Path.GetExtension(path));
    }

    private void OnLoadError(string requestURL, EasyBgDownloaderCtl.DOWNLOAD_ERROR errorCode, string errorMessage)
    {
        Debug.LogError("下载错误：" + errorMessage);
        isDownload = false;
    }

    private void OnLoadComplete(string requestURL, string filePath)
    {

        Debug.Log("下载完成：" + requestURL);

        sliderDownload.value = 1;
        isDownload = false;

        isUnzip = true;

    }

    private void DeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Debug.Log("删除目录：" + path);

            Directory.Delete(path, true);
        }
    }

    public void DelayInvoke(Action ac, float time)
    {
        StartCoroutine(waitLoad(ac, time));
    }

    IEnumerator waitLoad(Action ac,float time)
    {
        yield return new WaitForSeconds(time);

        if (ac != null) ac.Invoke();

    }

    /// <summary>
    /// 开始解压
    /// </summary>
    /// <param name="targetFilePath"></param>
    /// <param name="saveDir"></param>
    /// <param name="onComplete"></param>
    void StartUnZip(string targetFilePath, string saveDir,Action onComplete = null)
    {
        unzipProgress = 0;

        UnZip(targetFilePath, saveDir, onComplete);
    }


    void FixedUpdate()
    {
  
        if (!string.IsNullOrEmpty(url) && ebdCtl.IsRunning(url))
        {
            string progressStr = (Math.Round(ebdCtl.GetProgress(url),4) * 100).ToString("f2") + "%";
            downloadProgress = (float)Math.Round(ebdCtl.GetProgress(url), 4) ;
 
            //Debug.Log("下载进度："+ progressStr);
 
        }

        if (isDownload)
        {
            sliderDownload.value = downloadProgress;
        }

        if (isUnzip)
        {
           
            //Debug.Log("unzipProgress: " + unzipProgress);
            sliderUnzip.value = unzipProgress;

            if (unzipProgress == 1) {

                SceneManager.LoadScene(1);

            }

        }
    }
 
    public void UnZip(string zipFilePath, string unZipDir, Action onComplete = null)
    {

        startWriteZip(zipFilePath, unZipDir, onComplete); 
         
    }

    private void startWriteZip(string zipFilePath, string unZipDir, Action onComplete = null)
    {
        float currentSize = 0;

        try
        {
            if (zipFilePath == string.Empty)
            {
                isUnzip = false;
                throw new Exception("压缩文件不能为空！");
            }
            if (!File.Exists(zipFilePath))
            {
                isUnzip = false;

                throw new FileNotFoundException("压缩文件不存在！" + zipFilePath);
            }

            FileInfo fileInfo = new FileInfo(zipFilePath);
            float totalLength = (float)fileInfo.Length;

            //Debug.Log("totalLength: " + totalLength);

            //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹  
            if (unZipDir == string.Empty)
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            if (!unZipDir.EndsWith("/"))
                unZipDir += "/";
            if (!Directory.Exists(unZipDir))
                Directory.CreateDirectory(unZipDir);

            Debug.Log("开始解压>>解压的文件是： " + zipFilePath);
 
            using (var zip = new ZipInputStream(File.OpenRead(zipFilePath)))
            {
                ZipEntry theEntry;
                ZipConstants.DefaultCodePage = Encoding.UTF8.CodePage;

                while ((theEntry = zip.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    //Debug.LogFormat("directoryName: {0}, fileName:{1}", directoryName, fileName);

                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        Directory.CreateDirectory(unZipDir + directoryName);
                    }

                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                        {

                            int size;

                            byte[] data = new byte[2048];

                            while (true)
                            {
                                size = zip.Read(data, 0, data.Length);

                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                    currentSize += size;//currentSize / 1785432;//
                                    unzipProgress = currentSize / totalLength  > 1 ? 1: currentSize / totalLength;//
                                }
                                else
                                {
                                    break;
                                }

                            }

                        }
                    }
                }
            }

            unzipProgress = 1;

            if (onComplete != null) onComplete.Invoke();

            //Debug.Log("解压完成，删除压缩包：" + filePath);
            //File.Delete(filePath);

        }
        catch (Exception ex)
        {
            isUnzip = false;
            Debug.LogError("解压出现异常：" + ex.Message);

        }
    }
     
}
