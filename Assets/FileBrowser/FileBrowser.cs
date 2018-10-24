using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using System.Net;
using FreeImageAPI;
using System.Runtime.InteropServices;

using System.Linq;
using System.Diagnostics;

//===========================================================================
public class FileBrowser : MonoBehaviour {
	
	[DllImport ("FreeImage")]
	private static extern FIBITMAP FreeImage_Load( FREE_IMAGE_FORMAT fif, string filename, int flags );
	
	[DllImport ("FreeImage")]
	private static extern void FreeImage_Unload( FIBITMAP dib );
	
	[DllImport ("FreeImage")]
	private static extern bool FreeImage_Save( FREE_IMAGE_FORMAT fif, FIBITMAP dib, string filename, FREE_IMAGE_SAVE_FLAGS flags );
	
	[DllImport ("FreeImage")]
	private static extern int FreeImage_GetHeight( FIBITMAP dib );
	
	[DllImport ("FreeImage")]
	private static extern int FreeImage_GetWidth( FIBITMAP dib );
	
	[DllImport ("FreeImage")]
	private static extern bool FreeImage_GetPixelColor( FIBITMAP dib, int x, int y, RGBQUAD value );
	
	[DllImport ("FreeImage")]
	private static extern FIBITMAP FreeImage_MakeThumbnail( FIBITMAP dib, int max_pixel_size, bool convert );



	// DEBUG: Use this to help visualize changes when modifying a GUI skin for the File Browser on the fly
	private const bool REFRESH_STYLES_EVERY_FRAME = true;

	// Functional Customizations
	public bool   showPlaces  = true;
	public bool   dirExplorer = true;
	public bool   acceptsDirectories = false;
	public bool   enableFileDelete   = true;
	public bool   hideExtensions     = true;
	public bool   showTextInput      = true;	// Needed for write dialogs to create new files
	public bool   inputMustExist     = true;	// Only makes sense for read dialogs, which we'll assume is the default
	public bool   overwriteWarn      = true;    // Only checked if inputMustExist==false. Only makes sense for write dialogs
	public string fileMasks   = "*.png;*.jpg;*.jpeg";	// Comma or Semicolon separated
	public bool   showHiddenFiles    = false;
	
	public string   startingFileName = "";
	public Collider UIBlocker = null;

	// Only used for the short version of ShowBrowser. Defaults to CWD if not set
	public string defaultDir = "";

	public bool   rememberLastDir = true;

	// The default of this should be true, but I'm leaving at false to make it behave the same as the old versions
	public bool   relativizePaths = false;

	// Look & Feel Customizations
	public GUISkin   skin = null;
	public Texture2D itemHoverImage;
	public Color     itemHoverTextColor = _uninitColor;
	public Texture2D itemSelectedImage;
	public Color     itemSelectedTextColor = _uninitColor;

	public Texture2D dirImage;
	public Texture2D fileImage;
	public int       dirsHeight  = 22;
	public int       filesHeight = 52;
	
	public Texture2D placesHDIcon;
	public Texture2D placesDesktopIcon;
	public Texture2D placesMyDocsIcon;
	public Texture2D placesMyPicsIcon;
	public Texture2D placesFavoritesIcon;
	public Texture2D addFavoritesIcon;

	public Texture2D resizeHandleImage;

	public Rect browserRect = new Rect (300, 400, 600, 400);
	public Rect errorRect = new Rect (300, 400, 400, 300);

	public Rect newFolderRect = new Rect (300, 400, 400, 200);
	bool showingDialogue = false;

	bool createNewFolder = false;
	string newFolderName = "NewFolder";

	enum DialogueType {
		None = 0,
		Delete = 1,
		Overwrite = 2
	}

	DialogueType dialogueType = DialogueType.None;
	
	// Sound Previewer customizations
	public SoundViewer.SoundViewerConfig soundPreviewOptions;
	public int soundPreviewVertOffset = 150;
	
	// Called when the user clicks cancel or select (path will be null when cancel clicked)
	public delegate void FinishedCallback(string path);

	// Callback used to return results from a call to FileBrowser.LoadTexture()
	public delegate void TextureLoadedCallback(Texture2D texture);
	
	public bool Active {
		get { return this.isActive; }
	}
	
	// Returns true whenever there's ANY FileBrowser active somewhere
	public  static bool HasActive() { return FileBrowser._hasActive; }
	private static bool _hasActive = false;
	
	// When using custom styles, tweak these to help properly divide the horizontal boxes among the window space
	public int extraPadding       = 5;
	public int extraPaddingPlaces = 5;	  // Used only when the browser is showing the places column

	//bool showingError = false;

	//===========================================================================
	// Base FileBrowser show function
	public void ShowBrowser(string title, FinishedCallback callback, string startingDir, Rect screenRect, string okString)
	{
		if (this.rememberLastDir && this.lastOkDirectory != "") {
			startingDir = this.lastOkDirectory;
		}

		startingDir = startingDir.FixPathSeparators();
		if (!Directory.Exists(startingDir)) {
			UnityEngine.Debug.LogError("Starting FileBrowser directory " + startingDir + " does not exist");
			return;
		}
		
		this.title          = title;
		this.callback       = callback;
		this.newDirectory   = startingDir;
		this.screenRect     = screenRect;
		this.okButtonString = okString;
		
		this.currentDirectory = "";
		this.isActive = true;
		this.saveMode = false;
		this.saveScreenshot = null;
		
		if (this.inputMustExist) this.overwriteWarn = false;
		if (!this.dirExplorer) this.rememberLastDir = false;
		
		this.typedFilename = this.startingFileName;
		
		if (this.UIBlocker) this.UIBlocker.gameObject.SetActive(true);

		this.deleteSelected = false;
		this.SwitchDirectory();
	}

	//===========================================================================
	// Shows a file browser at current working dir, centered on screen occupying 2/3 of w and h (or more on low resolutions), Ok button string "Select"
	public void ShowBrowser(string title, FinishedCallback callback) {
		string dir = this.defaultDir == "" ? Directory.GetCurrentDirectory() : this.defaultDir.FixPathSeparators();

		if (firstUse) {
			screenRect = this.GetDefaultRect ();
			firstUse = false;
		}
		this.ShowBrowser(title, callback, dir, screenRect, "Select");
	}
	public void ShowBrowser(string title, FinishedCallback callback, string okString) {
		string dir = this.defaultDir == "" ? Directory.GetCurrentDirectory() : this.defaultDir.FixPathSeparators();

		if (firstUse) {
			screenRect = this.GetDefaultRect ();
			firstUse = false;
		}
		this.ShowBrowser(title, callback, dir, screenRect, okString);
	}

	//===========================================================================
	// Shows a save-game file Browser:
	// - no directory changes, hide extensions, show text input, png and jpeg filter, overwrite warns, ok button string "Save"
    // Takes a screenshot and saves it into the saveBaseDir with the user-specified name. If screenshot if null, takes a screenshot at half resolution
	// On ok calls the callback with a file with the same name, on the saveDataDir (which must be diferent then saveBaseDir)
	public void ShowSaveBrowser(string title, FinishedCallback callback, string saveBaseDir, string saveDataDir, Rect screenRect, Texture2D screenshot)
	{
		this.dirExplorer        = false;
		this.acceptsDirectories = false;
		this.hideExtensions     = false;
		this.showTextInput      = true;
		this.inputMustExist     = false;
		this.overwriteWarn      = true;
		this.fileMasks          = "*.png;*.jpg;*.jpeg;*.tga;*.bmp;*.tiff";
		
		saveBaseDir = saveBaseDir.FixPathSeparators();
		if (!Directory.Exists(saveBaseDir)) {
			Directory.CreateDirectory(saveBaseDir);
		}
		
		this.ShowBrowser(title, callback, saveBaseDir, screenRect, "Save");
		//this.saveMode       = true;
		this.saveMode       = false;
		this.savedDataDir   = saveDataDir.FixPathSeparators();
		
		this.saveScreenshot = screenshot;
		if (screenshot == null) {
			this.StartCoroutine(this.TakeHalfScreenshot());
		}
	}

