using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;


public class BrowseView : MonoBehaviour {
	[SerializeField]
	private EasyBgDownloaderCtl ebdCtl;
	[SerializeField]
	private LocalFileManager localFileManager;
	[SerializeField]
	private Animator mobileUIAnimator;
	[SerializeField]
	private ScrollRect scrollRect;
	[SerializeField]
	private GameObject listCellBase;

	private FileInfo[] fileInfos;


	// Use this for initialization
	void Start () {
		this.listCellBase.SetActive (false);
		this.RefreshFileList ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}



	/*
	 * File List 
	 */
	public void RefreshFileList () {
		fileInfos = LocalFileManager.GetAllFilesAtPath (ebdCtl.GetDestinationDirPath ());

		//Show No file text
		if (fileInfos.Length == 0) {
			this.ShowNoFile ();
			return;
		}

		//Delete All List
		foreach (Transform listCell in listCellBase.gameObject.transform.parent.transform) {
			if (listCell.gameObject.name == "ChildCell") {
				GameObject.Destroy (listCell.gameObject);
			}
		}
			
		Vector2 scrollRectSizeDelta = new Vector2(0.0f, 0.0f);

		//Update List View
		for (int i = 0; i < fileInfos.Length; i++) {
			this.AddCell (fileInfos[i], i);

			//Update ScrollRect Height
			scrollRectSizeDelta.y += Screen.height / 10;
		}

		scrollRect.content.sizeDelta = scrollRectSizeDelta;

		//Show List
		this.ShowListView ();
	}

	public void ShowNoFile () {
		mobileUIAnimator.SetBool ("IsNoFile", true);
	}

	public void ShowListView () {
		mobileUIAnimator.SetBool ("IsNoFile", false);
	}

	private void AddCell (FileInfo fileInfo, int index) {
		GameObject newObj = Object.Instantiate (listCellBase) as GameObject;
		newObj.SetActive (true);
		FileListCell newCell = newObj.GetComponent<FileListCell> ();

		Vector3 scale = newCell.transform.localScale;
		Vector2 sizeDelta = newCell.CachedRectTransform.sizeDelta;
		Vector2 offsetMax = newCell.CachedRectTransform.offsetMax;
		Vector2 offsetMin = newCell.CachedRectTransform.offsetMin;
		newCell.transform.SetParent (listCellBase.transform.parent);

		offsetMin.y = newCell.CellBaseSizeHeight * (-(index + 1));
		offsetMax.y = newCell.CellBaseSizeHeight * (-index);

		newCell.transform.localScale = scale;
		newCell.CachedRectTransform.sizeDelta = sizeDelta;
		newCell.CachedRectTransform.offsetMax = offsetMax;
		newCell.CachedRectTransform.offsetMin = offsetMin;
		newCell.gameObject.name = "ChildCell";

		//Set file info
		newCell.FileNameLabel = fileInfo.Name;
		newCell.SizeLabel = GetDataSizeLabel (fileInfo.Length);
	}

	private static string GetDataSizeLabel (long byteSize) {
		if (byteSize < 1000) {
			return byteSize + " B";
		}
		long kbSize = byteSize / 1000;
		if (kbSize < 1000) {
			return kbSize + " KB";;
		}
		long mbSize = kbSize / 1000;
		if (mbSize < 1000) {
			return mbSize + " MB";
		}
		long gbSize = mbSize / 1000;
		if (gbSize < 1000) {
			return gbSize + " GB";
		}
		long tbSize = gbSize / 1000;
		if (tbSize < 1000) {
			return tbSize + " TB";
		} else {
			return "Too Big";
		}
	}
}
