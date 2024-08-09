# Updating

This is a document describing the things to look out for when updating this plugin to a newer version of the game.

## Guidelines

The main functionality of this plugin is very simple, and it uses public APIs of Dalamud and FFXIVClientStructs. So if something broke, and it won't compile, check for changes upstream first.

All of the signatures and possibly-changing-constants are kept in `Constants.cs`, so that's where you should look when the plugin compiles but still doesn't work.
