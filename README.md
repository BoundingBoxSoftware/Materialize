# Materialize
Materialize is a program for converting images to materials for use in video games and whatnot
I decided to port materialize to linux and keep improving it.

## Using
To use, unity is not necessary, you can use like a normal linux application.

## Building
I'm developing using Unity 2018.3.3f, then, is recomended to use it also. You can try to downgrade or upgrade the package, but mainly downgrading, something can go wrong.

## Added features
### Paste Images from clipboard on Linux
- You can copy a file in your file browser (Tested with nautilus) and then press  the "P" close to the slot you want to paste.
- **Highlight** - You can also press copy image on browser and it will paste also. This make it fast to take a image from internet
### Hide Gui while Rotating / Panning
- The GUI is hidden when panning/rotating the material plane.
### Native File Picker
- Added a new native file/folder picker - Unity Standalone File Browser - https://github.com/gkngkc/UnityStandaloneFileBrowser - Thanks to @gkngkc for the amazing work.
 
## Changed from original
### Save and Load Project
- When you save your project, every map will be saved in the same place, with them respective types, ex:myTexture_Diffuse.png.
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

## Not implemented
- QuickSave - Will implement in settings, then you can set the folder to save the texture. This will be a persistent setting, that means you can close and open the program without lose the Quick Save path. *Planed for v0.4*.
- Copy to clipboard. *Planed for v0.4*.
