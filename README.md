# Materialize
Materialize is a program for converting images to materials.

# Releases
https://github.com/mitchelldmccollum/Materialize/releases

## Motivation
I decided to maintain Materialize for Linux, Windows, and soon Mac.

## Contact
For sugestions, doubts or anything related to this program.
Make an issue and we can talk about it.

## Using
To use, unity is not necessary, you can use like a normal application.

## Building
I'm developing using Unity 2019.1.9f1, it is recomended to use it if you are building it yourself. You can try to downgrade or upgrade with unity but it may break some of the code.

## Added features
### Paste Images from clipboard on Linux
- You can copy a file in your file browser (Tested with nautilus) and then press  the "P" close to the slot you want to paste.
- **Highlight** - You can also press copy image on browser and it will paste also. This make it fast to take a image from internet
### Hide Gui while Rotating / Panning
- The GUI is hidden when panning/rotating the material plane.
### Native File Picker
- Added a new native file/folder picker - Unity Standalone File Browser - https://github.com/gkngkc/UnityStandaloneFileBrowser - Thanks to @gkngkc for the amazing work.
 ### Batch Textures Mode
 - You can export multiple textures using the same settings.
 ### FPS Limiter
 - This will limit your fps to 30 60 or 120 for your high refresh rate monitors.
 
 ## Changed from original
### Save and Load Project
- When you save your project, every map will be saved in the same place, with there respective types, ex:myTexture_Diffuse.png.
- The extension used will be the one set in the GUI Panel.
#### Suported extensions
##### Save
- jpg
- png
- tga
- exr

##### Load
- jpg
- png
- tga
- exr
- bmp

## Future Feature List
- QuickSave - Will implement in settings, then you can set the folder to save the texture. This will be a persistent setting, that means you can close and open the program without lose the Quick Save path. *Planned for .41*
- Copy to clipboard.
- New UI*Planned for .50*
- Ability to bake AO into Diffuse Map *Planned for .40*
- Add Texture Presets for Unreal Unity and Cryengine *Planned for .41*
- Create update notification system *Planned for .40*
