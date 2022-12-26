using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MasjidBandung;

public static class ApiGroup {
    // public const string Auth = "auth";
    // public const string AdBus = "adbus";
    public const string Public = "public";
    public const string Internal = "internal";
}

public static class SwaggerExtensions {
    public static IServiceCollection AddSwaggerGenOptions(this IServiceCollection service) {
        return service.AddSwaggerGen(
            opt => {
                opt.SwaggerDoc(
                    ApiGroup.Public, new OpenApiInfo {
                        Title = "Masjid Bandung",
                        Version = "v1",
                    }
                );
                opt.SwaggerDoc(
                    ApiGroup.Internal, new OpenApiInfo {
                        Title = "Internal API",
                        Version = "v1"
                    }
                );

                opt.SetOptions();
            }
        );
    }


    public static IApplicationBuilder UseSwaggerUiOptions(this IApplicationBuilder builder) {
        builder.UseSwaggerUI(
            options => {
                options.DocumentTitle = "Masjid Bandung API Documentation";
                options.RoutePrefix = "docs";

                options.SwaggerEndpoint("/swagger/public/swagger.json", "Masjid Bandung");
                options.SwaggerEndpoint("/swagger/internal/swagger.json", "Internal API (Unused/Deprecated)");
                // options.SwaggerEndpoint("/swagger/pantau/swagger.json", "Pantau REST API");
                // options.SwaggerEndpoint("/swagger/internal/swagger.json", "Internal API");
                // options.SwaggerEndpoint("/swagger/wilayah/swagger.json", "Wilayah Administrasi Indonesia");

                options.DisplayRequestDuration();
                options.EnablePersistAuthorization();
                options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);

                options.EnableTryItOutByDefault();
            }
            // options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "AdBus REST API")
        );
        return builder;
    }

    public static void SetOptions(this SwaggerGenOptions options) {
        options.OrderActionsBy(o => o.RelativePath);
        options.CustomSchemaIds(s => s.Name);
        // c.TagActionsBy(d => new List<string>() { d.GroupName ?? "tes" });
        // options.AddSecurityDefinition(
        //     JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme {
        //         In = ParameterLocation.Header,
        //         Description = "Deskripsi",
        //         Name = "Authorization",
        //         Scheme = JwtBearerDefaults.AuthenticationScheme,
        //         Type = SecuritySchemeType.Http
        //     }
        // );
        // options.AddSecurityRequirement(
        //     new OpenApiSecurityRequirement {
        //         {
        //             new OpenApiSecurityScheme {
        //                 Reference = new OpenApiReference {
        //                     Type = ReferenceType.SecurityScheme,
        //                     Id = JwtBearerDefaults.AuthenticationScheme
        //                 }
        //             },
        //             Array.Empty<string>()
        //         }
        //     }
        // );

        // Set the comments path for the Swagger JSON and UI.
        // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, typeof(Program).Assembly.GetName().Name + ".xml");
        options.IncludeXmlComments(xmlPath, true);
        options.EnableAnnotations();

        options.SchemaFilter<SwaggerExcludeFilter>();
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class SwaggerExcludeFilter : ISchemaFilter {
    public void Apply(OpenApiSchema schema, SchemaFilterContext context) {
        if (schema.Properties is null) return;
        var props = context.Type
            .GetProperties()
            .Where(t => t.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length > 0);
        foreach (var prop in props) {
            if (schema.Properties.ContainsKey(prop.Name)) schema.Properties.Remove(prop.Name);
        }
    }
}
