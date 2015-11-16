// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System.Text.RegularExpressions
open System

type DiffResult = { chunks : int; deletions : int; additons : int }

let (|Match|_|) pattern input =
    let m = Regex.Match(input, pattern) in
    if m.Success then Some (List.tail [ for g in m.Groups -> g.Value ]) else None

let (|FirstRegexGroup|_|) pattern input =
   let m = Regex.Match(input,pattern) 
   if (m.Success) then Some m.Groups.[1].Value else None  

let parse (input : string) : seq<DiffResult> =
    if input = null then Seq.empty
    elif (String.IsNullOrWhiteSpace input) then Seq.empty
    else 
        let lines = input.Split '\n'
        if lines.Length == 0 then Seq.empty
        else
            
            Seq.empty


[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0 // return an integer exit code