	//===========================================================================
	// Simplified signature of ShowSaveBrowser. save dir is ./saves, save data is ./saves/data, auto takes the screenshot
	public void ShowSaveBrowser(string title, FinishedCallback callback)
	{
		string saveBaseDir = this.defaultDir == "" ? Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "saves" : this.defaultDir.FixPathSeparators();
		string saveDataDir = saveBaseDir + Path.DirectorySeparatorChar + "data";

		if (firstUse) {
			screenRect = this.GetDefaultRect ();
			firstUse = false;
		}
		this.ShowSaveBrowser(title, callback, saveBaseDir, saveDataDir, screenRect, null);
	}

	//===========================================================================
	// Shows a load-game file Browser:
	// - no directory changes, hide extensions, png and jpeg filter, file must exist
	// Calls the callback with a file with the same name, on the saveDataDir (which must be diferent then saveBaseDir)
	//
	public void ShowLoadBrowser(string title, FinishedCallback callback, string saveBaseDir, string saveDataDir, Rect screenRect)
	{
		this.dirExplorer        = false;
		this.acceptsDirectories = false;
		this.hideExtensions     = false;
		this.inputMustExist     = true;
		this.overwriteWarn      = false;
		this.fileMasks          = "*.png;*.jpg;*.jpeg;*.tga;*.bmp;*.tiff";
		
		saveBaseDir = saveBaseDir.FixPathSeparators();
		if (!Directory.Exists(saveBaseDir)) {
			Directory.CreateDirectory(saveBaseDir);
		}

		this.ShowBrowser(title, callback, saveBaseDir, screenRect, "Load");
		this.saveMode     = false;
		this.savedDataDir = saveDataDir;
	}

	//===========================================================================
	// Simplified signature of ShowLoadBrowser. save dir is ./saves, save data is ./saves/data
	public void ShowLoadBrowser(string title, FinishedCallback callback)
	{
		string saveBaseDir = this.defaultDir == "" ? Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "saves" : this.defaultDir.FixPathSeparators();
		string saveDataDir = saveBaseDir + Path.DirectorySeparatorChar + "data";

		if (firstUse) {
			screenRect = this.GetDefaultRect ();
			firstUse = false;
		}
		this.ShowLoadBrowser(title, callback, saveBaseDir, saveDataDir, screenRect);
	}

	//===========================================================================
	// Returns a rectangle centered in the screen, with 2/3 of the screen size
	// But with minimum size of 600x450 (or actual Screen size if it's lower than that)
	public static Rect GetDefaultRect_()
	{
		int w = Screen.width  / 3 * 2;
		int h = Screen.height / 3 * 2;
		w = Mathf.Clamp(w, _minWidth,  Screen.width);
		h = Mathf.Clamp(h, _minHeight, Screen.height);
		int x = (Screen.width  - w) / 2;
		int y = (Screen.height - h) / 2;
		return new Rect(x, y, w, h);
	}
	public Rect GetDefaultRect() { return FileBrowser.GetDefaultRect_(); }
	
	//===========================================================================
	public void RefreshStyles() {
		this.setupCustomStyles();

		for (int i = 0; i < this.filesWithImages.Length; i++) {
			string displayName = this.hideExtensions ? Path.GetFileNameWithoutExtension(this.files[i]) : this.files[i];
			this.filesWithImages[i].text = displayName;
		}
	}
	
	//===========================================================================
	public void RefreshContents() {
		this.newDirectory = this.currentDirectory;
		this.currentDirectory = "";
		this.SwitchDirectory();
	}

	//===========================================================================
	// Private Interface follows
	// You can safelly ignore everything from here on
	//===========================================================================
	
	//===========================================================================
	// Privates - We hold a lot of privates, to make fewer computations inside OnGUI

	// bools so gui can access key events
	private bool keyReturn = false;


	private static Color _uninitColor  = new Color(1, 0, 1, 0);
	private static int   _minWidth   = 800;
	private static int   _minHeight  = 480;
	private static int   _placesSize = 48;

	private GUIStyle styleListItem;
	private GUIStyle styleDirsTop;
	private GUIStyle styleDirsTopBut;
	private GUIStyle styleDirsBrowser;
	private GUIStyle styleFileBrowser;
	private GUIStyle stylePreviewer;
	private GUIStyle stylePreviewTitle;

	private Texture2D previewTexture;
	private string    previewText = "";

	private string    newDirectory;
	private string    currentDirectory;
	private string[]  currentDirectoryParts;
	private bool      currentDirectoryMatches;
	private string    lastOkDirectory = "";

	private string[]     files;
	private GUIContent[] filesWithImages;
	private int          selectedFile;
	private string       typedFilename = "";

	private string[]     directories;
	private GUIContent[] directoriesWithImages;
	private GUIContent[] directoriesWithImagesRemove;
	private int          selectedDirectory;

	private bool    isActive;
	private string  title;
	private Rect    screenRect;
	private bool	firstUse = true;
	private string  okButtonString = "Select";

	private bool      saveMode = false;
	private string    savedDataDir;
	private Texture2D saveScreenshot;

	private Vector2 scrollPosition1;
	private Vector2 scrollPosition2;
	private Vector2 scrollPosition3;

	private FinishedCallback callback;

	private string[] fileMaskList;
	private string[] drivesList;

	private SoundViewer soundPreviewer;
	
	// TODO: A Proper MessageBox class would come in handy instead of these
	private string userError = "";
	private bool   deleteSelected    = false;
	private bool   overwriteSelected = false;
	private string overwriteFileName = "";

	//===========================================================================
	// Helper struct to hold places (drives and special folders) data
	private struct PlacesData {
		public PlacesData(string _path, string _name, Texture2D _icon) {
			path = _path; name = _name; icon = _icon;
		}
		public string path;
		public string name;
		public Texture2D icon;
	}
	private PlacesData[] placesList;
	private GUIStyle     placesIconStyle;
	private GUIStyle     placesLabelStyle;
	
	private int availableSpace;
	private int windowPadding;

	//=========================================================================
	// Saves a half-res screenshot into member this.saveScreenshot. Sets a flag to ignore GUI draw this frame
	private bool __ignoreGUI = false;
	IEnumerator TakeHalfScreenshot()
	{
		this.__ignoreGUI = true;
		yield return new WaitForEndOfFrame();

        Texture2D fullScreenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, true, true);
        fullScreenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        this.__ignoreGUI = false;
		
		// Save with lower res to save disk space
		this.saveScreenshot = new Texture2D(Screen.width / 2, Screen.height / 2, TextureFormat.RGB24, false, true);
		this.saveScreenshot.SetPixels(fullScreenshot.GetPixels(1));
        this.saveScreenshot.Apply();

