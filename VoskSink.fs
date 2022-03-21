module VoskSink

open System
open FM.LiveSwitch
open Vosk

type resultType = {
    conf      : float
    ``end``   : float
    start     : float
    word      : string
}

type VoskResult = {
    result    : resultType array
    text      : string
}

type VoskSink =
    inherit AudioSink

    new (model: Model) = {
        inherit AudioSink(new Pcm.Format(16000, 1))
        voskRecognizer = new VoskRecognizer(model, 16000f)
        textEvent = new Event<string>()
    }

    val voskRecognizer : VoskRecognizer
    val mutable textEvent : Event<string>

    member this.OnTextEvent = this.textEvent.Publish
    member this.RaiseTextEvent e = this.textEvent.Trigger e
    member this.GetResultFromJson (json : string) : VoskResult = System.Text.Json.JsonSerializer.Deserialize<VoskResult> json
    
    override this.Label : string = "Vosk Audio Transcriber"

    override this.DoDestroy () =
        let res = this.GetResultFromJson (this.voskRecognizer.Result())
        this.RaiseTextEvent res.text
        this.voskRecognizer.Dispose()
        ()

    override this.DoProcessFrame (frame: AudioFrame, buf: AudioBuffer) =
        let mutable result = false
        let dataBuf = buf.DataBuffer

        if dataBuf.Index = 0 then
            result <- this.voskRecognizer.AcceptWaveform(dataBuf.Data, dataBuf.Length)
        else
            let data = dataBuf.ToArray();
            result <- this.voskRecognizer.AcceptWaveform(data, data.Length)

        if result then
            let res = this.GetResultFromJson (this.voskRecognizer.Result())
            if not (String.IsNullOrWhiteSpace(res.text)) then
                this.RaiseTextEvent res.text
        