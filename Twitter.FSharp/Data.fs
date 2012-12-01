namespace TwitterDemo.FSharp

[<AutoOpen>]
module Data =
    
    let user_timeline userId count = sprintf @"https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name=%s&count=%d" userId count
    
    type user = {
        mutable name : string
        mutable profile_image_url : string
        mutable location : string
    }

    type tweet = {
        mutable id_str  : string
        mutable text    : string        
    }