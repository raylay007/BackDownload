using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SDKs.Downloader
{
    public class DownloadManager:MonoBehaviour
    {
        private static DownloadManager downloadMananger;
        private static Queue<WebFileDownloader> downloaderWaitings = new Queue<WebFileDownloader>();
        private static List<WebFileDownloader> downloaderLoadings = new List<WebFileDownloader>();

        private const int MAX_COUNT = 1;

        public static WebFileDownloader StartDownload(string url,string filePath)
        {
            if (downloadMananger == null)
            {
                GameObject downloadObject = new GameObject("DownloadObject");
                downloadMananger = downloadObject.AddComponent<DownloadManager>();
            }

            
            WebFileDownloader downloader = new WebFileDownloader(url, filePath);
            if (downloaderLoadings.Count >= MAX_COUNT)
            {
                downloaderWaitings.Enqueue(downloader);
            }
            else
            {
                downloadMananger.StartCoroutine(downloadMananger.Download(downloader));
            }
            
            return downloader;
        }

        private IEnumerator Download(WebFileDownloader downloader)
        {
            downloader.OnError += DownloadError;
            downloader.OnComplete += DownloadComplete;

            downloaderLoadings.Add(downloader);
            yield return downloader.Start();
        }

        private void DownloadComplete(WebFileDownloader d)
        {
            d.OnError -= DownloadError;
            d.OnComplete -= DownloadComplete;
            downloaderLoadings.Remove(d);
            if (downloaderLoadings.Count < MAX_COUNT && downloaderWaitings.Count > 0)
            {
                WebFileDownloader n = downloaderWaitings.Dequeue();
                downloadMananger.StartCoroutine(downloadMananger.Download(n));
            }
        }

        private void DownloadError(WebFileDownloader downloader)
        {
            //Debug.LogError("[DownloaderMnanager] DownloadError");
            StartCoroutine(DownloaderStart(downloader));
        }

        private IEnumerator DownloaderStart(WebFileDownloader downloader)
        {
            yield return new WaitForSeconds(3.0f);
            yield return downloader.Start();
        }
    }
}
