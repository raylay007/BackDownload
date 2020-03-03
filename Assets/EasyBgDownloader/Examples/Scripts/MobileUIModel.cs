using UnityEngine;
using System.Collections;
using System.IO;


public class MobileUIModel : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	/*
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}
	*/

	//Get all file infos
	public static FileInfo[] GetAllFilesAtPath (string dirPath) {
		DirectoryInfo directoryInfo = new DirectoryInfo (dirPath);
		FileInfo[] files = directoryInfo.GetFiles ();
		return files;
	}

	//Get file name with path
	public static string GetFileNameByFilePath (string filePath) {
		return Path.GetFileName (filePath);
	}

	//Get file surfix
	public static string GetFileSuffixByFileName (string fileName) {
		return Path.GetExtension (fileName);
	}
}
