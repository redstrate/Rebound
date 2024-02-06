# FFXIV Physics Fix

FFXIV's bone physics - which apply to character's hair, clothes and other body parts - are for some reason tied to the frame rate. If you run FFXIV above 30/60 FPS, the physics run so quickly that the bones only move slightly. In other words: the lower the frame rate the more "responsive" the physics look. This plugin aims to fix that, based on [Kirrana's plugin](https://github.com/Kirrana/xivlauncher_physics_plugin/) to be more opinionated with less options. This plugin locks the update at 48 FPS.

## What FPS was the physics system designed for?

Since the physics are tied to the frame rate, it's hard to tell what is the "intended look". However I'm assuming the current system is designed to work <60 FPS and ideally at 30 FPS. The reasoning is the PS3 version barely reached 30, and the PS4 version barely reached 60.

## What the plugin does

The plugin is extremely simple, and basically a pared down version of the original. If your current FPS is above 48, it will begin spreading out physics updates - basically locking it to a fixed rate. When the current FPS is below 48, the plugin effectively does nothing. Running physics at 30 looks really bad at really high refresh rates like 144hz (which my monitor uses.) I find updating 48 FPS to be a nice sweet spot as it's cleanly divisible by 144, and it doesn't look too glaringly obvious.

## Credits

Thanks to [Kirrana](https://github.com/Kirrana) and the contributors for the [original plugin](https://github.com/Kirrana/xivlauncher_physics_plugin/).

## License

This project is licensed under the [GNU AFFERO General Public License 3](LICENSE). Some code or assets may be licensed differently.

