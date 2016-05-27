using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Messages;
using static System.Diagnostics.Debug;
using System.Threading.Tasks;
using Android.Gms.Common;
using System.Text;
using Android.Content;

namespace ITCampBeacon.Droid
{
	[Activity(Label = "ITCampBeacon.Droid", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity, GoogleApiClient.IConnectionCallbacks, IResultCallback
	{
		int count = 1;

		GoogleApiClient _client;
		BeaconMessageListener _listener;
		BeaconSubscribeCallback _callback = new BeaconSubscribeCallback();

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);


			_client = new GoogleApiClient.Builder(this)
			  .AddConnectionCallbacks(this)
			  .AddOnConnectionFailedListener(r => WriteLine("Connection failed"))
			  .AddApi(NearbyClass.MessagesApi)

			  .Build();

			await Task.Run(() => _client.BlockingConnect());

			var status = await NearbyClass.Messages.GetPermissionStatusAsync(_client);

			if (status.IsSuccess)
				Subscribe();
			else
				status.StartResolutionForResult(this, ConnectionResult.ResolutionRequired);

		}

		protected override void OnStop()
		{
			base.OnStop();
			NearbyClass.Messages.Unsubscribe(_client, _listener);
		}

		void Subscribe()
		{
			var filter = new MessageFilter.Builder()
				.IncludeNamespacedType("itcamp-beacon", "string")
				.Build();

			var options = new SubscribeOptions.Builder()
				.SetStrategy(Strategy.BleOnly)
				.SetFilter(filter)
				.SetCallback(_callback).Build();

			// subscribe in the background
			NearbyClass.Messages.Subscribe(_client, GetPendingIntent(), options).SetResultCallback(this);

			// subscribe in the foreground
			_listener = new BeaconMessageListener(this);
			NearbyClass.Messages.Subscribe(_client, _listener, options).SetResultCallback(this);
		}


		private PendingIntent GetPendingIntent()
		{
			return PendingIntent.GetService(Application.Context, 0,
				new Intent(Application.Context, typeof(BackgroundSubscribeService)),
				PendingIntentFlags.UpdateCurrent);
		}

		public void OnConnected(Bundle connectionHint)
		{
			WriteLine("API connected");
		}

		public void OnConnectionSuspended(int cause)
		{
			WriteLine("API suspended");
		}

		public void OnResult(Java.Lang.Object result)
		{
			bool bp = true;
		}

		public void UpdateMessage(string messageText)
		{
			FindViewById<TextView>(Resource.Id.message_text_view).Text = messageText;
		}

	}


	public class BeaconMessageListener : MessageListener
	{
		private readonly MainActivity _activity;

		public BeaconMessageListener(MainActivity activity)
		{
			_activity = activity;
		}

		public override void OnFound(Android.Gms.Nearby.Messages.Message message)
		{
			var contents = Encoding.UTF8.GetString(message.GetContent());
			_activity.UpdateMessage("Found message:\n\n" +
				$"Namespace - '{message.Namespace}'\n" +
				$"Content - '{contents}'.");
		}

		public override void OnLost(Android.Gms.Nearby.Messages.Message message)
		{
			var contents = Encoding.UTF8.GetString(message.GetContent());
			_activity.UpdateMessage("Lost message:\n\n" +
									$"Namespace - '{message.Namespace}'\n" +
									$"Content - '{contents}'.");
		}
	}

	public class BeaconSubscribeCallback : SubscribeCallback
	{
		public override void OnExpired()
		{
			base.OnExpired();
			System.Diagnostics.Debug.WriteLine("Subscribed expired");
		}
	}
}


