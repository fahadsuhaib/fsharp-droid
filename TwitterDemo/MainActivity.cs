using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace TwitterDemo
{
    [Activity(Label = "Twitter Demo", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        //int count = 1;
        private TwitterDemo.FSharp.Agent.TwitterMbox twitterAgent;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.twitterAgent = TwitterDemo.FSharp.Agent.mkNewTwitterBox();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var listView = FindViewById<ListView>(Resource.Id.listView1);
#warning make sure to update username
            var username = "";
            //var tweets = this.twitterAgent.GetUserTimeline(username, 10);
            //listView.Adapter = new TweetsAdapter(tweets, this);
            this.twitterAgent.GetUserTimelineAsync(username, 10, (tweets) =>
                {
                    listView.Adapter = new TweetsAdapter(tweets, this);
                });
        }
    }

    public class TweetsAdapter : BaseAdapter<string>
    {
        private TwitterDemo.FSharp.Data.tweet[] tweets;
        private Activity context;

        public TweetsAdapter(TwitterDemo.FSharp.Data.tweet[] tweets, Activity context)
        {
            this.tweets = tweets;
            this.context = context;
        }

        public override string this[int position]
        {
            get { return this.tweets[position].text; }
        }

        public override int Count
        {
            get { return this.tweets.Length; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView; // re-use an existing view, if one is available
            if (view == null) // otherwise create a new one
                view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
            view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = tweets[position].text;
            return view;
        }
    }
}

