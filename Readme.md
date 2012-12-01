# F# MonoDroid Sample #

## Preliminary step ##
- Place the MonoDroid.FSharp.targets file in C:\Program Files (x86)\MSBuild\Novell\

## Using an F# Library Project with MonoDroid Application ##

- Create an Android application project under "MonoDroid" section
- Create an F# library project 
- Edit the F# library project
- Remove the following line and replace with the MonoDroid.FSharp.targets file,

**original**

`<Import Project="$(MSBuildExtensionsPath32)\FSharp\1.0\Microsoft.FSharp.Targets" Condition="!Exists('$(MSBuildBinPath)\Microsoft.Build.Tasks.v4.0.dll')" />
  <Import Project="$(MSBuildExtensionsPath32)\..\Microsoft F#\v4.0\Microsoft.FSharp.Targets" Condition=" Exists('$(MSBuildBinPath)\Microsoft.Build.Tasks.v4.0.dll')" />`

**updated**

`<Import Project="$(MSBuildExtensionsPath)\Novell\MonoDroid.FSharp.targets" />`

- Final step is to reference the FSharp.Core.dll to the Android Application and run the app!

The sample in this repository is a Twitter demo app that loads up the first 10 tweets for your twitter account. There are couple of settings that you would have to do,

- Get the Consumer Key and Consumer Secret from the developer twitter website ([link](https://dev.twitter.com/)).
- Generate the OAuth secret and key by following the below steps,
- You can find more details [here](https://dev.twitter.com/docs/auth/oauth/faq).  Execute the below code based from the sample in a F# script file or console app,

`// Execute the below three lines first and then copy-paste the verifier code that Twitter gives you
let oauth_token'', oauth_token_secret'', oauth_callback_confirmed = OAuth.requestToken()
let url = OAuth.authorizeURI + "?oauth_token=" + oauth_token''
System.Diagnostics.Process.Start(url) |> ignore`

- This will authenticate from twitter and execute the below code to generate the verifier key

`let mutable verifier = ""
let oauth_token, oauth_token_secret = OAuth.accessToken(oauth_token'', oauth_token_secret'', verifier)
printfn "%s" oauth_token
printfn "%s" oauth_token_secret`

- Replace these keys in the project and run, you should be able to see the tweets appearing in your screen!

![](https://github.com/fahadsuhaib/fsharp-droid/blob/master/output.png)