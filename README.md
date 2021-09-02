# Star Display
This is program to display acquired stars in casual playthrough! App reads game memory and depending on it displays Stars, Keys and 
Caps automatically. The level you are at is highlighted with yellow rectangle. 

# How to use?
Start Project64 1.6 (or any other version you like) and load ROM with hack. Download [Star Display](https://github.com/aglab2/SM64StarDisplay/releases/download/1.16/StarManager.exe), decompress zip file, go to unpacked folder. Execute StarDisplay.exe and wait for it to load. If your hack has layout support, it should load automatically. Else make your own layout, give it a try!

# Layouts
layouts are currently stored at [github](https://github.com/StarDisplayLayouts/layouts)

# Creating New Layouts
Your hack is not supported? You can easily create a layout yourself! Choose one of the current layouts as basis and edit it with _Settings > Configure Display_. Press on stars to hide/show them and press on text to edit it and switch line view! To get icons from hack you can load them with _Icons > Import Icons_.

# Speedrun
For speedrunners this program can be useful too! It can soft erase file A on ROM reset that is handy for reset-heavy runs!

# Star Display Sync
Star Display can sync your progress with other players online. You just need to get the [sync-server.exe](https://github.com/aglab2/SM64StarDisplay/blob/master/sync-server.exe?raw=true). On launch it will ask you for password that you will need to use in "Sync Online" menu. Server uses port 25565 that you should port forward if you want to host the server. Linux may also be used to host server.

# Features
 * Warp to any stage and any warp
 * Flexible configuration for background and general view
 * Support for multiple emulators: Project 64 any version, Mupen64
 * Add/Remove stars

# Credits
 * Original idea: FramePerfection
 * Beta testers: Ap616, katze789, Tomatobird8, OcarinaOfFan05, Sir_Flareon
 * Design tips: Ap616, Karisa113, Tomatobird8, CaptainYoshi64, TriforceTK
 * Layouts development: Mariocrash64, CaptainBowser, Sir_Flareon, TheGael95
 
# Notes
 * This code was used as a part of RHDC star display. It is more feature rich but also more bloated compared to this original code. Bugs in that version of Star Display or any UX issues may not be related to current release.
