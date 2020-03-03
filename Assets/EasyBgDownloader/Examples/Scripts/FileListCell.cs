using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FileListCell : MonoBehaviour {
	[SerializeField]
	private Text fileNameLabel;
	[SerializeField]
	private Text sizeLabel;

	private float cellBaseSizeHeight;
	public float CellBaseSizeHeight {
		get {
			return Screen.height / 10;
		}
	}

	private RectTransform cachedRectTransform;
	public RectTransform CachedRectTransform {
		get {
			if(cachedRectTransform == null) { 
				cachedRectTransform = GetComponent<RectTransform>(); }
			return cachedRectTransform;
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public string FileNameLabel {
		set {
			this.fileNameLabel.text = value;
		}
		get {
			return this.fileNameLabel.text;
		}
	}

	public string SizeLabel {
		set {
			this.sizeLabel.text = value;
		}
		get {
			return this.sizeLabel.text;
		}
	}
}
