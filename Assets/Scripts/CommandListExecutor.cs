using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using System.Xml;
using System.Xml.Serialization;

public struct CommandSettings {
	public bool normalMapMaxStyle;
	public bool normalMapMayaStyle;
	
	public bool postProcessEnabled;
	
	public PropChannelMap propRed;
	public PropChannelMap propGreen;
	public PropChannelMap propBlue;
}

public struct CommandOpen {
	public string filePath;
	public MapType mapType;
}

public struct CommandSave {
	public string filePath;
	public MapType mapType;
}

public struct CommandAOFromNormal {
	public string settings;
}

public struct CommandEdgeFromNormal {
	public string settings;
}

public struct CommandFlipNormalMapY {
	//public bool flipNormalMapY;
}

public struct CommandFileFormat {
	public FileFormat fileFormat;
}

public enum CommandType {
	Settings,
	Open,
	Save,
	HeightFromDiffuse,
	NormalFromHeight,
	Metallic,
	Smoothness,
	AOFromNormal,
	EdgeFromNormal,
	QuickSave,
	FlipNormalMapY,
	FileFormat,
	Wait
}

public struct Command {

	//public string xmlCommand;
	public CommandType commandType;
	public string extension;
	public string filePath;
	public MapType mapType;
	public string settings;

	public Settings projectSettings;

}

public class CommandList {
	
	public List<Command> commands;
	
}

public class CommandListExecutor : MonoBehaviour {

	public GameObject mainGuiObject;
	MainGui mainGui;

	public GameObject saveLoadProjectObject;
	SaveLoadProject saveLoad;

	public GameObject heightFromDiffuseGuiObject;
	HeightFromDiffuseGui heightFromDiffuseGui;

	public GameObject normalFromHeightGuiObject;
	NormalFromHeightGui normalFromHeightGui;

	public GameObject metallicGuiObject;
	MetallicGui metallicGui;

	public GameObject smoothnessGuiObject;
	SmoothnessGui smoothnessGui;

	public GameObject aoFromNormalGuiObject;
	AOFromNormalGui aoFromNormalGui;

	public GameObject edgeFromNormalGuiObject;
	EdgeFromNormalGui edgeFromNormalGui;

	public GameObject materialGuiObject;
	MaterialGui materialGui;

	public SettingsGui settingsGui;

	// Use this for initialization
	void Start () {
		//string[] arguments = Environment.GetCommandLineArgs(); 
		mainGui = mainGuiObject.GetComponent<MainGui> ();
		saveLoad = saveLoadProjectObject.GetComponent<SaveLoadProject> ();

		heightFromDiffuseGui = heightFromDiffuseGuiObject.GetComponent<HeightFromDiffuseGui> ();
		normalFromHeightGui = normalFromHeightGuiObject.GetComponent<NormalFromHeightGui> ();
		metallicGui = metallicGuiObject.GetComponent<MetallicGui> ();
		smoothnessGui = smoothnessGuiObject.GetComponent<SmoothnessGui> ();
		aoFromNormalGui = aoFromNormalGuiObject.GetComponent<AOFromNormalGui> ();
		edgeFromNormalGui = edgeFromNormalGuiObject.GetComponent<EdgeFromNormalGui> ();

		materialGui = materialGuiObject.GetComponent<MaterialGui> ();

		StartCoroutine (StartCommandString());

	}

	/*
	int wait = 20;
	void Update(){
		wait -= 1;
		if (wait == 0) {
			SaveTestString ();
		}
	}
	*/

	IEnumerator StartCommandString(){
		yield return new WaitForSeconds (0.1f);
		string commandString = ClipboardHelper.clipBoard;
		if( commandString.Contains( "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" ) ){
			ProcessCommands ( commandString );
		}
	}

	public void SaveTestString() {

		CommandList commandList = new CommandList ();
		commandList.commands = new List<Command> ();

		Command command = new Command();
		command.commandType = CommandType.Settings;
		command.projectSettings = settingsGui.settings;
		commandList.commands.Add (command);

		command = new Command();
		command.commandType = CommandType.Open;
		command.filePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_diffuse.bmp";
		command.mapType = MapType.DiffuseOriginal;
		commandList.commands.Add (command);

		command = new Command();
		command.commandType = CommandType.Open;
		command.filePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_normal.bmp";
		command.mapType = MapType.Normal;
		commandList.commands.Add (command);

		command = new Command();
		command.commandType = CommandType.FlipNormalMapY;
		commandList.commands.Add (command);

		command = new Command();
		command.commandType = CommandType.AOFromNormal;
		commandList.commands.Add (command);

		command = new Command();
		command.commandType = CommandType.EdgeFromNormal;
		commandList.commands.Add (command);

		command = new Command();
		command.commandType = CommandType.FileFormat;
		command.extension = "tga";
		commandList.commands.Add (command);

		command = new Command();
		command.commandType = CommandType.QuickSave;
		command.filePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_property.bmp";
		commandList.commands.Add (command);


		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		var serializer = new XmlSerializer(typeof(CommandList));
		var stream = new System.IO.StringWriter( sb );
		serializer.Serialize(stream, commandList);
		ClipboardHelper.clipBoard = stream.ToString ();

		Debug.Log (stream.ToString ());
	}

