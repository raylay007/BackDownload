using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace SDKs.Downloader
{
    public class WebFileDownloader
    {
        private const string EXTAINSION_NAME = ".download";
        public event Action<WebFileDownloader> OnComplete;
        public event Action<WebFileDownloader> OnDownloading; 
        public event Action<WebFileDownloader> OnError;

        private string url;
        private string filePath;
        private float progress;
        private string error;
        private byte[] netDatas;

        /// <summary>
        /// 下载中出现的错误
        /// </summary>
        public string Error
        {
            get { return error; }
        }

        /// <summary>
        /// 下载完成后的数据
        /// </summary>
        public byte[] NetDatas
        {
            get { return netDatas; }
        }

        public WebFileDownloader(string url, string filePath)
        {
            this.url = url;
            this.filePath = filePath;
        }

        /// <summary>
        /// 网络路径
        /// </summary>
        public string Url
        {
            get
            {
                return url;
            }
        }

        /// <summary>
        /// 下载进度
        /// </summary>
        public float Progress
        {
            get
            {
                return progress;
            }
        }

        /// <summary>
        /// 数据保存在本地的路径
        /// </summary>
        public string FilePath
        {
            get
            {
                return filePath;
            }
        }
 
        public IEnumerator Start(bool resume = false)
        {
            Debug.LogFormat("[WebFileDownloader] Start :{0}", this.url);

            var headRequest = UnityWebRequest.Head(url);
            yield return headRequest.SendWebRequest();
            //Debug.LogError("HeadRequest Error:" + headRequest.isNetworkError);
            if (headRequest.isNetworkError)
            {
                if (OnError != null)
                {
                    OnError(this);
                }
            }
            else
            {
                string len = headRequest.GetResponseHeader("Content-Length");

                Debug.Log("---------------------------获得长度："+len);

                if (string.IsNullOrEmpty(len))
                {
                  //  Debug.Log(" 下载出错:" + len);
                    if (OnError != null)
                    {
                        OnError(this);
                    }
                }
                else
                {
                    var totalLength = long.Parse(len);
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    string fileName = Path.GetFileName(this.url);
                    string filePath2 = filePath + "/" + fileName;
                    string tempFilePath = filePath2 + EXTAINSION_NAME;
                    tempFilePath = tempFilePath.Replace("\\", "/");
                    filePath2 = filePath2.Replace("\\", "/");
                    string dir = Path.GetDirectoryName(filePath2);

                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    long lastLen = 0;
                    float _progress = 0;
 
                    using (FileStream stream = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        var streamLength = stream.Length;

                        lastLen = streamLength;
 
                        if (!resume)
                        {
                            streamLength = 0;
                        }

                        if (streamLength < totalLength)
                        {
                            stream.Seek(streamLength, SeekOrigin.Current);
                        }


                        UnityWebRequest request = UnityWebRequest.Get(this.url);
                         
                        request.SetRequestHeader("Range", "bytes=" + streamLength + "-" + totalLength);
                        request.SendWebRequest();
                        error = "";

                        int offset = 0;
                        while (true)
                        {
                            if (request.isNetworkError)
                            {
                                long code = request.responseCode;
                              //  Debug.LogFormat("[WebFileDownloader] Error:{0}", code);
                                error = request.error + ",responeseCode:" + request.responseCode;
                                break;
                            }

                            byte[] data = request.downloadHandler.data;
                            int length = data.Length - offset;
                            stream.Write(data, offset, length);
                            offset += length;
                            streamLength += length;
 
                            _progress = (float)streamLength / (float)totalLength;

                            float val = (float)(streamLength );

                            progress = val / totalLength;

                            //Debug.LogFormat("val:{0}, totalLength:{1} ,streamLength:{2} ,lastLen:{3} ===当前进度：{4}  实际progress:{5}",
                            //    val, totalLength, streamLength, lastLen, progress, _progress);

                            if (OnDownloading != null) OnDownloading.Invoke(this);

                            if (_progress >= 1)
                            {
                                break;
                            }

                            yield return 0;
                        }

                        netDatas = request.downloadHandler.data;

                        headRequest.Dispose();
                        request.downloadHandler.Dispose();
                        request = null;
                        stream.Close();
                    }

                    if (string.IsNullOrEmpty(error))
                    {
                        if (File.Exists(tempFilePath))
                        {
                            ////Debug.Log("开始删除临时文件："+ tempFilePath);
                            if (File.Exists(filePath2))
                            {
                                File.Delete(filePath2);
                            }

                            File.Move(tempFilePath, filePath2);
                            //Debug.LogFormat("存入本地文件地址：{0}", filePath2);

                        }

                        //Debug.Log(" 下载完成");

                        if (OnComplete != null)
                        {
                            OnComplete.Invoke(this);
                        }
                    }
                    else
                    {
                        //Debug.Log(" 下载出错");
                        if (OnError != null)
                        {
                            OnError(this);
                        }
                    }
                }
            }

        }
    }



}
