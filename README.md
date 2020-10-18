# New Brown Corpus Unity Scripts

Data collection and utility scripts in Unity. The full Unity project isn't provided, as it contains paid assets. These scripts are likely to stop working out-of-the-box as SteamVR and Unity continue to evolve, but the idea behind the implementation is pretty simple and (hopefully) easy to adapt.

## Setup
- Create a scene using SteamVR v2
- Tag moving objects "Dynamic", and static objects "Static"
- To each of these objects, add Tracked.cs
- Create a GameController object and add DataCollection.cs
- Set the 'Head' public variabe in DataCollection to the SteamVR Head
- Modify "/path/to/data.txt" in DataCollection.cs to your desired path

## Recording
- Press R to start/stop recording. Spatial data of each object should be written in real-time
- To record images, add Playback.cs to your GameController
- Set "/path/to/images/" and "/path/to/data.txt" in Playback.cs to your desired paths
- Create a dummy player/camera setup for playback
- Set the 'playbackCam' public variable to the duplicate camera
- Set the 'player' public variable as the original SteamVR player object
- On play, the originally recorded object data should be replicated
- Enable the 'record' public variable to save playback frames to the previously specified path

This playback-based image saving protocol is due to the high overhead of recording video realtime in VR. However, this may not be an issue in the future, in which case it may be easier to use realtime screen capture.
