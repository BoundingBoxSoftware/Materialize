# Materialize

Materialize is an open source application for generating property maps for games from Diffuse maps or Height maps.

The original website is at [http://www.boundingboxsoftware.com/materialize/](http://www.boundingboxsoftware.com/materialize/) and the original repository is at [https://github.com/BoundingBoxSoftware/Materialize](https://github.com/BoundingBoxSoftware/Materialize).

The original functionality as described on the website is for creating property maps:
```
Diffuse -> Height
Diffuse -> Metallic
Diffuse -> Smoothness
Height -> Normal
Height + Diffuse -> Normal
Normal -> Edge
Normal -> Occlusion
Normal + Height -> Occlusion
Normal -> Height
```

The main application is unmaintained, this fork provides the following enhancements:
- Upgraded Unity to version 2022.3 (the system is not compatible with Unity 6).
- Property maps can now include an Alpha channel as additional information.
- The default property map output is correct for Unity Terrain property maps.
