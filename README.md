# To-do list
* ~~Move connection logic away from individual pages and into the mainpage~~
* ~~Save/sync profiles from child page (Logic to save in parent)~~
* ~~Load profiles when selecting from the dropdown~~
* Split the profiles between Lighting and Layout

# Anne Keyboard Windows
A Universal Windows App for controlling the an Anne Pro keyboard over Bluetooth Low Energy.

## Requirements
This app has been tested against Windows 10. Other version of Windows is not supported due to Universal Windows App (UWP) platform only supporting Windows 10.
The keyboard has to be in L0 mode by: pressing ```Fn+B, Fn+0, ESC, Fn+B, +```

## Installation
* Enable Development mode on your Windows 10 machine by following the [instructions here](https://msdn.microsoft.com/windows/uwp/get-started/enable-your-device-for-development).
* Download the latest binary files from [Github](https://github.com/kprinssu/anne-keyboard-windows/releases). 
* Install the app by extracting the zip file, and right-clicking on ```Add-AppDevPackage.ps1``` and select ```Run with PowerShell```. 
* The app should now be installed and will be on Start menu.

## Supported Features
* Automatic keyboard pairing (launch the app, and it should start scanning for the keyboard)
* Create and manage profiles
* Set keyboard backlight colours

## Planned Features
* Support for changing keyboard layouts
* Implement keyboard macros
* Improve changing multiple button keyboard light colours

## Known Bugs
* None so far. 
Please submit an issue to report any bugs.

## License
The codebase and the project are released under the permissive MIT License. 
The images and icons are generated using the Font Awesome font released under the [OFIL license](http://scripts.sil.org/OFL), and the font can be found on [Github](http://scripts.sil.org/OFL). 
