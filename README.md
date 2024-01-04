# GARWDashUploader
Command line program written in C# to add new dashscreens to your GARW device via wifi.

**Usage:**
* Connect your laptop to the GARW wifi SSID after the ignition is turned on and the GARW device boots.
* From a command line run './GARWDashUploader --input PathToYourDashScreen.zip' to upload a new dash screen.

* You can also reset your GARW and remove all 3rd party screens using the '--purge' option, or reboot the device with the '--reboot' option.  None of these actions will affect the built in screens.


**Notes:**
* Your new dash design <_DASHNAME_>.zip file must be named the same as your <_DASHNAME_>.qml file (it is case sensitive).  You also must include a 162x99 <_DASHNAME_>.qml.png file that is a screenshot of the dash which will show in the GARW settings screen.  If you are following the standard dash screen design pattern, your <_DASHNAME_>_main.zip will contain 3 files and a folder of icons/assetts.  The first file will be named <_DASHNAME_>_main.qml which is the settings menu (when you press up on the GARW controller).  Your new screen design code will be named <_DASHNAME_>.qml, and your settings preview image file will be named <_DASHNAME_>_main.qml.png.


**Once the dash is uploaded successfully follow these instructions to enable the dash on the GARW unit:**
* Enter the settings of the GARW by pressing and holding RIGHT (from the mobile app).
* Go into the dash settings (far left menu 'Driving Screens' and press UP).  
* Now press RIGHT twice to get to the "Up/Down to Browse Library" subscreen.  
* Press UP until you get to the far right of the list and you should now see your new dash title with the PNG file screenshot that you added in the .zip file.
* Now press RIGHT twice more until you get to "Up/Down to change active screen".  You can now change your available screens (bottom row) 
and toggle/select your new screen into a new or existing screen slot by pressing UP or DOWN (it will show you the screen active in the top row and change the picture in the bottom row to match).
* Once it is selected, keep pressing RIGHT until you get to the "Main Menu" top label, and press UP to save and exit back to the main menu.
* Hold RIGHT until you return to the main dash screen and press right or left until you see your new screen live!


