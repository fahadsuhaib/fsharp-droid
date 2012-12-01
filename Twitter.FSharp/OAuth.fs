namespace TwitterDemo.FSharp
open System
open System.IO
open System.Net
open System.Security.Cryptography
open System.Text

module OAuth =

    module Mod =
        // Twitter OAuth Constants
        let consumerKey = failwith ""
        let consumerSecret = failwith ""
        let requestTokenURI = "https://api.twitter.com/oauth/request_token"
        let accessTokenURI = "https://api.twitter.com/oauth/access_token"
        let authorizeURI = "https://api.twitter.com/oauth/authorize"

        // Utilities
        let unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
        let urlEncode str = 
            let encodedUrl = 
                String.init (String.length str) (fun i -> 
                let symbol = str.[i]
                if unreservedChars.IndexOf(symbol) = -1 then
                    "%" + String.Format("{0:X2}", int symbol)
                else
                    string symbol)
            encodedUrl

        // Core Algorithms
        let hmacsha1 signingKey str = 
            let converter = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey : string))
            let inBytes = Encoding.ASCII.GetBytes(str : string)
            let outBytes = converter.ComputeHash(inBytes)
            Convert.ToBase64String(outBytes)

        let compositeSigningKey consumerSecret tokenSecret = 
            urlEncode(consumerSecret) + "&" + urlEncode(tokenSecret)

        let baseString httpMethod baseUri queryParameters =             
            httpMethod + "&" + 
            urlEncode(baseUri) + "&" +
            (queryParameters 
             |> Seq.sortBy (fun (k,v) -> k)
             |> Seq.map (fun (k,v) -> urlEncode(k)+"%3D"+urlEncode(v))             
             |> String.concat "%26")

        let createAuthorizeHeader queryParameters = 
            let headerValue = 
                "OAuth " + 
                (queryParameters
                 |> Seq.map (fun (k,v) -> urlEncode(k)+"\x3D\""+urlEncode(v)+"\"")
                 |> String.concat ",")
            headerValue

        let currentUnixTime() = floor (DateTime.UtcNow - DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds

        /// Request a token from Twitter and return:
        ///  oauth_token, oauth_token_secret, oauth_callback_confirmed
        let requestToken() = 
            let signingKey = compositeSigningKey consumerSecret ""

            let queryParameters = 
                ["oauth_callback", "oob";
                 "oauth_consumer_key", consumerKey;
                 "oauth_nonce", System.Guid.NewGuid().ToString().Substring(24);
                 "oauth_signature_method", "HMAC-SHA1";
                 "oauth_timestamp", currentUnixTime().ToString();
                 "oauth_version", "1.0"]

            let signingString = baseString "POST" requestTokenURI queryParameters
            let oauth_signature = hmacsha1 signingKey signingString

            let realQueryParameters = ("oauth_signature", oauth_signature)::queryParameters

            let req = WebRequest.Create(requestTokenURI, Method="POST")
            let headerValue = createAuthorizeHeader realQueryParameters
            req.Headers.Add(HttpRequestHeader.Authorization, headerValue)
    
            let resp = req.GetResponse()
            let stream = resp.GetResponseStream()
            let txt = (new StreamReader(stream)).ReadToEnd()
    
            let parts = txt.Split('&')
            (parts.[0].Split('=').[1],
             parts.[1].Split('=').[1],
             parts.[2].Split('=').[1] = "true")

        /// Get an access token from Twitter and returns:
        ///   oauth_token, oauth_token_secret
        let accessToken(token, tokenSecret, verifier) =
            let signingKey = compositeSigningKey consumerSecret tokenSecret

            let queryParameters = 
                ["oauth_consumer_key", consumerKey;
                 "oauth_nonce", System.Guid.NewGuid().ToString().Substring(24);
                 "oauth_signature_method", "HMAC-SHA1";
                 "oauth_token", token;
                 "oauth_timestamp", currentUnixTime().ToString();
                 "oauth_verifier", verifier;
                 "oauth_version", "1.0"]

            let signingString = baseString "POST" accessTokenURI queryParameters
            let oauth_signature = hmacsha1 signingKey signingString
    
            let realQueryParameters = ("oauth_signature", oauth_signature)::queryParameters
    
            let req = WebRequest.Create(accessTokenURI, Method="POST")
            let headerValue = createAuthorizeHeader realQueryParameters
            req.Headers.Add(HttpRequestHeader.Authorization, headerValue)
    
            let resp = req.GetResponse()
            let stream = resp.GetResponseStream()
            let txt = (new StreamReader(stream)).ReadToEnd()
    
            let parts = txt.Split('&')
            (parts.[0].Split('=').[1],
             parts.[1].Split('=').[1])
    
        /// Compute the 'Authorization' header for the given request data
        let authHeaderAfterAuthenticated url httpMethod token tokenSecret queryParams = 
            let signingKey = compositeSigningKey consumerSecret tokenSecret

            let queryParameters = 
                    ["oauth_consumer_key", consumerKey;
                     "oauth_nonce", System.Guid.NewGuid().ToString().Substring(24);
                     "oauth_signature_method", "HMAC-SHA1";
                     "oauth_token", token;
                     "oauth_timestamp", currentUnixTime().ToString();
                     "oauth_version", "1.0"]

            let signingQueryParameters = 
                List.append queryParameters queryParams

            let signingString = baseString httpMethod url (signingQueryParameters |> List.toArray)
            let oauth_signature = hmacsha1 signingKey signingString
            let realQueryParameters = ("oauth_signature", oauth_signature)::queryParameters
            let headerValue = createAuthorizeHeader realQueryParameters
            headerValue

        /// Add an Authorization header to an existing WebRequest 
        let addAuthHeaderForUser (webRequest : WebRequest, token : string, tokenSecret : string, queryParams) = 
            let mutable url = webRequest.RequestUri.ToString()
            let mutable qParam = queryParams
            if url.Contains("?") then
                let paramUrls = url.Substring( url.IndexOf("?") + 1, (url.Length - 1) - url.IndexOf("?"))
                let mutable s = [String.Empty, String.Empty;]
                for str in paramUrls.Split([|'&'|]) do
                    let k = str.Split([|'='|])
                    qParam <- (k.[0], k.[1]) :: qParam
                url <- url.Substring(0, url.IndexOf("?"))

            let httpMethod = webRequest.Method
            let header = authHeaderAfterAuthenticated url httpMethod token tokenSecret qParam
            webRequest.Headers.Add(HttpRequestHeader.Authorization, header)

    let requestToken = Mod.requestToken()

    let accessToken token tokenSecret verifier = Mod.accessToken(token, tokenSecret, verifier)

    let addOAuthHeader webRequest token tokenSecret queryParams = Mod.addAuthHeaderForUser(webRequest, token, tokenSecret, queryParams)