# LiveSwitch-Transcription-Service
LiveSwitch WebRTC local transcription service written in FSharp F# using VOSK. 

## About

Lightweight console project that joins an existing LiveSwitch conference or broadcast and transcribes the participants. Application then sends back the completed sentances to the channel using the signaling layer. **Console application requires participants use SFU (single forwarding unit) mode.**

## Setup

1. Download the VOSK models you wish to use from https://alphacephei.com/vosk/models
2. Rename the folder to model and have it copy to the bin folder. 
3. Edit the values in the configuration area at the top of Program.fs 
4. Run and enjoy.

You may want to setup your client application to display the incoming messages being sent out from this program. 

