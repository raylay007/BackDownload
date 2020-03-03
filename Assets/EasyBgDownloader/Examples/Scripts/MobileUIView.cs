using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MobileUIView : MonoBehaviour {
	[SerializeField]
	private Animator mobileUIAnimator;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	/*
	 * Header
	 */
	[HeaderAttribute ("Header")]
	[SerializeField]
	private Text headerLabel;

	public void ChangeHeaderLabel (string title) {
		headerLabel.text = title;
	}
	// END : Header


	/*
	 * Main Page
	 */
	//[HeaderAttribute ("Footer Tab")]
	public void GoDownlaodPage () {
		mobileUIAnimator.SetBool ("IsOpenDownload", true);	
	}

	public void GoBrowsePage () {
		mobileUIAnimator.SetBool ("IsOpenDownload", false);
	}

	public bool IsOpenDownloadPage () {
		return mobileUIAnimator.GetBool ("IsOpenDownload");
	}
	// END : Main Page


	/*
	 * Downloading Panel
	 */
	[HeaderAttribute ("Downloading Panel")]
	public Slider progressManager;
	[SerializeField]
	private Text percentageLabel;
	[SerializeField]
	private Text downloadingFileNameLabel;

	public void ChangeProgress (float progress) {
		this.progressManager.value = progress;
		//this.changePercentageLabel (progress);
	}

	public void ChangePercentageLabel (float progress) {
		int percentage = (int)(progress * 100.0f);
		percentageLabel.text = percentage.ToString () + "%";
	}

	public void ChangeDownloadingFileName (string fileName) {
		downloadingFileNameLabel.text = fileName;
	}

	public void ChangeDownloadingFileNameWithPath (string filePath) {
		string fileName = LocalFileManager.GetFileNameByFilePath (filePath);
		this.ChangeDownloadingFileName (fileName);
	}
		
	// END : Downloading Panel
    
    /*
	 * Text Feild
	 */
    [HeaderAttribute ("Text Field")]
    [SerializeField]
    private InputField textArea;
    
    public string GetInputText() {
        return textArea.text;
    }
    
    // END : Start&StopButton
    
	/*
	 * Start&StopButton
	 */
	[HeaderAttribute ("Start & Stop Button UI")]
	[SerializeField]
	private Text buttonLabel;

	public void EnableStartButton () {
		buttonLabel.text = "START";
		mobileUIAnimator.SetBool ("IsStartButton", true);
	}

	public void EnableStopButton () {
		buttonLabel.text = "STOP";
		mobileUIAnimator.SetBool ("IsStartButton", false);
	}

	public bool IsStartEnabled () {
		bool isStartBtn = mobileUIAnimator.GetBool ("IsStartButton");
		return isStartBtn;
	}
	// END : Start&StopButton

}
