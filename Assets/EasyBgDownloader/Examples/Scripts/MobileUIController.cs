using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(MobileUIView))]
public class MobileUIController : MonoBehaviour {
	[SerializeField]
	private EasyBgDownloaderCtl ebdCtl;
	[SerializeField]
	private MobileUIView mobileUIView;
	[SerializeField]
	private BrowseView browseView;
    
    private string currentDownloadURL = "";


	// Use this for initialization
	void Start () {
		ebdCtl.OnComplete = OnCompleteDownload;
        ebdCtl.OnError = OnErrorDownload2;
	}

    private void OnErrorDownload2(string requestURL, EasyBgDownloaderCtl.DOWNLOAD_ERROR errorCode, string errorMessage)
    {
        mobileUIView.ChangeHeaderLabel(errorMessage);

    }

    // Update is called once per frame
    void Update () {
       //update download progress
	   if (!string.IsNullOrEmpty(currentDownloadURL) && ebdCtl.IsRunning(currentDownloadURL)) {
           mobileUIView.ChangeHeaderLabel("DOWNLOADING");
           mobileUIView.ChangeDownloadingFileNameWithPath(currentDownloadURL);
           mobileUIView.ChangeProgress(ebdCtl.GetProgress(currentDownloadURL));
       }
	}


	/*
	 * Downloading Panel
	 */
	public void OnProgressChanged () {
		mobileUIView.ChangePercentageLabel (mobileUIView.progressManager.value);
	}
	// END : Downloading Panel

	/*
	 * Start&StopButton
	 */
	public void OnClickCtlButton () {
        string inputText = mobileUIView.GetInputText();
        if (string.IsNullOrEmpty(inputText)) {
            return;
        }
        
		if (mobileUIView.IsStartEnabled()) {
			OnClickStartBtn (inputText);
		} else {
			OnClickStopBtn (inputText);
		}
	}

	private void OnClickStartBtn (string inputURL) {
        currentDownloadURL = inputURL;
		mobileUIView.EnableStopButton ();
        ebdCtl.StartDL(inputURL);
	}

	private void OnClickStopBtn (string inputURL) {
        currentDownloadURL = null;
        mobileUIView.ChangeHeaderLabel("CANCELD");
        mobileUIView.ChangeDownloadingFileName("No file downloading");
        mobileUIView.ChangeProgress(0.0f);
		mobileUIView.EnableStartButton ();
        ebdCtl.StopDL(inputURL);
	}
	// END : Start&StopButton

	/*
	 * Footer Tab
	 */
	public void OnClickTabDownload () {
		if (!mobileUIView.IsOpenDownloadPage()) {
			mobileUIView.GoDownlaodPage ();
		}
	}
	public void OnClickTabBrowse () {
		if (mobileUIView.IsOpenDownloadPage()) {
			mobileUIView.GoBrowsePage ();
		}
	}
	// END : Footer Tab

	/*
	 * EBD Event
	 */
	public void OnCompleteDownload (string requestURL, string destPath) {
        browseView.RefreshFileList ();
        if (currentDownloadURL == requestURL) {
		    mobileUIView.EnableStartButton ();
            mobileUIView.ChangeDownloadingFileName("No file downloading");
            mobileUIView.ChangeHeaderLabel("COMPLETE");
            mobileUIView.ChangeProgress(0.0f);
        }
	}
    
    public void OnErrorDownload (string requestURL, string errorMessage) {
        mobileUIView.ChangeHeaderLabel(errorMessage);
    }
}
