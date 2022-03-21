// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp
module Program

open System
open FM.LiveSwitch
open VoskSink
open Vosk

// You need to setup the VOSK models, they can be found at: https://alphacephei.com/vosk/models
// Replace these values before you get started.
type conf =
    static member Gateway      with get() : string = "https://cloud.liveswitch.io/"
    static member AppId        with get() : string  = ""
    static member SharedSecret with get() : string  = ""

let mutable voskModel : Model = null

let setupLogging() =
    FM.LiveSwitch.Log.RegisterProvider(FM.LiveSwitch.ConsoleLogProvider(FM.LiveSwitch.LogLevel.Debug));
    FM.LiveSwitch.Log.DefaultLogLevel = FM.LiveSwitch.LogLevel.Debug

let createClient (): Client =
    Client(conf.Gateway, conf.AppId, "Transcript AI", "Service")

let getToken (client: Client, channel : string) =
    Token.GenerateClientRegisterToken(client, [|ChannelClaim(channel)|], conf.SharedSecret)
    |> fun token -> (client, token)

let openSFUDownstreamConnection (connInfo: ConnectionInfo, channel: FM.LiveSwitch.Channel) : SfuDownstreamConnection =
    match connInfo.HasAudio with
        | true ->
            let audioSink = VoskSink(voskModel)
            audioSink.voskRecognizer.SetMaxAlternatives(0)
            audioSink.voskRecognizer.SetWords(true)
            audioSink.OnTextEvent.Add(fun s ->
                channel.SendMessage(sprintf "[%s]: %s" (if not (String.IsNullOrWhiteSpace(connInfo.UserAlias)) then connInfo.UserAlias else connInfo.UserId) s) |> ignore
            )
            let audioTrack : AudioTrack = AudioTrack(Opus.Depacketizer()).Next(Opus.Decoder()).Next(SoundConverter(AudioConfig(16000, 1))).Next(audioSink)
            let audiostream : AudioStream = if connInfo.HasAudio then AudioStream(null, audioTrack) else null
            let connection = channel.CreateSfuDownstreamConnection(connInfo, audiostream)
            connection.Open().Then(fun conn ->
                Log.Debug("Connection Opened")
            ).Fail(fun ex ->
                Log.Fatal("Failed to open  downstream.")
            ) |> ignore

            connection.add_OnStateChange(fun conn ->
                match conn.State with
                    | ConnectionState.Closing | ConnectionState.Failing -> audioSink.Destroy() |> ignore
                    | _ -> printfn "Connection State: %s" (conn.State.ToString())
                |> ignore
            )

            connection
        | false -> null

  

let recordChannel (channel: Channel) =
    channel.add_OnRemoteUpstreamConnectionOpen(Action1<ConnectionInfo>(fun remoteConnectionInfo ->
        let _ = openSFUDownstreamConnection(remoteConnectionInfo, channel)
        ()
    ))
    for remote in channel.RemoteUpstreamConnectionInfos do
        let _ = openSFUDownstreamConnection(remote, channel)
        ()
    0

let afterRegistered(channels: Channel array) =
    try
        channels |> Array.toList |> List.iter (fun channel ->
            recordChannel(channel) |> ignore
        ) |> ignore
    with
        | ArgumentNullException as ex -> printf "List was null"
        | ArgumentException as ex -> printf "List is empty in After Registered."
        | Exception as ex -> printf "Exception in After Registerd: %s" ex.Message

let registerClient (client: FM.LiveSwitch.Client, token: string) =
    client.Register(token).Then(fun channels -> 
        afterRegistered(channels)
    ).Fail(fun ex -> 
        printf "Exception: %s" ex.Message
        Log.Fatal("Register exception! ", ex)
    )

[<EntryPoint>]
let main argv =

    setupLogging() |> ignore

    voskModel <- new Model("model")

    printf "Please enter in the channel id: "
    let channel = Console.ReadLine()
    
    createClient()
     |> (fun client -> getToken (client, channel))
     |> registerClient |> ignore

    Console.ReadKey() |> ignore
    0 // return an integer exit code
