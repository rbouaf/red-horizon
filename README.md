
![demo_banner](https://github.com/rbouaf/red-horizon/blob/1fb778d324e037bdb3b86634f0ba00d188649ebe/demo_banner.png)
A Unity 3D Mars rover simulation using authentic NASA terrain data, featuring physics-based rover controls, AI-driven pathfinding, checkpoint navigation, geological sampling, and a real-time telemetry HUD. 

Software Engineering Project for COMP 361 @ McGill University
![demo_map](https://github.com/rbouaf/red-horizon/blob/1fb778d324e037bdb3b86634f0ba00d188649ebe/demo_map.png)
![demo_main](https://github.com/rbouaf/red-horizon/blob/1fb778d324e037bdb3b86634f0ba00d188649ebe/demo_main.png)
![demo_checkpoint](https://github.com/rbouaf/red-horizon/blob/1fb778d324e037bdb3b86634f0ba00d188649ebe/demo_checkpoint.png)
![demo_sample](https://github.com/rbouaf/red-horizon/blob/1fb778d324e037bdb3b86634f0ba00d188649ebe/demo_sample.png)

Run inside Unity's playtest mode for now, errors in the build would require 10-30 additional hours of debugging.
It should be set already but if not, the resolution is meant to be 910x512.

Controls
- `W` `A` `S` `D` or arrow keys to move arround in Free Roam mode. 
- Press `M` to switch location or quit to start menu.
- Use the mouse to interact with the UI.

From the start menu you can choose between 5 landing sites on Mars.

The terrain is generated from real Mars data from NASA (at https://trek.nasa.gov/mars/)
<br>Realistic physics and environment with real-time telemetry.

The maps feature interest points: These are checkpoints, once reached, an information pop-up containing interesting facts about that special location. 
<br> You can hide the info popup by clicking on the `Hide Checkpoint Info` button

You may select a different "brain". Pick from:
- **Free Roam**: default controls controlled manually by the player
- **PathFinding AI**: Will automatically move from checkpoint to checkpoint, using pathfinding to avoid obstacles.
- **Sampling AI**: Once activated you have access to the `Sample` button. This will scan the surrounding area's rocks to find rare minerals near the rover, each rock has a rarity score.
You go back to default controls by clicking on **Free Roam**. 

The rover has battery: the battery will decrease when moving, if you run out of battery, you can't move until the sun comes back to recharge your battery. Additionally, the solar panels collect dust over time, which decreases your charging speed, you must click the `Brush Solar Panels` button to restore them to max efficiency.

The day night cycle is sped up about 360 times.
