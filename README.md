# LiveSwitch Transcription Micro Service
LiveSwitch WebRTC real time local transcription service written in FSharp F# using VOSK.

## About
Hobby project to understand audio pipelines within LiveSwitch's flexible client SDK. 

Lightweight console project that joins an existing LiveSwitch WebRTC conference or broadcast and transcribes the participants in real time. Application sends back the completed sentances to the channel using the signaling layer. **Console application requires participants use SFU (single forwarding unit) mode.**

## Setup
1. Download the VOSK models you wish to use from https://alphacephei.com/vosk/models
2. Rename the folder to model and have it copy to the bin folder. 
3. Edit the values in the configuration area at the top of Program.fs 
4. Run and enjoy.

You may want to setup your client application to display the incoming messages being sent out from this program. 

### Powered By

- LiveSwitch (https://www.liveswitch.io/)
- VOSK (https://alphacephei.com/vosk/)

### Notes
Not an official LiveSwitch product or offering within LiveSwitch. Do not contact them about supporting this project. 
