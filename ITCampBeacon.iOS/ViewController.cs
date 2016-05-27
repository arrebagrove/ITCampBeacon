using System;
using CoreLocation;
using Foundation;
using UIKit;
using System.Linq;

namespace ITCampBeacon.iOS
{
	public partial class ViewController : UIViewController
	{
		CLLocationManager LocationManager = ((AppDelegate)UIApplication.SharedApplication.Delegate).LocationManager;
		CLBeaconRegion BeaconRegion;

		#region Color from distance
		private UIColor ColorFromDistance(double distance)
		{
			if (distance < 0.0d)
				return UIColor.Gray;
			else if (distance < 1.0d)
				return UIColor.Green;
			else if (distance < 5.0d)
				return UIColor.Orange;
			else
				return UIColor.Red;
		}
		#endregion

	

		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			LocationManager.DidRangeBeacons += LocationManager_DidRangeBeacons;

			BeaconRegion = new CLBeaconRegion(AppDelegate.RegionUuid, "RangingBeacon");

			LocationManager.StartRangingBeacons(BeaconRegion);
		}

		public override void ViewDidUnload()
		{
			base.ViewDidUnload();
			LocationManager.DidRangeBeacons -= LocationManager_DidRangeBeacons;
			LocationManager.StopMonitoring(BeaconRegion);
		}

		void LocationManager_DidRangeBeacons(object sender, CLRegionBeaconsRangedEventArgs e)
		{
			var beacon = e.Beacons.FirstOrDefault();
			if (beacon == null)
			{
				View.BackgroundColor = UIColor.White;
				lblDistance.Text = "Waiting for the beacon...";
			}
			else if (beacon.Accuracy >= 0)
			{
				View.BackgroundColor = ColorFromDistance(beacon.Accuracy);
				lblDistance.Text = $"Distance to beacon\n{beacon.Accuracy:N1}m";
			}
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}

