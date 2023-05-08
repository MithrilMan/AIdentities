# Deploy Instruction

To deploy this app in a standalone installer, we are using [Electron.Net](https://github.com/ElectronNET/Electron.NET) so please go there to obtain technical information about how the build and installation process works.

The .bat files included in this folder are used to build the Electron.Net setup packages, each one for its own target OS:

- **build-win.bat**
  
- **build-linux.bat**
  
- **build-osx.bat**
  builds the package for macOS 

  > *Note*  
  > (macOS builds can be produced on either Linux or macOS machines only)



## build-win.bat

This command builds the package for Windows.

The artifact is copied into the root repository folder in `/bin/Desktop`



## build-linux.bat

This command builds the package for Linux.

In order to build it, you have either to compile it on Linux or you can use docker with an image that take care of spinning a Linux container for that purpose 

```
docker pull electronuserland/builder
docker run --rm -ti -v <YOUR-PROJECT-ABSOLUTE-PATH-TO-ELECTRON-PROJECT>\:/project -w /project electronuserland/builder
```

Where `<YOUR-PROJECT-ABSOLUTE-PATH-TO-ELECTRON-PROJECT>` is the absolute path of the Electron project, in our case the `AIdentities.UI` project.

Then you need to log into the container and type the following to update the Electron package

```
yarn upgrade
yarn global add electron-builder
yarn global add @electron/asar
cd <YOUR-LINUX-OBJ-FOLDER>
electron-builder -l
```

Where `<YOUR-LINUX-OBJ-FOLDER>` is where build-linux.bat has generated the intermediate build (e.g. `/obj/desktop/linux` within your AIdentities.UI folder)

You should then find the compiled app (e.g. `a-identities.-ui-1.0.0.AppImage`) into the `dist` folder inside `<YOUR-LINUX-OBJ-FOLDER>` folder.



## build-osx.bat

This command builds the package for macOS.

This can only be run on a Mac or on Linux.

You could reuse the previous configured docker instance, changing the paths according and probably installing some more package (e.g. try with `@electron/osx-sign`)

> **Note**  
> I haven't tried yet to compile for Mac, please report back if you find this working or help fix this documentation.