	void OnApplicationFocus(bool focusStatus) {
		if( focusStatus ){
			string commandString = ClipboardHelper.clipBoard;
			if( commandString.Contains( "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" ) ){
				ProcessCommands ( commandString );
			}
		}
	}

	public void ProcessCommands () {
		string commandString = ClipboardHelper.clipBoard;
		if( commandString.Contains( "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" ) ){
			StartCoroutine ( ProcessCommandsCoroutine ( commandString ) );
		}
	}

	public void ProcessCommands ( string commandString ) {
		StartCoroutine ( ProcessCommandsCoroutine ( commandString ) );
	}

	IEnumerator ProcessCommandsCoroutine( string commandString ) {

		//string commandString = ClipboardHelper.clipBoard;
		
		var serializer = new XmlSerializer(typeof(CommandList));
		var stream = new System.IO.StringReader (commandString);
		CommandList commandList = serializer.Deserialize(stream) as CommandList;
		
		for (int i = 0; i < commandList.commands.Count; i++) {
			Command thisCommand = commandList.commands[i];
			if( thisCommand.commandType == CommandType.Settings ){
				settingsGui.settings = thisCommand.projectSettings;
				settingsGui.SetSettings ();
			}else if( thisCommand.commandType == CommandType.Open ){
				StartCoroutine ( saveLoad.LoadTexture( thisCommand.mapType, thisCommand.filePath ) );
				while( saveLoad.busy ){ yield return new WaitForSeconds (0.1f); }
			}else if( thisCommand.commandType == CommandType.Save ){
				switch( thisCommand.mapType ){
				case MapType.Height:
					StartCoroutine ( saveLoad.SaveTexture( thisCommand.extension, mainGui._HeightMap, thisCommand.filePath ) );
					break;
				case MapType.Diffuse:
					StartCoroutine ( saveLoad.SaveTexture( thisCommand.extension, mainGui._DiffuseMapOriginal, thisCommand.filePath ) );
					break;
				case MapType.Metallic:
					StartCoroutine ( saveLoad.SaveTexture( thisCommand.extension, mainGui._MetallicMap, thisCommand.filePath ) );
					break;
				case MapType.Smoothness:
					StartCoroutine ( saveLoad.SaveTexture( thisCommand.extension, mainGui._SmoothnessMap, thisCommand.filePath ) );
					break;
				case MapType.Edge:
					StartCoroutine ( saveLoad.SaveTexture( thisCommand.extension, mainGui._EdgeMap, thisCommand.filePath ) );
					break;
				case MapType.AO:
					StartCoroutine ( saveLoad.SaveTexture( thisCommand.extension, mainGui._AOMap, thisCommand.filePath ) );
					break;
				case MapType.Property:
					mainGui.ProcessPropertyMap();
					StartCoroutine ( saveLoad.SaveTexture( thisCommand.extension, mainGui._PropertyMap, thisCommand.filePath ) );
					break;
				default:
					break;
				}
				while( saveLoad.busy ){ yield return new WaitForSeconds (0.1f); }
			}else if( thisCommand.commandType == CommandType.FlipNormalMapY ){
				mainGui.FlipNormalMapY();
			}else if( thisCommand.commandType == CommandType.FileFormat ){
				mainGui.SetFormat(thisCommand.extension);
			}else if( thisCommand.commandType == CommandType.HeightFromDiffuse ){
				mainGui.CloseWindows();
				heightFromDiffuseGuiObject.SetActive(true);
				yield return new WaitForSeconds (0.1f);
				heightFromDiffuseGui.InitializeTextures();
				yield return new WaitForSeconds (0.1f);
				StartCoroutine( heightFromDiffuseGui.ProcessDiffuse() );
				while( heightFromDiffuseGui.busy ){ yield return new WaitForSeconds (0.1f); }
				StartCoroutine( heightFromDiffuseGui.ProcessHeight() );
				while( heightFromDiffuseGui.busy ){ yield return new WaitForSeconds (0.1f); }
				heightFromDiffuseGui.Close();
			}else if( thisCommand.commandType == CommandType.NormalFromHeight ){
				mainGui.CloseWindows();
				normalFromHeightGuiObject.SetActive(true);
				yield return new WaitForSeconds (0.1f);
				normalFromHeightGui.InitializeTextures();
				yield return new WaitForSeconds (0.1f);
				StartCoroutine( normalFromHeightGui.ProcessHeight() );
				while( normalFromHeightGui.busy ){ yield return new WaitForSeconds (0.1f); }
				StartCoroutine( normalFromHeightGui.ProcessNormal() );
				while( normalFromHeightGui.busy ){ yield return new WaitForSeconds (0.1f); }
				normalFromHeightGui.Close();
			}else if( thisCommand.commandType == CommandType.Metallic ){
				mainGui.CloseWindows();
				metallicGuiObject.SetActive(true);
				yield return new WaitForSeconds (0.1f);
				metallicGui.InitializeTextures();
				yield return new WaitForSeconds (0.1f);
				StartCoroutine( metallicGui.ProcessBlur() );
				while( metallicGui.busy ){ yield return new WaitForSeconds (0.1f); }
				StartCoroutine( metallicGui.ProcessMetallic() );
				while( metallicGui.busy ){ yield return new WaitForSeconds (0.1f); }
				metallicGui.Close();
			}else if( thisCommand.commandType == CommandType.Smoothness ){
				mainGui.CloseWindows();
				smoothnessGuiObject.SetActive(true);
				yield return new WaitForSeconds (0.1f);
				smoothnessGui.InitializeTextures();
				yield return new WaitForSeconds (0.1f);
				StartCoroutine( smoothnessGui.ProcessBlur() );
				while( smoothnessGui.busy ){ yield return new WaitForSeconds (0.1f); }
				StartCoroutine( smoothnessGui.ProcessSmoothness() );
				while( smoothnessGui.busy ){ yield return new WaitForSeconds (0.1f); }
				smoothnessGui.Close();
			}else if( thisCommand.commandType == CommandType.AOFromNormal ){
				mainGui.CloseWindows();
				aoFromNormalGuiObject.SetActive(true);
				yield return new WaitForSeconds (0.1f);
				aoFromNormalGui.InitializeTextures();
				yield return new WaitForSeconds (0.1f);
				StartCoroutine( aoFromNormalGui.ProcessNormalDepth() );
				while( aoFromNormalGui.busy ){ yield return new WaitForSeconds (0.1f); }
				StartCoroutine( aoFromNormalGui.ProcessAO() );
				while( aoFromNormalGui.busy ){ yield return new WaitForSeconds (0.1f); }
				aoFromNormalGui.Close();
			}else if( thisCommand.commandType == CommandType.EdgeFromNormal ){
				mainGui.CloseWindows();
				edgeFromNormalGuiObject.SetActive(true);
				yield return new WaitForSeconds (0.1f);
				edgeFromNormalGui.InitializeTextures();
				yield return new WaitForSeconds (0.1f);
				StartCoroutine( edgeFromNormalGui.ProcessNormal() );
				while( edgeFromNormalGui.busy ){ yield return new WaitForSeconds (0.1f); }
				StartCoroutine( edgeFromNormalGui.ProcessEdge() );
				while( edgeFromNormalGui.busy ){ yield return new WaitForSeconds (0.1f); }
				edgeFromNormalGui.Close();
			}else if( thisCommand.commandType == CommandType.QuickSave ){

				switch( thisCommand.mapType ){
				case MapType.Height:
					mainGui.QuicksavePathHeight = thisCommand.filePath;
					break;
				case MapType.Diffuse:
					mainGui.QuicksavePathDiffuse = thisCommand.filePath;
					break;
				case MapType.Normal:
					mainGui.QuicksavePathNormal = thisCommand.filePath;
					break;
				case MapType.Metallic:
					mainGui.QuicksavePathMetallic = thisCommand.filePath;
					break;
				case MapType.Smoothness:
					mainGui.QuicksavePathSmoothness = thisCommand.filePath;
					break;
				case MapType.Edge:
					mainGui.QuicksavePathEdge = thisCommand.filePath;
					break;
				case MapType.AO:
					mainGui.QuicksavePathAO = thisCommand.filePath;
					break;
				case MapType.Property:
					mainGui.QuicksavePathProperty = thisCommand.filePath;
					break;
				default:
					break;
				}
			}

			yield return new WaitForSeconds (0.1f);

			ClipboardHelper.clipBoard = "";
			
		}

		yield return new WaitForSeconds (0.1f);

		mainGui.CloseWindows();
		mainGui.FixSize();
		materialGuiObject.SetActive(true);
		materialGui.Initialize();
	}

}
