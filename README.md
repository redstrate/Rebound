# Rebound

FFXIV's bone physics consist of wind and bounce physics. These apply to the character's hair, clothes and other body parts. However, some aspects appear broken on high framerates, especially those above 60 FPS. Notably bouncing happens so quickly that it's practically non-existent, and wind effects tend to appear jittery. 

This plugin fixes this bug by locking updates to 60 FPS, and is based on [Kirrana's plugin](https://github.com/Kirrana/xivlauncher_physics_plugin/) to be more opinionated with less options.

## Download

This plugin is currently available in [my personal Dalamud repository](https://github.com/redstrate/DalamudPlugins).

## What FPS was the physics system designed for?

I initially thought the physics was meant to run at 30 FPS, because of PS3/PS4 console performance. However, the "bounce rate" applied to bones is "60.0" (float in BoneSimulator at offset `0x54`) and the physics generally look good at 60 FPS even when the game is running >200 FPS. It also avoids other issues like hair bangs clipping through the head because character positions are updated too infrequently. 

## What the plugin does

The plugin is extremely simple, and basically a pared down version of Kirrana's. If your current FPS is above 60, it will begin spreading out physics updates - basically locking it to a fixed rate. When the current FPS is below 60, the plugin effectively does nothing.

## Caveats and lessons learned

* `BoneSimulator::Update` is fundamentally broken. It's also amazing how it's stayed broken for this long. Don't expect to be able to fudge the numbers to make it work without skipping, it's simply impossible.
  * Surprisingly, the function limits the delta frame rate in numerous ways. I tried fudging the delta frame rate it gets from `Framework`, but it's not possible due to the bone simulator jobs running in parallel with other animation jobs. Basically, you'll end up with statically animated objects like flags running at a different speed tshan everything else.
* Clipping is rare but unfortunately unfixable. Due to the character updating at a different speed than the bones, it will happen.

## Credits

Thanks to [Kirrana](https://github.com/Kirrana) and the contributors for the [original plugin](https://github.com/Kirrana/xivlauncher_physics_plugin/).

## License

This project is licensed under the [GNU AFFERO General Public License 3](LICENSE). Some code or assets may be licensed differently.

