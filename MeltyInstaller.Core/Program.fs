module MeltyInstaller.Core.Program

open System
open System.IO
open System.Windows
open FSharp.Data
open Ookii.Dialogs.Wpf
open Elmish
open Elmish.WPF


type Model =
  {
    Path: string
    StatusMsg: string }
    
        
let init () =
  {
    Path = ""
    StatusMsg = "" },
  []

type Msg =
  | SetText of string
  | OpenPath
  | OpenPathSuccess of string
  | OpenPathCanceled
  | OpenPathFailed of exn
  | Install
  | InstallSuccess
  | InstallFailed of exn

let openPath () =
  async {
    let dialog = VistaFolderBrowserDialog()
    dialog.Description <- "Please select a folder."
    dialog.UseDescriptionForTitle <- true
    let result = dialog.ShowDialog ()
    if result.HasValue && result.Value then
      let contents = dialog.SelectedPath
      return OpenPathSuccess contents
    else return OpenPathCanceled
  }

let download (url, path) =
  async {
    let! request = Http.AsyncRequestStream(url)
    use outputFile = new System.IO.FileStream(path + "\\test.png",System.IO.FileMode.Create)
    do! request.ResponseStream.CopyToAsync( outputFile ) |> Async.AwaitIAsyncResult |> Async.Ignore
    return InstallSuccess
  }


let update msg m =
  match msg with
  | SetText s -> { m with Path = s}, Cmd.none
  | OpenPath -> m, Cmd.OfAsync.either openPath () id OpenPathFailed
  | OpenPathSuccess s -> { m with Path = s; StatusMsg = sprintf "Successfully OpenPath at %O" DateTimeOffset.Now }, Cmd.none
  | OpenPathCanceled -> { m with StatusMsg = "OpenPath canceled" }, Cmd.none
  | OpenPathFailed ex -> { m with StatusMsg = sprintf "OpenPath failed with exception %s: %s" (ex.GetType().Name) ex.Message }, Cmd.none
  | Install -> m, Cmd.OfAsync.either download ("https://raw.githubusercontent.com/fsharp/FSharp.Data/master/misc/logo.png", m.Path) id InstallFailed
  | InstallSuccess -> { m with StatusMsg = "Install success" }, Cmd.none
  | InstallFailed ex -> { m with StatusMsg = sprintf "Install failed with exception %s: %s" (ex.GetType().Name) ex.Message }, Cmd.none


let bindings () : Binding<Model, Msg> list = [
  "Path" |> Binding.twoWay ((fun m -> m.Path), SetText)
  "StatusMsg" |> Binding.twoWay ((fun m -> m.StatusMsg), SetText)
  "OpenPath" |> Binding.cmd OpenPath
  "Install" |> Binding.cmd Install
]


let designVm = ViewModel.designInstance (init () |> fst) (bindings ())


let main window =
  WpfProgram.mkProgram init update bindings
  |> WpfProgram.startElmishLoop window