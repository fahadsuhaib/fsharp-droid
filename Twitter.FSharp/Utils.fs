namespace TwitterDemo.FSharp
open ServiceStack.Text

[<AutoOpen>]
module Utils =
    
    /// Serialize object to json
    let serialize (t:'a) = t |> JsonSerializer.SerializeToString

    /// Deserialize json to object
    let deserialize (t:string) : 'a = t |> JsonSerializer.DeserializeFromString 