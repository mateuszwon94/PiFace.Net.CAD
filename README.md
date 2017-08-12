# **PiFace.Net.CAD** #
--------------
**[UNOFFICIAL]** PiFace Control and Display .Net wrapper. Create by rewriting [libpifacecad](https://github.com/piface/libpifacecad). Based on [libmcp23s17](https://github.com/piface/libmcp23s17).

Works only on linux on Raspberry Pi. Testet only on Raspbarian on Raspberry 3B. 
All steps needed to be done to run app which use this library are described below.

## Done ##
 - Rewrite all LCD function from C library.
    - User can clear screen, enable and disable display, cursor, blinking cursor, backlight, autoscroll, change entry mode, manipulate cursor position, write text or custom bitmap and also store it.

## To Be Done ##
 - Implement events handling for clicking and holding buttons and change switch position.
 - If possible implement IR support.

## Examples ##
Examples will be attached to repo soon.

## Build app with .Net Core ##
 1. If you do not have [.Net Core 2.0](https://www.microsoft.com/net/core/preview#windowscmd) yet, install it for your platform.
 2. Create new console project `dotnet new colsole`.
 3. Add nuget package [PiFace.Net.CAD](https://www.nuget.org/packages/PiFace.Net.CAD/) `dotnet add package PiFace.Net.CAD`, and restore project `dotnet restore`.
 4. Write some code.
 5. Publish app for Linux enviroment `dotnet publish -r linux-arm` for debug, and `dotnet publish -r linux-arm -c Release` for release configuration.
 6. In folder `./bin/<Configuration>/netcoreapp2.0/linux-arm/publish` you will find app with almost all dependences. (Only one which you must provide by yourself is `libmcp23s17.so`)
 7. Copy `publish` folder to your Raspberry Pi 2/3 memory.

## Prepare enviroment on Raspberry Pi ##
Following steps need to be done only once.

 1. Install [.NET Core Native prerequisites](https://github.com/dotnet/core/blob/master/Documentation/prereqs.md) for your OS. On Raspbarrian you should use following command: `sudo apt-get install libunwind8 libunwind8-dev gettext libicu-dev liblttng-ust-dev libcurl4-openssl-dev libssl-dev uuid-dev unzip`. 
 2. Beeing in youre home directory execute `git clone https://github.com/piface/libmcp23s17` and then `cd libmcp23s17`.
 3. Using any text editor in third line of `Makefile` change `static` to `shared`. It should look like `LIBRARY=shared`.
 4. Compile and install libmcp23s17 by using `make && sudo make install`.
 5. You can remove libmcp23s17 directory.

## Run app on Raspberrry Pi ##
 1. From `/usr/local/lib` copy `libmcp23s17.so` to your app folder.
 2. Enter your app folder and then type `chmod 755 ./<your app name>`.
 3. Execute program by typing `./<your app name>`.


