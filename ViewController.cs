using System;

using UIKit;

using Microblink;

namespace iOS
{
	public partial class ViewController : UIViewController
	{
        public MBBarcodeRecognizer BarcodeRecognizer { get; private set; }

        // there are plenty of recognizers available - see iOS documentation
        // for more information: https://github.com/BlinkID/blinkid-ios/blob/master/README.md

        CustomDelegate _customDelegate;

		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.

            _customDelegate = new CustomDelegate(this);

            // set license key for iOS with bundle ID com.microblink.sample
            MBMicroblinkSDK.SharedInstance().SetLicenseKey("sRwAAAEVY28uemEubXJ0bmtyc3RuLnZtYXBwwF+fBiJNUqXmd04lt/qhhoFxchZeLEYpa0KSu+pk189nojHgNDMpjOq/GrxS151TB7owAW7+LqU5CKu8fxg9lvE4b8eVpGMx6ZCl10L9QJLPGuDQxPScGxJLzpmqscNddKlYfni4DuPvWTa3KXCFOpFtaZ8WybT4NyiSDloUls+qd/mEWqlZEr8WJi6eO3GGJY5hFgldGsfsBXOifc/XC4a2gt6+nBuAWfPIhLD2zXhQfdqgttbpCJSVgJNiwsciVP5hNFGXhtYY/i07jxybcbed/MkiRwPqjoMiQGmGDSGeDS6xFK+wDBPa/x+EMLQF39uqYRpV", null);
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}

		partial void StartScanningButton_TouchUpInside (UIButton sender)
		{
            BarcodeRecognizer = new MBBarcodeRecognizer
            {
                NullQuietZoneAllowed = false,
                ScanQrCode = true,
                ScanCode39 = true,
                ScanCode128 = true
            };

            // create a collection of recognizers that will be used for scanning
            var recognizerCollection = new MBRecognizerCollection(new MBRecognizer[] { BarcodeRecognizer });

            // create a settings object for overlay that will be used. For ID it's best to use DocumentOverlayViewController
            // there are also other overlays available - check iOS documentation
            var barcodeOverlaySettings = new MBBarcodeOverlaySettings
            {
                CameraSettings = new MBCameraSettings
                {
                    CameraType = MBCameraType.Front
                }
            };
            var blinkIdOverlay = new MBBarcodeOverlayViewController(barcodeOverlaySettings, recognizerCollection, _customDelegate);

            // finally, create a recognizerRunnerViewController
            var recognizerRunnerViewController = MBViewControllerFactory.RecognizerRunnerViewControllerWithOverlayViewController(blinkIdOverlay);

            // in ObjC recognizerRunnerViewController would actually be of type UIViewController<MBRecognizerRunnerViewController>*, but this construct
            // is not supported in C#, so we need to use Runtime.GetINativeObject to cast obtained IMBReocognizerRunnerViewController into UIViewController
            // that can be presented
            this.PresentViewController(ObjCRuntime.Runtime.GetINativeObject<UIViewController>(recognizerRunnerViewController.Handle, false), true, null);
		}

        public void HandleBarCodeResult(string blinkidResult)
        {
            var i = blinkidResult;
        }

        private class CustomDelegate : MBBarcodeOverlayViewControllerDelegate
        {
            private readonly ViewController _me;

            public CustomDelegate(ViewController me)
            {
                _me = me;
            }

            public override void BarcodeOverlayViewControllerDidFinishScanning(MBBarcodeOverlayViewController barcodeOverlayViewController, MBRecognizerResultState state)
            {
                barcodeOverlayViewController.RecognizerRunnerViewController.PauseScanning();

                if (_me.BarcodeRecognizer.Result.ResultState == MBRecognizerResultState.Valid)
                {
                    var blinkidResult = _me.BarcodeRecognizer.Result.StringData;

                    UIApplication.SharedApplication.InvokeOnMainThread(delegate
                    {
                        _me.DismissViewController(true, () => _me.HandleBarCodeResult(blinkidResult));
                    });
                }
            }

            public override void BarcodeOverlayViewControllerDidTapClose(MBBarcodeOverlayViewController barcodeOverlayViewController)
            {
                _me.DismissViewController(true, null);
            }
        }
	}
}

