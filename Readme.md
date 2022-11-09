# The King's Table (Legacy)
This was the original version of The King's Table, made in C# using Winforms.

This version has been abandoned in favor of [the Avalonia version](https://github.com/Brayconn/TheKingsTable) for performance reasons.
Turns out that Mono's Winforms support works... but it's *painfully* slow.
Heck, even on Windows it lags when resizing the program.

Also, the included appveyor config is broken.
Shouldn't take too much to fix it, but there's bigger build issues to solve first.


# Building
In the process of migrating to the Avalonia version, I horribly broke all the libraries this version of the program depends on.
At some point I will probably fix this version of the program to work with those new APIs, but that's an *extremely* low priority, so right now you're kinda out of luck...

In theory the project is just a regular Visual Studio solution file though, so once the API is fixed it should build pretty normally.

That said, it has a LOT of other projects you'll need to download too

- [LayeredPictureBox](https://github.com/Brayconn/LayeredPictureBox)
- [CaveStoryModdingFramework](https://github.com/Brayconn/CaveStoryModdingFramework)
- [PETools](https://github.com/Brayconn/PETools) 
  - Make sure you use my overhaul, and not the original broken/super old parent repo.
- [LocalizeableComponentModel](https://github.com/Brayconn/LocalizeableComponentModel)
- [WinFormsKeybinds](https://github.com/Brayconn/WinFormsKeybinds)

# Running

## Windows

It's just an exe...

## Mac

It's the same exe, but you have to run it using wine.

In *theory* it should work with mono,
but the winforms implementation on macOS is limited to 32bit,
and hasn't been updated in a long time, so I can't recommend it.

## Linux

It's still the same exe, just run it using mono.

If you aren't using the `mono-complete` package,
make sure you at least have `mono-locale-extras`,
otherwise TKT will crash because it can't find Shift-JIS.