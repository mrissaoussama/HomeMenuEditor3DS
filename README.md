this is a tool that lets you move around 3DS titles on the home menu.
currently it supports moving titles in the main menu and from/to folders.
instructions:
- Get Launcher.dat and Savedata.dat from your console using FBI. to do this, go to "ext save data" and choose the correct home menu title id (mine is first), copy the savedata to your sd card.
- do the same for launcher.dat except go to "system save data" instead.
- optional: use https://gbatemp.net/threads/smdh-dumper-hbl-mod.398834/ to extract smdh files from installed titles and put them in icondata. there are already some icons there.
- put both dat files in the app directory and launch it
- BACK UP YOUR DAT FILES FIRST. although there is low chance this can cause damage, it's better to do this.
- you should see your home menu layout. click on a title to select it, then click on another title or an empty slot to swap.
- click on a folder to open its contents. you can swap titles here too.
- save and check the changes. now move the files back to your 3DS and copy them from the sd card using fbi. just go back to the previous directories and paste.
- restart your console and you should see the changes.
  ![image](https://github.com/user-attachments/assets/f0b4b870-1644-40cd-a051-6012a127b7d3)

   there could still be some bugs, some may be visual so save to see the final result.
plans:
- fix bugs
- add swapping between two folders
- create,move and delete folders (code is there but not fully tested)
- sort titles or group them in folders using a title database (by developer,genre ect)
- figure out how to install devkitpro and make it work with visual studio/visual studio code
- make a simpler homebrew version of this with sort/group presets and easier import/export of both dat files and smdh files.
