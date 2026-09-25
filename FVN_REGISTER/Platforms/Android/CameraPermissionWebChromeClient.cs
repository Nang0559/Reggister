using Android.Webkit;
using Microsoft.Maui.ApplicationModel;

namespace FVN_REGISTER.Platforms.Android;

internal sealed class CameraPermissionWebChromeClient : WebChromeClient
{
    public override void OnPermissionRequest(PermissionRequest request)
    {
        var resources = request.GetResources();

        if (resources.Any(x =>
            string.Equals(
                x,
                PermissionRequest.ResourceVideoCapture,
                StringComparison.OrdinalIgnoreCase)))
        {
            var status = Permissions.CheckStatusAsync<Permissions.Camera>().GetAwaiter().GetResult();

            if (status == PermissionStatus.Granted)
            {
                request.Grant(resources);
            }
            else
            {
                request.Deny();
            }

            return;
        }

        base.OnPermissionRequest(request);
    }
}
