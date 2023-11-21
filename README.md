# GARWDashUploader
Command line program written in C# to add new dashscreens to your GARW device via wifi.

**Usage:**
From a command line run './GARWDashUploader PathToYourDashScreen.zip'

**Notes:**
Your .zip file must be named the same as your dash.qml file (it is case sensitive).  You also must include a 162x99 dash.qml.png file that is a screenshot of the dash which will show in the GARW settings screen.

**Once the dash is uploaded successfully follow these instructions to enable the dash on the GARW unit:**
Enter the settings of the GARW by pressing and holding RIGHT (from the mobile app).
Go into the dash settings (far left menu 'Driving Screens' and press UP).  Now press RIGHT twice to get to the "Up/Down to Browse Library" subscreen.  
Press UP until you get to the far right of the list and you should now see your new dash title with the PNG file screenshot that you added in the .zip file.
Now press RIGHT twice more until you get to "Up/Down to change active screen".  You can now change your available screens (bottom row) 
and toggle/select your new screen into a new or existing screen slot by pressing UP or DOWN (it will show you the screen active in the top row and change the picture in the bottom row to match).
Once it is selected, keep pressing RIGHT until you get to the "Main Menu" top label, and press UP to save and exit back to the main menu.
Hold RIGHT until you return to the main dash screen and press right or left until you see your new screen live!


