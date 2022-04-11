using Swan.DependencyInjection;

namespace XorusCalendarBot.Api;

public static class BaseControllerContainer
{
    public static T WithContainer<T>(this T controller, DependencyContainer container) where T : BaseController
    {
        controller.Container = container;
        return controller;
    }
}