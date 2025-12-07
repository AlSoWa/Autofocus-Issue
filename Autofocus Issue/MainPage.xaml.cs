using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Autofocus_Issue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await this.StartPreviewAsync();
        }
        
        private async ValueTask StartPreviewAsync()
        {
            if (await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture) is { Count: > 0 } camDevice)
            {
                // For this example we just take the first device.
                this.mediaCapture = new MediaCapture();

                var settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = camDevice[1].Id,
                    StreamingCaptureMode = StreamingCaptureMode.Video,
                    MemoryPreference = MediaCaptureMemoryPreference.Auto
                };

                await this.mediaCapture.InitializeAsync(settings);

                foreach (var prop in this.mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview)) 
                {
                    if (prop is VideoEncodingProperties vProp)
                    {
                        // For the example we just select the first available resolution.
                        await this.mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, vProp);
                        break;
                    }
                }

                PreviewControl.Source = this.mediaCapture;
                await this.mediaCapture.StartPreviewAsync();

                if (this.mediaCapture.VideoDeviceController.Focus is { } focusCtrl)
                {
                    if (focusCtrl.TryGetAuto(out var autoFocus))
                    {
                        if (autoFocus)
                        {
                            if (!focusCtrl.TrySetAuto(false))
                            {
                                await new MessageDialog("Failed to disable autofocus").ShowAsync();
                                return;
                            }

                            if (!focusCtrl.TrySetValue(50.0))
                            {
                                await new MessageDialog("Failed to set autofocus value").ShowAsync();
                                return;
                            }
                        }
                    }
                }

                return;
            }

            await new MessageDialog("No video device available").ShowAsync();
        }
    }
}
