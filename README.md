# ASP.NET MVC Route Localizer

This library adds localization support for the convention-based ASP.NET MVC routing system. It is a lightweight and transparent extension for the framework's built-in routing mechanism by translating routes into multiple URLs in different languages, and vice versa.

## Getting Started

First, you need to add the [MvcRouteLocalizer NuGet package](https://www.nuget.org/packages/MvcRouteLocalizer/) to your project:

```
Install-Package MvcRouteLocalizer
```

Then add the following line at the end of your `RouteConfig` class:

```csharp
public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

        routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
        );

        routes.Localize();
    }
}
```

This doesn't do much now, because the default route dictionary provider only translates controllers and actions where the custom `[UrlName(…)]` attribute is present. So your controller classes should look like this:

```csharp
using MvcRouteLocalizer;

namespace MyApplication.Controllers
{
    [UrlName("home")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [UrlName("about-us")]
        public ActionResult About()
        {
            return View();
        }

        [UrlName("contact-us")]
        public ActionResult Contact()
        {
            return View();
        }
    }
}
```

You can either decorate all your controller classes and action methods with this attribute like in above example, or bring all the translations into a separate `.resx` file.

## Working With Resource Files

If you have to translate lots of controllers and actions, or your application is multilingual, you can create a resource (`.resx`) file with all the required translations. A French resource file for an e-commerce admin panel would look like this:

Name             | Value
---------------- | --------
Products         | produits
Products/Details | details
Products/Create  | creer
Products/Edit    | modifier
Products/Delete  | supprimer

Where the `ProductsController` class has 4 action methods: `Details`, `Create`, `Edit` and `Delete`. In this case, a URL like `/produits/modifier/5` will match the `ProductController`'s `Edit` action.

> Please keep in mind that names with trailing slashes are not valid identifiers, so you should turn off code generation altogether for these resource files.

Once you set up the resource file, all you need is to import it in the configuration section:

```csharp
using System.Reflection;
using System.Resources;
using MvcRouteLocalizer;

public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        // ...

        routes.Localize(options =>
        {
            var resourceAssembly = Assembly.GetExecutingAssembly();
            var resourceManager = new ResourceManager("MyNamespace.MyResourceFile", resourceAssembly);

            options.Dictionary = new ResxRouteDictionaryProvider(resourceManager);
        });
    }
}
```

The `ResourceManager` is the good old resource reader class that comes with the .NET Framework, while the `ResxRouteDictionaryProvider` creates the bijection between the original route values and their corresponding translations. You'll need to add the `System.Resources` and `MvcRouteLocalizer` namespaces for these classes.

You can also create multiple localization resource files, each for every supported language. In this case, the `ResourceManager` will take care of providing the required translations based on the current culture (`CultureInfo.CurrentUICulture`).

## Setting Up Hyphenated URLs

If you don't need to translate the URLs into non-English languages but want lowercase URLs with hyphens as separators, you only have to add a `HyphenatedRouteDictionaryProvider` instance in the localization configuration:

```csharp
using MvcRouteLocalizer;

public class RouteConfig
{
    public static void RegisterRoutes(RouteCollection routes)
    {
        // ...

        routes.Localize(options =>
        {
            options.Dictionary = new HyphenatedRouteDictionaryProvider();
        });
    }
}
```

In this way, the localizer will look for all the possible controllers and actions and convert their names into a hyphenated *kebab* case. For example, a URL like `/Home/AboutUs` would be converted to `/home/about-us`. But if you have special cases where you want to change the default functionality, you can also use the `[UrlName(…)]` attribute to overwrite the auto-generated names.

## Fully Transparent

The routing localization is handled transparently, as you don't have to pay any attention to how the system handles the localized URLs. The `RouteData` object will always contain the original controller and action names. So if you have a request with the URL `/home/about-us`, it will be automatically converted back to `["controller"] = Home` and `["action"] = "AboutUs"` route values. Similarly, you can always refer to the native names of your controllers and actions, as they will be automatically translated in the URL generation process.

> Please note that if you use the `[ActionName(…)]` attribute, the original name of the action will be the one you specified in the attribute, just like its view's name. The `.resx` files should also use this name instead of the method's identifier.

## Further Improvements

This small library has still lots of room for improvement. In the upcoming versions, *areas* and *literals* are also going to be localizable, and a culture-based URL prefix is in the plans, too. Furthermore, supporting ASP.NET Core is going to be the main focus in the long run.