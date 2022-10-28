# LIV for BONELAB

Adds limited [**LIV**](https://store.steampowered.com/app/755540/LIV/) support to Bonelab. Supports using LIV for controlling an extra camera (third person, selfie camera, smoothed first person, etc).

**Does not currently support Mixed Reality Capture or LIV avatars.**

## Installation

If you don't have MelonLoader already, install it first:

- Download the [Melonloader Installer](https://github.com/HerpDerpinstine/MelonLoader/releases/latest/download/MelonLoader.Installer.exe).
- Run the installer.
- Under the "Automated" tab, click the "Select" button next to "Unity Game:".
- Find your BONELAB exe (`BONELAB_Steam_Windows64.exe`). Be default this should be in `C:\Program Files (x86)\Steam\steamapps\common\BONELAB`.
- Click "Install".

![image](https://user-images.githubusercontent.com/3955124/198567268-47dc0f65-486a-4ecc-8323-0aadf4ffa5b9.png)

Now install the actual LIV mod:

- [Download the latest release zip](https://github.com/LIV/BonelabLIV/releases/latest/download/BonelabLIV.zip).
- Extract the zip contents to your MelonLoader mods folder. By default this should be in `BONELAB\Mods`.

## Usage

- Run the game with mods enabled. If you followed the instructions above to install MelonLoader, mods will be enabled if you start BONELAB as you normally would.
- Launch the LIV PCVR Avatars tool.
- Change capture to manual mode and select the BONELAB exe (`BONELAB_Steam_Windows64.exe`).

![image](https://user-images.githubusercontent.com/3955124/198566858-49ac6b4d-b907-4833-bd21-84355e3e242e.png)

## Adjusting the LIV cameras

BONELAB does this annoying thing where if the game gets unfocused (like if you open the SteamVR dashboard, or if you open the LIV menu), it will change the VR camera position. This makes it difficult to adjust the LIV cameras. I couldn't figure out a good way to automate this fix, so for now I just added a debug key that needs to be held while pausing the game.

- Outside VR, make sure the BONELAB window is focused (just click on the window).
- Press L on your keyboard. This will make the player body tracking stop working.
- Open LIV or the SteamVR dashboard.
- Once you're done, press L again to re-enable the player body behaviour.

## Known problems and reporting bugs

Check the [issues list](https://github.com/LIV/BonelabLIV/issues) for known bugs. Create a new issue if you run into problems.