		Destroy(fullScreenshot);
		yield break;
	}

	//===========================================================================
	void Start()
	{
		// Fill out the list of places
		List<PlacesData> _placesList = new List<PlacesData>();
	
		// Adding a special "My Computer"-type dir, which we'll mark with the string %%ROOT%%,
		// to list all the available drives, in addition to the special folder, whitout adding a combo box anywhere
		_placesList.Add(new PlacesData("%%ROOT%%", "Drives", this.placesHDIcon));

		this.__AddSpecialFolder(Environment.SpecialFolder.Desktop,     "Desktop",  this.placesDesktopIcon, ref _placesList);
		this.__AddSpecialFolder(Environment.SpecialFolder.MyDocuments, "My Docs",  this.placesMyDocsIcon,  ref _placesList);
		this.__AddSpecialFolder(Environment.SpecialFolder.MyPictures,  "My Pics",  this.placesMyPicsIcon,  ref _placesList);
		//this.__AddSpecialFolder("Favorites",  "Favorites",  this.placesFavoritesIcon,  ref _placesList);

		this.placesList = new PlacesData[_placesList.Count];
		for (int i = 0; i < _placesList.Count; i++) {
			this.placesList[i] = _placesList[i];
		}
		
		this.drivesList = Environment.GetLogicalDrives();
		
		if (this.fileMasks == null) this.fileMasks = "*";

		if (this.UIBlocker) this.UIBlocker.gameObject.SetActive(false);

		this.soundPreviewer = this.gameObject.AddComponent<SoundViewer>();
		this.soundPreviewer.configs = this.soundPreviewOptions;
		this.soundPreviewer.Disable();
		this.soundPreviewer.Start();
	}
	
	//===========================================================================
	// If the folder exists on this machine, add it the the list of places
	private void __AddSpecialFolder(Environment.SpecialFolder folder, string name, Texture2D icon, ref List<PlacesData> placesList)
	{
		string dir = Environment.GetFolderPath(folder);
		if (dir != "") {
			placesList.Add(new PlacesData(dir, name, icon));
		}
	}

	string GetPathToFavorites(){
		string pathToFile = Application.dataPath;
		//string pathToFile = Application.persistentDataPath;
		if (Application.isEditor) {
			pathToFile = pathToFile + "\\favorites.txt";
		} else {
			pathToFile = pathToFile.Substring (0, pathToFile.Length - 16) + "favorites.txt";
		}
		return pathToFile;
	}

	List<string> GetFavorites(){
		List<string> allDirectories = new List<string>();

		string favoritesPath = GetPathToFavorites ();

		if (File.Exists (favoritesPath)) {
			StreamReader theReader = new StreamReader(favoritesPath, Encoding.Default);
			string line;
			do {
				line = theReader.ReadLine();
				if (line != null)
				{
					if( IsLegitDirectory(line) ){
						allDirectories.Add(line);
					}
				}
			}
			while (line != null);
			theReader.Close ();
		}else{
			File.Create(favoritesPath);
		}

		return allDirectories;
	}

	void SaveFavorites( List<string> allDirectories ){

		string favoritesPath = GetPathToFavorites ();

		if (!File.Exists (favoritesPath)) {
			File.Create(favoritesPath);
		}
		StreamWriter theWriter = new StreamWriter(favoritesPath);
		foreach (var dir in allDirectories) {
			theWriter.WriteLine (dir);
		}

		theWriter.Close();
	}

	void AddFavorite( string newFolder ){

		List<string> allDirectories = GetFavorites ();
		bool append = true;
		foreach(var dir in allDirectories) {
			if (dir == newFolder) {
				append = false;
			}
		}

		if( append == true ){
			allDirectories.Add (newFolder);
		}

		this.SaveFavorites (allDirectories);

	}

	bool IsLegitDirectory ( string dir ){
		if (dir != "Favorites" && dir != "%%ROOT%%") {
			if (Directory.Exists (dir)) {
				return true;
			}
		}
		return false;
	}

	void AddCurrentDirToFavorites() {

		if ( IsLegitDirectory(this.currentDirectory) ) {
			this.AddFavorite (this.currentDirectory);
			//this.RefreshContents ();
			//this.SwitchDirectory ();
		}
	}

	void RemoveFavorite( int dirToRemove ){
		List<string> allDirectories = GetFavorites ();
		allDirectories.RemoveAt (dirToRemove + 1);
		SaveFavorites (allDirectories);
		this.RefreshContents ();
	}
	
	//===========================================================================
	private void SwitchDirectory()
	{
		if (this.newDirectory == null || this.currentDirectory == this.newDirectory) {
			return;
		}
		
		if (this.newDirectory == "%%ROOT%%") {
			this.currentDirectory = this.newDirectory;
		} else if (this.newDirectory == "Favorites") {
			this.currentDirectory = this.newDirectory;
		} else {
			this.currentDirectory = Path.GetFullPath(this.newDirectory);
		}
		
		this.scrollPosition1   = Vector2.zero;
		this.scrollPosition2   = Vector2.zero;
		this.selectedDirectory = -1;
		this.selectedFile      = -1;
		this.userError = "";
		
		string[] directories;
		if (this.currentDirectory == "%%ROOT%%") {
			directories = this.drivesList.Clone () as string[];
		} else if (this.currentDirectory == "Favorites"){
			try {
				List<string> allDirectories = GetFavorites();
				List<string> dirsList = new List<string>();
				foreach(var dir in allDirectories) {
					bool isHidden = (File.GetAttributes(dir) & FileAttributes.Hidden) == FileAttributes.Hidden;
					if (!this.showHiddenFiles && isHidden) continue;
					dirsList.Add(dir);
				}
				directories = dirsList.ToArray();
			} catch(IOException) {
				this.userError = "Unable to access drive " + this.newDirectory;
				UnityEngine.Debug.LogWarning(this.userError);
				//TODO Keep this from repainting or whatever
				this.newDirectory = null;
				return;
			} catch(UnauthorizedAccessException) {
				this.userError = "Access denied to directory " + this.newDirectory;
				UnityEngine.Debug.LogWarning(this.userError);
				this.newDirectory = null;
				return;
			}
		} else {
			try {
				string[] allDirectories = Directory.GetDirectories(this.newDirectory);
				List<string> dirsList = new List<string>();
				foreach(var dir in allDirectories) {
					bool isHidden = (File.GetAttributes(dir) & FileAttributes.Hidden) == FileAttributes.Hidden;
					if (!this.showHiddenFiles && isHidden) continue;
					dirsList.Add(dir);
				}
				directories = dirsList.ToArray();
			} catch(IOException) {
				this.userError = "Unable to access drive " + this.newDirectory;
				UnityEngine.Debug.LogWarning(this.userError);
				//TODO Keep this from repainting or whatever
				this.newDirectory = null;
				return;
			} catch(UnauthorizedAccessException) {
				this.userError = "Access denied to directory " + this.newDirectory;
				UnityEngine.Debug.LogWarning(this.userError);
				this.newDirectory = null;
				return;
			}
		}
		
		// Needed to avoid leaking memory
		Resources.UnloadUnusedAssets();
		
		if (this.currentDirectory == "%%ROOT%%") {
			this.currentDirectoryParts = new string[] {"Drives"};
		} else if (this.currentDirectory == "/") {
			this.currentDirectoryParts = new string[] {""};
		} else if (this.currentDirectory == "Favorites") {
			this.currentDirectoryParts = new string[] {"Favorites"};
		} else {
			// Remove a trailing separator to avoid having a dangling directory part
			// But don't remove it from the currentDirectory string itself or it would break when getting entries of the root drive
			string path = this.currentDirectory;
			if (this.currentDirectory.EndsWith(Path.DirectorySeparatorChar.ToString())) {
				path = this.currentDirectory.Remove(this.currentDirectory.Length - 1);
			}
			this.currentDirectoryParts = path.Split(Path.DirectorySeparatorChar);
		}
		
		for (int i = 0; i < directories.Length; ++i) {
			if (this.currentDirectory == "%%ROOT%%") {
				directories [i] = directories [i].Replace (Path.DirectorySeparatorChar.ToString (), "");
			} else if (this.currentDirectory == "Favorites") {
				// do nothing
			} else {
				directories[i] = directories[i].Substring(directories[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
			}
		}
		Array.Sort(directories);

		// Adding '..' to the list of directories (if we are not at drive root)
		bool addParent = this.currentDirectoryParts.Length > 1;
		this.directories = new string[directories.Length + (addParent ? 1 : 0)];
		if (addParent) {
			this.directories[0] = "..";
		}
		for (int i = 0; i < directories.Length; ++i) {
			this.directories[i + (addParent ? 1 : 0)] = directories[i];
		}
		
		// Multiple file masks accepted
		this.fileMasks = this.fileMasks.Replace(',', ';');
		this.fileMaskList = this.fileMasks.Split(';');
		List<String> fileList = new List<String>();
		if (this.currentDirectory != "%%ROOT%%" && this.currentDirectory != "Favorites") {
			for (int i = 0; i < this.fileMaskList.Length; ++i) {
				fileList.AddRange(Directory.GetFiles(this.currentDirectory, this.fileMaskList[i]));
			}
		}
		this.files = fileList.ToArray();
		
		for (int i = 0; i < this.files.Length; ++i) {
			this.files[i] = Path.GetFileName(this.files[i]);
		}
		Array.Sort(this.files);

		this.newDirectory = null;
		
		// Create the folder and files GUIContent with the icons and strings
		this.StopAllCoroutines();
		this.directoriesWithImages = new GUIContent[this.directories.Length];
		for (int i = 0; i < this.directoriesWithImages.Length; i++) {
			this.directoriesWithImages[i] = new GUIContent(this.directories[i], dirImage);
		}

		this.filesWithImages = new GUIContent[this.files.Length];
		for (int i = 0; i < this.filesWithImages.Length; i++) {
			string displayName = this.hideExtensions ? Path.GetFileNameWithoutExtension(this.files[i]) : this.files[i];
			this.filesWithImages[i] = new GUIContent(displayName, fileImage);
		}
		
		this.availableSpace = -1;
		this.soundPreviewer.Disable();
		
		// Load icons in coroutines to avoid taking too long and lock the GUI
		this.StartCoroutine(this.loadIcons());	
	}

	//===========================================================================
	// Load images into the file icons themselves
	// Loading in sequence in a single coroutine instead of creating one for each icon is better

	string MakeNewThumb( string filename, string tempName, string fileLower, int size ){

		//string newFilename = Application.persistentDataPath + "/" + tempName + ".png";
		string newFilename = Application.dataPath + "/" + tempName + ".png";

		if (fileLower.EndsWith (".mtz")) {

			string imageFileName = filename.Substring(0, filename.LastIndexOf(".mtz" ) );

			if( File.Exists( imageFileName + "_diffuseOriginal.bmp" ) ){
				imageFileName += "_diffuseOriginal.bmp";
			}else if( File.Exists( imageFileName + "_diffuse.bmp" ) ){
				imageFileName += "_diffuse.bmp";
			}else if( File.Exists( imageFileName + "_diffuseOriginal.jpg" ) ){
				imageFileName += "_diffuseOriginal.jpg";
			}else if( File.Exists( imageFileName + "_diffuse.jpg" ) ){
				imageFileName += "_diffuse.jpg";
			}else if( File.Exists( imageFileName + "_diffuseOriginal.png" ) ){
				imageFileName += "_diffuseOriginal.png";
			}else if( File.Exists( imageFileName + "_diffuse.png" ) ){
				imageFileName += "_diffuse.png";
			}else if( File.Exists( imageFileName + "_diffuseOriginal.tga" ) ){
				imageFileName += "_diffuseOriginal.tga";
			}else if( File.Exists( imageFileName + "_diffuse.tga" ) ){
				imageFileName += "_diffuse.tga";
			}else if( File.Exists( imageFileName + "_diffuseOriginal.tif" ) ){
				imageFileName += "_diffuseOriginal.tif";
			}else if( File.Exists( imageFileName + "_diffuse.tif" ) ){
				imageFileName += "_diffuse.tif";
			}else if( File.Exists( imageFileName + "_diffuseOriginal.tiff" ) ){
				imageFileName += "_diffuseOriginal.tiff";
			}else if( File.Exists( imageFileName + "_diffuse.tiff" ) ){
				imageFileName += "_diffuse.tiff";
			}else{
				return "null";
			}

			filename = imageFileName;
			fileLower = imageFileName.ToLower();
		}

		try{
			FREE_IMAGE_FORMAT imageFormat = FREE_IMAGE_FORMAT.FIF_UNKNOWN;
			if( fileLower.EndsWith(".bmp") ){
				imageFormat = FREE_IMAGE_FORMAT.FIF_BMP;
			}else if( fileLower.EndsWith(".tga") ){
				imageFormat = FREE_IMAGE_FORMAT.FIF_TARGA;
			}else if( fileLower.EndsWith(".tif") || fileLower.EndsWith(".tiff") ){
				imageFormat = FREE_IMAGE_FORMAT.FIF_TIFF;
			}else if( fileLower.EndsWith(".png") ){
				imageFormat = FREE_IMAGE_FORMAT.FIF_PNG;
			}else if( fileLower.EndsWith(".jpg") || fileLower.EndsWith(".jpeg") ){
				imageFormat = FREE_IMAGE_FORMAT.FIF_JPEG;
			}else{
				imageFormat = FREE_IMAGE_FORMAT.FIF_UNKNOWN;
			}
			FIBITMAP fiBitmap = FreeImage_Load (imageFormat, filename, 0);
			FIBITMAP fiThumbnail = FreeImage_MakeThumbnail(fiBitmap, size, true);

			//newFilename = Application.dataPath + "/tempThumb.png";

			if( File.Exists( newFilename ) ){
				File.SetAttributes(newFilename, FileAttributes.Normal );
			}

			bool thumbnailSuccess = FreeImage_Save( FREE_IMAGE_FORMAT.FIF_PNG, fiThumbnail, newFilename, 0);

			FreeImage_Unload(fiBitmap);
			FreeImage_Unload(fiThumbnail);
		}
		
		catch (System.Exception e) {
			UnityEngine.Debug.Log( e );
			UnityEngine.Debug.Log( "Could not import with Free Image Importer" );
		}


		return newFilename;
	}

	IEnumerator loadIcons()
	{
		for (int i = 0; i < this.filesWithImages.Length; i++) {
			yield return null;
			Resources.UnloadUnusedAssets();
			
			string filename  = this.currentDirectory + Path.DirectorySeparatorChar + this.files[i];
			string fileLower = filename.ToLower();
			if (!fileLower.EndsWith(".jpg") && !fileLower.EndsWith(".jpeg") && !fileLower.EndsWith(".png") && !fileLower.EndsWith(".bmp") && !fileLower.EndsWith(".tga") && !fileLower.EndsWith(".tiff") && !fileLower.EndsWith(".tif") && !fileLower.EndsWith(".mtz") ) {
				continue;
			}

			UnityEngine.Debug.Log("Loading Icons");

			filename = MakeNewThumb( filename, "tempThumb", fileLower, 128 );

			if( filename != "null" ){

				WWW www = new WWW("file://" + filename);
				yield return www;
				
				GUIContent item = this.filesWithImages[i];
				Texture2D tex = new Texture2D(www.texture.width, www.texture.height, TextureFormat.ARGB32, false, true);
				www.LoadImageIntoTexture(tex);

				//File.Delete(filename);
				
				// Scale down to the file icons height
				float ratio = (float)tex.width / tex.height;
				int h = this.filesHeight;
				item.image = tex.ScaleTexture((int)(ratio * h), h);

			}else{
				yield return null;
			}

		}
		Resources.UnloadUnusedAssets();
	}

	//===========================================================================
	// Update checks for Delete pressings to update the delete flag, and arows and enter for keyboard navigation
	void Update()
	{
		if (!this.isActive) return;
	
		if (this.enableFileDelete && Input.GetKeyUp(KeyCode.Delete)) {
			CenterErrorWindow();
			if (this.currentDirectory == "Favorites") {
				if ( this.selectedDirectory > -1) {
						this.deleteSelected = true;
				}
			}else if (this.selectedFile > -1 || this.selectedDirectory > 0 ) {
				this.deleteSelected = true;
			}
		}

		if (Input.GetKeyUp (KeyCode.Return)) {
			this.keyReturn = true;
		} else {
			this.keyReturn = false;
		}

		/*
		if (this.keyReturn && this.selectedDirectory > -1) {
			this.DirectoryDoubleClickCallback(this.selectedDirectory);
			return;
		}
		if (this.keyReturn && this.selectedFile > -1) {
			this.FileDoubleClickCallback(this.selectedFile);
			return;
		}
		*/

		int movedIndex = 0;
		if (Input.GetKeyDown(KeyCode.UpArrow))   movedIndex--;
		if (Input.GetKeyDown(KeyCode.DownArrow)) movedIndex++;
		
		if (movedIndex == 0) return;
		
		if (this.selectedFile > -1) {
			this.selectedFile += movedIndex;
			this.selectedFile = Mathf.Clamp(this.selectedFile, 0, this.files.Length - 1);
			this.FileSingleClickCallback(this.selectedFile);
		}
		
		if (this.selectedDirectory > -1) {
			this.selectedDirectory += movedIndex;
		}
	}
	
	//===========================================================================
	private void setupCustomStyles()
	{
		// Use the "List Item" custom style if one is defined, or create one based on Label style if not
		this.styleListItem = this.skin.FindStyle("List Item");
		if (this.styleListItem == null) {
			this.styleListItem = new GUIStyle(this.skin.label);
		}
		this.styleListItem.alignment = TextAnchor.MiddleLeft;
		
		// Overwrite hover and toggled backgrounds and text colors if they are set
		if (this.itemHoverTextColor    == _uninitColor) this.itemHoverTextColor    = this.skin.label.hover.textColor;
		if (this.itemSelectedTextColor == _uninitColor) this.itemSelectedTextColor = this.skin.label.onNormal.textColor;

		if (this.itemHoverImage    != null) this.styleListItem.hover.background = this.itemHoverImage;
		if (this.itemSelectedImage != null) {
			this.styleListItem.onNormal.background = this.itemSelectedImage;
			this.styleListItem.onHover.background  = this.itemSelectedImage;
		}
		this.styleListItem.hover.textColor    = this.itemHoverTextColor;
		this.styleListItem.onNormal.textColor = this.itemSelectedTextColor;
		this.styleListItem.onHover.textColor  = this.itemSelectedTextColor;
		
		// Other custom styles
		this.styleDirsTop     = new GUIStyle(this.skin.label);
		this.styleDirsTopBut  = new GUIStyle(this.skin.button);
		this.stylePreviewer   = new GUIStyle(this.skin.label);
		this.styleDirsBrowser = new GUIStyle(this.styleListItem);
		this.styleFileBrowser = new GUIStyle(this.styleListItem);
		this.stylePreviewTitle= new GUIStyle(this.skin.label);

		this.styleDirsTop.alignment     = TextAnchor.MiddleCenter;
		this.styleDirsTopBut.alignment  = TextAnchor.MiddleCenter;
		this.stylePreviewTitle.alignment= TextAnchor.UpperCenter;
		this.stylePreviewer.alignment   = TextAnchor.MiddleCenter;

		this.styleDirsTop.wordWrap        = false;
		this.styleDirsTop.stretchWidth    = true;
		this.styleDirsTop.fixedHeight     = this.skin.button.fixedHeight;
		this.styleDirsTopBut.fixedHeight  = this.skin.button.fixedHeight;
		this.styleDirsBrowser.fixedHeight = this.dirsHeight;
		this.styleFileBrowser.fixedHeight = this.filesHeight;
		
		this.stylePreviewer.fixedHeight   = 0;
		this.stylePreviewer.fixedWidth    = 0;
		this.stylePreviewer.stretchHeight = true;
		this.stylePreviewer.stretchWidth  = true;
		
		this.styleDirsTopBut.padding = new RectOffset(10, 10, 5, 5);
		this.styleDirsTop.padding    = new RectOffset(5,  5,  5, 5);
		this.styleDirsTop.margin.bottom      = 10;
		this.styleDirsTopBut.margin.bottom   = 10;
		this.stylePreviewTitle.margin.bottom = 0;
		
		this.placesIconStyle  = new GUIStyle(this.skin.button);
		this.placesIconStyle.fixedHeight = _placesSize;
		this.placesIconStyle.fixedWidth  = _placesSize;
		this.placesIconStyle.alignment   = TextAnchor.MiddleCenter;
		
		this.placesLabelStyle = new GUIStyle(this.skin.label);
		this.placesLabelStyle.padding.top = 0;
		this.placesLabelStyle.alignment   = TextAnchor.UpperLeft;
		this.placesLabelStyle.fixedWidth  = 55;
	}
	
	//===========================================================================
	// Discounts GUI style paddings, and the places column
	private int _CalcAvailableWidth() {
		int winPad = (GUI.skin.window.padding.left - GUI.skin.window.overflow.left) * 2;
		int boxPad = (GUI.skin.box.padding.left    - GUI.skin.box.overflow.left) * 6;
		int placesSize = (int) GUI.skin.label.CalcSize(new GUIContent("My Docs")).x + this.extraPaddingPlaces;

		bool _showPlaces = this.showPlaces && this.dirExplorer;
		return (int) (this.screenRect.width - winPad - boxPad - (_showPlaces ? placesSize : 0) - this.extraPadding);
	}
		
	//===========================================================================
	void UpdateSoundPreviewRect() {
		float slice8 = this.availableSpace / (this.dirExplorer ? 8.0f : 6.0f);
		
		float w = slice8 * 3 - 30;
		float x = this.screenRect.x + this.screenRect.width - w - windowPadding - 15;
		float y = this.screenRect.y + this.soundPreviewVertOffset;
		float h = this.screenRect.height - (y - this.screenRect.y) * 2;
		this.soundPreviewer.viewRect = new Rect(x / Screen.width, y / Screen.height, w / Screen.width, h / Screen.height);
	}


	void GuiWindow( int windowID ){

		bool showingError = this.userError.Length > 1 || this.deleteSelected || this.createNewFolder;
		GUI.enabled = !showingError;

		GUILayout.Space (8);

		// Top directory parts
		if (this.dirExplorer) {
			GUILayout.BeginHorizontal ();
			for (int parentIndex = 0; parentIndex < this.currentDirectoryParts.Length; ++parentIndex) {
				if (parentIndex == this.currentDirectoryParts.Length - 1) {
					GUILayout.Label (this.currentDirectoryParts [parentIndex], this.styleDirsTop);
				} else if (GUILayout.Button (this.currentDirectoryParts [parentIndex], this.styleDirsTopBut)) {
					string parentDirectoryName = this.currentDirectory;
					for (int i = this.currentDirectoryParts.Length - 1; i > parentIndex; --i) {
						parentDirectoryName = Path.GetDirectoryName (parentDirectoryName);
					}
					this.newDirectory = parentDirectoryName;
				}
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}
		
		GUILayout.BeginHorizontal();
		
		// This holds 1/8 of the remaining size, discounting the places column and paddings
		// 1/6 if directory browser won't show. Discount extra width if the places column will show
		float slice8 = this.availableSpace / (this.dirExplorer ? 8.0f : 6.0f);
		
		int vBarPad = (int) GUI.skin.verticalScrollbar.fixedWidth;
		this.styleFileBrowser.fixedWidth = slice8 * 3 - vBarPad * 2;
		
		if (this.soundPreviewer.enabled) {
			this.UpdateSoundPreviewRect();
		}
		
		// Places (Drives and Special Folders)
		if (this.showPlaces && this.dirExplorer) {
			GUILayout.BeginVertical();
			for (int i = 0; i < this.placesList.Length; i++) {
				if (GUILayout.Button(this.placesList[i].icon, this.placesIconStyle)) {
					this.newDirectory = this.placesList[i].path;
				}
				GUILayout.Label (this.placesList[i].name, this.placesLabelStyle, GUILayout.MaxHeight(18));
			}

			// Favorites Icon
			if (GUILayout.Button(this.placesFavoritesIcon, this.placesIconStyle)) {
				this.newDirectory = "Favorites";
			}
			GUILayout.Label ("Favorites", this.placesLabelStyle, GUILayout.MaxHeight(18));

			// Add to Favorites Icon
			if (GUILayout.Button(this.addFavoritesIcon, this.placesIconStyle)) {
				AddCurrentDirToFavorites ();
			}
			GUILayout.Label ("Add To Favorites", this.placesLabelStyle, GUILayout.MaxHeight(18));

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}
		
		// Directory browser
		if (this.dirExplorer) {
			this.scrollPosition1 = GUILayout.BeginScrollView(
				this.scrollPosition1, false, false, this.skin.horizontalScrollbar, this.skin.verticalScrollbar,
				GUI.skin.box, GUILayout.Width(slice8 * 2)
			);
			if (this.currentDirectory == "Favorites") {
				this.selectedDirectory = GUILayoutx.SelectionList(
					this.selectedDirectory, this.directoriesWithImages, this.styleDirsBrowser,
					DirectoryDoubleClickCallback, null
				);
				if (this.selectedDirectory > -1) this.selectedFile = -1;
			} else {
				this.selectedDirectory = GUILayoutx.SelectionList(
					this.selectedDirectory, this.directoriesWithImages, this.styleDirsBrowser,
					DirectoryDoubleClickCallback, null
				);
				if (this.selectedDirectory > -1) this.selectedFile = -1;
			}
			GUILayout.EndScrollView();
		}


		// Files browser
		this.scrollPosition2 = GUILayout.BeginScrollView(
			this.scrollPosition2, false, false, this.skin.horizontalScrollbar, this.skin.verticalScrollbar,
			this.skin.box, GUILayout.Width(slice8 * 3)
			);
		if (this.filesWithImages.Length == 0) {
			GUILayout.Label("No entries", this.styleDirsTop, GUILayout.Width(slice8 * 3 - 30));
		} else {
			this.selectedFile = GUILayoutx.SelectionList(
				this.selectedFile, this.filesWithImages, this.styleFileBrowser,
				FileDoubleClickCallback, FileSingleClickCallback
				);
			if (this.selectedFile > -1) this.selectedDirectory = -1;
		}
		GUILayout.EndScrollView();
		
		GUILayout.FlexibleSpace();
		
		// Previewer
		int vBarW = (int)GUI.skin.verticalScrollbar.fixedWidth;
		int previewW = (int)(slice8 * 3 - vBarW);
		GUILayout.BeginVertical();
		if (this.selectedFile < 0) {
			string previewString = this.saveMode ? "" : "Nothing to preview";
			GUILayout.Label(previewString, this.stylePreviewTitle, GUILayout.Width(previewW));
			this.previewTexture = null;
		} else {
			string filename = this.files[this.selectedFile];
			#if UNITY_WEBPLAYER
			string filetime = "";
			#else
			string fullname = Path.Combine(this.currentDirectory, filename);
			string filetime = System.IO.File.GetLastWriteTime(fullname).ToString();
			string filesize = ((new System.IO.FileInfo(fullname)).Length / 1000).ToString();
			filename += " (" + filesize + " KB";
			if (this.previewTexture) {
				filename += ", " + this.previewTexture.width + " x " + this.previewTexture.height;
			}
			filename += ")";
			#endif
			
			if (this.soundPreviewer.enabled) {
				AudioClip clip = this.soundPreviewer.configs.aSource.clip;
				string channels = clip.channels == 1 ? "Mono, " : "Stereo, ";
				TimeSpan t = TimeSpan.FromSeconds(clip.length);
				string niceTime = string.Format("{0:D2}:{1:D2}.{2:D3}", t.Minutes, t.Seconds, t.Milliseconds);
				filename += "\n" + clip.frequency + " Hz, " + channels + niceTime;
			}
			
			GUILayout.Label(filename, this.stylePreviewTitle, GUILayout.Width(previewW));
			GUILayout.Label(filetime, this.stylePreviewTitle, GUILayout.Width(previewW));
			GUILayout.FlexibleSpace();
		}
		this.scrollPosition3 = GUILayout.BeginScrollView(
			this.scrollPosition3, this.skin.scrollView, GUILayout.Width(previewW + vBarW + 6)
			);
		if (this.saveMode && this.selectedFile < 0) {
			this.previewTexture = this.saveScreenshot;
		}

		
		if (this.previewTexture != null) {
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			float hFactor = (float)this.previewTexture.height / this.previewTexture.width;
			GUILayout.Label(
				this.previewTexture, this.stylePreviewer,
				GUILayout.Width(previewW), GUILayout.Height(previewW * hFactor)
				);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
		} else if (this.previewText.Length > 0) {
			GUILayout.TextArea(this.previewText);
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		
		GUILayout.EndHorizontal();
		
		// Bottom part: text input, ok / cancel
		GUILayout.BeginHorizontal();

		GUILayout.Space(60);
		if( GUILayout.Button("Paste Directory", GUILayout.Width(120))) {
			PasteDirCallback();
		}

		if (GUILayout.Button ("Create New Folder", GUILayout.Width (120))) {
			CenterNewFolderWindow();
			this.createNewFolder = true;
		}

		if (this.showTextInput) {
			GUILayout.Space(10);
			GUILayout.Label("Filename: ", GUILayout.Width(60));
			
			GUI.SetNextControlName("FilenameTextField");
			string newName = GUILayout.TextField(this.typedFilename);
			
			if (this.typedFilename != newName) {
				this.typedFilename = newName;
				this.selectedFile = -1;
				for (int i = 0; i < this.filesWithImages.Length; i++) {
					if (this.filesWithImages[i].text.ToLower() == newName.ToLower()) {
						this.selectedFile = i;
						this.FileSingleClickCallback(i);
					}
				}
			}
			GUILayout.Space(20);
		} else {
			GUILayout.FlexibleSpace();
		}
		
		if (GUILayout.Button("Cancel", GUILayout.Width(80))) {
			this.Close();
			this.callback(null);
		}
		
		bool dirEnables  = this.acceptsDirectories && this.typedFilename.Length == 0;
		bool fileEnables = this.selectedFile > -1 || (this.inputMustExist == false && this.typedFilename.Length > 0);
		GUI.enabled = (dirEnables || fileEnables) && !showingError;

		bool enterPressed = (Event.current.type == EventType.KeyDown && Event.current.character == '\n');

		// OK Button
		if (GUILayout.Button(this.okButtonString, GUILayout.Width(80)) || ( enterPressed && GUI.enabled ) ) {
			this.soundPreviewer.Disable();
			this.StartCoroutine(this.FileSelected());
		}
		GUI.enabled = true;
		GUILayout.Space(20);
		GUILayout.EndHorizontal();

		Rect dragRect = new Rect( 0, 0, this.screenRect.width, this.screenRect.height - 25 );
		GUI.DragWindow( dragRect );
		dragRect = new Rect( 0, 0, this.screenRect.width - 25, this.screenRect.height );
		GUI.DragWindow( dragRect );

		GUI.DrawTexture (new Rect (this.screenRect.width - 32, this.screenRect.height - 32, 32, 32), resizeHandleImage);
	}


	void ErrorWindow( int windowID){

		string text = this.userError;
		string filename = "";
		if(this.selectedFile > -1) {
			filename = Path.Combine(this.currentDirectory, this.files[this.selectedFile]);
		}

		string folderName = "";
		if(this.selectedDirectory > 0) {
			folderName = Path.Combine(this.currentDirectory, this.directories[this.selectedDirectory]);
		}

		if (this.overwriteSelected) filename = this.overwriteFileName;
		
		if (this.deleteSelected) {
			if (this.currentDirectory == "Favorites") {
				if (this.selectedDirectory > -1) {
					text = "Remove " + this.directoriesWithImages [this.selectedDirectory].text + " From Favorites?";
				} else {
					this.deleteSelected = false;
				}
			} else {
				if (this.selectedDirectory > 0) {
					text = "Delete " + this.directoriesWithImages [this.selectedDirectory].text + "?";
				} else if (this.selectedFile > -1) {
					text = "Delete " + this.filesWithImages [this.selectedFile].text + "?";
				} else {
					this.deleteSelected = false;
				}
			}

		}

		GUILayout.FlexibleSpace();
		GUILayout.Label(text, this.styleDirsTop);
		GUILayout.FlexibleSpace();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		if (!this.deleteSelected && !this.overwriteSelected) {
			if (GUILayout.Button("Ok", this.styleDirsTopBut, GUILayout.Width(80))) {
				this.userError = "";
			}
		} else {
			if (GUILayout.Button("Confirm", this.styleDirsTopBut, GUILayout.Width(80))) {

				if (this.currentDirectory == "Favorites") {
					RemoveFavorite (this.selectedDirectory);
				} else {
					if (this.selectedDirectory > 0) {
						//Directory.Delete (folderName);
						try{
							Process myProcess = new Process ();
							myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
							myProcess.StartInfo.CreateNoWindow = true;
							myProcess.StartInfo.UseShellExecute = false;
							myProcess.StartInfo.FileName = Application.streamingAssetsPath.Replace ("/", "\\") + "\\f2rb.exe";
							myProcess.StartInfo.Arguments = folderName.Replace ("/", "\\");
							myProcess.EnableRaisingEvents = true;
							myProcess.Start();
							myProcess.WaitForExit ();
						} catch (Exception e ){
							UnityEngine.Debug.Log (e);
						}
					} else if (this.selectedFile > -1) {
						//File.Delete (filename);
						try{
							Process myProcess = new Process ();
							myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
							myProcess.StartInfo.CreateNoWindow = true;
							myProcess.StartInfo.UseShellExecute = false;
							myProcess.StartInfo.FileName = Application.streamingAssetsPath.Replace ("/", "\\") + "\\f2rb.exe";
							myProcess.StartInfo.Arguments = filename.Replace ("/", "\\");
							myProcess.EnableRaisingEvents = true;
							myProcess.Start();
							myProcess.WaitForExit ();
						} catch (Exception e ){
							UnityEngine.Debug.Log (e);
						}
					}
				}

				if (this.deleteSelected) {
					this.RefreshContents();
				}

				this.deleteSelected    = false;
				this.overwriteSelected = true;
				this.userError = "";
			}
			if (GUILayout.Button("Cancel", this.styleDirsTopBut, GUILayout.Width(80))) {
				this.deleteSelected    = false;
				this.overwriteSelected = false;
				this.userError = "";
			}
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUI.DragWindow ();

	}

	void CreateFolder ( string folderPath, string folderName ){
		
		Directory.CreateDirectory (Path.Combine (folderPath, folderName));

	}

	void NewFolderWindow( int windowID ){

		string folderPath = this.currentDirectory;

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		newFolderName = GUILayout.TextField (newFolderName, GUILayout.Width(200));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Confirm", this.styleDirsTopBut, GUILayout.Width(80))) {
			CreateFolder( folderPath, newFolderName );
			newFolderName = "NewFolder";
			this.createNewFolder = false;
			this.RefreshContents();
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Cancel", this.styleDirsTopBut, GUILayout.Width(80))) {
			newFolderName = "NewFolder";
			this.createNewFolder = false;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUI.DragWindow ();
		
	}

	
	Vector3 mousePos;
	Vector3 clickedPosition;
	int minWindowWidth = 500;
	int minWindowHeight = 400;
	Rect originalWindowRect;
	bool allowWindowResize = true;
	bool handleClicked = false;

	//===========================================================================
	void OnGUI() {
		if (!this.Active || this.__ignoreGUI) return;
		FileBrowser._hasActive = true;
		
		if (this.availableSpace == -1 || REFRESH_STYLES_EVERY_FRAME) {
			this.availableSpace = this._CalcAvailableWidth();
			this.windowPadding  = GUI.skin.window.padding.left - GUI.skin.window.overflow.left;
		}
		
		// Use a custom skin if set, or the default one if not
		if (this.skin == null) {
			this.skin = GUI.skin;
			GUI.FocusControl("FilenameTextField");
		}

		GUI.skin = this.skin;
		if (this.styleListItem == null || REFRESH_STYLES_EVERY_FRAME) this.setupCustomStyles();

		this.screenRect = GUI.Window(100, this.screenRect, GuiWindow, this.title);

		// Resize window by dragging corner...must be done outside window code, or else mouse drag events outside the window are unrecognized
		//mousePos = GUIUtility.ScreenToGUIPoint(new Vector2( Input.mousePosition.x, Screen.height - Input.mousePosition.y ));
		mousePos = Event.current.mousePosition;
		if (allowWindowResize) {
				if (Event.current.type == EventType.MouseDown && new Rect(this.screenRect.width + this.screenRect.x - 25, this.screenRect.height + this.screenRect.y - 25, 25, 25).Contains (mousePos)) {
				handleClicked = true;
				clickedPosition = mousePos;
				originalWindowRect = this.screenRect;
			}
			if (handleClicked) {
				if (Event.current.type == EventType.MouseDrag) {
					this.screenRect.width = Mathf.Clamp (originalWindowRect.width + (mousePos.x - clickedPosition.x), minWindowWidth, 1600);
					this.screenRect.height = Mathf.Clamp (originalWindowRect.height + (mousePos.y - clickedPosition.y), minWindowHeight, 1200);
					//UpdateRects();
				}
				else if (Event.current.type == EventType.MouseUp) {
					handleClicked = false;
				}
			}
		}


		// Make new Folder
		if ( this.createNewFolder ) {
			newFolderRect = GUI.Window (101, newFolderRect, NewFolderWindow, "Create New Folder");
		}


		// Crude message box to display IO errors and delete/overwrite confirmation
		bool showingError = this.userError.Length > 1 || this.deleteSelected;
		if (showingError) {

			//errorRect.width = this.screenRect.width * 0.5f;
			//errorRect.height = this.screenRect.height * 0.5f;
			errorRect = GUI.Window (102, errorRect, ErrorWindow, "Error");
		}


		if (Event.current.type == EventType.Repaint) {
			this.SwitchDirectory();
		}
	}

	void CenterNewFolderWindow(){
		this.newFolderRect.center = this.screenRect.center;
	}

	void CenterErrorWindow(){
		this.errorRect.center = this.screenRect.center;
	}

	//===========================================================================
	// Checks for confirmation of overwriteWarn is true and the file already exists
	IEnumerator FileSelected()
	{
		string saveName     = "";
		string finalName    = "";
		string saveShotName = "";

		//UnityEngine.Debug.Log ("Is this save mode? " + this.saveMode);
		
		if (this.saveMode) {
			saveName = this.selectedFile > -1 ? this.filesWithImages[this.selectedFile].text : this.typedFilename;
			saveName = saveName.ReplaceInsensitive(".png", "").ReplaceInsensitive(".jpg", "").ReplaceInsensitive(".jpeg", "");
			
			saveShotName = Path.Combine(this.currentDirectory, saveName + ".png");
			finalName    = Path.Combine(this.savedDataDir,     saveName);
			
			saveShotName = Path.GetFullPath(saveShotName);
			
		} else {
			if (this.selectedFile > -1) {
				finalName = Path.Combine(this.currentDirectory, this.files[this.selectedFile]);
			} else if (this.typedFilename.Length > 0) {
				finalName = Path.Combine(this.currentDirectory, this.typedFilename);
			} else if (this.selectedDirectory > -1) {
				finalName = Path.Combine(this.currentDirectory, this.directories[this.selectedDirectory]);
			} else {
				finalName = this.currentDirectory;
			}
		}
		
		try {
			finalName = Path.GetFullPath(finalName);
		} catch (ArgumentException) {}
		
		if (this.overwriteWarn && (File.Exists(finalName) || File.Exists(saveShotName))) {
			this.overwriteSelected = true;
			this.overwriteFileName = finalName;
			this.userError = "Overwrite " + (this.saveMode ? saveName : finalName) + "?";
			while (this.userError.Length > 0) {		// Wait for user confirmation
				yield return null;
			}
			if (!this.overwriteSelected) yield break;
			this.overwriteSelected = false;
		}
		
		this.SelectConfirmed(finalName, saveShotName);
	}
	
	//--------------
	void SelectConfirmed(string finalName, string saveShotName)
	{
		if (this.saveMode && this.saveScreenshot) {
#if UNITY_WEBPLAYER
			UnityEngine.Debug.LogError("Webplayer does not support FileSystem write");
#else
			byte[] png = this.saveScreenshot.EncodeToPNG();
			File.WriteAllBytes(saveShotName, png);
#endif
		}

		//UnityEngine.Debug.Log ("Name Coming From File Borwser: " + finalName);
		
		this.lastOkDirectory = Path.GetDirectoryName(finalName);

		if (this.relativizePaths) RelativizatePath(ref finalName);
		this.callback(finalName);
		
		this.Close();
	}
	
	//===========================================================================
	void Close() {
		this.soundPreviewer.Disable();
		this.isActive = false;
		FileBrowser._hasActive = false;
		Resources.UnloadUnusedAssets();
		if (this.UIBlocker) this.UIBlocker.gameObject.SetActive(false);
	}
	
	//===========================================================================
	IEnumerator loadPreview(string filename)
	{
		string lower = filename.ToLower();
		
		bool isImage = lower.EndsWith(".jpg") || lower.EndsWith(".jpeg") || lower.EndsWith(".png") || lower.EndsWith(".tga") || lower.EndsWith(".bmp") || lower.EndsWith(".tif") || lower.EndsWith(".tiff");
		bool isSound = lower.EndsWith(".wav") || lower.EndsWith(".ogg");
		bool isMaterial = lower.EndsWith(".mtz");

		//string newFilename = Application.dataPath + "/tempPreview.png";

		// Load preview images
		if ( isMaterial || isImage ) {
			string newFilename = MakeNewThumb(filename, "tempPreview", lower, 512 );
			if( newFilename != "null" ){
				WWW www = new WWW("file://" + newFilename);
				yield return www;
				this.previewTexture = www.texture;
				//File.Delete(newFilename);
			}
			yield break;
		}

		// Load preview sounds
		if ( isSound ) {
			WWW www = new WWW("file://" + filename);
			yield return www;

			this.UpdateSoundPreviewRect();
			this.soundPreviewer.enabled = true;
			this.soundPreviewer.configs = this.soundPreviewOptions;
			this.soundPreviewer.PreviewClip(www.GetAudioClip(false, false));
			yield break;
		}
		
		// Check if we want to try previewing with text it anyway
		string[] texts = { "txt", "rtf", "doc", "xml", "html", "cs", "java", "py", "cpp", "c", "php", "ini", "reg", "odf", "ods", "meta" };
		string ext = Path.GetExtension(lower);
		bool doTextPreview = texts.Any(ext.Contains);
		
		if (!doTextPreview) {
			// If it's not too big and doesn't have a lot of \0's in it, go for it
			FileInfo fi = new FileInfo(filename);
			if (fi.Length < 1000000) {
				try {
					Byte[] bytes = File.ReadAllBytes(filename);
					doTextPreview = true;
					for (int i = 0; i < (bytes.Length - 2); i++) {
						if (bytes[i] == '\0' && bytes[i+1] == '\0') {
							doTextPreview = false;
							break;
						}
					}
				} catch (IOException) {
					doTextPreview = false;
				}
			}
		}

		this.previewText = "";
				
		// Preview everything else as text?
		if (doTextPreview) {
			try {
				StreamReader stream = new StreamReader(filename);
				Char[] buffer = new Char[2050];
			    stream.Read(buffer, 0, 2048);
			    stream.Close();
				this.previewText = new string(buffer);
			} catch (IOException) {
				this.previewText = "Unable to preview";
			}
		}
	}

	//===========================================================================
	private void FileSingleClickCallback(int i) {
		this.previewText     = "";
		this.previewTexture  = null;
		this.scrollPosition3 = Vector2.zero;
		this.soundPreviewer.Disable();
		
		this.typedFilename = this.files[i];
		
		this.StartCoroutine(loadPreview(Path.Combine(this.currentDirectory, this.files[i])));
	}
	
	//===========================================================================
	private void FileDoubleClickCallback(int i) {
		this.soundPreviewer.Disable();
		this.StopAllCoroutines();
		this.StartCoroutine(this.FileSelected());
	}

	//===========================================================================
	private void DirectoryDoubleClickCallback(int i) {
		this.newDirectory = Path.Combine(this.currentDirectory, this.directories[i]);
		this.newDirectory = Path.GetFullPath(this.newDirectory);
		this.SwitchDirectory();
	}

	//===========================================================================
	private void PasteDirCallback() {
		string newDir = ClipboardHelper.clipBoard;
		if (Path.HasExtension (newDir)) {
			newDir = Path.GetDirectoryName (newDir);
		}else if (!newDir.EndsWith (Path.DirectorySeparatorChar.ToString())) {
			newDir += Path.DirectorySeparatorChar;
		}
		this.newDirectory = newDir;
		this.SwitchDirectory();
	}
	
	//===========================================================================
	// Call this function to load the contents of the path image file into a new Texture2D
	// Assyncronous, returns the results calling a TextureLoadedCallback
	//===========================================================================
	public void GetTexture(string path, TextureLoadedCallback callback) {
		this.StartCoroutine(this.__GetTexture(path, callback));
	}
	
	//--------------
	IEnumerator __GetTexture(string path, TextureLoadedCallback callback)
	{
		path = Path.GetFullPath(path);
		
		WWW www = new WWW("file://" + path);
		yield return www;
		Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.ARGB32, false, true);
		texture.wrapMode = TextureWrapMode.Clamp;
		www.LoadImageIntoTexture(texture);
		
		callback(texture);
	}
	
	//=========================================================================
	// Removes the current working dir from path, making it relative to it
	public static string RelativizatePath(ref string path)
	{
		path = path.FixPathSeparators();
		string cwd  = System.IO.Directory.GetCurrentDirectory();
		path = path.Replace(cwd, ".");
		return path;
	}
}

//===========================================================================
// Changes all forward and backward slashes of this string to the current system's proper path separator
static public class FileBrowserExtensions
{
	//=========================================================================
	static public string FixPathSeparators(this string path)
	{
		string str = path.Replace('/',  Path.DirectorySeparatorChar);
		return       str .Replace('\\', Path.DirectorySeparatorChar);
	}
	
	//=========================================================================
	static public Texture2D ScaleTexture(this Texture2D source, int targetWidth, int targetHeight)
	{
		Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false, true);
		Color[]   rpixels = result.GetPixels(0);
		float incX = ((float)1 / source.width)  * ((float)source.width  / targetWidth);
		float incY = ((float)1 / source.height) * ((float)source.height / targetHeight);
		for(int px = 0; px < rpixels.Length; px++) {
			rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth),
							incY * ((float)Mathf.Floor(px / targetWidth)));
		}
		result.SetPixels(rpixels, 0);
		result.Apply();
		return result;
	}
	
	//=========================================================================
	static public string ReplaceInsensitive(this string str, string from, string to)
	{
		str = Regex.Replace(str, from, to, RegexOptions.IgnoreCase);
		return str;
	}
}

