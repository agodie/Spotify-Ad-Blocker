Yet Another EZBlocker Fork!
===========================

EZBlocker is a Spotify Ad Blocker written in C# for Windows 7/8/10/11. The goal for EZBlocker is to be the most reliable ad blocker for Spotify.

When an advertisement is playing, EZBlocker will mute Spotify until it's over.

YAEZBlocker is a fork to the original EZBlocker application, which aims at:

1. remove the Analytics framework to avoid sending data to undesired third-parties,
2. avoid cluttering the system, removing the following:

    * usage of registry keys to store application state,
    * saving of host file to System32 folder
    
3. Code clean up and build-system integration (CMake)

This is also my first (and likely only) C# project.
So one of the goals is to become more confident with the language and tools associated.

# How to build

Visual Studio with C# build-tools and CMake are required.

The build has been tested with:

* Visual Studio 2019 Community Edition, and
* CMake 3.22

To build the program, use CMake to generate a VS solution and then compile the associated project.
Using a console prompt:

* Go to `src` folder;
* Create and move to a `build` directory;
* generate a VS solution with CMake `cmake.exe .. -G "Visual Studio 16 2019"`;
* open the VS solution created;
* remove projects `ALL_BUILD` and `ZERO_CHECK` (*workaround is under study*)
* build the project

# Credits

This is fork to the original EZBlocker provided by [Xeroday](https://github.com/Xeroday/Spotify-Ad-Blocker).
