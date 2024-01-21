# DLCList Helper

A small console app that generates the dlclist.xml file (GTA 5 modding) based on the folders contained in the "dlcpacks" folder (original and "mods"). 
Simplifies the installation of add-on mods and the updating of the "mods" folder after a game update.

![Preview of the console app and the generated OIV](./preview.png)

The location of the game folder is detected automatically if the tool folder is :
- in the game's folder
- in the game folder's parent folder (next to the game folder)
- if GTA 5 is installed in the default location (Steam/Epic Games/Rockstar)

Otherwise (or if the detected location is incorrect) you will need to enter the location manually.