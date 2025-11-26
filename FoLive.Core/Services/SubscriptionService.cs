using System;
using System.Threading.Tasks;
using FoLive.Core.Models;

namespace FoLive.Core.Services;

public class SubscriptionService
{
    private readonly AuthService _authService;

    public SubscriptionService(AuthService authService)
    {
        _authService = authService;
    }

    public bool CanAddStream(int currentStreamCount)
    {
        // Kiểm tra đăng nhập trước
        if (!_authService.IsLoggedIn)
        {
            return false;
        }

        var subscription = _authService.CurrentUser?.Subscription;
        if (subscription == null || !subscription.IsActive)
        {
            return currentStreamCount < 1; // Free plan: 1 stream
        }

        return currentStreamCount < subscription.MaxStreams;
    }

    public bool CanUseAdvancedFeatures()
    {
        // Kiểm tra đăng nhập trước
        if (!_authService.IsLoggedIn)
        {
            return false;
        }

        var subscription = _authService.CurrentUser?.Subscription;
        if (subscription == null || !subscription.IsActive)
        {
            return false; // Free plan: no advanced features
        }

        return subscription.CanUseAdvancedFeatures;
    }

    public bool CanUseScreenCapture()
    {
        // Kiểm tra đăng nhập trước
        if (!_authService.IsLoggedIn)
        {
            return false;
        }

        var subscription = _authService.CurrentUser?.Subscription;
        if (subscription == null || !subscription.IsActive)
        {
            return false; // Free plan: no screen capture
        }

        return subscription.CanUseScreenCapture;
    }

    public bool CanUseMultipleSources()
    {
        // Kiểm tra đăng nhập trước
        if (!_authService.IsLoggedIn)
        {
            return false;
        }

        var subscription = _authService.CurrentUser?.Subscription;
        if (subscription == null || !subscription.IsActive)
        {
            return false; // Free plan: only file source
        }

        return subscription.CanUseMultipleSources;
    }

    public string GetPlanName()
    {
        // Kiểm tra đăng nhập trước
        if (!_authService.IsLoggedIn)
        {
            return "Chưa đăng nhập";
        }

        var subscription = _authService.CurrentUser?.Subscription;
        if (subscription == null || !subscription.IsActive)
        {
            return "Miễn phí";
        }

        return subscription.Plan switch
        {
            "monthly" => "Gói Tháng",
            "yearly" => "Gói Năm",
            "lifetime" => "Gói Vĩnh viễn",
            _ => "Miễn phí"
        };
    }

    public int GetMaxStreams()
    {
        // Kiểm tra đăng nhập trước
        if (!_authService.IsLoggedIn)
        {
            return 0;
        }

        var subscription = _authService.CurrentUser?.Subscription;
        if (subscription == null || !subscription.IsActive)
        {
            return 1; // Free plan: 1 stream
        }

        return subscription.MaxStreams;
    }

    public string GetFeatureRestrictionMessage(string feature)
    {
        // Kiểm tra đăng nhập trước
        if (!_authService.IsLoggedIn)
        {
            return "Vui lòng đăng nhập để sử dụng tính năng này.";
        }

        var planName = GetPlanName();
        return feature switch
        {
            "add_stream" => $"Bạn đã đạt giới hạn số luồng cho gói {planName}. Vui lòng nâng cấp để thêm nhiều luồng hơn.",
            "screen_capture" => $"Tính năng quay màn hình chỉ dành cho gói trả phí. Vui lòng nâng cấp để sử dụng.",
            "advanced_features" => $"Tính năng nâng cao chỉ dành cho gói trả phí. Vui lòng nâng cấp để sử dụng.",
            "multiple_sources" => $"Nhiều nguồn chỉ dành cho gói trả phí. Vui lòng nâng cấp để sử dụng.",
            _ => $"Tính năng này chỉ dành cho gói trả phí. Vui lòng nâng cấp để sử dụng."
        };
    }
}


