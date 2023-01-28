# CornModel
A Minecraft Model Reader for Unity written in C#.

## > About
__CornModel__ is a Minecraft resource loader which can be easily merged into and used by other Minecraft-related Unity projects. It parses Minecraft: Java Edition's json model format, reads models from json files and builds them into geometry data. For block models, face-culling data is contained in the loaded geometry data, so it is possible to build optimized large chunks of block meshes with all hidden faces culled.

## > Building & Running
The program is made and tested with Unity 2022.2.4f1, so it is recommended to use this version(or newer) of Unity to build this app.

You'll need to manually prepare a vanilla 1.16.5 resource pack and put them under the <code>Resource Packs</code> folder for the app to use. The path of resources should be like <code>\<Your Project Path\>\Resource Packs\vanilla-1.16.5\assets\XXX</code> (and of course this path can be changed in the code to anywhere you like).

Then, add a <code>pack.mcmeta</code> in the resource folder. In this file you could simply write
```json
{
  "pack": {
    "pack_format": 6,
    "description": "Blah blah blah"
  }
}
```
and that'll do the trick. The value of <code>pack_format</code> is not used by CornCraft yet, so it doesn't matter here.

## > License
Most code in this repository is open source under CDDL-1.0, and this license applies to all source code except those mention their author and license or with specific license attached.

The full CDDL-1.0 license can be reviewed [here](http://opensource.org/licenses/CDDL-1.0).

## > Screenshots
![Screenshot 1](https://s2.loli.net/2022/10/24/8vzrXcRkGHWNI2L.png)
![Screenshot 2](https://s2.loli.net/2022/12/06/1A7fpaGYJtgKwsX.png)