namespace TwitterDemo.FSharp
open System.IO
open System.Net

module Agent =
    
    let oauth_token_secret = failwith ""
    let oauth_token = failwith ""
    
    let httpAsync (url:string) = 
        async { let req = WebRequest.Create(url)    
                OAuth.addOAuthHeader req oauth_token oauth_token_secret []
                //let! resp = req.AsyncGetResponse()
                try
                    let resp = req.GetResponse()
                    // rest is a callback
                    use stream = resp.GetResponseStream() 
                    use reader = new StreamReader(stream) 
                    let text = reader.ReadToEnd() 
                    return text 
                with e -> return (e.Message + "\r\n" + e.StackTrace)}

    type Action =
    /// Get the latest user timeline for userId and count
    | UserTimeline  of string * int * AsyncReplyChannel<tweet[]>
    
    type TwitterMbox() =
        
        let mbox = MailboxProcessor<Action>.Start(fun inbox ->
            let rec loop() = async {
                let! req = inbox.Receive()
                match req with
                | Action.UserTimeline(userId, int, r) ->
                    let url = Data.user_timeline userId int
                    let! tweetTxt = httpAsync url
                    let d : tweet[] = deserialize tweetTxt
                    r.Reply(d)
                    return! loop()
            }

            loop()
        )

        member x.GetUserTimelineAsync(userId, count, f: System.Action<tweet[]>) = 
            let computeMsg = mbox.PostAndAsyncReply(fun r -> UserTimeline(userId, count, r))
            Async.StartWithContinuations(computeMsg,
                (fun tweets -> f.Invoke(tweets)),
                (fun _ -> ()),
                (fun _ -> ()))

        member x.GetUserTimeline(userId, count) = mbox.PostAndReply(fun r -> UserTimeline(userId, count, r))

        interface System.IDisposable with
            member x.Dispose() =
                (mbox :> System.IDisposable).Dispose()

    let mkNewTwitterBox() = new TwitterMbox()