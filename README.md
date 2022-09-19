# CornModel
A Minecraft Model Reader for Unity written in C#.

## > About
__CornModel__ is Minecraft resource loader which can be easily merged into and used by other Minecraft-related Unity projects. It parses Minecraft: Java Edition's block json model format, reads block models and blockstate models from json files and builds them into geometry data. Face-culling data is contained in the loaded geometry data, so it is possible to build optimized large chunks of block meshes with all hidden faces culled.

## > Building & Running
The program is made and tested with Unity 2021.3.6f1c1, so it is recommended to use this version(or newer) of Unity to build this app.

You'll need to manually prepare a vanilla 1.16.5 resource pack and put them under the <code>Resource Packs</code> folder for the app to use. The path of resources should be like <code>\<Your Project Path\>\Resource Packs\vanilla-1.16.5\assets\XXX</code> (and of course this path can be changed in the code to anywhere you like).

When the resource pack is ready, you can then run the Python3 script called <code>block_atlas_gen.py</code> in the <code>Resource Packs</code> folder to generate block texture atlas for running the app.

## > License
Most code in this repository is open source under CDDL-1.0, and this license applies to all source code except those mention their author and license or with specific license attached.

The full CDDL-1.0 license can be reviewed [here](http://opensource.org/licenses/CDDL-1.0).

## > Screenshot
![Screenshot](https://github.com/DevBobcorn/CornModel/blob/master/Preview.png?raw=true)
