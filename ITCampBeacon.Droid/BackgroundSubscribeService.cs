using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Messages;
using Android.Support.V4.App;

namespace ITCampBeacon.Droid
{
	[Service]
	public class BackgroundSubscribeService : IntentService
	{
		private const int MESSAGES_NOTIFICATION_ID = 1;

		private readonly BackgroundMessageListener _listener;

		public BackgroundSubscribeService() : base("BackgroundSubscribeService")
		{
			_listener = new BackgroundMessageListener(this);
		}

		public override void OnCreate()
		{
			base.OnCreate();
			UpdateNotification("Created", "");
		}

		public class BackgroundMessageListener : MessageListener
		{
			readonly BackgroundSubscribeService _intentService;

			public BackgroundMessageListener(BackgroundSubscribeService intentService)
			{
				_intentService = intentService;
			}

			public override void OnFound(Message message)
			{
				var contents = Encoding.UTF8.GetString(message.GetContent());
				_intentService.UpdateNotification("Found message",
					$"Namespace - '{message.Namespace}', Content - '{contents}'.");
			}

			public override void OnLost(Message message)
			{
				var contents = Encoding.UTF8.GetString(message.GetContent());
				_intentService.UpdateNotification("Lost message",
					$"Namespace - '{message.Namespace}', Content - '{contents}'.");
			}
		}

		protected override void OnHandleIntent(Intent intent)
		{
			if (intent != null)
				NearbyClass.Messages.HandleIntent(intent, _listener);
		}

		#region code to send notification

		private void UpdateNotification(string contentTitle, string contentText)
		{
			var notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
			var launchIntent = new Intent(Application.Context, typeof(MainActivity));

			launchIntent.SetAction(Intent.ActionMain);
			launchIntent.AddCategory(Intent.CategoryLauncher);
			PendingIntent pi = PendingIntent.GetActivity(Application.Context, 0,
				launchIntent, PendingIntentFlags.UpdateCurrent);

			var notificationBuilder = new NotificationCompat.Builder(this)
				.SetSmallIcon(Resource.Drawable.ic_stars_black_48dp)
				.SetContentTitle(contentTitle)
				.SetContentText(contentText)
				.SetStyle(new NotificationCompat.BigTextStyle().BigText(contentText))
				.SetOngoing(true)
				.SetContentIntent(pi);

			notificationManager.Notify(MESSAGES_NOTIFICATION_ID,
				notificationBuilder.Build());
		}

		#endregion
	}
}

