# CornModel
A Minecraft Model Reader for Unity written in C#.

## > About
__CornModel__ is a Minecraft resource loader which can be easily merged into and used by other Minecraft-related Unity projects. It parses Minecraft: Java Edition's json model format, reads models from json files and builds them into geometry data. For block models, face-culling data is contained in the loaded geometry data, so it is possible to build optimized large chunks of block meshes with all hidden faces culled.

## > Building & Running
The program is made and tested with Unity 2022.3.3f1, so it is recommended to use this version(or newer) of Unity to build this app.

Resource files will now be automatically downloaded if they're not present, so manual downloading is no longer necessary.

## > License
Most code in this repository is open source under CDDL-1.0, and this license applies to all source code except those mention their author and license or with specific license attached.

The full CDDL-1.0 license can be reviewed [here](http://opensource.org/licenses/CDDL-1.0).

## > Screenshots
![Screenshot 1](https://s2.loli.net/2022/10/24/8vzrXcRkGHWNI2L.png)
![Screenshot 2](https://s2.loli.net/2022/12/06/1A7fpaGYJtgKwsX.png)