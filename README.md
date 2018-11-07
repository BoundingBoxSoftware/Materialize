# Materialize
Materialize is a program for converting images to materials for use in video games and whatnot
I made a port to use ImageSharp instead of c based FreeImage used in original Materialize, since this will make it more cross platform.

## ATTENTION: Requires Unity 2018.3 due to csharp 7.x used in ImageSharp

## Issues
Thumbnails in file manager are slow, it's the price for changing a c native api to a c# one (Maybe some optimization can be made).
